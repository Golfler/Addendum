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
    
    public partial class GF_Feedback
    {
        public long ID { get; set; }
        public Nullable<long> CourseID { get; set; }
        public Nullable<long> GolferID { get; set; }
        public string FeedbackTest { get; set; }
        public string SendTo { get; set; }
        public Nullable<long> CartieID { get; set; }
        public Nullable<bool> IsRead { get; set; }
        public string Status { get; set; }
        public Nullable<bool> IsActive { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public Nullable<long> CreatedBy { get; set; }
        public Nullable<System.DateTime> ModifyDate { get; set; }
        public Nullable<long> ModifyBy { get; set; }
    
        public virtual GF_CourseInfo GF_CourseInfo { get; set; }
        public virtual GF_Golfer GF_Golfer { get; set; }
    }
}
