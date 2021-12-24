using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Trip.PasvAPI.Models;
using Trip.PasvAPI.Models.Repository;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Trip.PasvAPI.Controllers
{
       public class CodeController : Controller
    {
        const int PAGE_SIZE = 50;

        //private IMapper _mapper;

        //public CodeController(IMapper mapper)
        //{
        //    this._mapper = mapper;
        //}
          
        // GET: /<controller>/
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        ///   產出結果給 bootstrap-table
        /// </summary>
        /// <param name="filter">Json String (Nullable)</param>
        /// <param name="sort">Filed-Name</param>
        /// <param name="order">Asc/Desc</param>
        /// <param name="offset">Skip</param>
        /// <param name="limit">Size</param>
        /// <returns></returns>
        public IActionResult FetchData(string filter, string sort, string order, int offset, int limit)
        {
            Dictionary<string, object> jsonData = new Dictionary<string, object>();

            try
            {
                var codeRepos = HttpContext.RequestServices.GetService<CodeRepository>();

                int totalCount = codeRepos.GetCount(filter);
                var sorting = string.IsNullOrEmpty(sort) ? string.Empty : $"{sort} {order}";
                var codes = codeRepos.GetCodes(filter, sorting, limit, offset, CultureInfo.CurrentCulture.ToString());

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
            var code = new CodeModel() { code_name = new Dictionary<string, string>() };

            return View("Edit", code);
        }

        public IActionResult InsertItem([FromBody] CodeModel req)
        {
            var jsonData = new Dictionary<string, object>();

            try
            {
                var codeRepos = HttpContext.RequestServices.GetService<CodeRepository>();
                req.create_user = "SYSTEM";  // User.FindFirst("Account").Value;
                codeRepos.InsertItem(req);

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
            var codeRepos = HttpContext.RequestServices.GetService<CodeRepository>();
            var code = codeRepos.GetCode(id);

            ViewBag.CodeNames = code.code_name;

            return View(code);
        }

        // 自動完成(Auto-Complete) 查詢專用
        public IActionResult QueryCodeTypes(string id)
        {
            try
            {
                var codeRepos = HttpContext.RequestServices.GetService<CodeRepository>();
                List<string> geoCities = codeRepos.GetCodeTypesForAC(id);

                return Json(geoCities);
            }
            catch (Exception ex)
            {
                // Website.Instance.logger.FatalFormat("Message={0},StackTrace={1}", ex.Message, ex.StackTrace);
                return Json(string.Empty);
            }
        }

        public IActionResult UpdateItem([FromBody] CodeModel req)
        {
            var jsonData = new Dictionary<string, object>();

            try
            {
                var codeRepos = HttpContext.RequestServices.GetService<CodeRepository>();
                req.modify_user = "SYSTEM";  // User.FindFirst("Account").Value;
                codeRepos.UpdateItem(req);

                jsonData.Add("status", true);
            }
            catch (Exception ex)
            {
                // Website.Instance.logger.FatalFormat("Message={0},StackTrace={1}", ex.Message, ex.StackTrace);
            }

            return Json(jsonData);
        }
    }
}
