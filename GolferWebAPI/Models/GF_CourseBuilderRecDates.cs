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
    
    public partial class GF_CourseBuilderRecDates
    {
        public long RecDateId { get; set; }
        public Nullable<System.DateTime> RecDate { get; set; }
        public Nullable<long> CourseBuilderId { get; set; }
        public Nullable<long> CourseId { get; set; }
        public string Status { get; set; }
    }
}
