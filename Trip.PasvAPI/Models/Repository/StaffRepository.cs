using System;
using Newtonsoft.Json.Linq;
using Npgsql;
using Dapper;
using Trip.PasvAPI.AppCode;
using System.Collections.Generic;
using Trip.PasvAPI.Models.Model;
using System.Linq;

namespace Trip.PasvAPI.Models.Repository
{
    public class StaffRepository
    {
        private string query_sql = @"
WITH code AS (
    SELECT code_type, code_no, COALESCE(code_name->>:locale, code_name->>'basic') AS code_name
    FROM code
)
SELECT a.*, c1.code_name AS status_name, c2.code_name AS type_name
FROM staff a
LEFT JOIN code AS c1 ON a.staff_status=c1.code_no AND c1.code_type='STAFF_STATUS' 
LEFT JOIN code AS c2 ON a.staff_type=c2.code_no AND c2.code_type='STAFF_TYPE'
";

        //////////////////////////////////////////////

        public int GetCount(string filter)
        {
            try
            {
                var _filter = FilterParsing(filter);

                using (var conn = new NpgsqlConnection(Website.Instance.SqlConnectionString))
                {
                    string sqlStmt = @"SELECT COUNT(*) FROM staff a WHERE 1=1 ";

                    if (!string.IsNullOrEmpty(_filter)) sqlStmt += $" {_filter}";

                    return conn.QuerySingle<int>(sqlStmt);
                }
            }
            catch (Exception ex)
            {
                // Website.Instance.logger.Fatal($"GetCount:Exception:Message={ex.Message},StackTrace={ex.StackTrace}");
                throw ex;
            }
        }

        public List<StaffModel> GetStaffs(string filter, string sorting, int size, int skip, string locale)
        {
            try
            {
                var _filter = FilterParsing(filter);
                // SqlMapper.AddTypeHandler(typeof(Dictionary<string, string>), new ObjectJsonMapper());

                using (var conn = new NpgsqlConnection(Website.Instance.SqlConnectionString))
                {
                    var sqlStmt = query_sql + @"
WHERE 1=1 ";

                    // 過濾 Filter
                    if (!string.IsNullOrEmpty(_filter)) sqlStmt += $" {_filter} \n";
                    // 排序 Sorting
                    if (!string.IsNullOrEmpty(sorting)) sqlStmt += $" ORDER BY {sorting} \n";

                    sqlStmt += $" LIMIT :size OFFSET :skip";

                    var staffs = conn.Query<StaffModel>(sqlStmt, new { locale, skip, size }).ToList();
                    return staffs;
                }
            }
            catch (Exception ex)
            {
                Website.Instance.logger.Fatal($"GetCodes:Exception:Message={ex.Message},StackTrace={ex.StackTrace}");
                throw ex;
            }
        }

        private string FilterParsing(string strJson)
        {
            var _filter = "";
            if (string.IsNullOrEmpty(strJson)) return _filter;

            var cond = JObject.Parse(strJson);

            // 查詢員工帳號(email)
            if (cond["account"] != null && !string.IsNullOrEmpty(cond["account"]?.ToString()))
            {
                _filter = $" AND a.staff_account::text LIKE '%{cond["account"]}%' ";
            }
            // 查詢員工姓名
            if (cond["name"] != null && !string.IsNullOrEmpty(cond["name"]?.ToString()))
            {
                _filter += $" AND a.staff_name::text LIKE '%{cond["name"]}%' ";
            }
            // 依建立區間查詢
            if (!string.IsNullOrEmpty(cond["sdate"]?.ToString()) && !string.IsNullOrEmpty(cond["edate"]?.ToString()))
            {
                _filter += $" AND (a.create_date BETWEEN '{cond["sdate"]}' AND '{cond["edate"]}')\n";
            }
            else if (!string.IsNullOrEmpty(cond["sdate"]?.ToString()) && string.IsNullOrEmpty(cond["edate"]?.ToString()))
            {
                _filter += $" AND (a.create_date >= '{cond["sdate"]}')\n";
            }
            else if (string.IsNullOrEmpty(cond["sdate"]?.ToString()) && !string.IsNullOrEmpty(cond["edate"]?.ToString()))
            {
                _filter += $" AND (a.create_date <= '{cond["edate"]}')\n";
            }

            return _filter;
        }

