using System;
namespace Trip.PasvAPI.Models.Model.Trip
{
    public class TTdReqHeaderModel
    {
        public string accountId { get; set; }
        public string serviceName { get; set; }
        public string requestTime { get; set; }
        public string version { get; set; }
        public string sign { get; set; }
    }
}
