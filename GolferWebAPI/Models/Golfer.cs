using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GolferWebAPI.Models;
using ErrorLibrary;
using System.IO;
namespace GolferWebAPI.Models
{
    public partial class GF_Golfer
    {
        GolflerEntities _db = null;
        // public string? MobileNo { get; set; }


        #region Save and Update Golfer Info
        /// <summary>
        /// Created By:Arun
        /// Created Date: 23 March 2015
        /// Purpose: Add or update golfer Info.
        /// </summary>
        /// <param name="objReg"></param>
        /// <returns></returns>
        public Result AddOrUpdateGolferInfo(GF_Golfer objReg)
        {
            bool isMail = false;
            try
            {
                _db = new GolflerEntities();
                if (objReg.GF_ID == 0)
                {
                    var user = _db.GF_Golfer.FirstOrDefault(p => p.Email.ToLower() == objReg.Email.ToLower() &&
                        p.Status != StatusType.Delete);
                    if (user != null)
                    {
                        return new Result { Id = 0, Status = 0, Error = "User already exists." };
                    }

                    objReg.CreatedOn = DateTime.Now;
                    objReg.Status = StatusType.Active;
                    objReg.Salt = CommonLibClass.FetchRandStr(3);
                    objReg.Password = CommonFunctions.EncryptString(objReg.Password, objReg.Salt);

                    #region image code
                    if (!string.IsNullOrEmpty(objReg.Image))
                    {
                        byte[] bt = Convert.FromBase64String(objReg.Image);
                        MemoryStream ms = new MemoryStream(bt);
                        System.Drawing.Image img = System.Drawing.Image.FromStream(ms);
                        var imageName = DateTime.Now.Ticks + "-Golfer";
                        img.Save(HttpContext.Current.Server.MapPath("~/Upload/Golferimages/" + imageName + ".jpg"), System.Drawing.Imaging.ImageFormat.Jpeg);
                        var imagePath = "/Upload/Golferimages/" + imageName + ".jpg";
                        objReg.Image = imagePath;
                    }
                    #endregion

                    objReg.IsOnline = true;
                    objReg.IsReceiveEmail = true;
                    objReg.IsReceivePushNotification = true;
                    objReg.IsReceiveEmailGolfer = true;
                    objReg.IsReceivePushNotificationGolfer = true;
                    objReg.measurement = "yards";
                    objReg.speed = "mph";
                    objReg.temperature = "fahrenheit"; 
                    _db.GF_Golfer.Add(objReg);
                    _db.SaveChanges();


                    #region Send Mail To golfer
                    if (objReg.IsReceiveEmail ?? true)
                    {
                        AscynMails.callingAscynMails(objReg);
                    }
                    #endregion
                    //
                    //Get course Detail based on location
                    var lstCourse = _db.GF_SP_GetCourseLocation(objReg.Latitude, objReg.Longitude, Convert.ToDouble(ConfigClass.Distance));

                    string Tee = CommonFunctions.GetTeeValue(Convert.ToInt32(objReg.Tee));
                    return new Result
                    {
                        Id = objReg.GF_ID,
                        Status = 1,
                        Error = "You have been registered successfully.",
                        record = new
                        {
                            objReg.GF_ID,
                            objReg.FirstName,
                            objReg.LastName,
                            objReg.Email,
                            objReg.Latitude,
                            objReg.Longitude,
                            objReg.MobileNo,
                            objReg.CreatedOn,
                            Image = objReg.Image == null ? (ConfigClass.ImageSiteUrl + "/Images/chat%20icons/768/chat_golfer.png") : (ConfigClass.ImageSiteUrl + objReg.Image),

                            objReg.IsReceivePushNotification,
                            objReg.IsReceiveEmail,
                            objReg.IsReceivePushNotificationGolfer,
                            objReg.IsReceiveEmailGolfer,
                            Tee,
                            Measurement = string.IsNullOrEmpty(objReg.measurement) ? "yards" : objReg.measurement,
                            Speed = string.IsNullOrEmpty(objReg.speed) ? "mph" : objReg.speed,
                            Temperature = string.IsNullOrEmpty(objReg.temperature) ? "fahrenheit" : objReg.temperature,
                            Courses = lstCourse

                        }
                    };

                }
                else
                {
                    if (objReg.GF_ID < 0)
                    {
                        return new Result { Error = "Id is either null or less than or equal to 0.", Id = 0, record = null, Status = 0 };
                    }
                    var objGolfer = _db.GF_Golfer.FirstOrDefault(p => p.GF_ID == objReg.GF_ID);
                    if (objGolfer != null)
                    {
                        objGolfer.DeviceSerialNo = Convert.ToString(objReg.DeviceSerialNo);
                        objGolfer.FirstName = objReg.FirstName;
                        objGolfer.LastName = objReg.LastName;
                        objGolfer.MobileNo = objReg.MobileNo ?? objGolfer.MobileNo;
                        objGolfer.Latitude = objReg.Latitude;
                        objGolfer.Longitude = objReg.Longitude;
                        objGolfer.IsReceivePushNotification = objReg.IsReceivePushNotification;
                        objGolfer.IsReceiveEmail = objReg.IsReceiveEmail;
                        objGolfer.IsReceivePushNotificationGolfer = objReg.IsReceivePushNotificationGolfer;
                        objGolfer.IsReceiveEmailGolfer = objReg.IsReceiveEmailGolfer;
                        var objGolferDuplicate = _db.GF_Golfer.FirstOrDefault(p => p.GF_ID != objReg.GF_ID && p.Email == objReg.Email);
                        if (objGolferDuplicate != null)
                        {
                            return new Result { Id = 0, Status = 0, Error = "Email already exists.", record = null };
                        }
                        if (objGolfer.Email != objReg.Email)
                        {
                            isMail = true;
                        }
                        objGolfer.Email = objReg.Email;
                        objGolfer.Tee = objReg.Tee;

                        if (objReg.DeviceType.ToLower().Trim() == "ios")
                        {
                            objGolfer.GCMID = string.Empty;
                            objGolfer.APNID = objReg.APNID;
                            objGolfer.DeviceType = objReg.DeviceType;
                        }
                        else
                        {
                            objGolfer.APNID = string.Empty;
                            objGolfer.GCMID = objReg.GCMID;
                            objGolfer.DeviceType = objReg.DeviceType;
                        }
                        #region image code
                        if (!string.IsNullOrEmpty(objReg.Image))
                        {
                            byte[] bt = Convert.FromBase64String(objReg.Image);
                            MemoryStream ms = new MemoryStream(bt);
                            System.Drawing.Image img = System.Drawing.Image.FromStream(ms);
                            var imageName = DateTime.Now.Ticks + "-Golfer";
                            img.Save(HttpContext.Current.Server.MapPath("~/Upload/Golferimages/" + imageName + ".jpg"), System.Drawing.Imaging.ImageFormat.Jpeg);
                            var imagePath = "/Upload/Golferimages/" + imageName + ".jpg";

                            objGolfer.Image = imagePath;
                        }
                        #endregion

                        if (CommonFunctions.DecryptString(objGolfer.Password, objGolfer.Salt) != objReg.Password)
                        {
                            objGolfer.Salt = CommonLibClass.FetchRandStr(3);
                            objGolfer.Password = CommonFunctions.EncryptString(objReg.Password, objGolfer.Salt);
                            isMail = true;
                        }
                        _db.SaveChanges();

                        #region Send Mail To golfer
                        if ((objReg.IsReceiveEmail ?? true) && isMail)
                        {
                            //AscynMails.callingAscynMails(objReg);
                        }
                        #endregion
                        //Get course Detail based on location
                        var lstCourse = _db.GF_SP_GetCourseLocation(objReg.Latitude, objReg.Longitude, Convert.ToDouble(ConfigClass.Distance));
                        string Tee = CommonFunctions.GetTeeValue(Convert.ToInt32(objReg.Tee));
                        return new Result
                        {
                            Id = objReg.GF_ID,
                            Status = 1,
                            Error = "Your profile has been updated successfully.",
                            record = new
                            {
                                objGolfer.GF_ID,
                                objGolfer.FirstName,
                                objGolfer.LastName,
                                objGolfer.Email,
                                objGolfer.Latitude,
                                objGolfer.Longitude,
                                objGolfer.MobileNo,
                                objGolfer.CreatedOn,

                                Image = objGolfer.Image == null ? (ConfigClass.ImageSiteUrl + "/Images/chat%20icons/768/chat_golfer.png") : (ConfigClass.ImageSiteUrl + objGolfer.Image),
                                objReg.IsReceivePushNotification,
                                objReg.IsReceiveEmail,
                                objReg.IsReceivePushNotificationGolfer,
                                objReg.IsReceiveEmailGolfer,
                                Tee,
                                Measurement = string.IsNullOrEmpty(objReg.measurement) ? "yards" : objReg.measurement,
                                Speed = string.IsNullOrEmpty(objReg.speed) ? "mph" : objReg.speed,
                                Temperature = string.IsNullOrEmpty(objReg.temperature) ? "fahrenheit" : objReg.temperature,
                                Courses = lstCourse
                            }
                        };
                    }
                    else
                    {
                        return new Result { Id = objReg.GF_ID, Status = 0, Error = "User does not exists.", record = null };
                    }
                }
            }
            catch (Exception ex)
            {
                return new Result { Id = 0, Status = 0, Error = ex.Message, record = null };
            }

        }

