using System;
using Npgsql;
using Dapper;
using Trip.PasvAPI.AppCode;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace Trip.PasvAPI.Models.Repository
{
    public class CodeRepository
    {
        #region 查詢作業 --- start

        public int GetCount(string filter)
        {
            try
            {
                var _filter = FilterParsing(filter);

                using (var conn = new NpgsqlConnection(Website.Instance.SqlConnectionString))
                {
                    string sqlStmt = @"SELECT COUNT(*) FROM code a WHERE 1=1 ";

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

        public List<CodeInfoModel> GetCodes(string filter, string sorting, int size, int skip,
            string locale)
        {
            try
            {
                var _filter = FilterParsing(filter);

                using (var conn = new NpgsqlConnection(Website.Instance.SqlConnectionString))
                {
                    var sqlStmt = @"SELECT a.code_oid,  a.code_type, a.code_no,
  COALESCE(a.code_name->>:locale, a.code_name->>'basic') as code_name,
  a.sort, a.create_user, a.create_date, a.modify_user, a.modify_date
FROM code a
WHERE 1=1 ";

                    // 過濾 Filter
                    if (!string.IsNullOrEmpty(_filter)) sqlStmt += $" {_filter} \n";
                    // 排序 Sorting
                    if (!string.IsNullOrEmpty(sorting)) sqlStmt += $" ORDER BY {sorting} \n";

                    sqlStmt += $" LIMIT :size OFFSET :skip";

                    return conn.Query<CodeInfoModel>(sqlStmt, new { locale, skip, size }).ToList();
                }
            }
            catch (Exception ex)
            {
                // Website.Instance.logger.Fatal($"GetCodes:Exception:Message={ex.Message},StackTrace={ex.StackTrace}");
                throw ex;
            }
        }

        private string FilterParsing(string strJson)
        {
            var _filter = "";
            if (string.IsNullOrEmpty(strJson)) return _filter;

            var cond = JObject.Parse(strJson);
            if (!string.IsNullOrEmpty(cond["code_type"]?.ToString()))
            {
                _filter = $" AND a.code_type='{cond["code_type"]}'\n";
            }

            return _filter;
        }

        public CodeModel GetCode(Int64 code_oid)
        {
            try
            {
                SqlMapper.AddTypeHandler(typeof(Dictionary<string, string>), new ObjectJsonMapper());

                using (var conn = new NpgsqlConnection(Website.Instance.SqlConnectionString))
                {
                    var sqlStmt = @"SELECT a.code_oid, a.code_type, a.code_no, a.code_name,
  a.sort, a.create_user, a.create_date, a.modify_user, a.modify_date
FROM code a
WHERE code_oid=:code_oid";

                    return conn.QuerySingleOrDefault<CodeModel>(sqlStmt, new { code_oid });
                }
            }
            catch (Exception ex)
            {
                // Website.Instance.logger.Fatal($"GetCodes:Exception:Message={ex.Message},StackTrace={ex.StackTrace}");
                throw ex;
            }
        }

        public List<CodeModel> LoadFromTypes(params string[] types)
        {
            try
            {
                SqlMapper.AddTypeHandler(typeof(Dictionary<string, string>), new ObjectJsonMapper());

                using (var conn = new NpgsqlConnection(Website.Instance.SqlConnectionString))
                {
                    var sqlStmt = @"SELECT a.* FROM code a WHERE code_type=ANY(ARRAY[:types])";

                    return conn.Query<CodeModel>(sqlStmt, new { types }).ToList();
                }
            }
            catch (Exception ex)
            {
                // Website.Instance.logger.Fatal($"GetCodes:Exception:Message={ex.Message},StackTrace={ex.StackTrace}");
                throw ex;
            }
        }

        public List<string> GetCodeTypesForAC(string query)
        {
            try
            {
                using (var conn = new NpgsqlConnection(Website.Instance.SqlConnectionString))
                {
                    var sqlStmt = @"SELECT DISTINCT code_type FROM code WHERE LOWER(code_type) LIKE :query ";
                    return conn.Query<string>(sqlStmt, new { query = $"%{query?.ToLower()}%" }).ToList();
                }
            }
            catch (Exception ex)
            {
                // Website.Instance.logger.Fatal($"GetCodes:Exception:Message={ex.Message},StackTrace={ex.StackTrace}");
                throw ex;
            }
        }

        public List<CodeEssentialModel> GetCodesByType(string code_type, string locale)
        {
            try
            {
                using (var conn = new NpgsqlConnection(Website.Instance.SqlConnectionString))
                {
                    var sqlStmt = @"SELECT DISTINCT code_type, code_no, sort,
    COALESCE(code_name->>:locale, code_name->>'basic') as code_name
FROM code
WHERE code_type=:code_type
ORDER BY sort";

                    return conn.Query<CodeEssentialModel>(sqlStmt, new { code_type, locale }).ToList();
                }
            }
            catch (Exception ex)
            {
                // Website.Instance.logger.Fatal($"GetCodes:Exception:Message={ex.Message},StackTrace={ex.StackTrace}");
                throw ex;
            }
        }

        public List<string> GetCodeTypes()
        {
            try
            {
                using (var conn = new NpgsqlConnection(Website.Instance.SqlConnectionString))
                {
                    var sqlStmt = @"SELECT DISTINCT code_type FROM code ORDER BY code_type ";
                    return conn.Query<string>(sqlStmt).ToList();
                }
            }
            catch (Exception ex)
            {
                // Website.Instance.logger.Fatal($"GetCodes:Exception:Message={ex.Message},StackTrace={ex.StackTrace}");
                throw ex;
            }
        }

        #endregion 查詢作業 --- end

        ////////////

        public void InsertItem(CodeModel req)
        {
            try
            {
                SqlMapper.AddTypeHandler(typeof(Dictionary<string, string>), new ObjectJsonMapper());

                using (var conn = new NpgsqlConnection(Website.Instance.SqlConnectionString))
                {
                    var sqlStmt = @"INSERT INTO code (code_type, code_no, code_name, sort, create_user)
VALUES(:code_type, :code_no, :code_name::jsonb, :sort, :create_user)";

                    conn.Execute(sqlStmt, req);
                }
            }
            catch (Exception ex)
            {
                // Website.Instance.logger.Fatal($"GetCodes:Exception:Message={ex.Message},StackTrace={ex.StackTrace}");
                throw ex;
            }
        }

        public void UpdateItem(CodeModel req)
        {
            try
            {
                SqlMapper.AddTypeHandler(typeof(Dictionary<string, string>), new ObjectJsonMapper());

                using (var conn = new NpgsqlConnection(Website.Instance.SqlConnectionString))
                {
                    var sqlStmt = @"UPDATE code SET code_type=:code_type, code_no=:code_no,
  code_name=:code_name::jsonb, sort=:sort, modify_user=:modify_user, modify_date=now()
WHERE code_oid=:code_oid ";

                    conn.Execute(sqlStmt, req);
                }
            }
            catch (Exception ex)
            {
                // Website.Instance.logger.Fatal($"GetCodes:Exception:Message={ex.Message},StackTrace={ex.StackTrace}");
                throw ex;
            }

        }
    }
}
