using System;
namespace Trip.PasvAPI.Models.Model.Trip
{
    // 回覆給 TTdOpen 的 JSON 結構
    public class TTdResponseModel
    {
        public TTdResponseHeaderModel header { get; set; }
        public class TTdResponseHeaderModel
        {
            public string resultCode { get; set; }
            public string resultMessage { get; set; }
        }

        public dynamic body { get; set; }
    } 
}
