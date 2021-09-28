using System;
using System.Collections.Generic;

namespace Trip.PasvAPI.Models.Model.Trip
{
    public class OrderConsumedNoticeReqModel
    { 
		public string sequenceId { get; set; }
		public string otaOrderId { get; set; }
		public string supplierOrderId { get; set; }
		public List<ItemModel> items { get; set; }
		public class ItemModel
		{
			public string itemId { get; set; }
			public string useStartDate { get; set; }
			public string useEndDate { get; set; }
			public int quantity { get; set; }
			public int useQuantity { get; set; }
			public string remark { get; set; }
			public ItemDiscount discount { get; set; }
			public class ItemDiscount
			{
				public List<ItemDiscountPolicyModel> policyList { get; set; }
				public class ItemDiscountPolicyModel
				{
					public string date { get; set; }
					public int quantity { get; set; }
				}
			}
			public float lostAmount { get; set; }
			public string lostAmountCurrency { get; set; }
		}
	}
}
