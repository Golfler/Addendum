using System.ComponentModel.DataAnnotations;

namespace Golfler.Models
{
    /// <summary>
    /// Created By: Renuka Hira
    /// Created on: 1st July, 2013
    /// </summary>
    /// <remarks>Model for forgot screen</remarks>
    public class ForgotModelAdmin
    {
        [Required(ErrorMessage = "Required")]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email Address")]
        [RegularExpression(@"^[\w-\.]{1,}\@([\da-zA-Z-]{1,}\.){1,}[\da-zA-Z-]{2,3}$", ErrorMessage = "Please enter correct email.")]
        public string Email { get; set; }
        public long CourseID { get; set; }
        public string Type { get; set; }
    }
}