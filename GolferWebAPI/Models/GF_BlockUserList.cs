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
    
    public partial class GF_BlockUserList
    {
        public long ID { get; set; }
        public Nullable<long> UserId { get; set; }
        public Nullable<long> BlockedUserId { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public Nullable<bool> IsGolferUser { get; set; }
        public Nullable<bool> IsBlockedGolferUser { get; set; }
    }
}
