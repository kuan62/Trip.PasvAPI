using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Trip.PasvAPI.Models.Model
{
    public class OrderDetailModel
    {
        public string result { get; set; }
        public string result_msg { get; set; }
        public string partner_order_no { get; set; }
        public string prod_no { get; set; }
        public string pkg_no { get; set; }
        public string buyer_first_name { get; set; }
        public string buyer_last_name { get; set; }
        public string buyer_Email { get; set; }
        public string buyer_tel_country_code { get; set; }
        public string buyer_tel_number { get; set; }
        public string s_date { get; set; }
        public string e_date { get; set; }
        [JsonProperty("event")]
        public string time_slots { get; set; }
        public string event_no { get; set; }
        public string event_backup_data { get; set; }
        public double total_price { get; set; }
        public string currency { get; set; }
        public string order_note { get; set; }
        public List<Custom> custom { get; set; }
        public List<Traffic> traffic { get; set; }
        public MobileDevice mobile_device { get; set; }
        public List<SkuModel> skus { get; set; }
        public string order_no { get; set; }
        public string order_date { get; set; }
        public string order_status { get; set; }
        public int qty_total { get; set; }
        public double total_pay { get; set; }
        public double refund_price { get; set; }
    }

    public class SkuModel
    {
        public string sku_id { get; set; }
        public Dictionary<string, string> spec { get; set; }
        public int qty { get; set; }
        public double? price { get; set; }
    }
}
