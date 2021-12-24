using System;
namespace Trip.PasvAPI.Models.Model
{
     public class StaffModel
    {
        public Int64 staff_oid { get; set; }
        public string staff_account { get; set; }
        public string staff_name { get; set; }
        public string staff_type { get; set; }
        public string type_name { get; set; }
        public string staff_status { get; set; }
        public string status_name { get; set; }
        public string staff_password { get; set; }
        public string create_user { get; set; }
        public DateTime? create_date { get; set; }
        public string modify_user { get; set; }
        public DateTime? modify_date { get; set; }
    }
}
