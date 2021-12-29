using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Npgsql;
using Trip.PasvAPI.AppCode;
using Trip.PasvAPI.Models.Model;
using Trip.PasvAPI.Models.Model.Trip;
using Trip.PasvAPI.Proxy;

namespace Trip.PasvAPI.Models.Repository
{
    public class OrderBookingRepository
    {
        private readonly TripOrderRepository _tripOrderRepos;
        private readonly OrderMasterRepository _masterRepos;
        private readonly ProductProxy _prodProxy;
        private readonly BookingProxy _bookingProxy;
        // private readonly IMemoryCache _cache;

        public OrderBookingRepository(TripOrderRepository tripOrderRepos, ProductProxy prodProxy,
            BookingProxy bookingProxy, OrderMasterRepository masterRepos)
        {
            this._tripOrderRepos = tripOrderRepos;
            this._prodProxy = prodProxy;
            this._bookingProxy = bookingProxy;
            this._masterRepos = masterRepos;
        }

        //查詢product
        //查詢package 取sku token
        //沒有場次的金額{"2021-11-21":{"b2b_price":{"fullday":170},"b2c_price":{"fullday":200}}}
        //有場次{"2021-11-21":{"b2b_price":{"09:00":508,"14:00":508},"b2c_price":{"09:00":535,"14:00":535}}}
        //要確認 1.有位控 2.日期必須要有金額必須相同
        public ResponeObj crtOrder(CreateOrderReqModel tripOrder)
        {
            var jsonTrip = JsonConvert.SerializeObject(tripOrder);

            System.Console.WriteLine($"crtOrder start! guid={tripOrder?.sequenceId},  tripOrder ={jsonTrip}");
            Website.Instance.logger.Info($"crtOrder start! guid={tripOrder?.sequenceId},  tripOrder ={jsonTrip}");

            BookingProdInfo bookingInfo = new BookingProdInfo();
            ResponeObj resp = new ResponeObj();

            try
            {
                resp = this.chkInputData(tripOrder);
                if (resp.resp_code != "0000")
                {
                    return resp;
                }

                //一開始還是要取出prod_oid & pkg_oid, 在位控&價格還是要foreach重取skus
                // "PROD_OID|PKG_OID|SKU_ID|HH:MM"
                var prodInfoTemp = tripOrder.items[0]?.PLU?.ToString()?.Split('|');
                if (prodInfoTemp.Count() > 2)
                {
                    bookingInfo.prod_oid = prodInfoTemp[0];
                    bookingInfo.pkg_oid = prodInfoTemp[1];
                    bookingInfo.sku_oid = prodInfoTemp[2];
                }
                else
                {
                    resp.resp_code = "1001";
                    resp.resp_msg = "PLU异常！";
                    return resp;
                    //throw new Exception("PLU异常！");
                }

                ProductRespModel prod = null;
                var result = _prodProxy.GetProduct(new ProductReqModel()
                {
                    locale = LocaleMapping.ToKKdayLocale(tripOrder.items[0].locale),
                    state = MarketingMapping.ToKKdayState(tripOrder.items[0].locale), // 設為台灣地區為主,
                    prod_no = bookingInfo.prod_oid
                }, Website.Instance.B2dApiAuthorToken);

                Console.WriteLine($"Product Result => {result}");

                var settings = new JsonSerializerSettings
                {
                    MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
                    DateParseHandling = DateParseHandling.None,
                    Converters = { new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal } }
                };

                prod = JsonConvert.DeserializeObject<ProductRespModel>(result, settings);

                //產品異常
                if (!prod.result.Equals("00"))
                {
                    resp.resp_code = "1002";
                    resp.resp_msg = "产品已经下架";
                    return resp;
                    //throw new NullReferenceException($"prod_oid={ "103966" } is invalid!");
                }

                //判斷市場可否賣
                if (prod?.prod_marketing["TW"]?.is_sale != true && prod?.prod_marketing["CN"]?.is_sale != true)
                {
                    resp.resp_code = "1002";
                    resp.resp_msg = "市㘯未开放！";
                    return resp;
                    //throw new Exception("市㘯未开放！");
                }

                PackageRespModel pkg = null;
                // fetch the value from the source

                result = _prodProxy.GetPackage(new QueryPackageModel()
                {
                    prod_no = bookingInfo.prod_oid,
                    pkg_no = bookingInfo.pkg_oid,
                    // currency = Website.Instance.AgentCurrency,
                    locale = LocaleMapping.ToKKdayLocale(tripOrder.items[0].locale),
                    state = MarketingMapping.ToKKdayState(tripOrder.items[0].locale)
                }, Website.Instance.B2dApiAuthorToken);

                Console.WriteLine($"Package Result => {result}");

                settings = new JsonSerializerSettings
                {
                    MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
                    DateParseHandling = DateParseHandling.None,
                    Converters = { new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal } }
                };

