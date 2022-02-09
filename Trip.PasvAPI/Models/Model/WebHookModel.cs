using System;
using System.Collections.Generic;

/*
 {
	"result_type":"order",
	"order":
	{
	  "order_no":"21KK123123",
	  "status":"GO_OK",
	  "voucher":
		{
		  "isGenerate":"true"
		}		
	}
 }
 */
namespace Trip.PasvAPI.Models.Model
{
    public class WebHookModel
    {
        public string result_type { get; set; } // 傳回型別有"order","product"兩種，傳回"order"代表是order別的主動通知

        // 訂單通知
        public class WebHookOrderModel
        {
            public string order_no { get; set; } //	訂單編號
            public string status { get; set; }  // 四種訂單狀態：GO:處理中 GO_OK:已處理 CX_ING:取消中 CX:已取消

            public class OrderVoucherWebHookModel
            {
                public bool isGenerate { get; set; } // 憑證是否產生
            }
            public OrderVoucherWebHookModel voucher { get; set; } // 憑證的物件 
        }
        public WebHookOrderModel order { get; set; }

        // 商品通知
        public class WebHookProductModel
        {
            public string prod_no { get; set; }  // 產品編號
            public string pkg_no { get; set; } //餐編號
            public bool is_active { get; set; } // 產品或套餐是否上架（如果只有給prod_no則是產品是否上架，如果有套餐編號，則是對應套餐是否上架）

            public class WebHookProductMarketingModel
            {
                public string market { get; set; }  // 產品市場名稱
                public bool is_sales { get; set; } // 產品市場是否可售
            }
            public List<WebHookProductMarketingModel> marketing { get; set; }
        }
        public WebHookProductModel product { get; set; } // product型別的物件 
    }
}
