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

        ////////////////////////


        public int GetCount(string filter)
        {
            try
            { 
                using (var conn = new NpgsqlConnection(Website.Instance.SqlConnectionString))
                {
                    var sqlStmt = @$"select count(1) from order_master a where 1=1 ";

                    return conn.QuerySingle<int>(sqlStmt);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<OrderMasterExModel> GetOrders(string filter, string sorting, int size, int skip)
        {
            try
            {
                SqlMapper.AddTypeHandler(typeof(BookingDataModel), new ObjectJsonMapper());
                SqlMapper.AddTypeHandler(typeof(Dictionary<string, object>), new ObjectJsonMapper());

                using (var conn = new NpgsqlConnection(Website.Instance.SqlConnectionString))
                {
                    var sqlStmt = @$"select distinct a.*, b.prod_name, b.ota_prod_name
from order_master a 
left join product_map b on b.prod_oid = a.booking_info->>'prod_no' and b.pkg_oid=a.booking_info->>'pkg_no'
where 1=1 ";

                    return conn.Query<OrderMasterExModel>(sqlStmt).ToList();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public OrderMasterModel GetOrder(string order_master_mid = null, string order_mid = null)
        {
            try
            {
                if (string.IsNullOrEmpty(order_master_mid) && string.IsNullOrEmpty(order_mid)) return null;

                SqlMapper.AddTypeHandler(typeof(BookingDataModel), new ObjectJsonMapper());
                SqlMapper.AddTypeHandler(typeof(Dictionary<string, object>), new ObjectJsonMapper());

                using (var conn = new NpgsqlConnection(Website.Instance.SqlConnectionString))
                {
                    var sqlStmt = @$"select * from order_master where 1=1 ";
                    var sqlParams = new DynamicParameters();

                    if (!string.IsNullOrEmpty(order_master_mid))
                    {
                        sqlStmt += " and order_master_mid=:order_master_mid";
                        sqlParams.Add("order_master_mid", order_master_mid);
                    }

                    if (!string.IsNullOrEmpty(order_mid))
                    {
                        sqlStmt += " AND order_mid=:order_mid";
                        sqlParams.Add("order_mid", order_mid);
                    }

                    return conn.QuerySingleOrDefault<OrderMasterModel>(sqlStmt, sqlParams);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string GetOrderMasterMid(string ota_order_id)
        {
            try
            { 
                using (var conn = new NpgsqlConnection(Website.Instance.SqlConnectionString))
                {
                    var sqlStmt = @$"select a.order_master_mid
from order_master a
join trip_order b on a.ota_order_id=b.ota_order_id
where b.ota_order_id=:ota_order_id";

                    return conn.QuerySingleOrDefault<string>(sqlStmt, new { ota_order_id });
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

        public OrderMasterModel CreateDummyOrder(TripOrderModel req)
        { 
            var item = req.items.FirstOrDefault();

            // 加入旅客索引
            var trip_pax_lst = new List<int>();
            item.passengers.ForEach(p => {
                var idx = item.passengers.IndexOf(p);
                trip_pax_lst.Add(idx);
            });

            // 建立訂單(包括 order_master_mid)
            var order_master_mid = GetDummyOrderMasterMid();
            var order_data = GetDummyOrderData();

            // 新增記錄到 order_master 資料表
            var model = new OrderMasterModel()
            {
                order_master_mid = order_master_mid,
                order_mid = order_data.order_mid,
                order_oid = order_data.order_oid,
                booking_info = new BookingDataModel(),
                status = "GO",
                ota_order_id = req.trip_order_oid.ToString(),
                ota_sequence_id = req.sequence_id,
                ota_item_seq = 0,
                ota_item_pax = trip_pax_lst.ToArray(),
                ota_item_plu = item.PLU,
                create_user = "SYSTEM",
                create_time = DateTime.Now
            };
             
            Insert(model);

            return model;
        }

        #endregion Dummy Operations --- start

        //////////////

        public void ChangeStatus(string order_master_mid, string status)
        {
            try
            {
                using (var conn = new NpgsqlConnection(Website.Instance.SqlConnectionString))
                {
                    var sqlStmt = @$"UPDATE order_master SET status=:status WHERE order_master_mid=:order_master_mid ";
                    conn.Execute(sqlStmt, new { order_master_mid, status }); 
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool CancelApply(string order_master_mid, string trip_sequence_id)
        {
            try
            {
                using (var conn = new NpgsqlConnection(Website.Instance.SqlConnectionString))
                {
                    var sqlStmt = @$"UPDATE order_master SET status='CX_ING', trip_sequence_id=:trip_sequence_id
WHERE order_master_mid=:order_master_mid ";

                    var result = conn.Execute(sqlStmt, new { order_master_mid, trip_sequence_id });
                    return result > 0 ? true : false;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<OrderMasterExModel> QueryOrder(string order_master_mid)
        {
            try
            {
                SqlMapper.AddTypeHandler(typeof(BookingDataModel), new ObjectJsonMapper());
                SqlMapper.AddTypeHandler(typeof(Dictionary<string, object>), new ObjectJsonMapper());

                using (var conn = new NpgsqlConnection(Website.Instance.SqlConnectionString))
                {
                    var sqlStmt = @"
SELECT a.*, b.ota_order_id, b.items->a.trip_item_seq as item,
  b.items->a.trip_item_seq->>'itemId' as trip_item_id,
  b.items->a.trip_item_seq->>'useStartDate' as use_start_date,
  b.items->a.trip_item_seq->>'useEndDate' as use_end_date,
  b.items->a.trip_item_seq->>'quantity' as use_quantity
FROM order_master a
LEFT JOIN trip_order b ON a.trip_order_oid = b.trip_order_oid
WHERE a.order_master_mid = :order_master_mid ";

                    return conn.Query<OrderMasterExModel>(sqlStmt, new { order_master_mid = order_master_mid }).ToList();

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        } 

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
                    var sqlStmt = @$"INSERT INTO order_master(order_master_mid, order_mid, order_oid, ota_order_id, ota_sequence_id,
 ota_item_seq, ota_item_pax, ota_item_plu, ota_tag, status, booking_info, param1,  currency, amount, create_user)
VALUES(:order_master_mid, :order_mid, :order_oid, :ota_order_id, :ota_sequence_id, :ota_item_seq, :ota_item_pax, :ota_item_plu,
 :ota_tag, :status, :booking_info::jsonb, :param1::jsonb, :currency, :amount, :create_user) ";

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
