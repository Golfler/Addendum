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
    
    public partial class GF_CourseVisitLog
    {
        public long ID { get; set; }
        public Nullable<long> GolferID { get; set; }
        public Nullable<long> CourseID { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public Nullable<System.DateTime> ModifyDate { get; set; }
    
        public virtual GF_CourseInfo GF_CourseInfo { get; set; }
        public virtual GF_Golfer GF_Golfer { get; set; }
    }
}
