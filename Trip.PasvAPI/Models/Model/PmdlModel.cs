using System;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Trip.PasvAPI.AppCode;

namespace Trip.PasvAPI.Models.Model
{
    public partial class PmdlModel
    {
        [JsonProperty("PMDL_EXCHANGE")]
        public PmdlExchange PmdlExchange { get; set; }

        [JsonProperty("PMDL_NOTICE")]
        public PmdlNotice PmdlNotice { get; set; }

        [JsonProperty("PMDL_GRAPHIC")]
        public PmdlGraphic PmdlGraphic { get; set; }

        [JsonProperty("PMDL_SCHEDULE")]
        public PmdlSchedule PmdlSchedule { get; set; }

        [JsonProperty("PMDL_EXTRA_FEE")]
        public PmdlExtraFee PmdlExtraFee { get; set; }

        [JsonProperty("PMDL_NATIONALITY")]
        public PmdlNationality PmdlNationality { get; set; }

        [JsonProperty("PMDL_HOWTO_SUMMARY")]
        public PmdlSummary PmdlHowtoSummary { get; set; }

        [JsonProperty("PMDL_VENUE_LOCATION")]
        public PmdlVenueLocation PmdlVenueLocation { get; set; }

        [JsonProperty("PMDL_SUGGESTED_ROUTE")]
        public PmdlSuggestedRoute PmdlSuggestedRoute { get; set; }

        [JsonProperty("PMDL_PURCHASE_SUMMARY")]
        public PmdlSummary PmdlPurchaseSummary { get; set; }

        [JsonProperty("PMDL_EXCHANGE_LOCATION")]
        public PmdlExchangeLocation PmdlExchangeLocation { get; set; }

        [JsonProperty("PMDL_INTRODUCE_SUMMARY")]
        public PmdlSummary PmdlIntroduceSummary { get; set; }

        [JsonProperty("PMDL_EXPERIENCE_LOCATION")]
        public PmdlExperienceLocation PmdlExperienceLocation { get; set; }

        ////////

        [JsonProperty("PMDL_WIFI")]
        public PmdlWifi PmdlWifi { get; set; }

        [JsonProperty("PMDL_INC_NINC")]
        public PmdlIncNinc PmdlIncNinc { get; set; }

        [JsonProperty("PMDL_SIM_CARD")]
        public PmdlSimCard PmdlSimCard { get; set; }

        [JsonProperty("PMDL_USE_VALID")]
        public PmdlUseValid PmdlUseValid { get; set; }

        [JsonProperty("PMDL_PACKAGE_DESC")]
        public PmdlPackageDesc PmdlPackageDesc { get; set; }

        [JsonProperty("PMDL_REFUND_POLICY")]
        public PmdlRefundPolicy PmdlRefundPolicy { get; set; }

        [JsonProperty("PMDL_EXCHANGE_VALID")]
        public PmdlExchangeValid PmdlExchangeValid { get; set; }
    }

    #region PMDL for product --- start

    public partial class PmdlExchange
    {
        [JsonProperty("module_title")]
        public string ModuleTitle { get; set; }

        [JsonProperty("content")]
        public PmdlExchangeContent Content { get; set; }
    }

    public partial class PmdlExchangeContent
    {
        [JsonProperty("properties")]
        public PurpleProperties Properties { get; set; }
    }

    public partial class PurpleProperties
    {
        [JsonProperty("exchange_type")]
        public ExchangeType ExchangeType { get; set; }

        [JsonProperty("description")]
        public ContentElement Description { get; set; }
    }

    public partial class ContentElement
    {
        [JsonProperty("desc")]
        public string Desc { get; set; }
    }

    public partial class ExchangeType
    {
        [JsonProperty("desc")]
        public string Desc { get; set; }

        [JsonProperty("title", NullValueHandling = NullValueHandling.Ignore)]
        public string Title { get; set; }
    }

    public partial class PmdlExchangeLocation
    {
        [JsonProperty("module_title")]
        public string ModuleTitle { get; set; }

        [JsonProperty("content")]
        public PmdlExchangeLocationContent Content { get; set; }
    }

    public partial class PmdlExchangeLocationContent
    {
        [JsonProperty("properties")]
        public FluffyProperties Properties { get; set; }
    }

