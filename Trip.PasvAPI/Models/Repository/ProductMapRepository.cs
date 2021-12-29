using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Dapper;
using Newtonsoft.Json.Linq;
using Npgsql;
using Trip.PasvAPI.AppCode;
using Trip.PasvAPI.Models.Model;

namespace Trip.PasvAPI.Models.Repository
{
    public class ProductMapRepository
    {
        #region 查詢作業 --- start

        private string query_sql = $@"
with code as (
    select code_type, code_no, coalesce(code_name->>:locale, code_name->>'basic') as code_name
    from code
)
select a.*, c1.code_name as map_status_name, c2.code_name as map_mode_name
from product_map a
left join code as c1 on a.map_status=c1.code_no and c1.code_type='MAP_STATUS'
left join code as c2 on a.map_mode=c2.code_no and c2.code_type='MAP_MODE' 
";

        public int GetCount(string filter = null, string locale = null)
        {
            try
            {
                using (var conn = new NpgsqlConnection(Website.Instance.SqlConnectionString))
                {
                    var sqlParams = new ExpandoObject() as IDictionary<string, object>;
                    var sqlStmt = $@"select count(*) from ({query_sql}) a
where 1=1 ";

                    var _filter = FilterParsing(filter);
                    if (!string.IsNullOrEmpty(_filter.cond))
                    {
                        sqlStmt += $" {_filter.cond} \n";
                        sqlParams.Merge(_filter.args as IDictionary<string, object>);
                    }

                    sqlParams.Add("locale", locale); // 必要的

                    return conn.QuerySingle<int>(sqlStmt, sqlParams);
                }
            }
            catch (Exception ex)
            {
                // Website.Instance.logger.Fatal($"GetCount:Exception:Message={ex.Message},StackTrace={ex.StackTrace}");
                throw ex;
            }
        }

        public List<ProductMapExModel> GetMaps(string filter, string sorting, int size, int skip, string locale = null)
        {
            try
            {
                SqlMapper.AddTypeHandler(typeof(List<string>), new ObjectJsonMapper());

                using (var conn = new NpgsqlConnection(Website.Instance.SqlConnectionString))
                {
                    var sqlParams = new ExpandoObject() as IDictionary<string, object>;
                    var sqlStmt = $@"select * from ({query_sql}) a
where 1=1 ";

                    var _filter = FilterParsing(filter);
                    // 過濾 Filter
                    if (!string.IsNullOrEmpty(_filter.cond))
                    {
                        sqlStmt += $" {_filter.cond} \n";
                        sqlParams.Merge(_filter.args as IDictionary<string, object>);
                    }
                    // 排序 Sorting
                    if (!string.IsNullOrEmpty(sorting)) sqlStmt += $" order by {sorting} \n";

                    sqlStmt += $" limit :size offset :skip";

                    sqlParams.Add("locale", locale); // 必要的!
                    sqlParams.Add("skip", skip);
                    sqlParams.Add("size", size);

                    return conn.Query<ProductMapExModel>(sqlStmt, sqlParams).ToList();
                }
            }
            catch (Exception ex)
            {
                // Website.Instance.logger.Fatal($"GetCodes:Exception:Message={ex.Message},StackTrace={ex.StackTrace}");
                throw ex;
            }
        }

        private (string cond, dynamic args) FilterParsing(string strJson)
        {
            var _filter = "";
            var _params = new ExpandoObject() as IDictionary<string, object>;

            if (string.IsNullOrEmpty(strJson)) return (cond: _filter, args: _params);

            var cond = JObject.Parse(strJson); 

            if (!string.IsNullOrEmpty(cond["ota_prod_id"]?.ToString()))
            {
               _filter += $" AND a.ota_prod_id = :ota_prod_id \n";
                _params.Add("ota_prod_id", cond["ota_prod_id"].ToString());
            }

            if (!string.IsNullOrEmpty(cond["ota_prod_name"]?.ToString()))
            {
                _filter += $" AND a.ota_prod_name LIKE :ota_prod_name \n";
                _params.Add("ota_prod_name", $"%{cond["ota_prod_name"]}%");
            }

            if (!string.IsNullOrEmpty(cond["prod_name"]?.ToString()))
            {
                _filter += $" AND a.prod_name LIKE :prod_name \n";
                _params.Add("prod_name", $"%{cond["prod_name"]}%");
            }

             if (!string.IsNullOrEmpty(cond["pkg_name"]?.ToString()))
            {
                _filter += $" AND a.pkg_name LIKE :pkg_name \n";
                _params.Add("pkg_name", $"%{cond["pkg_name"]}%");
            }

            if (!string.IsNullOrEmpty(cond["sku_name"]?.ToString()))
            {
                _filter += $" AND a.sku_name LIKE :sku_name \n";
                _params.Add("sku_name", $"%{cond["sku_name"]}%");
            }

            if (!string.IsNullOrEmpty(cond["map_status"]?.ToString()))
            {
                _filter += $" AND a.map_status = :map_status \n";
                _params.Add("map_status", cond["map_status"].ToString());
            }

            return (cond: _filter, args: _params);
        }
         
