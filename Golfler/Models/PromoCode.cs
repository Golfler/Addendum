using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Newtonsoft.Json.Linq;

namespace Golfler.Models
{
    /// <summary>
    /// Created By: Amit Kumar
    /// Created on: 01 April 2015
    /// </summary>
    /// <remarks>MetaData and partial classes for validation</remarks>
    [MetadataType(typeof(PromoCodeMetaData))]
    public partial class GF_PromoCode
    {
        public bool Active { get { return Status == StatusType.Active; } set { Status = (value) ? Status = StatusType.Active : Status = StatusType.InActive; } }
        public bool OneTimeUse { get { return IsOneTimeUse == null ? false : (IsOneTimeUse ?? false); } set { IsOneTimeUse = value; } }
    }

    class PromoCodeMetaData
    {
        [AllowHtml]
        [Required(ErrorMessage = "Required")]
        [DataType(DataType.Text)]
        [Display(Name = "Promo Code")]
        [RegularExpression(RegularExp.AlphaNumeric, ErrorMessage = "Please enter alphabet numeric")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string PromoCode { get; set; }

        [Display(Name = "Discount")]
        [Required(ErrorMessage = "Required")]
        [RegularExpression(RegularExp.Numeric, ErrorMessage = "Please enter valid discount.")]
        public string Discount { get; set; }

        [Display(Name = "Expiry Date")]
        [Required(ErrorMessage = "Required")]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime ExpiryDate { get; set; }

        [Display(Name = "Food Item")]
        [Required(ErrorMessage = "Required")]
        public long ReferenceID { get; set; }

        [Display(Name = "Reference Type")]
        public long ReferenceType { get; set; }

        [DisplayName("Is Active")]
        public bool Active { get; set; }

        [DisplayName("One Time Use")]
        public bool IsOneTimeUse { get; set; }

        [Display(Name = "Discount Type")]
        [Required(ErrorMessage = "Required")]
        public string DiscountType { get; set; }
    }
}