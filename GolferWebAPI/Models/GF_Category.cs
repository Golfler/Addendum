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
    
    public partial class GF_Category
    {
        public GF_Category()
        {
            this.GF_SubCategory = new HashSet<GF_SubCategory>();
            this.GF_FoodCommission = new HashSet<GF_FoodCommission>();
            this.GF_CourseMenu = new HashSet<GF_CourseMenu>();
        }
    
        public long ID { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public Nullable<bool> IsActive { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public Nullable<System.DateTime> ModifyDate { get; set; }
        public string ModifyBy { get; set; }
        public string Type { get; set; }
        public Nullable<int> DisplayOrder { get; set; }
        public string CartType { get; set; }
    
        public virtual ICollection<GF_SubCategory> GF_SubCategory { get; set; }
        public virtual ICollection<GF_FoodCommission> GF_FoodCommission { get; set; }
        public virtual ICollection<GF_CourseMenu> GF_CourseMenu { get; set; }
    }
}