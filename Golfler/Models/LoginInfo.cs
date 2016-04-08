using System;
using System.Collections;
using System.Web;
using System.Web.Mvc;

namespace Golfler.Models
{
    /// <summary>
    /// Created by : Amit
    /// Creation Date : March,2015
    /// Purpose: LoginInfo Class  and properties
    /// </summary>
    public static class LoginInfo
    {
        public static long UserId { get { return Convert.ToInt64(HttpContext.Current.Session["UserId"]); } }
        public static long CourseUserId
        {
            get
            {
                if (HttpContext.Current.Session == null)
                {
                    return Convert.ToInt64(CourseIdWebService);
                }
                else
                {
                    return Convert.ToInt64(HttpContext.Current.Session["AdminCourseId"]);
                }
            }
            set
            {
                if (HttpContext.Current.Session == null)
                {
                    CourseIdWebService = value;
                }
                else
                {
                    HttpContext.Current.Session["AdminCourseId"] = value;
                }
            }
        }
        private static long CourseIdWebService;

        public static long SuperAdminId { get { return Convert.ToInt64(HttpContext.Current.Session["SuperAdminId"]); } set { HttpContext.Current.Session["SuperAdminId"] = value; } }
        public static bool IsSuper { get { return Type.Contains(UserTypeChar.SuperAdminChar); } }
        
        public static string UserName { get { return Convert.ToString(HttpContext.Current.Session["UserName"]); } }
        public static string FirstName { get { return Convert.ToString(HttpContext.Current.Session["FirstName"]); } set { HttpContext.Current.Session["FirstName"] = value; } }
        public static string LastName { get { return Convert.ToString(HttpContext.Current.Session["LastName"]); } set { HttpContext.Current.Session["LastName"] = value; } }
        public static string Type { get { return Convert.ToString(HttpContext.Current.Session["Type"]); } }
        public static string LoginUserType { get { return Type; } }
        public static DateTime? LastLogin { get { return (DateTime?)HttpContext.Current.Session["LastLogin"]; } }
        public static string LastLoginIP { get { return Convert.ToString(HttpContext.Current.Session["LastLoginIP"]).Trim() == "" ? "-" : Convert.ToString(HttpContext.Current.Session["LastLoginIP"]).Trim(); } }

        public static long CourseId { get { return Convert.ToInt64(HttpContext.Current.Session["CourseId"]); } }
        public static string CourseName { get { return Convert.ToString(HttpContext.Current.Session["AdminCourseName"]); } set { HttpContext.Current.Session["AdminCourseName"] = value; } }
        public static string CourseEmail { get { return (string)HttpContext.Current.Session["CourseEmail"]; } }
        
        public static long MasterUserId { get { return Convert.ToInt64(HttpContext.Current.Session["MasterUserId"]); } }
        public static string MasterType { get { return Convert.ToString(HttpContext.Current.Session["MasterType"]); } }
        public static string UserAuthType { get { return Convert.ToString(HttpContext.Current.Session["UserAuthType"]); } }
        public static string LoginType { get { return Convert.ToString(HttpContext.Current.Session["LoginType"]); } }
        public static string UserLoginImage { get { return Convert.ToString(HttpContext.Current.Session["UserLoginImage"]); } }

