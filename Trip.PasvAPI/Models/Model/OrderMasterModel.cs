using System;
using System.Collections.Generic;

namespace Trip.PasvAPI.Models.Model
{
	public class OrderMasterModel
	{ 
		public Int64 order_master_seq { get; set; }
		public string order_master_mid { get; set; }
		public string order_mid { get; set; }
		public Int64 order_oid { get; set; }
		// OTA 相關欄位
		public string ota_order_id { get; set; }
		public int ota_item_seq { get; set; }
		public string ota_sequence_id { get; set; }
		public string ota_item_plu { get; set; }
		public int [] ota_item_pax { get; set; }
		public string ota_tag { get; set; }

		public string currency { get; set; }
		public double amount { get; set; }

		public string status { get; set; }
		public BookingDataModel booking_info { get; set; }
		public Dictionary<string, object> param1 { get; set; }
		public string create_user { get; set; }
		public DateTime? create_time { get; set; }
		public string modify_user { get; set; }
		public DateTime? modify_time { get; set; }
	}

	public class OrderMasterExModel : OrderMasterModel
    {
		public string prod_name { get; set; }
		public string ota_prod_name { get; set; }
		public string ota_item_id { get; set; }
		public string use_start_date { get; set; }
		public string use_end_date { get; set; }
		public int use_quantity { get; set; }
	}
}