    public partial class FluffyProperties
    {
        [JsonProperty("collect_info")]
        public ExchangeType CollectInfo { get; set; }

        [JsonProperty("return_info")]
        public ExchangeType ReturnInfo { get; set; }

        [JsonProperty("locations")]
        public Locations Locations { get; set; }
    }

    public partial class Locations
    {
        [JsonProperty("list")]
        public LocationsList[] List { get; set; }

        [JsonProperty("table_key_langs")]
        public LocationsTableKeyLangs TableKeyLangs { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }
    }

    public partial class LocationsList
    {
        [JsonProperty("location_info")]
        public LocationInfo LocationInfo { get; set; }

        [JsonProperty("station_list")]
        public StationList StationList { get; set; }
    }

    public partial class LocationInfo
    {
        [JsonProperty("properties")]
        public LocationInfoProperties Properties { get; set; }
    }

    public partial class LocationInfoProperties
    {
        [JsonProperty("airport_name", NullValueHandling = NullValueHandling.Ignore)]
        public ExchangeType AirportName { get; set; }

        [JsonProperty("terminal", NullValueHandling = NullValueHandling.Ignore)]
        public ExchangeType Terminal { get; set; }

        [JsonProperty("store_name", NullValueHandling = NullValueHandling.Ignore)]
        public ExchangeType StoreName { get; set; }

        [JsonProperty("latlng", NullValueHandling = NullValueHandling.Ignore)]
        public Latlng Latlng { get; set; }
    }

    public partial class Latlng
    {
        [JsonProperty("latitude")]
        public string Latitude { get; set; }

        [JsonProperty("longitude")]
        public string Longitude { get; set; }

        [JsonProperty("map_snap_url")]
        public string MapSnapUrl { get; set; }

        [JsonProperty("zoom_lv")]
        [JsonConverter(typeof(PmdlParseStringConverter))]
        public long ZoomLv { get; set; }

        [JsonProperty("desc")]
        public string Desc { get; set; }

        [JsonProperty("title", NullValueHandling = NullValueHandling.Ignore)]
        public string Title { get; set; }

        [JsonProperty("place_id", NullValueHandling = NullValueHandling.Ignore)]
        public string PlaceId { get; set; }
    }

    public partial class StationList
    {
        [JsonProperty("list")]
        public StationListList[] List { get; set; }

        [JsonProperty("table_key_langs")]
        public StationListTableKeyLangs TableKeyLangs { get; set; }
    }

    public partial class StationListList
    {
        [JsonProperty("station_desc", NullValueHandling = NullValueHandling.Ignore)]
        public ContentElement StationDesc { get; set; }

        [JsonProperty("provide_service")]
        public ContentElement ProvideService { get; set; }

        [JsonProperty("active_time", NullValueHandling = NullValueHandling.Ignore)]
        public ActiveTime ActiveTime { get; set; }

        [JsonProperty("photo")]
        public Video Photo { get; set; }

        [JsonProperty("video", NullValueHandling = NullValueHandling.Ignore)]
        public Video Video { get; set; }

        [JsonProperty("active_time_desc", NullValueHandling = NullValueHandling.Ignore)]
        public ContentElement ActiveTimeDesc { get; set; }

        [JsonProperty("arrival_desc", NullValueHandling = NullValueHandling.Ignore)]
        public ContentElement ArrivalDesc { get; set; }

        [JsonProperty("active_tel_desc", NullValueHandling = NullValueHandling.Ignore)]
        public ContentElement ActiveTelDesc { get; set; }
    }

    public partial class ActiveTime
    {
        [JsonProperty("list")]
        public ActiveTimeList[] List { get; set; }

        [JsonProperty("table_key_langs")]
        public ActiveTimeTableKeyLangs TableKeyLangs { get; set; }
    }

    public partial class ActiveTimeList
    {
        [JsonProperty("week_title")]
        public ContentElement WeekTitle { get; set; }

        [JsonProperty("close_desc", NullValueHandling = NullValueHandling.Ignore)]
        public ContentElement CloseDesc { get; set; }

        [JsonProperty("start_time", NullValueHandling = NullValueHandling.Ignore)]
        public ContentElement StartTime { get; set; }

