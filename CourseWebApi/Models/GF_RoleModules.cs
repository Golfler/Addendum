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
    
    public partial class GF_RoleModules
    {
        public long ID { get; set; }
        public Nullable<long> RoleID { get; set; }
        public Nullable<long> ModuleID { get; set; }
        public bool AddFlag { get; set; }
        public bool UpdateFlag { get; set; }
        public bool ReadFlag { get; set; }
        public bool DeleteFlag { get; set; }
    
        public virtual GF_Modules GF_Modules { get; set; }
        public virtual GF_Roles GF_Roles { get; set; }
    }
}
