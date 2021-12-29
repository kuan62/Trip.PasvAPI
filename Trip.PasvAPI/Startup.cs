using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Trip.PasvAPI.AppCode;
using Trip.PasvAPI.Models.Repository;
using Trip.PasvAPI.Proxy;

namespace Trip.PasvAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            // 網站初始化
            Website.Instance.Init(configuration);
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
             // 使用者語系
            var supportedCultures = new[]
            {
                new CultureInfo("zh-TW")
                //new CultureInfo("en-US"), 
                //new CultureInfo("zh-CN"),
                //new CultureInfo("ja-JP"),
                //new CultureInfo("ko-KR"),
            };

            services.Configure<RequestLocalizationOptions>(options =>
            {

                options.DefaultRequestCulture = new RequestCulture(culture: "zh-TW", uiCulture: "zh-TW");
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;

                options.RequestCultureProviders = new List<IRequestCultureProvider>
                {
                    // 依序進行判斷文化特性，以上都沒有才會以 DefaultRequestCulture 決定語系。
                    new CookieRequestCultureProvider(), // (1) 透過 cookie 的值判斷要求的文化特性資訊。
                    new QueryStringRequestCultureProvider(), // (2) 透過查詢字串中的值，決定要求的文化特性資訊。
                    new AcceptLanguageHeaderRequestCultureProvider(), // (3) 透過 Accept-Language 標頭的值，判斷要求的文化特性資訊。 
                };
            });

            #region Dependent Injection

            // Proxies
            services.AddSingleton<TtdOpenProxy>();
            services.AddSingleton<ProductProxy>();
            services.AddSingleton<BookingProxy>();
            services.AddSingleton<OrderProxy>();

            // Repository
            services.AddSingleton<TripTransLogRepository>();
            services.AddSingleton<TripOrderRepository>();
            services.AddSingleton<OrderMasterRepository>();
            services.AddSingleton<ProductRepository>();
            services.AddSingleton<ProductMapRepository>();
            services.AddSingleton<StaffRepository>();
            services.AddSingleton<CodeRepository>();
            services.AddSingleton<OrderBookingRepository>();

            #endregion Dependent Injection

            /////////

            #region Cookie 驗證 --- start

            // 新增 Cookie 驗證服務
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie("Broker.Staff", options =>
                {
                    options.Cookie.Name = ".Broker.Staff.SharedCookie";
                    options.LoginPath = "/Login/";
                    options.SlidingExpiration = true;
                    // options.Cookie.Domain = "kkday.com";

                    options.Events.OnValidatePrincipal = (context) =>
                    {
                        int failCount = 0;
                        var identity = (ClaimsIdentity)context.Principal.Identity;
                        var versionKey = identity.FindFirst("Ver");
                        if (versionKey == null)
                        {
                            failCount++;
                        }
                        else
                        {
                            var serverVersion = new Version(Website.Instance.PrincipleVersion);
                            var localeVersion = new Version(versionKey.Value);
                            // Principal Version => 0:同版本, 1:不同版本
                            if (serverVersion.CompareTo(localeVersion) == 1)
                            {
                                failCount++;
                            }
                        }
                          
                        if (failCount > 0)
                        {
                            context.RejectPrincipal();
                            context.Response.Redirect("/Login/SignOutAsync");
                        }

                        // 驗證cookie內IdentityType是否存在
                        if (identity.FindFirst("IdentityType") == null)
                        {
                            // 不存在則否認此cookie
                            context.RejectPrincipal();
                        }

                        return Task.CompletedTask;
                    };

                });

            // 指定Cookie授權政策區分不同身分者
            services.AddAuthorization(options =>
            {
                options.AddPolicy("Staff.Only", policy => policy.RequireClaim("IdentityType", "USER"));
            });

            #endregion Cookie 驗證 --- end

            /////////

            services.AddControllers().AddNewtonsoftJson();
            services.AddControllersWithViews().AddNewtonsoftJson();
            services.AddRazorPages().AddNewtonsoftJson();

            services.AddControllersWithViews();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            // 抓取遠端 Client IP
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor |
                ForwardedHeaders.XForwardedProto
            });

            // 允許全區跨域存取 Enable Global CORS  
            app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

            // 多語系初始設定
            var locOptions = app.ApplicationServices.GetService<IOptions<RequestLocalizationOptions>>();
            app.UseRequestLocalization(locOptions.Value);

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