        public ProductMapExModel GetMap(Int64? map_seq = null, Int64? ota_prod_oid = null, Int64? prod_oid = null, string locale = null)
        {
            try
            {
                if (map_seq == null && prod_oid == null && ota_prod_oid == null) throw new NullReferenceException();

                SqlMapper.AddTypeHandler(typeof(List<string>), new ObjectJsonMapper());

                using (var conn = new NpgsqlConnection(Website.Instance.SqlConnectionString))
                {
                    var sqlParams = new ExpandoObject() as IDictionary<string, object>;
                    var sqlStmt = $@"select * from ({query_sql}) a
where 1=1 ";
                    if (map_seq != null)
                    {
                        sqlStmt += $@" and a.map_seq=:map_seq ";
                        sqlParams.Add("map_seq", map_seq);
                    }

                    if (ota_prod_oid != null)
                    {
                        sqlStmt += $@" and a.ota_prod_oid=:ota_prod_oid ";
                        sqlParams.Add("ota_prod_oid", ota_prod_oid);
                    }

                    if (prod_oid != null)
                    {
                        sqlStmt += $@" and a.prod_oid=:prod_oid ";
                        sqlParams.Add("prod_oid", prod_oid);
                    }

                    sqlParams.Add("locale", locale); // 必要的!

                    return conn.QuerySingleOrDefault<ProductMapExModel>(sqlStmt, sqlParams);
                }
            }
            catch (Exception ex)
            {
                // Website.Instance.logger.Fatal($"GetCodes:Exception:Message={ex.Message},StackTrace={ex.StackTrace}");
                throw ex;
            }
        }


        public ProductMapModel GetProductMapByPLU(string ota_plu)
        {
            try
            {
                SqlMapper.AddTypeHandler(typeof(List<string>), new ObjectJsonMapper());

                using (var conn = new NpgsqlConnection(Website.Instance.SqlConnectionString))
                {
                    var sqlParams = new ExpandoObject() as IDictionary<string, object>;
                    var sqlStmt = $@"select * from product_map a where ota_plu=:ota_plu ";
                    sqlParams.Add("ota_plu", ota_plu);
                     
                    return conn.QuerySingleOrDefault<ProductMapModel>(sqlStmt, sqlParams);
                }
            }
            catch (Exception ex)
            {
                // Website.Instance.logger.Fatal($"GetCodes:Exception:Message={ex.Message},StackTrace={ex.StackTrace}");
                throw ex;
            }
        }

        #endregion 查詢作業 --- end

        /////////////////////
        
        public void Insert(ProductMapModel req)
        {
            try
            {
                using (var conn = new NpgsqlConnection(Website.Instance.SqlConnectionString))
                { 
                    var sqlStmt = @"insert into product_map (ota_prod_id, ota_prod_name, ota_plu, map_prod_mode, map_pkg_mode,
  map_time_mode, map_status, prod_oid, prod_name, pkg_oid, pkg_name, item_oid, sku_id, sku_nae, time_slots, create_user)
values (:ota_prod_id, :ota_prod_name, :ota_plu, :map_prod_mode, :map_pkg_mode, :map_time_mode, :map_status, :prod_oid, :prod_name,
  :pkg_oid, :pkg_name, :item_oid, :sku_id, :sku_nae, :time_slots, :create_user) "; 
                    
                    conn.Execute(sqlStmt, req);
                }
            }
            catch (Exception ex)
            {
                // Website.Instance.logger.Fatal($"GetCount:Exception:Message={ex.Message},StackTrace={ex.StackTrace}");
                throw ex;
            }
        }

