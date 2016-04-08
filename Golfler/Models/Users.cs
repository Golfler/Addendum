using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;

namespace Golfler.Models
{
    /// <summary>
    /// Created By:
    /// Created on:
    /// </summary>
    /// <remarks>MetaData and partial classes for validation</remarks>
    [MetadataType(typeof(UsersMetaData))]
    public partial class GF_AdminUsers
    {
        public bool Active { get { return Status == StatusType.Active; } set { Status = (value) ? Status = StatusType.Active : Status = StatusType.InActive; } }
        public bool IsCourseUser { get; set; }
        public bool Select { get; set; }
        public string Name { get { return FirstName + " " + LastName; } }
        public string RoleName { get { return GF_Roles == null ? new GF_Roles().Name = "" : GF_Roles.Name; } }

        public string UserType { get; set; }
        public string Role { get; set; }
        public string CourseName { get; set; }

        public string RePassword { get { return Password == null ? null : Password; } set { } }
    }

    class UsersMetaData
    {
        [AllowHtml]
        [Required(ErrorMessage = "Required")]
        [DataType(DataType.Text)]
        [Display(Name = "Username")]
        [StringLength(25, MinimumLength = 5, ErrorMessage = "Minimum length of user name is 5.")]
        [RegularExpression(RegularExp.AlphaNumeric, ErrorMessage = "Please enter alphabet numeric")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string UserName { get; set; }

        [AllowHtml]
        [RegularExpression(@"^[^<>]*$", ErrorMessage = "Please Enter Valid {0}!!")]
        [Required(ErrorMessage = "Required")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Password { get; set; }

        [System.Web.Mvc.Compare("Password", ErrorMessage = "{1} and {0} do not match.")]
        [Required(ErrorMessage = "Required")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        public string RePassword { get; set; }

        [Display(Name = "First Name")]
        [Required(ErrorMessage = "Required")]
        [RegularExpression(RegularExp.Alphabets, ErrorMessage = "Please enter alphabet.")]
        [StringLength(50, ErrorMessage = "First name cannot be greater than 50 character long.")]
        public string FirstName { get; set; }

        [Display(Name = "Last Name")]
        [Required(ErrorMessage = "Required")]
        [RegularExpression(RegularExp.Alphabets, ErrorMessage = "Please enter alphabet.")]
        [StringLength(50, ErrorMessage = "Last name cannot be greater than 50 character long.")]
        public string LastName { get; set; }

        [Display(Name = "Email")]
        [Required(ErrorMessage = "Required")]
        [StringLength(200, ErrorMessage = "Email cannot be greater than 200 character long.")]
        [RegularExpression(RegularExp.Email, ErrorMessage = "Please enter valid email address.")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Email { get; set; }

        [Display(Name = "Phone")]
        [RegularExpression(RegularExp.PhoneNotReq, ErrorMessage = "Please enter valid phone number.")]
        [StringLength(20, ErrorMessage = "Phone number cannot be greater than 20 character long.")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        [DefaultValue("")]
        public string Phone { get; set; }

        [DisplayName("Role")]
        public long RoleId { get; set; }

        [DisplayName("Is Active")]
        public bool Active { get; set; }

        [Display(Name = "User Type")]
        [Required(ErrorMessage = "Required")]
        public string Type { get; set; }
    }
}
