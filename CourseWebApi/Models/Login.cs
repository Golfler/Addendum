using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using CourseWebApi.Models;
using CourseWebAPI.Models;
using ErrorLibrary;
namespace CourseWebApi.Models
{
    public class Login
    {
        private GolflerEntities _db = null;

        public Login()
        {
            _db = new GolflerEntities();
        }

        public string Email { get; set; }
        public string Password { get; set; }
        public string DeviceId { get; set; }
        public string DeviceType { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public string DeviceSerialNo { get; set; }
        public long ID { get; set; }
        public string Image { get; set; }
        public string UserName { get; set; }
        public int? AppVersion { get; set; }
        //#region Login Course User
        /// <summary>
        /// Created By:Veera
        /// Created Date:24 March 2015
        /// Purpose: Course Login web service
        /// </summary>
        /// <param name="Email"></param>
        /// <param name="Password"></param>
        /// <param name="DeviceId"></param>
        /// <param name="DeviceType"></param>
        /// <param name="Latitude"></param>
        /// <param name="Longitude"></param>
        /// <returns></returns>
        public CourseLoginResult LoginCourseUser(string UserName, string Password, string DeviceId, string DeviceType, string Latitude, string Longitude,
            int? AppVersion, string DeviceSerialNo)
        {
            try
            {
                var objAdminUser = _db.GF_AdminUsers.FirstOrDefault(p => p.UserName.ToLower() == UserName.ToLower() &&
                    p.Status != StatusType.Delete);

                if (objAdminUser.Type == UserType.PowerAdmin)
                {
                    return new CourseLoginResult
                        {
                            Id = 0,
                            Status = 0,
                            Error = "You are not allow to login via App. Please login into web portal."
                        };
                }

                if (loginLock())
                {
                    return new CourseLoginResult { Id = 0, Status = 0, Error = "There is some problem with application. Please contact your system administrator." };
                }

                if (objAdminUser != null)
                {
                    if (objAdminUser.Status != "A")
                    {
                        return new CourseLoginResult { Id = objAdminUser.ID, Status = 0, Error = "your account has been deactivated.Please contact to administrator." };
                    }

                    if (CommonFunctions.DecryptString(objAdminUser.Password, objAdminUser.SALT) == Password)
                    {
                        objAdminUser.IsOnline = true;
                        if (DeviceType.ToLower() == "ios")
                            objAdminUser.APNID = DeviceId;
                        else
                            objAdminUser.GCMID = DeviceId;
                        objAdminUser.DeviceType = DeviceType;
                        objAdminUser.DeviceSerialNo = DeviceSerialNo;

                        objAdminUser.AppVersion = AppVersion;
                        _db.SaveChanges();


                        #region logout other users from this device
                        if (objAdminUser.DeviceType.ToLower().Equals("ios"))
                        {
                            CommonFunctions.LogoutOtherUsers("course", objAdminUser.ID, objAdminUser.DeviceType, objAdminUser.APNID,objAdminUser.DeviceSerialNo);
                        }
                        else
                        {
                            CommonFunctions.LogoutOtherUsers("course", objAdminUser.ID, objAdminUser.DeviceType, objAdminUser.GCMID, objAdminUser.DeviceSerialNo);
                        }
                        #endregion

                        //#region image code
                        ////if (string.IsNullOrEmpty(objAdminUser.Image))
                        ////{
                        //    byte[] bt = Convert.FromBase64String(Image);
                        //    MemoryStream ms = new MemoryStream(bt);
                        //    System.Drawing.Image img = System.Drawing.Image.FromStream(ms);
                        //    var imageName = DateTime.Now.Ticks + "-Course";
                        //    img.Save(HttpContext.Current.Server.MapPath("~/Upload/Courseimages/" + imageName + ".jpg"), System.Drawing.Imaging.ImageFormat.Jpeg);
                        //    var imagePath = ConfigClass.SiteUrl + "Upload/Courseimages/" + imageName + ".jpg";
                        //    objAdminUser.Image = imagePath;
                        ////}
                        //#endregion

                        //if ((objAdminUser.DeviceType != DeviceType) || (objAdminUser.DeviceType != DeviceId))
                        //{
                        //    if (DeviceType.ToLower() == "ios")
                        //        objAdminUser.APNID = DeviceId;
                        //    else
                        //        objAdminUser.GCMID = DeviceId;

                        //    objAdminUser.DeviceType = DeviceType;

                        //}
                       
                        #region User Current Position

                        //Code By: Amit Kumar
                        //Created Date: 04 June 2015
                        //When ever course user logged in mobile application
                        //then its current location will saved in user current position table

                        GF_UserCurrentPosition userPosition = _db.GF_UserCurrentPosition.FirstOrDefault(x => x.ReferenceID == objAdminUser.ID &&
                            x.ReferenceType == objAdminUser.Type &&
                            x.CourseID == objAdminUser.CourseId);

                        if (userPosition != null)
                        {
                            //Update
                            userPosition.Latitude = Latitude;
                            userPosition.Longitude = Longitude;
                            userPosition.ModifyBy = objAdminUser.ID;
                            userPosition.ModifyDate = DateTime.UtcNow;
                        }
                        else
                        {
                            //Save
                            userPosition = new GF_UserCurrentPosition();
                            userPosition.CourseID = objAdminUser.CourseId;
                            userPosition.ReferenceID = objAdminUser.ID;
                            userPosition.ReferenceType = objAdminUser.Type;
                            userPosition.Latitude = Latitude;
                            userPosition.Longitude = Longitude;
                            userPosition.CreatedBy = objAdminUser.ID;
                            userPosition.CreatedDate = DateTime.UtcNow;
                            userPosition.ModifyBy = objAdminUser.ID;
                            userPosition.ModifyDate = DateTime.UtcNow;

                            _db.GF_UserCurrentPosition.Add(userPosition);
                        }

                        _db.SaveChanges();

                        #endregion

                        return new CourseLoginResult
                        {
                            Id = objAdminUser.ID,
                            Status = 1,
                            Error = "Login successfully.",
                            record = new
                            {
                                //CourseId = _db.GF_CourseUsers.FirstOrDefault(p => p.UserID == objAdminUser.ID).CourseID,
                                CourseId = objAdminUser.CourseId,//_db.GF_CourseUsers.FirstOrDefault(p => p.UserID == objAdminUser.ID).CourseID,
                                objAdminUser.FirstName,
                                objAdminUser.LastName,
                                objAdminUser.Email,
                                objAdminUser.Type,
                                objAdminUser.UserName,
                                Image = objAdminUser.Image == null ? GetCourseUserImage(objAdminUser.Type) : (ConfigClass.ImageSiteUrl + objAdminUser.Image),
                                CourseName = _db.GF_CourseInfo.FirstOrDefault(p => p.ID == objAdminUser.CourseId).COURSE_NAME,
                                timeInterval = CommonFunctions.GetIntervalForCourse(Convert.ToInt64(objAdminUser.CourseId))
                            }
                        };
                    }
                    else
                    {
                        return new CourseLoginResult { Id = objAdminUser.ID, Status = 0, Error = "Password’s do not match, Please re-enter." };
                    }
                }
                else
                {
                    return new CourseLoginResult { Id = 0, Status = 0, Error = "Please input assigned course username, not your e-mail address." };
                }
            }
            catch (Exception ex)
            {
                return new CourseLoginResult { Id = 0, Status = 0, Error = ex.Message };
            }
        }


        #region Forgot Password
        /// <summary>
        /// Created By:Veera
        /// Created Date: 24 March 2015
        /// Purpose: Forgot Password location
        /// </summary>
        /// <param name="Email"></param>
        /// <returns></returns>
        public Result ForgotPassword(string UserName)
        {
            try
            {
                _db = new GolflerEntities();
                GF_AdminUsers reg = _db.GF_AdminUsers.FirstOrDefault(p => p.UserName.ToLower() == UserName.ToLower());
                if (reg == null)
                {
                    return new Result { Id = 0, Status = 0, Error = "User does not exists.", record = null };
                }

                else
                {
                    if (reg.Status.Trim() != StatusType.Active)
                    {
                        return new Result { Status = 0, Error = "Your account has been deactivated. Please contact to course administrator.", record = null };
                    }

                    //string _salt = reg.SALT;
                    //string newPassword = CommonLibClass.FetchRandStr(8);
                    //string _password = "";
                    //CommonFunctions.GeneratePassword(newPassword, "old", ref _salt, ref _password);

                    //reg.Password = _password;
                    ////
                    //if (_db.SaveChanges() > 0)
                    //{
                    #region send email
                    IQueryable<GF_EmailTemplatesFields> templateFields = null;

                    //
                    long courseid = Convert.ToInt64(reg.CourseId);
                    string mailresult = "";

                    var param = EmailParams.GetEmailParamsNew(ref _db, "Forgot Password", ref templateFields, courseid, "CA", ref mailresult, true);


                    if (param != null)
                    {
                        if (!ApplicationEmails.CourseUserForGotMail(reg, CommonFunctions.DecryptString(reg.Password, reg.SALT), param, ref templateFields, courseid))
                        {
                            // Message = String.Format(Resources.Resources.mailerror);
                            return new Result { Status = 0, Error = "An error occured while sending mail.", record = null };
                        }
                        else
                        {
                            return new Result { Status = 1, Error = "An email has been sent to you to reset your password.", record = null };
                        }
                    }
                    else
                    {
                        return new Result { Status = 0, Error = "An error occured while sending mail.", record = null };
                    }
                    #endregion
                    //}
                    //else
                    //{
                    //    return new Result { Status = 0, Error = "An error occured while saving password.", record = null };
                    //}

                    //
                    /* old 
                     * if (CommonFunctions.SendMailForgot(reg, newPassword))
                     {
                         if (_db.SaveChanges() > 0)
                         {
                             return new Result { Status = 1, Error = "Your new password is sent on your email address. Please login with new password.", record = null };
                         }
                         else
                         {
                             return new Result { Status = 0, Error = "An error occured while saving your entries.Please try again after some time.", record = null };
                         }
                     }
                     else
                     {
                       
                     }*/

                }
            }
            catch (Exception ex)
            {
                return new Result { Id = 0, Status = 0, Error = ex.Message, record = null };
            }
        }
        #endregion

        #region Logout
        /// <summary>
        /// Created by:Veera
        /// Created Date: 24 March2015
        /// Purpose:logout logged in user.
        /// </summary>
        /// <param name="userid"></param>
        /// <returns></returns>
        public Result Logout(long userid)
        {
            try
            {
                _db = new GolflerEntities();
                var objUser = _db.GF_AdminUsers.FirstOrDefault(x => x.ID == userid);
                if (objUser != null)
                {
                    if (objUser.DeviceType == "ios")
                        objUser.GCMID = string.Empty;
                    else
                        objUser.APNID = string.Empty;

                    objUser.IsOnline = false;
                    _db.SaveChanges();

                    return new Result { Id = 0, Status = 1, Error = "Logout Successfully." };
                }
                else
                {
                    return new Result { Id = 0, Status = 0, Error = "User not available." };
                }
            }
            catch (Exception ex)
            {

                ErrorClass.WriteLog(ex, HttpContext.Current.Request);
                return new Result { Id = 0, Status = 0, Error = ex.Message };
            }

        }

        #endregion

        public bool loginLock()
        {
            try
            {
                var user = _db.GF_AdminUsers.FirstOrDefault(x => x.Type == UserType.SuperAdmin);

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

        public string GetCourseUserImage(string type)
        {
            string result = "";
            if (type == "CK")
            {
                result = ConfigurationManager.AppSettings["DefaultImagePath"] + "Images/chat%20icons/768/chat_kitchen.png";

            }
            else if (type == "CP")
            {
                result = ConfigurationManager.AppSettings["DefaultImagePath"] + "Images/chat%20icons/768/chat_proshop.png";
            }
            else if (type == "CR")
            {
                result = ConfigurationManager.AppSettings["DefaultImagePath"] + "Images/chat%20icons/768/chat_ranger.png";
            }
            else if (type == "CC")
            {
                result = ConfigurationManager.AppSettings["DefaultImagePath"] + "Images/chat%20icons/768/chat_cartie.png";
            }

            else
            { result = ""; }

            return result;

        }

    }
}