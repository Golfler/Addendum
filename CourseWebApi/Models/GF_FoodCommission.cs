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
    
    public partial class GF_FoodCommission
    {
        public long ID { get; set; }
        public Nullable<long> CourseID { get; set; }
        public Nullable<long> CategoryID { get; set; }
        public Nullable<decimal> Commission { get; set; }
    
        public virtual GF_Category GF_Category { get; set; }
        public virtual GF_CourseInfo GF_CourseInfo { get; set; }
    }
}