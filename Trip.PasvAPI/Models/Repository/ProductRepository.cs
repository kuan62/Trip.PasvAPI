using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
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

        public ProductRespModel GetProduct(string token, Int64 prod_oid, string locale)
        {
            // 判斷 KKday 商品編號
            if (prod_oid == 0) throw new InvalidOperationException($"prod_oid = 0 is invalid");

            var cacheKey = $"KKDAY_PROD_{prod_oid}_{locale}";
            ProductRespModel prod = null;

            if (!_cache.TryGetValue(cacheKey, out prod))
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
            else
            {
                throw new Exception($"{cacheKey} retrieved from cache.");
            }

            return prod; 
        }

        #endregion 查詢 KKday 商品

        ////////////////

        #region 查詢 KKday 商品&套餐+旅規

        public PackageRespModel GetPackage(string token, Int64 prod_oid, Int64 pkg_oid, string sku_id, string locale)
        { 
            var cacheKey = $"KKDAY_PROD_PKG_{prod_oid}_{pkg_oid}_{locale}";
            PackageRespModel pkg = null;

            if (!_cache.TryGetValue(cacheKey, out pkg))
            {
                // fetch the value from the source
                var result = _prodProxy.GetPackage(new QueryPackageModel()
                {
                    prod_no = prod_oid.ToString(),
                    pkg_no = pkg_oid.ToString(),
                    // currency = Website.Instance.AgentCurrency,
                    locale = LocaleMapping.ToKKdayLocale(locale),
                    state = MarketingMapping.ToKKdayState(locale),  // 設為台灣地區為主
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
            else
            {
                throw new Exception($"{cacheKey} retrieved from cache.");
            }

            return pkg;
        }

        #endregion 查詢 KKday 商品&套餐+旅規
    }
}