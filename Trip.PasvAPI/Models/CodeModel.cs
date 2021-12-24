using System;
using System.Collections.Generic;

namespace Trip.PasvAPI.Models
{
     public class CodeBasicModel 
    {
        public Int64 code_oid { get; set; }
        public string code_type { get; set; }
        public string code_no { get; set; }
        public string sort { get; set; }
        public string create_user { get; set; }
        public DateTime create_date { get; set; }
        public string modify_user { get; set; }
        public DateTime? modify_date { get; set; }
    }

    public class CodeModel : CodeBasicModel
    {
        public Dictionary<string, string> code_name { get; set; }
    }

    public class CodeInfoModel : CodeBasicModel
    {
        public string code_name { get; set; }
    }

    ///////////////////////////

    public class CodeEssentialModel
    {
        public string code_type { get; set; }
        public string code_no { get; set; }
        public string code_name { get; set; }
        public string sort { get; set; }
    }
}
