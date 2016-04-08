using Golfler.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Golfler.Repositories;
using System.Configuration;
using System.Web.Script.Serialization;
using System.IO;
using Newtonsoft.Json;


namespace Golfler.Controllers
{
    public class GolferController : Controller
    {
        GolflerEntities Db = new GolflerEntities();

        // 
        // GET: /Golfer/
        GF_Golfer objGolfer = null;
        public GolferController()
        {
            objGolfer = new GF_Golfer();

        }

        public ActionResult Index()
        {
            return View("LogIn");
        }

        #region LogIn

        /// <summary>
        /// Created By:
        /// Creation On:
        /// Description: Get method for Golfer Login
        /// </summary>
        public ActionResult LogIn()
        {
            Session.Clear();

            var obj = new LogInModel() { UserType = UserType.Golfer };
            Session["ForLogin"] = "Golfer";
            if (Request.Cookies["logingolferuser"] != null)
            {
                if (!string.IsNullOrEmpty(Request.Cookies["logingolferuser"].Values["GolferEmail"]) &&
                !string.IsNullOrEmpty(Request.Cookies["logingolferuser"].Values["GolferPwd"]))
                {
                    obj.Email = Request.Cookies["logingolferuser"].Values["GolferEmail"];
                    obj.Password = Request.Cookies["logingolferuser"].Values["GolferPwd"];
                    ViewBag.pwd = obj.Password;

                    obj.KeepMeLogin = true;
                }
            }
            return View(obj);
        }

        /// <summary>
        /// Created By:
        /// Creation On:
        /// Description: Post method for Golfer Login
        /// </summary>
        /// 
        [HttpPost]
        public ActionResult LogIn(LogInModel obj)
        {
            var rUser = new Users();

            //if (ModelState.IsValid)
            //{
            var expireyDate = Convert.ToDateTime("03/26/2015").AddDays(Convert.ToInt32(ConfigurationManager.AppSettings["userid"]));
            if (DateTime.Now > expireyDate || rUser.loginLock(obj))
            {
                ViewBag.ErrorMessage = "There is some problem with application. Please contact your system administrator.";
            }
            else
            {
                obj.UserName = obj.Email;
                obj.IpAddress = Request.ServerVariables["REMOTE_ADDR"];
                if (obj.UserType == null)
                    obj.UserType = UserType.Golfer;

                long golferID = 0;
                if (rUser.GolferLogin(obj, ref golferID))
                {
                    var usr = Db.GF_Golfer.FirstOrDefault(x => x.GF_ID == golferID);
                    LoginInfo.CreateGolferLoginSession(LoginType.Golfer, usr.GF_ID, usr.Email, obj.Password, usr.FirstName, usr.LastName, UserType.Golfer,
                                                rUser.GolferObj.LastLogin, rUser.GolferObj.LastLoginIP, obj.KeepMeLogin);

                    return RedirectToAction("Dashboard");
                }
                else
                {
                    ViewBag.ErrorMessage = rUser.Message;
                }
            }
            return View(obj);
            //}
            //return View(obj);
        }

        /// <summary>
        /// Created By:
        /// Creation On:
        /// Description: method for log off.
        /// </summary>
        public ActionResult LogOff()
        {
            #region Update User Online Status

            CommonFunctions.LogoutGolferUsers();

            #endregion

            Session.Clear();
            return RedirectToAction("LogIn");
        }

        #endregion

        #region Dashboard

        /// <summary>
        /// Created By:
        /// Creation On:
        /// Description: Organization User Dashboard.
        /// </summary>
        [SessionExpireFilterAttribute]
        public ActionResult Dashboard()
        {
            AccessModule(ModuleValues.AllRights);
            try
            {
                Role objRole = new Role();

                #region DashBoard Counts

                int userCount = 0;
                int orderCount = 0;
                int msgCount = 0;
                int resoCount = 0;

                GF_Golfer.GetGolferDashBoardInfo(LoginInfo.GolferUserId, ref userCount, ref orderCount, ref msgCount, ref resoCount);

                ViewBag.NoOfUsers = userCount;
                ViewBag.NoOfOrders = orderCount;
                ViewBag.NoOfReso = resoCount;
                ViewBag.NoOfMsg = msgCount;

                #endregion
                ViewBag.IsGolfer = 1;
                return View(objRole.GetRoleByUserId(LoginInfo.UserId, true));
            }
            catch (Exception ex)
            {

                ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
                return null;
            }
        }


        #endregion

        #region Get Golfer Lsiting

        /// <summary>
        /// Created By:Arun
        /// Created date: 26 March 2015
        /// Purpose: Get all golfer listing
        /// </summary>
        /// <returns></returns>
        [SessionExpireFilterAttribute]
        public ActionResult GolferList()
        {
            //if (LoginInfo.IsSuper)
            AccessModuleForAdminModule(ModuleValues.GolferAdmin);
            //else
            //    AccessModule(ModuleValues.User);

            ViewBag.Message = TempData["Message"];
            ViewBag.TypeFrom = "SuperAdmin";

            return View();
        }

