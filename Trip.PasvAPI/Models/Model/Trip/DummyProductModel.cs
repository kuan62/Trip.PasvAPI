using System;
using System.Collections.Generic;

namespace Trip.PasvAPI.Models.Model.Trip
{
    public class DummyProductModel
    {
		public Int64 dummy_prod_oid { get; set; }
		public string plu { get; set; }
		public string prod_name { get; set; }
		public string status { get; set; }
		public string prod_type { get; set; }
		public int confirm_type { get; set; }
		public string sales_mode { get; set; }
		public bool check_price { get; set; }
		public double price { get; set; }
		public string price_currency { get; set; }
		public double net_price { get; set; }
		public string net_p1rice_currency { get; set; }
		public int qty { get; set; }
		public string booking_confirm_mode { get; set; }
		public string refund_confirm_mode { get; set; }
		public string cancel_confirm_mode { get; set; }
		public string amend_confirm_mode { get; set; }
		public string voucher_sender { get; set; }
		public string voucher_type { get; set; }
		public string delivery_type { get; set; }
		public string create_user { get; set; }
		public Dictionary<string,object> param1 { get; set; }
		public DateTime? create_time { get; set; }
		public string modify_user { get; set; }
		public DateTime? modify_time { get; set; }
	}
}
