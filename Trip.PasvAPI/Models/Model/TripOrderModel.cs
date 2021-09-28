using System;
using System.Collections.Generic;
using Trip.PasvAPI.Models.Model.Trip;

namespace Trip.PasvAPI.Models.Model
{
	public class TripOrderModel
	{
		public Int64 trip_order_oid { get; set; }
		public string sequence_id { get; set; }
		public string ota_order_id { get; set; }
		public int confirm_type { get; set; }
		public List<CreateOrderReqModel.OrderContactModel> contacts { get; set; }
		public List<CreateOrderReqModel.OrderCouponModel> coupons { get; set; }
		public List<CreateOrderReqModel.OrderItemModel> items { get; set; }
		public string status { get; set; }
		public string note { get; set; }
		public string create_user { get; set; }
		public DateTime? create_time { get; set; }
		public string modify_user { get; set; }
		public DateTime? modify_time { get; set; }
	}
}
