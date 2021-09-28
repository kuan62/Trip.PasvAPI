using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Trip.PasvAPI.Models;
using Trip.PasvAPI.Models.Model;

namespace Trip.PasvAPI.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            /*
            var resp = new OrderVerificationRespModel();
            resp.header = new OrderVerificationRespModel.HeaderModel()
            {
                resultCode = Trip.PasvAPI.Models.Enum.TTdResultCodeEnum.PRODUCT_REMOVED.GetHashCode().ToString("0000"),
                resultMessage = "驗證成功"
            };

            var _inventorys = new List<OrderVerificationRespModel.BodyModel.ItemModel.InventorysModel>();
            _inventorys.Add(new OrderVerificationRespModel.BodyModel.ItemModel.InventorysModel()
            {
                quantity = 0,
                useDate = "2021-09-30"
            });

            var _items = new List<OrderVerificationRespModel.BodyModel.ItemModel>();
            _items.Add(new OrderVerificationRespModel.BodyModel.ItemModel()
            {
                PLU = "test-plu-1",
                inventorys = _inventorys
            });

            resp.body = new OrderVerificationRespModel.BodyModel()
            {
                items = _items
            };

            var jsonData = System.Text.Json.JsonSerializer.Serialize(resp);
            // var req = System.Text.Json.JsonSerializer.Deserialize<OrderVerificationRespModel>(jsonData);
            */
             
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
