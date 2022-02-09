using System;
using System.Collections.Generic;

namespace Trip.PasvAPI.Models.Model
{
    public class VoucherQueryRespModel
    {
        public string result { get; set; }
        public string result_msg { get; set; }

        public class VoucherFileInfoModel
        {
            public string order_file_id { get; set; }
            public int barcode_count { get; set; }
            public string order_no { get; set; }
            public string guide_lang { get; set; }
            public string prod_name { get; set; }
            public string pkg_name { get; set; }
            public int price1_qty { get; set; }
            public int price2_qty { get; set; }
            public int price3_qty { get; set; }
            public int price4_qty { get; set; }

            public class VoucherFileInfoLastModifiedModel
            {
                public string date { get; set; }
            }
            public VoucherFileInfoLastModifiedModel last_modified { get; set; }
        }
        public List<VoucherFileInfoModel> file { get; set; }
    }

    ////////

    public class VoucherDownloadRespModel
    {
        public string result { get; set; }
        public string result_msg { get; set; }

        public class VoucherFileModel
        {
            public string order_file_id { get; set; } // 檔案編號
            public string file_name { get; set; } // 檔案名稱
            public string file_intro { get; set; } // 檔案敘述
            public string ontent_type { get; set; } // 檔案類型
            public string encode_str { get; set; } // 檔案編碼
        }
        public List<VoucherFileModel> file { get; set; } // 檔案列表 
    }
     
}
