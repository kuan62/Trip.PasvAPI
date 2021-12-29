using System;
using System.Collections.Generic;

namespace Trip.PasvAPI.Models.Model.Trip
{
    public class QueryOrderReqModel
    {
        public string sequenceId { get; set; }
        public string otaOrderId { get; set; }
        public string supplierOrderId { get; set; } // KKday 訂單主檔
    }

    //////////

    public class QueryOrderRespModel
    {
        public string otaOrderId { get; set; }
        public string supplierOrderId { get; set; } // KKday 訂單主檔
        public List<ItemNodeModel> items { get; set; }
        public class ItemNodeModel
        {
            public string itemId { get; set; }
            public string useStartDate { get; set; }
            public string useEndDate { get; set; }
            public int orderStatus { get; set; }
            public int quantity { get; set; } // 订单数量
            public int useQuantity { get; set; } // 实际使用数量
            public int cancelQuantity { get; set; } // 实际取消数量
        }
    }
}
