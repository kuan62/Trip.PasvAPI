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
    public class VoucherProxy
    {
        private readonly IConfiguration _config;

        public VoucherProxy(IConfiguration config)
        {
            _config = config;
        }

        public string GetVoucherLst(string order_mid, string authorToken)
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
                        var req = new ExpandoObject() as IDictionary<string, object>;
                        req.Add("order_no", order_mid); 
                       
                        var content = JsonConvert.SerializeObject(req, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                        var reqUrl = $"{kkdayUrl}/Voucher/QueryVoucherList";
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
                            Website.Instance.logger.Info($"B2D Voucher Response: {jsonResult}");

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

        // 
        public string GetVoucher(string order_mid, string order_file_id, string authorToken)
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
                        var req = new ExpandoObject() as IDictionary<string, object>;
                        req.Add("order_no", order_mid);
                        req.Add("order_file_id", order_file_id);
                       
                        var content = JsonConvert.SerializeObject(req, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                        var reqUrl = $"{kkdayUrl}/Voucher/Download";
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
                            Website.Instance.logger.Info($"B2D Voucher Response: {jsonResult}");

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
