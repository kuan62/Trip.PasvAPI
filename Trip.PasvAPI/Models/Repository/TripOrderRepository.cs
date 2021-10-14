using System;
using System.Collections.Generic;
using Trip.PasvAPI.Models.Model;
using Npgsql;
using Trip.PasvAPI.AppCode;
using Dapper;
using System.Linq;
using Trip.PasvAPI.Models.Model.Trip;

namespace Trip.PasvAPI.Models.Repository
{
    public class TripOrderRepository
    {
        public List<TripOrderModel> GetOrders()
        {
            try
            {
                SqlMapper.AddTypeHandler(typeof(List<CreateOrderReqModel.OrderContactModel>), new ObjectJsonMapper());
                SqlMapper.AddTypeHandler(typeof(List<CreateOrderReqModel.OrderCouponModel>), new ObjectJsonMapper());
                SqlMapper.AddTypeHandler(typeof(List<CreateOrderReqModel.OrderItemModel>), new ObjectJsonMapper());

                using (var conn = new NpgsqlConnection(Website.Instance.SqlConnectionString))
                {
                    var sqlStmt = @$"SELECT * FROM trip_order";

                    return conn.Query<TripOrderModel>(sqlStmt).ToList();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public TripOrderModel GetOrder(string ota_order_id)
        {
            try
            {
                SqlMapper.AddTypeHandler(typeof(List<CreateOrderReqModel.OrderContactModel>), new ObjectJsonMapper());
                SqlMapper.AddTypeHandler(typeof(List<CreateOrderReqModel.OrderCouponModel>), new ObjectJsonMapper());
                SqlMapper.AddTypeHandler(typeof(List<CreateOrderReqModel.OrderItemModel>), new ObjectJsonMapper());

                using (var conn = new NpgsqlConnection(Website.Instance.SqlConnectionString))
                {
                    var sqlStmt = @$"SELECT * FROM trip_order WHERE ota_order_id=:ota_order_id";

                    return conn.QuerySingleOrDefault<TripOrderModel>(sqlStmt, new { ota_order_id });
                }
            }
            catch (Exception ex)
            {
                Website.Instance.logger.Fatal($"Exception: Message={ex.Message}, StackTrace={ex.StackTrace}");
                throw ex;
            }
        }

        //////////////

        public bool IsDuplicated(string ota_order_id)
        {
            try
            { 
                using (var conn = new NpgsqlConnection(Website.Instance.SqlConnectionString))
                {
                    var sqlStmt = @$"SELECT CASE WHEN Count(*) > 0 THEN true ELSE false END is_duplicated FROM trip_order WHERE ota_order_id=:ota_order_id";

                    var is_duplicated = conn.QuerySingle<bool>(sqlStmt, new { ota_order_id });
                    return is_duplicated; 
                }
            }
            catch (Exception ex)
            {
                Website.Instance.logger.Fatal($"Exception: Message={ex.Message}, StackTrace={ex.StackTrace}");
                throw ex;
            }
        }

        public Int64 Insert(TripOrderModel req)
        {
            try
            {
                SqlMapper.AddTypeHandler(typeof(List<CreateOrderReqModel.OrderContactModel>), new ObjectJsonMapper());
                SqlMapper.AddTypeHandler(typeof(List<CreateOrderReqModel.OrderCouponModel>), new ObjectJsonMapper());
                SqlMapper.AddTypeHandler(typeof(List<CreateOrderReqModel.OrderItemModel>), new ObjectJsonMapper());

                using (var conn = new NpgsqlConnection(Website.Instance.SqlConnectionString))
                {
                    var sqlStmt = @$"INSERT INTO trip_order (sequence_id, ota_order_id, confirm_type, contacts, coupons, items, status, note, create_user)
VALUES(:sequence_id, :ota_order_id, :confirm_type, :contacts::jsonb, :coupons::jsonb, :items::jsonb, :status, :note, :create_user)
RETURNING trip_order_oid ";

                    return conn.ExecuteScalar<Int64>(sqlStmt, req);
                }
            }
            catch(Exception ex)
            {
                Website.Instance.logger.Fatal($"Exception: Message={ex.Message}, StackTrace={ex.StackTrace}");
                throw ex;
            }
        } 
         
    }
}
