//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Golfler.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class GF_ResolutionMessageHistory
    {
        public long ID { get; set; }
        public Nullable<long> MessageID { get; set; }
        public string Message { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public string Status { get; set; }
        public Nullable<bool> IsActive { get; set; }
        public Nullable<long> LogUserID { get; set; }
        public string UserType { get; set; }
    
        public virtual GF_ResolutionCenter GF_ResolutionCenter { get; set; }
    }
}
