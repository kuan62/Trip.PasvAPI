using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
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

            #endregion Dependent Injection

            ///

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
