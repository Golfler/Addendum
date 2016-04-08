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
    
    public partial class GF_Courses
    {
        public GF_Courses()
        {
            this.GF_CourseUsers = new HashSet<GF_CourseUsers>();
        }
    
        public long ID { get; set; }
        public string Name { get; set; }
        public Nullable<long> TemplateID { get; set; }
        public string TemplateLogo { get; set; }
        public Nullable<long> MembershipLevel { get; set; }
        public Nullable<long> ParentOrganization { get; set; }
        public string ContactName { get; set; }
        public string ConatctNameLast { get; set; }
        public string ContactDesignation { get; set; }
        public string ContactPhone { get; set; }
        public string ContactMail { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string Country { get; set; }
        public string State { get; set; }
        public string City { get; set; }
        public string Zipcode { get; set; }
        public string Description { get; set; }
        public Nullable<bool> EndUserRegistration { get; set; }
        public Nullable<bool> AppPurchase { get; set; }
        public Nullable<bool> Review { get; set; }
        public Nullable<bool> CreateChild { get; set; }
        public Nullable<bool> ApproveEndUser { get; set; }
        public Nullable<bool> EnrollDecvice { get; set; }
        public Nullable<bool> GlobalAds { get; set; }
        public string AciveDirectory { get; set; }
        public string AppStoreLink { get; set; }
        public Nullable<bool> IsMdMEnabled { get; set; }
        public string MDMUrl { get; set; }
        public string UserAuthencation { get; set; }
        public Nullable<long> CreatedBy { get; set; }
        public Nullable<System.DateTime> CreatedOn { get; set; }
        public Nullable<long> ModifiedBy { get; set; }
        public Nullable<System.DateTime> ModifiedOn { get; set; }
        public string Status { get; set; }
        public string BannerImage { get; set; }
        public string TenantId { get; set; }
        public Nullable<bool> showLogo { get; set; }
        public Nullable<System.DateTime> ADSyncDate { get; set; }
        public Nullable<bool> CopyMenus { get; set; }
        public Nullable<bool> CopyEmail { get; set; }
        public Nullable<bool> CopyCategories { get; set; }
        public Nullable<bool> CopyApps { get; set; }
        public Nullable<bool> CopyAppCategories { get; set; }
        public string ADUsername { get; set; }
        public string ADPassword { get; set; }
    
        public virtual ICollection<GF_CourseUsers> GF_CourseUsers { get; set; }
    }
}