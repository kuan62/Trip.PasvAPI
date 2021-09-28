using System;
using System.Collections.Generic;

namespace Trip.PasvAPI.Models.Model
{ 

    /// <summary>
    /// 查詢pmch的 request
    /// </summary>
    public class PmgwModel
    {
        public string prod_no { get; set; }//對應prodOid
        public string s_date { get; set; }//對應goSdate
        public string e_date { get; set; }//對應goEdate
        public string country_code { get; set; }
        public string locale { get; set; }//對應lang
        public string state { get; set; }//對應市場
        public string prod_type { get; set; }//對應main_cat 從ConfirmList取
        public string currency { get; set; }
        public string is_tourism { get; set; }//從ConfirmList取
        public string sale_state { get; set; }//從ConfirmList取
        public string forbidden { get; set; }//從ConfirmList取
        public string browser { get; set; }//瀏覽器資訊
        public string os_name { get; set; }//作業系統
        public string comp_xid { get; set; }
        public string source_code { get; set; } //區分FINEDAY -BENZ

        #region 滿足confirmList
        public string guid { get; set; }//取得價格資訊的guidKey從queryPackage取得
        public string price_token { get; set; }//由系統取得guid
        public string pkg_no { get; set; }
        public string item_no { get; set; }
        public List<ConfirmSku> sku { get; set; }
        public double? total_price { get; set; }//訂單總價
        public string event_time { get; set; }

        #endregion

    }

    public class ConfirmSku
    {
        public string sku_id { get; set; }
        public Dictionary<string, string> spec { get; set; }//語系spec
        public int qty { get; set; }
        public double? price { get; set; }//精準度以currency的小數點為主
        public double usd_price { get; set; }
        public double gross_rate { get; set; }//利潤以美金
    }

    /// <summary>
    /// 查詢pmch的 response
    /// </summary>
    public class PmchList : PmchLstResponse
    {
        public string result { get; set; }
        public string result_msg { get; set; }
        public string price_token { get; set; }//吐回confirmList的guidKey

    }


    public class PmchLstResponse
    {
        public pmchData data { get; set; }
    }


    public class pmchData
    {
        public List<Pmgw> pmch_list { get; set; }
    }

    public class Pmgw
    {
        public string pmch_oid { get; set; }
        public string pmch_code { get; set; }
        public string pmch_pay_url { get; set; }
        public string is_3d { get; set; }
        public string acctdoc_receive_method { get; set; }
        public string version { get; set; }
        public InterfaceSetting interface_setting { get; set; }
        public pmgwSetting pmgw_setting { get; set; }
    }

    public class InterfaceSetting
    {
        public string is_need_card_input { get; set; }
        public List<LogoList> logo_list { get; set; }
        public List<string> accepted_card_type_list { get; set; }
        public string accepted_currency { get; set; }
        public List<string> other_info_list { get; set; }
    }

    public class LogoList
    {
        public string logoName { get; set; }
        public string logoUrl { get; set; }
        public List<string> accepted_card_type_list { get; set; }
    }

    public class pmgwSetting
    {
        //adyen會呈現的模式
        public string account { get; set; }
        public string api_key { get; set; }
        public string is_components { get; set; }
        public Dictionary<string, string> origin_keys { get; set; }

        //無adyen會呈現的模式
        public string merId { get; set; }
        public string merchantId { get; set; }
        public string terminalId { get; set; }
        public string mpiUrl { get; set; }

        public string publishable_key { get; set; }
        public string secret_key { get; set; }

        //tappay會呈現的模式 (只抓要使用的欄位 app_key & app_id)
        public string app_id { get; set; }
        public string app_key { get; set; }
        public string partner_key { get; set; }
    }

    public class CurrencyModel
    {
        public string result { get; set; }
        public string result_msg { get; set; }
        public List<currencies> currencies { get; set; }
    }

    public class currencies
    {
        public string code { get; set; }
        public double rate { get; set; }
        public int precision { get; set; }
        public string roundingMethod { get; set; }
    }

