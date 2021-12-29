using System;
using System.Collections.Generic;

namespace Trip.PasvAPI.Models.Model
{
    public partial class BookingDataModel
    {
        public string guid { get; set; }
        public string partner_order_no { get; set; }
        public Int64 prod_no { get; set; }
        public string prod_name { get; set; } //新商品名稱
        public Int64 pkg_no { get; set; }
        public string pkg_name { get; set; } //pkg name
        public Int64 item_no { get; set; }
        public string s_date { get; set; } // yyyy-mm-dd
        public string e_date { get; set; } // yyyy-mm-dd
        public string event_time { get; set; } // "hh:mm"
        public List<BookingDataSkuModel> skus { get; set; }
        public string buyer_first_name { get; set; }
        public string buyer_last_name { get; set; }
        public string buyer_email { get; set; }
        public string buyer_tel_country_code { get; set; }
        public string buyer_tel_number { get; set; }
        public string buyer_country { get; set; }
        public string guide_lang { get; set; }
        public string order_note { get; set; }
        public double total_price { get; set; }
        public List<BookingDataCustomModel> custom { get; set; }
        public List<BookingDataTrafficModel> traffic { get; set; }
        public BookingDataMobileDeviceModel mobile_device { get; set; }
        public string event_backup_data { get; set; } // (ex: 1/20191124/1300, 2/20191125/1400)
        public BookingDataPaymentModel pay { get; set; }
        public double? ota_pricee { get; set; }

        //額外補充
        public string member_oid { get; set; }
        public string uid { get; set; }
        public string reservation_oid { get; set; }
        public string payment_type { get; set; }  //付款方式
        public string efr { get; set; }
        public string encryptPmchList { get; set; }
        public CardAdyen cardAdyen { get; set; }
        public CardTappay cardTappay { get; set; }
        public CardStripe cardStripe { get; set; }
        public string isPincode { get; set; }  //判斷是否存PINCODE
        public string vnd_prod_version { get; set; }
        public string identity { get; set; } //這個值來自於頁面，決定是member or cs
        public string pincode { get; set; } //
        public string atm_account { get; set; }
        public string atm_last_5_number { get; set; }
        public string isChgOrder { get; set; } //1:一般  2：賓士改單
        //額外補充--end

        public BookingDataModel Clone()
        {
            return (BookingDataModel)this.MemberwiseClone();
        }
    }

    public partial class BookingDataSkuModel
    {
        public string sku_id { get; set; }
        public double price { get; set; }
        public int qty { get; set; }
    }

    public partial class BookingDataCustomModel
    {
        public string cus_type { get; set; }
        public string english_last_name { get; set; }
        public string english_first_name { get; set; }
        public string native_last_name { get; set; }
        public string native_first_name { get; set; }
        public string tel_country_code { get; set; }
        public string tel_number { get; set; }
        public string gender { get; set; }
        public string contact_app { get; set; }
        public string contact_app_account { get; set; }
        public string country_cities { get; set; }
        public string zipcode { get; set; }
        public string address { get; set; }
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
        public string allergy_food { get; set; }
        public bool have_app { get; set; }
        public string guide_lang { get; set; }
    }

    public partial class BookingDataTrafficModel
    {
        public BookingDataTrafficCarModel car { get; set; }
        public BookingDataTrafficFlightModel flight { get; set; }
        public BookingDataTrafficQtyModel qty { get; set; } // 各旅客身分的行李, 安全座椅, 等
    }

    public partial class BookingDataTrafficCarModel
    {
        public string traffic_type { get; set; }
        public string area_code { get; set; }
        public string s_location { get; set; }
        public string e_location { get; set; }
        public string s_address { get; set; }
        public string e_address { get; set; }
        public string s_date { get; set; } // yyyy-mm-dd
        public string e_date { get; set; } // yyyy-mm-dd
        public string s_time { get; set; } // hh:mm
        public string e_time { get; set; } // hh:mm
        public bool? provide_wifi { get; set; }
        public bool? provide_gps { get; set; }
        public bool? is_rent_customize { get; set; }
    }

    public partial class BookingDataTrafficFlightModel
    {
        public string traffic_type { get; set; }
        public string arrival_airport { get; set; }
        public string arrival_flightType { get; set; }
        public string arrival_airlineName { get; set; }
        public string arrival_flightNo { get; set; }
        public string arrival_terminalNo { get; set; }
        public bool arrival_visa { get; set; }
        public string arrival_date { get; set; } // yyyy-mm-dd
        public string arrival_time { get; set; } // hh:mm
        public string departure_airport { get; set; }
        public string departure_flightType { get; set; }
        public string departure_airlineName { get; set; }
        public string departure_flightNo { get; set; }
        public string departure_terminalNo { get; set; }
        public bool departure_haveBeenInCountry { get; set; }
        public string departure_date { get; set; } // yyyy-mm-dd
        public string departure_time { get; set; } // hh:mm
    }

    public partial class BookingDataTrafficQtyModel
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

    public partial class BookingDataMobileDeviceModel
    {
        public string mobile_model_no { get; set; }
        public string IMEI { get; set; }
        public string active_date { get; set; }
    }

    public partial class BookingDataPaymentModel
    {
        public string type { get; set; }
    }
    
    public partial class CardAdyen
    {
        public string encrypted_card_number { get; set; }
        public string encrypted_expiry_month { get; set; }
        public string encrypted_expiry_year { get; set; }
        public string encrypted_security_code { get; set; }
    }

    public partial class CardTappay
    {

        public string tappayPrime { get; set; }
        public string tappayIssuer { get; set; }
    }

    public partial class CardStripe
    {
        public string stripeSourceId { get; set; }
    }
     
    public partial class ResponeObj
    {
        public string resp_code { get; set; }
        public string resp_msg { get; set; }
        public string order_no { get; set; }
        public string order_oid { get; set; }
        public string order_master_mid { get; set; }
    }

    public class inputTestObjModel
    {
        public Int64 prod_oid { get; set; }
        public Int64 pkg_oid { get; set; }
    }

}