        [JsonProperty("end_time", NullValueHandling = NullValueHandling.Ignore)]
        public ContentElement EndTime { get; set; }

        [JsonProperty("last_admission", NullValueHandling = NullValueHandling.Ignore)]
        public ContentElement LastAdmission { get; set; }
    }

    public partial class ActiveTimeTableKeyLangs
    {
        [JsonProperty("week_title")]
        public string WeekTitle { get; set; }

        [JsonProperty("start_time")]
        public string StartTime { get; set; }

        [JsonProperty("end_time")]
        public string EndTime { get; set; }

        [JsonProperty("close_desc")]
        public string CloseDesc { get; set; }

        [JsonProperty("last_admission", NullValueHandling = NullValueHandling.Ignore)]
        public string LastAdmission { get; set; }
    }

    public partial class Video
    {
        [JsonProperty("media")]
        public Media[] Media { get; set; }
    }

    public partial class Media
    {
        [JsonProperty("source_content")]
        public string SourceContent { get; set; }
    }

    public partial class StationListTableKeyLangs
    {
        [JsonProperty("station_desc")]
        public string StationDesc { get; set; }

        [JsonProperty("provide_service")]
        public string ProvideService { get; set; }

        [JsonProperty("arrival_desc")]
        public string ArrivalDesc { get; set; }

        [JsonProperty("photo")]
        public string Photo { get; set; }

        [JsonProperty("video")]
        public string Video { get; set; }

        [JsonProperty("active_time")]
        public string ActiveTime { get; set; }

        [JsonProperty("active_time_desc")]
        public string ActiveTimeDesc { get; set; }

        [JsonProperty("active_tel_desc")]
        public string ActiveTelDesc { get; set; }
    }

    public partial class LocationsTableKeyLangs
    {
        [JsonProperty("city_info")]
        public string CityInfo { get; set; }

        [JsonProperty("location_info")]
        public string LocationInfo { get; set; }

        [JsonProperty("station_list")]
        public string StationList { get; set; }
    }

    public partial class PmdlExperienceLocation
    {
        [JsonProperty("module_title")]
        public string ModuleTitle { get; set; }

        [JsonProperty("content")]
        public PmdlExperienceLocationContent Content { get; set; }
    }

    public partial class PmdlExperienceLocationContent
    {
        [JsonProperty("list")]
        public PurpleList[] List { get; set; }

        [JsonProperty("table_key_langs")]
        public PurpleTableKeyLangs TableKeyLangs { get; set; }
    }

    public partial class PurpleList
    {
        [JsonProperty("active_time_desc")]
        public ContentElement ActiveTimeDesc { get; set; }

        [JsonProperty("arrival_desc")]
        public ContentElement ArrivalDesc { get; set; }

        [JsonProperty("active_time")]
        public ActiveTime ActiveTime { get; set; }

        [JsonProperty("location_name")]
        public ContentElement LocationName { get; set; }

        [JsonProperty("latlng")]
        public Latlng Latlng { get; set; }

        [JsonProperty("video", NullValueHandling = NullValueHandling.Ignore)]
        public Video Video { get; set; }
    }

    public partial class PurpleTableKeyLangs
    {
        [JsonProperty("location_name")]
        public string LocationName { get; set; }

        [JsonProperty("latlng")]
        public string Latlng { get; set; }

        [JsonProperty("photo")]
        public string Photo { get; set; }

        [JsonProperty("video")]
        public string Video { get; set; }

        [JsonProperty("active_time")]
        public string ActiveTime { get; set; }

        [JsonProperty("active_time_desc")]
        public string ActiveTimeDesc { get; set; }

        [JsonProperty("arrival_desc")]
        public string ArrivalDesc { get; set; }
    }

    public partial class PmdlExtraFee
    {
        [JsonProperty("module_title")]
        public string ModuleTitle { get; set; }

        [JsonProperty("content")]
        public PmdlExtraFeeContent Content { get; set; }
    }

    public partial class PmdlExtraFeeContent
    {
        [JsonProperty("list")]
        public ExchangeType[] List { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }
    }

    public partial class PmdlGraphic
    {
        [JsonProperty("module_title")]
        public string ModuleTitle { get; set; }

