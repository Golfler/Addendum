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
    
    public partial class GF_Roles
    {
        public GF_Roles()
        {
            this.GF_AdminUsers = new HashSet<GF_AdminUsers>();
            this.GF_RoleModules = new HashSet<GF_RoleModules>();
        }
    
        public long ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public long CreatedBy { get; set; }
        public System.DateTime CreatedOn { get; set; }
        public Nullable<long> ModifiedBy { get; set; }
        public Nullable<System.DateTime> ModifiedOn { get; set; }
        public string Status { get; set; }
        public Nullable<long> CourseUserId { get; set; }
    
        public virtual ICollection<GF_AdminUsers> GF_AdminUsers { get; set; }
        public virtual ICollection<GF_RoleModules> GF_RoleModules { get; set; }
    }
}
