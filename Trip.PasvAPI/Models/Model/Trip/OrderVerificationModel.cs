using System;
using System.Collections.Generic;

namespace Trip.PasvAPI.Models.Model.Trip
{
    public class OrderVerificationReqModel
    {
        public string sequenceId { get; set; }
        // 联系人节点
        public List<ContactModel> contacts { get; set; }
        public class ContactModel
        {
            public string name { get; set; }
            public string mobile { get; set; }
            public string intlCode { get; set; }
            public string optionalMobile { get; set; }
            public string optionalIntlCode { get; set; }
            public string email { get; set; }
        }
        // 订单项节点
        public List<ItemModel> items { get; set; }
        public class ItemModel
        {
            public string PLU { get; set; }
            public string useStartDate { get; set; }
            public string useEndDate { get; set; }
            public float price { get; set; }
            public string priceCurrency { get; set; }
            public float cost { get; set; }
            public string costCurrency { get; set; }
            public int quantity { get; set; }
            // 出行人节点
            public List<PassengerModel> passengers { get; set; }
            public class PassengerModel
            {
                public string name { get; set; }
                public string firstNa { get; set; }
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

            public List<AdjunctionModel> adjunctions { get; set; }
            // 下单附加属性节点
            public class AdjunctionModel
            {
                public string name { get; set; }
                public string nameCode { get; set; } // 名称编码（系统标识）
                public string content { get; set; }
                public string contentCode { get; set; } // 内容编码（系统标识）
            }

        }
    }

    ///////////////

    public class OrderVerificationRespModel
    {
        public List<ItemModel> items { get; set; }
        public class ItemModel
        {
            public string PLU { get; set; }
            // 剩余库存数量节点
            public List<InventorysModel> inventorys { get; set; }
            public class InventorysModel
            {
                public string useDate { get; set; }
                public int quantity { get; set; }
            }
        }
    }
}