        [JsonProperty("content")]
        public PmdlGraphicContent Content { get; set; }
    }

    public partial class PmdlGraphicContent
    {
        [JsonProperty("list")]
        public FluffyList[] List { get; set; }
    }

    public partial class FluffyList
    {
        [JsonProperty("media", NullValueHandling = NullValueHandling.Ignore)]
        public Media[] Media { get; set; }

        [JsonProperty("desc")]
        public string Desc { get; set; }
    }

    public partial class PmdlSummary
    {
        [JsonProperty("module_title")]
        public string ModuleTitle { get; set; }

        [JsonProperty("content")]
        public ContentElement Content { get; set; }
    }

    public partial class PmdlNationality
    {
        [JsonProperty("module_title")]
        public string ModuleTitle { get; set; }

        [JsonProperty("content")]
        public PmdlNationalityContent Content { get; set; }
    }

    public partial class PmdlNationalityContent
    {
        [JsonProperty("properties")]
        public TentacledProperties Properties { get; set; }
    }

    public partial class TentacledProperties
    {
        [JsonProperty("nationality_allow")]
        public ExchangeType NationalityAllow { get; set; }

        [JsonProperty("notes")]
        public Notes Notes { get; set; }
    }

    public partial class Notes
    {
        [JsonProperty("list")]
        public ContentElement[] List { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }
    }

    public partial class PmdlNotice
    {
        [JsonProperty("module_title")]
        public string ModuleTitle { get; set; }

        [JsonProperty("content")]
        public PmdlNoticeContent Content { get; set; }
    }

    public partial class PmdlNoticeContent
    {
        [JsonProperty("properties")]
        public StickyProperties Properties { get; set; }
    }

    public partial class StickyProperties
    {
        [JsonProperty("reminds")]
        public Reminds Reminds { get; set; }

        [JsonProperty("cust_reminds")]
        public CustRemindsClass CustReminds { get; set; }

        [JsonProperty("cust_reminds_after")]
        public Notes CustRemindsAfter { get; set; }
    }

    public partial class CustRemindsClass
    {
        [JsonProperty("list")]
        public ContentElement[] List { get; set; }
    }

    public partial class Reminds
    {
        [JsonProperty("list")]
        public ExchangeType[] List { get; set; }
    }

    public partial class PmdlSchedule
    {
        [JsonProperty("module_title")]
        public string ModuleTitle { get; set; }

        [JsonProperty("content")]
        public PmdlScheduleContent Content { get; set; }
    }

    public partial class PmdlScheduleContent
    {
        [JsonProperty("properties")]
        public IndigoProperties Properties { get; set; }
    }

    public partial class IndigoProperties
    {
        [JsonProperty("total_day")]
        public ExchangeType TotalDay { get; set; }

        [JsonProperty("schedule_list")]
        public ScheduleList ScheduleList { get; set; }
    }

    public partial class ScheduleList
    {
        [JsonProperty("list")]
        public ScheduleListList[] List { get; set; }

        [JsonProperty("table_key_langs")]
        public ScheduleListTableKeyLangs TableKeyLangs { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }
    }

    public partial class ScheduleListList
    {
        [JsonProperty("daily_title")]
        public ContentElement DailyTitle { get; set; }

        [JsonProperty("meals")]
        public CustRemindsClass Meals { get; set; }

        [JsonProperty("daily_schedule_list")]
        public DailyScheduleList DailyScheduleList { get; set; }
    }

    public partial class DailyScheduleList
    {
        [JsonProperty("table_key_langs")]
        public DailyScheduleListTableKeyLangs TableKeyLangs { get; set; }

        [JsonProperty("list")]
        public DailyScheduleListList[] List { get; set; }
    }

    public partial class DailyScheduleListList
    {
        [JsonProperty("time")]
        public ContentElement Time { get; set; }

        [JsonProperty("content")]
        public ContentElement Content { get; set; }

        [JsonProperty("media")]
        public Video Media { get; set; }
    }

    public partial class DailyScheduleListTableKeyLangs
    {
        [JsonProperty("time")]
        public string Time { get; set; }

        [JsonProperty("content")]
        public string Content { get; set; }

        [JsonProperty("media")]
        public string Media { get; set; }
    }

