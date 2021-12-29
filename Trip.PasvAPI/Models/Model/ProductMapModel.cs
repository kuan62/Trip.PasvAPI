using System;
using System.Collections.Generic;

namespace Trip.PasvAPI.Models.Model
{
	public class ProductMapModel
	{
		public Int64 map_seq { get; set; }
		public string ota_prod_id { get; set; }
		public string ota_prod_name { get; set; }
		public string ota_plu { get; set; }
		public string map_mode { get; set; }
		public string map_status { get; set; }
		public bool allow_seltime { get; set; }
		public string prod_oid { get; set; }
		public string prod_name { get; set; }
		public string pkg_oid { get; set; }
		public string pkg_name { get; set; }
		public string item_oid { get; set; }
		public string sku_id { get; set; }
		public string sku_name { get; set; }
		public string time_slot { get; set; }
		public bool enable_timeslot { get; set; }
		public string create_user { get; set; }
		public DateTime? create_date { get; set; }
		public string modify_user { get; set; }
		public DateTime? modify_date { get; set; }
	}

	public class ProductMapExModel : ProductMapModel
	{
		public string map_mode_name { get; set; }
		public string map_status_name { get; set; }
	}

	///////////////////

	public class InsertProductMapModel
    {
        public Int64? map_seq { get; set; }
		public string ota_prod_id { get; set; }
		public string ota_prod_name { get; set; }
		public string map_mode { get; set; }
		public string map_status { get; set; }
		public bool allow_seltime { get; set; }
	    public Int64 prod_oid { get; set; }
        public string prod_name { get; set; }

        public class PackageModel
        {
            public Int64 pkg_oid { get; set; }
            public string pkg_name { get; set; }

            public class SkuModel
            {
                public string sku_id { get; set; }
                public string sku_name { get; set; }
            }
            public List<SkuModel> skus { get; set; }
        }
        public List<PackageModel> pkgs { get; set; }
		public List<string> time_slots { get; set; }
		public string create_user { get; set; }
    }
}
