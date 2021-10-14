using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Trip.PasvAPI.AppCode;
using Trip.PasvAPI.Models.Enum;
using Trip.PasvAPI.Models.Model;
using Trip.PasvAPI.Models.Model.Trip;
using Trip.PasvAPI.Models.Repository;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Trip.PasvAPI.Controllers
{
    [Route("api/[controller]")]
    public class PasvController : Controller
    {
        private readonly IMemoryCache _cache;
        private readonly string secretKey;
        private readonly string aesIV;

        public PasvController(IMemoryCache memoryCache)
        {
            this._cache = memoryCache;
            this.secretKey = Website.Instance.Configuration["Proxy:Trip:AesKey"];
            this.aesIV = Website.Instance.Configuration["Proxy:Trip:AesIV"];
        }

        // GET: api/values
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public TTdResponseModel Post([FromBody] JObject req)
        {
            Console.WriteLine($" ===> TTdOpen API Server Type: {req["header"]["serviceName"]}");

            var logRepos = HttpContext.RequestServices.GetService<TripTransLogRepository>();
            var productRepos = HttpContext.RequestServices.GetService<ProductRepository>();
            var orderMasterRepos = HttpContext.RequestServices.GetService<OrderMasterRepository>();
            var tripOrderRepos = HttpContext.RequestServices.GetService<TripOrderRepository>();

            var token = Guid.NewGuid().ToString("N");
            var resp_code = TTdResultCodeEnum.SUCCESS.GetHashCode().ToString("D4"); // 預設正常
            var resp_msg = "";
            dynamic resp_body = null;

            // STEP-1 API 指令區分, KKday 角色為 "非零售供应商 Non-retail suppliers"!!
            var body = TripAesCryptHelper.Decrypt(req["body"].ToString(), secretKey, aesIV);
            var log_oid = logRepos.Insert(req.ToString(), body, "TRIP");
            TTdResponseModel resp = null;


            // 檢查 Sign 是否正確?
            // 格式 => md5(accountId + serviceName + requestTime + body + version + signkey).toLowerCase();
            var req_header = req["header"];
            var req_sign = Md5Helper.ToMD5($"{req_header["accountId"]}{req_header["serviceName"]}{req_header["requestTime"]}{req["body"]}{req_header["version"]}{Website.Instance.SignKey}").ToLower();
            if (!req_header["sign"].ToString().Equals(req_sign))
            {
                // 回傳錯誤結果
                resp_code = TTdResultCodeEnum.WRONG_SIGNATURE.GetHashCode().ToString("D4");
                resp_msg = "签名错误"; 

                resp = new TTdResponseModel()
                {
                    header = new TTdResponseModel.TTdResponseHeaderModel()
                    {
                        resultCode = resp_code,
                        resultMessage = resp_msg
                    }
                }; 

                logRepos.SetResponse(log_oid, JsonConvert.SerializeObject(resp),  null, "KKDAY");

                return resp;
            }

            // 檢查 Supplier 帳號是否正確?
            if (!req_header["accountId"].ToString().Equals(Website.Instance.AgentAccount))
            {
                // 回傳錯誤結果
                resp_code = TTdResultCodeEnum.INCORRECT_SUPPLUER.GetHashCode().ToString("D4");
                resp_msg = "供应商账户信息不正确";

                resp = new TTdResponseModel()
                {
                    header = new TTdResponseModel.TTdResponseHeaderModel()
                    {
                        resultCode = resp_code,
                        resultMessage = resp_msg
                    }
                };

                logRepos.SetResponse(log_oid, JsonConvert.SerializeObject(resp), null, "KKDAY");

                return resp;
            }


            switch (req["header"]["serviceName"]?.ToString())
            {
                // 订单验证 Order verification (*)
                case "VerifyOrder":
                    {
                        try
                        {
                            var order = JObject.Parse(body).ToObject<OrderVerificationReqModel>();
                            var order_items = new List<OrderVerificationReqModel.ItemModel>();

                            order.items.ForEach(i =>
                            {
                                // var locale = "zh-TW";
                                var dummyProd = productRepos.GetDummyProduct(i.PLU);

                                // (1) 判斷产品ID异常，則回傳錯誤碼: 1001
                                if (dummyProd == null)
                                {
                                    resp_code = TTdResultCodeEnum.INVALID_PLU.GetHashCode().ToString("D4");
                                    resp_msg = $" sequenceId={ order.sequenceId } : 有無效的PLU";
                                    resp_body = null;

                                }
                                // (2) 判斷庫存不足, 則回傳錯誤碼: 1003
                                else if (i.quantity > dummyProd.qty)
                                {
                                    resp_code = TTdResultCodeEnum.INSUFFICIENT_INVENTORY.GetHashCode().ToString("D4");
                                    resp_msg = $" PLU={ i.PLU } : 庫存不足";

                                    // 回傳-項目清單(Items)
                                    var _items = new List<OrderVerificationRespModel.ItemModel>();
                                    foreach (var _item in order.items)
                                    {
                                        var _inventorys = new List<OrderVerificationRespModel.ItemModel.InventorysModel>();
                                        _inventorys.Add(new OrderVerificationRespModel.ItemModel.InventorysModel()
                                        {
                                            quantity = 10,
                                            useDate = _item.useStartDate
                                        });

                                        _items.Add(new OrderVerificationRespModel.ItemModel()
                                        {
                                            PLU = _item.PLU,
                                            inventorys = _inventorys
                                        });
                                    }

                                    // 回傳-主體(Body)
                                    resp_body = new OrderVerificationRespModel()
                                    {
                                        items = _items,
                                    };
                                }
                                // (3) 判斷缺失出行人, 則回傳錯誤碼: 1005/1006
                                else if (i.passengers.Count() < 1)
                                {
                                    resp_code = TTdResultCodeEnum.MISSING_INFO.GetHashCode().ToString("D4");
                                    resp_msg = $" PLU={ i.PLU } : 缺失出行人";
                                    resp_body = null;
                                }
                                // (4) 判斷缺失证件(ageType, cardNo), 則回傳錯誤碼: 1005/1006
                                else if (i.passengers.Where(p => string.IsNullOrEmpty(p.ageType) || string.IsNullOrEmpty(p.cardNo)).Count() > 0)
                                {
                                    resp_code = TTdResultCodeEnum.MISSING_INFO.GetHashCode().ToString("D4");
                                    resp_msg = $" PLU={ i.PLU } : 缺失证件";
                                    resp_body = null;
                                }
                                // 檢驗無誤!! 加入待處理項目清單
                                else
                                {
                                    order_items.Add(i);
                                }
                               
                            });

                            if (order_items.Count() > 0)
                            {

                                // 回傳-結果代碼(Result Code)-成功
                                resp_code = TTdResultCodeEnum.SUCCESS.GetHashCode().ToString("D4");
                                resp_msg = "訂單驗證成功";

                                // 回傳-項目清單(Items)
                                var _items = new List<OrderVerificationRespModel.ItemModel>();
                                foreach (var _item in order_items)
                                {
                                    var _inventorys = new List<OrderVerificationRespModel.ItemModel.InventorysModel>();
                                    _inventorys.Add(new OrderVerificationRespModel.ItemModel.InventorysModel()
                                    {
                                        quantity = 10,
                                        useDate = _item.useStartDate
                                    });

                                    _items.Add(new OrderVerificationRespModel.ItemModel()
                                    {
                                        PLU = _item.PLU,
                                        inventorys = _inventorys
                                    });
                                }

                                // 回傳-主體(Body)
                                resp_body = new OrderVerificationRespModel()
                                {
                                    items = _items,
                                };

                            }

                            #region KKday 商品整合區塊 ---- start

                            /*
                            // openid 格式 => "PROD_PKG_ITEM,SKU"
                            var tokens = PLU.Split(','); // openid 格式 => "PROD_PKG_ITEM,SKU"
                            var oid_params = tokens[0].Split('_');
                            var prod_oid = oid_params.Length > 0 ? Convert.ToInt64(oid_params[0]) : 0;
                            var pkg_oid = oid_params.Length > 1 ? Convert.ToInt64(oid_params[1]) : 0;
                            var item_oid = oid_params.Length > 2 ? Convert.ToInt64(oid_params[2]) : 0;
                            var sku_id = tokens[1];
                            var guidToken = Guid.NewGuid().ToString("N");

                            // fetch the value from the source
                            var pkg = productRepos.GetPackage(token, prod_oid, pkg_oid, sku_id, locale);

                            // 判斷套餐的位控資料


                            // 回傳-結果代碼(Result Code)-成功
                            resp_code = TTdResultCodeEnum.SUCCESS.GetHashCode().ToString("D4");
                            resp_msg = "訂單驗證成功";

                            // 回傳-項目清單(Items)
                            var _items = new List<OrderVerificationRespModel.ItemModel>();
                            foreach (var _item in order.items)
                            {
                                var _inventorys = new List<OrderVerificationRespModel.ItemModel.InventorysModel>();
                                _inventorys.Add(new OrderVerificationRespModel.ItemModel.InventorysModel()
                                {
                                    quantity = 10,
                                    useDate = usedDate
                                });

                                _items.Add(new OrderVerificationRespModel.ItemModel()
                                {
                                    PLU = _item.PLU,
                                    inventorys = _inventorys 
                                });
                            }

                            // 回傳-主體(Body)
                            resp_body = new OrderVerificationRespModel()
                            {
                                items = _items,
                            };

                            */

                            #endregion KKday 商品整合區塊 ---- end
                        }
                        catch (Exception co_ex)
                        {
                            // 回傳狀態
                            resp_code = TTdResultCodeEnum.INTERNAL_ERROR.GetHashCode().ToString("D4");
                            resp_msg = co_ex.Message;
                            resp_body = null;
                        }

                        break;
                    }
                // 订单新订 New order placed (*)
                case "CreateOrder":
                    {
                        try
                        {
                            var order = JObject.Parse(body).ToObject<CreateOrderReqModel>();

                            // 確認資料有無重複
                            var is_duplicated = tripOrderRepos.IsDuplicated(order.otaOrderId);
                            if (is_duplicated)
                            {
                                // 回傳代碼(Result Code)-訊息: 訂單已重複需回傳 otaItemOid
                                resp_code = TTdResultCodeEnum.SUCCESS.GetHashCode().ToString("D4");
                                resp_msg = $"otaOrderId='{ order.otaOrderId }' is duplicated!"; 
                                 
                                // 回傳-項目清單(Items)
                                var _items = new List<CreateOrderRespModel.OrderItemModel>();
                                foreach (var _item in order.items)
                                {
                                    var remaing_qty = productRepos.GetDummyProductQty(_item.PLU);

                                    _items.Add(new CreateOrderRespModel.OrderItemModel()
                                    {
                                        itemId = _item.itemId, // 重複的商品 id
                                        inventorys = new CreateOrderRespModel.OrderItemModel.OrderItemInventoryModel()
                                        {
                                            quantity = remaing_qty,
                                            useDate = _item.useStartDate
                                        }
                                    });
                                }

                                // 回傳-主體(Body)
                                resp_body = new CreateOrderRespModel()
                                {
                                    otaOrderId = order.otaOrderId,
                                    supplierOrderId = orderMasterRepos.GetOrderMasterMid(order.otaOrderId),
                                    supplierConfirmType = 1, // 1.新订已确认
                                    voucherSender = 2, // 2.供应商发送凭证
                                    items = _items,
                                };

                            }
                            else
                            {
                                var order_items = new List<CreateOrderReqModel.OrderItemModel>();

                                order.items.ForEach(i =>
                                {
                                    var dummyProd = productRepos.GetDummyProduct(i.PLU);

                                    // (1) 判斷产品ID异常，則回傳錯誤碼: 1001
                                    if (dummyProd == null)
                                    {
                                        resp_code = TTdResultCodeEnum.INVALID_PLU.GetHashCode().ToString("D4");
                                        resp_msg = $" sequenceId={ order.sequenceId } : 有無效的PLU";
                                        resp_body = null;

                                    }
                                    // (2) 判斷庫存不足, 則回傳錯誤碼: 1003
                                    else if (i.quantity > dummyProd.qty)
                                    {
                                        resp_code = TTdResultCodeEnum.INSUFFICIENT_INVENTORY.GetHashCode().ToString("D4");
                                        resp_msg = $" PLU={ i.PLU } : 庫存不足";

                                        // 回傳-項目清單(Items)
                                        var _items = new List<CreateOrderRespModel.OrderItemModel>();
                                        foreach (var _item in order_items)
                                        {
                                            _items.Add(new CreateOrderRespModel.OrderItemModel()
                                            {
                                                itemId = _item.itemId,
                                                inventorys = new CreateOrderRespModel.OrderItemModel.OrderItemInventoryModel()
                                                {
                                                    quantity = 0,
                                                    useDate = _item.useStartDate
                                                }
                                            });
                                        }

                                        // 回傳-主體(Body)
                                        resp_body = new CreateOrderRespModel()
                                        {
                                            items = _items,
                                        };
                                    }
                                    // (3) 判斷缺失出行人, 則回傳錯誤碼: 1005/1006
                                    else if (i.passengers.Count() < 1)
                                    {
                                        resp_code = TTdResultCodeEnum.MISSING_INFO.GetHashCode().ToString("D4");
                                        resp_msg = $" PLU={ i.PLU } : 缺失出行人";
                                        resp_body = null;
                                    }
                                    // (4) 判斷缺失证件(ageType, cardNo), 則回傳錯誤碼: 1005/1006
                                    else if (i.passengers.Where(p => string.IsNullOrEmpty(p.ageType) || string.IsNullOrEmpty(p.cardNo)).Count() > 0)
                                    {
                                        resp_code = TTdResultCodeEnum.MISSING_INFO.GetHashCode().ToString("D4");
                                        resp_msg = $" PLU={ i.PLU } : 缺失证件";
                                        resp_body = null;
                                    }
                                    // 檢驗無誤!! 加入待處理項目清單
                                    else
                                    {
                                        order_items.Add(i);
                                    }
                                });

                                if (order_items.Count() > 0)
                                {
                                    // 正常進單，新增 Trip 訂單紀錄 
                                    var tripOrder = new TripOrderModel()
                                    {
                                        sequence_id = order.sequenceId,
                                        ota_order_id = order.otaOrderId,
                                        confirm_type = order.confirmType,
                                        contacts = order.contacts,
                                        coupons = order.coupons,
                                        items = order.items,
                                        status = "NW",
                                        create_user = "SYSTEM"
                                    };

                                    // 回填 trip order 識別碼
                                    tripOrder.trip_order_oid = tripOrderRepos.Insert(tripOrder);

                                    /////////

                                    var result = orderMasterRepos.CreateDummyOrder(tripOrder);
                                    if (result != null)
                                    {
                                        var remaing_qty = productRepos.GetDummyProductQty(order.items.FirstOrDefault()?.PLU);

                                        // 回傳-結果代碼(Result Code)-成功
                                        resp_code = TTdResultCodeEnum.SUCCESS.GetHashCode().ToString("D4");
                                        resp_msg = "訂單建立成功";

                                        // 回傳-項目清單(Items)
                                        var _items = new List<CreateOrderRespModel.OrderItemModel>();
                                        foreach (var _item in order_items)
                                        {
                                            _items.Add(new CreateOrderRespModel.OrderItemModel()
                                            {
                                                itemId = _item.itemId,
                                                inventorys = new CreateOrderRespModel.OrderItemModel.OrderItemInventoryModel()
                                                {
                                                    quantity = remaing_qty,
                                                    useDate = _item.useStartDate
                                                }
                                            });
                                        }

                                        // 回傳-主體(Body)
                                        resp_body = new CreateOrderRespModel()
                                        {
                                            otaOrderId = order.otaOrderId,
                                            supplierOrderId = result.kkday_order_master_mid, // KKday 主訂單號
                                            supplierConfirmType = 1, // 1.新订已确认
                                            voucherSender = 2, // 2.供应商发送凭证
                                            items = _items,
                                        };
                                    }

                                    ///////

                                    #region KKday 商品整合區塊 ---- start

                                    /*
                                    // PLU 格式 => "PROD_PKG_ITEM,SKU"
                                    var tokens = PLU.Split(','); // openid 格式 => "PROD_PKG_ITEM,SKU"
                                    var locale = order.items.Select(i => i.locale).FirstOrDefault();
                                    var oid_params = tokens[0].Split('_'); 
                                    var prod_oid = oid_params.Length > 0 ? Convert.ToInt64(oid_params[0]) : 0;
                                    var pkg_oid = oid_params.Length > 1 ? Convert.ToInt64(oid_params[1]) : 0;
                                    var item_oid = oid_params.Length > 2 ? Convert.ToInt64(oid_params[2]) : 0;
                                    var sku_id = tokens[1];
                                    var guidToken = Guid.NewGuid().ToString("N");

                                    // 正嘗進單，新增 Trip 訂單紀錄 
                                    var tripOrder = new TripOrderModel()
                                    {
                                        sequence_id = order.sequenceId,
                                        ota_order_id = order.otaOrderId,
                                        confirm_type = order.confirmType,
                                        contacts = order.contacts,
                                        coupons = order.coupons,
                                        items = order.items,
                                        status = "NW",
                                        create_user = "SYSTEM"
                                    };

                                    var trip_order_oid = tripOrderRepos.Insert(tripOrder);

                                    #region 建立 KKday 訂單 --- start

                                    // 判斷幣別是否相符
                                    var curr_unmatch = order.items.Where(i => i.priceCurrency != Website.Instance.AgentCurrency).Select(i => i.priceCurrency).Distinct();
                                    if (curr_unmatch.Count() > 0) throw new InvalidOperationException($"currency = '{ string.Join(',', curr_unmatch) }' unmatched");

                                    //  取得套餐資料
                                    var prod = productRepos.GetProduct(token, prod_oid, locale);

                                    //  取得套餐資料
                                    var pkg = productRepos.GetPackage(token, prod_oid, pkg_oid, sku_id, locale);

                                    // 判斷套餐的庫存資料

                                    // 成立 KKday 訂單
                                    var result = orderMasterRepos.CreateOrder(trip_order_oid, PLU, locale, prod, pkg); 
                                    if (result.model != null)
                                    {
                                        // 回傳-結果代碼(Result Code)-成功
                                        resp_code = TTdResultCodeEnum.SUCCESS.GetHashCode().ToString("D4");
                                        resp_msg = "訂單建立成功";

                                        // 回傳-項目清單(Items)
                                        var _items = new List<CreateOrderRespModel.OrderItemModel>();
                                        foreach (var _item in order.items)
                                        {
                                            _items.Add(new CreateOrderRespModel.OrderItemModel()
                                            {
                                                itemId = _item.itemId,
                                                inventorys = new CreateOrderRespModel.OrderItemModel.OrderItemInventoryModel()
                                                {
                                                    quantity = result.qty,
                                                    useDate = usedDate
                                                }
                                            });
                                        }

                                        // 回傳-主體(Body)
                                        resp_body = new CreateOrderRespModel()
                                        {
                                            otaOrderId = order.otaOrderId,
                                            supplierOrderId = result.model.kkday_order_master_mid, // KKday 主訂單號
                                            supplierConfirmType = 1, // 1.新订已确认
                                            voucherSender = 2, // 2.供应商发送凭证
                                            items = _items,
                                        };

                                        #endregion 建立 KKday 訂單 --- end
                                    }
                                    */
                                    #endregion KKday 商品整合區塊 ---- end
                                } 

                            }
                        }
                        catch (Exception co_ex)
                        {
                            // 回傳狀態
                            resp_code = TTdResultCodeEnum.INTERNAL_ERROR.GetHashCode().ToString("D4");
                            resp_msg = co_ex.Message;
                            resp_body = null;
                        }

                        break;
                    }
                // 订单新订确认 New order confirmation (?)
                case "CreateOrderConfirm": break;

                // 订单出行通知 Order travel notification (?)
                case "OrderTravelNotice": break;

                // 订单退款 Order refund (X)
                case "RefundOrder": break;

                // 订单退款确认 Order refund confirmation (X)
                case "RefundOrderConfirm": break;

                // 订单核销通知 Order consumed notification (*)
                case "OrderConsumedNotice": break;

                // 订单取消 Order cancellation (*)
                case "CancelOrder":
                    {
                        var order = JObject.Parse(body).ToObject<CancelOrderReqModel>();

                        var tripOrder = tripOrderRepos.GetOrder(order.otaOrderId);
                        var orderMaster = orderMasterRepos.GetOrder(order.supplierOrderId);

                        // (1) 订单号异常
                        if (tripOrder == null)
                        {
                            resp_code = TTdResultCodeEnum.CX_ORDER_NOT_FOUND.GetHashCode().ToString("D4");
                            resp_msg = $"otaOrderId={ order.otaOrderId } : 找不到";

                            resp_body = null;
                        }
                        else if (orderMaster == null)
                        {
                            resp_code = TTdResultCodeEnum.CX_ORDER_NOT_FOUND.GetHashCode().ToString("D4");
                            resp_msg = $"supplierOrderId={ order.supplierOrderId } : 找不到";

                            resp_body = null;
                        }
                        // (2) 取消份数异常
                        else if (tripOrder.items.Sum(i=>i.quantity) != order.items.Sum(i => i.quantity))
                        {
                            resp_code = TTdResultCodeEnum.CX_QTY_INCORRECT.GetHashCode().ToString("D4");
                            resp_msg = $"otaOrderId={ order.otaOrderId } : 取消份数异常";

                            resp_body = null;
                        }
                        // (3) 重复取消
                        else if (orderMaster.status == "CX")
                        {
                            resp_code = TTdResultCodeEnum.SUCCESS.GetHashCode().ToString("D4");
                            resp_msg = $"otaOrderId={ order.otaOrderId } : 重复取消";

                            // 回傳-項目清單(Items)
                            var _items = new List<CancelOrderRespModel.ItemNodeModel>();
                            foreach (var _item in order.items)
                            {
                                _items.Add(new CancelOrderRespModel.ItemNodeModel()
                                {
                                    itemId = _item.itemId
                                });
                            }

                            // 回傳-主體(Body)
                            resp_body = new CancelOrderRespModel()
                            {
                                supplierConfirmType = 1,
                                items = _items,
                            };
                        }
                        // 檢查無誤!
                        else
                        {
                            var result = orderMasterRepos.CancelOrder(order.supplierOrderId);
                            if (result)
                            {
                                // 回傳-結果代碼(Result Code)-成功
                                resp_code = TTdResultCodeEnum.SUCCESS.GetHashCode().ToString("D4");
                                resp_msg = "訂單取消申請成功";

                                // 回傳-項目清單(Items)
                                var _items = new List<CancelOrderRespModel.ItemNodeModel>();
                                foreach (var _item in order.items)
                                {
                                    _items.Add(new CancelOrderRespModel.ItemNodeModel()
                                    {
                                        itemId = _item.itemId
                                    });
                                }

                                // 回傳-主體(Body)
                                resp_body = new CancelOrderRespModel()
                                {
                                    supplierConfirmType = 1,
                                    items = _items,
                                };
                            }
                        }

                        break;
                    }
                // 订单取消确认 Order cancellation confirmation (?)
                case "CancelOrderConfirm": break;

                // 订单查询 Order inquiry (*)
                case "QueryOrder":
                    {
                        var order = JObject.Parse(body).ToObject<QueryOrderReqModel>();

                        var tripOrder = tripOrderRepos.GetOrder(order.otaOrderId);
                        var masters = orderMasterRepos.QueryOrder(order.supplierOrderId);

                        // (1) 订单号异常
                        if (tripOrder == null)
                        {
                            resp_code = TTdResultCodeEnum.QUERY_ORDER_NOT_FOUND.GetHashCode().ToString("D4");
                            resp_msg = $"otaOrderId={ order.otaOrderId } : 找不到";

                            resp_body = null;
                        }
                        else if (masters == null)
                        {
                            resp_code = TTdResultCodeEnum.QUERY_ORDER_NOT_FOUND.GetHashCode().ToString("D4");
                            resp_msg = $"supplierOrderId={ order.supplierOrderId } : 找不到";

                            resp_body = null;
                        }
                        // 檢查無誤!
                        else
                        {
                            // 回傳-結果代碼(Result Code)-成功
                            resp_code = TTdResultCodeEnum.SUCCESS.GetHashCode().ToString("D4");
                            resp_msg = "訂單查詢成功";

                            // 回傳-項目清單(Items)
                            var masterOrder = masters.FirstOrDefault();
                            var _items = new List<QueryOrderRespModel.ItemNodeModel>();
                            var _orderStatus = TripOrderStatusMapping.Convert(masterOrder.status);
                            _items.Add(new QueryOrderRespModel.ItemNodeModel()
                            {
                                itemId = masterOrder.trip_item_id,
                                orderStatus = _orderStatus,
                                useStartDate = masterOrder.use_start_date,
                                useEndDate = masterOrder.use_end_date,
                                quantity = 1,
                                useQuantity = 0,
                                cancelQuantity = _orderStatus == (int)TTdOrderStatusEnum.ALL_CANCELED ? 1 : 0,
                            });

                            // 回傳-主體(Body)
                            resp_body = new QueryOrderRespModel()
                            {
                                otaOrderId = masterOrder.ota_order_id,
                                supplierOrderId = masterOrder.kkday_order_master_mid,
                                items = _items
                            };
                        }

                        break;
                    }

                // 订单凭证发送 Order voucher sending (X)
                case "SendVoucher": break;

                // 订单修改 Order modify (X)
                case "EditOrder": break;

                // 订单修改确认 Order modify confirmation (X)
                case "EditOrderConfirm": break;

                default: break;
            }


            // 回傳結果
            body = JsonConvert.SerializeObject(resp_body);
            resp = new TTdResponseModel()
            {
                header = new TTdResponseModel.TTdResponseHeaderModel()
                {
                    resultCode = resp_code,
                    resultMessage = resp_msg
                },
                body = TripAesCryptHelper.Encrypt(body, secretKey, aesIV)
            };


            logRepos.SetResponse(log_oid, JsonConvert.SerializeObject(resp), body, "KKDAY");

            return resp;
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
