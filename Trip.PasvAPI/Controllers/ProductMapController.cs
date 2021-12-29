using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc; 
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Trip.PasvAPI.Models.Model;
using Trip.PasvAPI.Models.Repository;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Trip.PasvAPI.Controllers
{
    [Authorize(AuthenticationSchemes = "Broker.Staff", Policy = "Staff.Only")]
    public class ProductMapController : Controller
    {
        // GET: /<controller>/
        public IActionResult Index()
        {
            return View();
        }
         
        public IActionResult FetchData(string filter, string sort, string order, int offset, int limit)
        {
            Dictionary<string, object> jsonData = new Dictionary<string, object>();

            try
            {
                var mapRepos = HttpContext.RequestServices.GetService<ProductMapRepository>();

                int totalCount = mapRepos.GetCount(filter);
                var sorting = string.IsNullOrEmpty(sort) ? string.Empty : $"{sort} {order}";
                var codes = mapRepos.GetMaps(filter, sorting, limit, offset, CultureInfo.CurrentCulture.ToString());

                jsonData.Add("total", totalCount);
                jsonData.Add("totalNotFiltered", totalCount);
                jsonData.Add("rows", codes);
            }
            catch (Exception ex)
            {
                // Website.Instance.logger.Fatal($"Exception:Message={ex.Message},StackTrace={ex.StackTrace}");
                jsonData.Add("total", 0);
                jsonData.Add("totalNotFiltered", 0);
                jsonData.Add("rows", new string[] { });
            }

            return Json(jsonData);
        }

        public IActionResult Add()
        {
            var prodMap = new ProductMapExModel();

            return View(prodMap);
        }

        public IActionResult Insert([FromBody] ProductMapModel req)
        {
            var jsonData = new Dictionary<string, object>();

            try
            {
                var mapRepos = HttpContext.RequestServices.GetService<ProductMapRepository>();
                req.create_user = User.FindFirst("Account").Value;
                mapRepos.Insert(req);

                jsonData.Add("status", true);
            }
            catch (Exception ex)
            {
                // Website.Instance.logger.FatalFormat("Message={0},StackTrace={1}", ex.Message, ex.StackTrace);
                jsonData.Clear();
                jsonData.Add("status", false);
                jsonData.Add("msg", ex.Message);
            }

            return Json(jsonData);
        }

        public IActionResult Edit(Int64 id)
        {
            var mapRepos = HttpContext.RequestServices.GetService<ProductMapRepository>();
            var prodMap = mapRepos.GetMap(map_seq: id);

            return View(prodMap);
        }

        public IActionResult Update([FromBody] ProductMapModel req)
        {
            var jsonData = new Dictionary<string, object>();

            try
            {
                var mapRepos = HttpContext.RequestServices.GetService<ProductMapRepository>();
                req.modify_user = User.FindFirst("Account").Value;
                mapRepos.Update(req);

                jsonData.Add("status", true);
            }
            catch (Exception ex)
            {
                // Website.Instance.logger.FatalFormat("Message={0},StackTrace={1}", ex.Message, ex.StackTrace);
                jsonData.Clear();
                jsonData.Add("status", false);
                jsonData.Add("msg", ex.Message);
            }

            return Json(jsonData);
        }

        public IActionResult RemoveMaps(Int64[] maps)
        {
            var jsonData = new Dictionary<string, object>();

            try
            {
                var mapRepos = HttpContext.RequestServices.GetService<ProductMapRepository>();
                mapRepos.Remove(maps);

                jsonData.Add("status", true);
            }
            catch (Exception ex)
            {
                // Website.Instance.logger.FatalFormat("Message={0},StackTrace={1}", ex.Message, ex.StackTrace);
                jsonData.Clear();
                jsonData.Add("status", false);
                jsonData.Add("msg", ex.Message);
            }

            return Json(jsonData);
        }

        public IActionResult QueryProduct(string id)
        {
            try
            {
                var prodRepos = HttpContext.RequestServices.GetService<ProductRepository>();

                var result = prodRepos.GetProductList(id)?.Select(p => new { id = p.prod_oid, text = string.Format("{0} {1}", p.prod_oid, p.prod_name) }).ToList();
                return Json(result);
            }
            catch (Exception ex)
            {
                // Website.Instance.logger.FatalFormat("Message={0},StackTrace={1}", ex.Message, ex.StackTrace); 
                return Json(new string[] { ex.Message });
            }
        }

        public IActionResult QueryPackage(Int64 id)
        {
            var jsonData = new Dictionary<string, object>();

            try
            {
                var prodRepos = HttpContext.RequestServices.GetService<ProductRepository>();

                var result = prodRepos.GetPackageList(id).Select(p => new { id = p.pkg_oid, text = string.Format("{0} {1}", p.pkg_oid, p.pkg_name) }).ToList();

                jsonData.Add("status", true);
                jsonData.Add("pkgs", result);
            }
            catch (Exception ex)
            {
                // Website.Instance.logger.FatalFormat("Message={0},StackTrace={1}", ex.Message, ex.StackTrace);
                jsonData.Clear();
                jsonData.Add("status", false);
                jsonData.Add("msg", ex.Message);
            }

            return Json(jsonData);
        }

        public IActionResult QueryPackageSku(Int64 prod_oid, Int64 pkg_oid)
        {
            var jsonData = new Dictionary<string, object>();

            try
            {
                var prodRepos = HttpContext.RequestServices.GetService<ProductRepository>();

                var result = prodRepos.GetPackgeSkuList(prod_oid, pkg_oid);

                jsonData.Add("status", true);
                jsonData.Add("item", result[0]);
            }
            catch (Exception ex)
            {
                // Website.Instance.logger.FatalFormat("Message={0},StackTrace={1}", ex.Message, ex.StackTrace);
                jsonData.Clear();
                jsonData.Add("status", false);
                jsonData.Add("msg", ex.Message);
            }

            return Json(jsonData);
        }

        public IActionResult ExpandProductMap([FromBody] InsertProductMapModel req)
        {
            var jsonData = new Dictionary<string, object>();

            try
            {
                var mapRepos = HttpContext.RequestServices.GetService<ProductMapRepository>();
                req.create_user = User.FindFirst("Account").Value;

                mapRepos.Expand(req);

                jsonData.Add("status", true);
            }
            catch (Exception ex)
            {
                // Website.Instance.logger.FatalFormat("Message={0},StackTrace={1}", ex.Message, ex.StackTrace);
                jsonData.Clear();
                jsonData.Add("status", false);
                jsonData.Add("msg", ex.Message);
            }

            return Json(jsonData);
        }
    }
}
