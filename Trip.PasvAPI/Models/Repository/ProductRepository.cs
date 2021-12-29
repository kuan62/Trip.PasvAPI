using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
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
    public class ProductRepository
    {
        private readonly ProductProxy _prodProxy;
        private readonly IMemoryCache _cache;
         
        public ProductRepository(ProductProxy prodProxy, IMemoryCache memoryCache)
        {
            this._cache = memoryCache;
            this._prodProxy = prodProxy;
        }


        ////////////////

        #region Dummy 沙箱商品 --- start

        public DummyProductModel GetDummyProduct(string plu)
        {
            try
            {
                SqlMapper.AddTypeHandler(typeof(Dictionary<string, object>), new ObjectJsonMapper());
                using (var conn = new NpgsqlConnection(Website.Instance.SqlConnectionString))
                {
                    var sqlStmt = @$"SELECT * FROM dummy_product WHERE plu=:plu ";

                    return conn.QuerySingleOrDefault<DummyProductModel>(sqlStmt, new { plu });
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public int GetDummyProductQty(string plu)
        {
            try
            {
                using (var conn = new NpgsqlConnection(Website.Instance.SqlConnectionString))
                {
                    var sqlStmt = @$"SELECT qty FROM dummy_product WHERE plu=:plu ";

                    return conn.QuerySingleOrDefault<int>(sqlStmt, new { plu });
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion Dummy 沙箱商品 --- end

        ////////////////

        #region 查詢 KKday 商品

        public List<ProductPickerModel> GetProductList(string key)
        {
            try
            {
                var picker = new List<ProductPickerModel>();
                var result = _prodProxy.Search(key, Website.Instance.B2dApiAuthorToken);
                // 分析搜尋結果，整理商品清單
                if (!string.IsNullOrEmpty(result))
                {
                    var prod_lst = JArray.FromObject(JObject.Parse(result)["prods"]).Select(p => new ProductPickerModel { prod_oid = Convert.ToInt64(p["prod_no"]), prod_name = p["prod_name"].ToString() });
                    foreach (var pd in prod_lst)
                    { 
                        // 商品加入清單
                        picker.Add(pd);
                    }
                }

                return picker;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public List<ProductPickerModel.PackageModel> GetPackageList(Int64 prod_oid)
        {
            try
            {
                var pkg_list = new List<ProductPickerModel.PackageModel>();
                var guid_token = Guid.NewGuid().ToString("N");
                var locale = "zh-TW";
                 
                var prod = GetProduct(guid_token, prod_oid, locale);
                return prod.pkg.Select(k => new ProductPickerModel.PackageModel {  pkg_oid = k.pkg_no, pkg_name = k.pkg_name }).ToList();  
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public List<ProductPickerModel.PackageModel.ItemModel> GetPackgeSkuList(Int64 prod_oid, Int64 pkg_oid)
        {
            try
            {
                var item_list = new List<ProductPickerModel.PackageModel.ItemModel>();
                var guid_token = Guid.NewGuid().ToString("N");
                var locale = "zh-TW";
                 
                var pkg = GetPackage(guid_token, prod_oid, pkg_oid, locale);

                var skus = new List<ProductPickerModel.PackageModel.ItemModel.skuModel>();
                foreach (var _sku in pkg.item[0].skus)
                {
                    skus.Add(new ProductPickerModel.PackageModel.ItemModel.skuModel()
                    {
                        sku_id = _sku.sku_id,
                        sku_name = string.Join(",", _sku.spec.Select(s => s.Value))
                    });
                }

                item_list.Add(new ProductPickerModel.PackageModel.ItemModel()
                {
                    item_oid = pkg.item[0].item_no,
                    sku = skus
                });

                return item_list;

            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public ProductRespModel GetProduct(string token, Int64 prod_oid, string locale = null)
        {
            // 判斷 KKday 商品編號
            if (prod_oid == 0) throw new InvalidOperationException($"prod_oid = 0 is invalid");

            var cacheKey = $"KKDAY_PROD_{prod_oid}_{locale}";
            ProductRespModel prod = null;

            //if (!_cache.TryGetValue(cacheKey, out prod))
            {
                var result = _prodProxy.GetProduct(new ProductReqModel()
                {
                    locale = LocaleMapping.ToKKdayLocale(locale),
                    state = MarketingMapping.ToKKdayState(locale), // 設為台灣地區為主
                    prod_no = prod_oid.ToString()
                }, Website.Instance.B2dApiAuthorToken);

                Console.WriteLine($"Product Result => {result}");

                var settings = new JsonSerializerSettings
                {
                    MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
                    DateParseHandling = DateParseHandling.None,
                    Converters = { new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal } }
                };

                prod = JsonConvert.DeserializeObject<ProductRespModel>(result, settings);
                if (!prod.result.Equals("00"))
                {
                    throw new NullReferenceException($"prod_oid={ prod_oid } is invalid!");
                }

                // store in the cache
                _cache.Set(cacheKey, prod, new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromHours(3)));
            }
            //else
            //{
            //    throw new Exception($"{cacheKey} retrieved from cache.");
            //}

            return prod; 
        }

        #endregion 查詢 KKday 商品

        ////////////////

        #region 查詢 套餐+旅規

        public PackageRespModel GetPackage(string token, Int64 prod_oid, Int64 pkg_oid, string locale = null)
        { 
            var cacheKey = $"KKDAY_PROD_PKG_{prod_oid}_{pkg_oid}_{locale}";
            PackageRespModel pkg = null;

            //if (!_cache.TryGetValue(cacheKey, out pkg))
            {
                // fetch the value from the source
                var result = _prodProxy.GetPackage(new QueryPackageModel()
                {
                    prod_no = prod_oid.ToString(),
                    pkg_no = pkg_oid.ToString(),
                    // currency = Website.Instance.AgentCurrency,
                    locale = LocaleMapping.ToKKdayLocale(locale ?? "zh-TW"),
                    state = MarketingMapping.ToKKdayState(locale ?? "zh-TW"),  // 設為台灣地區為主
                }, Website.Instance.B2dApiAuthorToken);

                Console.WriteLine($"Package Result => {result}");

                var settings = new JsonSerializerSettings
                {
                    MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
                    DateParseHandling = DateParseHandling.None,
                    Converters = { new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal } }
                };

                pkg = JsonConvert.DeserializeObject<PackageRespModel>(result, settings);

                if (pkg == null || pkg.item == null || pkg.item.Count < 1)
                {
                    throw new NullReferenceException($"Product={ prod_oid }, Package={ pkg_oid } with null object.");
                }

                // store in the cache
                _cache.Set(cacheKey, pkg, new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromHours(3)));
            }
            //else
            //{
            //    throw new Exception($"{cacheKey} retrieved from cache.");
            //}

            return pkg;
        }

        public int GetPackageQty(string token, Int64 prod_oid, Int64 pkg_oid, string sku_id, string start_date)
        {
            var pkg = GetPackage(token, prod_oid, pkg_oid);
            var item = pkg.item[0];
            var qty = 0;

            if (item.inventory_set != 0)
            {
                if (item?.position?.result == "00" && item.inventory_set == 1 && item.inventory_type == 0)
                {
                    qty = (item.position.item_remain_qty ?? 0);
                }
                else if (item?.position?.result == "00" && item.inventory_set == 1 && item.inventory_type == 1)
                {
                    var item_qty = item.position.itemCal_qty.Where(c => c.date == start_date).Select(c => c.remain_qty.FirstOrDefault().Value).FirstOrDefault();
                    qty = item_qty != null ? Convert.ToInt32(item_qty) : 0; // Key=場次?
                }
                else if (item?.position?.result == "00" && item.inventory_set == 2 && item.inventory_type == 0)
                {
                    var sku_qty = item.position.skuCal_qty.Where(s => s.sku_id == sku_id).SelectMany(s =>
                                    s.sku_cal.Where(c => c.date == start_date).Select(q => q.remain_qty.FirstOrDefault().Value)).FirstOrDefault();
                    qty = sku_qty.HasValue ? sku_qty.Value : 0;
                }
            }
            else // inventory_set=0, 不限量
            {
                qty = 20; // 數量一律為(上限) 20 !!
            }

            return qty;
        }

        #endregion 查詢 套餐+旅規

        /////////////////
    }
}