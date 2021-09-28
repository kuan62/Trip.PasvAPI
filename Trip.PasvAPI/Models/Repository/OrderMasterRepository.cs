using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Npgsql;
using Trip.PasvAPI.AppCode;
using Trip.PasvAPI.Models.Model;
using Trip.PasvAPI.Models.Model.Trip;
using Trip.PasvAPI.Proxy;

namespace Trip.PasvAPI.Models.Repository
{
    public class OrderMasterRepository
    {
        private readonly TripOrderRepository _tripOrderRepos;
        private readonly ProductProxy _prodProxy;
        private readonly BookingProxy _bookingProxy;
        private readonly IMemoryCache _cache;

        public OrderMasterRepository(TripOrderRepository tripOrderRepos, ProductProxy prodProxy,
            BookingProxy bookingProxy, IMemoryCache memoryCache)
        {
            this._tripOrderRepos = tripOrderRepos;
            this._prodProxy = prodProxy;
            this._bookingProxy = bookingProxy;
            this._cache = memoryCache;
        }

        public List<OrderMasterModel> GetOrders()
        {
            try
            {
                SqlMapper.AddTypeHandler(typeof(BookingDataModel), new ObjectJsonMapper());
                SqlMapper.AddTypeHandler(typeof(Dictionary<string, object>), new ObjectJsonMapper());

                using (var conn = new NpgsqlConnection(Website.Instance.SqlConnectionString))
                {
                    var sqlStmt = @$"SELECT * FROM order_master";

                    return conn.Query<OrderMasterModel>(sqlStmt).ToList();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public OrderMasterModel GetOrder(string kkday_order_mid)
        {
            try
            {
                SqlMapper.AddTypeHandler(typeof(BookingDataModel), new ObjectJsonMapper());
                SqlMapper.AddTypeHandler(typeof(Dictionary<string, object>), new ObjectJsonMapper());

                using (var conn = new NpgsqlConnection(Website.Instance.SqlConnectionString))
                {
                    var sqlStmt = @$"SELECT * FROM order_master WHERE kkday_order_mid=:kkday_order_mid";

                    return conn.QuerySingleOrDefault<OrderMasterModel>(sqlStmt);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        //////////////

        #region Dummy Operations --- start

        public string GetDummyOrderMasterMid()
        {
            using (var conn = new NpgsqlConnection(Website.Instance.SqlConnectionString))
            {
                var sqlStmt = @"select to_char(now(), 'YY') || 'MM' || LPAD(nextval('dummy_order_master_mid_seq')::text, 9, '0') as order_master_mid";
                return conn.QuerySingle<string>(sqlStmt);
            }
        }

        public (Int64 order_oid, string order_mid) GetDummyOrderData()
        {
            using (var conn = new NpgsqlConnection(Website.Instance.SqlConnectionString))
            {
                var sqlStmt = @"select nextval('dummy_order_mid_seq') as order_oid";
                var order_oid = conn.QuerySingle<Int64>(sqlStmt);
                var order_mid = $"{DateTime.Today.ToString("yy")}KK{order_oid.ToString("000000000")}";

                return (order_oid, order_mid);
            }
        }

        public (OrderMasterModel model, int qty) CreateDummyOrder(Int64 trip_order_oid, string plu)
        {
            var order_master_mid = GetDummyOrderMasterMid();
            var order_data = GetDummyOrderData();
            var trip_pax_lst = new List<int>();
            
            var model = new OrderMasterModel()
            {
                kkday_order_master_mid = order_master_mid,
                kkday_order_mid = order_data.order_mid,
                kkday_order_oid = order_data.order_oid,
                trip_order_oid = trip_order_oid,
                trip_item_seq = 1,
                trip_item_pax = trip_pax_lst,
                trip_item_plu = plu,
                create_user = "SYSTEM",
                create_time = DateTime.Now
            };

            var qty = 10;

            return (model, qty);
        }

        #endregion Dummy Operations --- start

        //////////////

        public (OrderMasterModel model, int qty) CreateOrder(Int64 trip_order_oid, string plu, string locale, ProductRespModel prod, PackageRespModel pkg)
        {
            OrderMasterModel model = null;
            var qty = 0;
            

            ////////////////

            #region 查詢 KKday 商品

            var result = _prodProxy.GetProduct(new ProductReqModel()
            {
                locale = LocaleMapping.ToKKdayLocale(locale),
                state = MarketingMapping.ToKKdayState(locale), // 設為台灣地區為主
                prod_no = prod.prod.prod_no.ToString()
            }, Website.Instance.B2dApiAuthorToken);

            Console.WriteLine($"Product Result => {result}");

            var settings = new JsonSerializerSettings
            {
                MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
                DateParseHandling = DateParseHandling.None,
                Converters = { new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal } }
            };

            var prodResp = JsonConvert.DeserializeObject<ProductRespModel>(result, settings);
            if (!prodResp.result.Equals("00"))
            {
                throw new NullReferenceException($"prod_oid={ prod.prod.prod_no } is invalid!");
            }

            #endregion 查詢 KKday 商品

            ////////////////

            #region 查詢 KKday 商品&套餐+旅規
 
            result = _prodProxy.GetPackage(new QueryPackageModel() {
                        prod_no = prod.prod.prod_no.ToString(),
                        pkg_no =  pkg.pkg_no.ToString(),
                        currency = Website.Instance.AgentCurrency,
                        locale = LocaleMapping.ToKKdayLocale(locale),
                        state = MarketingMapping.ToKKdayState(locale),  // 設為台灣地區為主
                    }, Website.Instance.B2dApiAuthorToken);

            Console.WriteLine($"Package Result => {result}"); 
            
            var resp = JsonConvert.DeserializeObject<PackageRespModel>(result, settings);

            //if (resp == null || resp.item == null || resp.item.Count < 1)
            //{
            //    throw new NullReferenceException($"Product={req.prod_no}, Package={req.pkg_no} with null object.");
            //}

            #endregion 查詢 KKday 商品&套餐+旅規

            // 建立 KKday 訂單，取得 order_master_mid & order_mid

            //////// 建立訂單 ////////

            var bookingData = new BookingDataModel()
            {
                payment_type = "arType"
            };

            //result = _bookingProxy.Booking(bookingData, Website.Instance.B2dApiAuthorToken);
            //Console.WriteLine(" booking reuslt => " + result);

            //var bookingResp = JObject.Parse(result);
            //if (bookingResp["result"].ToString() != "00")
            //{
            ////4.1失敗->回壓訂單狀態
            //orderMaster.order_master_status = "NG";//失敗了
            //orderMaster.modify_user = "SYSTEM";
            //orderMaster.modify_date = DateTime.Now;
            //bookingRepos.UpdateOrderMasterFail(orderMaster);
            //Website.Instance.logger.Info($"Booking_Step2_成立訂單失敗!" + bookingResp["result_msg"]?.ToString());
            //bookingRepos.slackPost(slackNotice.note, null, null, $"Booking_Step2_成立訂單失敗!需求單號:{orderMaster?.reservation_oid},商編:{bookingData?.prod_no},訊息:{bookingResp["result_msg"]?.ToString()}");
            //status.status = "FAIL";
            //status.msgErr = bookingResp["result_msg"]?.ToString();
            //return Json(status);
            //}
            return (model, qty);
        }

        //////////////
        
        public void Insert(OrderMasterModel req)
        {
            try
            {
                SqlMapper.AddTypeHandler(typeof(BookingDataModel), new ObjectJsonMapper());
                SqlMapper.AddTypeHandler(typeof(Dictionary<string, object>), new ObjectJsonMapper());

                using (var conn = new NpgsqlConnection(Website.Instance.SqlConnectionString))
                {
                    var sqlStmt = @$"INSERT INTO order_master(kkday_order_master_mid, kkday_order_mid, kkday_order_oid,
  trip_order_oid, trip_item_seq, trip_item_pax, trip_item_plu, status, booking_info, param1, create_user)
VALUES(:kkday_order_master_mid, :kkday_order_mid, :kkday_order_oid, :trip_order_oid, :trip_item_seq, :trip_item_pax, :trip_item_plu,
  :status, :booking_info::jsonb, :param1::jsonb, :create_user) ";

                    conn.Execute(sqlStmt, req);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
