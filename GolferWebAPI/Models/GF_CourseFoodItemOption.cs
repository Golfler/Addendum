//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace GolferWebAPI.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class GF_CourseFoodItemOption
    {
        public long ID { get; set; }
        public Nullable<long> ItemDetailID { get; set; }
        public Nullable<long> MenuItemOptionID { get; set; }
        public string Name { get; set; }
        public Nullable<bool> IsActive { get; set; }
        public Nullable<long> CourseID { get; set; }
    
        public virtual GF_CourseFoodItemDetail GF_CourseFoodItemDetail { get; set; }
        public virtual GF_CourseInfo GF_CourseInfo { get; set; }
        public virtual GF_MenuItemOption GF_MenuItemOption { get; set; }
    }
}