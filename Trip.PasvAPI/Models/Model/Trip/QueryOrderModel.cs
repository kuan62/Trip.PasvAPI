using System;
using System.Collections.Generic;

namespace Trip.PasvAPI.Models.Model.Trip
{
    public class QueryOrderReqModel
    {
        public string sequenceId { get; set; }
        public string otaOrderId { get; set; }
        public string supplierOrderId { get; set; }
    }

    //////////

    public class QueryOrderRespModel
    {
        public string otaOrderId { get; set; }
        public string supplierOrderId { get; set; }
        public List<ItemNodeModel> items { get; set; }
        public class ItemNodeModel
        {
            public string itemId { get; set; }
            public string useStartDate { get; set; }
            public string useEndDate { get; set; }
            public int orderStatus { get; set; }
            public int quantity { get; set; }
            public int useQuantity { get; set; }
            public int cancelQuantity { get; set; }
        }
    }
}
