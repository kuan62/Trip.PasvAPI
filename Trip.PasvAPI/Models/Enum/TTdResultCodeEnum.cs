﻿using System;
using System.Runtime.Serialization;

namespace Trip.PasvAPI.Models.Enum
{
    public enum TTdResultCodeEnum
    {
        SUCCESS = 0000, //操作成功 Operation succeeded 
        PARSING_ERROR = 0001, //报文解析失败 Message parsing failed
        WRONG_SIGNATURE = 0002, //签名错误 Wrong Signature
        INCORRECT_SUPPLUER = 0003, //供应商账户信息不正确 Incorrect supplier account information
        INVALID_PLU = 1001, //产品PLU不存在错误 Product PLU does not exist error
        PRODUCT_REMOVED = 1002, //产品已经下架 The product has been removed
        INSUFFICIENT_INVENTORY = 1003, //库存不足 Inventory shortage
        PURCHCASE_RESTRICT = 1004, //被限购 Purchase restriction 已经预订过了
        MISSING_INFO = 1005, //信息缺失+具体缺失信息 Miss info + Specific Missing info 例如出行人信息、预定附加信息、配送信息等缺失 
        INFO_ERROR = 1006, //信息错误+具体错误信息 Info error + specific error info 例如出行人信息、预定附加信息、配送信息等错误 
        INVALID_PRICE = 1007, //产品价格不存在 Product price does not exist
        INSUFFICIENT_BALANCE = 1008, //账户余额不足 Account balance is not enough
        DATE_ERROR = 1009, //日期错误+具体错误类型日期 Date error + specific error type date
        UNGROUP = 1010, //不成团 Unable to be a group
        // 1100-1199 自行定义 Self-definition
        INTERNAL_ERROR = 1100,
        DUPLICATED = 1101
    }
}