        internal static void CreateLoginSession(string logintype,
                                                long userId,
                                                long CourseId,
                                                string CourseName,
                                                string email,
                                                string userName, string password,
                                                string firstName,
                                                string lastName,
                                                string type,
                                                DateTime? lastLogin,
                                                string lastLoginIP, bool KeepMeLogin = false,
            long masteruserid = 0, string mastertype = "", bool loginback = false, string userAuthType = "", string loginImage = "")
        {
            HttpContext.Current.Session["LoginType"] = logintype;
            HttpContext.Current.Session["UserId"] = userId;
            HttpContext.Current.Session["AdminCourseId"] = CourseId;
            HttpContext.Current.Session["CourseId"] = CourseId;
            HttpContext.Current.Session["AdminCourseName"] = CourseName;
            HttpContext.Current.Session["CourseEmail"] = email;
            HttpContext.Current.Session["UserName"] = userName;
            HttpContext.Current.Session["FirstName"] = firstName;
            HttpContext.Current.Session["LastName"] = lastName;
            HttpContext.Current.Session["Type"] = type;
            HttpContext.Current.Session["LastLogin"] = lastLogin;
            HttpContext.Current.Session["LastLoginIP"] = lastLoginIP;
            HttpContext.Current.Session["UserAuthType"] = userAuthType;
            HttpContext.Current.Session["UserLoginImage"] = loginImage;

            if (MasterUserId == 0)
            {
                HttpContext.Current.Session["SuperAdminId"] = masteruserid;
                HttpContext.Current.Session["MasterUserId"] = masteruserid;
                HttpContext.Current.Session["MasterType"] = mastertype;
            }
            else if (loginback)
            {
                HttpContext.Current.Session["SuperAdminId"] = null;
                HttpContext.Current.Session["MasterUserId"] = null;
                HttpContext.Current.Session["MasterType"] = null;

                GolferLoginOffSession();
            }
            if (masteruserid == 0)
            {
                if (HttpContext.Current.Request.Cookies["loginsuper"] != null)
                {
                    HttpContext.Current.Response.Cookies["loginsuper"].Expires = DateTime.Now.AddDays(-1);
                }
                if (HttpContext.Current.Request.Cookies["logincauser"] != null)
                {
                    HttpContext.Current.Response.Cookies["logincauser"].Expires = DateTime.Now.AddDays(-1);
                }
                if (HttpContext.Current.Request.Cookies["logincourseuser"] != null)
                {
                    HttpContext.Current.Response.Cookies["logincourseuser"].Expires = DateTime.Now.AddDays(-1);
                }

                if (type == UserType.Admin || type == UserType.SuperAdmin)
                {
                    if (KeepMeLogin)
                    {
                        HttpContext.Current.Response.Cookies["loginsuper"].Values.Add("uname", userName);
                        HttpContext.Current.Response.Cookies["loginsuper"].Values.Add("pwd", password);
                    }
                    HttpContext.Current.Response.Cookies["loginsuper"].Values.Add("url", HttpContext.Current.Request.Url.AbsolutePath);
                    HttpContext.Current.Response.Cookies["loginsuper"].Expires = DateTime.Now.AddDays(30);
                }
                else if (type == UserType.CourseAdmin)
                {
                    if (KeepMeLogin)
                    {
                        HttpContext.Current.Response.Cookies["logincauser"].Values.Add("uname", userName);
                        HttpContext.Current.Response.Cookies["logincauser"].Values.Add("pwd", password);
                    }
                    HttpContext.Current.Response.Cookies["logincauser"].Values.Add("url", HttpContext.Current.Request.Url.AbsolutePath);
                    HttpContext.Current.Response.Cookies["logincauser"].Expires = DateTime.Now.AddDays(30);
                    HttpContext.Current.Session["CourseNewMsgCount"] = null;
                }
                else
                {
                    if (KeepMeLogin)
                    {
                        HttpContext.Current.Response.Cookies["logincourseuser"].Values.Add("uname", userName);
                        HttpContext.Current.Response.Cookies["logincourseuser"].Values.Add("pwd", password);
                    }
                    HttpContext.Current.Response.Cookies["logincourseuser"].Values.Add("course", LoginInfo.CourseName);
                    HttpContext.Current.Response.Cookies["logincourseuser"].Values.Add("url", HttpContext.Current.Request.Url.AbsolutePath);
                    HttpContext.Current.Response.Cookies["logincourseuser"].Expires = DateTime.Now.AddDays(30);
                    HttpContext.Current.Session["CourseNewMsgCount"] = null;
                }
            }
        }

        public static bool IsLoginUser { get { return UserId > 0; } }

        public static string Name { get { return FirstName + " " + LastName; } }

        public static void LoginOffSession()
        {
            #region Update User Online Status

            CommonFunctions.LogoutAdminUsers();

            #endregion

            HttpContext.Current.Session["LoginType"] = null;
            HttpContext.Current.Session["UserId"] = null;
            HttpContext.Current.Session["AdminCourseId"] = null;
            HttpContext.Current.Session["AdminCourseName"] = null;
            HttpContext.Current.Session["SuperAdminId"] = null;
            HttpContext.Current.Session["UserName"] = null;
            HttpContext.Current.Session["FirstName"] = null;
            HttpContext.Current.Session["LastName"] = null;
            HttpContext.Current.Session["Type"] = null;
            HttpContext.Current.Session["UserAuthType"] = null;
            HttpContext.Current.Session["LastLogin"] = null;
            HttpContext.Current.Session["LastLoginIP"] = null;

            HttpContext.Current.Session["MasterUserId"] = null;
            HttpContext.Current.Session["MasterType"] = null;
            HttpContext.Current.Session["CourseNewMsgCount"] = null;
        }

