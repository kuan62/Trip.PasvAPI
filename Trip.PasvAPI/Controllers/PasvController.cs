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

            switch (req["header"]["serviceName"]?.ToString())
            {
                // 订单验证 Order verification (*)
                case "VerifyOrder":
                    {

                        try
                        { 
                            var order = JObject.Parse(body).ToObject<OrderVerificationReqModel>();
                            
                            var locale = "zh-TW";
                            var PLU = order.items.Select(i => i.PLU).Distinct().FirstOrDefault();
                            var usedDate = order.items.FirstOrDefault().useStartDate;


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
                            var is_duplicated = false; // tripOrderRepos.IsDuplicated(order.sequenceId);
                            if (is_duplicated)
                            {
                                // 回傳-結果代碼(Result Code)-訂單已重複
                                resp_code = TTdResultCodeEnum.DUPLICATED.GetHashCode().ToString("D4");
                                resp_msg = $"sequenceId='{ order.sequenceId }' is duplicated!";
                                resp_body = null;
                            }
                            else
                            {
                                var PLU = order.items.Select(i => i.PLU).Distinct().FirstOrDefault();
                                var usedDate = order.items.FirstOrDefault().useStartDate;

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

                                /////////

                                // 檢查出行人(旅客)
                                if (order.items.Select(i => i.passengers).FirstOrDefault().Count() < 1) throw new Exception("缺失出行人");

                                var result = orderMasterRepos.CreateDummyOrder(trip_order_oid, PLU);
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
                                }

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
                                supplierConfirmType = 2, // 2.取消待确认（当confirmType =2时需异步返回确认结果的）
                                items = _items,
                            };
                        }

                        break;
                    }
                // 订单取消确认 Order cancellation confirmation (?)
                case "CancelOrderConfirm": break;

                // 订单查询 Order inquiry (*)
                case "QueryOrder": break;

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
            var resp = new TTdResponseModel()
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
