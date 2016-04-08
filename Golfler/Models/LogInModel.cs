using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Golfler.Models
{
    /// <summary>
    /// Created By:
    /// Created on:
    /// </summary>
    /// <remarks>Validation class used for validating Admin Login</remarks>
    public class LogInModel
    {
        [AllowHtml]
        [RegularExpression(RegularExp.AlphaNumeric, ErrorMessage = "Please enter a valid username.")]
        [Required(ErrorMessage = "Required")]
        [DataType(DataType.Text)]
        [Display(Name = "User Name")]
        public string UserName { get; set; }

        [AllowHtml]
        [RegularExpression(RegularExp.Email, ErrorMessage = "Please enter a valid email.")]
        [DataType(DataType.Text)]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [AllowHtml]
        [RegularExpression(@"^[^<>]*$", ErrorMessage = "Please enter a valid password.")]
        [Required(ErrorMessage = "Required")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Remember me on this Computer")]
        public bool KeepMeLogin { get; set; }

        public string UserType { get; set; }

        public string IpAddress { get; set; }

        public long CourseID { get; set; }
    }

    /// <summary>
    /// Created By:
    /// Created on:
    /// </summary>
    /// <remarks>Model for forgot screen</remarks>
    public class ForgotModel
    {
        [Required(ErrorMessage = "Required")]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email Address")]
        [RegularExpression(@"^[\w-\.]{1,}\@([\da-zA-Z-]{1,}\.){1,}[\da-zA-Z-]{2,3}$", ErrorMessage = "Please enter valid email.")]
        public string Email { get; set; }
    }
}