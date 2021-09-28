using System;
using Dapper;
using Npgsql;
using Trip.PasvAPI.AppCode;

namespace Trip.PasvAPI.Models.Repository
{
    public class TripTransLogRepository
    {
        public Int64 Insert(string request, string request_user)
        {
            try
            {
                using(var conn = new NpgsqlConnection(Website.Instance.SqlConnectionString))
                {
                    var sqlStmt = @"INSERT INTO trip_trans_log (request, request_user) VALUES(:request::jsonb, :request_user) RETURNING log_oid";

                    return conn.ExecuteScalar<Int64>(sqlStmt, new { request, request_user });
                }
            }
            catch(Exception ex)
            {
                throw ex;
            } 
        }

        public void SetResponse(Int64 log_oid, string response, string response_user)
        {
            try
            { 
                using (var conn = new NpgsqlConnection(Website.Instance.SqlConnectionString))
                {
                    var sqlStmt = @"UPDATE trip_trans_log SET response=:response::jsonb, response_user=:response_user, response_time=now() WHERE log_oid=:log_oid ";
                     
                    conn.Execute(sqlStmt, new { response, response_user, log_oid });
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        } 
    }
}
