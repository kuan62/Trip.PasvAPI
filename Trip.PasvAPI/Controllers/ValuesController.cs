using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Trip.PasvAPI.AppCode;
using Trip.PasvAPI.Models.Model;
using Trip.PasvAPI.Proxy;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Trip.PasvAPI.Controllers
{
    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
        // GET: api/values
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }
         
        [HttpGet("product/{prod_oid}")]
        public ProductRespModel GetProduct(Int64 prod_oid)
        {
            var prodProxy = HttpContext.RequestServices.GetService<ProductProxy>();
             
            var result = prodProxy.GetProduct(new ProductReqModel()
            {
                locale = "zh-tw",
                state = "tw", // 設為台灣地區為主
                prod_no = prod_oid.ToString()
            }, Website.Instance.B2dApiAuthorToken);

            var settings = new JsonSerializerSettings
            {
                MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
                DateParseHandling = DateParseHandling.None,
                Converters = { new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal } }
            };

            var prodResp = JsonConvert.DeserializeObject<ProductRespModel>(result, settings);

            //if (!prodResp.result.Equals("00"))
            //{
            //    //ViewBag.ProdNo = prod_no;
            //    //ViewBag.ErrorMessage = prodResp.result_msg;
            //    throw new Exception($"prod_oid={ prod_oid } is invalid, error={ prodResp.result_msg }");
            //}

            result = prodProxy.GetCountryCityList("zh-TW", Website.Instance.B2dApiAuthorToken);

            return prodResp;
        }

        [HttpGet("package/{prod_oid}/{pkg_oid}")]
        public PackageRespModel GetPackage(Int64 prod_oid, Int64 pkg_oid)
        {
            var req = new QueryPackageModel() {
                 prod_no = prod_oid.ToString(),
                 pkg_no = pkg_oid.ToString(),
                 currency = "TWD",
                 locale = "zh-tw",
                 state = "tw",  // 設為台灣地區為主
            };
              
            var prodProxy = HttpContext.RequestServices.GetService<ProductProxy>();
            var result = prodProxy.GetPackage(req, Website.Instance.B2dApiAuthorToken);
            Console.WriteLine($"Package Result => {result}");

            var settings = new JsonSerializerSettings
            {
                MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
                DateParseHandling = DateParseHandling.None,
                Converters = { new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal } }
            };
            var resp = JsonConvert.DeserializeObject<PackageRespModel>(result, settings);

            //if (resp == null || resp.item == null || resp.item.Count < 1)
            //{
            //    throw new NullReferenceException($"Product={req.prod_no}, Package={req.pkg_no} with null object.");
            //}

            return resp;
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
        }
    }
}
