using System;
using System.Collections.Generic;
using Trip.PasvAPI.Models.Enum;

namespace Trip.PasvAPI.Models.Model.Trip
{
    public class CreateOrderReqModel
    { 
        public string sequenceId { get; set; }
        public string otaOrderId { get; set; }
        public int confirmType { get; set; }

        public List<OrderContactModel> contacts { get; set; }
        public class OrderContactModel
        {
            public string name { get; set; }
            public string mobile { get; set; } 
            public string intlCode { get; set; } 
            public string optionalMobile { get; set; } 
            public string optionalIntlCode { get; set; } 
            public string email { get; set; }
        }

        public List<OrderCouponModel> coupons { get; set; }
        public class OrderCouponModel
        {
            public int type { get; set; }
            public string code { get; set; }
            public string name { get; set; }
            public float amount { get; set; }
            public string amountCurrency { get; set; }
        }

        public List<OrderItemModel> items { get; set; }
        public class OrderItemModel
        {
            public string itemId { get; set; }
            public string openId { get; set; }
            public string PLU { get; set; } // 格式 => PROD_OID|PKG_OID|ITEM_OID|SKU
            public string locale { get; set; }
            public string distributionChannel { get; set; }
            public string useStartDate { get; set; }
            public string useEndDate { get; set; }
            public string lastConfirmTime { get; set; }
            public string remark { get; set; }
            public float price { get; set; }
            public string priceCurrency { get; set; }
            public float cost { get; set; }
            public string costCurrency { get; set; }
            public float? suggestedPrice { get; set; }
            public string suggestedPriceCurrency { get; set; }
            public int quantity { get; set; }
             
            public List<OrderItemPassengerModel> passengers { get; set; }
            public class OrderItemPassengerModel
            {
                public string name { get; set; }
                public string firstName { get; set; }
                public string lastName { get; set; }
                public string mobile { get; set; }
                public string intlCode { get; set; }
                public string cardType { get; set; }
                public string cardNo { get; set; }
                public string birthDate { get; set; }
                public string ageType { get; set; }
                public string gender { get; set; }
                public string nationalityCode { get; set; }
                public string nationalityName { get; set; }
                public string cardIssueCountry { get; set; }
                public string cardIssuePlace { get; set; }
                public string cardIssueDate { get; set; }
                public string cardValidDate { get; set; }
                public string birthPlace { get; set; }
                public float height { get; set; }
                public float weight { get; set; }
                public float myopiaDegreeL { get; set; }
                public float myopiaDegreeR { get; set; }
                public float shoeSize { get; set; }
            }

            public List<OrderItemAdjunctionModel> adjunctions { get; set; }
            public class OrderItemAdjunctionModel
            {
                public string name { get; set; }
                public string nameCode { get; set; }
                public string content { get; set; }
                public string contentCode { get; set; }
            }

            public OrderItemDepositModel deposit { get; set; }
            public class OrderItemDepositModel
            {
                public int type { get; set; }
                public float amount { get; set; }
                public string amountCurrency { get; set; }
            }

            public OrderItemExpressDeliveryModel expressDelivery { get; set; }
            public class OrderItemExpressDeliveryModel
            {
                public int type { get; set; }
                public string name { get; set; }
                public string mobile { get; set; }
                public string intlCode { get; set; }
                public string country { get; set; }
                public string province { get; set; }
                public string city { get; set; }
                public string district { get; set; }
                public string address { get; set; }
            }
        }
    }

    /////////

    public class CreateOrderRespModel
    {
        public string otaOrderId { get; set; }
        public string supplierOrderId { get; set; }
        public int supplierConfirmType { get; set; }
        public int voucherSender { get; set; }

        public List<OrderVoucherModel> vouchers { get; set; }
        public class OrderVoucherModel
        {
            public string itemId { get; set; }
            public int voucherType { get; set; }
            public string voucherCode { get; set; }
            public string voucherData { get; set; }
        }

        public List<OrderItemModel> items { get; set; }
        public class OrderItemModel
        {
            public string itemId { get; set; }

            public OrderItemInventoryModel inventorys { get; set; }
            public class OrderItemInventoryModel
            {
                public string useDate { get; set; }
                public int quantity { get; set; }
            }
        }
    }
}
