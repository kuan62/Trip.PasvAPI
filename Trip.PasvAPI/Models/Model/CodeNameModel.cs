using System;
using Newtonsoft.Json;

namespace Trip.PasvAPI.Models.Model
{
    public partial class CodeNameModel
    {
        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
