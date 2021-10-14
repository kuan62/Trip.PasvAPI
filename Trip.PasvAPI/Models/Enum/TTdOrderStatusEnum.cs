using System;
namespace Trip.PasvAPI.Models.Enum
{
    public enum TTdOrderStatusEnum
    {
        NEW_WAIT = 1, // 新订待确认 New order to be confirmed	
        NEW_CONFIRMED = 2,  // 新订已确认 The new order has been confirmed
        CANCEL_APPLY = 3,   // 取消待确认 Cancelationl to be confirmed	
        PARTIAL_CANCELED = 4, // 部分取消 Partially canceled 使用前的部分取消的状态 The status of partial cancellation before use
        ALL_CANCELED = 5, // 全部取消 All canceled	
        PICKED_ITEMS = 6, // 已取物品（票券、物件）Picked items(tickets, objects)
        PARTIAL_USED = 7, // 部分使用Partially used 使用后的部分使用状态 The status of partially use after use
        ALL_USED = 8, //	全部使用 All used	
        RETURNED = 9,   // 已还物品（票券、物件）Returneditems(tickets, objects)
        EXPIRED = 10	 // 已过期 Expired
    }
}
