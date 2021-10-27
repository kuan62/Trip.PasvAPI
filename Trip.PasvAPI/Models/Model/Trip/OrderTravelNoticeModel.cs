using System;
using System.Collections.Generic;

namespace Trip.PasvAPI.Models.Model.Trip
{
	public class OrderTravelNoticeReqModel
	{
		public string sequenceId { get; set; } 
		public string otaOrderId { get; set; }
		public string supplierOrderId { get; set; }

		public List<VoucherModel> vouchers { get; set; }
		public class VoucherModel
		{
			public string itemId { get; set; } 
			public int voucherType { get; set; } 
			public string voucherCode { get; set; }
			public string voucherData { get; set; }
		}

		public List<ItemModel> items { get; set; }
		public class ItemModel
		{
			public string itemId { get; set; }
			public string remark { get; set; }

			public ExpressDeliveryModel expressDelivery { get; set; }
			public class ExpressDeliveryModel{
				public int deliveryType { get; set; } // 1.客人自提 2.供应商自己送达 3.供应商委托快递公司送达（必需提供快递信息）
				public string companyCode { get; set; }
				public string companyName { get; set; }
				public string trackingNumber { get; set; }
				public string goodsName { get; set; }
				public string goodsQuantity { get; set; }
				public string sendMessage { get; set; }
				public string receiveMessage { get; set; }
			}

			public List<TravelInformationModel> travelInformations { get; set; }
			public class TravelInformationModel {
				public string name { get; set; }
				public string content { get; set; }
			}
        }
	}
}