        public void Expand(InsertProductMapModel req)
        {
            try
            {
                SqlMapper.AddTypeHandler(typeof(List<string>), new ObjectJsonMapper());

                using (var conn = new NpgsqlConnection(Website.Instance.SqlConnectionString))
                {
                    conn.Open();
                    using (var trans = conn.BeginTransaction())
                    {
                        foreach (var pkg in req.pkgs)
                        {
                            foreach (var sku in pkg.skus)
                            {
                                // 把場次時間展開, 並新增對應
                                if (req.allow_seltime && req.time_slots?.Count() > 0)
                                {
                                    foreach (var slot in req.time_slots)
                                    {
                                        var plu = $"{req.prod_oid}|{pkg.pkg_oid}|{sku.sku_id}|{slot}";

                                        var sqlStmt = @"insert into product_map (ota_prod_id, ota_prod_name, ota_plu, map_mode, map_status,
  allow_seltime, prod_oid, prod_name, pkg_oid, pkg_name, sku_id, sku_name, time_slot, create_user)
values (:ota_prod_id, :ota_prod_name, :ota_plu, :map_mode, :map_status, :allow_seltime, :prod_oid, :prod_name,
  :pkg_oid, :pkg_name, :sku_id, :sku_name, :time_slot, :create_user) ";

                                        conn.Execute(sqlStmt, new
                                        {
                                            ota_prod_id = req.ota_prod_id,
                                            ota_prod_name = req.ota_prod_name,
                                            ota_plu = plu,
                                            map_mode = req.map_mode,
                                            map_status = req.map_status,
                                            allow_seltime = req.allow_seltime,
                                            prod_oid = req.prod_oid,
                                            prod_name = req.prod_name,
                                            pkg_oid = pkg.pkg_oid,
                                            pkg_name = pkg.pkg_name,
                                            sku_id = sku.sku_id,
                                            sku_name = sku.sku_name,
                                            time_slot = slot,
                                            create_user = req.create_user
                                        }, transaction: trans);
                                    }
                                }
                                // 新增無場次對應
                                else
                                {
                                    var plu = $"{req.prod_oid}|{pkg.pkg_oid}|{sku.sku_id}";

                                    var sqlStmt = @"insert into product_map (ota_prod_id, ota_prod_name, ota_plu, map_mode, map_status,
  allow_seltime, prod_oid, prod_name, pkg_oid, pkg_name, sku_id, sku_name, create_user)
values (:ota_prod_id, :ota_prod_name, :ota_plu, :map_mode, :map_status, :allow_seltime, :prod_oid, :prod_name,
  :pkg_oid, :pkg_name, :sku_id, :sku_name, :create_user) ";

                                    conn.Execute(sqlStmt, new
                                    {
                                        ota_prod_id = req.ota_prod_id,
                                        ota_prod_name = req.ota_prod_name,
                                        ota_plu = plu,
                                        map_mode = req.map_mode,
                                        map_status = req.map_status,
                                        allow_seltime = req.allow_seltime,
                                        prod_oid = req.prod_oid,
                                        prod_name = req.prod_name,
                                        pkg_oid = pkg.pkg_oid,
                                        pkg_name = pkg.pkg_name,
                                        sku_id = sku.sku_id,
                                        sku_name = sku.sku_name,
                                        create_user = req.create_user
                                    }, transaction: trans);
                                }
                            }
                        }

                        trans.Commit();
                    }
                }
            }
            catch (Exception ex)
            {
                // Website.Instance.logger.Fatal($"GetCount:Exception:Message={ex.Message},StackTrace={ex.StackTrace}");
                throw ex;
            }
        }

        public void Update(ProductMapModel req)
        {
             try
            {
                using (var conn = new NpgsqlConnection(Website.Instance.SqlConnectionString))
                {
                     var sqlStmt = @"update product_map set ota_prod_id=:ota_prod_id, ota_prod_name=:ota_prod_name,
 map_mode=:map_mode, map_status=:map_status, allow_seltime=:allow_seltime, time_slot=:time_slot,
 modify_user=:modify_user, modify_date=now()
where map_seq=:map_seq "; 
                    

                    conn.Execute(sqlStmt, req);
                }
            }
            catch (Exception ex)
            {
                // Website.Instance.logger.Fatal($"GetCount:Exception:Message={ex.Message},StackTrace={ex.StackTrace}");
                throw ex;
            }
        }

        public void Remove(Int64[] map_array)
        {
             try
            {
                using (var conn = new NpgsqlConnection(Website.Instance.SqlConnectionString))
                {
                     var sqlStmt = @"delete from product_map where map_seq=ANY(ARRAY[:map_array])"; 

                    conn.Execute(sqlStmt, new { map_array });
                }
            }
            catch (Exception ex)
            {
                // Website.Instance.logger.Fatal($"GetCount:Exception:Message={ex.Message},StackTrace={ex.StackTrace}");
                throw ex;
            }
        }
    }
}