        public StaffModel GetStaff(string account = null, Int64? oid = null, string passwd = null, string locale = null)
        {
            try
            {
                if (string.IsNullOrEmpty(locale) && string.IsNullOrEmpty(account) && oid == null && string.IsNullOrEmpty(passwd))
                {
                    throw new NullReferenceException("Arguments are null");
                }

                using (var conn = new NpgsqlConnection(Website.Instance.SqlConnectionString))
                {
                    var sqlStmt = query_sql + @"
WHERE 1=1 ";

                    var dynaParams = new DynamicParameters();
                    dynaParams.Add("locale", locale);

                    if (!string.IsNullOrEmpty(account))
                    {
                        sqlStmt += " AND a.staff_account=:account";
                        dynaParams.Add("account", account);
                    }

                    if (oid != null)
                    {
                        sqlStmt += " AND a.staff_oid=:oid";
                        dynaParams.Add("oid", oid.Value);
                    }

                    if (!string.IsNullOrEmpty(passwd))
                    {
                        sqlStmt += " AND a.staff_password=:passwd";
                        dynaParams.Add("passwd", passwd);
                    }

                    return conn.QueryFirstOrDefault<StaffModel>(sql: sqlStmt, param: dynaParams);
                }
            }
            catch (Exception ex)
            {
                Website.Instance.logger.Fatal($"Staff.Get:Exception:Message={ex.Message},StackTrace={ex.StackTrace}");
                throw ex;
            }
        }

        public void Insert(StaffModel req)
        {
            try
            {
                using (var conn = new NpgsqlConnection(Website.Instance.SqlConnectionString))
                {
                    var sqlStmt = @"INSERT INTO staff (staff_type, staff_status,
staff_account, staff_name, staff_password, create_user)
VALUES(:staff_type, :staff_status, :staff_account, :staff_name, :staff_password, :create_user)";

                    conn.Execute(sqlStmt, req);
                }
            }
            catch (Exception ex)
            {
                Website.Instance.logger.Fatal($"Staff.Insert:Exception:Message={ex.Message},StackTrace={ex.StackTrace}");
                throw ex;
            }
        }

        public void Update(StaffModel req)
        {
            try
            {
                using (var conn = new NpgsqlConnection(Website.Instance.SqlConnectionString))
                {
                    var sqlStmt = @"UPDATE staff SET staff_type=:staff_type, staff_status=:staff_status,
staff_account=:staff_account, staff_name=:staff_name, modify_user=:modify_user, modify_date=now()
WHERE staff_oid=:staff_oid ";

                    conn.Execute(sqlStmt, req);
                }
            }
            catch (Exception ex)
            {
                Website.Instance.logger.Fatal($"Staff.Update:Exception:Message={ex.Message},StackTrace={ex.StackTrace}");
                throw ex;
            }
        }

        public void SetPassword(Int64 oid, string psw, string modify_user)
        {
            try
            {
                using (var conn = new NpgsqlConnection(Website.Instance.SqlConnectionString))
                {

                    var sqlStmt = @"UPDATE staff SET staff_password=:psw,
modify_user=:modify_user, modify_date=now()
WHERE staff_oid=:oid ";

                    conn.Execute(sqlStmt, new { oid, psw, modify_user });
                }
            }
            catch (Exception ex)
            {
                Website.Instance.logger.Fatal($"Staff.SetPassword:Exception:Message={ex.Message},StackTrace={ex.StackTrace}");
                throw ex;
            }
        }
    }
}
