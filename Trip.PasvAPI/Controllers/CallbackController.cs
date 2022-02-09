using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Trip.PasvAPI.AppCode;
using Trip.PasvAPI.Models.Model;
using Trip.PasvAPI.Models.Model.Trip;
using Trip.PasvAPI.Models.Repository;
using Trip.PasvAPI.Proxy;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Trip.PasvAPI.Controllers
{
    [Route("api/[controller]")]
    public class CallbackController : Controller
    {
        private readonly IMemoryCache _cache;
        private readonly string secretKey;
        private readonly string aesIV;

        public CallbackController(IMemoryCache memoryCache)
        {
            this._cache = memoryCache;
            this.secretKey = Website.Instance.Configuration["Proxy:Trip:AesKey"];
            this.aesIV = Website.Instance.Configuration["Proxy:Trip:AesIV"];
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody] WebHookModel req)
        {
            try
            {
                // 訂單憑證已產出，需通知 Trip.com
                if (req.result_type.Equals("order") && req.order.status.Equals("GO_OK"))
                {
                    var result = "";

                    var logRepos = HttpContext.RequestServices.GetService<TripTransLogRepository>();
                    var orderMasterRepos = HttpContext.RequestServices.GetService<OrderMasterRepository>();
                    var tripOrderRepos = HttpContext.RequestServices.GetService<TripOrderRepository>();
                    var ttdOpenProxy = HttpContext.RequestServices.GetService<Proxy.TtdOpenProxy>();
                    var voucherProxy = HttpContext.RequestServices.GetService<VoucherProxy>();

                    // 取出 KKday 訂單
                    var master = orderMasterRepos.GetOrder(order_mid: req.order.order_no);
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

                    #endregion 取得 B2D 憑證清單 --- end

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

                    var cmd = new Dictionary<string, object>();
                    cmd.Add("header", _header);
                    cmd.Add("body", _encryBody);

                    var jsonData = JsonConvert.SerializeObject(cmd);
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
            }
            catch (Exception ex)
            {
                Website.Instance.logger.Fatal($"OrderConfirm Exception, Message={ex.Message},StackTrace={ex.StackTrace}");
            } 
        }

        #region Useless --- start

        /*
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
        public void Post([FromBody] string value)
        {
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
        }*/

        #endregion Useless --- end
    }
}
