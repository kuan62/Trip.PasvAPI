using System;
using System.Collections.Generic;

namespace Trip.PasvAPI.Models.Model.Trip
{
    public class CancelOrderConfirmReqModel
	{
		public string sequenceId { get; set; }
		public string otaOrderId { get; set; }
		public string supplierOrderId { get; set; }
		public string confirmResultCode { get; set; }
		public string confirmResultMessage { get; set; }
		public List<ItemModel> items { get; set; }
		public class ItemModel {
			public string itemId { get; set; }
		}
    }
}
