using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
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
    [AllowAnonymous]
    public class PasvSandBoxController : Controller
    {
        private readonly IMemoryCache _cache;
        private readonly string secretKey;
        private readonly string aesIV;

        public PasvSandBoxController(IMemoryCache memoryCache)
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
                                int invalid_count = 0;

                                if (!string.IsNullOrEmpty(dummyProd?.param1.GetValue("condition")?.ToString()))
                                {
                                    var cond = dummyProd.param1.GetValue("condition").ToString();


                                    if (cond.IndexOf("ageType") != -1)
                                    {
                                        invalid_count += (i.passengers.Where(p => string.IsNullOrEmpty(p.ageType)).Count() > 0) ? 1 : 0;
                                    }

                                    if (cond.IndexOf("cardNo") != -1)
                                    {
                                        invalid_count += (i.passengers.Where(p => string.IsNullOrEmpty(p.cardNo)).Count() > 0) ? 1 : 0;
                                    }
                                }

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
                                // (4) 判斷缺失证件(ageType, cardNo, etc), 則回傳錯誤碼: 1005/1006 
                                else if (invalid_count > 0)
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
                                var dummyProd = productRepos.GetDummyProduct(order.items.FirstOrDefault()?.PLU);
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
                                    supplierConfirmType = Convert.ToInt32(dummyProd.param1.GetValue("supplierConfirmType")??1),
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
                                    int invalid_spec_count = 0;

                                    if (!string.IsNullOrEmpty(dummyProd?.param1.GetValue("condition")?.ToString()))
                                    { 
                                        var cond = dummyProd.param1.GetValue("condition").ToString();
                                        if (cond.IndexOf("ageType") != -1)
                                        {
                                            invalid_spec_count += (i.passengers.Where(p => string.IsNullOrEmpty(p.ageType)).Count() > 0) ? 1 : 0;
                                        }

                                        if (cond.IndexOf("cardNo") != -1)
                                        {
                                            invalid_spec_count += (i.passengers.Where(p => string.IsNullOrEmpty(p.cardNo)).Count() > 0) ? 1 : 0;
                                        }
                                    }

                                    // (1) 判斷产品ID异常，則回傳錯誤碼: 1001
                                    if (dummyProd == null)
                                    {
                                        resp_code = TTdResultCodeEnum.INVALID_PLU.GetHashCode().ToString("D4");
                                        resp_msg = $" sequenceId={ order.sequenceId } : 有無效的PLU";
                                        resp_body = null;
                                        return;
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
                                        return;
                                    }
                                    // (4) 判斷缺失证件(ageType, cardNo), 則回傳錯誤碼: 1005/1006
                                    else if (invalid_spec_count > 0)
                                    {
                                        resp_code = TTdResultCodeEnum.MISSING_INFO.GetHashCode().ToString("D4");
                                        resp_msg = $" PLU={ i.PLU } : 缺失证件";
                                        resp_body = null;
                                        return;

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
                                        Console.WriteLine($" ===> New order_master_mid={ result.order_master_mid }");

                                        var dummyProd = productRepos.GetDummyProduct(order.items.FirstOrDefault()?.PLU);
                                         
                                        // 回傳-結果代碼(Result Code)-成功
                                        resp_code = TTdResultCodeEnum.SUCCESS.GetHashCode().ToString("D4");
                                        resp_msg = "訂單建立成功";

                                        // 回傳-項目清單(Items)
                                        var _items = new List<CreateOrderRespModel.OrderItemModel>();
                                        foreach (var _item in order_items)
                                        {
                                            var remaing_qty = productRepos.GetDummyProductQty(order.items.FirstOrDefault()?.PLU);
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
                                            supplierOrderId = result.order_master_mid, // KKday 主訂單號
                                            supplierConfirmType = Convert.ToInt32(dummyProd.param1.GetValue("supplierConfirmType") ?? 1),
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
                                            supplierOrderId = result.model.order_master_mid, // KKday 主訂單號
                                            supplierConfirmType = Convert.ToInt32(dummyProd.param1.GetValue("supplierConfirmType") ?? 1),
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
                        var dummyProd = productRepos.GetDummyProduct(order.items.FirstOrDefault()?.PLU);

                        // (1) 订单号异常 : 2001
                        if (tripOrder == null)
                        {
                            resp_code = TTdResultCodeEnum.CX_ORDER_NOT_FOUND.GetHashCode().ToString("D4");
                            resp_msg = $"otaOrderId={ order.otaOrderId } : 找不到";

                            resp_body = null;
                        }
                        // (1) 订单号异常: 2001
                        else if (orderMaster == null)
                        {
                            resp_code = TTdResultCodeEnum.CX_ORDER_NOT_FOUND.GetHashCode().ToString("D4");
                            resp_msg = $"supplierOrderId={ order.supplierOrderId } : 找不到";

                            resp_body = null;
                        }
                        // (2) 取消份数异常: 2004
                        else if (tripOrder.items.Sum(i=>i.quantity) != order.items.Sum(i => i.quantity))
                        {
                            resp_code = TTdResultCodeEnum.CX_QTY_INCORRECT.GetHashCode().ToString("D4");
                            resp_msg = $"otaOrderId={ order.otaOrderId } : 取消份数异常";

                            resp_body = null;
                        }
                        // (3) 重复取消: 0000
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
                                supplierConfirmType = Convert.ToInt32(dummyProd.param1.GetValue("supplierConfirmType") ?? 1),
                                items = _items,
                            };
                        }
                        // (4) 该订单已经使用: 2002
                        else if (orderMaster != null && orderMaster.status == "GO_OK")
                        {
                            resp_code = TTdResultCodeEnum.CX_ORDER_USED.GetHashCode().ToString("D4");
                            resp_msg = $"supplierOrderId={ order.supplierOrderId } : 该订单已经使用";

                            resp_body = null;
                        }
                        // 檢查無誤!
                        else
                        { 
                            var result = orderMasterRepos.UpdateCancelApply(order.supplierOrderId, order.sequenceId);
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
                                    //   1.取消已确认 (当confirmType = 1, 需同步返回确认结果) 
                                    // * 2.取消待确认 (当confirmType = 2, 需异步返回确认结果)
                                    supplierConfirmType = 2,
                                    items = _items,
                                };
                            }
                        }

                        break;
                    }

                // 订单查询 Order inquiry (*)
                case "QueryOrder":
                    {
                        var order = JObject.Parse(body).ToObject<QueryOrderReqModel>();

                        var tripOrder = tripOrderRepos.GetOrder(order.otaOrderId);
                        var masters = orderMasterRepos.QueryOrder(order.supplierOrderId);

                        // (1) 订单号异常: 4001
                        if (tripOrder == null)
                        {
                            resp_code = TTdResultCodeEnum.QUERY_ORDER_NOT_FOUND.GetHashCode().ToString("D4");
                            resp_msg = $"otaOrderId={ order.otaOrderId } : 找不到";

                            resp_body = null;
                        }
                        // (1) 订单号异常: 4001
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
                                itemId = masterOrder.ota_item_id,
                                orderStatus = _orderStatus,
                                useStartDate = masterOrder.use_start_date,
                                useEndDate = masterOrder.use_end_date,
                                quantity = 1,
                                useQuantity = masterOrder.status.Equals("GO_OK")  ? masterOrder.use_quantity : 0,
                                cancelQuantity = _orderStatus == (int)TTdOrderStatusEnum.ALL_CANCELED ? 1 : 0,
                            });

                            // 回傳-主體(Body)
                            resp_body = new QueryOrderRespModel()
                            {
                                otaOrderId = masterOrder.ota_order_id.ToString(),
                                supplierOrderId = masterOrder.order_master_mid,
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

        ///////////

        #region 订单新订确认 New order confirmation --- start
 
        // POST api/Pasv/OrderConfirm
        [HttpGet("OrderConfirm/{id?}")]
        public string OrderConfirm(string id)
        {
            var result = "";

            try
            {
                var logRepos = HttpContext.RequestServices.GetService<TripTransLogRepository>();
                var orderMasterRepos = HttpContext.RequestServices.GetService<OrderMasterRepository>();
                var tripOrderRepos = HttpContext.RequestServices.GetService<TripOrderRepository>();
                var ttdOpenProxy = HttpContext.RequestServices.GetService<Proxy.TtdOpenProxy>();

                // 取出訂單
                var kkdayOrder = orderMasterRepos.GetOrder(order_master_mid: id);
                var tripOrder = tripOrderRepos.GetOrder(kkdayOrder.ota_order_id);

                // 準備請求參數
                var _header = new TTdReqHeaderModel()
                {
                    accountId = Website.Instance.AgentAccount,
                    serviceName = "CreateOrderConfirm",
                    requestTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"), // "2017-01-05 10:00:00",
                    version = "1.0",
                    sign = "",
                };

                var _vouchers = new List<NewOrderConfirmationReqModel.VouchersModel>();
                _vouchers.Add(new NewOrderConfirmationReqModel.VouchersModel()
                {

                });

                var _items = new List<NewOrderConfirmationReqModel.ItemModel>();
                tripOrder.items.ForEach(i =>
                {
                    var _inventorys = new List<NewOrderConfirmationReqModel.ItemModel.InventoryModel>();

                    _items.Add(new NewOrderConfirmationReqModel.ItemModel()
                    {
                        itemId = i.itemId,
                        inventorys = _inventorys
                    });
                });

                var _body = new NewOrderConfirmationReqModel()
                {
                    sequenceId = tripOrder.sequence_id,
                    otaOrderId = tripOrder.ota_order_id,
                    supplierOrderId = kkdayOrder.order_master_mid,
                    confirmResultCode = "0000",
                    confirmResultMessage = "确认信息",
                    voucherSender = 2,// 1. Ctrip sends voucher 2. Suppliers send the voucher
                    vouchers = _vouchers,
                    items = _items
                };

                var _encryBody = TripAesCryptHelper.Encrypt(JsonConvert.SerializeObject(_body), secretKey, aesIV);

                // 更新 header.sign 值
                _header.sign = Md5Helper.ToMD5($"{ _header.accountId }{ _header.serviceName }{ _header.requestTime }{ _encryBody }{ _header.version }{ Website.Instance.SignKey }").ToLower();

                var req = new Dictionary<string, object>();
                req.Add("header", _header);
                req.Add("body", _encryBody);

                var jsonData = JsonConvert.SerializeObject(req);
                var log_oid = logRepos.Insert(jsonData, JsonConvert.SerializeObject(_body), "KKDAY");

                // 回調 Ctrip ttdstpAPI
                result = ttdOpenProxy.PostAsync(jsonData).GetAwaiter().GetResult(); 
                logRepos.SetResponse(log_oid, result, null, "TRIP");

                // 判斷回傳結果
                if (result?.IndexOf("header") != -1)
                {
                    var resp = JObject.Parse(result);
                    var resp_header = resp["header"].ToObject<TTdResponseModel.TTdResponseHeaderModel>();
                }
            }
            catch(Exception ex)
            {
                Website.Instance.logger.Fatal($"OrderConfirm Exception, Message={ex.Message},StackTrace={ex.StackTrace}");
            }

            return result;
        }

        #endregion 订单新订确认 New order confirmation --- end

        ///////////

        #region 订单取消确认 Order cancellation confirmation --- start

        // POST api/Pasv/CancelOrderConfirm
        [HttpGet("CancelOrderConfirm/{id?}")]
        public string CancelOrderConfirm(string id, bool is_reject)
        {
            var result = "";

            try
            {
                var logRepos = HttpContext.RequestServices.GetService<TripTransLogRepository>();
                var orderMasterRepos = HttpContext.RequestServices.GetService<OrderMasterRepository>();
                var tripOrderRepos = HttpContext.RequestServices.GetService<TripOrderRepository>();
                var ttdOpenProxy = HttpContext.RequestServices.GetService<Proxy.TtdOpenProxy>();

                // 取出訂單
                var kkdayOrder = orderMasterRepos.GetOrder(order_master_mid: id);
                var tripOrder = tripOrderRepos.GetOrder(kkdayOrder.ota_order_id);

                // 準備請求參數
                var _header = new TTdReqHeaderModel()
                {
                    accountId = Website.Instance.AgentAccount,
                    serviceName = "CancelOrderConfirm",
                    requestTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"), // "2017-01-05 10:00:00",
                    version = "1.0"
                };
                 
                var _items = new List<CancelOrderConfirmReqModel.ItemModel>();
                tripOrder.items.ForEach(i =>
                { 
                    _items.Add(new CancelOrderConfirmReqModel.ItemModel()
                    {
                        itemId = i.itemId
                    });
                });

                var _body = new CancelOrderConfirmReqModel()
                {
                    sequenceId = kkdayOrder.ota_sequence_id,
                    otaOrderId = tripOrder.ota_order_id,
                    supplierOrderId = kkdayOrder.order_master_mid,
                    confirmResultCode = "0000",
                    confirmResultMessage = "确认信息", 
                    items = _items
                };

                if(is_reject)
                {
                    _body.confirmResultCode = "2001";
                    _body.confirmResultMessage = "取消失敗";
                }

                var _encryBody = TripAesCryptHelper.Encrypt(JsonConvert.SerializeObject(_body), secretKey, aesIV);

                // 更新 header.sign 值
                _header.sign = Md5Helper.ToMD5($"{ _header.accountId }{ _header.serviceName }{ _header.requestTime }{ _encryBody }{ _header.version }{ Website.Instance.SignKey }").ToLower();
                 
                var req = new Dictionary<string, object>();
                req.Add("header", _header);
                req.Add("body", _encryBody);

                var jsonData = JsonConvert.SerializeObject(req);
                var log_oid = logRepos.Insert(jsonData, JsonConvert.SerializeObject(_body), "KKDAY");

                // 回調 Ctrip ttdstpAPI
                result = ttdOpenProxy.PostAsync(jsonData).GetAwaiter().GetResult();
                logRepos.SetResponse(log_oid, result, null, "TRIP");

                // 判斷回傳結果
                if (result?.IndexOf("header") != -1)
                {
                    var resp = JObject.Parse(result);
                    var resp_header = resp["header"].ToObject<TTdResponseModel.TTdResponseHeaderModel>();
                } 
            }
            catch (Exception ex)
            {
                Website.Instance.logger.Fatal($"OrderConfirm Exception, Message={ex.Message},StackTrace={ex.StackTrace}");
            }

            return result;
        }

        #endregion 订单取消确认 Order cancellation confirmation --- end

        ///////////

        #region 订单核销通知 Order Consumed Notice --- start

        // POST api/Pasv/OrderConsumedNotice
        [HttpGet("OrderConsumedNotice/{id?}")]
        public string OrderConsumedNotice(string id)
        {
            var result = "";

            try
            {
                var logRepos = HttpContext.RequestServices.GetService<TripTransLogRepository>();
                var orderMasterRepos = HttpContext.RequestServices.GetService<OrderMasterRepository>();
                var tripOrderRepos = HttpContext.RequestServices.GetService<TripOrderRepository>();
                var ttdOpenProxy = HttpContext.RequestServices.GetService<Proxy.TtdOpenProxy>();

                // 取出訂單
                var kkdayOrder = orderMasterRepos.GetOrder(order_master_mid: id);
                var tripOrder = tripOrderRepos.GetOrder(kkdayOrder.ota_order_id);
                 
                // 準備請求參數
                var _header = new TTdReqHeaderModel()
                {
                    accountId = Website.Instance.AgentAccount,
                    serviceName = "OrderConsumedNotice",
                    requestTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"), // "2017-01-05 10:00:00",
                    version = "1.0"
                };

                var _items = new List<OrderConsumedNoticeReqModel.ItemModel>();
                tripOrder.items.ForEach(i =>
                {
                    _items.Add(new OrderConsumedNoticeReqModel.ItemModel()
                    {
                        itemId = i.itemId,
                        quantity = 1,
                        useQuantity = 1
                    });
                });

                var _body = new OrderConsumedNoticeReqModel()
                {
                    sequenceId = tripOrder.sequence_id,
                    otaOrderId = tripOrder.ota_order_id,
                    supplierOrderId = kkdayOrder.order_master_mid,
                    items = _items
                };

                var _encryBody = TripAesCryptHelper.Encrypt(JsonConvert.SerializeObject(_body), secretKey, aesIV);

                // 更新 header.sign 值
                _header.sign = Md5Helper.ToMD5($"{ _header.accountId }{ _header.serviceName }{ _header.requestTime }{ _encryBody }{ _header.version }{ Website.Instance.SignKey }").ToLower();

                var req = new Dictionary<string, object>();
                req.Add("header", _header);
                req.Add("body", _encryBody);

                var jsonData = JsonConvert.SerializeObject(req);
                var log_oid = logRepos.Insert(jsonData, JsonConvert.SerializeObject(_body), "KKDAY");

                // 回調 Ctrip ttdstpAPI
                result = ttdOpenProxy.PostAsync(jsonData).GetAwaiter().GetResult();
                logRepos.SetResponse(log_oid, result, null, "TRIP");

                // 判斷回傳結果
                if (result?.IndexOf("header") != -1)
                {
                    var resp = JObject.Parse(result);
                    var resp_header = resp["header"].ToObject<TTdResponseModel.TTdResponseHeaderModel>();
                }
            }
            catch (Exception ex)
            {
                Website.Instance.logger.Fatal($"OrderConfirm Exception, Message={ex.Message},StackTrace={ex.StackTrace}");
            }

            return result;
        }

        #endregion 订单核销通知 Order Consumed Notice --- end

        ///////////

        #region 订单出行通知 Order Travel Notice --- start

        // POST api/Pasv/OrderTravelNotice
        [HttpGet("OrderTravelNotice/{id?}")]
        public string OrderTravelNotice(string id)
        {
            var result = "";

            try
            {
                var logRepos = HttpContext.RequestServices.GetService<TripTransLogRepository>();
                var orderMasterRepos = HttpContext.RequestServices.GetService<OrderMasterRepository>();
                var tripOrderRepos = HttpContext.RequestServices.GetService<TripOrderRepository>();
                var ttdOpenProxy = HttpContext.RequestServices.GetService<Proxy.TtdOpenProxy>();
                var productRepos = HttpContext.RequestServices.GetService<ProductRepository>(); 

                // 取出訂單
                var kkdayOrder = orderMasterRepos.GetOrder(order_master_mid: id);
                var tripOrder = tripOrderRepos.GetOrder(kkdayOrder.ota_order_id); 
                var dummyProd = productRepos.GetDummyProduct(tripOrder.items.FirstOrDefault()?.PLU);

                // 準備請求參數
                var _header = new TTdReqHeaderModel()
                {
                    accountId = Website.Instance.AgentAccount,
                    serviceName = "OrderTravelNotice",
                    requestTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"), // "2017-01-05 10:00:00",
                    version = "1.0"
                };
                 
                var _items = new List<OrderTravelNoticeReqModel.ItemModel>();
                tripOrder.items.ForEach(i =>
                {
                    
                    var _expressDelivery = new OrderTravelNoticeReqModel.ItemModel.ExpressDeliveryModel()
                    {
                        deliveryType = Convert.ToInt32(dummyProd.param1.GetValue("deliveryType"))
                    };

                    _items.Add(new OrderTravelNoticeReqModel.ItemModel()
                    {
                        itemId = i.itemId, 
                        expressDelivery = _expressDelivery
                    });
                });

                var _body = new OrderTravelNoticeReqModel()
                {
                    sequenceId = tripOrder.sequence_id,
                    otaOrderId = tripOrder.ota_order_id,
                    supplierOrderId = kkdayOrder.order_master_mid,
                    items = _items


                };

                var _encryBody = TripAesCryptHelper.Encrypt(JsonConvert.SerializeObject(_body), secretKey, aesIV);

                // 更新 header.sign 值
                _header.sign = Md5Helper.ToMD5($"{ _header.accountId }{ _header.serviceName }{ _header.requestTime }{ _encryBody }{ _header.version }{ Website.Instance.SignKey }").ToLower();

                var req = new Dictionary<string, object>();
                req.Add("header", _header);
                req.Add("body", _encryBody);

                var jsonData = JsonConvert.SerializeObject(req);
                var log_oid = logRepos.Insert(jsonData, JsonConvert.SerializeObject(_body), "KKDAY");

                // 回調 Ctrip ttdstpAPI
                result = ttdOpenProxy.PostAsync(jsonData).GetAwaiter().GetResult();
                logRepos.SetResponse(log_oid, result, null, "TRIP");

                // 判斷回傳結果
                if (result?.IndexOf("header") != -1)
                {
                    var resp = JObject.Parse(result);
                    var resp_header = resp["header"].ToObject<TTdResponseModel.TTdResponseHeaderModel>();
                }
            }
            catch (Exception ex)
            {
                Website.Instance.logger.Fatal($"OrderConfirm Exception, Message={ex.Message},StackTrace={ex.StackTrace}");
            }

            return result;
        }

        #endregion 订单出行通知 Order Travel Notice --- end

        ///////////

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