        #region Golfer Login Session Detail

        public static long GolferUserId
        {
            get { return Convert.ToInt64(HttpContext.Current.Session["GolferUserId"]); }
        }

        public static string GolferEmail { get { return Convert.ToString(HttpContext.Current.Session["GolferEmail"]); } }
        public static string GolferFirstName { get { return Convert.ToString(HttpContext.Current.Session["GolferFirstName"]); } set { HttpContext.Current.Session["GolferFirstName"] = value; } }
        public static string GolferLastName { get { return Convert.ToString(HttpContext.Current.Session["GolferLastName"]); } set { HttpContext.Current.Session["GolferLastName"] = value; } }
        public static string GolferType { get { return Convert.ToString(HttpContext.Current.Session["GolferType"]); } }
        public static DateTime? GolferLastLogin { get { return (DateTime?)HttpContext.Current.Session["GolferLastLogin"]; } }
        public static string GolferLastLoginIP { get { return Convert.ToString(HttpContext.Current.Session["GolferLastLoginIP"]).Trim() == "" ? "-" : Convert.ToString(HttpContext.Current.Session["GolferLastLoginIP"]).Trim(); } }
        
        internal static void CreateGolferLoginSession(string logintype,
                                                long userId,
                                                string email,
                                                string password,
                                                string firstName,
                                                string lastName,
                                                string type,
                                                DateTime? lastLogin,
                                                string lastLoginIP,
                                                bool KeepMeLogin = false,
                                                long masteruserid = 0,
                                                string mastertype = "",
                                                bool loginback = false,
                                                string loginImage = "")
        {
            HttpContext.Current.Session["LoginType"] = logintype;
            HttpContext.Current.Session["GolferUserId"] = userId;
            HttpContext.Current.Session["GolferEmail"] = email;
            HttpContext.Current.Session["GolferFirstName"] = firstName;
            HttpContext.Current.Session["GolferLastName"] = lastName;
            HttpContext.Current.Session["GolferType"] = type;
            HttpContext.Current.Session["GolferLastLogin"] = lastLogin;
            HttpContext.Current.Session["GolferLastLoginIP"] = lastLoginIP;
            HttpContext.Current.Session["UserLoginImage"] = loginImage;

            if (masteruserid > 0)
            {
                HttpContext.Current.Session["SuperAdminId"] = masteruserid;
                HttpContext.Current.Session["MasterUserId"] = masteruserid;
                HttpContext.Current.Session["MasterType"] = mastertype;
            }

            if (KeepMeLogin)
            {
                HttpContext.Current.Response.Cookies["logingolferuser"].Values.Add("GolferEmail", email);
                HttpContext.Current.Response.Cookies["logingolferuser"].Values.Add("GolferPwd", password);
            }
            HttpContext.Current.Response.Cookies["logingolferuser"].Values.Add("GolferUrl", HttpContext.Current.Request.Url.AbsolutePath);
            HttpContext.Current.Response.Cookies["logingolferuser"].Expires = DateTime.Now.AddDays(30);
            HttpContext.Current.Session["GolferNewMsgCount"] = null;
        }

        public static bool IsGolferLoginUser { get { return GolferUserId > 0; } }
        public static string GolferName { get { return GolferFirstName + " " + GolferLastName; } }

        public static void GolferLoginOffSession()
        {
            #region Update User Online Status

            CommonFunctions.LogoutGolferUsers();

            #endregion

            HttpContext.Current.Session["LoginType"] = null;
            HttpContext.Current.Session["GolferUserId"] = null;
            HttpContext.Current.Session["GolferEmail"] = null;
            HttpContext.Current.Session["GolferFirstName"] = null;
            HttpContext.Current.Session["GolferLastName"] = null;
            HttpContext.Current.Session["GolferLastLogin"] = null;
            HttpContext.Current.Session["GolferLastLoginIP"] = null;
            HttpContext.Current.Session["GolferNewMsgCount"] = null;
        }

        #endregion
    }
}