        #endregion

        #region Forgot Password
        /// <summary>
        /// Created By:Arun
        /// Created Date: 24 March 2015
        /// Purpose: Forgot Password location
        /// </summary>
        /// <param name="Email"></param>
        /// <returns></returns>
        public Result ForgotPassword(string Email)
        {
            try
            {
                _db = new GolflerEntities();
                GF_Golfer reg = _db.GF_Golfer.FirstOrDefault(p => p.Email.ToLower() == Email.ToLower());
                if (reg == null)
                {
                    return new Result { Id = 0, Status = 0, Error = "User does not exists.", record = null };
                }
                else
                {
                    if (reg.Status.Trim() != StatusType.Active)
                    {
                        return new Result { Status = 0, Error = "Your account has been deactivated. Please contact to administrator.", record = null };
                    }

                    //string _salt = reg.Salt;
                    //string newPassword = CommonLibClass.FetchRandStr(8);
                    //string _password = "";
                    //CommonFunctions.GeneratePassword(newPassword, "old", ref _salt, ref _password);
                    //reg.Password = _password;
                    //if (_db.SaveChanges() > 0)
                    //{
                    IQueryable<GF_EmailTemplatesFields> templateFields = null;
                    // var param = EmailParams.GetEmailParams(ref _db, "Forgot Password", ref templateFields);
                    long courseid = 0;
                    try
                    {
                        courseid = Convert.ToInt64(_db.GF_GolferUser.FirstOrDefault(x => x.GolferID == reg.GF_ID).CourseID);
                    }
                    catch
                    {
                        courseid = 0;
                    }

                    string mailresult = "";

                    var param = EmailParams.GetEmailParamsNew(ref _db, "Forgot Password", ref templateFields, courseid, "G", ref mailresult, true);


                    if (param != null)
                    {
                        if (templateFields != null)
                        {

                            if (!ApplicationEmails.GolferUserForGotMail(reg, CommonFunctions.DecryptString(reg.Password, reg.Salt), param, ref templateFields, Convert.ToInt64(courseid)))
                            {
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
                    }
                    else
                    {
                        return new Result { Status = 0, Error = "An error occured while sending mail.", record = null };
                    }
                    //}
                    //else
                    //{
                    //    return new Result { Status = 0, Error = "An error occured while saving your entries.Please try again after some time.", record = null };
                    //}

                    ////if (CommonFunctions.SendMailForgot(reg, newPassword))
                    ////{
                    ////    if (_db.SaveChanges() > 0)
                    ////    {
                    ////        return new Result { Status = 1, Error = "Your new password is sent on your email address. Please login with new password.", record = null };
                    ////    }
                    ////    else
                    ////    {
                    ////        return new Result { Status = 0, Error = "An error occured while saving your entries.Please try again after some time.", record = null };
                    ////    }
                    ////}
                    ////else
                    ////{
                    ////    return new Result { Status = 0, Error = "An error occured while sending mail.", record = null };
                    ////}

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
        /// Created by:Arun
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
                var objUser = _db.GF_Golfer.FirstOrDefault(x => x.GF_ID == userid);
                if (objUser != null)
                {
                    if (objUser.DeviceType == "ios")
                        objUser.APNID = string.Empty;
                    else
                        objUser.GCMID = string.Empty;
                }
                objUser.IsOnline = false;
                objUser.AppVersion = 0;
                _db.SaveChanges();

                #region Quit from all Game Round
                var allActiveGameRound = _db.GF_GamePlayRound.Where(y => y.GolferID == userid && y.IsQuit == false).ToList();
                foreach (var gameplay in allActiveGameRound)
                {
                    gameplay.IsQuit = true;
                    _db.SaveChanges();
                }
                #endregion

                #region remove data for changing location
                //Clear GF_GolferChangingLocation table for golfer
                var deleteGolferChangingLocation = _db.GF_GolferChangingLocation.Where(y => y.GolferId == userid);
                foreach (var golferChangingLocation in deleteGolferChangingLocation)
                {
                    _db.GF_GolferChangingLocation.Remove(golferChangingLocation);
                }
                _db.SaveChanges();
                #endregion

                #region delete relation with course
                var lstCourseRelation = _db.GF_GolferUser.Where(x => x.GolferID == userid).ToList();
                foreach (var rel in lstCourseRelation)
                {
                    _db.GF_GolferUser.Remove(rel);
                    _db.SaveChanges();
                }
                #endregion

                #region delete from paceofplay
                var lstpaceofplay = _db.GF_GolferPaceofPlay.Where(x => x.GolferId == userid && x.Status != StatusType.Delete).ToList();

                foreach (var rel in lstpaceofplay)
                {
                    rel.Status = StatusType.Delete;
                    _db.SaveChanges();
                }
                #endregion

                #region delete from paceofplay Temp
                var lstpaceofplayTemp = _db.GF_GolferPaceofPlay_Temp.Where(x => x.GolferId == userid).ToList();
                foreach (var rel in lstpaceofplayTemp)
                {
                    _db.GF_GolferPaceofPlay_Temp.Remove(rel);
                }
                _db.SaveChanges();
                #endregion

                return new Result { Id = 0, Status = 1, Error = "Logout Successfully." };
            }
            catch (Exception ex)
            {
                ErrorClass.WriteLog(ex, HttpContext.Current.Request);
                return new Result { Id = 0, Status = 0, Error = ex.Message };
            }

        }

        #endregion

        #region Update Golfer Location Info
        /// <summary>
        /// Created By:Veera
        /// Created Date: 3 April 2015
        /// Purpose:Update Golfer Location Info
        /// </summary>
        /// <param name="objReg"></param>
        /// <returns></returns>
        public Result UpdateGolferLocation(GF_GolferChangingLocation objLoc)
        {
            try
            {
                _db = new GolflerEntities();
                _db.GF_GolferChangingLocation.Add(objLoc);
                _db.SaveChanges();
                return new Result
                {
                    Id = objLoc.Id,
                    Status = 1,
                    Error = "Success.",
                    record = null
                };
            }
            catch (Exception ex)
            {
                return new Result { Id = 0, Status = 0, Error = ex.Message, record = null };
            }

        }

        #endregion

        #region GolferByCourseId List
        ///// <summary>
        ///// Created By: Veera
        ///// Created Date: 03 April 2015
        ///// Purpose: List of Orders History
        ///// </summary>
        ///// <param name="orderHistory"></param>
        ///// <returns></returns>
        public GolferMapResult GetGolferByCourseIdList(string CourseId)
        {
            try
            {
                _db = new GolflerEntities();

                var lstOrders = _db.GF_SP_GetGolfersListingByCourseId(Convert.ToInt64(CourseId)).ToList();
                foreach (var item in lstOrders)
                {
                    string CurrentHole = "";
                    string CurrentHoleTime = "";
                    string TotalTimeSpend = "";
                    Dictionary<string, string> HoleTimings = new Dictionary<string, string>();
                    string AverageTime = "";
                    string Round = "";

                    #region Golfer Logout Status check
                    try
                    {
                        CommonFunctions.GolferAutoLogout(Convert.ToInt64(CourseId));
                    }
                    catch (Exception ex)
                    {
                        // ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
                    }
                    #endregion

                    CommonFunctions.GetGolferHoleInformation(Convert.ToInt64(CourseId), Convert.ToInt64(item.Id), ref CurrentHole, ref CurrentHoleTime, ref TotalTimeSpend, ref HoleTimings, ref Round, ref AverageTime);

                    if (Round == "No round started yet.")
                    {
                        if (CurrentHole == "" || CurrentHole == "0")
                        {
                            CurrentHole = "N.A." + " (No round started yet)";
                        }
                        else
                        {
                            CurrentHole = CurrentHole + " (No round started yet)";
                        }
                        if (TotalTimeSpend == "")
                        {
                            TotalTimeSpend = "Total Time: " + "n.a.";
                        }
                        else
                        {
                            TotalTimeSpend = "Total Time: " + TotalTimeSpend;
                        }
                    }
                    else
                    {
                        if (CurrentHole == "" || CurrentHole == "0")
                        {
                            CurrentHole = "N.A.";
                        }

                        if (AverageTime == "")
                        {
                            TotalTimeSpend = "Pace: " + "n.a.";
                        }
                        else
                        {
                            TotalTimeSpend = "Pace: " + AverageTime;
                        }
                    }

                    item.CurrentHole = CurrentHole;

                    item.TotalTimeSpend = TotalTimeSpend;

                    // item.CurrentHoleTime = CurrentHoleTime; // TEMP DOWN THIS AND SET IT TO TOTAL TIME SPEND
                    item.CurrentHoleTime = TotalTimeSpend;

                    item.HoleTimings = HoleTimings;

                    if (item.Hexa == "5")
                    {

                        System.Drawing.Color red = System.Drawing.ColorTranslator.FromHtml("#0000FF");
                        var HEXColor = System.Drawing.ColorTranslator.ToHtml(red); //Generate random HEX color code
                        var RGBColorAll = System.Drawing.ColorTranslator.FromHtml(HEXColor); //Convert HEX color code into RGB color code
                        var RGBColor = RGBColorAll.R + "," + RGBColorAll.G + "," + RGBColorAll.B; //Convert HEX color code into RGB color code
                        var HUEColor = System.Drawing.Color.FromArgb(RGBColorAll.R, RGBColorAll.G, RGBColorAll.B).GetHue(); //Convert RGB color code into HUE color code
                        item.Hexa = HEXColor;
                        item.RGB = Convert.ToString(RGBColor);
                        item.HUE = Convert.ToString(HUEColor);

                    }
                    else if (item.Hexa == "10")
                    {
                        System.Drawing.Color red = System.Drawing.ColorTranslator.FromHtml("#FFFF00");
                        var HEXColor = System.Drawing.ColorTranslator.ToHtml(red); //Generate random HEX color code
                        var RGBColorAll = System.Drawing.ColorTranslator.FromHtml(HEXColor); //Convert HEX color code into RGB color code
                        var RGBColor = RGBColorAll.R + "," + RGBColorAll.G + "," + RGBColorAll.B; //Convert HEX color code into RGB color code
                        var HUEColor = System.Drawing.Color.FromArgb(RGBColorAll.R, RGBColorAll.G, RGBColorAll.B).GetHue(); //Convert RGB color code into HUE color code
                        item.Hexa = HEXColor;
                        item.RGB = Convert.ToString(RGBColor);
                        item.HUE = Convert.ToString(HUEColor);
                    }
                    else if (item.Hexa == "20")
                    {
                        System.Drawing.Color red = System.Drawing.ColorTranslator.FromHtml("#FF0000");
                        var HEXColor = System.Drawing.ColorTranslator.ToHtml(red); //Generate random HEX color code
                        var RGBColorAll = System.Drawing.ColorTranslator.FromHtml(HEXColor); //Convert HEX color code into RGB color code
                        var RGBColor = RGBColorAll.R + "," + RGBColorAll.G + "," + RGBColorAll.B; //Convert HEX color code into RGB color code
                        var HUEColor = System.Drawing.Color.FromArgb(RGBColorAll.R, RGBColorAll.G, RGBColorAll.B).GetHue(); //Convert RGB color code into HUE color code
                        item.Hexa = HEXColor;
                        item.RGB = Convert.ToString(RGBColor);
                        item.HUE = Convert.ToString(HUEColor);
                    }
                    else
                    {

                    }
                }

                #region Course Holes Detail

                long cID = Convert.ToInt64(CourseId);

                var courseBuilderID = _db.GF_CourseBuilder.FirstOrDefault(x => x.CourseID == cID && x.CoordinateType == "O");

                var courseRec = _db.GF_CourseBuilderRecDates.FirstOrDefault(x => x.RecDate == DateTime.Now);
                long? courseBuilderId = (courseRec != null ? courseRec.CourseBuilderId : (courseBuilderID != null ? courseBuilderID.ID : 0));

                var lstCourseHole = _db.GF_CourseBuilderHolesDetail.Where(x => x.CourseBuilderID == (courseBuilderId ?? 0)).ToList()
                                    .Select(x => new
                                    {
                                        x.HoleNumber,
                                        x.Latitude,
                                        x.Longitude,
                                        DragItemType = DragItemType.GetFullDragItemType(x.DragItemType)
                                    });

                //var lstCourseHole = courseBuilderID == null ? null :
                //    _db.GF_CourseBuilderHolesDetail.Where(x => x.CourseBuilderID == (courseRec != null ? courseRec.CourseBuilderId : courseBuilderID.ID)).ToList()
                //                                   .Select(x => new
                //                                   {
                //                                       x.HoleNumber,
                //                                       x.Latitude,
                //                                       x.Longitude,
                //                                       DragItemType = DragItemType.GetFullDragItemType(x.DragItemType)
                                                   //});

                #endregion

                if (lstOrders.Count() > 0 || (lstCourseHole != null && lstCourseHole.Count() > 0))
                {
                    return new GolferMapResult
                    {
                        Status = 1,
                        Error = "Success",
                        Golfers = new
                        {
                            Orders = lstOrders,
                            CourseHole = lstCourseHole
                        }
                    };
                }
                else
                {
                    return new GolferMapResult
                    {

                        Status = 0,
                        Error = "No Golfer data.",
                        Golfers = null
                    };
                }
            }
            catch (Exception ex)
            {
                return new GolferMapResult
                {

                    Status = 0,
                    Error = ex.Message,
                    Golfers = null
                };
            }
        }

        /// <summary>
        /// Created By: Ramesh Kalra
        ///  Created Date: 12 May 2015
        ///  Purpose: Get Golfers location with Hole Position
        /// </summary>
        /// <param name="CourseId"></param>
        /// <returns></returns>
        public GolferMapResult GetGolfersPositionByCourseId(string CourseId)
        {
            try
            {
                _db = new GolflerEntities();

                var lstGolfers = _db.GF_SP_GetGolfersListingByCourseId(Convert.ToInt64(CourseId)).ToList();
                foreach (var item in lstGolfers)
                {

                    string CurrentHole = "";
                    string CurrentHoleTime = "";
                    string TotalTimeSpend = "";
                    Dictionary<string, string> HoleTimings = new Dictionary<string, string>();
                    string AverageTime = "";
                    string Round = "";

                    CommonFunctions.GetGolferHoleInformation(Convert.ToInt64(CourseId), Convert.ToInt64(item.Id), ref CurrentHole, ref CurrentHoleTime, ref TotalTimeSpend, ref HoleTimings, ref Round, ref AverageTime);

                    if (Round == "No round started yet.")
                    {
                        if (CurrentHole == "" || CurrentHole == "0")
                        {
                            CurrentHole = "N.A." + " (No round started yet)";
                        }
                        else
                        {
                            CurrentHole = CurrentHole + " (No round started yet)";
                        }
                        if (TotalTimeSpend == "")
                        {
                            TotalTimeSpend = "Total Time: " + "n.a.";
                        }
                        else
                        {
                            TotalTimeSpend = "Total Time: " + TotalTimeSpend;
                        }
                    }
                    else
                    {
                        if (CurrentHole == "" || CurrentHole == "0")
                        {
                            CurrentHole = "N.A.";
                        }

                        if (AverageTime == "")
                        {
                            TotalTimeSpend = "Pace: " + "n.a.";
                        }
                        else
                        {
                            TotalTimeSpend = "Pace: " + AverageTime;
                        }
                    }


                    item.CurrentHole = CurrentHole;

                    item.TotalTimeSpend = TotalTimeSpend;

                    // item.CurrentHoleTime = CurrentHoleTime; // TEMP DOWN THIS AND SET IT TO TOTAL TIME SPEND
                    item.CurrentHoleTime = TotalTimeSpend;

                    item.HoleTimings = HoleTimings;


                    if (item.Hexa == "5")
                    {
                        System.Drawing.Color red = System.Drawing.ColorTranslator.FromHtml("#0000FF");
                        var HEXColor = System.Drawing.ColorTranslator.ToHtml(red); //Generate random HEX color code
                        var RGBColorAll = System.Drawing.ColorTranslator.FromHtml(HEXColor); //Convert HEX color code into RGB color code
                        var RGBColor = RGBColorAll.R + "," + RGBColorAll.G + "," + RGBColorAll.B; //Convert HEX color code into RGB color code
                        var HUEColor = System.Drawing.Color.FromArgb(RGBColorAll.R, RGBColorAll.G, RGBColorAll.B).GetHue(); //Convert RGB color code into HUE color code
                        item.Hexa = HEXColor;
                        item.RGB = Convert.ToString(RGBColor);
                        item.HUE = Convert.ToString(HUEColor);

                    }
                    else if (item.Hexa == "10")
                    {
                        System.Drawing.Color red = System.Drawing.ColorTranslator.FromHtml("#FFFF00");
                        var HEXColor = System.Drawing.ColorTranslator.ToHtml(red); //Generate random HEX color code
                        var RGBColorAll = System.Drawing.ColorTranslator.FromHtml(HEXColor); //Convert HEX color code into RGB color code
                        var RGBColor = RGBColorAll.R + "," + RGBColorAll.G + "," + RGBColorAll.B; //Convert HEX color code into RGB color code
                        var HUEColor = System.Drawing.Color.FromArgb(RGBColorAll.R, RGBColorAll.G, RGBColorAll.B).GetHue(); //Convert RGB color code into HUE color code
                        item.Hexa = HEXColor;
                        item.RGB = Convert.ToString(RGBColor);
                        item.HUE = Convert.ToString(HUEColor);
                    }
                    else if (item.Hexa == "20")
                    {
                        System.Drawing.Color red = System.Drawing.ColorTranslator.FromHtml("#FF0000");
                        var HEXColor = System.Drawing.ColorTranslator.ToHtml(red); //Generate random HEX color code
                        var RGBColorAll = System.Drawing.ColorTranslator.FromHtml(HEXColor); //Convert HEX color code into RGB color code
                        var RGBColor = RGBColorAll.R + "," + RGBColorAll.G + "," + RGBColorAll.B; //Convert HEX color code into RGB color code
                        var HUEColor = System.Drawing.Color.FromArgb(RGBColorAll.R, RGBColorAll.G, RGBColorAll.B).GetHue(); //Convert RGB color code into HUE color code
                        item.Hexa = HEXColor;
                        item.RGB = Convert.ToString(RGBColor);
                        item.HUE = Convert.ToString(HUEColor);
                    }
                    else
                    {

                    }
                }
                if (lstGolfers.Count() > 0)
                {
                    return new GolferMapResult
                    {
                        Status = 1,
                        Error = "Success",
                        Golfers = lstGolfers

                    };
                }
                else
                {
                    return new GolferMapResult
                    {

                        Status = 0,
                        Error = "No order(s) has been placed.",
                        Golfers = null
                    };
                }
            }
            catch (Exception ex)
            {
                return new GolferMapResult
                {

                    Status = 0,
                    Error = ex.Message,
                    Golfers = null
                };
            }
        }

        #endregion

        #region Golfer Prefrence Setting

        /// <summary>
        /// Created By: Amit Kumar
        /// Created on: 10 August 2015
        /// </summary>
        /// <remarks>Save golfer preference setting</remarks>
        public Result SaveGolferPreferenceSetting(GF_Golfer golfer)
        {
            try
            {
                _db = new GolflerEntities();
                var lstGolfer = _db.GF_Golfer.FirstOrDefault(x => x.GF_ID == golfer.GF_ID);

                if (lstGolfer != null)
                {
                    lstGolfer.measurement = golfer.measurement.Trim();
                    lstGolfer.temperature = golfer.temperature.Trim();
                    lstGolfer.speed = golfer.speed.Trim();

                    _db.SaveChanges();

                    return new Result
                    {
                        Id = golfer.GF_ID,
                        Status = 1,
                        Error = "Setting saved sucessfully.",
                        record = new
                        {
                            golfer.GF_ID,
                            golfer.measurement,
                            golfer.temperature,
                            golfer.speed
                        }
                    };
                }
                else
                {
                    return new Result { Id = golfer.GF_ID, Status = 0, Error = "User does not exists.", record = null };
                }
            }
            catch (Exception ex)
            {
                return new Result { Id = 0, Status = 0, Error = ex.Message, record = null };
            }
        }

        #endregion
    }
}