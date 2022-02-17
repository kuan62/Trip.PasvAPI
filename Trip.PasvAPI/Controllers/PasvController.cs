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
using Trip.PasvAPI.Proxy; 

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Trip.PasvAPI.Controllers
{
    [Route("api/[controller]")]
    [AllowAnonymous]
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
            var mapRepos = HttpContext.RequestServices.GetService<ProductMapRepository>(); // 商品映射
            var prodRepos = HttpContext.RequestServices.GetService<ProductRepository>(); // KKday 商品
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
            //var req_sign = Md5Helper.ToMD5($"{req_header["accountId"]}{req_header["serviceName"]}{req_header["requestTime"]}{req["body"]}{req_header["version"]}{Website.Instance.SignKey}").ToLower();
            //if (!req_header["sign"].ToString().Equals(req_sign))
            //{
            //    // 回傳錯誤結果
            //    resp_code = TTdResultCodeEnum.WRONG_SIGNATURE.GetHashCode().ToString("D4");
            //    resp_msg = "签名错误"; 

            //    resp = new TTdResponseModel()
            //    {
            //        header = new TTdResponseModel.TTdResponseHeaderModel()
            //        {
            //            resultCode = resp_code,
            //            resultMessage = resp_msg
            //        }
            //    }; 

            //    logRepos.SetResponse(log_oid, JsonConvert.SerializeObject(resp),  null, "KKDAY");

            //    return resp;
            //}

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
                                var map_prod = mapRepos.GetProductMapByPLU(i.PLU);
                                int invalid_count = 0;

                                // (1) 判斷产品ID异常，則回傳錯誤碼: 1001
                                if (map_prod == null)
                                {
                                    resp_code = TTdResultCodeEnum.INVALID_PLU.GetHashCode().ToString("D4");
                                    resp_msg = $" sequenceId={ order.sequenceId } : 有無效的PLU";
                                    resp_body = null;
                                }
                                else
                                {

                                    //if (!string.IsNullOrEmpty(prod?.param1.GetValue("condition")?.ToString()))
                                    //{
                                    //    var cond = dummyProd.param1.GetValue("condition").ToString(); 

                                    //    if (cond.IndexOf("ageType") != -1)
                                    //    {
                                    //        invalid_count += (i.passengers.Where(p => string.IsNullOrEmpty(p.ageType)).Count() > 0) ? 1 : 0;
                                    //    }

                                    //    if (cond.IndexOf("cardNo") != -1)
                                    //    {
                                    //        invalid_count += (i.passengers.Where(p => string.IsNullOrEmpty(p.cardNo)).Count() > 0) ? 1 : 0;
                                    //    }
                                    //}

                                    var remaing_qty = prodRepos.GetPackageQty(token, Convert.ToInt64(map_prod.prod_oid), Convert.ToInt64(map_prod.pkg_oid),
                                                        map_prod.sku_id, order.items[0].useStartDate);


                                    // (2) 判斷庫存不足, 則回傳錯誤碼: 1003
                                    if (i.quantity > remaing_qty)
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
                                    /*  
                                    // (3) 判斷缺失出行人, 則回傳錯誤碼: 1005/1006...(沙箱測試)
                                    else if (i.passengers.Count() < 1)
                                    {
                                        resp_code = TTdResultCodeEnum.MISSING_INFO.GetHashCode().ToString("D4");
                                        resp_msg = $" PLU={ i.PLU } : 缺失出行人";
                                        resp_body = null;
                                    }
                                    // (4) 判斷缺失证件(ageType, cardNo, etc), 則回傳錯誤碼: 1005/1006...(沙箱測試)
                                    else if (invalid_count > 0)
                                    {
                                        resp_code = TTdResultCodeEnum.MISSING_INFO.GetHashCode().ToString("D4");
                                        resp_msg = $" PLU={ i.PLU } : 缺失证件";
                                        resp_body = null;
                                    }
                                    */
                                    // 檢驗無誤!! 加入待處理項目清單
                                    else
                                    {
                                        order_items.Add(i);
                                    }
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
                                var map_prod = mapRepos.GetProductMapByPLU(order.items[0].PLU);
                                var _items = new List<CreateOrderRespModel.OrderItemModel>();

                                if (map_prod != null)
                                {
                                    foreach (var _item in order.items)
                                    {
                                        var remaing_qty = prodRepos.GetPackageQty(token, Convert.ToInt64(map_prod.prod_oid), Convert.ToInt64(map_prod.pkg_oid),
                                                            map_prod.sku_id, order.items[0].useStartDate);

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
                                }

                                // 回傳-主體(Body)
                                resp_body = new CreateOrderRespModel()
                                {
                                    otaOrderId = order.otaOrderId,
                                    supplierOrderId = orderMasterRepos.GetOrderMasterMid(order.otaOrderId),
                                    supplierConfirmType = 2, // 2.新订待确认 (搭配 CreateOrderConfirm)
                                    voucherSender = 1, // 1.携程发送凭证
                                    items = _items,
                                };
                            }
                            else
                            {
                                var order_items = new List<CreateOrderReqModel.OrderItemModel>();
                                var map_prod = mapRepos.GetProductMapByPLU(order.items[0].PLU);

                                // (1) 判斷产品ID异常，則回傳錯誤碼: 1001
                                if (map_prod == null)
                                {
                                    resp_code = TTdResultCodeEnum.INVALID_PLU.GetHashCode().ToString("D4");
                                    resp_msg = $" sequenceId={ order.sequenceId } : 有無效的PLU";
                                    resp_body = null;
                                }
                                else
                                {
                                    var remaing_qty = (map_prod == null) ? 0 : prodRepos.GetPackageQty(token, Convert.ToInt64(map_prod.prod_oid), Convert.ToInt64(map_prod.pkg_oid),
                                                        map_prod.sku_id, order.items[0].useStartDate);

                                    order.items.ForEach(i =>
                                    {
                                        int invalid_spec_count = 0;

                                        // (2) 判斷庫存不足, 則回傳錯誤碼: 1003
                                        if (i.quantity > remaing_qty)
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
                                        /*
                                        // (3) 判斷缺失出行人, 則回傳錯誤碼: 1005/1006...(沙箱測試)
                                        else if (i.passengers.Count() < 1)
                                        {
                                            resp_code = TTdResultCodeEnum.MISSING_INFO.GetHashCode().ToString("D4");
                                            resp_msg = $" PLU={ i.PLU } : 缺失出行人";
                                            resp_body = null;
                                            return;
                                        }
                                        // (4) 判斷缺失证件(ageType, cardNo), 則回傳錯誤碼: 1005/1006...(沙箱測試)
                                        else if (invalid_spec_count > 0)
                                        {
                                            resp_code = TTdResultCodeEnum.MISSING_INFO.GetHashCode().ToString("D4");
                                            resp_msg = $" PLU={ i.PLU } : 缺失证件";
                                            resp_body = null;
                                            return;

                                        }
                                        */
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

                                        var bookingRepos = HttpContext.RequestServices.GetService<OrderBookingRepository>();
                                        var result = bookingRepos.crtOrder(order);

                                        if (result != null)
                                        {
                                            Console.WriteLine($" ===> New order_master_mid={ result.order_master_mid }");

                                            // 訂購成功
                                            if (result.resp_code.Equals("00") || result.resp_code.Equals("0000"))
                                            {
                                                // 再查一次剩餘位控
                                                remaing_qty = prodRepos.GetPackageQty(token, Convert.ToInt64(map_prod.prod_oid), Convert.ToInt64(map_prod.pkg_oid),
                                                                map_prod.sku_id, order.items[0].useStartDate);

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
                                                    supplierOrderId = result.order_master_mid, // KKday 主訂單號
                                                    supplierConfirmType = 2, // 2.新订待确认（当 confirmType =2时需异步返回确认结果的）
                                                    voucherSender = 1, // 1.携程发送凭证
                                                    items = _items,
                                                };
                                            }
                                            // 訂購失敗
                                            else
                                            {
                                                resp_code = result.resp_code;
                                                resp_msg = result.resp_msg;
                                                resp_body = null;
                                            } 
                                        }

                                    }
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

                        var orderProxy = HttpContext.RequestServices.GetService<OrderProxy>();
                        var tripOrder = tripOrderRepos.GetOrder(order.otaOrderId);
                        var master = orderMasterRepos.GetOrder(order_master_mid: order.supplierOrderId, ota_order_id: order.otaOrderId);
                        var map_prod = mapRepos.GetProductMapByPLU(order.items.FirstOrDefault()?.PLU);

                        var json_result = orderProxy.GetOrderDetail(master.order_mid, Website.Instance.B2dApiAuthorToken);
                        var kkday_order = JsonConvert.DeserializeObject<OrderDetailModel>(json_result);
                          
                        // (1) 订单号异常 : 2001
                        if (tripOrder == null)
                        {
                            resp_code = TTdResultCodeEnum.CX_ORDER_NOT_FOUND.GetHashCode().ToString("D4");
                            resp_msg = $"otaOrderId={ order.otaOrderId } : 找不到";

                            resp_body = null;
                        }
                        else if (master == null)
                        {
                            resp_code = TTdResultCodeEnum.CX_ORDER_NOT_FOUND.GetHashCode().ToString("D4");
                            resp_msg = $"supplierOrderId={ order.supplierOrderId } : 找不到";

                            resp_body = null;
                        }

                        // (2) 取消份数异常: 2004
                        if (tripOrder.items.Sum(i=>i.quantity) != order.items.Sum(i => i.quantity))
                        {
                            resp_code = TTdResultCodeEnum.CX_QTY_INCORRECT.GetHashCode().ToString("D4");
                            resp_msg = $"otaOrderId={ order.otaOrderId } : 取消份数异常";

                            resp_body = null;
                        }
                        // (3) 重复取消: 0000
                        else if (master.status == "CX")
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
                        // (4) 该订单已经使用: 2002
                        else if (kkday_order != null && kkday_order.order_status == "BACK")
                        {
                            resp_code = TTdResultCodeEnum.CX_ORDER_USED.GetHashCode().ToString("D4");
                            resp_msg = $"supplierOrderId={ order.supplierOrderId } : 该订单已经使用";

                            resp_body = null;
                        }
                        // 檢查無誤!
                        else
                        {
                            // 呼叫 B2D API 進行訂單取消, "MC001" (Tour changed or cancel)
                            var b2d_result = orderProxy.CancelApply(master.order_mid, "MC001", Website.Instance.B2dApiAuthorToken);

                            // 更改本地訂單記錄為 CX_ING
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

                        var orderProxy = HttpContext.RequestServices.GetService<OrderProxy>();
                        var tripOrder = tripOrderRepos.GetOrder(order.otaOrderId);
                        var master = orderMasterRepos.GetOrder(order_master_mid: order.supplierOrderId, ota_order_id: order.otaOrderId);

                        var json_result = orderProxy.GetOrderDetail(master.order_mid, Website.Instance.B2dApiAuthorToken);
                        var kkday_order = JsonConvert.DeserializeObject<OrderDetailModel>(json_result);
                         
                        // (1) 订单号异常: 4001
                        if (tripOrder == null)
                        {
                            resp_code = TTdResultCodeEnum.QUERY_ORDER_NOT_FOUND.GetHashCode().ToString("D4");
                            resp_msg = $"otaOrderId={ order.otaOrderId } : 找不到";

                            resp_body = null;
                        }
                        else if (kkday_order == null || kkday_order.result != "00")
                        {
                            resp_code = TTdResultCodeEnum.QUERY_ORDER_NOT_FOUND.GetHashCode().ToString("D4");
                            resp_msg = $"KKday.order_master_mid={ order.supplierOrderId } : 找不到";

                            resp_body = null;
                        }
                        // (1) 订单号异常: 4001
                        else if (master == null)
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
                            var _items = new List<QueryOrderRespModel.ItemNodeModel>();
                            var _orderStatus = TripOrderStatusMapping.Convert(kkday_order.order_status);
                            _items.Add(new QueryOrderRespModel.ItemNodeModel()
                            {
                                itemId = tripOrder.items[0].itemId,
                                orderStatus = _orderStatus,
                                useStartDate = kkday_order.s_date,
                                useEndDate = kkday_order.e_date,
                                quantity = kkday_order.qty_total,
                                useQuantity = kkday_order.order_status.StartsWith("GO") ? kkday_order.qty_total: 0,
                                cancelQuantity = kkday_order.order_status.Equals("CX") ? kkday_order.qty_total : 0,
                            });

                            // 回傳-主體(Body)
                            resp_body = new QueryOrderRespModel()
                            {
                                otaOrderId = tripOrder.ota_order_id,
                                supplierOrderId = master.order_master_mid,
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
                var voucherProxy = HttpContext.RequestServices.GetService<VoucherProxy>();

                // 取出訂單
                var master = orderMasterRepos.GetOrder(order_master_mid: id);
                var tripOrder = tripOrderRepos.GetOrder(master.ota_order_id);

                #region 取得 B2D 憑證清單 --- start

                var _itemId = tripOrder.items.FirstOrDefault()?.itemId;
                var _vouchers = new List<NewOrderConfirmationReqModel.VouchersModel>();
                var voucher_files = new Dictionary<string, string>(); // <file_name, base64_voucher>

                // 抓取 B2D 憑證清單
                var json_result = voucherProxy.GetVoucherLst(master.order_mid, authorToken: Website.Instance.B2dApiAuthorToken); 
                var b2d_voucher_lst = JsonConvert.DeserializeObject<VoucherQueryRespModel>(json_result); 
                if (Convert.ToInt32(b2d_voucher_lst.result) != 0) throw new Exception($"{master.order_mid}: voucher get list error ({b2d_voucher_lst.result}).");
                foreach (var _file in b2d_voucher_lst.file)
                {
                    // 取得該憑證資料
                    json_result = voucherProxy.GetVoucher(master.order_mid, _file.order_file_id, Website.Instance.B2dApiAuthorToken);
                    var b2d_voucher = JsonConvert.DeserializeObject<VoucherDownloadRespModel>(json_result);
                    if (Convert.ToInt32(b2d_voucher.result) != 0) throw new Exception($"{master.order_mid}: voucher is invalid ({b2d_voucher.result}).");

                    foreach (var v_file in b2d_voucher.file)
                    {
                        _vouchers.Add(new NewOrderConfirmationReqModel.VouchersModel()
                        {
                            itemId = _itemId,
                            voucherType = 5, // PDF
                            voucherData = v_file.encode_str
                        }); 
                    }
                }

                #endregion  取得 B2D 憑證清單 --- end

                // 準備請求參數
                var _header = new TTdReqHeaderModel()
                {
                    accountId = Website.Instance.AgentAccount,
                    serviceName = "CreateOrderConfirm",
                    requestTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"), // "2017-01-05 10:00:00",
                    version = "1.0",
                    sign = "",
                };

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
                    supplierOrderId = master.order_master_mid,
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
            catch (Exception ex)
            {
                Website.Instance.logger.Fatal($"OrderConfirm Exception, Message={ex.Message},StackTrace={ex.StackTrace}");
                throw ex;
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
                var master = orderMasterRepos.GetOrder(order_master_mid: id);
                var tripOrder = tripOrderRepos.GetOrder(master.ota_order_id);

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
                    sequenceId = master.ota_sequence_id,
                    otaOrderId = tripOrder.ota_order_id,
                    supplierOrderId = master.order_master_mid,
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
                throw ex;
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
                var master = orderMasterRepos.GetOrder(order_master_mid: id);
                var tripOrder = tripOrderRepos.GetOrder(master.ota_order_id);
                 
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
                    supplierOrderId = master.order_master_mid,
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
                throw ex;
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
                var master = orderMasterRepos.GetOrder(order_master_mid: id);
                var tripOrder = tripOrderRepos.GetOrder(master.ota_order_id); 
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
                    supplierOrderId = master.order_master_mid,
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
                throw ex;
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