    public class QueryRSAmountModel
    {
        public string result { get; set; }
        public string result_msg { get; set; }
        public double? now_amount { get; set; }
        public List<AmountDtlModel> Amounts { get; set; }
    }
    public class AmountDtlModel
    {
        public string result { get; set; }
        public string result_msg { get; set; }
        public int? xid { get; set; }
        public int? mst_xid { get; set; }
        public int? parent_xid { get; set; }
        public bool? is_valid { get; set; }
        public double? amt { get; set; }
        public double? safety_amt { get; set; }
        public bool? is_pay { get; set; }
        public double? sub_amt { get; set; }
        public double? prcp_amt { get; set; }
        public string source { get; set; }
        public string crt_user { get; set; }
        public string crt_datetime { get; set; }
        public string upd_user { get; set; }
        public string upd_datetime { get; set; }
        public string note { get; set; }
    }


    public class stripeMetadataMst
    {
        public string currency { get; set; }
        public string amount { get; set; }
        public stringMetadataDtl metadata { get; set; }
    }

    public class stringMetadataDtl
    {
        public string email { get; set; }
        public string lang { get; set; }
        public string country_cd { get; set; }
        public string member_uuid { get; set; }
        //public string product_id { get; set; }
        public string pmch_oid { get; set; }
        public string platform { get; set; } = "B2D";
    }


    public class returnStatus
    {
        public string status { get; set; }
        public string jsonStr { get; set; }
        public string msgErr { get; set; }
        public string url { get; set; }
        //public PmchSslRequest pmchSslRequest { get; set; }
        public string paymentType { get; set; }              // 人工額度="arType"
        public PmchSslRequestCar pmchSslRequest { get; set; }
        public string pmchSslRequestEncryption { get; set; }  //stripe 需要加密傳送

    }

    public class PmchSslRequestCar//2019.09購物車
    {
        public string lang_code { get; set; } //新版的有語言環境
        public CallJson json { get; set; }
    }

    public abstract class CallJson
    {

    }

    //新版
    public class CallJsonPay : CallJson
    {
        public string pmch_oid { get; set; }
        public string is_3d { get; set; }
        public string pay_currency { get; set; }
        public double pay_amount { get; set; }
        public string return_url { get; set; }
        public string cancel_url { get; set; }
        public string user_locale { get; set; }
        public string logo_url { get; set; }
        public string payment_param1 { get; set; }
        public paymentParam2 payment_param2 { get; set; }
        public payment_source_info payment_source_info { get; set; }
        public credit_card_info credit_card_info { get; set; }
        public payer_info payer_info { get; set; }
        public List<product_info> items { get; set; }//原product_info 2019.09更改
        public member member { get; set; }

    }

    public class CallJsonPayforStripe : CallJson
    {
        public string pmch_oid { get; set; }
        public string is_3d { get; set; }
        public string pay_currency { get; set; }
        public double pay_amount { get; set; }
        public string return_url { get; set; }
        public string cancel_url { get; set; }
        public string user_locale { get; set; }
        public string logo_url { get; set; }
        public string payment_param1 { get; set; }
        public string payment_param2 { get; set; }
        public payment_source_info payment_source_info { get; set; }
        public credit_card_info credit_card_info { get; set; }
        public payer_info payer_info { get; set; }
        public List<product_info> items { get; set; }//原product_info 2019.09更改
        public member member { get; set; }

    }


    //新版stripe
    public class CallJsonPayStripe : CallJson
    {
        public string pmch_oid { get; set; }
        public string is_3d { get; set; }
        public string pay_currency { get; set; }
        public double pay_amount { get; set; }
        public string return_url { get; set; }
        public string cancel_url { get; set; }
        public string user_locale { get; set; }
        public string logo_url { get; set; }
        public string payment_param1 { get; set; }
        public string payment_param2 { get; set; }
        public payment_source_info payment_source_info { get; set; }
        public credit_card_info credit_card_info { get; set; }
        public payer_info payer_info { get; set; }
        public List<product_info> items { get; set; } //2019.09改名 原product_info
        public member member { get; set; }

    }

    //新版stripe
    public class CallJsonPayTappay : CallJson
    {
        public string pmch_oid { get; set; }
        public string is_3d { get; set; }
        public string pay_currency { get; set; }
        public double pay_amount { get; set; }
        public string return_url { get; set; }
        public string cancel_url { get; set; }
        public string user_locale { get; set; }
        public string logo_url { get; set; }
        public string payment_param1 { get; set; }
        public payment_param2 payment_param2 { get; set; }
        public payment_source_info payment_source_info { get; set; }

