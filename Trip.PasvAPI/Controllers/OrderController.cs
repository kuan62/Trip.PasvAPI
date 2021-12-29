using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization; 
using Microsoft.Extensions.DependencyInjection;
using Trip.PasvAPI.Models.Repository;
using System.Globalization;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Trip.PasvAPI.Controllers
{
    [Authorize(AuthenticationSchemes = "Broker.Staff", Policy = "Staff.Only")]
    public class OrderController : Controller
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
                var orderRepos = HttpContext.RequestServices.GetService<OrderMasterRepository>();

                int totalCount = orderRepos.GetCount(filter);
                var sorting = string.IsNullOrEmpty(sort) ? string.Empty : $"{sort} {order}";
                var codes = orderRepos.GetOrders(filter, sorting, limit, offset);

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

    }
}
