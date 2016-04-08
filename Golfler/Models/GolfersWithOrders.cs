using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Golfler.Models
{
    public class GolfersWithOrders
    {
        public Nullable<long> Id { get; set; }
        public string Name { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public string type { get; set; }
        public string Hexa { get; set; }
        public string RGB { get; set; }
        public string HUE { get; set; }
        public string Admin_Name { get; set; }
        public string Admin_Latitude { get; set; }
        public string Admin_Longitude { get; set; }
        public string Admin_type { get; set; }
        public string Order_Details { get; set; }
    }
}