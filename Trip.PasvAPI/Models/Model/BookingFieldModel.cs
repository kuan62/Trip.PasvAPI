﻿using System;
using System.Collections.Generic;
using Trip.PasvAPI.AppCode;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Trip.PasvAPI.Models.Model
{
    public class BookingFieldModel
    {
        [JsonProperty("custom")]
        public Custom Custom { get; set; }

        [JsonProperty("traffic")]
        public Traffic Traffic { get; set; }

        [JsonProperty("mobile_device")]
        public MobileDevice MobileDevice { get; set; }

        [JsonProperty("ref_pkg")]
        public Dictionary<string, RefPkgAttribute> RefPkg { get; set; }
    }

    //////////////

    #region Custom --- start

    public partial class CustomAttribute
    {
        [JsonProperty("is_require")]
        public string IsRequire { get; set; }

        [JsonProperty("is_visible")]
        public string IsVisible { get; set; }

        [JsonProperty("is_send_used")]
        public string IsSendUsed { get; set; }

        [JsonProperty("is_perParticipant")]
        public string IsPerParticipant { get; set; }

        [JsonProperty("is_lead_used")]
        public string IsLeadUsed { get; set; }

        [JsonProperty("is_contact_used")]
        public string IsContactUsed { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }

    public partial class Custom
    {
        [JsonProperty("cus_type")]
        public CusType CusType { get; set; }

        [JsonProperty("english_last_name")]
        public CustomAttribute EnglishLastName { get; set; }

        [JsonProperty("english_first_name")]
        public CustomAttribute EnglishFirstName { get; set; }

        [JsonProperty("native_last_name")]
        public CustomAttribute NativeLastName { get; set; }

        [JsonProperty("native_first_name")]
        public CustomAttribute NativeFirstName { get; set; }

        [JsonProperty("tel_country_code")]
        public TelCountryCode TelCountryCode { get; set; }

        [JsonProperty("tel_number")]
        public CustomAttribute TelNumber { get; set; }

        [JsonProperty("gender")]
        public Gender Gender { get; set; }

        [JsonProperty("contact_app")]
        public ContactApp ContactApp { get; set; }

        [JsonProperty("contact_app_account")]
        public CustomAttribute ContactAppAccount { get; set; }

        [JsonProperty("country_cities")]
        public CountryCities CountryCities { get; set; }

        [JsonProperty("zipcode")]
        public CustomAttribute Zipcode { get; set; }

        [JsonProperty("address")]
        public CustomAttribute Address { get; set; }

        [JsonProperty("nationality")]
        public Nationality Nationality { get; set; }

        [JsonProperty("mtp_no")]
        public CustomAttribute MtpNo { get; set; }

        [JsonProperty("id_no")]
        public CustomAttribute IdNo { get; set; }

        [JsonProperty("passport_no")]
        public CustomAttribute PassportNo { get; set; }

        [JsonProperty("passport_expdate")]
        public CustomAttribute PassportExpdate { get; set; }

        [JsonProperty("birth")]
        public CustomAttribute Birth { get; set; }

        [JsonProperty("height")]
        public CustomAttribute Height { get; set; }

        [JsonProperty("height_unit")]
        public HeightUnit HeightUnit { get; set; }

        [JsonProperty("weight")]
        public CustomAttribute Weight { get; set; }

        [JsonProperty("weight_unit")]
        public WeightUnit WeightUnit { get; set; }

        [JsonProperty("shoe")]
        public Shoe Shoe { get; set; }

        [JsonProperty("shoe_unit")]
        public ShoeUnit ShoeUnit { get; set; }

        [JsonProperty("shoe_type")]
        public ShoeType ShoeType { get; set; }

        [JsonProperty("glass_degree")]
        public GlassDegree GlassDegree { get; set; }

        [JsonProperty("meal")]
        public Meal Meal { get; set; }

        [JsonProperty("allergy_food")]
        public CustomAttribute AllergyFood { get; set; }

        [JsonProperty("guide_lang")]
        public GuideLang GuideLang { get; set; }
    }

    public partial class CusType : CustomAttribute
    {
        [JsonProperty("list_option", NullValueHandling = NullValueHandling.Ignore)]
        public string[] ListOption { get; set; }
    }

    public partial class TelCountryCode : CustomAttribute
    {
        [JsonProperty("list_option", NullValueHandling = NullValueHandling.Ignore)]
        public List<CodeInfo> ListOption { get; set; }
    }

    public partial class CodeInfo
    {
        [JsonProperty("code", NullValueHandling = NullValueHandling.Ignore)]
        public string Code { get; set; }

        [JsonProperty("info", NullValueHandling = NullValueHandling.Ignore)]
        public string Info { get; set; }
    }

    public partial class Gender : CustomAttribute
    {
        [JsonProperty("list_option", NullValueHandling = NullValueHandling.Ignore)]
        public List<IdName> ListOption { get; set; }
    }

    public partial class ContactApp : CustomAttribute
    {
        [JsonProperty("list_option", NullValueHandling = NullValueHandling.Ignore)]
        public List<ContactAppSupport> ListOption { get; set; }
    }

    public partial class HeightUnit : CustomAttribute
    {
        [JsonProperty("list_option", NullValueHandling = NullValueHandling.Ignore)]
        public List<IdName> ListOption { get; set; }
    }

    public partial class Nationality : CustomAttribute
    {
        [JsonProperty("list_option", NullValueHandling = NullValueHandling.Ignore)]
        public List<CodeNameModel> ListOption { get; set; }
    }

    public partial class CountryCities : CustomAttribute
    {
        [JsonProperty("list_option", NullValueHandling = NullValueHandling.Ignore)]
        public List<CountryCity> ListOption { get; set; }
    }

    public partial class WeightUnit : CustomAttribute
    {
        [JsonProperty("list_option", NullValueHandling = NullValueHandling.Ignore)]
        public List<IdName> ListOption { get; set; }
    }

    public partial class Shoe : CustomAttribute
    {
        [JsonProperty("list_option", NullValueHandling = NullValueHandling.Ignore)]
        public List<Dictionary<string, List<ShoeUnitInfo>>> ListOption { get; set; }
    }
    public partial class ShoeUnit : CustomAttribute
    {
        [JsonProperty("list_option", NullValueHandling = NullValueHandling.Ignore)]
        public List<IdName> ListOption { get; set; }
    }

    public partial class ShoeType : CustomAttribute
    {
        [JsonProperty("list_option", NullValueHandling = NullValueHandling.Ignore)]
        public List<IdName> ListOption { get; set; }
    }

    public partial class GlassDegree : CustomAttribute
    {
        [JsonProperty("list_option", NullValueHandling = NullValueHandling.Ignore)]
        public List<Dictionary<string, string>> ListOption { get; set; }
    }

    public partial class Meal : CustomAttribute
    {
        [JsonProperty("list_option", NullValueHandling = NullValueHandling.Ignore)]
        public List<MealTypeInfo> ListOption { get; set; }
    }

    public partial class GuideLang : CustomAttribute
    {
        [JsonProperty("list_option", NullValueHandling = NullValueHandling.Ignore)]
        public List<CodeNameModel> ListOption { get; set; }
    }

    public partial class IdName
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public partial class ContactAppSupport
    {
        [JsonProperty("supported", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Supported { get; set; }

        [JsonProperty("app_type")]
        public string AppType { get; set; }

        [JsonProperty("app_name")]
        public string AppName { get; set; }
    }

    public partial class CountryCity : IdName
    {
        [JsonProperty("cities", NullValueHandling = NullValueHandling.Ignore)]
        public List<CityCodeName> cities { get; set; }
    }

    public partial class CityCodeName
    {
        [JsonProperty("city_code")]
        public string CityCode { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public partial class ShoeUnitInfo
    {
        [JsonProperty("unit_code")]
        public string UnitCode { get; set; }

        [JsonProperty("unit_name")]
        public string UnitName { get; set; }

        [JsonProperty("size_range_start")]
        public string SizeRangeStart { get; set; }

        [JsonProperty("size_range_end")]
        public string SizeRangeEnd { get; set; }
    }

    public partial class MealTypeInfo
    {
        [JsonProperty("is_provided", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsProvided { get; set; }

        [JsonProperty("meal_type")]
        public string MealType { get; set; }

        [JsonProperty("meal_type_name")]
        public string MealTypeName { get; set; }
    }

    #endregion Custom --- end

    //////////////

    #region Traffic --- start

    public partial class Traffic
    {
        [JsonProperty("car")]
        public Car Car { get; set; }

        [JsonProperty("qty")]
        public Qty Qty { get; set; }

        [JsonProperty("flight")]
        public Flight Flight { get; set; }
    }

    public partial class TrafficAttribute
    {
        [JsonProperty("is_require")]
        public string IsRequire { get; set; }

        [JsonProperty("is_visible")]
        public string IsVisible { get; set; }

        [JsonProperty("is_pickup_used")]
        public string IsPickupUsed { get; set; }

        [JsonProperty("is_rentcar_used")]
        public string IsRentcarUsed { get; set; }

        [JsonProperty("is_voucher_used")]
        public string IsVoucherUsed { get; set; }

        [JsonProperty("is_cus_used")]
        public string IsCusUsed { get; set; }

        [JsonProperty("ref_source")]
        public string RefSource { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }

    public partial class Car
    {
        [JsonProperty("traffic_type")]
        public TrafficType TrafficType { get; set; }

        [JsonProperty("location_list")]
        public LocationList LocationList { get; set; }

        [JsonProperty("s_location")]
        public TrafficAttribute SLocation { get; set; }

        [JsonProperty("s_date")]
        public TrafficAttribute SDate { get; set; }

        [JsonProperty("s_time")]
        public TrafficAttribute STime { get; set; }

        [JsonProperty("e_location")]
        public TrafficAttribute ELocation { get; set; }

        [JsonProperty("e_date")]
        public TrafficAttribute EDate { get; set; }

        [JsonProperty("e_time")]
        public TrafficAttribute ETime { get; set; }

        [JsonProperty("provide_wifi")]
        public TrafficAttribute ProvideWifi { get; set; }

        [JsonProperty("provide_gps")]
        public TrafficAttribute ProvideGps { get; set; }
    }

    public partial class TrafficType : TrafficAttribute
    {
        [JsonProperty("list_option", NullValueHandling = NullValueHandling.Ignore)]
        public string[] ListOption { get; set; }
    }

    public partial class LocationList : TrafficAttribute
    {
        [JsonProperty("list_option")]
        public List<LocationListOption> ListOption { get; set; }
    }

    public partial class LocationListOption
    {
        [JsonProperty("traffic_type")]
        public string TrafficType { get; set; }

        [JsonProperty("sort", NullValueHandling = NullValueHandling.Ignore)]
        public long? Sort { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("address", NullValueHandling = NullValueHandling.Ignore)]
        public string Address { get; set; }

        [JsonProperty("s_time", NullValueHandling = NullValueHandling.Ignore)]
        public string STime { get; set; }

        [JsonProperty("e_time", NullValueHandling = NullValueHandling.Ignore)]
        public string ETime { get; set; }

        [JsonProperty("hour", NullValueHandling = NullValueHandling.Ignore)]
        public string Hour { get; set; }

        [JsonProperty("min", NullValueHandling = NullValueHandling.Ignore)]
        public string Min { get; set; }

        [JsonProperty("routeEng", NullValueHandling = NullValueHandling.Ignore)]
        public string RouteEng { get; set; }

        [JsonProperty("routeLocal", NullValueHandling = NullValueHandling.Ignore)]
        public string RouteLocal { get; set; }

        [JsonProperty("code", NullValueHandling = NullValueHandling.Ignore)]
        public string Code { get; set; }

        [JsonProperty("interval", NullValueHandling = NullValueHandling.Ignore)]
        public long? Interval { get; set; }

        [JsonProperty("note", NullValueHandling = NullValueHandling.Ignore)]
        public string Note { get; set; }

        [JsonProperty("time_list", NullValueHandling = NullValueHandling.Ignore)]
        public TimeList[] TimeList { get; set; }
    }

    public partial class TimeList
    {
        [JsonProperty("weekDays")]
        public string WeekDays { get; set; }

        [JsonProperty("from")]
        public TimeInfo From { get; set; }

        [JsonProperty("to")]
        public TimeInfo To { get; set; }
    }

    public partial class TimeInfo
    {
        [JsonProperty("hour")]
        public long Hour { get; set; }

        [JsonProperty("minute")]
        public long Minute { get; set; }
    }


    #endregion Traffic --- end

    //////////////

    #region Flight --- start

    public partial class Flight
    {
        [JsonProperty("traffic_type")]
        public FlightTrafficType TrafficType { get; set; }

        [JsonProperty("airport_list")]
        public AirportList AirportList { get; set; }

        [JsonProperty("arrival_airport")]
        public ArrivalAirport ArrivalAirport { get; set; }

        [JsonProperty("arrival_flightType")]
        public ArrivalFlightType ArrivalFlightType { get; set; }

        [JsonProperty("arrival_airlineName")]
        public FlightAttribute ArrivalAirlineName { get; set; }

        [JsonProperty("arrival_flightNo")]
        public FlightAttribute ArrivalFlightNo { get; set; }

        [JsonProperty("arrival_terminalNo")]
        public FlightAttribute ArrivalTerminalNo { get; set; }

        [JsonProperty("arrival_visa")]
        public FlightAttribute ArrivalVisa { get; set; }

        [JsonProperty("arrival_date")]
        public FlightAttribute ArrivalDate { get; set; }

        [JsonProperty("arrival_time")]
        public FlightAttribute ArrivalTime { get; set; }

        [JsonProperty("departure_airport")]
        public DepartureAirport DepartureAirport { get; set; }

        [JsonProperty("departure_flightType")]
        public DepartureFlightType DepartureFlightType { get; set; }

        [JsonProperty("departure_airlineName")]
        public FlightAttribute DepartureAirlineName { get; set; }

        [JsonProperty("departure_flightNo")]
        public FlightAttribute DepartureFlightNo { get; set; }

        [JsonProperty("departure_terminalNo")]
        public FlightAttribute DepartureTerminalNo { get; set; }

        [JsonProperty("departure_date")]
        public FlightAttribute DepartureDate { get; set; }

        [JsonProperty("departure_time")]
        public FlightAttribute DepartureTime { get; set; }

        [JsonProperty("arrival_pkg_no", NullValueHandling = NullValueHandling.Ignore)]
        public List<Int64> ArrivalPkgNo { get; set; }

        [JsonProperty("departure_pkg_no", NullValueHandling = NullValueHandling.Ignore)]
        public List<Int64> DeparturePkgNo { get; set; }
    }

    public partial class FlightAttribute
    {
        [JsonProperty("is_require")]
        public string IsRequire { get; set; }

        [JsonProperty("is_visible")]
        public string IsVisible { get; set; }

        [JsonProperty("is_flight_used")]
        public string IsFlightUsed { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }

    public partial class FlightTrafficType : FlightAttribute
    {
        [JsonProperty("list_option", NullValueHandling = NullValueHandling.Ignore)]
        public string[] ListOption { get; set; }
    }

    public partial class AirportList : FlightAttribute
    {
        [JsonProperty("list_option", NullValueHandling = NullValueHandling.Ignore)]
        public AirportListListOption[] ListOption { get; set; }
    }

    public partial class AirportListListOption
    {
        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("area")]
        public string Area { get; set; }
    }

    public partial class ArrivalFlightType : FlightAttribute
    {
        [JsonProperty("list_option", NullValueHandling = NullValueHandling.Ignore)]
        public List<IdName> ListOption { get; set; }
    }

    public partial class ArrivalAirport : FlightAttribute
    {
        [JsonProperty("ref_source")]
        public string RefSource { get; set; }
    }

    public partial class DepartureAirport : FlightAttribute
    {
        [JsonProperty("ref_source")]
        public string RefSource { get; set; }
    }

    public partial class DepartureFlightType : FlightAttribute
    {
        [JsonProperty("list_option", NullValueHandling = NullValueHandling.Ignore)]
        public List<IdName> ListOption { get; set; }
    }

    public partial class Qty
    {
        [JsonProperty("traffic_type")]
        public FlightTrafficType TrafficType { get; set; }

        [JsonProperty("CarPsg_adult")]
        public FlightAttribute CarPsgAdult { get; set; }

        [JsonProperty("CarPsg_child")]
        public FlightAttribute CarPsgChild { get; set; }

        [JsonProperty("CarPsg_infant")]
        public FlightAttribute CarPsgInfant { get; set; }

        [JsonProperty("SafetySeat_sup_child")]
        public FlightAttribute SafetySeatSupChild { get; set; }

        [JsonProperty("SafetySeat_sup_infant")]
        public FlightAttribute SafetySeatSupInfant { get; set; }

        [JsonProperty("SafetySeat_self_child")]
        public FlightAttribute SafetySeatSelfChild { get; set; }

        [JsonProperty("SafetySeat_self_infant")]
        public FlightAttribute SafetySeatSelfInfant { get; set; }

        [JsonProperty("Luggage_carry")]
        public FlightAttribute LuggageCarry { get; set; }

        [JsonProperty("Luggage_check")]
        public FlightAttribute LuggageCheck { get; set; }
    }

    #endregion Flight --- end

    //////////////

    #region Mobile --- start

    public partial class MobileDevice
    {
        [JsonProperty("mobile_model_no")]
        public MobileAttribute MobileModelNo { get; set; }

        [JsonProperty("IMEI")]
        public MobileAttribute Imei { get; set; }

        [JsonProperty("active_date")]
        public MobileAttribute ActiveDate { get; set; }
    }

    public partial class MobileAttribute
    {
        [JsonProperty("is_require")]
        public string IsRequire { get; set; }

        [JsonProperty("is_visible")]
        public string IsVisible { get; set; }

        [JsonProperty("is_mobile_used")]
        public string IsMobileUsed { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }

    #endregion Mobile --- end

    //////////////

    #region Reference Package --- start

    public partial class RefPkgAttribute
    {
        public RefPkgCustomer customer { get; set; }
        public RefPkgTraffic traffic { get; set; }
    }

    public partial class RefPkgCustomer
    {
        public List<string> cus_type { get; set; }
        public string address { get; set; }
        public string country_cities { get; set; }
    }

    public partial class RefPkgTraffic
    {
        public List<string> traffic_type { get; set; }
        public string s_date { get; set; }
        public string s_time { get; set; }
        public string s_location { get; set; }
        public string e_date { get; set; }
        public string e_time { get; set; }
        public string e_location { get; set; }
        public string departure_flight { get; set; }
        public string arrival_flight { get; set; }
    }

    #endregion  Reference Package  --- end

    //////////////
}
