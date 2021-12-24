using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Trip.PasvAPI.AppCode;
using Trip.PasvAPI.Models.Model;
using Trip.PasvAPI.Models.Repository;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Trip.PasvAPI.Controllers
{
    public class StaffController : Controller
    {
        // GET: /<controller>/
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        ///   產出結果給 bootstrap-table
        /// </summary> 
        public IActionResult FetchData(string filter, string sort, string order, int offset, int limit)
        {
            Dictionary<string, object> jsonData = new Dictionary<string, object>();

            try
            {
                var staffRepos = HttpContext.RequestServices.GetService<StaffRepository>();

                int totalCount = staffRepos.GetCount(filter);
                var sorting = string.IsNullOrEmpty(sort) ? string.Empty : $"{sort} {order}";
                var staffs = staffRepos.GetStaffs(filter, sorting, limit, offset, CultureInfo.CurrentCulture.ToString());

                jsonData.Add("total", totalCount);
                jsonData.Add("totalNotFiltered", totalCount);
                jsonData.Add("rows", staffs);
            }
            catch (Exception ex)
            {
                Website.Instance.logger.Fatal($"Exception:Message={ex.Message},StackTrace={ex.StackTrace}");
                jsonData.Add("total", 0);
                jsonData.Add("totalNotFiltered", 0);
                jsonData.Add("rows", new string[] { });
            }

            return Json(jsonData);
        }

        public IActionResult Add()
        {
            var staff = new StaffModel();

            return View("Edit", staff);
        }

        public IActionResult Insert([FromBody] StaffModel req)
        {
            var jsonData = new Dictionary<string, object>();

            try
            {
                var staffRepos = HttpContext.RequestServices.GetService<StaffRepository>();
                req.create_user = "SYSTEM"; // User.FindFirst("Account").Value;
                staffRepos.Insert(req);

                jsonData.Add("status", true);
            }
            catch (Exception ex)
            {
                Website.Instance.logger.FatalFormat("Message={0},StackTrace={1}", ex.Message, ex.StackTrace);
                jsonData.Clear();
                jsonData.Add("status", false);
                jsonData.Add("msg", ex.Message);
            }

            return Json(jsonData);
        }

        public IActionResult Edit(Int64 id)
        {
            var locale = CultureInfo.CurrentCulture.ToString();
            var staffRepos = HttpContext.RequestServices.GetService<StaffRepository>();
            var staff = staffRepos.GetStaff(oid: id, locale: locale);

            return View(staff);
        }

        public IActionResult Update([FromBody] StaffModel req)
        {
            var jsonData = new Dictionary<string, object>();

            try
            {
                var staffRepos = HttpContext.RequestServices.GetService<StaffRepository>();
                req.modify_user = "SYSTEM"; // User.FindFirst("Account").Value;
                staffRepos.Update(req);

                jsonData.Add("status", true);
            }
            catch (Exception ex)
            {
                Website.Instance.logger.FatalFormat("Message={0},StackTrace={1}", ex.Message, ex.StackTrace);
                jsonData.Clear();
                jsonData.Add("status", false);
                jsonData.Add("msg", ex.Message);
            }

            return Json(jsonData);
        }

        public IActionResult Password(Int64 id)
        {
            var locale = CultureInfo.CurrentCulture.ToString();
            var staffRepos = HttpContext.RequestServices.GetService<StaffRepository>();
            var staff = staffRepos.GetStaff(oid: id, locale: locale);

            return View(staff);
        }

        public IActionResult SetPassword(Int64 oid, string psw)
        {
            var jsonData = new Dictionary<string, object>();

            try
            {
                var staffRepos = HttpContext.RequestServices.GetService<StaffRepository>();
                var modify_user = "SYSTEM"; // User.FindFirst("Account").Value;
                staffRepos.SetPassword(oid, AesCryptHelper.aesEncryptBase64(psw, Website.Instance.AesCryptoKey), modify_user);

                jsonData.Add("status", true);
            }
            catch (Exception ex)
            {
                Website.Instance.logger.FatalFormat("Message={0},StackTrace={1}", ex.Message, ex.StackTrace);
                jsonData.Clear();
                jsonData.Add("status", false);
                jsonData.Add("msg", ex.Message);
            }

            return Json(jsonData);
        }
    }
}
