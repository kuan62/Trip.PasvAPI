using System;
using System.Collections.Generic;

namespace Trip.PasvAPI.Models.Model.Trip
{
    public class CancelOrderReqModel
    { 
		public string sequenceId { get; set; }
		public string otaOrderId { get; set; }
		public string supplierOrderId { get; set; }
		public int confirmType { get; set; }
		public List<ItemNoteModel> items { get; set; }
		public class ItemNoteModel
		{
			public string itemId { get; set; }
			public string PLU { get; set; }
			public string lastConfirmTime { get; set; }
			public int quantity { get; set; }
			public float amount { get; set; }
			public string amountCurrency { get; set; }
		}
	}

	////////
	
	public class CancelOrderRespModel
	{
		public int supplierConfirmType { get; set; }
		public List<ItemNodeModel> items { get; set; }
		public class ItemNodeModel
		{
			public string itemId { get; set; }
		}
	}
}