        public payer_info payer_info { get; set; }
        public List<product_info> items { get; set; }
        public member member { get; set; }

    }

    public class CallJsonPayTappayforBenz : CallJson
    {
        public string pmch_oid { get; set; }
        public string is_3d { get; set; }
        public string pay_currency { get; set; }
        public double pay_amount { get; set; }
        public string return_url { get; set; }
        public string cancel_url { get; set; }
        public string user_locale { get; set; }
        public string logo_url { get; set; }
        public payment_param1 payment_param1 { get; set; }
        public payment_param2 payment_param2 { get; set; }
        public payment_source_info payment_source_info { get; set; }

        public payer_info payer_info { get; set; }
        public List<product_info> items { get; set; }
        public member member { get; set; }

    }

    public class payment_param1
    {
        public string uid { get; set; }
        public string reservation_id { get; set; }
        public string order_id { get; set; }
    }

    public class payment_param2   //tappay
    {
        public string prime { get; set; }
    }

    public class PaymentSourceInfo
    {
        public string sourceType { get; set; }
        public string orderMid { get; set; }
    }

    public class CreditCardInfo
    {
        public string cardHolder { get; set; }
        public string cardType { get; set; }
        public string cardNo { get; set; }
        public string cardCvv { get; set; }
        public string cardExp { get; set; }
    }

    public class paymentParam2
    {

        public string encrypted_card_number { get; set; } //卡號 經adyen編碼後
        public string encrypted_expiry_month { get; set; } //到期月 經adyen編碼後
        public string encrypted_expiry_year { get; set; } //到期年 經adyen編碼後
        public string encrypted_security_code { get; set; } //CVV 經adyen編碼後
    }

    public class PayerInfo
    {
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string phone { get; set; }
        public string email { get; set; }

    }

    public class PayProductInfo
    {

        public string prodOid { get; set; }
        public string prodName { get; set; }
    }

    public class PayMember
    {
        public string memberUuid { get; set; }
        public string riskStatus { get; set; }

    }

    //新版
    public class payment_source_info
    {
        public string source_type { get; set; } //KKDAY (本站)CHANNEL(分銷)ONLINE_POST(線上刷卡機)
        public string order_mid { get; set; }
        public string source_ref_no { get; set; }  //order_master_mid
        public string source_code { get; set; } //訂單來源
    }

    public class payer_info //付款人資訊
    {
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string phone { get; set; }
        public string email { get; set; }
    }

    public class product_info
    {
        public string prod_oid { get; set; }
        public string prod_name { get; set; }
        public string pkg_oid { get; set; } //
        public string use_date { get; set; }
        public Int32 total_qty { get; set; }
        public List<string> kkday_product_country_code_list { get; set; }

    }

    public class member
    {
        public string member_uuid { get; set; } //會員編號
        public string risk_status { get; set; } //風險狀態代碼另外想1.白名單012.一般02 defult3.可疑03 4.黑名單04
        public string ip { get; set; }
    }

    public class credit_card_info //信用卡資訊
    {
        public string card_holder { get; set; } //卡片持有人
        public string card_type { get; set; } //卡別
        public string card_no { get; set; } //卡號(加密過的)
        public string card_cvv { get; set; } //CVV
        public string card_exp { get; set; } //到期年月yyyymm

    }



    //信用卡金流回傳欄位
    public class PmchSslResponse
    {
        public metadata metadata { get; set; }
        public Jsondata data { get; set; }
        public string order_no { get; set; }
    }

    public class metadata
    {
        public string status { get; set; }
        public string desc { get; set; }
    }

    public class Jsondata
    {
        public string pmgw_trans_no { get; set; }
        public string pmgw_method { get; set; }
        public string transaction_code { get; set; }
        public string pay_currency { get; set; }
        public decimal pay_amount { get; set; }
        public Boolean is_3d { get; set; }
        public member_info member_info { get; set; }
        public string is_fraud { get; set; }
        public string risk_note { get; set; }
        public string receive_method { get; set; }
    }

    public class member_info
    {
        public string encode_card_no { get; set; }
    }

    public class PmchValid
    {
        public string mid { get; set; }
        public PmchSslResponse jsondata { get; set; }

    }
}
