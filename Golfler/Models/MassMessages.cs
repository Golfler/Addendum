using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Golfler.Models
{
    public class MassMessages
    {
        public Int64 Id { get; set; }
        public Int64 userid { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Type { get; set; }
        public string CourseName { get; set; }
    }
}