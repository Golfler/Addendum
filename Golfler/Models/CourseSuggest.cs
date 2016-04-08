using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Golfler.Models
{
    [MetadataType(typeof(CourseSuggest))]
    public partial class GF_CourseSuggest
    {
        public bool Active { get { return Status == StatusType.Active; } set { Status = (value) ? Status = StatusType.Active : Status = StatusType.InActive; } }
    }

    public class CourseSuggest
    {
        public Int64 courseid { get; set; }
        public string coursename { get; set; }
        public Int64 NoOfSuggestions { get; set; }
    }
}