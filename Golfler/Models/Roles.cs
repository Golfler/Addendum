using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Collections.Generic;

namespace Golfler.Models
{
    /// <summary>
    /// Created By: Renuka Hira
    /// Created on: 1st July, 2013
    /// </summary>
    /// <remarks>MetaData and partial classes for validation</remarks>
    [MetadataType(typeof(RolesMetaData))]
    public partial class GF_Roles
    {
        public bool Active { get { return Status == StatusType.Active; } set { Status = (value) ? Status = StatusType.Active : Status = StatusType.InActive; } }

        public GF_RoleModules[] ArrRoleModules { get; set; }
    }

    class RolesMetaData
    {
        [Required(ErrorMessage = "Required")]
        [DisplayName("Role Name")]
        [RegularExpression(RegularExp.AlphaNumeric, ErrorMessage = "Role Name only contains alphabets and numbers")]
        [StringLength(50, ErrorMessage = "Role Name cannot be longer than 500 characters.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Required")]
        [DisplayName("Description")]
        [StringLength(500, ErrorMessage = "Description cannot be longer than 500 characters.")]
        [DefaultValue("")]
        public string Description { get; set; }

        [DisplayName("Is Active")]
        public bool Active { get; set; }
    }

    public partial class GF_RoleModules
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public bool Visible { get {
            return (ReadFlag || AddFlag || DeleteFlag || UpdateFlag);
        }
        }
        public bool IsSpecial { get; set; }
        public bool IsBasic { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsFrontEnd { get; set; }
        public string ModuleGroupName { get; set; }
    }
}