    public partial class ScheduleListTableKeyLangs
    {
        [JsonProperty("meals")]
        public string Meals { get; set; }

        [JsonProperty("daily_schedule_list")]
        public string DailyScheduleList { get; set; }
    }

    public partial class PmdlSuggestedRoute
    {
        [JsonProperty("module_title")]
        public string ModuleTitle { get; set; }

        [JsonProperty("content")]
        public CustRemindsClass Content { get; set; }
    }

    public partial class PmdlVenueLocation
    {
        [JsonProperty("module_title")]
        public string ModuleTitle { get; set; }

        [JsonProperty("content")]
        public PmdlVenueLocationContent Content { get; set; }
    }

    public partial class PmdlVenueLocationContent
    {
        [JsonProperty("list")]
        public TentacledList[] List { get; set; }

        [JsonProperty("table_key_langs")]
        public FluffyTableKeyLangs TableKeyLangs { get; set; }
    }

    public partial class TentacledList
    {
        [JsonProperty("location_name")]
        public ContentElement LocationName { get; set; }

        [JsonProperty("latlng")]
        public Latlng Latlng { get; set; }

        [JsonProperty("gather")]
        public ContentElement Gather { get; set; }

        [JsonProperty("setout")]
        public ContentElement Setout { get; set; }

        [JsonProperty("arrival_desc")]
        public ContentElement ArrivalDesc { get; set; }

        [JsonProperty("photo", NullValueHandling = NullValueHandling.Ignore)]
        public Video Photo { get; set; }

        [JsonProperty("video")]
        public Video Video { get; set; }
    }

    public partial class FluffyTableKeyLangs
    {
        [JsonProperty("location_name")]
        public string LocationName { get; set; }

        [JsonProperty("latlng")]
        public string Latlng { get; set; }

        [JsonProperty("photo")]
        public string Photo { get; set; }

        [JsonProperty("video")]
        public string Video { get; set; }

        [JsonProperty("gather")]
        public string Gather { get; set; }

        [JsonProperty("setout")]
        public string Setout { get; set; }

        [JsonProperty("arrival_desc")]
        public string ArrivalDesc { get; set; }
    }

    #endregion PMDL for product --- end

    /////////////

    #region PMDL for package --- start

    public partial class PmdlExchangeValid
    {
        [JsonProperty("module_title")]
        public string ModuleTitle { get; set; }

        [JsonProperty("content")]
        public PmdlExchangeValidContent Content { get; set; }
    }


    public partial class PmdlExchangeValidContent
    {
        [JsonProperty("properties")]
        public PurpleProperties Properties { get; set; }
    }

    public partial class PurpleProperties
    {
        [JsonProperty("exchange")]
        public Exchange Exchange { get; set; }

        [JsonProperty("exchange_description")]
        public Exchange ExchangeDescription { get; set; }
    }

    public partial class Exchange
    {
        [JsonProperty("desc")]
        public string Desc { get; set; }
    }

    public partial class PmdlIncNinc
    {
        [JsonProperty("module_title")]
        public string ModuleTitle { get; set; }

        [JsonProperty("content")]
        public PmdlIncNincContent Content { get; set; }
    }

    public partial class PmdlIncNincContent
    {
        [JsonProperty("properties")]
        public FluffyProperties Properties { get; set; }
    }

    public partial class FluffyProperties
    {
        [JsonProperty("include_item")]
        public IncludeItem IncludeItem { get; set; }

        [JsonProperty("not_include_item")]
        public IncludeItem NotIncludeItem { get; set; }
    }

    public partial class IncludeItem
    {
        [JsonProperty("list")]
        public Exchange[] List { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }
    }

    public partial class PmdlPackageDesc
    {
        [JsonProperty("module_title")]
        public string ModuleTitle { get; set; }

        [JsonProperty("content")]
        public PmdlPackageDescContent Content { get; set; }
    }

    public partial class PmdlPackageDescContent
    {
        [JsonProperty("list")]
        public Exchange[] List { get; set; }
    }

    public partial class PmdlRefundPolicy
    {
        [JsonProperty("module_title")]
        public string ModuleTitle { get; set; }

        [JsonProperty("content")]
        public PmdlRefundPolicyContent Content { get; set; }
    }

