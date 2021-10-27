using System;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;
using Trip.PasvAPI.AppCode;

namespace Trip.PasvAPI.Proxy
{
    public class TtdOpenProxy
    {
        private string _endpoint_url;

        public TtdOpenProxy(IConfiguration config)
        {
            _endpoint_url = config["Proxy:Trip:Url"];
        }

        public async System.Threading.Tasks.Task<string> PostAsync(string json_data)
        {
            try
            {
                using (var handler = new HttpClientHandler())
                {
                    // Ignore Certificate Error!!
                    handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;

                    using (var client = new HttpClient(handler))
                    {
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        using (HttpContent content = new StringContent(json_data))
                        {
                            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                            using (var response = await client.PostAsync(_endpoint_url, content))
                            {
                                //如果 httpstatus code 不是 200 時會直接丟出 exception
                                //response.EnsureSuccessStatusCode();

                                //if (response.StatusCode != System.Net.HttpStatusCode.OK ||
                                //    response.StatusCode != System.Net.HttpStatusCode.NoContent)
                                //{
                                //    throw new Exception($"Http Response.StatusCode={ response.StatusCode }");
                                //}

                                return await response.Content.ReadAsStringAsync();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
