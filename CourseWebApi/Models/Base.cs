using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CourseWebApi.Models
{
    public class Base
    {
        public int Status { get; set; }
        public string Error { get; set; }
    }

    public class Result : Base
    {
        public long Id { get; set; }
        public object record { get; set; }
    }

    public class CourseOrderResult : Result
    {
        public object CourseUser { get; set; }
    }

    public class PageResult : Result
    {
        public long totalRecords { get; set; }
        public long pageCount { get; set; }
    }

    public class CourseLoginResult : Base
    {
        public long? Id { get; set; }
        public object record { get; set; }
    }

    public class OrderResult : Base
    {
        public IEnumerable<object> Orders { get; set; }
    }
    public class WeatherResult : Base
    {
        public List<weatherList> weather { get; set; }
        public List<mainList> main { get; set; }
        public List<windList> wind { get; set; }
       
    }
    public class weatherList
    {
        public string main { get; set; }
        public string description { get; set; }
      
    }
    public class mainList
    {
        public string temp { get; set; }
        public string temp_min { get; set; }
        public string temp_max { get; set; }
        public string pressure { get; set; }
        public string humidity { get; set; }
      
    }
    public class windList
    {
        public string speed { get; set; }
    }

    public class pushNotificationResult
    {
        public string ScreenName { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }
    }
}