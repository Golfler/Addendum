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
    
    public partial class GF_OrderMenuOptionDetail
    {
        public long ID { get; set; }
        public Nullable<long> OrderDetailID { get; set; }
        public Nullable<long> MenuOptionID { get; set; }
        public string MenuOptionName { get; set; }
    
        public virtual GF_MenuItemOption GF_MenuItemOption { get; set; }
        public virtual GF_OrderDetails GF_OrderDetails { get; set; }
    }
}
