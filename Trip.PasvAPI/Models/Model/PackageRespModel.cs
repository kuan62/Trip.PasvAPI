using System;
namespace Trip.PasvAPI.Models.Model
{
    public partial class PackageRespModel : PackageSkuModel
    {
        public string result { get; set; }
        public string result_msg { get; set; }
    }
}
