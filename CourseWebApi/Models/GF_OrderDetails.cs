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
    
    public partial class GF_OrderDetails
    {
        public GF_OrderDetails()
        {
            this.GF_OrderMenuOptionDetail = new HashSet<GF_OrderMenuOptionDetail>();
        }
    
        public long ID { get; set; }
        public Nullable<long> OrderID { get; set; }
        public Nullable<long> MenuItemID { get; set; }
        public Nullable<int> Quantity { get; set; }
        public Nullable<decimal> UnitPrice { get; set; }
        public Nullable<decimal> CostPrice { get; set; }
    
        public virtual GF_MenuItems GF_MenuItems { get; set; }
        public virtual GF_Order GF_Order { get; set; }
        public virtual ICollection<GF_OrderMenuOptionDetail> GF_OrderMenuOptionDetail { get; set; }
    }
}
