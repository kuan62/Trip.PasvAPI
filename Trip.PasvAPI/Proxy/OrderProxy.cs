using System;
using System.Net.Http;
using System.Text;
using Trip.PasvAPI.AppCode;
using Trip.PasvAPI.Models.Model;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Dynamic;
using System.Collections.Generic;

namespace Trip.PasvAPI.Proxy
{
    public class OrderProxy
    {
        private readonly IConfiguration _config;

        public OrderProxy(IConfiguration config)
        {
            _config = config;
        }

        public string GetOrders(QuerytOrdersReqModel req,string authorToken)
        {
            try
            {
                var jsonResult = "";
                var kkdayUrl = _config["KKdayApiUrl"];
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

                        string reqUrl = $"{kkdayUrl}/Order/QueryOrders";
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

        //簡易版
        public string GetOrderDetail(string order_no,string authorToken)
        {
            try
            {
                var jsonResult = "";
                var kkdayUrl = _config["KKdayApiUrl"];
                //var authorToken = Website.Instance.KKdayApiAuthorizeToken;

                using (var handler = new HttpClientHandler())
                {
                    // Ignore Certificate Error!!
                    handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;

                    using (var client = new HttpClient(handler))
                    { 
                        string reqUrl = $"{kkdayUrl}/Order/QueryOrderDtl/" + order_no;
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

        //詳細版，有商品資訊
        public string GetOrderDetailByProdInfo(string order_no, string authorToken, string locale_lang)
        {
            try
            {
                var jsonResult = "";
                var kkdayUrl = _config["KKdayApiUrl"];
                //var authorToken = Website.Instance.KKdayApiAuthorizeToken;

                using (var handler = new HttpClientHandler())
                {
                    // Ignore Certificate Error!!
                    handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
                    
                    using (var client = new HttpClient(handler))
                    {
                        var content = JsonConvert.SerializeObject(new { locale_lang }, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                        string reqUrl = $"{kkdayUrl}/Order/QueryOrderProductInfo/" + order_no;
                        using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, reqUrl))
                        {
                            request.Headers.Add("Authorization", $"Bearer {authorToken}");
                            request.Headers.Add("Accept", "application/json");
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

        public string CancelApply(string order_mid, string cancel_type, string authorToken)
        {
             try
            {
                var jsonResult = "";
                var kkdayUrl = _config["KKdayApiUrl"];
                //var authorToken = Website.Instance.KKdayApiAuthorizeToken;

                using (var handler = new HttpClientHandler())
                {
                    // Ignore Certificate Error!!
                    handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
                    
                    using (var client = new HttpClient(handler))
                    {
                        var req = new ExpandoObject() as IDictionary<string, object>;
                        req.Add("order_no", order_mid);
                        req.Add("cancel_type", cancel_type);
                        req.Add("cancel_desc", string.Empty);

                        var content = JsonConvert.SerializeObject(req, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                        var reqUrl = $"{kkdayUrl}/Order/Cancel";
                        using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, reqUrl))
                        {
                            request.Headers.Add("Authorization", $"Bearer {authorToken}");
                            request.Headers.Add("Accept", "application/json");
                            request.Content = new StringContent(
                            content,
                            Encoding.UTF8,
                            "application/json"
                            );
                            var response = client.SendAsync(request).Result;
                            jsonResult = response.Content.ReadAsStringAsync().Result;
                            Website.Instance.logger.Info($"B2D CancelApply Response: {jsonResult}");

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