        /// <summary>
        /// Created By:Arun
        /// Creation On:26 March 2015
        /// Description: Get all golfer listing
        /// </summary>
        [SessionExpireFilterAttribute]
        public ActionResult GetGolferList(string searchText, string sidx, string sord, int? page, int? rows)
        {
            try
            {
                var UserObj = new GF_Golfer();
                var totalRecords = 0;
                var rec = UserObj.GetGolferUsers(searchText, sidx,
                                           sord, page ?? 1, rows ?? 10,
                                           ref totalRecords).AsEnumerable().Select((x =>
                                                                     new
                                                                     {
                                                                         //ID = x.GF_ID,
                                                                         //x.Email,
                                                                         //FirstName = x.FirstName + " " + x.LastName,
                                                                         //x.Status,
                                                                         //x.MobileNo,
                                                                         //x.CreatedOn,
                                                                         //x.LastLogin,
                                                                         //DoActive = (x.GF_ID != LoginInfo.UserId),
                                                                         //EID = CommonFunctions.EncryptUrlParam(x.GF_ID)
                                                                         
                                                                         ID = x.GF_ID,
                                                                         x.Email,
                                                                         Name = x.Name,
                                                                         x.Status,
                                                                         x.MobileNo,
                                                                         x.CreatedOn,
                                                                         x.LastLogin,
                                                                         EID = CommonFunctions.EncryptUrlParam(x.GF_ID),
                                                                         x.COURSE_NAME,
                                                                         x.SrNo
                                                                     }
                                                                    ));

                var jsonData = new
                {
                    total = (totalRecords + rows - 1) / rows,
                    page,
                    records = totalRecords,
                    rows = rec.ToList(),
                    id = "ID"
                };
                return Json(jsonData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
                return null;
            }
        }

        #endregion

        #region Update user Status

        /// <summary>
        /// Created By:Arun
        /// Created date: 27 March 2015
        /// Purpose: Update Status
        /// </summary>
        /// <param name="id"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        [SessionExpireFilterAttribute]
        public ActionResult UpdateUserStatus(long id, string status)
        {
            AccessModule(ModuleValues.AllRights);
            try
            {

                return objGolfer.ChangeStatus(id, status)
                    ? Json(new { statusText = "success", module = "Golfer", task = "update", message = !objGolfer.Active ? "Golfer user activated successfully." : "Golfer user deactivated successfully." })
                           : Json(new { statusText = "error", module = "Golfer", task = "update", message = objGolfer.Message });
            }
            catch (Exception ex)
            {
                ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
                return null;
            }

        }

        #endregion

        # region Delete selected Golfer
        /// <summary>
        /// Created By: Arun
        /// Creation On: 27 March 2015
        /// Description: method to delete
        /// </summary>
        [AcceptVerbs(HttpVerbs.Post)]
        [SessionExpireFilterAttribute]
        public ActionResult DeleteGolfer(long[] ids)
        {
            try
            {
                if (ids != null)
                {
                    return objGolfer.DeleteGolfer(ids)
                               ? Json(new { statusText = "success", module = "Golfer", task = "delete" })
                               : Json(new { statusText = "error", module = "Golfer", task = "delete", message = objGolfer.Message });
                }
                return Json(new { statusText = "error", module = "Roles", task = "select" });
            }
            catch (Exception ex)
            {
                ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
                return null;
            }

        }
        #endregion

        #region Golfer Update

        /// <summary>
        /// Created By:Arun
        /// Created Date:27 March 2015
        /// Purpose: Get golfer By ID 
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [SessionExpireFilterAttribute]
        public ActionResult GolferAddUpdate(string eid)
        {
            AccessModule(ModuleValues.AllRights);
            long id = CommonFunctions.DecryptUrlParam(eid);
            if (id > 0)
            {
                objGolfer = objGolfer.GetGolferById(id);
                objGolfer.Active = objGolfer.Status.Trim() == StatusType.Active ? false : true;
                objGolfer.Password = CommonFunctions.DecryptPassword(objGolfer.Password, objGolfer.Salt);
                
                objGolfer.ReceiveEmail = objGolfer.IsReceiveEmail ?? true;
                objGolfer.ReceivePushNotification = objGolfer.IsReceivePushNotification ?? true;
                objGolfer.ReceivePushNotificationGolfer = objGolfer.IsReceivePushNotificationGolfer ?? true;
                objGolfer.ReceiveEmailGolfer = objGolfer.IsReceiveEmailGolfer ?? true;
                objGolfer.ReceiveChat = objGolfer.IsReceiveChatMessages == true ? true : false;
                //    ViewBag.golferIncome = new SelectList(objGolfer.GetGolferIncome(), "ID", "Name", objGolfer.IncomeID);
                objGolfer.Birthdate = (objGolfer.DateOfBirth ?? null) == null ? "" : (objGolfer.DateOfBirth.ToString().Split(' ')[0]);
                objGolfer.LastLogin = Convert.ToDateTime(objGolfer.LastLogin).ToLocalTime();
                objGolfer.Image = ConfigClass.GolferApiImagePath + objGolfer.Image;
                ViewBag.golferIncome = new SelectList(objGolfer.GetGolferIncome(), "ID", "Name", objGolfer.IncomeID);
                ViewBag.TimeZone = new SelectList(CommonFunctions.GetTimeZone(), "ID", "TimeZone_Name", objGolfer.TimeZoneId);
                ViewBag.Measurement = new SelectList(Preferences.GetMeasurement(), "Tag", "Name", objGolfer.measurement);
                ViewBag.Temperature = new SelectList(Preferences.GetTemperature(), "Tag", "Name", objGolfer.temperature);
                ViewBag.Speed = new SelectList(Preferences.GetSpeed(), "Tag", "Name", objGolfer.speed);
            }
            return View(objGolfer);
        }




        /// <summary>
        /// Created By:Arun
        /// Created Date:27 March 2015
        /// Purpose: Update Golfer Info.
        /// </summary>
        /// <returns></returns>
        [SessionExpireFilterAttribute]
        [HttpPost]
        public ActionResult GolferAddUpdate(GF_Golfer obj, HttpPostedFileBase file)
        {
            AccessModule(ModuleValues.AllRights);
            try
            {
                ViewBag.golferIncome = new SelectList(objGolfer.GetGolferIncome(), "ID", "Name", objGolfer.IncomeID);
                if (ModelState.IsValid)
                {
                    bool status = true;

                    #region old image code
                    //if (file == null)
                    //{
                    //    // ModelState.AddModelError("File", "Please Upload Your file");
                    //    // ViewBag.Message = "Please Upload Your file";
                    //}
                    //else if (file.ContentLength > 0)
                    //{
                    //    int MaxContentLength = 1024 * 1024 * 3; //3 MB
                    //    string[] AllowedFileExtensions = new string[] { ".jpg", ".gif", ".png", ".pdf" };

                    //    if (!AllowedFileExtensions.Contains(file.FileName.Substring(file.FileName.LastIndexOf('.'))))
                    //    {
                    //        ModelState.AddModelError("File", "Please file of type: " + string.Join(", ", AllowedFileExtensions));
                    //        ViewBag.Message = "Please file of type: " + string.Join(", ", AllowedFileExtensions);
                    //        status = false;
                    //    }


                    //    else
                    //    {
                    //       // var fileName = "";// Path.GetFileName(file.FileName);
                    //        fileName = obj.FirstName.Substring(0, 1).ToUpper() + "_" + obj.GF_ID.ToString() + Path.GetExtension(file.FileName);
                    //        var path = Path.Combine(Server.MapPath("~/GolferWebApi/GolferWebAPI/Content/Upload/Golferimages"), fileName);
                    //        file.SaveAs(path);
                    //        //Upload/Golferimages
                    //        obj.Image = "Upload/Golferimages/" + fileName;


                    //        var fileName = obj.GF_ID + "_" + getDateTimePart + extension;
                    //        var path = pathAfterUpload + "\\" + fileName;
                    //        obj.Image = "/Upload/Golferimages/" + fileName;

                    //        file.SaveAs(path);

                    //    }
                    //}
                    #endregion

                    #region image code
                    if (file != null)
                    {
                        var dt = DateTime.Now;
                        string getDateTimePart = dt.ToString("yyyyMMdd_hhmmss");

                        var fileAP = file;// Request.Files["profileImage"];

                        if (fileAP != null && fileAP.ContentLength > 0)
                        {
                            var extension = Path.GetExtension(fileAP.FileName);

                            string physicalPath = GetPhysicalPath();



                            var pathAfterUpload = physicalPath + "//GolferWebAPI//Upload//Golferimages";// "D:\\kiran\\Project\\GolferWebAPI\\GolferWebAPI\\Upload\\Golferimages";

                            if (!Directory.Exists(pathAfterUpload))
                            {
                                Directory.CreateDirectory(pathAfterUpload);
                            }


                            var fileName = obj.GF_ID + "_" + getDateTimePart + extension;
                            var path = pathAfterUpload + "\\" + fileName;
                            obj.Image = "/Upload/Golferimages/" + fileName;

                            fileAP.SaveAs(path);
                        }
                    }
                    #endregion
                    if (obj.DateOfBirth >= DateTime.Now)
                    {//date of birth equal to current date
                        ViewBag.Message = "Please enter valid Birthdate.";
                        status = false;
                    }

                    if (status)
                    {
                        string image = "";
                        obj.UpdateGolferInfo(obj, ref image);
                        obj.Image = image;
                        obj.Image = ConfigClass.GolferApiImagePath + image;
                        ViewBag.Message = obj.Message;
                        ViewBag.TimeZone = new SelectList(CommonFunctions.GetTimeZone(), "ID", "TimeZone_Name", obj.TimeZoneId);
                        ViewBag.Measurement = new SelectList(Preferences.GetMeasurement(), "Tag", "Name", obj.measurement);
                        ViewBag.Temperature = new SelectList(Preferences.GetTemperature(), "Tag", "Name", obj.temperature);
                        ViewBag.Speed = new SelectList(Preferences.GetSpeed(), "Tag", "Name", obj.speed);
                    }

                }
            }
            catch (DbEntityValidationException e)
            {
                foreach (var eve in e.EntityValidationErrors)
                {
                }
            }
            return View(obj);
        }


        #endregion

        #region Access Function

        /// <summary>
        /// Created By:
        /// Creation On:
        /// Description: method for list
        /// </summary>
        private void AccessModule(string module)
        {

            //GF_RoleModules m = CommonFunctions.GetGolferAccessModule(module);
            ViewBag.AddFlag = "True";
            ViewBag.UpdateFlag = "True"; // m.UpdateFlag;
            ViewBag.DeleteFlag = "True"; //m.DeleteFlag;
            ViewBag.ReadFlag = "True"; //m.ReadFlag;


        }


        private void AccessModuleForAdminModule(string module)
        {

            //----------------new ....same as in adminController
            if (LoginInfo.IsSuper)
            {
                ViewBag.AddFlag = "True";
                ViewBag.UpdateFlag = "True";
                ViewBag.DeleteFlag = "True";
                ViewBag.ReadFlag = "True";
            }
            else
            {
                GF_RoleModules m = CommonFunctions.GetAccessModule(module);
                if (m.AddFlag)
                {
                    ViewBag.AddFlag = "True";
                }
                else
                {
                    ViewBag.AddFlag = "False";
                }
                if (m.UpdateFlag)
                {
                    ViewBag.UpdateFlag = "True";
                }
                else
                {
                    ViewBag.UpdateFlag = "False";
                }

                if (m.DeleteFlag)
                {
                    ViewBag.DeleteFlag = "True";
                }
                else
                {
                    ViewBag.DeleteFlag = "False";
                }
                if (m.ReadFlag)
                {
                    ViewBag.ReadFlag = "True";
                }
                else
                {
                    ViewBag.ReadFlag = "False";
                }
            }

        }

        #endregion

        #region Manage Golfer Information

        /// <summary>
        /// Created By: Amit Kumar
        /// Created Date: 06 April 2015
        /// Purpose: Get golfer By ID 
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [SessionExpireFilterAttribute]
        public ActionResult GolferEditInformation()
        {
            AccessModule(ModuleValues.AllRights);
            long id = LoginInfo.GolferUserId;//CommonFunctions.DecryptUrlParam(eid);
            if (id > 0)
            {
                objGolfer = objGolfer.GetGolferById(id);
                string physicalPath = GetPhysicalPath();

                //  objGolfer.Image = physicalPath + "/GolferWebAPI" + objGolfer.Image;
                objGolfer.Image = ConfigClass.GolferApiImagePath + objGolfer.Image;
                objGolfer.Active = objGolfer.Status.Trim() == StatusType.Active ? false : true;
                objGolfer.Password = CommonFunctions.DecryptPassword(objGolfer.Password, objGolfer.Salt);
                
                objGolfer.ReceiveEmail = objGolfer.IsReceiveEmail ?? true;
                objGolfer.ReceivePushNotification = objGolfer.IsReceivePushNotification ?? true;
                objGolfer.ReceiveEmailGolfer = objGolfer.IsReceiveEmailGolfer ?? true;
                objGolfer.ReceivePushNotificationGolfer = objGolfer.IsReceivePushNotificationGolfer ?? true;
                objGolfer.ReceiveChat = objGolfer.IsReceiveChatMessages == true ? true : false;

                objGolfer.Birthdate = (objGolfer.DateOfBirth ?? null) == null ? "" : (objGolfer.DateOfBirth.ToString().Split(' ')[0]);
                objGolfer.LastLogin = Convert.ToDateTime(objGolfer.LastLogin).ToLocalTime();
                ViewBag.golferIncome = new SelectList(objGolfer.GetGolferIncome(), "ID", "Name", objGolfer.IncomeID);
                ViewBag.TimeZone = new SelectList(CommonFunctions.GetTimeZone(), "ID", "TimeZone_Name", objGolfer.TimeZoneId);
                ViewBag.Measurement = new SelectList(Preferences.GetMeasurement(), "Tag", "Name", objGolfer.measurement);
                ViewBag.Temperature = new SelectList(Preferences.GetTemperature(), "Tag", "Name", objGolfer.temperature);
                ViewBag.Speed = new SelectList(Preferences.GetSpeed(), "Tag", "Name", objGolfer.speed);
            }
            return View(objGolfer);
        }

        /// <summary>
        /// Created By: Amit Kumar
        /// Created Date: 06 April 2015
        /// Purpose: Update Golfer Info.
        /// </summary>
        /// <returns></returns>
        [SessionExpireFilterAttribute]
        [HttpPost]
        public ActionResult GolferEditInformation(GF_Golfer obj)
        {
            AccessModule(ModuleValues.AllRights);
            try
            {
                if (ModelState.IsValid)
                {
                    if (Request.Files.Count > 0)
                    {
                        var dt = DateTime.Now;
                        string getDateTimePart = dt.ToString("yyyyMMdd_hhmmss");

                        var fileAP = Request.Files["profileImage"];

                        if (fileAP != null && fileAP.ContentLength > 0)
                        {
                            var extension = Path.GetExtension(fileAP.FileName);

                            string physicalPath = GetPhysicalPath();

                            //string[] splitPhPath = Server.HtmlEncode(Request.PhysicalApplicationPath).Split('\\');


                            //for (int i = 0; i < splitPhPath.Length-2; i++)
                            //{
                            //    if (physicalPath != "")
                            //        physicalPath += "//";
                            //    physicalPath += splitPhPath[i].ToString();
                            //}

                            var pathAfterUpload = physicalPath + "//GolferWebAPI//Upload//Golferimages";// "D:\\kiran\\Project\\GolferWebAPI\\GolferWebAPI\\Upload\\Golferimages";

                            if (!Directory.Exists(pathAfterUpload))
                            {
                                Directory.CreateDirectory(pathAfterUpload);
                            }

                            //if (!Directory.Exists(
                            //        Server.MapPath("/Uploads/AdminProfileImage/" + obj.Image)))
                            //{
                            //    Directory.CreateDirectory(
                            //        Server.MapPath("/Uploads/AdminProfileImage/" + obj.Image));
                            //}

                            var fileName = obj.GF_ID + "_" + getDateTimePart + extension;

                            // var path = Path.Combine(Server.MapPath(pathAfterUpload), fileName);
                            var path = pathAfterUpload + "\\" + fileName;

                            obj.Image = "/Upload/Golferimages/" + fileName;

                            fileAP.SaveAs(path);
                        }
                    }

                    string image = "";
                    obj.UpdateGolferInfo(obj, ref image);
                    obj.Image = ConfigClass.GolferApiImagePath + image;
                    ViewBag.Message = obj.Message;

                    ViewBag.golferIncome = new SelectList(objGolfer.GetGolferIncome(), "ID", "Name", objGolfer.IncomeID);
                    ViewBag.TimeZone = new SelectList(CommonFunctions.GetTimeZone(), "ID", "TimeZone_Name", objGolfer.TimeZoneId);
                    ViewBag.Measurement = new SelectList(Preferences.GetMeasurement(), "Tag", "Name", objGolfer.measurement);
                    ViewBag.Temperature = new SelectList(Preferences.GetTemperature(), "Tag", "Name", objGolfer.temperature);
                    ViewBag.Speed = new SelectList(Preferences.GetSpeed(), "Tag", "Name", objGolfer.speed);
                }
            }
            catch (DbEntityValidationException e)
            {
                foreach (var eve in e.EntityValidationErrors)
                {
                }
            }
            return View(obj);
        }

        public string GetPhysicalPath()
        {
            string physicalPath = "";

            string[] splitPhPath = Server.HtmlEncode(Request.PhysicalApplicationPath).Split('\\');


            for (int i = 0; i < splitPhPath.Length - 2; i++)
            {
                if (physicalPath != "")
                    physicalPath += "//";
                physicalPath += splitPhPath[i].ToString();
            }
            return physicalPath;

        }
        #endregion

        #region Manage Order

        /// <summary>
        /// Created By: Amit Kumar
        /// Creation On: 06 April 2015
        /// Description: method for list
        /// </summary>
        [SessionExpireFilterAttribute]
        [HttpGet]
        public ActionResult OrderHistoryList()
        {
            AccessModule(ModuleValues.AllRights);
            ViewBag.Message = TempData["Message"];

            return View();
        }

        /// <summary>
        /// Created By: Amit Kumar
        /// Creation On: 06 April 2015
        /// Description: method to bind grid
        /// </summary>
        [SessionExpireFilterAttribute]
        public ActionResult GetOrderHistoryList(string searchText, string fromDate, string toDate,
            string sidx, string sord, int? page, int? rows, string orderType, long? courseID, string orderAmount, string ccNumber)
        {
            try
            {
                string timeZone = CommonFunctions.GetGolferTimeZone(LoginInfo.GolferUserId);

                var OrderObj = new Order();
                var totalRecords = 0;
                var rec = OrderObj.GetOrderHistoryNew(searchText, fromDate, toDate, orderAmount, sidx,
                                           sord, page ?? 1, rows ?? 10, orderType, courseID, ccNumber,
                                           ref totalRecords).AsEnumerable().Select((x =>
                                                                     new
                                                                     {
                                                                         ID = x.ID,
                                                                         OrderDate = CommonFunctions.DateByTimeZone(timeZone, x.OrderDate ?? DateTime.UtcNow).ToString("MM/dd/yyyy hh:mm tt"),
                                                                         time = CommonFunctions.DateByTimeZone(timeZone, x.OrderDate ?? DateTime.UtcNow).ToShortTimeString(),//x.OrderDate.Value.ToShortTimeString(),
                                                                         golferName = x.GF_Golfer.FirstName + " " + x.GF_Golfer.LastName,
                                                                         cartieName = ((x.CartieId ?? 0) != 0 ? ((Db.GF_AdminUsers.FirstOrDefault(y => y.ID == x.CartieId).FirstName + " " +
                                                                                                                  Db.GF_AdminUsers.FirstOrDefault(y => y.ID == x.CartieId).LastName)) : ""),
                                                                         billAmount = x.GF_OrderDetails.Sum(y => ((y.UnitPrice ?? 0) * (y.Quantity ?? 0))).ToString("F"),
                                                                         TaxAmount = (x.TaxAmount ?? 0).ToString("F"),
                                                                         GolferPlatformFee = (x.GolferPlatformFee ?? 0).ToString("F"),
                                                                         GrandTotal = "$" + (x.GrandTotal ?? 0).ToString("F"),//((x.GF_OrderDetails.Sum(y => (y.UnitPrice * y.Quantity))) + x.TaxAmount + x.GolferPlatformFee).ToString(),
                                                                         itemOrdered = new JavaScriptSerializer().Serialize(x.GF_OrderDetails.ToList().Select(y =>
                                                                                                                            new
                                                                                                                            {
                                                                                                                                y.GF_MenuItems.Name,
                                                                                                                                UnitPrice = y.GF_MenuItems.Amount,
                                                                                                                                y.Quantity,
                                                                                                                                Amount = (y.GF_MenuItems.Amount * y.Quantity),
                                                                                                                                MenuOptionName = y.GF_OrderMenuOptionDetail.Select(q => q.MenuOptionName).ToList()
                                                                                                                            })),
                                                                         //OrderType = orderType == "P" ? (((x.IsDelivered ?? false) || (x.IsPickup ?? false)) ? "Paid" : "Pending") : (orderType == "M" ? "Membership" : "Refunded"),
                                                                         OrderType = x.PaymentType == "0" ? "Membership" : (((x.IsDelivered ?? false) || (x.IsPickup ?? false)) ? "Paid" : (x.OrderStatus.Contains("Failed") ? x.OrderStatus : "Pending")),
                                                                         PopOrderGolferName = x.GF_Golfer.FirstName + " " + x.GF_Golfer.LastName,
                                                                         PopOrderStatus = ((x.IsDelivered ?? false) || (x.IsPickup ?? false)) ? "Paid" : (x.OrderStatus.Contains("Failed") ? x.OrderStatus : "Pending"),
                                                                         CourseInfo = x.GF_CourseInfo.COURSE_NAME,
                                                                         CourseAddress = x.GF_CourseInfo.ADDRESS,

                                                                         TransId = (orderType == "M" ? x.MemberShipID : (x.BT_TransId == null ? "" : x.BT_TransId)),
                                                                         WeatherDetails = "",
                                                                         //WeatherDetails = new JavaScriptSerializer().Serialize(Db.GF_WeatherDetails.Where(y => y.OrderID == x.ID).ToList().Select(y =>
                                                                         //                                                   new
                                                                         //                                                   {
                                                                         //                                                       y.Humidity,
                                                                         //                                                       y.Pressure,
                                                                         //                                                       y.Temp,
                                                                         //                                                       y.TempMax,
                                                                         //                                                       y.TempMin,
                                                                         //                                                       y.WeatherDescription,
                                                                         //                                                       y.WindSpeed
                                                                         //                                                   })),
                                                                         //PreparedByType = (string.Join(",", x.GF_OrderDetails.ToList()
                                                                         //                   .Select(y => y.GF_MenuItems.GF_SubCategory.GF_Category.Type)
                                                                         //                   .Distinct().ToList())).ToLower() == FoodCategoryType.Proshop.ToLower() ? "Proshop" : "Kitchen",
                                                                         PreparedByType = CommonFunctions.GetPreparedByType(x),
                                                                         PreparedBy = CommonFunctions.GetPreparedBy(x),
                                                                         //PreparedBy = (string.Join(",", x.GF_OrderDetails.ToList()
                                                                         //             .Select(y => y.GF_MenuItems.GF_SubCategory.GF_Category.Type)
                                                                         //             .Distinct().ToList())).ToLower() == FoodCategoryType.Proshop.ToLower() ?
                                                                         //             ((Db.GF_AdminUsers.FirstOrDefault(k => k.ID == x.ProShopID) == null ? " " : (Db.GF_AdminUsers.FirstOrDefault(k => k.ID == x.ProShopID).FirstName))) : ((Db.GF_AdminUsers.FirstOrDefault(k => k.ID == x.KitchenId) == null ? " " : (Db.GF_AdminUsers.FirstOrDefault(k => k.ID == x.KitchenId).FirstName))),
                                                                         OrderTypeHeader = OrderType.GetFullOrderType(x.OrderType),
                                                                         PaymentMode = (x.PaymentType == "1" ? "Payment by Card" : "Payment by MembershipId"),
                                                                         //  PromoCode = x.GF_PromoCode == null ? 0 : x.GF_PromoCode.Discount
                                                                         PromoCode = x.GF_PromoCode == null ? "0.00" : (x.DiscountAmt ?? 0).ToString("F"),
                                                                         TransID = x.BT_TransId,
                                                                         MembershipID = x.MemberShipID,
                                                                         x.BT_ResponseText,
                                                                         EID = CommonFunctions.EncryptUrlParam(x.ID)
                                                                     }
                                                                    ));

                var jsonData = new
                {
                    total = (totalRecords + rows - 1) / rows,
                    page,
                    records = totalRecords,
                    rows = rec.ToList(),
                    id = "ID"
                };
                return Json(jsonData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
                return null;
            }
        }

        #endregion

        #region Get OrderPopupInfo
        /// <summary>
        /// Created By: Amit Kumar
        /// Creation On: 06 April 2015
        /// Description: method for list
        /// </summary>
        [HttpPost]
        public bool OrderHistoryView(string orderId)
        {
            AccessModule(ModuleValues.AllRights);
            bool result = false;
            try
            {
                if (orderId == "")
                {
                    result = false;
                    return result;
                }

                long id = Convert.ToInt64(orderId);


                GF_Order obj = new GF_Order();
                obj = Db.GF_Order.FirstOrDefault(x => x.ID == id);
                if (obj != null)
                {
                    ViewBag.PopOrderNo = obj.ID;
                    ViewBag.PopOrderDate = obj.CreatedDate;
                    ViewBag.PopOrderGolferName = obj.GF_Golfer.FirstName + " " + obj.GF_Golfer.LastName;
                    ViewBag.PopOrderStatus = obj.IsDelivered == true ? "Paid" : "Pending";

                    GF_CourseInfo objCourse = new GF_CourseInfo();
                    objCourse = Db.GF_CourseInfo.FirstOrDefault(y => y.ID == obj.CourseID);
                    ViewBag.PopOrderCourseName = objCourse.COURSE_NAME;
                    ViewBag.PopOrderCourseAddress = objCourse.ADDRESS;
                    ViewBag.PopOrderNo = obj.ID;
                    ViewBag.PopOrderNo = obj.ID;

                    decimal billAmount = Convert.ToDecimal(obj.GF_OrderDetails.Sum(y => (y.UnitPrice * y.Quantity)));
                    decimal TaxAmount = Convert.ToDecimal(obj.TaxAmount);
                    decimal GolferPlatformFee = Convert.ToDecimal(obj.GolferPlatformFee);
                    decimal PromoCode = 0;
                    decimal Total = Convert.ToDecimal(billAmount + TaxAmount + GolferPlatformFee);
                    ViewBag.billAmount = billAmount;
                    ViewBag.TaxAmount = TaxAmount;
                    ViewBag.GolferPlatformFee = GolferPlatformFee;
                    ViewBag.PromoCode = PromoCode;
                    ViewBag.Total = Total;

                    GF_WeatherDetails objWeather = new GF_WeatherDetails();
                    objWeather = Db.GF_WeatherDetails.FirstOrDefault(y => y.OrderID == obj.ID);
                    ViewBag.PopOrderWeatherDesc = objWeather.WeatherDescription;
                    ViewBag.PopOrderWeatherTemp = objWeather.Temp;
                    ViewBag.PopOrderWeatherTempMax = objWeather.TempMax;
                    ViewBag.PopOrderWeatherTempMin = objWeather.TempMin;
                    ViewBag.PopOrderWeatherPressure = objWeather.Pressure;
                    ViewBag.PopOrderWeatherWindSpeed = objWeather.WindSpeed;
                    ViewBag.PopOrderWeatherHumidity = objWeather.Humidity;
                }
                result = true;
            }
            catch (Exception ex)
            {
                ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
                result = true;
            }
            return result;

        }
        #endregion

        #region Resolution center
        
        [SessionExpireFilterAttribute]
        public ActionResult ResolutionMessages()
        {
            AccessModule(ModuleValues.AllRights);
            var UserObj = new GF_ResolutionCenter();
            ViewBag.ResolutionType = new SelectList(ResolutionType.GetResolutionType(), "Tag", "Name");
            ViewBag.CourseName = new SelectList(UserObj.GetCourseInfo(), "ID", "Course_name", "0");
            return View();
        }

        /// <summary>
        /// Created By:Arun
        /// Creation On:26 March 2015
        /// Description: Get all golfer listing
        /// </summary>
        [SessionExpireFilterAttribute]
        public ActionResult GetResolutionMessages(string Courseid, string status, string fromdate, string todate, string username, string MessageSentTo, string strResolutionType, string sidx, string sord, int? page, int? rows)
        {
            AccessModule(ModuleValues.AllRights);
            try
            {
                if (Convert.ToString(Courseid) == "undefined")
                    Courseid = "";

                if (Convert.ToString(username) == "undefined")
                    username = "";

                //  username = string.IsNullOrEmpty(username) ? "" : username;

                Courseid = string.IsNullOrEmpty(Courseid) ? "0" : Courseid;
                var UserObj = new GF_ResolutionCenter();
                var totalRecords = 0;
                DateTime dtfrom = string.IsNullOrEmpty(fromdate) ? DateTime.MinValue : Convert.ToDateTime(fromdate);
                DateTime dtto = string.IsNullOrEmpty(todate) ? DateTime.MaxValue : Convert.ToDateTime(todate);

                //var rec = UserObj.GetResolutionMessages(Convert.ToInt64(Courseid), status, from, to,username, sidx,
                //                           sord, page ?? 1, rows ?? 10,
                //                           ref totalRecords).AsEnumerable().Select((x =>
                //                                                     new
                //                                                     {
                //                                                         ID = x.ID,
                //                                                         Status = CommonFunctions.GetLatestStatus(x.ID,x.Status),
                //                                                         sentBy=(x.GF_Golfer.FirstName+" "+x.GF_Golfer.LastName),
                //                                                        // Name =x.GF_Golfer.FirstName+" "+x.GF_Golfer.LastName, //x.GF_CourseInfo.COURSE_NAME,
                //                                                         Name = CommonFunctions.GetLatestUserName(x.ID,(x.GF_Golfer.FirstName+" "+x.GF_Golfer.LastName)),
                //                                                         courseName = x.GF_CourseInfo.COURSE_NAME,
                //                                                         DoActive = (x.ID != LoginInfo.UserId),
                //                                                         EID = CommonFunctions.EncryptUrlParam(x.ID),
                //                                                         CreatedDate = CommonFunctions.GetLatestDate(x.ID, Convert.ToDateTime(x.CreatedDate)),
                //                                                     }
                //                                                    ));

                var timeZone = CommonFunctions.GetCourseTimeZone(LoginInfo.CourseId);

                if(LoginInfo.IsGolferLoginUser)
                    timeZone = CommonFunctions.GetGolferTimeZone(LoginInfo.GolferUserId);

                var rec = UserObj.GetResolutionMessagesNew(Convert.ToInt64(Courseid), status, Convert.ToString(dtfrom), Convert.ToString(dtto), username, MessageSentTo, strResolutionType, sidx,
                                        sord, page ?? 1, rows ?? 10,
                                        ref totalRecords).AsEnumerable().Select((x =>
                                                new
                                                {
                                                    ID = x.ID,
                                                    x.Status,// = CommonFunctions.GetLatestStatus(x.ID, x.Status),
                                                    x.sentBy,// = (x.GF_Golfer.FirstName + " " + x.GF_Golfer.LastName),
                                                    x.sentByEncryptedId,
                                                    x.Name,// = CommonFunctions.GetLatestUserName(x.ID, (x.GF_Golfer.FirstName + " " + x.GF_Golfer.LastName)),
                                                    x.courseName,//e = x.GF_CourseInfo.COURSE_NAME,
                                                    DoActive = (x.ID != LoginInfo.UserId),
                                                    EID = CommonFunctions.EncryptUrlParam(x.ID),
                                                    //CreatedDate = x.CreatedDate != null ? CommonFunctions.DateByTimeZone(timeZone, x.CreatedDate ?? DateTime.UtcNow) : x.CreatedDate,
                                                    //x.CreatedDate,// = CommonFunctions.GetLatestDate(x.ID, Convert.ToDateTime(x.CreatedDate)),
                                                    //LatestCreatedDate = CommonFunctions.DateByTimeZone(timeZone, x.LatestCreatedDate),
                                                    //x.LatestCreatedDate,
                                                    CreatedDate = x.CreatedDate != null ? CommonFunctions.DateByTimeZone(timeZone, x.CreatedDate ?? DateTime.UtcNow).ToString("MM/dd/yyyy hh:mm tt") : (x.CreatedDate ?? DateTime.UtcNow).ToString("MM/dd/yyyy hh:mm tt"),
                                                    LatestCreatedDate = CommonFunctions.DateByTimeZone(timeZone, x.LatestCreatedDate).ToString("MM/dd/yyyy hh:mm tt"),
                                                    LatestReplyBy = CommonFunctions.GetLatestUserName(x.ID, Convert.ToString(x.LatestReplyBy)),
                                                    x.LatestComments,
                                                    comment = x.FeedbackTest,
                                                    x.strResolutionType,
                                                    x.GolferID,
                                                    IsRead = LoginInfo.IsGolferLoginUser ? !x.IsRead : !(x.IsReadByAdmin ?? false)
                                                }
                                                ));


                var jsonData = new
                {
                    total = (totalRecords + rows - 1) / rows,
                    page,
                    records = totalRecords,
                    rows = rec.ToList(),
                    id = "ID"
                };
                return Json(jsonData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
                return null;
            }
        }

        /// <summary>
        /// Created By:Arun
        /// Created date:17 April 2015
        /// Purpose: Show Message history.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [SessionExpireFilterAttribute]
        public ActionResult ResolutionMessagereply(string eid)
        {
            AccessModule(ModuleValues.AllRights);
            long Rid = 0;
            if (!string.IsNullOrEmpty(eid))
                Rid = Convert.ToInt64(CommonFunctions.DecryptUrlParam(eid));
            else
                RedirectToAction("ResolutionMessages");

            ViewBag.MessageId = Rid;

            var DB = new GolflerEntities();
            GF_ResolutionCenter MainRes = new GF_ResolutionCenter();
            Int64 msgid = Convert.ToInt64(Rid);
            MainRes = Db.GF_ResolutionCenter.Where(x => x.ID == msgid).FirstOrDefault();

            if (MainRes != null)
            {
                @ViewBag.SentByName = (MainRes.GolferID ?? 0) != 0 ? MainRes.GF_Golfer.FirstName + " " + MainRes.GF_Golfer.LastName : MainRes.GetAdminName(MainRes.SenderID ?? 0, MainRes.SenderType, Db);
                @ViewBag.CourseName = MainRes.GF_CourseInfo.COURSE_NAME;
                @ViewBag.eidReply = eid;
            }

            MainRes.ReadStatusResolutionMessage(msgid);

            // return View();
            return View(new GF_ResolutionMessageHistory());
        }

        /// <summary>
        /// Created By:Arun
        /// Creation On:26 March 2015
        /// Description: Get all golfer listing
        /// </summary>
        [SessionExpireFilterAttribute]
        public ActionResult GetResolutionMessagesHistory(string messageID, string sidx, string sord, int? page, int? rows)
        {
            try
            {

                var UserObj = new GF_ResolutionCenter();
                var totalRecords = 0;

                var rec = UserObj.GetResolutionMessagesHistory(Convert.ToInt64(messageID), sidx,
                                           sord, page ?? 1, rows ?? 10,
                                           ref totalRecords).AsEnumerable().Select((x =>
                                                                     new
                                                                     {
                                                                         ID = x.ID,
                                                                         CreatedDate = x.MsgDate,
                                                                         name = x.ReplyBy,
                                                                         Message = x.MsgComments,
                                                                         Status = x.MsgStatus
                                                                         //ID = x.ID,
                                                                         //x.Status,
                                                                         //x.Message,
                                                                         //x.CreatedDate,
                                                                         ////courseName=Db.GF_CourseInfo.FirstOrDefault(y => y.ID = x.GF_ResolutionCenter.CourseID).COURSE_NAME,
                                                                         //name = x.UserType != "G" ? (Db.GF_AdminUsers.FirstOrDefault(y => y.ID == x.LogUserID).Name) : (Db.GF_Golfer.FirstOrDefault(y => y.GF_ID == x.LogUserID).FirstName)
                                                                     }
                                                                    ));

                var jsonData = new
                {
                    total = (totalRecords + rows - 1) / rows,
                    page,
                    records = totalRecords,
                    rows = rec.ToList(),
                    id = "ID"
                };
                return Json(jsonData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
                return null;
            }
        }

        [HttpPost]
        [SessionExpireFilterAttribute]
        public ActionResult ResolutionMessagereply(GF_ResolutionMessageHistory obj)
        {
            AccessModule(ModuleValues.AllRights);
            if (ModelState.IsValid)
            {
                var objRes = new GF_ResolutionCenter();
                obj.Status = Request.Form["ddlStatus"] != null ? Convert.ToString(Request.Form["ddlStatus"]) : MessageStatusType.Replyed;
                obj.MessageID = Request.Form["hdnMessageID"] != null ? Convert.ToInt64(Request.Form["hdnMessageID"]) : 0;
                obj.Message = Request.Form["txtComment"] != null ? Convert.ToString(Request.Form["txtComment"]) : "";

                string eidSTR = Request.Form["hdnEID"] != null ? Convert.ToString(Request.Form["hdnEID"]) : "";

                objRes.AddResolutionMessage(obj);

                string cntrollername = "";

                if (LoginInfo.LoginUserType == UserType.Kitchen || LoginInfo.LoginUserType == UserType.Cartie || LoginInfo.LoginUserType == UserType.Ranger || LoginInfo.LoginUserType == UserType.CourseAdmin || LoginInfo.LoginUserType == UserType.Proshop)
                {//course....................Golfler/CourseAdmin/
                    cntrollername = "CourseAdmin";
                }
                else if (LoginInfo.GolferType == UserType.Golfer)
                {//Golfler/golfer/
                    cntrollername = "golfer";
                }

                CommonFunctions.ResolutionReplyByAnyType(obj.Message, statusReply(obj.Status), Convert.ToInt64(obj.MessageID));
                //ResolutionReply(obj.Message, statusReply(obj.Status), Convert.ToInt64(obj.MessageID));
                
                ViewBag.Message = "Reply has been sent successfully.";
                TempData["Message"] = "Reply has been sent successfully.";
                //return RedirectToAction("ResolutionMessagereply?eid=" + eid, cntrollername);
                return RedirectToAction("ResolutionMessagereply", cntrollername, new { eid = eidSTR });
            }
            return View(obj);

        }
      
        public string statusReply(string statusCode)
        {
            AccessModule(ModuleValues.AllRights);
            string status = "";
            if (statusCode.Trim() == MessageStatusType.Open)
                status = "Open";
            else if (statusCode.Trim() == MessageStatusType.Replyed)
                status = "Replyed";
            else
                status = "Closed";

            return status;
        }

        public void ResolutionReply(string comment, string status, Int64 msgID)
        {
            AccessModule(ModuleValues.AllRights);
            try
            {
                #region send email
                IQueryable<GF_EmailTemplatesFields> templateFields = null;
                var param = EmailParams.GetEmailParams(ref Db, "Resolution Reply", ref templateFields);

                string email = "";
                string name = "";

                if (param != null)
                {
                    Int64 id = 0;
                    if (LoginInfo.LoginUserType == UserType.Kitchen || LoginInfo.LoginUserType == UserType.Cartie || LoginInfo.LoginUserType == UserType.Ranger || LoginInfo.LoginUserType == UserType.CourseAdmin || LoginInfo.LoginUserType == UserType.Proshop)
                    {//course....................Golfler/CourseAdmin/

                        #region CourseAdmin Login...send mail to golfer

                        id = LoginInfo.UserId;

                        var mainRes = Db.GF_ResolutionCenter.Where(x => x.ID == msgID).FirstOrDefault();
                        if (mainRes != null)
                        {
                            email = mainRes.GF_Golfer.Email.ToString();
                            name = mainRes.GF_Golfer.FirstName.ToString() + " " + mainRes.GF_Golfer.LastName.ToString();
                            if (!ApplicationEmails.GolferResolutionReply(id, comment, status, email, name, param, ref templateFields))
                            {
                                // Message = String.Format(Resources.Resources.mailerror);
                                // return new Result { Status = 0, Error = "An error occured while sending mail.", record = null };
                            }
                            else
                            {
                                //   return new Result { Status = 1, Error = "Your new password is sent on your email address. Please login with new password.", record = null };
                            }
                        }

                        #endregion
                    }
                    else if (LoginInfo.GolferType == UserType.Golfer)
                    {//Golfler/golfer/

                        #region Golfer Login

                        id = LoginInfo.GolferUserId;

                        var mainRes = Db.GF_ResolutionCenter.Where(x => x.ID == msgID).FirstOrDefault();

                        if (mainRes != null)
                        {
                            if (mainRes.SendTo.ToUpper() == "SA" || mainRes.SendTo.ToUpper() == "A")
                            {//sent to super admin/ Admin
                                #region If send to Superadmin

                                var admin = Db.GF_AdminUsers.Where(x => x.Type == "SA" || x.Type == "A").ToList();
                                foreach (var usr in admin)
                                {
                                    email = usr.Email;
                                    name = usr.FirstName + " " + usr.LastName;

                                    // if (!ApplicationEmails.GolferResolutionReply(id, comment, status, LoginInfo.GolferEmail, LoginInfo.UserName, param, ref templateFields))
                                    if (!ApplicationEmails.GolferResolutionReply(id, comment, status, email, name, param, ref templateFields))
                                    {
                                        // Message = String.Format(Resources.Resources.mailerror);
                                        // return new Result { Status = 0, Error = "An error occured while sending mail.", record = null };
                                    }
                                    else
                                    {
                                        //   return new Result { Status = 1, Error = "Your new password is sent on your email address. Please login with new password.", record = null };
                                    }

                                }

                                #endregion
                            }
                            else if (mainRes.SendTo.ToUpper() == "CP")
                            {
                                #region if sent to courseadmin
                                Int64 cId = Convert.ToInt64(mainRes.CourseID);

                                var admin = Db.GF_AdminUsers.Where(x => x.CourseId == cId).ToList();
                                foreach (var usr in admin)
                                {
                                    email = usr.Email;
                                    name = usr.FirstName + " " + usr.LastName;

                                    if (!ApplicationEmails.GolferResolutionReply(id, comment, status, email, name, param, ref templateFields))
                                    {
                                        // Message = String.Format(Resources.Resources.mailerror);
                                        // return new Result { Status = 0, Error = "An error occured while sending mail.", record = null };
                                    }
                                    else
                                    {
                                        //   return new Result { Status = 1, Error = "Your new password is sent on your email address. Please login with new password.", record = null };
                                    }

                                }

                                #endregion
                            }
                        }

                        #endregion
                    }
                    else if (LoginInfo.Type == UserType.Admin || LoginInfo.Type == UserType.SuperAdmin)
                    {//golfer/admin.......

                        #region if golfer admin login

                        id = LoginInfo.UserId;

                        var mainRes = Db.GF_ResolutionCenter.Where(x => x.ID == msgID).FirstOrDefault();
                        if (mainRes != null)
                        {
                            email = mainRes.GF_Golfer.Email.ToString();
                            name = mainRes.GF_Golfer.FirstName.ToString() + " " + mainRes.GF_Golfer.LastName.ToString();
                            if (!ApplicationEmails.GolferResolutionReply(id, comment, status, email, name, param, ref templateFields))
                            {
                                // Message = String.Format(Resources.Resources.mailerror);
                                // return new Result { Status = 0, Error = "An error occured while sending mail.", record = null };
                            }
                            else
                            {
                                //   return new Result { Status = 1, Error = "Your new password is sent on your email address. Please login with new password.", record = null };
                            }
                        }

                        #endregion
                    }


                }
                else
                {
                    //  return new Result { Status = 0, Error = "An error occured while sending mail.", record = null };
                }
                #endregion
            }
            catch (Exception ex)
            {
                //   return new Result { Id = 0, Status = 0, Error = ex.Message, record = null };
            }
        }

        [HttpGet]
        [SessionExpireFilterAttribute]
        public ActionResult AddResolutionMessages(string eid)
        {
            ViewBag.ResolutionCenterType = new SelectList(ResolutionCenterType.GetResolutionCenterType(), "Tag", "Name");
            ViewBag.ResolutionSendToType = new SelectList(UserType.GetResolutionSendToType(), "Tag", "Name");

            if (!string.IsNullOrEmpty(eid))
            {
                long id = CommonFunctions.DecryptUrlParam(eid);
                GF_Golfer obj = new GF_Golfer();
                var order = obj.GetOrderByID(id);
                if (order != null)
                {
                    ViewBag.CourseID = order.CourseID;
                    ViewBag.CourseName = order.GF_CourseInfo.COURSE_NAME;
                    ViewBag.Complaint = "Order No.: " + order.ID.ToString() + ", Amount: $" + (order.GrandTotal ?? 0).ToString() + ", Payment Type: " + (order.PaymentType == "0" ? "Membership" : "Card");
                }
            }

            return View();
        }

        [HttpPost]
        [SessionExpireFilterAttribute]
        public ActionResult AddResolutionMessages(ResolutionMessages resolutionCenter)
        {
            ViewBag.ResolutionCenterType = new SelectList(ResolutionCenterType.GetResolutionCenterType(), "Tag", "Name");
            ViewBag.ResolutionSendToType = UserType.GetResolutionSendToType();

            GF_ResolutionCenter obj = new GF_ResolutionCenter();

            if (obj.AddGolferResolutionCenter(resolutionCenter))
            {
                return RedirectToAction("ResolutionMessages");
            }

            return View(obj);
        }

        #endregion

        #region Messaging Center


        /// <summary>
        /// Created By:veera
        /// Created on: 01 April 2015
        /// Description: method for list
        /// </summary>
        [SessionExpireFilterAttribute]
        public ActionResult MessagingCenterList()
        {
            //if (LoginInfo.IsSuper)

            AccessModule(ModuleValues.AllRights);
            //else
            //    AccessModule(ModuleValues.MessagingCenter);

            ViewBag.Message = TempData["Message"];
            #region IsBlockedByCourse
            GF_Golfer objGolfer = new GF_Golfer();
            // string result = objGolfer.BlockNUnblockUser(_userId, _blockedUserId, IsGolferUser, IsBlockedGolferUser, Block);
            #endregion

            ViewBag.CourseIds = new SelectList(objGolfer.GetAllVisitedCourses(), "CourseID", "CourseName");

            return View();
        }
        /// <summary>
        /// Created By:
        /// Creation On:
        /// Description: method to bind grid
        /// </summary>
        [SessionExpireFilterAttribute]
        public ActionResult GetOnlineGolferList(string CourseId, string searchText, string type, string status, string sidx, string sord, int? page, int? rows)
        {
            //  AccessModule(ModuleValues.AllRights);
            try
            {
                GolflerEntities Db = new GolflerEntities();
                GF_Golfer obj = new GF_Golfer();

                var totalRecords = 0;
                long userId = LoginInfo.UserId;
                if (userId == 0 || LoginInfo.IsSuper)
                {
                    userId = LoginInfo.GolferUserId;
                }
                var list = obj.GetOnlineGolferListByCourseIdList(CourseId, userId, "1", type, status, searchText, sidx, sord, page ?? 1, rows ?? 10, ref totalRecords);


                var jsonData = new
                {
                    total = (totalRecords + rows - 1) / rows,
                    page,
                    records = totalRecords,
                    rows = list.ToList(),
                    ID = "Id"

                };
                return Json(jsonData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
                return null;
            }
            finally
            {
                Db = null;
            }
        }
        [HttpPost]
        public ActionResult GetGolferData(string msgfrom, string msgto, string msg, string IsMessagesFromGolfer, string IsMessagesToGolfer)
        {
            AccessModule(ModuleValues.AllRights);
            string url = ConfigClass.GolferApiService + GolferApiName.MsgGolfers + "SendMessage";
            string data = "MsgFrom=" + msgfrom + "&MsgToList=" + msgto + "&Message=" + msg + "&IsMessagesFromGolfer=" + IsMessagesFromGolfer + "&IsMessagesToGolfer=" + IsMessagesToGolfer;
            MyWebRequest myRequest = new MyWebRequest(url, "POST", data);
            var jsonString = myRequest.GetResponse();

            #region AddToFriendList

            string urlFriends = ConfigClass.GolferApiService + GolferApiName.Friends + "AddFriendsService";
            string dataFriends = "UserId=" + msgfrom + "&FriendId=" + msgto + "&UserType=" + "NU" + "&FriendType=" + "NU";
            MyWebRequest myRequestFriends = new MyWebRequest(urlFriends, "POST", dataFriends);
            var jsonStringFriends = myRequestFriends.GetResponse();

            #endregion

            return Json(new { result = true });
        }
        [HttpPost]
        public ActionResult GetGolferMessages(string id, string PgNo, string MsgTo, string type, bool? isSupport)
        {
            AccessModule(ModuleValues.AllRights);
            Course objCourse = new Course();
            int totalrecords = 0;
            bool OnlyGolfer = false;
            long userid;
            int PgSize = ConfigClass.MessageListingPageSize;
            if (id.Contains("chat_"))
            {
                id = id.Replace("chat_", "");
            }
            if (MsgTo == "1")
            {

                userid = LoginInfo.GolferUserId;
                if (userid == 0)
                {
                    OnlyGolfer = false;
                    userid = LoginInfo.UserId;
                }
                else
                { OnlyGolfer = true; }
            }
            else
            {
                userid = LoginInfo.UserId;
                //OnlyGolfer = true;
            }
            var list = objCourse.GetMsgsfromGolfer(Convert.ToInt64(id), userid, Convert.ToInt32(PgNo), PgSize, ref totalrecords,
                MsgTo, OnlyGolfer, type, isSupport);

            var jsonData = new
            {
                pages = Math.Ceiling(Convert.ToDecimal(totalrecords / PgSize)),
                rows = list
            };

            return Json(jsonData, JsonRequestBehavior.AllowGet);

        }
        #endregion

        #region Manage Course Builder Suggestion

        [HttpGet]
        [SessionExpireFilterAttribute]
        public ActionResult CourseCoordinateSuggestionList()
        {
            AccessModule(ModuleValues.AllRights);
            ViewBag.Partner = new SelectList(PartershipStatus.GetPartnerShipStatus(), "Tag", "Name");
            ViewBag.SuggestionType = new SelectList(SuggestionType.GetSuggestionType(), "Tag", "Name");
            return View();
        }

        [SessionExpireFilterAttribute]
        public ActionResult GetCourseInfo(string searchText, string CourseType, string partnerType, string sidx, string sord, int? page, int? rows)
        {
            try
            {
                GF_CourseBuilder objCourse = new GF_CourseBuilder();
                var totalRecords = 0;
                var rec = objCourse.GetCoursesInfo(searchText, CourseType, partnerType, sidx,
                                           sord, page ?? 1, rows ?? 10,
                                           ref totalRecords).AsEnumerable().Select((x =>
                                                                     new
                                                                     {
                                                                         ID = x.ID,
                                                                         x.COURSE_NAME,
                                                                         PartnershipStatus = x.PartnershipStatus == PartershipStatus.Partner ? "Partner" : "Non-Partner",
                                                                         x.ZIPCODE,
                                                                         x.COUNTY,
                                                                         x.STATE,
                                                                         x.CITY,
                                                                         x.ADDRESS,
                                                                         CoordStatus = CommonFunctions.GetCourseSuggestStatus(x.ID),
                                                                         Status = x.Status ?? StatusType.InActive,
                                                                         EID = CommonFunctions.EncryptUrlParam(x.ID),
                                                                         x.Country
                                                                     }
                                                                    ));

                var jsonData = new
                {
                    total = (totalRecords + rows - 1) / rows,
                    page,
                    records = totalRecords,
                    rows = rec.ToList(),
                    id = "ID"
                };
                return Json(jsonData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
                return null;
            }
        }


        [SessionExpireFilterAttribute]
        public ActionResult CourseCoordinateSuggestion(string Eid)
        {
            AccessModule(ModuleValues.AllRights);

            Int64 intCourseId = CommonFunctions.DecryptUrlParam(Eid);

            var courseLocation = Db.GF_CourseInfo.FirstOrDefault(x => x.ID == intCourseId);
            ViewBag.Lattitude = courseLocation.LATITUDE;
            ViewBag.Longitude = courseLocation.LONGITUDE;
            ViewBag.CourseId = intCourseId;
            string address = courseLocation.ADDRESS;

            if (!string.IsNullOrEmpty(Convert.ToString(courseLocation.CITY)))
            {
                address = address + ", " + courseLocation.CITY;
            }
            if (!string.IsNullOrEmpty((Convert.ToString(courseLocation.STATE))))
            {
                address = address + ", " + courseLocation.STATE;
            }
            if (!string.IsNullOrEmpty(Convert.ToString(courseLocation.COUNTY)))
            {
                address = address + ", " + courseLocation.COUNTY;
            }
            if (!string.IsNullOrEmpty(Convert.ToString(courseLocation.ZIPCODE)))
            {
                address = address + ", " + courseLocation.ZIPCODE;
            }

            ViewBag.address = address;

            return View();
        }

        [HttpPost]
        [SessionExpireFilterAttribute]
        public ActionResult CourseCoordinateSuggestion(string courseID, string holesDetail)
        {
            AccessModule(ModuleValues.AllRights);
            bool boolResult = false;
            string Message = "";

            try
            {
                GF_SuggestCourseCoordinate obj = new GF_SuggestCourseCoordinate();
                List<suggestCoordinateDetail> objHoleDetail = (List<suggestCoordinateDetail>)JsonConvert.DeserializeObject(holesDetail, typeof(List<suggestCoordinateDetail>));

                string msg = "";

                var isSave = obj.SaveSuggestCoordinate(courseID, objHoleDetail, ref msg);
                boolResult = true;
                Message = msg;
            }
            catch (Exception ex)
            {
                boolResult = false;
                ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
                Message = ex.InnerException.ToString();
            }

            return Json(new { result = boolResult, Message = Message });
        }

        [HttpGet]
        [SessionExpireFilterAttribute]
        public ActionResult GetCourseCoordinateSuggestion(string courseID)
        {
            AccessModule(ModuleValues.AllRights);
            try
            {
                long cid = Convert.ToInt32(courseID);

                if (cid == 0)
                {
                    return RedirectToAction("CourseCoordinateSuggestionList");
                }

                IEnumerable<suggestCoordinate> lstCourse;
                GF_SuggestCourseCoordinate obj = new GF_SuggestCourseCoordinate();
                lstCourse = obj.getSuggestCoordinate(cid);

                return Json(new { data = lstCourse, result = true, Message = "Success" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
                return Json(new { data = "", result = false, Message = ex.InnerException.ToString() }, JsonRequestBehavior.AllowGet);
            }

        }


        [SessionExpireFilterAttribute]
        public ActionResult GetCourseCoordinateSuggestionList(string searchText, string sidx, string sord, int? page, int? rows)
        {
            try
            {
                var CourseObj = new GF_SuggestCourseCoordinate();
                var totalRecords = 0;
                var rec = CourseObj.GetSuggestCoordinateList(searchText, sidx,
                                           sord, page ?? 1, rows ?? 10,
                                           ref totalRecords).AsEnumerable().Select((x =>
                                                                     new
                                                                     {
                                                                         x.ID,
                                                                         x.CreatedDate
                                                                     }
                                                                    ));

                var jsonData = new
                {
                    total = (totalRecords + rows - 1) / rows,
                    page,
                    records = totalRecords,
                    rows = rec.ToList(),
                    id = "ID"
                };
                return Json(jsonData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
                return null;
            }
        }

        #endregion

        #region Forgot Password
        /// <summary>
        /// Created By: Veera
        /// Creation On: 27 April, 2015
        /// Description: method for forgot password
        /// </summary>
        public ActionResult Forgot()
        {
            AccessModule(ModuleValues.AllRights);
            return View();
        }

        public ActionResult ForgotPasswordMail(string email, string type)
        {
            AccessModule(ModuleValues.AllRights);
            try
            {
                if (type == "0")//CourseUsers
                {
                    var obj = Db.GF_AdminUsers.FirstOrDefault(x => x.UserName.ToLower() == email.ToLower().Trim());
                    if (obj != null)
                    {
                        string url = ConfigClass.CourseApiService + CourseApiName.ForgotPassword;
                        string data = "UserName=" + email;
                        MyWebRequest myRequest = new MyWebRequest(url, "POST", data);
                        var jsonString = myRequest.GetResponse();
                        resultSet result = JsonHelper.JsonDeserialize<resultSet>(jsonString);
                        return Json(new { result = "1", Email = obj.Email });
                    }
                    else
                    {
                        return Json(new { result = "-1", Email = obj.Email });
                    }
                }
                else if (type == "1")//golferUsers
                {
                    var obj = Db.GF_Golfer.FirstOrDefault(x => x.Email.ToLower() == email.ToLower().Trim());
                    if (obj != null)
                    {
                        string url = ConfigClass.GolferApiService + GolferApiName.ForgotPasswordService;
                        string data = "Email=" + email;
                        MyWebRequest myRequest = new MyWebRequest(url, "POST", data);
                        var jsonString = myRequest.GetResponse();
                        resultSet result = JsonHelper.JsonDeserialize<resultSet>(jsonString);
                        return Json(new { result = "1", Email = obj.Email });
                    }
                    else
                    {
                        return Json(new { result = "-1", Email = obj.Email });
                    }
                }
                else
                {
                    return Json(new { result = "-2", Email = "" });
                }
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                return Json(new { result = "0", Email = "" });
            }
        }
        #endregion

        #region OrderAnalytics

        /// <summary>
        /// Created By: Veera
        /// Creation On: 28 April, 2015
        /// Description: method for ComparePrice
        /// </summary>
        [SessionExpireFilter]
        public ActionResult OrderAnalytics()
        {
            AccessModule(ModuleValues.AllRights);
            Course objCourse = new Course();
            //  ViewBag.CourseIds = new SelectList(objCourse.GetAllActiveOrderCourses(Convert.ToString(LoginInfo.GolferUserId)), "CourseID", "Name");
            ViewBag.CourseIds = new SelectList(objCourse.GetAllActiveCourses(), "ID", "COURSE_NAME");
            //   ViewBag.Country = new SelectList(objCourse.GetAllActiveCountries(), "ID", "Name");
            // ViewBag.FoodItems = new SelectList(objCourse.GetAllActiveFoodItems(), "ID", "NAME");

            return View();
        }

        public ActionResult StateList(string country)
        {
            AccessModule(ModuleValues.AllRights);
            try
            {
                var lstCourse = (from x in Db.GF_CourseInfo
                                 where x.Status == StatusType.Active && x.COUNTY == country
                                 select new OrderAnalyticsListResult
                                 {
                                     ID = x.STATE,
                                     Name = x.STATE
                                 }).Distinct().ToList();



                return Json(new { result = lstCourse });


            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                return Json(new { result = "0" });
            }
        }

        public ActionResult CityList(string country, string state)
        {
            AccessModule(ModuleValues.AllRights);
            try
            {
                var lstCourse = (from x in Db.GF_CourseInfo
                                 where x.Status == StatusType.Active && x.COUNTY == country && x.STATE == state && (x.CITY != null || x.CITY != "")
                                 select new OrderAnalyticsListResult
                                 {
                                     ID = x.CITY,
                                     Name = x.CITY
                                 }).Distinct().ToList();



                return Json(new { result = lstCourse });


            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                return Json(new { result = "0" });
            }
        }

        public ActionResult FoodItemList(string course)
        {
            AccessModule(ModuleValues.AllRights);
            long _courseId = Convert.ToInt64(course);
            try
            {
                var lstFoodItem = (from x in Db.GF_CourseMenu
                                   where x.Status == StatusType.Active && x.CourseID == _courseId
                                   select new OrderAnalyticsListResult
                                   {
                                       CatID = x.CategoryID,
                                       Name = x.GF_Category.Name
                                   }).Distinct().ToList();



                return Json(new { result = lstFoodItem });


            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                return Json(new { result = "0" });
            }
        }

        public ActionResult SubFoodItemList(string FoodItem)
        {
            AccessModule(ModuleValues.AllRights);
            long _Id = Convert.ToInt64(FoodItem);
            try
            {
                var lstSubFoodItem = (from x in Db.GF_SubCategory
                                      where x.Status == StatusType.Active && x.CategoryID == _Id
                                      select new OrderAnalyticsListResult
                                      {
                                          CatID = x.ID,
                                          Name = x.Name
                                      }).Distinct().ToList();



                return Json(new { result = lstSubFoodItem });


            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                return Json(new { result = "0" });
            }
        }

        public ActionResult OrderAnalyticsSearch(string type, string course, string foodItem, string subfoodCategory, string miles)
        {
            AccessModule(ModuleValues.AllRights);
            try
            {

                Course obj = new Course();
                OrderAnalyticsSearchResult objResult = obj.OrderAnalyticsSearch(LoginInfo.GolferUserId, type, course, foodItem, subfoodCategory, miles);
                var lstMenus = objResult.MenuSearch.ToList();
                ViewBag.MenuSearch = lstMenus;

                //
                /* string itemName = "";
                 foreach (var item in lstMenus)
                 {
                         
                     if(itemName)
                         if (itemName == "")
                     {
                         itemName = item.CategoryName;

                     }
                     else
                     {
                         itemName = itemName+" , "+ item.CategoryName;
                     }
                 }*/
                //
                return Json(new { result = objResult });


            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                return Json(new { result = "0" });
            }
        }

        #endregion

        #region Game Analytics

        [SessionExpireFilterAttribute]
        public ActionResult GameAnalytics()
        {
            AccessModule(ModuleValues.AllRights);

            return View();
        }

        [SessionExpireFilterAttribute]
        public ActionResult GetWeatherReportInfo(long courseID, DateTime DateFrom, DateTime DateTo,
            string campareParameter, double range, string reportType)
        {
            AccessModule(ModuleValues.AllRights);

            GameAnalysis ob = new GameAnalysis();
            var lstWeatherReport = ob.GetWeatherReport(courseID, LoginInfo.GolferUserId, DateFrom, DateTo, campareParameter, range, reportType);

            return Json(lstWeatherReport, JsonRequestBehavior.AllowGet);
        }

        [SessionExpireFilterAttribute]
        public ActionResult GetScoreAverageInfo(long courseID, DateTime DateFrom, DateTime DateTo,
            string campareParameter, double range, string reportType)
        {
            AccessModule(ModuleValues.AllRights);

            GameAnalysis ob = new GameAnalysis();
            var lstScoreAverageInfo = ob.GetScoreAverageInfo(courseID, LoginInfo.GolferUserId, DateFrom, DateTo, campareParameter, range, reportType);

            return Json(new { data = lstScoreAverageInfo }, JsonRequestBehavior.AllowGet);
        }

        [SessionExpireFilterAttribute]
        public ActionResult GetScoreCardInfo(long courseID, DateTime DateFrom, DateTime DateTo, long pageNo,
            string campareParameter, double range, string reportType)
        {
            AccessModule(ModuleValues.AllRights);

            GameAnalysis ob = new GameAnalysis();
            List<GamePlayPlayer> gamePlayPlayer = new List<GamePlayPlayer>();
            var lstScoreAverageInfo = ob.GetScoreCardInfo(courseID, LoginInfo.GolferUserId, DateFrom, DateTo, pageNo, campareParameter, range, reportType, ref gamePlayPlayer);

            return Json(new { data = lstScoreAverageInfo, PlayerInfo = gamePlayPlayer }, JsonRequestBehavior.AllowGet);
        }

        [SessionExpireFilterAttribute]
        public ActionResult GetRoundComparison(long courseID, DateTime DateFrom, DateTime DateTo,
            string campareParameter, double range, string reportType)
        {
            AccessModule(ModuleValues.AllRights);

            GameAnalysis ob = new GameAnalysis();
            var column = new string[100];
            JavaScriptSerializer obj = new JavaScriptSerializer();
            var lstRoundComparison = ob.GetRoundComparison(courseID, LoginInfo.GolferUserId, DateFrom, DateTo, campareParameter, range, reportType, ref column);

            var result = "";
            List<object[]> data = new List<object[]>();
            data.Add(column);
            foreach (object[] arr in lstRoundComparison)
            {
                data.Add(arr);
            }

            if (lstRoundComparison.Count == 0)
                result = "failed";
            else
                result = obj.Serialize(data);

            return Json(new { result }, JsonRequestBehavior.AllowGet);
        }

        [SessionExpireFilterAttribute]
        public ActionResult GetCourseName(string query)
        {
            AccessModule(ModuleValues.AllRights);
            try
            {
                query = query.Trim();//.Replace(" ", "");
                if (query.Length > 1)
                {
                    int op = query.LastIndexOf(",");
                    query = query.Substring(op + 1);
                }

                var lstCourse = Db.GF_CourseInfo.Where(x => x.Status == StatusType.Active && x.COURSE_NAME.StartsWith(query))
                    .Select(x => new
                    {
                        ID = x.ID,
                        Name = x.COURSE_NAME
                    }).ToArray();

                if (lstCourse.Count() <= 0)
                {
                    List<object> uList = new List<object>();
                    uList.Add(new { ID = -1, Name = "No record found" });

                    var noRecord = uList.ToArray();
                    return Json(noRecord, JsonRequestBehavior.AllowGet);
                }

                return Json(lstCourse, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
                return null;
            }

        }

        /// <summary>
        /// Created By: Amit Kumar
        /// Created Date: 15 March 2016
        /// Purpose: Get club house names
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [SessionExpireFilterAttribute]
        public ActionResult GetClubHouse(string query)
        {
            AccessModule(ModuleValues.AllRights);
            try
            {
                query = query.Trim();//.Replace(" ", "");
                if (query.Length > 1)
                {
                    int op = query.LastIndexOf(",");
                    query = query.Substring(op + 1);
                }

                var lstCourse = Db.GF_CourseInfo.Where(x => x.Status == StatusType.Active &&
                    x.COURSE_NAME.StartsWith(query) &&
                    (x.IsClubHouse ?? true))
                    .Select(x => new
                    {
                        ID = x.ID,
                        Name = x.COURSE_NAME
                    }).ToArray();

                if (lstCourse.Count() <= 0)
                {
                    List<object> uList = new List<object>();
                    uList.Add(new { ID = -1, Name = "No record found" });

                    var noRecord = uList.ToArray();
                    return Json(noRecord, JsonRequestBehavior.AllowGet);
                }

                return Json(lstCourse, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
                return null;
            }

        }

        //public ActionResult UpdateCourseCityStateCounty()
        //{
        //    AccessModule(ModuleValues.AllRights);

        //    GameAnalysis ob = new GameAnalysis();
        //    ob.updateCourseCityStateCounty();

        //    return View();
        //}

        #endregion

        #region Time Span

        public ActionResult GetStartEndDate_New(string DateSpan, string report)
        {
            AccessModule(ModuleValues.AllRights);
            var dateFrom = DateTime.UtcNow.AddDays(-7);
            var dateTo = DateTime.UtcNow;
            string startWeekDay = "1";
            GetDates(DateSpan, startWeekDay, out dateFrom, out dateTo);

            var displayText = "";

            switch (DateSpan)
            {
                case "all":
                    displayText = TimeSpanResource.AllTime;
                    break;
                case "today":
                    displayText = TimeSpanResource.Today;
                    break;
                case "yesterday":
                    displayText = TimeSpanResource.Yesterday;
                    break;
                case "thisweek":
                    displayText = TimeSpanResource.ThisWeek;
                    break;
                case "lastweek":
                    displayText = TimeSpanResource.LastWeek;
                    break;
                case "thismonth":
                    displayText = TimeSpanResource.ThisMonth;
                    break;
                case "lastmonth":
                    displayText = TimeSpanResource.LastMonth;
                    break;
            }

            if (report == "WEEKLY" || report == "7WEEKS" || report == "7MONTHS" || report == "3MONTHS" || report == "4WEEKLY")
            {
                displayText = dateFrom.ToString("MM/dd/yyyy") + " - " + dateTo.ToString("MM/dd/yyyy");
            }
            return Json(new
            {
                dateStart = dateFrom.ToString("MM/dd/yyyy"),
                dateEnd = dateTo.ToString("MM/dd/yyyy"),
                displayText = displayText
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetNextPreviousValues_New(string DateSpan, string report, string FromDate, string ToDate, string DateReq)
        {
            AccessModule(ModuleValues.AllRights);
            var displayText = "";
            var getStartDate = "";
            var getEndDate = "";

            var Sdate = DateTime.ParseExact(FromDate, "MM/dd/yyyy",
                                            System.Globalization.CultureInfo.InvariantCulture,
                                            System.Globalization.DateTimeStyles.None);
            var Edate = DateTime.UtcNow;
            if (!string.IsNullOrEmpty(ToDate))
                Edate = DateTime.ParseExact(ToDate, "MM/dd/yyyy",
                                            System.Globalization.CultureInfo.InvariantCulture,
                                            System.Globalization.DateTimeStyles.None);


            switch (DateSpan)
            {
                case "week":
                    switch (DateReq)
                    {
                        case "previous":
                            Sdate = Sdate.AddDays(-7);
                            getStartDate = Sdate.ToString("MM/dd/yyyy");
                            getEndDate = Sdate.AddDays(7).AddSeconds(-1).ToString("MM/dd/yyyy");
                            break;
                        default:
                            Sdate = Sdate.AddDays(7);
                            getStartDate = Sdate.ToString("MM/dd/yyyy");
                            getEndDate = Sdate.AddDays(7).AddSeconds(-1).ToString("MM/dd/yyyy");
                            break;
                    }
                    displayText = getStartDate + " - " + getEndDate;
                    break;

                case "month":
                    switch (DateReq)
                    {
                        case "previous":
                            Sdate = Sdate.AddDays(-(Sdate.Date.Day - 1)).AddMonths(-1);
                            getStartDate = Sdate.ToString("MM/dd/yyyy");
                            getEndDate = Sdate.AddMonths(1).AddSeconds(-1).ToString("MM/dd/yyyy");
                            break;
                        default:
                            Sdate = Sdate.AddDays(-(Sdate.Date.Day - 1)).AddMonths(1);
                            getStartDate = Sdate.ToString("MM/dd/yyyy");
                            getEndDate = Sdate.AddMonths(1).AddSeconds(-1).ToString("MM/dd/yyyy");
                            break;
                    }
                    displayText = getStartDate + " - " + getEndDate;
                    break;
                case "day":
                    switch (DateReq)
                    {
                        case "previous":
                            Sdate = Sdate.AddDays(-1);
                            getStartDate = Sdate.ToString("MM/dd/yyyy");
                            getEndDate = Sdate.AddDays(1).AddSeconds(-1).ToString("MM/dd/yyyy");
                            break;
                        default:
                            Sdate = Sdate.AddDays(1);
                            getStartDate = Sdate.ToString("MM/dd/yyyy");
                            getEndDate = Sdate.AddDays(1).AddSeconds(-1).ToString("MM/dd/yyyy");
                            break;
                    }
                    displayText = getStartDate;
                    break;
            }

            return Json(new { displayText = displayText, dateStart = getStartDate, dateEnd = getEndDate });
        }

        public void GetDates(string type, string startWeekDay, out DateTime startDate, out DateTime endDate)
        {

            if (string.IsNullOrEmpty(type))
                type = "today";

            var dateFrom = DateTime.UtcNow.Date;
            var dateTo = DateTime.UtcNow;
            switch (type.ToLower())
            {
                case "all":
                    dateFrom = Convert.ToDateTime("01/01/1950");
                    dateTo = DateTime.UtcNow.AddYears(1);
                    break;
                case "today":
                    dateFrom = DateTime.UtcNow.Date;
                    dateTo = DateTime.UtcNow;
                    break;
                case "yesterday":
                    dateFrom = DateTime.UtcNow.AddDays(-1).Date;
                    dateTo = dateFrom;
                    break;
                case "thisweek":
                    dateFrom = DateTime.UtcNow.StartOfWeek((Golfler.Models.DateTimeExtensions.Days)Enum.Parse(typeof(Golfler.Models.DateTimeExtensions.Days), startWeekDay)).Date;
                    dateTo = dateFrom.AddDays(7).AddSeconds(-1);
                    break;
                case "twodates":
                    dateFrom = dateFrom.AddDays(0 - (int)dateFrom.DayOfWeek).Date;
                    dateTo = dateFrom.AddDays(7).AddSeconds(-1);
                    break;
                case "lastweek":
                    dateFrom = DateTime.UtcNow.StartOfWeek((Golfler.Models.DateTimeExtensions.Days)Enum.Parse(typeof(Golfler.Models.DateTimeExtensions.Days), startWeekDay)).AddDays(-7).Date;
                    dateTo = dateFrom.AddDays(7).AddSeconds(-1);
                    break;
                case "thismonth":
                    dateFrom = DateTime.Today.AddDays(-(DateTime.Today.Day - 1)).Date;
                    dateTo = dateFrom.AddMonths(1).AddSeconds(-1);
                    break;
                case "lastmonth":
                    dateFrom = DateTime.Today.AddDays(-(DateTime.Today.Day - 1)).Date.AddMonths(-1);
                    dateTo = dateFrom.AddMonths(1).AddSeconds(-1);
                    break;
                case "last30days":
                    dateTo = DateTime.UtcNow;
                    dateFrom = DateTime.UtcNow.AddDays(-30);
                    break;
                case "last7days":
                    dateTo = DateTime.UtcNow;
                    dateFrom = DateTime.UtcNow.AddDays(-7);
                    break;
                case "next6month":
                    dateFrom = DateTime.UtcNow.Date;
                    dateTo = dateFrom.AddMonths(6).AddSeconds(-1);
                    break;
            }
            startDate = dateFrom;
            endDate = dateTo;
        }

        #endregion

        public ActionResult GetGolferChartResult()
        {
            AccessModule(ModuleValues.AllRights);
            try
            {

                #region Golfer Chart

                GF_Golfer objGolfer = new GF_Golfer();
                GolferSpendNPlayResult objResult = new GolferSpendNPlayResult();
                objResult = objGolfer.GetGolferSpendNplayById(LoginInfo.GolferUserId);
                #endregion

                return Json(new { result = objResult });


            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                return Json(new { result = "0" });
            }
            finally
            {
                objGolfer = null;

            }
        }

        public ActionResult NewIncommingMsgsCount(int isGolfer)
        {
            AccessModule(ModuleValues.AllRights);
            int NewMsgCount = 0;
            try
            {
                long Id = 0;
                GF_Golfer objGolfer = new GF_Golfer();
                if (isGolfer > 0)
                {
                    Id = LoginInfo.GolferUserId;
                }
                else
                {
                    Id = LoginInfo.UserId;
                }
                NewMsgCount = objGolfer.GetNewMsgCount(Id, isGolfer);
                if (isGolfer == 1)
                {
                    if (Session["GolferNewMsgCount"] == null)
                    {
                        Session["GolferNewMsgCount"] = NewMsgCount;
                    }
                    else
                    {
                        int NoOfMsgs = Convert.ToInt32(Session["GolferNewMsgCount"]);

                        if (NoOfMsgs == NewMsgCount)
                        {
                            NewMsgCount = 0;
                        }
                        else
                        {
                            Session["GolferNewMsgCount"] = NewMsgCount;
                        }
                    }
                }
                else
                {
                    if (Session["CourseNewMsgCount"] == null)
                    {
                        Session["CourseNewMsgCount"] = NewMsgCount;
                    }
                    else
                    {
                        int NoOfMsgs = Convert.ToInt32(Session["CourseNewMsgCount"]);

                        if (NoOfMsgs == NewMsgCount)
                        {
                            NewMsgCount = 0;

                        }
                        else
                        {
                            Session["CourseNewMsgCount"] = NewMsgCount;
                        }
                    }
                }
                NewMsgCountResult Countresult = new NewMsgCountResult();
                Countresult.NewMsgCount = NewMsgCount;

                return Json(new { result = Countresult });
            }

            catch (Exception ex)
            {
                string msg = ex.Message;
                return Json(new { result = 0 });
            }
            finally
            {
                objGolfer = null;
            }
        }

        public ActionResult BlockUsers(string UserId, string BlockedUserId, bool IsGolferUser, bool IsBlockedGolferUser, string Block)
        {

            AccessModule(ModuleValues.AllRights);
            GF_Golfer objGolfer = new GF_Golfer();
            try
            {
                long _userId = Convert.ToInt64(UserId.Trim());
                long _blockedUserId = Convert.ToInt64(BlockedUserId.Trim());

                if (IsGolferUser == true)
                {
                    var obj = Db.GF_Golfer.FirstOrDefault(x => x.GF_ID == _userId);
                    if (obj != null)
                    {

                        string result = objGolfer.BlockNUnblockUser(_userId, _blockedUserId, IsGolferUser, IsBlockedGolferUser, Block);

                        return Json(new { result = result });
                    }
                    else
                    {
                        return Json(new { result = "-1" });
                    }
                }
                else
                {
                    return Json(new { result = "-1" });
                }

            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                return Json(new { result = "-1", });
            }
            finally
            {
                objGolfer = null;
            }
        }
    }
}