    public partial class PmdlRefundPolicyContent
    {
        [JsonProperty("properties")]
        public TentacledProperties Properties { get; set; }
    }

    public partial class TentacledProperties
    {
        [JsonProperty("policy_type")]
        public PolicyType PolicyType { get; set; }

        [JsonProperty("partial_refund")]
        public IncludeItem PartialRefund { get; set; }
    }

    public partial class PolicyType
    {
        [JsonProperty("desc")]
        public string Desc { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }
    }

    public partial class PmdlSimCard
    {
        [JsonProperty("module_title")]
        public string ModuleTitle { get; set; }

        [JsonProperty("content")]
        public PmdlSimCardContent Content { get; set; }
    }

    public partial class PmdlSimCardContent
    {
        [JsonProperty("properties")]
        public StickyProperties Properties { get; set; }
    }

    public partial class StickyProperties
    {
        [JsonProperty("datatraffic")]
        public PolicyType Datatraffic { get; set; }

        [JsonProperty("transmission")]
        public PolicyType Transmission { get; set; }

        [JsonProperty("region")]
        public PolicyType Region { get; set; }

        [JsonProperty("telcom")]
        public PolicyType Telcom { get; set; }

        [JsonProperty("simcard_type")]
        public PolicyType SimcardType { get; set; }

        [JsonProperty("hotspot")]
        public PolicyType Hotspot { get; set; }

        [JsonProperty("band")]
        public PolicyType Band { get; set; }

        [JsonProperty("addvalue")]
        public PolicyType Addvalue { get; set; }

        [JsonProperty("excludes")]
        public PolicyType Excludes { get; set; }

        [JsonProperty("valid_date")]
        public PolicyType ValidDate { get; set; }

        [JsonProperty("unsupport")]
        public PolicyType Unsupport { get; set; }

        [JsonProperty("notes")]
        public IncludeItem Notes { get; set; }
    }

    public partial class PmdlUseValid
    {
        [JsonProperty("module_title")]
        public string ModuleTitle { get; set; }

        [JsonProperty("content")]
        public PmdlUseValidContent Content { get; set; }
    }

    public partial class PmdlUseValidContent
    {
        [JsonProperty("properties")]
        public IndigoProperties Properties { get; set; }
    }

    public partial class IndigoProperties
    {
        [JsonProperty("exchange")]
        public Exchange Exchange { get; set; }

        [JsonProperty("exchange_description")]
        public Exchange ExchangeDescription { get; set; }

        [JsonProperty("expired")]
        public Exchange Expired { get; set; }
    }

    public partial class PmdlWifi
    {
        [JsonProperty("module_title")]
        public string ModuleTitle { get; set; }

        [JsonProperty("content")]
        public PmdlWifiContent Content { get; set; }
    }

    public partial class PmdlWifiContent
    {
        [JsonProperty("properties")]
        public IndecentProperties Properties { get; set; }
    }

    public partial class IndecentProperties
    {
        [JsonProperty("datatraffic")]
        public PolicyType Datatraffic { get; set; }

        [JsonProperty("transmission")]
        public PolicyType Transmission { get; set; }

        [JsonProperty("region")]
        public PolicyType Region { get; set; }

        [JsonProperty("telcom")]
        public PolicyType Telcom { get; set; }

        [JsonProperty("charging_time")]
        public PolicyType ChargingTime { get; set; }

        [JsonProperty("power_duration")]
        public PolicyType PowerDuration { get; set; }

        [JsonProperty("deposit")]
        public PolicyType Deposit { get; set; }

        [JsonProperty("weight")]
        public PolicyType Weight { get; set; }

        [JsonProperty("size")]
        public PolicyType Size { get; set; }

        [JsonProperty("max_connections")]
        public PolicyType MaxConnections { get; set; }

        [JsonProperty("includes")]
        public PolicyType Includes { get; set; }

        [JsonProperty("notes")]
        public IncludeItem Notes { get; set; }
    }

    #endregion PMDL for package --- end

    /////////////

    public partial class PmdlModel
    {
        public static PmdlModel FromJson(string json) => JsonConvert.DeserializeObject<PmdlModel>(json, Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this PmdlModel self) => JsonConvert.SerializeObject(self, Converter.Settings);
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }
}
