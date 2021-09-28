using System;
using System.Net.Http;
using System.Text;
using Trip.PasvAPI.AppCode;
using Trip.PasvAPI.Models.Model;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Trip.PasvAPI.Proxy
{
    public class BookingProxy
    {
        private readonly IConfiguration _config;

        public BookingProxy(IConfiguration config)
        {
            _config = config;
        }

        //AR 付款
        public string Booking(BookingDataModel req,string authorToken)
        {
            try
            {
                if (!Convert.ToBoolean(_config["AllowBooking"]??"False")) throw new InvalidOperationException("Booking Denied");

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
                        Console.WriteLine($"Booking Req Payload => {content}");

                        #endregion JSON Payload

                        string reqUrl = $"{kkdayUrl}/Booking";
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

        //信用卡付款
        public string CrtOrder(BookingDataModel req, string authorToken)
        {
            try
            {
                if (!Convert.ToBoolean(_config["AllowBooking"] ?? "False")) throw new InvalidOperationException("Create Order Denied");

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
                        Console.WriteLine($"CrtOrder Req Payload => {content}");

                        #endregion JSON Payload

                        string reqUrl = $"{kkdayUrl}/Booking/CrtOrder";
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

        //PaymentValid/PayUpdSuccessUpdOrder2 
        public string PaymentValid(PmchSslResponse req, string authorToken)
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


                    PmchValid valid = new PmchValid();
                    valid.mid = req.order_no;
                    valid.jsondata = req;

                    using (var client = new HttpClient(handler))
                    {

                        #region JSON Payload

                        var content = JsonConvert.SerializeObject(valid, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                        Console.WriteLine($"PaymentValid Req Payload => {content}");

                        #endregion JSON Payload

                        string reqUrl = $"{kkdayUrl}/Booking/PaymentValid";
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


        public string GetPmgwList(PmgwModel pmgwRq, string authorToken)
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

                        var content = JsonConvert.SerializeObject(pmgwRq, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                        Console.WriteLine($"GetPmgwList Req Payload => {content}");

                        #endregion JSON Payload

                        string reqUrl = $"{kkdayUrl}/Booking/PmgwList";
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

        public CurrencyModel GetCurrencyRateList()
        {
            try
            {
                var jsonResult = "";
                var kkdayUrl = _config["B2D_API:Url"];
                var authorToken = _config["B2D_API:AuthorToken"];

                CurrencyModel currencyRate = new CurrencyModel();

                //建立連線到WMS API
                using (var handler = new HttpClientHandler())
                {

                    // Ignore Certificate Error!!
                    handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
                    string reqUrl = $"{kkdayUrl}/Common/GetCurrency";
                    using (var client = new HttpClient(handler))
                    {
                        using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, reqUrl))
                        {
                            request.Headers.Add("Authorization", $"Bearer {authorToken}");
                            request.Headers.Add("Accept", "application/json");
                            // Add body content
                             

                            var response = client.SendAsync(request).Result;
                            jsonResult = response.Content.ReadAsStringAsync().Result;

                            if (response.StatusCode != System.Net.HttpStatusCode.OK)
                            {
                                throw new Exception($"{response.StatusCode} => {JsonConvert.SerializeObject(jsonResult)} ");
                            }

                            currencyRate = JsonConvert.DeserializeObject<CurrencyModel>(jsonResult);
                        }
                    }
                }

                return currencyRate;
            }
            catch (Exception ex)
            {
                Website.Instance.logger.Fatal($"BookingProxy GetCurrencyRateList:Message={ex.Message},StackTrace={ex.StackTrace}");
                throw ex;
            }
        }

        //查詢額度剩餘金額
        public QueryRSAmountModel QueryAmount(string authorToken)
        {
            try
            {
                var jsonResult = "";
                var kkdayUrl = _config["B2D_API:Url"];
                //var authorToken = Website.Instance.KKdayApiAuthorizeToken;
                QueryRSAmountModel quryAmount = new QueryRSAmountModel();
                using (var handler = new HttpClientHandler())
                {
                    // Ignore Certificate Error!!
                    handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;

                    using (var client = new HttpClient(handler))
                    {

                        #region JSON Payload

                        Dictionary<string, string> dateTimenNow = new Dictionary<string, string>();
                        dateTimenNow.Add("s_date", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                        dateTimenNow.Add("e_date", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                        var content = JsonConvert.SerializeObject(dateTimenNow, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                        Console.WriteLine($"QueryAmount Req Payload => {content}");

                        #endregion JSON Payload

                        string reqUrl = $"{kkdayUrl}/Booking/QueryAmount";
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

                            quryAmount = JsonConvert.DeserializeObject<QueryRSAmountModel>(jsonResult);
                        }
                    }
                }

                return quryAmount;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        //查詢商品版本
        public string  QueryProdVersion(string prodNo,string authorToken)
        {
            try
            {
                var jsonResult = "";
                var kkdayUrl = _config["B2D_API:Url"];
                //var authorToken = Website.Instance.KKdayApiAuthorizeToken;
                QueryRSAmountModel quryAmount = new QueryRSAmountModel();
                using (var handler = new HttpClientHandler())
                {
                    // Ignore Certificate Error!!
                    handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;

                    using (var client = new HttpClient(handler))
                    {

                        #region JSON Payload

                        Dictionary<string, string> dateTimenNow = new Dictionary<string, string>();
                        dateTimenNow.Add("s_date", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                        dateTimenNow.Add("e_date", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                        var content = JsonConvert.SerializeObject(dateTimenNow, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                        Console.WriteLine($"QueryAmount Req Payload => {content}");

                        #endregion JSON Payload

                        string reqUrl = $"{kkdayUrl}/Product/QueryProdVersion?prod_no={prodNo}&locale=zh-tw&state=TW";
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
                            var result = JObject.Parse(jsonResult);
                            if (result["result"].ToString() != "00")
                            {
                                Website.Instance.logger.Fatal($"BookingProxy QueryProdVersion:Message={result["result_msg"]?.ToString()}");
                                throw new Exception(result["result_msg"]?.ToString());
                            }
                            else
                            {
                                return result["prod_version"]?.ToString();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Website.Instance.logger.Fatal($"BookingProxy QueryProdVersion:Message={ex.Message},StackTrace={ex.StackTrace}");
                throw ex;
            }
        }

    }
}
