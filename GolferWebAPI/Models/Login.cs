using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GolferWebAPI.Models
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
        public int? AppVersion { get; set; }

        #region Login User
        /// <summary> 
        /// Created By:Arun
        /// Created Date:23 March 2015
        /// Purpose: Login web service
        /// </summary>
        /// <param name="Email"></param>
        /// <param name="Password"></param>
        /// <param name="DeviceId"></param>
        /// <param name="DeviceType"></param>
        /// <returns></returns>
        public Result LoginUser(Login objLogin)
        {
            try
            {
                var objGolfer = _db.GF_Golfer.FirstOrDefault(p => p.Email.ToLower() == objLogin.Email.ToLower());

                if (loginLock())
                {
                    return new Result { Id = 0, Status = 0, Error = "There is some problem with application. Please contact your system administrator." };
                }

                if (objGolfer != null)
                {
                    if (objGolfer.Status.Trim() == StatusType.InActive ||
                        objGolfer.Status.Trim() == StatusType.Delete)
                    {
                        return new Result { Id = objGolfer.GF_ID, Status = 0, Error = "Your account has been deactivated.Please contact to administrator." };
                    }

                    if (CommonFunctions.DecryptString(objGolfer.Password, objGolfer.Salt) == objLogin.Password)
                    {
                        objGolfer.IsOnline = true;
                        objGolfer.DeviceSerialNo = Convert.ToString(objLogin.DeviceSerialNo);
                        //if ((objGolfer.DeviceType != objLogin.DeviceType) || (objGolfer.DeviceType != objLogin.DeviceId))
                        //{
                        if (objLogin.DeviceType.ToLower() == "ios")
                            objGolfer.APNID = objLogin.DeviceId;
                        else
                            objGolfer.GCMID = objLogin.DeviceId;
                        objGolfer.DeviceType = objLogin.DeviceType;
                        //}
                        objGolfer.AppVersion = objLogin.AppVersion;
                        _db.SaveChanges();

                        #region logout other users from this device

                        if (objGolfer.DeviceType.ToLower().Equals("ios"))
                        {
                            CommonFunctions.LogoutOtherUsers("golfer", objGolfer.GF_ID, objGolfer.DeviceType, objGolfer.APNID, objGolfer.DeviceSerialNo);
                        }
                        else
                        {
                            CommonFunctions.LogoutOtherUsers("golfer", objGolfer.GF_ID, objGolfer.DeviceType, objGolfer.GCMID, objGolfer.DeviceSerialNo);
                        }
                        #endregion

                        //Get course Detail based on location
                        var lstCourse = _db.GF_SP_GetCourseLocation(objLogin.Latitude, objLogin.Longitude, Convert.ToDouble(ConfigClass.Distance)).ToList().OrderBy(x => x.distnace);
                        string Tee = CommonFunctions.GetTeeValue(Convert.ToInt32(objGolfer.Tee));

                        #region Update Last Login Time

                        objGolfer.LastLogin = DateTime.UtcNow;
                        _db.SaveChanges();

                        #endregion

                        return new Result
                        {
                            Id = objGolfer.GF_ID,
                            Status = 1,
                            Error = "Login successfully.",
                            record = new
                            {
                                objGolfer.GF_ID,
                                objGolfer.FirstName,
                                objGolfer.LastName,
                                objGolfer.Email,
                                objGolfer.Latitude,
                                objGolfer.Longitude,
                                MobileNo = objGolfer.MobileNo ?? "",
                                objGolfer.CreatedOn,
                                //   objGolfer.Image,
                                Image = objGolfer.Image == null ? (ConfigClass.ImageSiteUrl + "/Images/chat%20icons/768/chat_golfer.png") : (ConfigClass.ImageSiteUrl + objGolfer.Image),

                                IsReceivePushNotification = objGolfer.IsReceivePushNotification ?? true,
                                IsReceiveEmail = objGolfer.IsReceiveEmail ?? true,
                                IsReceivePushNotificationGolfer = objGolfer.IsReceivePushNotificationGolfer ?? true,
                                IsReceiveEmailGolfer = objGolfer.IsReceiveEmailGolfer ?? true,
                                Tee,
                                Measurement = string.IsNullOrEmpty(objGolfer.measurement) ? "yards" : objGolfer.measurement,
                                Speed = string.IsNullOrEmpty(objGolfer.speed) ? "mph" : objGolfer.speed,
                                Temperature = string.IsNullOrEmpty(objGolfer.temperature) ? "fahrenheit" : objGolfer.temperature,
                                Courses = lstCourse
                            }
                        };
                    }
                    else
                    {
                        return new Result { Id = objGolfer.GF_ID, Status = 0, Error = "Password’s do not match, Please re-enter.", record = null };
                    }
                }
                else
                {
                    return new Result { Id = 0, Status = 0, Error = "User does not exists.", record = null };
                }
            }
            catch (Exception ex)
            {
                return new Result { Id = 0, Status = 0, Error = ex.Message };
            }
        }

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

        #endregion
    }
}