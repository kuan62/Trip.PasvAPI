using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Trip.PasvAPI.AppCode;
using Trip.PasvAPI.Models.Repository;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Trip.PasvAPI.Controllers
{
    [AllowAnonymous]
    public class LoginController : Controller
    {
        // GET: /<controller>/
        public IActionResult Index()
        {
            return View();
        }
         
        public async Task<IActionResult> SignInAsync(string account, string passwd)
        {
             var jsonData = new Dictionary<string, object>();

            try
            {
                var clientIP = Request.HttpContext.Connection.RemoteIpAddress.ToString().Replace("::1", "127.0.0.1");
                var staffRepos = HttpContext.RequestServices.GetService<StaffRepository>();
                 
                var staff = staffRepos.GetStaff(account: account, passwd: AesCryptHelper.aesEncryptBase64(passwd, Website.Instance.AesCryptoKey));
                if (staff == null) throw new NullReferenceException("無效的帳號或密碼，請重新輸入!");
                 
                var claims = new List<Claim>
                {
                    new Claim("Account", staff.staff_account),
                    new Claim(ClaimTypes.Name, staff.staff_name),
                    new Claim(ClaimTypes.Email, staff.staff_account), 
                    new Claim("IdentityType", "USER"),
                    new Claim("IP", clientIP), //IpAddress 存於Claims 
                    new Claim("Ver", Website.Instance.PrincipleVersion), // 帶入當前ClaimPriciple版號
                };

                var userIdentity = new ClaimsIdentity(claims, "login");

                ClaimsPrincipal principal = new ClaimsPrincipal(userIdentity);

                await HttpContext.SignInAsync(
                   "Broker.Staff",
                    principal,
                    new AuthenticationProperties()
                    {
                        ExpiresUtc = DateTime.UtcNow.AddDays(10), // 預設 Cookie 有效時間
                        IsPersistent = true, // req.RememberMe,
                        AllowRefresh = true
                    });

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
         
        public async Task<IActionResult> SignOutAsync()
        { 
            // 清除已登入 User
            await HttpContext.SignOutAsync("Broker.Staff");

            return Redirect("~/");
        }

    }
}