                pkg = JsonConvert.DeserializeObject<PackageRespModel>(result, settings);

                if (!pkg.result.Equals("00"))
                {
                    resp.resp_code = "1002";
                    resp.resp_msg = "产品已经下架";
                    return resp;
                    //throw new NullReferenceException($"Product={ "103966" }, Package={ "332130" } with null object.");
                }

                resp = this.chkAmount(tripOrder, pkg);if(resp.resp_code != "0000") { return resp; }//確認餘額
                resp = this.chkAllotmen(tripOrder, pkg); if (resp.resp_code != "0000") { return resp; }//確認庫存
                resp = this.chkPrice(tripOrder, pkg); if (resp.resp_code != "0000") { return resp; }//確認價格
                resp = this.chkBookingField(tripOrder?.sequenceId, prod, pkg); if (resp.resp_code != "0000") { return resp; }//確認旅規

                //滿足 BookingDataModel
                BookingDataModel bookingData = this.PrepareBookingModel(tripOrder, prod, pkg);
                //crtOrder
                if (bookingData != null)
                {
                    resp = _bookingProxy.Booking(tripOrder?.sequenceId, bookingData, Website.Instance.B2dApiAuthorToken);

                     // 成功, 寫入 order_master
                    if(resp.resp_code.Equals("00") || resp.resp_code.Equals("0000"))
                    {
                         var pax_lst = new List<int>();
                         tripOrder.items.ForEach(i => pax_lst.Add(tripOrder.items.IndexOf(i)));

                        _masterRepos.Insert(new OrderMasterModel()
                        {
                            order_master_mid = resp.order_master_mid,
                            order_mid = resp.order_no,
                            order_oid = Convert.ToInt64(resp.order_oid),
                            ota_sequence_id = tripOrder.sequenceId,
                            ota_order_id = tripOrder.otaOrderId,
                            ota_item_pax = pax_lst.ToArray(),
                            ota_item_plu = tripOrder.items[0].PLU,
                            booking_info = bookingData,
                            param1 = new Dictionary<string, object>(),
                            status = "OK",
                            create_user = "SYSTEM",
                            ota_tag = "TRIP"
                        }); ;

                    }
                    return resp;
                }
                else
                {
                    resp.resp_code = "1104";
                    resp.resp_msg = "轉換訂單失敗";
                    return resp;
                }
            }
            catch (Exception ex)
            {
                Website.Instance.logger.Info($"crtOrder 異常! guid={tripOrder?.sequenceId},  tripOrder ={ JsonConvert.SerializeObject(tripOrder)},ex:{ex?.Message?.ToString()},{ex?.StackTrace?.ToString()}");
                resp.resp_code = "1105";
                resp.resp_msg = "系統異常";
                return resp;
            }
        }


        //確認帶入資料是否正常
        private ResponeObj chkInputData(CreateOrderReqModel tripOrder)
        {
            ResponeObj res = new ResponeObj();
            Boolean chkLoop = true;
            res.resp_code = "0000";
            try
            {
                if (tripOrder == null || tripOrder?.items == null || tripOrder?.contacts==null)
                {
                    chkLoop = false;
                }

                foreach (var item in tripOrder?.items)
                {
                    if (string.IsNullOrEmpty(item.PLU) ||
                         item.quantity <= 0 ||
                            item.cost <= 0 ||
                          string.IsNullOrEmpty(item.useStartDate) ||
                         string.IsNullOrEmpty(item.useEndDate)
                        ) {
                        chkLoop = false;
                        break;
                    }
                }

                foreach (var cont in tripOrder?.contacts)
                {
                    if (string.IsNullOrEmpty(cont.email) ||
                        string.IsNullOrEmpty(cont.mobile) ||
                        string.IsNullOrEmpty(cont.intlCode) ||
                          string.IsNullOrEmpty(cont.name) )
                    {
                        chkLoop = false;
                        break;
                    }
                }

                if (chkLoop == false)
                {
                    Website.Instance.logger.Info($"chkInputData 帶入參數異常! guid={tripOrder?.sequenceId},  tripOrder ={ JsonConvert.SerializeObject(tripOrder)}");
                    res.resp_code = "1101";
                    res.resp_msg = "帶入參數異常";
                }
                return res;
            }
            catch (Exception ex)
            {
                //slack 通知
                Website.Instance.logger.Fatal($"chkInputData 帶入參數異常! guid={tripOrder?.sequenceId}, ex:{ex?.Message?.ToString()},{ex?.StackTrace?.ToString()}");
                res.resp_code = "1101";
                res.resp_msg = "帶入參數異常";
                return res;
            }

        }

        //確認餘額
        private ResponeObj chkAmount(CreateOrderReqModel tripOrder, PackageRespModel pkg)
        {
            ResponeObj res = new ResponeObj();
            res.resp_code = "0000";
            try
            {
                double bookingAmount = (tripOrder.items.Sum(x => x.quantity * x.cost));
                QueryRSAmountModel amount =_bookingProxy.QueryAmount(Website.Instance.B2dApiAuthorToken);

                if (amount.now_amount < bookingAmount)
                {
                    res.resp_code = "1008";
                    res.resp_msg = "账户余额不足";
                }

                return res;
            }
            catch (Exception ex)
            {
                //slack 通知
                Website.Instance.logger.Fatal($"chkAmount 餘額不足! guid={tripOrder?.sequenceId} ex:{ex?.Message?.ToString()},{ex?.StackTrace?.ToString()}");
                res.resp_code = "1008";
                return res;
            }
        }

        //確認位控
        private ResponeObj chkAllotmen(CreateOrderReqModel tripOrder, PackageRespModel pkg)
        {
            ResponeObj res = new ResponeObj();
            res.resp_code = "0000";
            Boolean chkLoop = false;
            try
            {
                //簡單判斷位控是否足夠
                //inventory_set: 位控機制 0(無限量）:不啟用位控機制, 1:品項, 2:SKU
                //inventory_type位控設定 null:無設定, 0:依總量設定, 1:依日期 / 場次設定

                //position object 位控資訊，當inventory_set = 0時，不需位控，position是null
                //position.item_remain_qty	int	當inventory_set=1與inventory_type=0時，商品總量  {"position":{"result":"0000","result_msg":"OK","item_remain_qty":164}}
                //position.itemCal_qty object 當inventory_set = 1與inventory_type = 1時，商品數量依照日期    {"position":{"itemCal_qty":[{"date":"2021-11-21","remain_qty":{"fullday":20}}]}}
                //position.sku_remain_qty object 當inventory_set = 2與inventory_type = 0時，每個sku的總量 {"position":{"result":"0000","result_msg":"OK","sku_remain_qty":[{"sku_id":"98a0c50dc9e484426315734388a17b35","remain_qty":300}]}}
                //position.skuCal_qty	object	當inventory_set=2與inventory_type=1時，每個sku的日期數量  {"position":{"skuCal_qty":[{"sku_id":"c318d78b33f57416a0c578e8921cc875","sku_cal":[{"date":"2021-11-21","remain_qty":{"16:00":30}}]}]}}

                Int64 inventory_set = pkg.item[0].inventory_set;
                Int64 inventory_type = pkg.item[0].inventory_type;

                if (inventory_set == 0) //0(無限量)
                {
                    res.resp_code = "0000";
                    return res;
                }

                if (pkg.item[0].position.result != "0000")
                {
                    res.resp_code = "1003";
                    res.resp_msg = "库存不足";
                }
                else
                {
                    foreach (var item in tripOrder.items)
                    {
                        BookingProdInfo bookingInfo = this.fullBookingProdInfo(tripOrder?.sequenceId, item);
                        if (bookingInfo == null) {
                            res.resp_code = "1001";
                            res.resp_msg = "产品PLU不存在/错误";
                            return res;
                        }

                        //position.item_remain_qty	int	當inventory_set=1與inventory_type=0時，商品總量  {"position":{"result":"0000","result_msg":"OK","item_remain_qty":164}}
                        if (inventory_set == 1 && inventory_type == 0)
                        {
                            if (item.quantity <= pkg.item[0].position.item_remain_qty)
                            {
                                //OK
                                chkLoop = true;
                            }
                            else
                            {
                                chkLoop = false;
                                break;
                            }
                        }

                        //position.itemCal_qty object 當inventory_set = 1與inventory_type = 1時，商品數量依照日期    {"position":{"itemCal_qty":[{"date":"2021-11-21","remain_qty":{"fullday":20}}]}}
                        else if (inventory_set == 1 && inventory_type == 1)
                        {
                            //eric說不會event！
                            var selDate = pkg.item[0].position.itemCal_qty.Where(x => x.date.Equals(item.useStartDate)).FirstOrDefault();
                            if (selDate != null)
                            {
                                var qty = selDate.remain_qty["fullday"];

                                if (qty != null)
                                {
                                    if (item.quantity <= Convert.ToInt32(qty))
                                    {
                                        //OK
                                        chkLoop = true;
                                    }
                                    else
                                    {
                                        chkLoop = false;
                                        break;
                                    }
                                }
                            }
                        }

                        //position.sku_remain_qty object 當inventory_set = 2與inventory_type = 0時，每個sku的總量 {"position":{"result":"0000","result_msg":"OK","sku_remain_qty":[{"sku_id":"98a0c50dc9e484426315734388a17b35","remain_qty":300}]}}
                        else if (inventory_set == 2 && inventory_type == 0)
                        {
                            var sku_remain_qty = pkg.item[0].position.sku_remain_qty;
                            foreach (var sku in sku_remain_qty)
                            {
                                if (sku.sku_id.Equals(bookingInfo.sku_oid))
                                {
                                    if (item.quantity <= sku.remain_qty)
                                    {
                                        chkLoop = true;
                                    }
                                    else
                                    {
                                        chkLoop = false;
                                        break;
                                    }
                                }
                            }
                        }

                        //position.skuCal_qty	object	當inventory_set=2與inventory_type=1時，每個sku的日期數量  {"position":{"skuCal_qty":[{"sku_id":"c318d78b33f57416a0c578e8921cc875","sku_cal":[{"date":"2021-11-21","remain_qty":{"16:00":30}}]}]}}
                        else if (inventory_set == 2 && inventory_type == 1)
                        {
                            var skuCal_qty = pkg.item[0].position.skuCal_qty;
                            foreach (var sku in skuCal_qty)
                            {
                                if (sku.sku_id.Equals(bookingInfo.sku_oid))
                                {
                                    chkLoop = true;
                                    var selDate = sku.sku_cal.Where(x => x.date.Equals(item.useStartDate)).FirstOrDefault();
                                    if (selDate != null)
                                    {
                                        var qty = selDate.remain_qty["fullday"];

                                        if (qty != null)
                                        {
                                            if (item.quantity <= Convert.ToInt32(qty))
                                            {
                                                //OK
                                                chkLoop = true;
                                            }
                                            else
                                            {
                                                chkLoop = false;
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (chkLoop == false)
                {
                    Website.Instance.logger.Info($"chkPosition 位控驗證失敗! guid={tripOrder?.sequenceId}");
                    res.resp_code = "1003";
                    res.resp_msg = "库存不足";
                }
                return res;
            }
            catch (Exception ex)
            {
                //slack 通知
                Website.Instance.logger.Fatal($"chkPosition 位控驗證失敗! guid={tripOrder?.sequenceId} ex:{ex?.Message?.ToString()},{ex?.StackTrace?.ToString()}");
                res.resp_code = "1003";
                res.resp_msg = "库存不足";
                return res;
            }
        }

        //確認價格
        private ResponeObj chkPrice(CreateOrderReqModel tripOrder, PackageRespModel pkg)
        {
            ResponeObj res = new ResponeObj();
            res.resp_code = "0000";
            Boolean chkLoop = false;
            try
            {
                //依據eric提obj.items.cost 為b2d的價格比對依據
                //eric說不會有event!
                //沒有場次的金額{"2021-11-21":{"b2b_price":{"fullday":170},"b2c_price":{"fullday":200}}}
                //有場次{"2021-11-21":{"b2b_price":{"09:00":508,"14:00":508},"b2c_price":{"09:00":535,"14:00":535}}}
                //要確認 1.有位控 2.日期必須要有金額必須相同 
                foreach (var bookingItem in tripOrder.items)
                {
                    BookingProdInfo bookingInfo = this.fullBookingProdInfo(tripOrder?.sequenceId, bookingItem);
                    if (bookingInfo == null)
                    {
                        res.resp_code = "1001";
                        res.resp_code = "产品PLU不存在/错误";
                        return res;
                    }
                    var sku = pkg.item[0].skus.Where(x => x.sku_id.Equals(bookingInfo.sku_oid))?.FirstOrDefault();

                    if (sku != null)
                    {
                        var b2dPrice = Convert.ToDouble(sku.calendar_detail[tripOrder.items[0].useStartDate]?["b2b_price"]?["fullday"]);
                        // 比對金額!
                        if (b2dPrice == tripOrder.items[0]?.cost)
                        {
                            chkLoop = true;
                        }
                        else
                        {
                            chkLoop = false;
                            break;
                        }
                    }
                }

                if (chkLoop == false)
                {
                    Website.Instance.logger.Info($"chkPosition 價格驗證失敗! guid={tripOrder?.sequenceId}");
                    res.resp_code = "1007";
                    res.resp_msg = "产品价格不存在";
                }

                return res;
            }
            catch (Exception ex)
            {
                //slack 通知
                Website.Instance.logger.Fatal($"chkPrice 價格驗證失敗! guid={tripOrder?.sequenceId} ex:{ex?.Message?.ToString()},{ex?.StackTrace?.ToString()}");
                res.resp_code = "1007";
                res.resp_msg = "产品价格不存在";
                return res;
            }

        }


        //確認旅規，目前必須不能有任何要填的旅規 -20211117-eric
        private ResponeObj chkBookingField(string guid,ProductRespModel prod, PackageRespModel pkg)
        {
            ResponeObj res = new ResponeObj();
            res.resp_code = "0000";

            try
            {
                BookingFieldModel bookingFiled = prod.booking_field;
                //找出這個pkg要填的
                var RefPkgTemp1 = bookingFiled.RefPkg.Where(x => x.Key.Equals("")).FirstOrDefault();
                RefPkgAttribute RefPkgTemp2 = RefPkgTemp1.Value;

                if (RefPkgTemp2 != null)
                {
                    Website.Instance.logger.Info($"chkBookingField 旅規不符! guid={guid}");
                    res.resp_code = "1102";
                    res.resp_msg = "旅規不符";
                    return res;
                }

                return res;
            }
            catch (Exception ex)
            {
                Website.Instance.logger.Fatal($"chkBookingField 旅規不符! guid={guid},ex:{ex?.Message?.ToString()},{ex?.StackTrace?.ToString()}");
                res.resp_code = "1102";
                res.resp_msg = "旅規不符";
                return res;
            }

            //foreach (var cusType in RefPkgTemp2.customer.cus_type)
            //{
            //    if (cusType == "cus_02") //依個人
            //    {
            //        if (bookingFiled?.Custom?.EnglishFirstName?.IsRequire == "True")
            //        {
            //        }
            //        if (bookingFiled?.Custom?.EnglishLastName?.IsRequire == "True")
            //        {
            //        }
            //        if (bookingFiled?.Custom?.NativeFirstName?.IsRequire == "True")
            //        {
            //        }
            //        if (bookingFiled?.Custom?.NativeLastName?.IsRequire == "True")
            //        {
            //        }
            //        if (bookingFiled?.Custom?.Birth?.IsRequire == "True")
            //        {
            //        }
            //        if (bookingFiled?.Custom?.GuideLang?.IsRequire == "True")
            //        {
            //        }
            //        if (bookingFiled?.Custom?.MtpNo?.IsRequire == "True")
            //        {
            //        }
            //        if (bookingFiled?.Custom?.IdNo?.IsRequire == "True")
            //        {
            //        }
            //        if (bookingFiled?.Custom?.PassportNo?.IsRequire == "True")
            //        {
            //        }
            //        if (bookingFiled?.Custom?.PassportExpdate?.IsRequire == "True")
            //        {
            //        }
            //        if (bookingFiled?.Custom?.Height?.IsRequire == "True")
            //        {
            //        }
            //        if (bookingFiled?.Custom?.HeightUnit?.IsRequire == "True")
            //        {
            //        }
            //        if (bookingFiled?.Custom?.Weight?.IsRequire == "True")
            //        {
            //        }
            //        if (bookingFiled?.Custom?.WeightUnit?.IsRequire == "True")
            //        {
            //        }
            //        if (bookingFiled?.Custom?.Shoe?.IsRequire == "True")
            //        {
            //        }
            //        if (bookingFiled?.Custom?.ShoeType?.IsRequire == "True")
            //        {
            //        }
            //        if (bookingFiled?.Custom?.ShoeUnit?.IsRequire == "True")
            //        {
            //        }
            //        if (bookingFiled?.Custom?.GlassDegree?.IsRequire == "True")
            //        {
            //        }
            //        if (bookingFiled?.Custom?.Meal?.IsRequire == "True")
            //        {
            //        }
            //        //allergy_food": null

            //    }
            //    if (cusType == "cus_01") //依代表人
            //    {
            //        if (bookingFiled?.Custom?.EnglishFirstName?.IsRequire == "True")
            //        {
            //        }
            //        if (bookingFiled?.Custom?.EnglishLastName?.IsRequire == "True")
            //        {
            //        }
            //        if (bookingFiled?.Custom?.NativeFirstName?.IsRequire == "True")
            //        {
            //        }
            //        if (bookingFiled?.Custom?.NativeLastName?.IsRequire == "True")
            //        {
            //        }
            //        if (bookingFiled?.Custom?.Birth?.IsRequire == "True")
            //        {
            //        }
            //        if (bookingFiled?.Custom?.GuideLang?.IsRequire == "True")
            //        {
            //        }
            //    }
            //    if (cusType == "send") //寄送地
            //    {
            //        if (bookingFiled?.Custom?.NativeFirstName?.IsRequire == "True")
            //        {
            //        }
            //        if (bookingFiled?.Custom?.NativeLastName?.IsRequire == "True")
            //        {
            //        }
            //        if (bookingFiled?.Custom?.Address?.IsRequire == "True")
            //        {
            //        }
            //        if (bookingFiled?.Custom?.CountryCities?.IsRequire == "True")
            //        {
            //        }
            //        if (bookingFiled?.Custom?.Zipcode?.IsRequire == "True")
            //        {
            //        }
            //        if (bookingFiled?.Custom?.TelCountryCode?.IsRequire == "True")
            //        {
            //        }
            //        if (bookingFiled?.Custom?.TelNumber?.IsRequire == "True")
            //        {
            //        }
            //    }
            //    if (cusType == "contact") //聯絡人
            //    {

            //        if (bookingFiled?.Custom?.NativeFirstName?.IsRequire == "True")
            //        {
            //        }
            //        if (bookingFiled?.Custom?.NativeLastName?.IsRequire == "True")
            //        {
            //        }
            //        if (bookingFiled?.Custom?.ContactApp?.IsRequire == "True")
            //        {
            //            //不能下訂
            //        }
            //        if (bookingFiled?.Custom?.ContactAppAccount?.IsRequire == "True")
            //        {
            //            //不能下訂
            //        }
            //        if (bookingFiled?.Custom?.TelCountryCode?.IsRequire == "True")
            //        {
            //        }
            //        if (bookingFiled?.Custom?.TelNumber?.IsRequire == "True")
            //        {
            //        }
            //    }
            //}

            //if (prod.booking_field?.Traffic?.Car?.TrafficType?.IsRequire == "True")
            //{
            //    //不能下訂
            //}

            //if (prod.booking_field?.MobileDevice?.Imei?.IsRequire == "True")
            //{
            //    //不能下訂
            //}
            //if (prod.booking_field?.MobileDevice?.MobileModelNo?.IsRequire == "True")
            //{
            //    //不能下訂
            //}
            //if (prod.booking_field?.MobileDevice?.ActiveDate?.IsRequire == "True")
            //{
            //    //不能下訂
            //}

        }

        //滿足
        private BookingDataModel PrepareBookingModel(CreateOrderReqModel tripOrder, ProductRespModel prod , PackageRespModel pkg)
        {
            BookingDataModel bookingData = new BookingDataModel();
            List<BookingDataCustomModel> custom = new List<BookingDataCustomModel>();
            List<BookingDataTrafficModel> traffic = new List<BookingDataTrafficModel>();

            bookingData.custom = custom;
            bookingData.traffic = traffic;

            try
            {
                BookingProdInfo prodInfo = fullBookingProdInfo( tripOrder?.sequenceId, tripOrder.items[0]);


                //帶入商品與套餐資料
                double total_price = 0;
                var skuLst = new List<BookingDataSkuModel>();

                tripOrder.items.ForEach(i => {
                    var sku_id = i.PLU.Split('|')[2];

                    var _sku = pkg.item[0].skus.Where(x => x.sku_id.Equals(sku_id))?.FirstOrDefault();
                    if (_sku != null)
                    {
                        var b2dPrice = Convert.ToDouble(_sku.calendar_detail[tripOrder.items[0].useStartDate]?["b2b_price"]?["fullday"]);
                        total_price += b2dPrice * i.quantity;
                    }

                    skuLst.Add(new BookingDataSkuModel() {
                        price = i.cost,
                        sku_id = sku_id,
                        qty = i.quantity
                    });
                });

                bookingData.guid = pkg.guid;
                bookingData.partner_order_no = Guid.NewGuid().ToString().Substring(0,15);
                bookingData.prod_no = prod.prod.prod_no;
                bookingData.prod_name = prod.prod.prod_name;
                bookingData.pkg_no = pkg.pkg_no;
                bookingData.pkg_name = pkg.pkg_name;
                bookingData.item_no = pkg.item_no[0];
                bookingData.s_date = tripOrder.items[0].useStartDate;
                bookingData.e_date = tripOrder.items[0].useEndDate;
                bookingData.event_time = null;
                bookingData.total_price = total_price;
                bookingData.ota_pricee = tripOrder.items.Sum(x => x.quantity * x.cost);
                bookingData.order_note = tripOrder.items[0].remark;
                // 帶入訂購人資訊
                bookingData.buyer_country = "CN";
                bookingData.buyer_email = tripOrder.contacts[0].email;

                if (tripOrder.contacts[0].name.Split(' ').Count() > 1)
                {
                    bookingData.buyer_first_name = tripOrder.contacts[0].name.Split(' ')[1];
                    bookingData.buyer_last_name = tripOrder.contacts[0].name.Split(' ')[0];
                }
                else
                {
                    bookingData.buyer_first_name = tripOrder.contacts[0].name.Substring(1, tripOrder.contacts[0].name.Length -1);
                    bookingData.buyer_last_name = tripOrder.contacts[0].name.Substring(0,1);
                }

                bookingData.buyer_tel_number= tripOrder.contacts[0].mobile;
                bookingData.buyer_tel_country_code = tripOrder.contacts[0].intlCode; 
                bookingData.skus = skuLst;

                BookingDataPaymentModel pay = new BookingDataPaymentModel();
                pay.type = "01";
                bookingData.pay = pay;

            }
            catch (Exception ex)
            {
                Website.Instance.logger.Fatal($"fullBookingModel 滿足bookingData異常! guid={tripOrder?.sequenceId},ex:{ex?.Message?.ToString()},{ex?.StackTrace?.ToString()}");
                bookingData = null;
            }

            return bookingData;
        }

        //解析 prod_oid pkg_oid skus 
        private BookingProdInfo fullBookingProdInfo(string guid, CreateOrderReqModel.OrderItemModel item)
        {
            try
            {
                ResponeObj res = new ResponeObj();
                BookingProdInfo bookingInfo = new BookingProdInfo();
                var prodInfoTemp = item?.PLU?.ToString()?.Split('|');
                if (prodInfoTemp.Count() > 2)
                {
                    bookingInfo.prod_oid = prodInfoTemp[0];
                    bookingInfo.pkg_oid = prodInfoTemp[1];
                    bookingInfo.sku_oid = prodInfoTemp[2];
                    return bookingInfo;
                }
                else
                {
                    Website.Instance.logger.Fatal($"fullBookingProdInfo 滿足BookingProdInfo異常1! guid={guid}item ={ JsonConvert.SerializeObject(item)}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Website.Instance.logger.Fatal($"fullBookingProdInfo 滿足BookingProdInfo異常2! guid={guid}item ={ JsonConvert.SerializeObject(item)},ex:{ex?.Message?.ToString()} ,{ex?.StackTrace?.ToString()}");
                return null;
            }
            
        }
    }
}
