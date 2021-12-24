using System;
using System.Collections.Generic;

namespace Trip.PasvAPI.Models.Model
{
    public class ProductPickerModel
    {
        public Int64 prod_oid { get; set; }
        public string prod_name { get; set; }

        public class PackageModel
        {
            public Int64 pkg_oid { get; set; }
            public string pkg_name { get; set; }

             public class ItemModel
            {
                public Int64 item_oid { get; set; }

                public class skuModel
                {
                    public string sku_id { get; set; }
                    public string sku_name { get; set; }
                }
                public List<skuModel> sku { get; set; }
            }
            public List<ItemModel> item { get; set; }
        }
        public List<PackageModel> pkg { get; set; }
    }
     
}
