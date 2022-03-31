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
        public List<OrderCustom> custom { get; set; }
        public List<OrderTraffic> traffic { get; set; }
        public OrderMobileDevice mobile_device { get; set; }
        public List<OrderSkuModel> skus { get; set; }
        public string order_no { get; set; }
        public string order_date { get; set; }
        public string order_status { get; set; }
        public int qty_total { get; set; }
        public double total_pay { get; set; }
        public double refund_price { get; set; }
    }

    public partial class OrderCustom
    {
        public string cus_type { get; set; }
        public string english_last_name { get; set; }
        public string english_first_name { get; set; }
        public string native_last_name { get; set; }
        public string native_first_name { get; set; }
        public string gender { get; set; }
        public string nationality { get; set; }
        public string MTP_no { get; set; }
        public string id_no { get; set; }
        public string passport_no { get; set; }
        public string passport_expdate { get; set; }
        public string birth { get; set; }
        public string height { get; set; }
        public string height_unit { get; set; }
        public string weight { get; set; }
        public string weight_unit { get; set; }
        public string shoe { get; set; }
        public string shoe_unit { get; set; }
        public string shoe_type { get; set; }
        public string glass_degree { get; set; }
        public string meal { get; set; }
    }

    public class OrderTraffic
    {
        public class OrdertrafficCar
        {
            public string traffic_type { get; set; }
            public string s_location { get; set; }
            public string e_location { get; set; }
            public string s_address { get; set; }
            public string e_address { get; set; }
            public string s_date { get; set; }
            public string e_date { get; set; }
            public string s_time { get; set; }
            public string e_time { get; set; }
        }
        public OrdertrafficCar car { get; set; }

        //
        public class OrderTrafficQty
        {
            public string traffic_type { get; set; }
            public string CarPsg_adult { get; set; }
            public string CarPsg_child { get; set; }
            public string CarPsg_infant { get; set; }
            public string SafetySeat_sup_child { get; set; }
            public string SafetySeat_sup_infant { get; set; }
            public string SafetySeat_self_child { get; set; }
            public string SafetySeat_self_infant { get; set; }
            public string Luggage_carry { get; set; }
            public string Luggage_check { get; set; }
        }
        public OrderTrafficQty qty { get; set; }

        //
        public class OrderTrafficFlight
        {
            public string traffic_type { get; set; }
            public string airport { get; set; }
            public string arrival_flightType { get; set; }
            public string arrival_airlineName { get; set; }
            public string arrival_flightNo { get; set; }
            public string arrival_terminalNo { get; set; }
            public string arrival_visa { get; set; }
            public string arrival_date { get; set; }
            public string arrival_time { get; set; }
            public string departure_flightType { get; set; }
            public string departure_airlineName { get; set; }
            public string departure_flightNo { get; set; }
            public string departure_terminalNo { get; set; }
            public string departure_date { get; set; }
            public string departure_time { get; set; }
        }
        public OrderTrafficFlight flight { get; set; } 
    }

    public class OrderMobileDevice
    {
        public string mobile_model_no { get; set; } 
        public string IMEI { get; set; } 
        public string active_date { get; set; } 
    }

    public class OrderSkuModel
    {
        public string sku_id { get; set; }
        public Dictionary<string, string> spec { get; set; }
        public int qty { get; set; }
        public double? price { get; set; }
    }
}
