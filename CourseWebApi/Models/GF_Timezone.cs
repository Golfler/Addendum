//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace CourseWebApi.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class GF_Timezone
    {
        public short timezone_id { get; set; }
        public string timezone_standard_identifier { get; set; }
        public string display_name { get; set; }
        public string standard_name { get; set; }
        public int baseutc_offset { get; set; }
        public string gmt_value { get; set; }
        public string daylight_name { get; set; }
        public Nullable<decimal> gmt_value_forCalculation { get; set; }
    }
}
