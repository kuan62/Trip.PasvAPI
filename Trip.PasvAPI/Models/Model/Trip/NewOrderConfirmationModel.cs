using System;
using System.Collections.Generic;

namespace Trip.PasvAPI.Models.Model.Trip
{
	public class NewOrderConfirmationReqModel
	{
		public string sequenceId { get; set; }
		public string otaOrderId { get; set; }
		public string supplierOrderId { get; set; }
		public string confirmResultCode { get; set; }
		public string confirmResultMessage { get; set; }
		public int voucherSender { get; set; }
		public List<VouchersModel> vouchers { get; set; }
		public class VouchersModel {
			public string itemId { get; set; }
			public int voucherType { get; set; }
			public string voucherCode { get; set; }
			public string voucherData { get; set; }
		}
		public List<ItemModel> items { get; set; }
		public class ItemModel {
			public string itemId { get; set; }
			public List<InventoryModel> inventorys { get; set; }
			public class InventoryModel {
				public string useDate { get; set; }
				public int quantity { get; set; }
			}
		}
	}
}
