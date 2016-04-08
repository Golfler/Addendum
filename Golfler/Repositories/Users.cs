using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Caching;
using Golfler.Models;

namespace Golfler.Repositories
{
    /// <summary>
    /// Created By: Renuka Hira
    /// Created on: 4st June, 2013
    /// </summary>
    /// <remarks>User class to get, add, update, delete</remarks>
    public class Users
    {
        public GF_AdminUsers UserObj { get; private set; }
        public GF_Golfer GolferObj { get; private set; }
        protected GolflerEntities Db;

        public string Message { get; private set; }

        public Int64 IdForQuickAdd { get; set; }
        public string FullNameForQuickAdd { get; set; }

        string _salt, _password = string.Empty;

        public string listCreationTime { get; set; }
        public string AdFetchTime { get; set; }
        //public List<string> distinctGroups { get; set; }

        public HashSet<string> distinctGroups { get; set; }

        public Dictionary<int?, string> allOrgsData { get; set; }

        long courseId;
        public long CourseId  //User belongs to Organization Id
        {
            get
            {
                if (courseId == 0 && UserObj.Type.Contains(UserType.CourseAdmin))
                {
                    var courseUser = Db.GF_CourseUsers.FirstOrDefault(x => x.UserID == UserObj.ID);
                    if (courseUser != null)
                    {
                        courseId = Convert.ToInt64(courseUser.CourseID);
                        courseName = courseUser.GF_CourseInfo.COURSE_NAME;
                        courseEmail = courseUser.GF_AdminUsers.Email;
                    }
                    else
                        courseId = 0;
                }
                return courseId;
            }
        }

        string courseName;
        public string CourseName { get { return courseName; } }
        string courseEmail;
        public string CourseEmail { get { return courseEmail; } }

        #region Constructors

        public Users()
        {
            Db = new GolflerEntities();
        }

        public Users(long? id)
        {
            Db = new GolflerEntities();
            UserObj = Db.GF_AdminUsers.FirstOrDefault(u => u.ID == id);
        }

        #endregion

        public bool SuperAdminLogin(long courseid)
        {
            var status = false;
            try
            {
                UserObj = (from u in Db.GF_AdminUsers
                           join l in Db.GF_CourseInfo on u.CourseId equals l.ID
                           where u.Status == StatusType.Active && l.ID == courseid && u.Type == UserType.SuperAdmin
                           select u).FirstOrDefault();

                status = true;
            }
            catch (Exception ex)
            {
                ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);

            }
            return status;
        }

        public string currentUserType { get; set; }
        public bool LoginWithID(long UserID)
        {
            var status = false;
            try
            {
                UserObj = (from u in Db.GF_AdminUsers
                           where u.Status == StatusType.Active && u.ID == UserID
                           select u).FirstOrDefault();
                if (UserObj != null)
                {
                    currentUserType = UserObj.Type;

                    status = true;
                }
            }
            catch (Exception ex)
            {
                ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
            }
            return status;
        }

        /// <summary>
        /// Created By: Renuka Hira
        /// Created on: 3rd June, 2013
        /// </summary>
        /// <remarks>login in admin section</remarks>
        public bool Login(LogInModel obj)
        {
            try
            {
                if (obj.UserType.Contains(UserType.Admin))
                {
                    UserObj = Db.GF_AdminUsers.FirstOrDefault(x => x.UserName == obj.UserName && x.Status == StatusType.Active &&
                        (x.Type == UserType.SuperAdmin || x.Type == UserType.Admin));
                }

                if (UserObj != null)
                {
                    if (UserObj.LoginAttempt == 4)
                    {
                        return false;
                    }

                    if (CommonFunctions.GetPassword(obj.Password, UserObj.SALT) == UserObj.Password)
                    {
                        var login = new { UserObj.LastLogin, UserObj.LastLoginIP };
                        UserObj.LastLogin = DateTime.UtcNow;
                        UserObj.LastLoginIP = obj.IpAddress;
                        UserObj.IsOnline = true;
                        UserObj.LoginAttempt = 0;
                        Db.Configuration.ValidateOnSaveEnabled = false;
                        Db.SaveChanges();
                        UserObj.LastLogin = Convert.ToDateTime(login.LastLogin).ToLocalTime();
                        UserObj.LastLoginIP = login.LastLoginIP;

                        return true;
                    }
                }
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
                return false;
            }
            catch (Exception ex)
            {
                ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
                return false;
            }
            return false;
        }

