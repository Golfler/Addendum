using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GolferWebAPI.Models
{
    public class Base
    {
        public int Status { get; set; }
        public string Error { get; set; }
    }

    public class ResultOrder : Base
    {
        public bool GolferLoginStatus { get; set; }
        public long Id { get; set; }
        public object record { get; set; }
    }

    public class Result : Base
    {
        public long Id { get; set; }
        public object record { get; set; }
    }
    public class MessageCountResult : Base
    {
        public int Status { get; set; }
        public string Error { get; set; }
        public int UserCount { get; set; }
    }
    public class MessageResult : Base
    {
        public IEnumerable<object> Messages { get; set; }
    }
    public class FriendsResult : Base
    {
        public long Id { get; set; }
        public string FriendType { get; set; }
        public string UserType { get; set; }
        public IEnumerable<object> Friends { get; set; }
    }
    public class GolferMapResult : Base
    {
        //Veera Says No idea if i change ienumerable to simple object
        public object Golfers { get; set; }
    }

    public partial class GF_SP_GetGolfersListingByCourseId_Result
    {
        public string CurrentHole = "";
        public string CurrentHoleTime = "";
        public string TotalTimeSpend = "";
        public Dictionary<string, string> HoleTimings = new Dictionary<string, string>();
    }
}