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
    
    public partial class GF_MenuItemOption
    {
        public GF_MenuItemOption()
        {
            this.GF_OrderMenuOptionDetail = new HashSet<GF_OrderMenuOptionDetail>();
            this.GF_CourseFoodItemOption = new HashSet<GF_CourseFoodItemOption>();
        }
    
        public long ID { get; set; }
        public Nullable<long> MenuItemID { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public Nullable<long> CourseID { get; set; }
    
        public virtual GF_MenuItems GF_MenuItems { get; set; }
        public virtual ICollection<GF_OrderMenuOptionDetail> GF_OrderMenuOptionDetail { get; set; }
        public virtual ICollection<GF_CourseFoodItemOption> GF_CourseFoodItemOption { get; set; }
    }
}