        public bool loginLock(LogInModel obj, int loginAttempt)
        {
            try
            {
                UserObj = Db.GF_AdminUsers.FirstOrDefault(x => x.UserName == obj.UserName && x.Status == StatusType.Active);

                if (UserObj != null)
                {
                    if (CommonFunctions.GetPassword(obj.Password, UserObj.SALT) == UserObj.Password)
                    {
                        return false;
                    }
                    else
                    {
                        UserObj.LoginAttempt = loginAttempt;
                        UserObj.LastLoginAttempt = DateTime.Now;
                        Db.SaveChanges();

                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
                return false;
            }
        }

        public bool loginLock(LogInModel obj)
        {
            try
            {
                var user = Db.GF_AdminUsers.FirstOrDefault(x => x.Type == UserType.SuperAdmin);

                if (user != null)
                {
                    if (user.Status == StatusType.Active)
                        return false;
                    else
                        return true;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
                return false;
            }
        }

        /// <summary>
        /// Created By:
        /// Created on:
        /// </summary>
        /// <remarks>login in course admin section</remarks>
        public bool CourseLogin(LogInModel obj)
        {
            try
            {
                UserObj = new GF_AdminUsers();

                //if (obj.UserType.Contains(UserType.CourseAdmin))
                //{
                UserObj = Db.GF_AdminUsers.FirstOrDefault(x => x.UserName.ToLower() == obj.UserName.ToLower() && x.Status == StatusType.Active &&
                    ((x.Type.Contains(UserType.CourseAdmin)) || (x.Type.Contains(UserType.Cartie)) ||
                     (x.Type.Contains(UserType.Kitchen)) || (x.Type.Contains(UserType.Ranger)) ||
                     (x.Type.Contains(UserType.Proshop)) || (x.Type.Contains(UserType.PowerAdmin))));
                //}

                if (UserObj != null)
                {
                    if (UserObj.LoginAttempt == 4)
                    {
                        return false;
                    }

                    if (CommonFunctions.GetPassword(obj.Password, UserObj.SALT) == UserObj.Password)
                    {
                        var login = new { UserObj.LastLogin, UserObj.LastLoginIP };
                        UserObj.LastLogin = DateTime.UtcNow;
                        UserObj.LastLoginIP = obj.IpAddress;
                        UserObj.IsOnline = true;
                        UserObj.LoginAttempt = 0;
                        Db.Configuration.ValidateOnSaveEnabled = false;
                        Db.SaveChanges();

                        #region Update User Lat/Long same as Course Location if user use web panel

                        var courseLatLng = Db.GF_CourseInfo.FirstOrDefault(x => x.ID == UserObj.CourseId);
                        if (courseLatLng != null)
                        {
                            GF_UserCurrentPosition userPosition = Db.GF_UserCurrentPosition.FirstOrDefault(x => x.ReferenceID == UserObj.ID &&
                                x.ReferenceType == UserObj.Type &&
                                x.CourseID == UserObj.CourseId);

                            if (userPosition != null)
                            {
                                //Update
                                userPosition.Latitude = courseLatLng.LATITUDE;
                                userPosition.Longitude = courseLatLng.LONGITUDE;
                                userPosition.ModifyBy = UserObj.ID;
                                userPosition.ModifyDate = DateTime.UtcNow;
                                Db.SaveChanges();
                            }
                            else
                            {
                                //Save
                                userPosition = new GF_UserCurrentPosition();
                                userPosition.CourseID = UserObj.CourseId;
                                userPosition.ReferenceID = UserObj.ID;
                                userPosition.ReferenceType = UserObj.Type;
                                userPosition.Latitude = courseLatLng.LATITUDE;
                                userPosition.Longitude = courseLatLng.LONGITUDE;
                                userPosition.CreatedBy = UserObj.ID;
                                userPosition.CreatedDate = DateTime.UtcNow;

                                Db.GF_UserCurrentPosition.Add(userPosition);
                                Db.SaveChanges();
                            }
                        }

                        #endregion

                        UserObj.LastLogin = Convert.ToDateTime(login.LastLogin).ToLocalTime();
                        UserObj.LastLoginIP = login.LastLoginIP;

                        return true;
                    }
                }
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
                return false;
            }
            catch (Exception ex)
            {
                ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
                return false;
            }
            return false;
        }

        /// <summary>
        /// Created By:
        /// Created on:
        /// </summary>
        /// <remarks>login in course admin section</remarks>
        public bool GolferLogin(LogInModel obj, ref long golferID)
        {
            try
            {
                GF_Golfer golfer = new GF_Golfer();

                if (obj.UserType.Contains(UserType.Golfer))
                {
                    golfer = Db.GF_Golfer.FirstOrDefault(x => x.Email == obj.UserName && x.Status != StatusType.Delete);
                }

                if (golfer != null)
                {
                    if (golfer.Status.Trim() == StatusType.Active)
                    {
                        if (CommonFunctions.GetPassword(obj.Password, golfer.Salt) == golfer.Password)
                        {
                            var login = new { golfer.LastLogin, golfer.LastLoginIP };
                            golfer.LastLogin = DateTime.UtcNow;
                            golfer.LastLoginIP = obj.IpAddress;
                            golfer.IsOnline = true;
                            Db.Configuration.ValidateOnSaveEnabled = false;
                            Db.SaveChanges();
                            //  golfer.LastLogin = Convert.ToDateTime(login.LastLogin).ToLocalTime();
                            //  golfer.LastLoginIP = login.LastLoginIP;

                            golferID = golfer.GF_ID;
                            GolferObj = new GF_Golfer();
                            GolferObj = golfer;

                            #region Update Golfer User Table

                            //Update the golfer course with value zero when user is login in Web Panel

                            var golferUser = Db.GF_GolferUser.FirstOrDefault(x => x.GolferID == golfer.GF_ID);

                            if (golferUser != null)
                            {
                                golferUser.CourseID = null;
                                Db.SaveChanges();
                            }

                            #endregion

                            return true;
                        }
                    }
                    else
                    {
                        Message = "Your account has been deactivated. Please contact to administrator.";
                        return false;
                    }
                }
                else
                {
                    Message = "Invalid User.";
                    return false;
                }
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException e)
            {
                string msg = e.Message;
            }
            catch (Exception ex)
            {
                ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
                return false;
            }
            return false;
        }

        /// <summary>
        /// Created By: Renuka Hira
        /// Created on: 3rd June, 2013
        /// </summary>
        /// <remarks>Get Admin User Listing</remarks>
        public IQueryable<GF_AdminUsers> GetAdminUsers(string filterExpression, string sortExpression, string sortDirection, int pageIndex, int pageSize, ref int totalRecords)
        {
            IQueryable<GF_AdminUsers> list;

            if (!String.IsNullOrWhiteSpace(filterExpression))
                list = Db.GF_AdminUsers.Where(x => x.Status != StatusType.Delete && (x.UserName.ToLower().Contains(filterExpression.ToLower()) ||
                     (x.FirstName + " " + x.LastName).ToLower().Contains(filterExpression.ToLower())) ||
                     (x.Email.ToLower().Contains(filterExpression.ToLower())) &&
                     (x.Type == UserType.Admin || x.Type == UserType.CourseAdmin))
                .ToList()
                .Select((x => new GF_AdminUsers
                    {
                        ID = x.ID,
                        UserName = x.UserName,
                        FirstName = x.FirstName,
                        LastName = x.LastName,
                        Email = x.Email,
                        UserType = UserType.GetFullUserType(x.Type),
                        Role = (x.GF_Roles != null) ? x.GF_Roles.Name : "",
                        Status = x.Status,
                        Type = x.Type,
                        CourseName = string.Join(",<br />  ", ((from a in Db.GF_CourseInfo
                                                                where a.ID == x.CourseId
                                                                select a.COURSE_NAME).ToList())),
                        CreatedOn = x.CreatedOn,
                        LastLogin = x.LastLogin
                    }
                )).AsQueryable();
            else
                list = Db.GF_AdminUsers.Where(x => x.Status != StatusType.Delete &&
                     (x.Type == UserType.Admin || x.Type == UserType.CourseAdmin))
                .ToList()
                .Select((x => new GF_AdminUsers
                {
                    ID = x.ID,
                    UserName = x.UserName,
                    FirstName = x.FirstName,
                    LastName = x.LastName,
                    Email = x.Email,
                    UserType = UserType.GetFullUserType(x.Type),
                    Role = (x.GF_Roles != null) ? x.GF_Roles.Name : "",
                    Status = x.Status,
                    Type = x.Type,
                    CourseName = string.Join(",<br />  ", ((from a in Db.GF_CourseInfo
                                                            where a.ID == x.CourseId
                                                            select a.COURSE_NAME).ToList())),
                    CreatedOn = x.CreatedOn,
                    LastLogin = x.LastLogin
                }
                )).AsQueryable();

            totalRecords = list.Count();

            return list.OrderBy(sortExpression + " " + sortDirection).Skip((pageIndex - 1) * pageSize).Take(pageSize);
        }

        /// <summary>
        /// Created By: Renuka Hira
        /// Created on: 3rd June, 2013
        /// </summary>
        /// <remarks>Get Course Admin User Listing</remarks>
        public IQueryable<GF_AdminUsers> GetCourseUsers(string filterExpression, string sortExpression, string sortDirection, int pageIndex, int pageSize, ref int totalRecords)
        {
            IQueryable<GF_AdminUsers> list;
            //x.CreatedBy == LoginInfo.UserId
            if (!String.IsNullOrWhiteSpace(filterExpression))
                list = Db.GF_AdminUsers.Where(x => x.CourseId == LoginInfo.CourseId &&
                     (x.UserName.ToLower().Contains(filterExpression.ToLower()) ||
                      x.Email.ToLower().Contains(filterExpression.ToLower()) ||
                     (x.FirstName + " " + x.LastName).ToLower().Contains(filterExpression.ToLower())) && x.Status != StatusType.Delete)
                .ToList()
                .Select((x => new GF_AdminUsers
                    {
                        ID = x.ID,
                        UserName = x.UserName,
                        FirstName = x.FirstName,
                        LastName = x.LastName,
                        Email = x.Email,
                        UserType = UserType.GetFullUserType(x.Type),
                        Role = (x.GF_Roles != null) ? x.GF_Roles.Name : "",
                        Status = x.Status,
                        Type = x.Type,
                        CourseName = string.Join(",<br />  ", ((from a in Db.GF_CourseInfo
                                                                where a.ID == x.CourseId
                                                                select a.COURSE_NAME).ToList())),
                        CreatedOn = x.CreatedOn,
                        LastLogin = x.LastLogin
                    }
                )).AsQueryable();
            else
                list = Db.GF_AdminUsers.Where(x => x.CourseId == LoginInfo.CourseId &&
                    x.Status != StatusType.Delete)
                .ToList()
                .Select((x => new GF_AdminUsers
                    {
                        ID = x.ID,
                        UserName = x.UserName,
                        FirstName = x.FirstName,
                        LastName = x.LastName,
                        Email = x.Email,
                        UserType = UserType.GetFullUserType(x.Type),
                        Role = (x.GF_Roles != null) ? x.GF_Roles.Name : "",
                        Status = x.Status,
                        Type = x.Type,
                        CourseName = string.Join(",<br />  ", ((from a in Db.GF_CourseInfo
                                                                where a.ID == x.CourseId
                                                                select a.COURSE_NAME).ToList())),
                        CreatedOn = x.CreatedOn,
                        LastLogin = x.LastLogin
                    }
                )).AsQueryable();

            totalRecords = list.Count();

            return list.OrderBy(sortExpression + " " + sortDirection).Skip((pageIndex - 1) * pageSize).Take(pageSize);
        }

        /// <summary>
        /// Created By: Renuka Hira
        /// Created on: 3rd June, 2013
        /// </summary>
        /// <remarks>Organization User Listing</remarks>
        public IQueryable<GF_AdminUsers> GetOrganizationUsers(string filterExpression, string RoleId, string sortExpression, string sortDirection, int pageIndex, int pageSize, ref int totalRecords, int gName)
        {
            IQueryable<GF_AdminUsers> list;
            var courseUsers = Db.GF_CourseUsers.Where(u => u.CourseID == LoginInfo.CourseId).Select(x => x.UserID).AsQueryable();
            if (!String.IsNullOrWhiteSpace(filterExpression))
                list = Db.GF_AdminUsers.Where(x => (x.UserName.ToLower().Contains(filterExpression.ToLower()) ||
                     (x.FirstName + " " + x.LastName).ToLower().Contains(filterExpression.ToLower()))
                     && x.Status != StatusType.Delete
                     && x.Type.Contains(LoginInfo.LoginUserType)
                     && (x.CourseId == LoginInfo.CourseId || (courseUsers.Contains(x.ID) && x.Type == UserType.CourseAdmin)));
            else
                list = Db.GF_AdminUsers.Where(x => x.Status != StatusType.Delete
                    && x.Type.Contains(LoginInfo.LoginUserType)
                    && (x.CourseId == LoginInfo.CourseId || (courseUsers.Contains(x.ID) && x.Type == UserType.CourseAdmin)));

            if (RoleId != "0" && RoleId != "")
            {
                long lngRoleId = Convert.ToInt64(RoleId);
                list = list.Where(x => x.RoleId == lngRoleId);
            }

            totalRecords = list.Count();

            return list.OrderBy(sortExpression + " " + sortDirection).Skip((pageIndex - 1) * pageSize).Take(pageSize);
        }

        public GF_AdminUsers GetUser(long? id)
        {
            if (Convert.ToInt64(id) > 0)
            {
                UserObj = Db.GF_AdminUsers.FirstOrDefault(x => x.ID == id);
                UserObj.IsCourseUser = UserObj.Type == UserType.CourseAdmin;
            }

            return UserObj ?? new GF_AdminUsers();
        }

        public IEnumerable<GF_Roles> GetUserRoles(string status, long? id)
        {
            List<GF_Roles> defaultMenus = new List<GF_Roles>();
            defaultMenus.Add(new GF_Roles() { ID = 0, Name = "Select" });
            long roleid = 0;
            if (UserObj != null)
                roleid = Convert.ToInt64(UserObj.RoleId);
            return defaultMenus.Union(Db.GF_Roles.Where(x => x.Status != StatusType.Delete && (x.Status.ToUpper() == status.ToUpper() ||
                                            x.ID == roleid) && x.CourseUserId == LoginInfo.CourseId).OrderBy(x => x.Name));
        }

        public bool Save(GF_AdminUsers obj)
        {
            string userPwd = obj.Password;
            try
            {
                if (ValidUser(obj))
                {
                    if (obj.ID > 0)
                    {
                        #region Update User

                        if (UserObj != null)
                        {
                            if (UserObj.Password != obj.Password)
                            {
                                CommonFunctions.GeneratePassword(obj.Password, ref _salt, ref _password);
                                UserObj.SALT = _salt;
                                UserObj.Password = _password;
                            }
                            if (obj.ID != LoginInfo.UserId)
                            {
                                if (obj.RoleId > 0)
                                    UserObj.RoleId = obj.RoleId;
                                else
                                    UserObj.RoleId = null;
                                UserObj.Status = obj.Status;

                                if (obj.IsCourseUser)
                                {
                                    UserObj.Type = UserType.CourseAdmin;
                                    UserObj.RoleId = null;
                                }
                                else
                                {
                                    if (obj.Type.Contains(UserType.Cartie) || obj.Type.Contains(UserType.Kitchen) ||
                                        obj.Type.Contains(UserType.Ranger) || obj.Type.Contains(UserType.Proshop) ||
                                        obj.Type.Contains(UserType.PowerAdmin))
                                    {
                                        UserObj.Type = obj.Type;
                                    }
                                    else
                                        UserObj.Type = UserType.Admin;
                                }

                                UserObj.Status = obj.Status;
                            }
                            UserObj.IsCourseUser = obj.IsCourseUser;
                            UserObj.Email = obj.Email;
                            UserObj.FirstName = obj.FirstName;
                            UserObj.LastName = obj.LastName;
                            UserObj.Phone = obj.Phone;
                            UserObj.UserName = obj.UserName;
                            UserObj.ReceiveResolutionMails = obj.ReceiveResolutionMails;
                            if (obj.Image != null)
                                UserObj.Image = obj.Image;
                            UserObj.ModifiedBy = LoginInfo.SuperAdminId == 0 ? LoginInfo.UserId : LoginInfo.SuperAdminId;
                            UserObj.ModifiedOn = DateTime.UtcNow;
                        }

                        Message = "update";
                        Db.SaveChanges();

                        #endregion update
                    }
                    else
                    {
                        #region new user

                        if (obj.RoleId == 0)
                            obj.RoleId = null;

                        CommonFunctions.GeneratePassword(obj.Password, ref _salt, ref _password);
                        obj.SALT = _salt;
                        obj.Password = _password;

                        if (obj.IsCourseUser)
                        {
                            obj.Type = UserType.CourseAdmin;
                            obj.RoleId = null;
                        }
                        else
                        {
                            if (obj.Type.Contains(UserType.Cartie) || obj.Type.Contains(UserType.Kitchen) ||
                                obj.Type.Contains(UserType.Ranger) || obj.Type.Contains(UserType.Proshop) ||
                                obj.Type.Contains(UserType.PowerAdmin))
                            {
                                //UserObj.Type = obj.Type;
                            }
                            else
                                obj.Type = UserType.Admin;
                        }

                        obj.CreatedBy = LoginInfo.SuperAdminId == 0 ? LoginInfo.UserId : LoginInfo.SuperAdminId;
                        obj.CreatedOn = DateTime.UtcNow;
                        Db.GF_AdminUsers.Add(obj);

                        Db.SaveChanges();

                        IdForQuickAdd = obj.ID;
                        FullNameForQuickAdd = obj.FirstName + " " + obj.LastName;
                        Message = "add";

                        #region Send Mail

                        try
                        {
                            Int64 courseidForParams = (LoginInfo.LoginUserType == UserType.CourseAdmin || LoginInfo.LoginUserType == UserType.Proshop) ? LoginInfo.CourseId : 0;
                            string templateType = (LoginInfo.LoginUserType == UserType.CourseAdmin || LoginInfo.LoginUserType == UserType.Proshop) ? EmailTemplateName.CourseUserRegistration : EmailTemplateName.AdminUserRegistration;
                            string mailresult = "";

                            IQueryable<GF_EmailTemplatesFields> templateFields = null;
                            var param = EmailParams.GetEmailParamsNew(ref Db, templateType, ref templateFields,
                                courseidForParams, LoginInfo.LoginType, ref mailresult);

                            if (mailresult == "") // Means Parameters are OK
                            {
                                if (ApplicationEmails.AdminUserRegistrationMail(ref Db, obj, param, ref templateFields))
                                {
                                    // Do steps for Mail Send successful
                                }
                                else
                                {
                                    // Do steps for Mail Failure. Mail failure reason can be find on "mailresult"
                                }
                            }
                            else
                            {
                                // Do steps for Parameters not available.Reason can be find on "mailresult"
                            }
                        }
                        catch (Exception ex)
                        {
                            ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
                        }

                        #endregion

                        #endregion
                    }

                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
                Message = "Exception: " + ex.Message;
                return false;
            }
        }

        private void AddCourseUser(long? courseID = 0)
        {
            if (courseID == 0)
                courseID = LoginInfo.CourseId;

            if (UserObj.GF_CourseUsers.Count(x => x.CourseID == courseID && x.UserID == UserObj.ID) == 0)                //check record in OrgUsr
                //insert record in OrgUsr
                UserObj.GF_CourseUsers.Add(new GF_CourseUsers()
                {
                    CourseID = courseID
                });
        }

        private void DeleteCourseUser(long? orgID = 0)
        {
            if (orgID == 0)
                orgID = LoginInfo.CourseId;

            if (Db.GF_CourseUsers.Count(x => x.CourseID == orgID && x.UserID == UserObj.ID) > 0)
            {
                Db.GF_CourseUsers.Remove(Db.GF_CourseUsers.FirstOrDefault(x => x.CourseID == orgID && x.UserID == UserObj.ID));
            }
        }


        #region Private Methods

        private bool ValidUser(GF_AdminUsers obj)
        {
            Message = string.Empty;

            var orgCreateIds = Db.GF_AdminUsers.Where(x => x.ID != obj.ID).Select(x => x.ID).Distinct();

            if (Db.GF_AdminUsers.Count(x => x.UserName.ToLower() == obj.UserName.ToLower() && orgCreateIds.Contains(x.ID) && x.Status != StatusType.Delete) > 0)
            {
                Message = "This username is already taken by some one in Golfler application. Please enter another one.";
            }
            else if (Db.GF_AdminUsers.Count(x => x.Email.ToLower() == obj.Email.ToLower() && orgCreateIds.Contains(x.ID) && x.Status != StatusType.Delete && x.CourseId == LoginInfo.CourseId) > 0)
                Message = "Email already exists.";

            return Message.Length == 0;
        }

        #endregion

        internal bool ChangeStatus(bool status)
        {
            GF_RoleModules objModuleRole = new GF_RoleModules();


            //objModuleRole = CommonFunctions.GetAccessModule(ModuleValues.AllRights);


            //if (!objModuleRole.UpdateFlag)
            //{
            //    Message = "unaccess";
            //    return false;
            //}

            if (UserObj != null)
            {
                if (UserObj.ID != LoginInfo.UserId)
                {
                    UserObj.Active = !status;
                    UserObj.ModifiedBy = LoginInfo.UserId;
                    UserObj.ModifiedOn = DateTime.Now;
                }
                Db.SaveChanges();
                return true;
            }
            return false;
        }

        public bool ResetPassword(ForgotModelAdmin obj)
        {
            UserObj = Db.GF_AdminUsers.FirstOrDefault(x => x.Email == obj.Email &&
                    x.Status == StatusType.Active && x.Type.Contains(obj.Type) &&
                    (x.CourseId == obj.CourseID ||
                        (x.Type == UserType.SuperAdmin && Db.GF_CourseUsers.Where(o =>
                            o.CourseID == obj.CourseID).Select(o =>
                                o.UserID).Contains(x.ID))));
            if (UserObj != null)
            {
                var newpassword = CommonLibClass.FetchRandStr(8);
                CommonFunctions.GeneratePassword(newpassword, ref _salt, ref _password);
                UserObj.SALT = _salt;
                UserObj.Password = _password;

                return false;
            }

            return false;
        }

        public bool DeleteAdminUsers(List<long> ids)
        {
            try
            {
                //GF_RoleModules objModuleRole = CommonFunctions.GetAccessModule(ModuleValues.User);
                //if (!objModuleRole.DeleteFlag)
                //{
                //    Message = "";// Resources.Resources.unaccess;
                //    return false;
                //}

                var users = Db.GF_AdminUsers.Where(x => ids.AsQueryable().Contains(x.ID));
                foreach (var u in users)
                {
                    u.Status = StatusType.Delete;
                    u.CourseId = 0;
                    u.ModifiedBy = LoginInfo.UserId;
                    u.ModifiedOn = DateTime.Now;
                }

                Db.GF_CourseUsers.Where(x => ids.AsQueryable().Contains(x.UserID ?? 0)).ToList().ForEach(x => Db.GF_CourseUsers.Remove(x));
                Db.Configuration.ValidateOnSaveEnabled = false;
                Db.SaveChanges();

                return true;
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                return false;
            }
        }

        public List<string> CheckUserLength(String IDs)
        {
            string[] strArr = IDs.Split(',');
            List<string> list = new List<string>();

            foreach (var id in strArr)
            {
                if (id.ToString().Length <= 6)
                {
                    list.Add(Convert.ToString(id));
                }
            }

            return list;
        }

        public List<GF_AdminUsers> GetAdminUserByID(long id)
        {
            try
            {
                var lstAdminUser = Db.GF_AdminUsers.Where(x => x.ID == id);

                return lstAdminUser.ToList();
            }
            catch
            {
                return new List<GF_AdminUsers>();
            }
        }

        public List<GF_RoleModules> GetRoleUserByID(long id)
        {
            try
            {
                var lstAdminUser = Db.GF_AdminUsers.FirstOrDefault(x => x.ID == id);

                if (lstAdminUser.RoleId > 0)
                {
                    var lstRole = Db.GF_RoleModules.Where(x => x.RoleID == (lstAdminUser.RoleId ?? 0));

                    return lstRole.ToList();
                }

                return new List<GF_RoleModules>();
            }
            catch
            {
                return new List<GF_RoleModules>();
            }
        }
    }

    public class UserData
    {
        public GF_AdminUsers UserObj { get; set; }
        public string CourseID { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
    }
}
