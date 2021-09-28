using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Trip.PasvAPI.Models.Model
{
    public class CountryCityRespModel
    {
        public string result { get; set; }
        public string result_msg { get; set; }
        public List<CountryCitiesCodeModel> Countries { get; set; }
    }

    public class CountryCitiesCodeModel
    {
        [JsonProperty("Country_Name")]
        public string CountryName { get; set; }

        [JsonProperty("Country_Code")]
        public string CountryCode { get; set; }

        [JsonProperty("Cities")]
        public CityCodeModel[] Cities { get; set; }
    }

    public partial class CityCodeModel
    {
        [JsonProperty("City_Name")]
        public string CityName { get; set; }

        [JsonProperty("City_Code")]
        public string CityCode { get; set; }
    }

    ///////////

    public class CountryCityMapModel
    {
        public string city_code { get; set; }
        public string country_code { get; set; }
        public string name { get; set; }
    }
}
