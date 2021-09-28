﻿using System;
using System.Collections.Generic;

namespace Trip.PasvAPI.Models.Model
{
    public partial class PackageSkuModel : PackageModel
    {
        public string guid { get; set; }
        public List<Item> item { get; set; }
    }

    public partial class Item
    {
        public long item_no { get; set; }
        public string unit { get; set; }
        public bool has_event { get; set; }
        public bool is_backup { get; set; }
        public bool last_spec_multi { get; set; }
        public long price_type { get; set; }
        public long price_rule_type { get; set; }
        public string voucher_type { get; set; }
        public UnitQuantityRule unit_quantity_rule { get; set; }
        public long b2c_min_price { get; set; }
        public long b2b_min_price { get; set; }
        public DateTimeOffset sale_s_date { get; set; }
        public string sale_s_date_event { get; set; }
        public DateTimeOffset sale_e_date { get; set; }
        public string sale_e_date_event { get; set; }
        public long inventory_set { get; set; }
        public long inventory_type { get; set; }
        public Skus[] skus { get; set; }
        public Position position { get; set; }
        public Spec[] specs { get; set; }
    }

    public partial class Skus
    {
        public string sku_id { get; set; }
        public string spec_token { get; set; }
        public SpecRef[] specs_ref { get; set; }
        public long official_price { get; set; }
        public long b2c_price { get; set; }
        public long b2b_price { get; set; }
        public string ticket_rule_spec_item { get; set; }
        public Dictionary<string, Dictionary<string, Dictionary<string, object>>> calendar_detail { get; set; }
        public SpecRule spec_rule { get; set; }
        public string spec_desc { get; set; }
    }

    public partial class UnitQuantityRule
    {
        public TotalRule total_rule { get; set; }
        public TicketRule ticket_rule { get; set; }
        public bool is_multiple_limit { get; set; }
    }

    public partial class TicketRule
    {
        public Ruleset[] rulesets { get; set; }
        public bool is_active { get; set; }
    }

    public partial class Ruleset
    {
        public string[] spec_items { get; set; }
        public long max_quantity { get; set; }
        public long min_quantity { get; set; }
        public bool? is_multiple_limit { get; set; }
    }

    public partial class TotalRule
    {
        public long max_quantity { get; set; }
        public long min_quantity { get; set; }
    }

    public partial class PartialRefund
    {
        public double value { get; set; }
        public long day_min { get; set; }
        public long? day_max { get; set; }
    }

    public partial class SpecRule
    {
        public long max_age { get; set; }
        public long min_age { get; set; }
    }

    public partial class SpecRef
    {
        public string spec_item_id { get; set; }
        public string spec_value_id { get; set; }
    }

    public partial class Position
    {
        public string result { get; set; }
        public string result_msg { get; set; }
        public int? item_remain_qty { get; set; }
        public ItemCalQty[] itemCal_qty { get; set; }
        public SkuCalQty[] skuCal_qty { get; set; }
        public SkuRemainQty[] sku_remain_qty { get; set; }

    }

    public partial class ItemCalQty
    {
        public string date { get; set; }
        public Dictionary<string, object> remain_qty { get; set; }
    }
    public partial class SkuCalQty
    {
        public string sku_id { get; set; }
        public SkuCal[] sku_cal { get; set; }
    }
    public partial class SkuCal
    {
        public string date { get; set; }
        public Dictionary<string, int?> remain_qty { get; set; }
    }
    public partial class SkuRemainQty
    {
        public string sku_id { get; set; }//分銷商的sku_oid=sku_id
        public int? remain_qty { get; set; }
    }

    #region Item.Spec 定義 

    public partial class Spec
    {
        public string spec_oid { get; set; }
        public string spec_title { get; set; }
        public SpecItem[] spec_items { get; set; }
    }

    public partial class SpecItem
    {
        public string name { get; set; }
        public string spec_item_oid { get; set; }
        public Rule rule { get; set; }
    }

    public partial class Rule
    {
        public AgeRule age_rule { get; set; }
        public HeightRule height_rule { get; set; }
    }

    public partial class AgeRule
    {
        public long? min { get; set; }
        public long? max { get; set; }
    }

    public partial class HeightRule
    {
        public int? min { get; set; }
        public int? max { get; set; }
    }

    #endregion Item.Spec 定義 
}
