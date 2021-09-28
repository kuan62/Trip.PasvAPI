using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Trip.PasvAPI.AppCode;
using Trip.PasvAPI.Models.Model;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;


namespace Trip.PasvAPI.Proxy
{
    public class ProductProxy
    {
        private readonly IConfiguration _config;

        public ProductProxy(IConfiguration config)
        {
            _config = config;
        }

        public string GetProduct(ProductReqModel req, string authorToken)
        {
            try
            {
                var jsonResult = "";
                var kkdayUrl = _config["B2D_API:Url"];
                //var authorToken = Website.Instance.KKdayApiAuthorizeToken;

                using (var handler = new HttpClientHandler())
                {
                    // Ignore Certificate Error!!
                    handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;

                    using (var client = new HttpClient(handler))
                    {

                        #region JSON Payload
                         
                        var content = JsonConvert.SerializeObject(req, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                        // Console.WriteLine($"Product Req Payload => {content}");

                        #endregion JSON Payload

                        string reqUrl = $"{kkdayUrl}/Product/QueryProduct";
                        using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, reqUrl))
                        {
                            request.Headers.Add("Authorization", $"Bearer {authorToken}");
                            request.Headers.Add("Accept", "application/json");
                            // Add body content
                            request.Content = new StringContent(
                                content,
                                Encoding.UTF8,
                                "application/json"
                            );

                            var response = client.SendAsync(request).Result;
                            jsonResult = response.Content.ReadAsStringAsync().Result;

                            if (response.StatusCode != System.Net.HttpStatusCode.OK)
                            {
                               throw new Exception($"{response.StatusCode} => {JsonConvert.SerializeObject(jsonResult)} ");
                            }
                        }
                    }
                }

                return jsonResult;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public string GetPackage(PackageReqModel req, string authorToken)
        {
            try
            {
                var jsonResult = "";
                var kkdayUrl = _config["B2D_API:Url"];
                //var authorToken = Website.Instance.KKdayApiAuthorizeToken;

                using (var handler = new HttpClientHandler())
                {
                    // Ignore Certificate Error!!
                    handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;

                    using (var client = new HttpClient(handler))
                    {

                        #region JSON Payload

                        // 過濾不必要的參數
                        req = JsonConvert.DeserializeObject<PackageReqModel>(JsonConvert.SerializeObject(req));

                        var content = JsonConvert.SerializeObject(req, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                        Console.WriteLine($"Package Req Payload => {content}");

                        #endregion JSON Payload

                        string reqUrl = $"{kkdayUrl}/Product/QueryPackage";
                        using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, reqUrl))
                        {
                            request.Headers.Add("Authorization", $"Bearer {authorToken}");
                            request.Headers.Add("Accept", "application/json");
                            // Add body content
                            request.Content = new StringContent(
                                content,
                                Encoding.UTF8,
                                "application/json"
                            );

                            var response = client.SendAsync(request).Result;
                            jsonResult = response.Content.ReadAsStringAsync().Result;

                            if (response.StatusCode != System.Net.HttpStatusCode.OK)
                            {
                                throw new Exception($"{response.StatusCode} => {JsonConvert.SerializeObject(jsonResult)} ");
                            }
                        }
                    }
                }

                return jsonResult;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public EventBackupRespModel GetEventBackup(EventBackupReqModel req, string authorToken)
        {
            try
            {
                var jsonResult = "";
                var kkdayUrl = _config["B2D_API:Url"];
                //var authorToken = Website.Instance.KKdayApiAuthorizeToken;

                using (var handler = new HttpClientHandler())
                {
                    // Ignore Certificate Error!!
                    handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;

                    using (var client = new HttpClient(handler))
                    {
                        #region JSON Payload
                         
                        var content = JsonConvert.SerializeObject(req, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                        // Console.WriteLine($"Package Req Payload => {content}");

                        #endregion JSON Payload

                        string reqUrl = $"{kkdayUrl}/Product/QueryBackupEvent";
                        using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, reqUrl))
                        {
                            request.Headers.Add("Authorization", $"Bearer {authorToken}");
                            request.Headers.Add("Accept", "application/json");
                            // Add body content
                            request.Content = new StringContent(
                                content,
                                Encoding.UTF8,
                                "application/json"
                            );

                            var response = client.SendAsync(request).Result;
                            jsonResult = response.Content.ReadAsStringAsync().Result;

                            if (response.StatusCode != System.Net.HttpStatusCode.OK)
                            {
                                throw new Exception($"{response.StatusCode} => {JsonConvert.SerializeObject(jsonResult)} ");
                            }
                        }
                    }
                }

                return JsonConvert.DeserializeObject<EventBackupRespModel>(jsonResult);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string GetCountryCityList(string locale, string authorToken)
        {
            try
            {
                var jsonResult = "";
                var kkdayUrl = _config["B2D_API:Url"];
                //var authorToken = Website.Instance.KKdayApiAuthorizeToken;

                using (var handler = new HttpClientHandler())
                {
                    // Ignore Certificate Error!!
                    handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;

                    using (var client = new HttpClient(handler))
                    {
                        string reqUrl = $"{kkdayUrl}/Product/QueryCityList?locale={locale}";
                        using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, reqUrl))
                        {
                            request.Headers.Add("Authorization", $"Bearer {authorToken}");
                            request.Headers.Add("Accept", "application/json");

                            var response = client.SendAsync(request).Result;
                            jsonResult = response.Content.ReadAsStringAsync().Result;

                            if (response.StatusCode != System.Net.HttpStatusCode.OK)
                            {
                                throw new Exception($"{response.StatusCode} => {JsonConvert.SerializeObject(jsonResult)} ");
                            }
                        }
                    }
                }

                return jsonResult;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
