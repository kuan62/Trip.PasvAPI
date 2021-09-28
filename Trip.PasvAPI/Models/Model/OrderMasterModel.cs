using System;
using System.Collections.Generic;

namespace Trip.PasvAPI.Models.Model
{
	public class OrderMasterModel
	{ 
		public Int64 order_master_oid { get; set; }
		public string kkday_order_master_mid { get; set; }
		public string kkday_order_mid { get; set; }
		public Int64 kkday_order_oid { get; set; }
		public Int64 trip_order_oid { get; set; }
		public int trip_item_seq { get; set; }
		public List<int> trip_item_pax { get; set; }
		public string trip_item_plu { get; set; }
		public string status { get; set; }
		public BookingDataModel booking_info { get; set; }
		public Dictionary<string, object> param1 { get; set; }
		public string create_user { get; set; }
		public DateTime? create_time { get; set; }
		public string modify_user { get; set; }
		public DateTime? modify_time { get; set; }
	}
}
