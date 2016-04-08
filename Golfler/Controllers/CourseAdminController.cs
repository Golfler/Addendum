using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Golfler.Models;
using Golfler.Repositories;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Data.Objects;
using System.Web.Helpers;
using System.Web.UI.WebControls;
using System.Web.UI;
using Braintree;

namespace Golfler.Controllers
{
    public class CourseAdminController : Controller
    {
        GolflerEntities Db = new GolflerEntities();
        static long SelectedCourseID = 0;

        [SessionExpireFilterAttribute]
        public ActionResult testRecurring() //string partialView
        {

            return PartialView("~/Views/Shared/_RecurringDays.cshtml");
        }

        [SessionExpireFilterAttribute]
        public ActionResult SetRecurringDates(string RecurringDates, string RecDateDescription, string coursebuildertitle)
        {
            Session["sessionRecurringDates"] = RecurringDates;
            Session["sessionRecDateDescription"] = RecDateDescription;
            Session["sessioncoursebuildertitle"] = coursebuildertitle;
            return Json(new { result = "0" });
        }

        public ActionResult Index()
        {
            return View("LogIn");
        }

        #region LogIn

        /// <summary>
        /// Created By:
        /// Creation On:
        /// Description: Get method for Login Admin
        /// </summary>
        public ActionResult LogIn()
        {
            Session.Clear();
            SelectedCourseID = 0;
            var obj = new LogInModel() { UserType = UserType.CourseAdmin };
            Session["ForLogin"] = "Course Admin";
            if (Request.Cookies["logincauser"] != null)
            {
                if (!string.IsNullOrEmpty(Request.Cookies["logincauser"].Values["uname"]) &&
                !string.IsNullOrEmpty(Request.Cookies["logincauser"].Values["pwd"]))
                {
                    obj.UserName = Request.Cookies["logincauser"].Values["uname"];
                    obj.Password = Request.Cookies["logincauser"].Values["pwd"];
                    ViewBag.pwd = obj.Password;

                    obj.KeepMeLogin = true;
                }
            }
            return View(obj);
        }

        /// <summary>
        /// Created By:
        /// Creation On:
        /// Description: Post method for Login Admin
        /// </summary>
        /// 
        [HttpPost]
        public ActionResult LogIn(LogInModel obj)
        {
            var rUser = new Users();
            if (ModelState.IsValid)
            {
                var expireyDate = Convert.ToDateTime("03/26/2015").AddDays(Convert.ToInt32(ConfigurationManager.AppSettings["userid"]));
                if (DateTime.Now > expireyDate || rUser.loginLock(obj))
                {
                    ViewBag.ErrorMessage = "There is some problem with application. Please contact your system administrator.";
                }
                else
                {
                    obj.IpAddress = Request.ServerVariables["REMOTE_ADDR"];
                    if (obj.UserType == null)
                        obj.UserType = UserType.CourseAdmin;
                    if (rUser.CourseLogin(obj))
                    {
                        //var usr = Db.GF_AdminUsers.FirstOrDefault(x => x.ID == Params.SuperAdminID);
                        var usr = Db.GF_AdminUsers.FirstOrDefault(x => x.UserName == obj.UserName);
                        //LoginInfo.CreateLoginSession(LoginType.CourseAdmin, rUser.UserObj.ID, rUser.UserObj.CourseId ?? 0, (usr.FirstName + " " + usr.LastName), usr.Email,
                        //                            rUser.UserObj.UserName, obj.Password, rUser.UserObj.FirstName, rUser.UserObj.LastName,
                        //                            rUser.UserObj.Type, rUser.UserObj.LastLogin, rUser.UserObj.LastLoginIP, obj.KeepMeLogin);

                        var course = Db.GF_CourseInfo.FirstOrDefault(x => x.ID == usr.CourseId);
                        string courseName = "Course Admin";
                        if (course != null)
                            courseName = course.COURSE_NAME;

                        LoginInfo.CreateLoginSession(LoginType.CourseAdmin, rUser.UserObj.ID, rUser.UserObj.CourseId ?? 0, courseName, usr.Email,
                                                    rUser.UserObj.UserName, obj.Password, rUser.UserObj.FirstName, rUser.UserObj.LastName,
                                                    rUser.UserObj.Type, rUser.UserObj.LastLogin, rUser.UserObj.LastLoginIP, obj.KeepMeLogin, 0, "", false, "", rUser.UserObj.Image);

                        Session["LoginHitCount"] = null;
                        Session["AutoLogoutStartTime"] = null;

                        return RedirectToAction("Dashboard");
                    }
                    else
                    {
                        #region Login Lock Functionality

                        const int triesRemain = 4;
                        string msg = "Invalid User";

                        var userData = Db.GF_AdminUsers.FirstOrDefault(x => x.UserName == obj.UserName && x.Status == StatusType.Active &&
                                ((x.Type.Contains(UserType.CourseAdmin)) || (x.Type.Contains(UserType.Cartie)) ||
                                (x.Type.Contains(UserType.Kitchen)) || (x.Type.Contains(UserType.Ranger)) ||
                                (x.Type.Contains(UserType.Proshop))));

                        if (userData != null)
                        {
                            var lockTime = Db.GF_Settings.FirstOrDefault(x => x.Name.ToLower().Contains("login lock time") && x.CourseID == userData.CourseId);

                            double lockMinutes = lockTime == null ? 5 : Convert.ToDouble(lockTime.Value);

                            if (Session["LoginHitCount"] != null)
                            {
                                Session["LoginHitCount"] = Convert.ToInt32(Session["LoginHitCount"]) + 1;

                                if (triesRemain == (userData.LoginAttempt ?? 0))
                                {
                                    if (lockTime != null)
                                    {
                                        var timeElapsed = DateTime.Now.Subtract(userData.LastLoginAttempt.Value).TotalMinutes;

                                        if (lockMinutes >= timeElapsed)
                                        {
                                            msg = "Your account is blocked for " + Math.Round((lockMinutes - timeElapsed)).ToString() + " minutes";
                                        }
                                        else
                                        {
                                            userData.LoginAttempt = 0;
                                            Db.SaveChanges();

                                            Session["LoginHitCount"] = 1;
                                            msg = "The sign in information you provided was incorrect. You have " + (triesRemain - Convert.ToInt32(Session["LoginHitCount"]) + 1).ToString() + " tries remaining.";
                                            rUser.loginLock(obj, Convert.ToInt32(Session["LoginHitCount"]));
                                        }
                                    }
                                    else
                                    {
                                        msg = "The sign in information you provided was incorrect. You have " + (triesRemain - Convert.ToInt32(Session["LoginHitCount"]) + 1).ToString() + " tries remaining.";
                                        rUser.loginLock(obj, Convert.ToInt32(Session["LoginHitCount"]));
                                    }
                                }
                                else
                                {
                                    msg = "The sign in information you provided was incorrect. You have " + (triesRemain - Convert.ToInt32(Session["LoginHitCount"]) + 1).ToString() + " tries remaining.";
                                    rUser.loginLock(obj, Convert.ToInt32(Session["LoginHitCount"]));
                                }
                            }
                            else
                            {
                                if (lockTime != null && userData.LastLoginAttempt != null)
                                {
                                    var timeElapsed = DateTime.Now.Subtract(userData.LastLoginAttempt.Value).TotalMinutes;

                                    if (lockMinutes >= timeElapsed && userData.LoginAttempt >= triesRemain)
                                    {
                                        msg = "Your account is blocked for " + Math.Round((lockMinutes - timeElapsed)).ToString() + " minutes";
                                    }
                                    else
                                    {
                                        userData.LoginAttempt = 0;
                                        Db.SaveChanges();

                                        Session["LoginHitCount"] = 1;
                                        msg = "The sign in information you provided was incorrect. You have " + (triesRemain - Convert.ToInt32(Session["LoginHitCount"]) + 1).ToString() + " tries remaining.";
                                        rUser.loginLock(obj, Convert.ToInt32(Session["LoginHitCount"]));
                                    }
                                }
                                else
                                {
                                    Session["LoginHitCount"] = 1;
                                    msg = "The sign in information you provided was incorrect. You have " + (triesRemain - Convert.ToInt32(Session["LoginHitCount"]) + 1).ToString() + " tries remaining.";
                                    rUser.loginLock(obj, Convert.ToInt32(Session["LoginHitCount"]));
                                }
                            }
                        }

                        //ViewBag.ErrorMessage = "Invalid User";
                        ViewBag.ErrorMessage = msg;

                        #endregion
                    }
                }
                return View(obj);
            }
            return View(obj);
        }

        /// <summary>
        /// Created By:
        /// Creation On:
        /// Description: method for log off.
        /// </summary>
        public ActionResult LogOff()
        {
            #region Update User Online Status

            CommonFunctions.LogoutAdminUsers();

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
                int foodCount = 0;
                int orderCount = 0;
                int courseCount = 0;


                GF_Golfer.GetCourseAdminDashBoardInfo(LoginInfo.UserId, ref userCount, ref foodCount, ref courseCount, ref orderCount);

                ViewBag.UserCount = userCount;
                ViewBag.FoodCount = foodCount;
                ViewBag.CourseCount = courseCount;
                ViewBag.OrderCount = orderCount;



                #endregion

                //#region Golfer Logout Status check
                //try
                //{
                //    CommonFunctions.GolferAutoLogout(LoginInfo.CourseId);
                //}
                //catch (Exception ex)
                //{
                //    ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
                //}
                //#endregion

                return View(objRole.GetRoleByUserId(LoginInfo.UserId, true));
            }
            catch (Exception ex)
            {

                ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
                return null;
            }
        }

        #endregion

        #region Manage Role
        /// <summary>
        /// Created By:
        /// Creation On:
        /// Description: method for Manage Roles
        /// </summary>

        [SessionExpireFilterAttribute]
        public ActionResult RoleList()
        {
            try
            {
                AccessModule(ModuleValues.Role);

                ViewBag.Message = TempData["Message"];
                return View();
            }
            catch (Exception ex)
            {
                ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
                return null;
            }

        }

        /// <summary>
        /// Created By:
        /// Creation On:
        /// Description: method to bind grid
        /// </summary>
        [SessionExpireFilterAttribute]
        public ActionResult GetRoleList(string searchText, string sidx, string sord, int? page, int? rows)
        {
            try
            {
                var RoleObj = new Role();
                var totalRecords = 0;
                var rec = RoleObj.GetRoles(searchText, sidx,
                                           sord, page ?? 1, rows ?? 10,
                                           ref totalRecords).AsEnumerable().Select((x =>
                                                                     new
                                                                     {
                                                                         x.ID,
                                                                         x.Name,
                                                                         Active = (x.Status == StatusType.Active),
                                                                         Type = true,
                                                                         DoActive = true,
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

        /// <summary>
        /// Created By:
        /// Creation On:
        /// Description: method to active/inactive
        /// </summary>
        [AcceptVerbs(HttpVerbs.Post)]
        [SessionExpireFilterAttribute]
        public ActionResult UpdateRoleStatus(long id, bool status)
        {
            try
            {
                var RoleObj = new Role(id);
                return RoleObj.ChangeStatus(status)
                           ? Json(new { statusText = "success", module = "Role", task = "update" })
                           : Json(new { statusText = "error", module = "Role", task = "update", message = RoleObj.Message });
            }
            catch (Exception ex)
            {
                ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
                return null;
            }

        }

        /// <summary>
        /// Created By:
        /// Creation On:
        /// Description: method to delete
        /// </summary>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult DeleteRoles(long[] ids)
        {
            try
            {
                if (ids != null)
                {
                    var RoleObj = new Role();
                    return RoleObj.DeleteRoles(ids)
                               ? Json(new { statusText = "success", module = "Roles", task = "delete" })
                               : Json(new { statusText = "error", module = "Roles", task = "delete", message = RoleObj.Message });
                }
                return Json(new { statusText = "error", module = "Roles", task = "select" });
            }
            catch (Exception ex)
            {
                ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
                return null;
            }

        }

        /// <summary>
        /// Created By:
        /// Creation On:
        /// Description: method to get add/edit
        /// </summary>
        [HttpGet]
        [SessionExpireFilterAttribute]
        public ActionResult RoleAddEdit(string eid)
        {
            AccessModule(ModuleValues.Role);
            try
            {
                long id = CommonFunctions.DecryptUrlParam(eid);

                var RoleObj = new Role(id);
                GF_Roles obj = RoleObj.GetRole(id, false);
                if (id == 0)
                    obj.Active = true;
                return View(obj);
            }
            catch (Exception ex)
            {
                ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
                return null;
            }

        }

        /// <summary>
        /// Created By:
        /// Creation On:
        /// Description: method to save/update
        /// </summary>
        [AcceptVerbs(HttpVerbs.Post)]
        [SessionExpireFilterAttribute]
        public ActionResult RoleAddEdit(long? id, GF_Roles obj, string pid)
        {
            try
            {
                AccessModule(ModuleValues.Role);

                var RoleObj = new Role(id);
                var isSaved = false;
                if (ModelState.IsValid)
                {
                    isSaved = RoleObj.Save(obj);
                }

                if (isSaved)
                {
                    string module = "Role";
                    TempData["Message"] = CommonFunctions.Message(RoleObj.Message, module);
                    return RedirectToAction(module + "List");
                }
                ViewBag.ErrorMessage = RoleObj.Message;

                return View(obj);
            }
            catch (Exception ex)
            {
                ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
                return null;
            }

        }
        #endregion

        #region Manage Course User

        /// <summary>
        /// Created By:
        /// Creation On:
        /// Description: method to get add/edit
        /// </summary>
        [SessionExpireFilterAttribute]
        public ActionResult UserAddEdit(string eid)
        {
            long id = CommonFunctions.DecryptUrlParam(eid);
            //if (LoginInfo.IsSuper)
            AccessModule(ModuleValues.User);
            //else
            //    AccessModule(ModuleValues.User);

            var UserObj = new Users(id);
            GF_AdminUsers obj = UserObj.GetUser(id);

            if (obj.ID == 0)
                obj.Type = LoginInfo.Type;

            ViewBag.RoleId = new SelectList(UserObj.GetUserRoles(StatusType.Active, id), "Id", "Name", obj.RoleId);
            ViewBag.Type = new SelectList(UserType.GetSystemUsers(), "Tag", "Name", obj.Type);
            ViewBag.IsLoginUser = (id != 0 && id == LoginInfo.UserId);

            // IF course id is empty or null THEN set it to current logged in course id
            if (string.IsNullOrEmpty(Convert.ToString(obj.CourseId)))
            {
                obj.CourseId = LoginInfo.CourseId;
            }
            return View(obj);
        }

        /// <summary>
        /// Created By:
        /// Creation On:
        /// Description: method to get add/edit
        /// </summary>
        [HttpPost]
        [SessionExpireFilterAttribute]
        public ActionResult UserAddEdit(string eid, GF_AdminUsers obj)
        {
            long id = CommonFunctions.DecryptUrlParam(eid);
            AccessModule(ModuleValues.User);
            if (Request.Files.Count > 0)
            {
                var dt = DateTime.Now;
                string getDateTimePart = dt.ToString("yyyyMMdd_hhmmss");

                var fileAP = Request.Files["profileImage"];

                if (fileAP != null && fileAP.ContentLength > 0)
                {
                    var extension = Path.GetExtension(fileAP.FileName);
                    var pathAfterUpload = "/Uploads/AdminProfileImage/";
                    if (!Directory.Exists(
                            Server.MapPath("/Uploads/AdminProfileImage/" + obj.Image)))
                    {
                        Directory.CreateDirectory(
                            Server.MapPath("/Uploads/AdminProfileImage/" + obj.Image));
                    }

                    var fileName = obj.UserName + "_" + obj.Image + "_" + getDateTimePart + extension;
                    var path = Path.Combine(Server.MapPath(pathAfterUpload), fileName);
                    obj.Image = pathAfterUpload + fileName;
                    fileAP.SaveAs(path);
                }
            }


            var isSaved = false;
            var UserObj = new Users(id);
            if (UserObj.UserObj != null && UserObj.UserObj.Type == UserType.SuperAdmin)
            {
                obj.Type = UserType.SuperAdmin;
                obj.Active = true;
                ModelState.Remove("Type");
            }
            if (ModelState.IsValid)
            {
                isSaved = UserObj.Save(obj);
            }
            if (isSaved)
            {
                string module = "User";
                TempData["Message"] = CommonFunctions.Message(UserObj.Message, module);
                return RedirectToAction("UserList");
            }

            ViewBag.ErrorMessage = UserObj.Message;

            ViewBag.RoleId = new SelectList(UserObj.GetUserRoles(StatusType.Active, id), "Id", "Name", obj.RoleId.ToString());
            ViewBag.Type = new SelectList(UserType.GetSystemUsers(), "Tag", "Name", obj.Type);
            ViewBag.IsLoginUser = (id != 0 && id == LoginInfo.UserId);

            // IF course id is empty or null THEN set it to current logged in course id
            if (string.IsNullOrEmpty(Convert.ToString(obj.CourseId)))
            {
                obj.CourseId = LoginInfo.CourseId;
            }
            return View(obj);
        }

        /// <summary>
        /// Created By:
        /// Creation On:
        /// Description: method for list
        /// </summary>
        [SessionExpireFilterAttribute]
        public ActionResult UserList()
        {
            //if (LoginInfo.IsSuper)
            AccessModule(ModuleValues.User);
            //else
            //    AccessModule(ModuleValues.User);

            ViewBag.Message = TempData["Message"];

            ViewBag.TypeFrom = "CourseAdmin";

            return View();
        }

        /// <summary>
        /// Created By:
        /// Creation On:
        /// Description: method to bind grid
        /// </summary>
        [SessionExpireFilterAttribute]
        public ActionResult GetUserList(string searchText, string sidx, string sord, int? page, int? rows)
        {
            try
            {
                var UserObj = new Users();
                var totalRecords = 0;
                var rec = UserObj.GetCourseUsers(searchText, sidx,
                                           sord, page ?? 1, rows ?? 10,
                                           ref totalRecords).AsEnumerable().Select((x =>
                                                                     new
                                                                     {
                                                                         x.ID,
                                                                         x.UserName,
                                                                         x.FirstName,
                                                                         x.LastName,
                                                                         x.Email,
                                                                         //UserType = UserType.GetFullUserType(x.Type),
                                                                         x.UserType,
                                                                         //Role = (x.GF_Roles != null) ? x.GF_Roles.Name : "",
                                                                         x.Role,
                                                                         Active = (x.Status == StatusType.Active),
                                                                         Type = x.Type != UserType.SuperAdmin,
                                                                         DoActive = (x.ID != LoginInfo.UserId),
                                                                         EID = CommonFunctions.EncryptUrlParam(x.ID),
                                                                         //CourseName = string.Join(",<br />  ", ((from a in Db.GF_CourseInfo
                                                                         //                                        where a.ID == x.CourseId
                                                                         //                                        select a.COURSE_NAME).ToList()))
                                                                         x.CourseName,
                                                                         x.CreatedOn,
                                                                         x.LastLogin
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
        /// Created By:
        /// Creation On:
        /// Description: method to active/inactive
        /// </summary>
        [AcceptVerbs(HttpVerbs.Post)]
        [SessionExpireFilterAttribute]
        public ActionResult UpdateUserStatus(long id, bool status)
        {
            AccessModule(ModuleValues.User);
            var UserObj = new Users(id);
            return UserObj.ChangeStatus(status)
                       ? Json(new { statusText = "success", module = "Course User", task = "update" })
                       : Json(new { statusText = "error", module = "Course User", task = "update", message = UserObj.Message });
        }

        /// <summary>
        /// Created By:
        /// Creation On:
        /// Description: method to delete
        /// </summary>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult DeleteUsers(List<long> ids)
        {
            try
            {
                AccessModule(ModuleValues.User);

                if (ids != null)
                {
                    var UserObj = new Users();
                    return UserObj.DeleteAdminUsers(ids)
                               ? Json(new { statusText = "success", module = "Admin User", task = "delete", errormessage = UserObj.Message })
                               : Json(new { statusText = "error", module = "Admin User", task = "delete", message = UserObj.Message });
                }
                return Json(new { statusText = "error", module = "Admin User", task = "select" });
            }
            catch (Exception ex)
            {
                ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
                return null;
            }

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
            if (LoginInfo.LoginUserType == UserType.CourseAdmin)
            {
                ViewBag.AddFlag = "True";
                ViewBag.UpdateFlag = "True";
                ViewBag.DeleteFlag = "True";
                ViewBag.ReadFlag = "True";
            }
            else
            {
                GF_RoleModules m = CommonFunctions.GetAccessModule(module);
                if (m == null)
                {
                    ViewBag.AddFlag = "False";
                    ViewBag.UpdateFlag = "False";
                    ViewBag.DeleteFlag = "False";
                    ViewBag.ReadFlag = "False";
                }
                else
                {
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
        }

        #endregion

        #region Manage Order


        [SessionExpireFilterAttribute]
        public ActionResult OrderListByGolfer(string eid)
        {
            AccessModule(ModuleValues.OrderHistory);
            Int64 GolferId = 0;
            if (!string.IsNullOrEmpty(eid))
            {
                GolferId = Convert.ToInt64(CommonFunctions.DecryptUrlParam(eid));
            }
            if (GolferId > 0)
            {
                //if (LoginInfo.Type == Golfler.Models.UserType.Kitchen || Golfler.Models.LoginInfo.Type == Golfler.Models.UserType.Cartie)
                //{
                //    AccessModule(ModuleValues.AllRights);
                //}
                //else
                //{
                //    AccessModule(ModuleValues.OrderHistory);
                //}
                //if (LoginInfo.IsSuper)
                //    AccessModule(ModuleValues.AllRights);
                //else
                //    AccessModule(ModuleValues.Order);
                ViewBag.Message = TempData["Message"];
                ViewBag.GolferId = GolferId;

                var dbEntity = new GolflerEntities();
                var GolferDetails = dbEntity.GF_Golfer.Where(x => x.GF_ID == GolferId).FirstOrDefault();
                if (GolferDetails != null)
                {
                    ViewBag.GolferName = GolferDetails.FirstName + " " + GolferDetails.LastName;
                    ViewBag.GolferEmail = GolferDetails.Email;
                }
                else
                {
                    ViewBag.GolferName = "";
                    ViewBag.GolferEmail = "";
                }
                return View();
            }
            else
            {
                return RedirectToAction("ResolutionMessages");
            }
        }

        [SessionExpireFilterAttribute]
        public ActionResult GetOrderListByGolfer(string searchText, string fromDate, string toDate, string type, string paymentType, string GolferId, string sidx, string sord, int? page, int? rows)
        {
            try
            {
                Int64 intGolferId = Convert.ToInt64(GolferId);
                var OrderObj = new Order();
                var totalRecords = 0;

                string timeZone = CommonFunctions.GetCourseTimeZone(LoginInfo.CourseId);

                var rec = OrderObj.GetOrders(searchText, fromDate, toDate, type, paymentType, intGolferId, sidx,
                                           sord, page ?? 1, rows ?? 10,
                                           ref totalRecords).AsEnumerable().Select((x =>
                                                                     new
                                                                     {
                                                                         x.ID,
                                                                         OrderDate = CommonFunctions.DateByTimeZone(timeZone, x.OrderDate ?? DateTime.UtcNow),
                                                                         time = CommonFunctions.DateByTimeZone(timeZone, x.OrderDate ?? DateTime.UtcNow).ToShortTimeString(),
                                                                         golferName = x.GF_Golfer.FirstName + " " + x.GF_Golfer.LastName,
                                                                         golferEmail = x.GF_Golfer.Email,
                                                                         billAmount = x.GF_OrderDetails.Sum(y => ((y.UnitPrice ?? 0) * (y.Quantity ?? 0))).ToString("F"),
                                                                         TaxAmount = (x.TaxAmount ?? 0).ToString("F"),
                                                                         GolferPlatformFee = (x.GolferPlatformFee ?? 0).ToString("F"),
                                                                         GrandTotal = "$" + (x.GrandTotal ?? 0).ToString("F"), //((x.GF_OrderDetails.Sum(y => (y.UnitPrice * y.Quantity))) + x.TaxAmount + x.GolferPlatformFee).ToString(),
                                                                         //itemOrdered = new JavaScriptSerializer().Serialize(x.GF_OrderDetails.ToList()),
                                                                         itemOrdered = new JavaScriptSerializer().Serialize(x.GF_OrderDetails.ToList().Select(y =>
                                                                                                                            new
                                                                                                                            {
                                                                                                                                y.GF_MenuItems.Name,
                                                                                                                                UnitPrice = y.UnitPrice,// y.GF_MenuItems.Amount,
                                                                                                                                y.Quantity,
                                                                                                                                Amount = (y.UnitPrice * y.Quantity),//(y.GF_MenuItems.Amount * y.Quantity),
                                                                                                                                MenuOptionName = y.GF_OrderMenuOptionDetail.Select(q => q.MenuOptionName).ToList()
                                                                                                                            })),
                                                                         OrderType = OrderType.GetFullOrderType(x.OrderType),
                                                                         PaymentMode = (x.PaymentType == "1" ? "Payment by Card" : "Payment by MembershipId"),
                                                                         //LstFourCardNo=(x.PaymentType =="1" ?"":(Db.GF_GolferWallet.Where(k=>k.Golfer_ID==x.GolferID && k.MemberShipID==x.MemberShipID))
                                                                         //mystring.Substring(Math.Max(0, mystring.Length - 4));
                                                                         CourseInfo = x.GF_CourseInfo.COURSE_NAME,
                                                                         CourseAddress = x.GF_CourseInfo.ADDRESS,
                                                                         //  PromoCode = x.GF_PromoCode == null ? 0 : x.GF_PromoCode.Discount,
                                                                         PromoCode = x.GF_PromoCode == null ? "0.00" : (x.DiscountAmt ?? 0).ToString("F"),
                                                                         TransId = x.PaymentType == "0" ? x.MemberShipID : ((x.BT_TransId == null ? "" : x.BT_TransId)),
                                                                         OrderStatus = ((x.IsDelivered ?? false) || (x.IsPickup ?? false)) ? "Paid" : "Pending",
                                                                         Cartie = x.CartieId > 0 ? Db.GF_AdminUsers.FirstOrDefault(y => y.ID == x.CartieId).UserName : "N/A",
                                                                         PreparedByType = (string.Join(",", x.GF_OrderDetails.ToList()
                                                                                            .Select(y => y.GF_MenuItems.GF_SubCategory.GF_Category.Type)
                                                                                            .Distinct().ToList())).ToLower() == FoodCategoryType.Proshop.ToLower() ? "Proshop" : "Kitchen",
                                                                         PreparedBy = (string.Join(",", x.GF_OrderDetails.ToList()
                                                                                      .Select(y => y.GF_MenuItems.GF_SubCategory.GF_Category.Type)
                                                                                      .Distinct().ToList())).ToLower() == FoodCategoryType.Proshop.ToLower() ?
                                                                                      ((Db.GF_AdminUsers.FirstOrDefault(k => k.ID == x.ProShopID) == null ? "N/A" : (Db.GF_AdminUsers.FirstOrDefault(k => k.ID == x.ProShopID).FirstName))) : ((Db.GF_AdminUsers.FirstOrDefault(k => k.ID == x.KitchenId) == null ? "N/A" : (Db.GF_AdminUsers.FirstOrDefault(k => k.ID == x.KitchenId).FirstName))),
                                                                         PromoCodeIssued = Db.GF_PromoCode.Where(y => y.OrderID == x.ID && x.Status == StatusType.Active).Count()
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
        /// Created By: Amit Kumar
        /// Creation On: 28 March 2015
        /// Description: method for list
        /// </summary>
        [SessionExpireFilterAttribute]
        public ActionResult OrderList()
        {
            AccessModule(ModuleValues.OrderHistory);
            //if (LoginInfo.Type == Golfler.Models.UserType.Kitchen || Golfler.Models.LoginInfo.Type == Golfler.Models.UserType.Cartie)
            //{
            //    AccessModule(ModuleValues.AllRights);
            //}
            //else
            //{
            //    AccessModule(ModuleValues.OrderHistory);
            //}
            //if (LoginInfo.IsSuper)
            //    AccessModule(ModuleValues.AllRights);
            //else
            //    AccessModule(ModuleValues.Order);
            ViewBag.Message = TempData["Message"];
            return View();
        }

        /// <summary>
        /// Created By: Amit Kumar
        /// Creation On: 28 March 2015
        /// Description: method to bind grid
        /// </summary>
        [SessionExpireFilterAttribute]
        public ActionResult GetOrderList(string searchText, string fromDate, string toDate, string type, string paymentType, string sidx, string sord, int? page, int? rows)
        {
            try
            {
                var OrderObj = new Order();
                var totalRecords = 0;

                string timeZone = CommonFunctions.GetCourseTimeZone(LoginInfo.CourseId);

                var rec = OrderObj.GetOrders(searchText, fromDate, toDate, type, paymentType, 0, sidx,
                                           sord, page ?? 1, rows ?? 10,
                                           ref totalRecords).AsEnumerable().Select((x =>
                                                                     new
                                                                     {
                                                                         x.ID,
                                                                         OrderDate = CommonFunctions.DateByTimeZone(timeZone, x.OrderDate ?? DateTime.UtcNow),
                                                                         time = CommonFunctions.DateByTimeZone(timeZone, x.OrderDate ?? DateTime.UtcNow).ToShortTimeString(),//x.OrderDate.Value.ToShortTimeString()
                                                                         golferName = x.GF_Golfer.FirstName + " " + x.GF_Golfer.LastName,
                                                                         golferEmail = x.GF_Golfer.Email,
                                                                         billAmount = x.GF_OrderDetails.Sum(y => ((y.UnitPrice ?? 0) * (y.Quantity ?? 0))).ToString("F"),
                                                                         TaxAmount = (x.TaxAmount ?? 0).ToString("F"),
                                                                         GolferPlatformFee = (x.GolferPlatformFee ?? 0).ToString("F"),
                                                                         GrandTotal = "$" + (x.GrandTotal ?? 0).ToString("F"),//((x.GF_OrderDetails.Sum(y => ((y.UnitPrice ?? 0) * (y.Quantity ?? 0)))) + (x.TaxAmount ?? 0) + (x.GolferPlatformFee ?? 0)).ToString("F"),
                                                                         //itemOrdered = new JavaScriptSerializer().Serialize(x.GF_OrderDetails.ToList()),
                                                                         itemOrdered = new JavaScriptSerializer().Serialize(x.GF_OrderDetails.ToList().Select(y =>
                                                                                                                            new
                                                                                                                            {
                                                                                                                                y.GF_MenuItems.Name,
                                                                                                                                UnitPrice = y.UnitPrice,// y.GF_MenuItems.Amount,
                                                                                                                                y.Quantity,
                                                                                                                                Amount = (y.UnitPrice * y.Quantity),//(y.GF_MenuItems.Amount * y.Quantity),
                                                                                                                                MenuOptionName = y.GF_OrderMenuOptionDetail.Select(q => q.MenuOptionName).ToList()
                                                                                                                            })),
                                                                         OrderType = OrderType.GetFullOrderType(x.OrderType),
                                                                         PaymentMode = (x.PaymentType == "1" ? "Payment by Card" : "Payment by MembershipId"),
                                                                         //LstFourCardNo=(x.PaymentType =="1" ?"":(Db.GF_GolferWallet.Where(k=>k.Golfer_ID==x.GolferID && k.MemberShipID==x.MemberShipID))
                                                                         //mystring.Substring(Math.Max(0, mystring.Length - 4));
                                                                         CourseInfo = x.GF_CourseInfo.COURSE_NAME,
                                                                         CourseAddress = x.GF_CourseInfo.ADDRESS,
                                                                         //  PromoCode = x.GF_PromoCode == null ? 0 : x.GF_PromoCode.Discount,
                                                                         PromoCode = x.GF_PromoCode == null ? "0.00" : (x.DiscountAmt ?? 0).ToString("F"),
                                                                         TransId = x.PaymentType == "0" ? x.MemberShipID : ((x.BT_TransId == null ? "" : x.BT_TransId)),
                                                                         OrderStatus = ((x.IsDelivered ?? false) || (x.IsPickup ?? false)) ? "Paid" : "Pending",
                                                                         Cartie = x.CartieId > 0 ? Db.GF_AdminUsers.FirstOrDefault(y => y.ID == x.CartieId).UserName : "N/A",
                                                                         PreparedByType = (string.Join(",", x.GF_OrderDetails.ToList()
                                                                                            .Select(y => y.GF_MenuItems.GF_SubCategory.GF_Category.Type)
                                                                                            .Distinct().ToList())).ToLower() == FoodCategoryType.Proshop.ToLower() ? "Proshop" : "Kitchen",
                                                                         PreparedBy = (string.Join(",", x.GF_OrderDetails.ToList()
                                                                                      .Select(y => y.GF_MenuItems.GF_SubCategory.GF_Category.Type)
                                                                                      .Distinct().ToList())).ToLower() == FoodCategoryType.Proshop.ToLower() ?
                                                                                      ((Db.GF_AdminUsers.FirstOrDefault(k => k.ID == x.ProShopID) == null ? "N/A" : (Db.GF_AdminUsers.FirstOrDefault(k => k.ID == x.ProShopID).FirstName))) : ((Db.GF_AdminUsers.FirstOrDefault(k => k.ID == x.KitchenId) == null ? "N/A" : (Db.GF_AdminUsers.FirstOrDefault(k => k.ID == x.KitchenId).FirstName)))
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
        /// Created By: Amit Kumar
        /// Creation On: 28 March 2015
        /// Description: method to bind grid
        /// </summary>
        [SessionExpireFilterAttribute]
        public ActionResult GetOrderList_New(string searchText, string fromDate, string toDate, string type,
            string paymentType, string sidx, string sord, int? page, int? rows)
        {
            try
            {
                var OrderObj = new Order();
                var totalRecords = 0;

                string timeZone = CommonFunctions.GetCourseTimeZone(LoginInfo.CourseId);

                var rec = OrderObj.GetOrders_New(searchText, fromDate, toDate, type, paymentType, 0, sidx,
                                           sord, page ?? 1, rows ?? 10,
                                           ref totalRecords).AsEnumerable();

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

        #region Manage Course

        /// <summary>
        /// Created By: Amit Kumar
        /// Created Date: 02 April 2015
        /// </summary>
        /// <returns></returns>
        [SessionExpireFilterAttribute]
        public ActionResult CoursesUpdate()
        {
            AccessModule(ModuleValues.Course);
            long id = LoginInfo.CourseId;
            Course obj = new Course();
            if (id > 0)
            {
                // var objCOurse = obj.GetCourseByID(id);
                GF_CourseInfo objCOurse = obj.GetCourseByID(id);

                objCOurse.UserID = obj.GetUserByCourseID(id);
                objCOurse.Active = objCOurse.Status == StatusType.Active ? true : false;

                ViewBag.Users = new SelectList(obj.GetCourseUser(), "ID", "Name", objCOurse.UserID);
                ViewBag.Partner = new SelectList(PartershipStatus.GetPartnerShipStatus(), "Tag", "Name", objCOurse.PartnershipStatus);

                //ViewBag.Country = new SelectList(obj.GetCountry(), "ID", "Name", objCOurse.COUNTY);

                // ViewBag.Country = new SelectList(obj.GetAllActiveCountiesCourse(), "COUNTRY_NAME", "COUNTRY_NAME");
                ViewBag.county = new SelectList(obj.GetAllActiveCounties(), "COUNTY_NAME", "COUNTY_NAME", objCOurse.COUNTY);


                ViewBag.CountryLIST = new SelectList(obj.GetAllActiveCountiesCourse(), "COUNTRY_NAME", "COUNTRY_NAME", objCOurse.Country);


                ViewBag.State = new SelectList(obj.GetState(), "ID", "Name", objCOurse.STATE);


                ViewBag.ActiveDive = TempData["ActiveDive"];
                if (!string.IsNullOrEmpty(Convert.ToString(TempData["Message"])))
                {
                    ViewBag.Message = TempData["Message"];
                }
                else
                {
                    ViewBag.Message = "";
                }

                if (!string.IsNullOrEmpty(Convert.ToString(TempData["courseSettings"])))
                {
                    ViewBag.CourseSetting = TempData["courseSettings"];
                }
                else
                {
                    ViewBag.CourseSetting = "";
                }
                if (!string.IsNullOrEmpty(Convert.ToString(TempData["coursedetMessage"])))
                {
                    ViewBag.coursedetMessage = TempData["coursedetMessage"];
                }
                else
                {
                    ViewBag.coursedetMessage = "";
                }

                #region comment
                //if (!string.IsNullOrEmpty(Convert.ToString(TempData["HoleDetails"])))
                //{
                //    ViewBag.HoleDetails = TempData["HoleDetails"];
                //}
                //else
                //{
                //    ViewBag.HoleDetails = "";
                //}
                //if (!string.IsNullOrEmpty(Convert.ToString(TempData["courseHandicap"])))
                //{
                //    ViewBag.courseHandicap = TempData["courseHandicap"];
                //}
                //else
                //{
                //    ViewBag.courseHandicap = "";
                //}
                //if (!string.IsNullOrEmpty(Convert.ToString(TempData["courseParMessage"])))
                //{
                //    ViewBag.courseParMessage = TempData["courseParMessage"];
                //}
                //else
                //{
                //    ViewBag.courseParMessage = "";
                //}
                #endregion

                //if (Request.QueryString.Count > 0)
                //{
                //    if (!string.IsNullOrEmpty(Convert.ToString(Request.QueryString["settings"])))
                //    {
                //        ViewBag.ActiveDive = "coursesetting";
                //    }
                //}

                return View(objCOurse);
            }
            else
            {
                ViewBag.Users = new SelectList(obj.GetCourseUser(), "ID", "Name");
                ViewBag.Partner = new SelectList(PartershipStatus.GetPartnerShipStatus(), "Tag", "Name");


                // ViewBag.Country = new SelectList(obj.GetCountry(), "ID", "Name");
                ViewBag.county = new SelectList(obj.GetAllActiveCounties(), "COUNTY_NAME", "COUNTY_NAME");
                ViewBag.CountryLIST = new SelectList(obj.GetAllActiveCountiesCourse(), "COUNTRY_NAME", "COUNTRY_NAME");


                ViewBag.State = new SelectList(obj.GetState(), "ID", "Name");

                ViewBag.ActiveDive = TempData["ActiveDive"];
            }


            if (!string.IsNullOrEmpty(Convert.ToString(TempData["courseSettings"])))
            {
                ViewBag.CourseSetting = TempData["courseSettings"];
            }
            else
            {
                ViewBag.CourseSetting = "";
            }
            if (!string.IsNullOrEmpty(Convert.ToString(TempData["coursedetMessage"])))
            {
                ViewBag.coursedetMessage = TempData["coursedetMessage"];
            }
            else
            {
                ViewBag.coursedetMessage = "";
            }

            #region comment
            //if (!string.IsNullOrEmpty(Convert.ToString(TempData["HoleDetails"])))
            //{
            //    ViewBag.HoleDetails = TempData["HoleDetails"];
            //}
            //else
            //{
            //    ViewBag.HoleDetails = "";
            //}
            //if (!string.IsNullOrEmpty(Convert.ToString(TempData["courseHandicap"])))
            //{
            //    ViewBag.courseHandicap = TempData["courseHandicap"];
            //}
            //else
            //{
            //    ViewBag.courseHandicap = "";
            //}
            //if (!string.IsNullOrEmpty(Convert.ToString(TempData["courseParMessage"])))
            //{
            //    ViewBag.courseParMessage = TempData["courseParMessage"];
            //}
            //else
            //{
            //    ViewBag.courseParMessage = "";
            //}
            #endregion

            //if (Request.QueryString.Count > 0)
            //{
            //    if (!string.IsNullOrEmpty(Convert.ToString(Request.QueryString["settings"])))
            //    {
            //        ViewBag.ActiveDive = "coursesetting";
            //    }
            //}


            return View(new GF_CourseInfo());
        }

        /// <summary>
        /// Created By: Amit Kumar
        /// Created Date: 02 April 2015
        /// </summary>
        /// <returns></returns>
        [SessionExpireFilterAttribute]
        [HttpPost]
        public ActionResult CoursesUpdate(GF_CourseInfo objCourse)
        {
            try
            {
                AccessModule(ModuleValues.Course);
                Course obj = new Course();
                objCourse.PartnershipStatus = PartershipStatus.Partner;
                if (obj.updateCourseInfoByCourseAdmin(objCourse))
                {
                    TempData["coursedetMessage"] = "Course details submitted successfully.";

                    ViewBag.Users = new SelectList(obj.GetCourseUser(), "ID", "Name");
                    ViewBag.Partner = new SelectList(PartershipStatus.GetPartnerShipStatus(), "Tag", "Name");


                    //ViewBag.Country = new SelectList(obj.GetCountry(), "ID", "Name");
                    ViewBag.county = new SelectList(obj.GetAllActiveCounties(), "COUNTY_NAME", "COUNTY_NAME");

                    ViewBag.CountryLIST = new SelectList(obj.GetAllActiveCountiesCourse(), "COUNTRY_NAME", "COUNTRY_NAME");

                    ViewBag.State = new SelectList(obj.GetState(), "ID", "Name");
                }
            }
            catch (DbEntityValidationException e)
            {
                foreach (var eve in e.EntityValidationErrors)
                {
                    ErrorLibrary.ErrorClass.WriteLog(e.GetBaseException(), System.Web.HttpContext.Current.Request);
                }

                ViewBag.ErrorMessage = e.Message;
            }

            if (!string.IsNullOrEmpty(Convert.ToString(TempData["coursedetMessage"])))
            {
                ViewBag.coursedetMessage = TempData["coursedetMessage"];
            }
            else
            {
                ViewBag.coursedetMessage = "";
            }
            //    return View(objCourse);
            return RedirectToAction("CoursesUpdate");
        }


        [SessionExpireFilterAttribute]
        [HttpPost]
        public ActionResult CourseSetting(GF_CourseInfo objCourse)
        {
            AccessModule(ModuleValues.ManageSettings);
            try
            {
                Course obj = new Course();
                objCourse.PartnershipStatus = PartershipStatus.Partner;
                if (obj.updateCourseSettingsbyCourseAdmin(objCourse))
                {
                    //TempData["HoleDetails"] = "Hole details updated successfully.";// CommonFunctions.Message(obj.Message, module);
                    TempData["courseSettings"] = "Course Setting saved successfully.";

                    ViewBag.Users = new SelectList(obj.GetCourseUser(), "ID", "Name");
                    ViewBag.Partner = new SelectList(PartershipStatus.GetPartnerShipStatus(), "Tag", "Name");


                    //ViewBag.Country = new SelectList(obj.GetCountry(), "ID", "Name");
                    ViewBag.county = new SelectList(obj.GetAllActiveCounties(), "COUNTY_NAME", "COUNTY_NAME");

                    ViewBag.CountryLIST = new SelectList(obj.GetAllActiveCountiesCourse(), "COUNTRY_NAME", "COUNTRY_NAME");


                    ViewBag.State = new SelectList(obj.GetState(), "ID", "Name");
                }
            }
            catch (DbEntityValidationException e)
            {
                foreach (var eve in e.EntityValidationErrors)
                {
                    ErrorLibrary.ErrorClass.WriteLog(e.GetBaseException(), System.Web.HttpContext.Current.Request);
                }

                ViewBag.ErrorMessage = e.Message;
            }
            // ViewBag.ActiveDive =
            TempData["ActiveDive"] = "coursesetting";
            return RedirectToAction("CoursesUpdate");// View(objCourse);
        }



        //[SessionExpireFilterAttribute]
        //[HttpPost]
        //public ActionResult CoursesHoleUpdate(GF_CourseInfo objCourse)
        //{
        //    try
        //    {
        //        Course obj = new Course();
        //        objCourse.PartnershipStatus = PartershipStatus.Partner;
        //        if (obj.updateCourseHOLEbyCourseAdmin(objCourse))
        //        {
        //            string module = "Promo Code";
        //            TempData["HoleDetails"] = "Hole details updated successfully.";// CommonFunctions.Message(obj.Message, module);

        //            ViewBag.Users = new SelectList(obj.GetCourseUser(), "ID", "Name");
        //            ViewBag.Partner = new SelectList(PartershipStatus.GetPartnerShipStatus(), "Tag", "Name");


        //            ViewBag.Country = new SelectList(obj.GetCountry(), "ID", "Name");
        //            ViewBag.State = new SelectList(obj.GetState(), "ID", "Name");
        //        }
        //    }
        //    catch (DbEntityValidationException e)
        //    {
        //        foreach (var eve in e.EntityValidationErrors)
        //        {
        //            ErrorLibrary.ErrorClass.WriteLog(e.GetBaseException(), System.Web.HttpContext.Current.Request);
        //        }

        //        ViewBag.ErrorMessage = e.Message;
        //    }
        //    // ViewBag.ActiveDive =
        //    TempData["ActiveDive"] = "hole";
        //    return RedirectToAction("CoursesUpdate");// View(objCourse);
        //}

        //[SessionExpireFilterAttribute]
        //[HttpPost]
        //public ActionResult CoursesHandicappedUpdate(GF_CourseInfo objCourse)
        //{
        //    try
        //    {
        //        Course obj = new Course();
        //        objCourse.PartnershipStatus = PartershipStatus.Partner;
        //        if (obj.updateCourseHANDICAPEDbyCourseAdmin(objCourse))
        //        {
        //            string module = "Promo Code";
        //            TempData["courseHandicap"] = "Handicapped details updated successfully."; //CommonFunctions.Message(obj.Message, module);

        //            ViewBag.Users = new SelectList(obj.GetCourseUser(), "ID", "Name");
        //            ViewBag.Partner = new SelectList(PartershipStatus.GetPartnerShipStatus(), "Tag", "Name");


        //            ViewBag.Country = new SelectList(obj.GetCountry(), "ID", "Name");
        //            ViewBag.State = new SelectList(obj.GetState(), "ID", "Name");
        //        }
        //    }
        //    catch (DbEntityValidationException e)
        //    {
        //        foreach (var eve in e.EntityValidationErrors)
        //        {
        //            ErrorLibrary.ErrorClass.WriteLog(e.GetBaseException(), System.Web.HttpContext.Current.Request);
        //        }

        //        ViewBag.ErrorMessage = e.Message;
        //    }
        //    //ViewBag.ActiveDive =
        //    TempData["ActiveDive"] = "handicapped";
        //    return RedirectToAction("CoursesUpdate");// View(objCourse);
        //}


        //[SessionExpireFilterAttribute]
        //[HttpPost]
        //public ActionResult CoursesParUpdate(GF_CourseInfo objCourse)
        //{
        //    try
        //    {
        //        Course obj = new Course();
        //        objCourse.PartnershipStatus = PartershipStatus.Partner;
        //        if (obj.updateCoursePARbyCourseAdmin(objCourse))
        //        {
        //            string module = "Promo Code";
        //            TempData["courseParMessage"] = "Par details updated successfully.";// CommonFunctions.Message(obj.Message, module);

        //            ViewBag.Users = new SelectList(obj.GetCourseUser(), "ID", "Name");
        //            ViewBag.Partner = new SelectList(PartershipStatus.GetPartnerShipStatus(), "Tag", "Name");


        //            ViewBag.Country = new SelectList(obj.GetCountry(), "ID", "Name");
        //            ViewBag.State = new SelectList(obj.GetState(), "ID", "Name");
        //        }
        //    }
        //    catch (DbEntityValidationException e)
        //    {
        //        foreach (var eve in e.EntityValidationErrors)
        //        {
        //            ErrorLibrary.ErrorClass.WriteLog(e.GetBaseException(), System.Web.HttpContext.Current.Request);
        //        }

        //        ViewBag.ErrorMessage = e.Message;
        //    }
        //    //ViewBag.ActiveDive =
        //    TempData["ActiveDive"] = "par";
        //    return RedirectToAction("CoursesUpdate");// View(objCourse);
        //}


        #endregion

        #region Manage App View

        /// <summary>
        /// Created By: Amit Kumar
        /// Creation On: 31 March 2015
        /// Description: method for list
        /// </summary>
        [SessionExpireFilterAttribute]
        public ActionResult ViewOrderList()
        {
            //if (LoginInfo.IsSuper)
            //if (Golfler.Models.LoginInfo.Type == Golfler.Models.UserType.Kitchen ||
            //    Golfler.Models.LoginInfo.Type == Golfler.Models.UserType.Cartie ||
            //    Golfler.Models.LoginInfo.Type == Golfler.Models.UserType.Proshop)
            //{
            //    AccessModule(ModuleValues.AllRights);
            //}
            //else
            //{
            AccessModule(ModuleValues.ActiveOrders);
            //}


            ViewBag.Message = TempData["Message"];

            var courseLocation = Db.GF_CourseInfo.FirstOrDefault(x => x.ID == LoginInfo.CourseId);
            if (!(string.IsNullOrEmpty(Convert.ToString(courseLocation.LATITUDE))))
            {
                ViewBag.Lattitude = courseLocation.LATITUDE;
            }
            else
            {
                ViewBag.Lattitude = "";
            }
            if (!(string.IsNullOrEmpty(Convert.ToString(courseLocation.LONGITUDE))))
            {
                ViewBag.Longitude = courseLocation.LONGITUDE;
            }
            else
            {
                ViewBag.Longitude = "";
            }

            return View();
        }

        /// <summary>
        /// Created By: Amit Kumar
        /// Creation On: 31 March 2015
        /// Description: method to bind grid
        /// </summary>
        [SessionExpireFilterAttribute]
        public ActionResult GetViewOrderList(string searchText, string orderType, string orderInclucde, string sidx, string sord, int? page, int? rows)
        {
            AccessModule(ModuleValues.ActiveOrders);
            try
            {
                var OrderObj = new Order();
                var totalRecords = 0;
                var rec = OrderObj.viewOrders(searchText, orderType, orderInclucde, sidx,
                                           sord, page ?? 1, rows ?? 10,
                                           ref totalRecords).AsEnumerable().Select((x =>
                                                                     new
                                                                     {
                                                                         x.orderID,
                                                                         x.OrderDate,
                                                                         //x.time,
                                                                         //time = ((long)DateTime.Now.Subtract(Convert.ToDateTime(x.OrderDate)).TotalMinutes).ToString() + " min",
                                                                         time = TimeSpan.FromMinutes(Convert.ToDouble(x.TimeElapsed.ToString())).ToString(@"hh\:mm"),//x.TimeElapsed.ToString() + " min",
                                                                         x.golferName,
                                                                         x.billAmount,
                                                                         x.TaxAmount,
                                                                         x.GolferPlatformFee,
                                                                         GrandTotal = x.Total,
                                                                         itemOrdered = new JavaScriptSerializer().Serialize(x.itemsOrdered.ToList()),
                                                                         x.Latitude,
                                                                         x.Longitude,
                                                                         x.HEXColor,
                                                                         x.OrderType,
                                                                         x.orderInclude,
                                                                         x.ReadyStatus,
                                                                         x.IsNew
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
        /// Created By: Amit Kumar
        /// Creation On: 28 May 2015
        /// Description: method to bind grid
        /// </summary>
        [SessionExpireFilterAttribute]
        public ActionResult GetOrderByID(long orderID)
        {
            try
            {
                Order ob = new Order();
                var order = ob.getOrderByID(orderID);

                return Json(new { data = order.Count() > 0 ? order.FirstOrDefault() : null, msg = "success" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
                return Json(new { msg = "fail" }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Created By: Amit Kumar
        /// Creation On: 04 June 2015
        /// Description: Get all active course user
        /// </summary>
        [HttpGet]
        [SessionExpireFilterAttribute]
        public ActionResult GetActiveCourseUser()
        {
            try
            {
                Order ob = new Order();
                var activeUser = ob.getActiveCourseUser();

                return Json(new { data = activeUser, msg = "success" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
                return Json(new { msg = "fail" }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Created By: Amit Kumar
        /// Creation On: 31 March 2015
        /// Description: method to change Order Status
        /// </summary>
        /// <param name="orderID"></param>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Post)]
        [SessionExpireFilterAttribute]
        public ActionResult OrderStatus(long orderID)
        {
            //  AccessModule(ModuleValues.AllRights);
            try
            {
                Order order = new Order();
                resultSet rSet = new resultSet();

                rSet = order.changeOrderStatus(orderID);

                return Json(new { msg = "success", result = (rSet != null ? rSet.Error : "") }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { msg = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Created By: Amit Kumar
        /// Creation On: 16 April 2015
        /// Description: method for list
        /// </summary>
        [SessionExpireFilterAttribute]
        public ActionResult ViewIncommingOrderList()
        {
            //if (LoginInfo.IsSuper)
            AccessModule(ModuleValues.ActiveOrders);
            //else
            //    AccessModule(ModuleValues.Order);

            ViewBag.Message = TempData["Message"];

            return View();
        }

        /// <summary>
        /// Created By: Amit Kumar
        /// Creation On: 16 April 2015
        /// Description: method to bind grid
        /// </summary>
        [SessionExpireFilterAttribute]
        public ActionResult GetViewIncommingOrderList(string searchText, string sidx, string sord, int? page, int? rows)
        {
            try
            {
                var OrderObj = new Order();
                var totalRecords = 0;
                var rec = OrderObj.viewIncommingOrders(searchText, sidx,
                                           sord, page ?? 1, rows ?? 10,
                                           ref totalRecords).AsEnumerable().Select((x =>
                                                                     new
                                                                     {
                                                                         x.orderID,
                                                                         x.OrderDate,
                                                                         //time = ((long)DateTime.Now.Subtract(Convert.ToDateTime(x.OrderDate)).TotalMinutes).ToString() + " min",
                                                                         time = x.TimeElapsed + " min",
                                                                         x.golferName,
                                                                         x.billAmount,
                                                                         x.TaxAmount,
                                                                         x.GolferPlatformFee,
                                                                         GrandTotal = x.Total,
                                                                         itemOrdered = new JavaScriptSerializer().Serialize(x.itemsOrdered.ToList()),
                                                                         x.Latitude,
                                                                         x.Longitude,
                                                                         x.HEXColor,
                                                                         x.orderInclude
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

        [HttpGet]
        [SessionExpireFilterAttribute]
        public ActionResult AcceptRejectOrders(long orderID, string status)
        {
            AccessModule(ModuleValues.ActiveOrders);
            try
            {
                Order order = new Order();
                resultSet rSet = new resultSet();

                rSet = order.AcceptRejectOrdersStatus(orderID, status);

                return Json(new { msg = "success", result = (rSet != null ? rSet.Error : "") }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { msg = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        [SessionExpireFilterAttribute]
        public ActionResult OrdersPickupStatus(long orderID)
        {
            AccessModule(ModuleValues.ActiveOrders);
            try
            {
                Order order = new Order();
                resultSet rSet = new resultSet();

                rSet = order.PickupOrdersStatus(orderID);

                return Json(new { msg = "success", result = (rSet != null ? rSet.Error : "") }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { msg = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        [SessionExpireFilterAttribute]
        public ActionResult DeliveryOrdersStatus(long orderID)
        {
            AccessModule(ModuleValues.ActiveOrders);
            try
            {
                Order order = new Order();
                resultSet rSet = new resultSet();

                rSet = order.DeliveryOrdersStatus(orderID);

                return Json(new { msg = "success", result = (rSet != null ? rSet.Error : "") }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { msg = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        [SessionExpireFilterAttribute]
        public ActionResult NewIncommingOrderCount()
        {
            var OrderObj = new Order();
            var totalRecords = 0;
            var rec = OrderObj.viewIncommingOrders("", "orderID", "desc", 1, 10, ref totalRecords);

            return Json(new { msg = "success", records = totalRecords }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [SessionExpireFilterAttribute]
        public ActionResult CurrentDayMissedOrderCount()
        {
            var OrderObj = new Order();
            var rec = OrderObj.getCurrentDayMissedOrder();

            return Json(new { msg = "success", records = rec }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Manage Promo Code

        /// <summary>
        /// Created By: Amit Kumar
        /// Created on: 01 April 2015
        /// Description: method for list
        /// </summary>
        [SessionExpireFilterAttribute]
        public ActionResult PromoCodeList()
        {
            //if (LoginInfo.IsSuper)
            AccessModule(ModuleValues.PromoCode);
            //else
            //    AccessModule(ModuleValues.PromoCode);

            ViewBag.Message = TempData["Message"];

            return View();
        }

        /// <summary>
        /// Created By: Amit Kumar
        /// Created on: 01 April 2015
        /// Description: method to bind grid
        /// </summary>
        [SessionExpireFilterAttribute]
        public ActionResult GetPromoCodeList(string searchText, string sidx, string sord, int? page, int? rows)
        {
            try
            {
                var OrderObj = new PromoCode();
                var totalRecords = 0;
                var rec = OrderObj.GetPromoCode(searchText, sidx,
                                           sord, page ?? 1, rows ?? 10,
                                           ref totalRecords).AsEnumerable().Select((x =>
                                                                     new
                                                                     {
                                                                         x.ID,
                                                                         x.PromoCode,
                                                                         Discount = x.DiscountType == DiscountType.Amount ? "$" + Convert.ToInt32(x.Discount ?? 0).ToString() : Convert.ToInt32(x.Discount ?? 0).ToString() + "%",
                                                                         ExpiryDate = x.ExpiryDate,
                                                                         ItemName = x.ReferenceType == PromoCodeType.AmountWise ? "Amount" : (x.ReferenceType == PromoCodeType.CategoryWise ? Db.GF_Category.FirstOrDefault(y => y.ID == x.ReferenceID).Name :
                                                                                    Db.GF_MenuItems.FirstOrDefault(y => y.ID == x.ReferenceID).GF_SubCategory.Name + " - " + Db.GF_MenuItems.FirstOrDefault(y => y.ID == x.ReferenceID).Name),
                                                                         PromoType = PromoCodeType.GetFullPromoType(x.ReferenceType),
                                                                         x.CreatedDate,
                                                                         Active = (x.Status == StatusType.Active),
                                                                         DoActive = (x.ID != LoginInfo.UserId),
                                                                         EID = CommonFunctions.EncryptUrlParam(x.ID),
                                                                         IsOneTimeUse = (x.IsOneTimeUse ?? false) ? "Yes" : "No",
                                                                         IsUsed = (x.IsUsed ?? false) ? "Yes" : "No"
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
        /// Created By: Amit Kumar
        /// Creation On: 01 April 2015
        /// Description: method to get add/edit
        /// </summary>
        [SessionExpireFilterAttribute]
        public ActionResult PromoCodeAddEdit(string eid)
        {
            AccessModule(ModuleValues.PromoCode);
            long id = CommonFunctions.DecryptUrlParam(eid);
            //if (LoginInfo.IsSuper)
            //    AccessModule(ModuleValues.AllRights);
            //else
            //    AccessModule(ModuleValues.PromoCode);

            var promoCode = new PromoCode(id);
            GF_PromoCode obj = promoCode.GetPromoCodeByID(id);

            ViewBag.DiscountType = new SelectList(DiscountType.GetDiscountType(), "Tag", "Name", obj.DiscountType);
            ViewBag.MenuItemList = new SelectList(promoCode.GetMenuItemList(), "ID", "Name", obj.ReferenceID);
            ViewBag.IsLoginUser = (id != 0 && id == LoginInfo.UserId);

            return View(obj);
        }

        /// <summary>
        /// Created By: Amit Kumar
        /// Creation On: 01 April 2015
        /// Description: method to get add/edit
        /// </summary>
        [HttpPost]
        [SessionExpireFilterAttribute]
        public ActionResult PromoCodeAddEdit(string eid, GF_PromoCode obj)
        {

            long id = CommonFunctions.DecryptUrlParam(eid);

            AccessModule(ModuleValues.PromoCode);

            var isSaved = false;
            var promoCode = new PromoCode(id);

            //if (ModelState.IsValid)
            //{
            isSaved = promoCode.Save(obj);
            //}
            if (isSaved)
            {
                string module = "Promo Code";
                TempData["Message"] = CommonFunctions.Message(promoCode.Message, module);
                return RedirectToAction("PromoCodeList");
            }

            ViewBag.ErrorMessage = promoCode.Message;

            ViewBag.DiscountType = new SelectList(DiscountType.GetDiscountType(), "Tag", "Name", obj.DiscountType);
            ViewBag.MenuItemList = new SelectList(promoCode.GetMenuItemList(), "ID", "Name", obj.ReferenceID ?? 0);
            ViewBag.IsLoginUser = (id != 0 && id == LoginInfo.UserId);

            return View(obj);
        }

        /// <summary>
        /// Created By: Amit Kumar
        /// Creation On: 01 April 2015
        /// Description: method to active/inactive
        /// </summary>
        [AcceptVerbs(HttpVerbs.Post)]
        [SessionExpireFilterAttribute]
        public ActionResult UpdatePromoCodeStatus(long id, bool status)
        {
            AccessModule(ModuleValues.PromoCode);

            var promoCode = new PromoCode(id);
            return promoCode.ChangeStatus(status)
                       ? Json(new { statusText = "success", module = "Promo Code", task = "update" })
                       : Json(new { statusText = "error", module = "Promo Code", task = "update", message = promoCode.Message });
        }

        /// <summary>
        /// Created By: Amit Kumar
        /// Creation On: 01 April 2015
        /// Description: method to delete
        /// </summary>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult DeletePromoCode(List<long> ids)
        {
            try
            {
                AccessModule(ModuleValues.PromoCode);

                if (ids != null)
                {
                    var promoCode = new PromoCode();
                    return promoCode.DeletePromoCodes(ids)
                               ? Json(new { statusText = "success", module = "Promo Code", task = "delete", errormessage = promoCode.Message })
                               : Json(new { statusText = "error", module = "Promo Code", task = "delete", message = promoCode.Message });
                }
                return Json(new { statusText = "error", module = "Promo Code", task = "select" });
            }
            catch (Exception ex)
            {
                ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
                return null;
            }

        }

        [HttpGet]
        public ActionResult GetFoodItem(string type)
        {
            AccessModule(ModuleValues.ManageFoodItems);
            PromoCode obj = new PromoCode();

            if (type == PromoCodeType.CategoryWise)
            {
                var category = obj.GetCategoryList().ToList().Select(x => new { x.ID, x.Name });
                return Json(new { msg = "Success", data = category }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var menuItems = obj.GetMenuItemList().ToList().Select(x => new { x.ID, x.Name });
                return Json(new { msg = "Success", data = menuItems }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Created By: Amit Kumar
        /// Creation On: 23 July 2015
        /// Description: Issue promo code
        /// </summary>
        [HttpPost]
        [SessionExpireFilterAttribute]
        public ActionResult IssuePromoCode(string eid, GF_PromoCode obj)
        {
            AccessModule(ModuleValues.PromoCode);

            var isSaved = false;
            var promoCode = new PromoCode();

            isSaved = promoCode.IssuePromoCode(obj);

            var order = Db.GF_Order.FirstOrDefault(x => x.ID == obj.OrderID);
            string golferID = "0";
            if (order != null)
            {
                golferID = CommonFunctions.EncryptUrlParam(order.GolferID ?? 0);
            }

            if (isSaved)
            {
                string module = "Promo Code";
                TempData["Message"] = "Promo Code has been issued.";
                //return RedirectToAction("OrderListByGolfer");
                return Redirect("OrderListByGolfer?eid=" + golferID);
            }

            ViewBag.ErrorMessage = promoCode.Message;

            return Redirect("OrderListByGolfer?eid=" + golferID);
        }

        #endregion

        #region Manage Gopher View


        public void CheckIdealUsers()
        {
            try
            {
                bool isAutoLogout = false;
                if (Session["AutoLogoutStartTime"] == null)
                {
                    Session["AutoLogoutStartTime"] = DateTime.UtcNow;
                    isAutoLogout = true;
                }
                else
                {
                    try
                    {
                        DateTime startCheck = Convert.ToDateTime(Session["AutoLogoutStartTime"]);
                        DateTime endCheck = DateTime.UtcNow;

                        int checkMinutes = 0;
                        try
                        {
                            checkMinutes = Convert.ToInt32(System.Web.Configuration.WebConfigurationManager.AppSettings["UserAutoLogoutSchedularMinutes"]);
                        }
                        catch
                        {
                            checkMinutes = 0;
                        }

                        int mins = Convert.ToInt32((Convert.ToDateTime(endCheck) - Convert.ToDateTime(startCheck)).TotalMinutes);
                        Int64 chekMins = Convert.ToInt64(checkMinutes);

                        if (chekMins > 0)
                        {
                            if (mins >= chekMins)
                            {
                                isAutoLogout = true;
                            }
                        }
                    }
                    catch
                    {

                    }
                }

                if (isAutoLogout)
                {
                    #region Golfer Logout Status check
                    try
                    {
                        CommonFunctions.GolferAutoLogout(LoginInfo.CourseId);

                    }
                    catch (Exception ex)
                    {
                        ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
                    }
                    #endregion
                    #region Course User Logout Status check
                    try
                    {

                        CommonFunctions.CourseUserAutoLogout(LoginInfo.CourseId);
                    }
                    catch (Exception ex)
                    {
                        ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
                    }
                    #endregion
                }
            }
            catch
            {

            }
        }

        /// <summary>
        /// Created By: Veera
        /// Creation On: 1 April 2015
        /// Description: method for list
        /// </summary>
        [SessionExpireFilterAttribute]
        public ActionResult ViewGolfersOnMap()
        {
            Session["AutoLogoutStartTime"] = null;
            CheckIdealUsers();
            AccessModule(ModuleValues.GopherView);

            //if (Golfler.Models.LoginInfo.Type == Golfler.Models.UserType.Ranger)
            //{
            //    AccessModule(ModuleValues.AllRights);
            //}
            //else
            //{
            //    AccessModule(ModuleValues.GopherView);
            //}

            ViewBag.Message = TempData["Message"];
            ViewBag.courseLoginId = LoginInfo.UserId;
            ViewBag.CourseLoginName = LoginInfo.FirstName + " " + LoginInfo.LastName;
            GF_Golfer obj = new GF_Golfer();
            var totalRecords = 0;

            var list = obj.GetGolferByCourseIdList(Convert.ToString(LoginInfo.CourseId), "", "", "", 0, 0, ref totalRecords);
            string jsonstring = "";
            if (list != null)
            {
                foreach (var item in list)
                //  var tempList = list;
                // for (int i = 0; i > tempList.Count(); i++)
                {
                    string CurrentHole = "";
                    string TotalTimeSpend = "";
                    string CurrentHoleTime = "";
                    string AverageTime = "";
                    string Round = "";
                    Int64 CurrentHoleTimeInMins = 0;
                    Dictionary<string, string> HoleTimings = new Dictionary<string, string>();
                    CommonFunctions.GetGolferHoleInformation(Convert.ToInt64(LoginInfo.CourseId), Convert.ToInt64(item.Id), ref CurrentHole, ref CurrentHoleTime, ref CurrentHoleTimeInMins, ref TotalTimeSpend, ref HoleTimings, ref Round, ref AverageTime);

                    if (Round == "No round started yet.")
                    {
                        if (CurrentHole == "" || CurrentHole == "0")
                        {
                            CurrentHole = "Hole: " + "n.a." + " (No round started yet)";
                        }
                        else
                        {
                            CurrentHole = "Hole: " + CurrentHole + " (No round started yet)";
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
                        if (CurrentHole == "")
                        {
                            CurrentHole = "Hole: ";
                            CurrentHole = CurrentHole + "n.a.";
                        }
                        else
                        {
                            CurrentHole = "Hole: " + CurrentHole;
                            // CurrentHole = CurrentHole + ", Time: " + CurrentHoleTime;
                        }
                        if (CurrentHoleTime == "")
                        {
                            TotalTimeSpend = "Current Hole Time: " + "n.a.";
                        }
                        else
                        {
                            TotalTimeSpend = "Current Hole Time: " + CurrentHoleTime;
                        }
                    }

                    string strNewType = "blue";
                    if (CurrentHole.Contains("No round started yet"))
                    {
                        strNewType = "blue";
                    }
                    else
                    {
                        if (CurrentHoleTimeInMins > 20)
                        {
                            strNewType = "red";
                        }
                        else
                        {
                            if (CurrentHoleTimeInMins > 15)
                            {
                                strNewType = "orange";
                            }
                            else
                            {
                                strNewType = "green";
                            }
                        }
                    }

                    string title = item.Admin_Name.Replace("\"", "") + "<br>" + CurrentHole + "<br>" + TotalTimeSpend;

                    if (jsonstring == "")
                    {
                        jsonstring = "{\"golfers\":[{\"Title\": \"" + title + "\",\"Id\": " + item.Id + ", \"Name\": " + item.Admin_Name.Replace(" ", "") + ", \"lat\": " + item.Admin_Latitude + ", \"lng\": " + item.Admin_Longitude + ", \"type\": " + strNewType + " }";
                    }
                    else
                    {
                        jsonstring = jsonstring + ",{\"Title\": \"" + title + "\",\"Id\": " + item.Id + ", \"Name\": " + item.Admin_Name.Replace(" ", "") + ", \"lat\": " + item.Admin_Latitude + ", \"lng\": " + item.Admin_Longitude + ", \"type\": " + strNewType + " }";

                    }



                    //if (jsonstring == "")
                    //{
                    //    jsonstring = "{\"golfers\":[{\"Id\": " + item.Id + ", \"Name\": " + item.Admin_Name.Replace(" ", "") + ",\"title\": " + title + ", \"lat\": " + item.Admin_Latitude + ", \"lng\": " + item.Admin_Longitude + ", \"type\": " + item.Admin_type + "}";
                    //}
                    //else
                    //{
                    //    jsonstring = jsonstring + ",{\"Id\": " + item.Id + ", \"Name\": " + item.Admin_Name.Replace(" ", "") + ",\"title\": " + title + ", \"lat\": " + item.Admin_Latitude + ", \"lng\": " + item.Admin_Longitude + ", \"type\": " + item.Admin_type + "}";

                    //}
                }
                jsonstring = jsonstring + "]}";
                ViewBag.viewGolferLength = list.Count();
            }
            else
            {
                ViewBag.viewGolferLength = 0;
            }
            ViewBag.viewEmployee = jsonstring;
            ViewBag.CourseLogin = LoginInfo.FirstName + " " + LoginInfo.LastName;
            //
            var course = Db.GF_CourseInfo.Where(x => x.ID == LoginInfo.CourseId);
            string CourseLatitude = "";
            string CourseLongitude = "";
            if (course != null)
            {
                foreach (var item in course)
                {
                    CourseLatitude = item.LATITUDE;
                    CourseLongitude = item.LONGITUDE;


                }
            }
            ViewBag.CourseLatitude = CourseLatitude;
            ViewBag.CourseLongitude = CourseLongitude;

            //
            return View();
        }
        public ActionResult GetCourseHoleDetails()
        {
            #region Course Holes Detail

            long cID = Convert.ToInt64(LoginInfo.CourseUserId);
            var courseBuilderID = Db.GF_CourseBuilder.FirstOrDefault(x => x.CourseID == cID && x.CoordinateType == "O");

            var courseRec = Db.GF_CourseBuilderRecDates.FirstOrDefault(x => x.RecDate == DateTime.Now);

            var lstCourseHole = courseBuilderID == null ? null :
                Db.GF_CourseBuilderHolesDetail.ToList().Where(x => x.CourseBuilderID == (courseRec != null ? courseRec.CourseBuilderId : courseBuilderID.ID))
                                               .Select(x => new
                                               {
                                                   HoleNumber = "Hole Number: " + Convert.ToString(x.HoleNumber),
                                                   x.Latitude,
                                                   x.Longitude,
                                                   DragItemType = DragItemType.GetFullDragItemType(x.DragItemType),
                                                   HoleNo = Convert.ToString(x.HoleNumber)
                                               });
            var jsonData = new { rows = lstCourseHole };
            return Json(jsonData, JsonRequestBehavior.AllowGet);


            #endregion
        }
        /// <summary>
        /// Created By:
        /// Creation On:
        /// Description: method to bind grid
        /// </summary>
        [SessionExpireFilterAttribute]
        public ActionResult GetGophieViewList(string searchText, string sidx, string sord, int? page, int? rows)
        {
            try
            {
                GolflerEntities Db = new GolflerEntities();
                GF_Golfer obj = new GF_Golfer();

                //var rec = MsgObj.GetMessagesSent(searchText, sidx,
                //                           sord, page ?? 1, rows ?? 10,
                //                           ref totalRecords).AsEnumerable().Select((x =>
                //                                                     new
                //                                                     {
                //                                                         x.ID,
                //                                                         Name = (Db.GF_Golfer.Where(y => y.GF_ID == x.MsgTo).FirstOrDefault().FirstName + " " + (Db.GF_Golfer.Where(y => y.GF_ID == x.MsgTo).FirstOrDefault().LastName)),
                //                                                         x.CreatedDate,
                //                                                         x.Message,
                //                                                         EID = CommonFunctions.EncryptUrlParam(x.ID)
                //                                                     }
                //     
                //                                        ));
                var totalRecords = 0;
                var list = obj.GetGolferByCourseIdList(Convert.ToString(LoginInfo.CourseId), searchText, sidx, sord, page ?? 1, rows ?? 10, ref totalRecords);

                if (list != null)
                {
                    var jsonData = new
                    {
                        total = (totalRecords + rows - 1) / rows,
                        page,
                        records = totalRecords,
                        rows = list.ToList(),
                        Id = "Id"
                    };
                    return Json(jsonData, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
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
            string data = "MsgFrom=" + msgfrom + "&MsgToList=" + msgto + "&Message=" + msg + "&IsMessagesFromGolfer=0&IsMessagesToGolfer=1";
            MyWebRequest myRequest = new MyWebRequest(url, "POST", data);
            var jsonString = myRequest.GetResponse();
            return Json(new { result = true });
        }
        [HttpPost]
        public ActionResult GetGolferMessages(string id, string PgNo, string MsgTo)
        {
            AccessModule(ModuleValues.GopherView);
            Course objCourse = new Course();
            int totalrecords = 0;
            int PgSize = ConfigClass.MessageListingPageSize;
            if (id.Contains("chat_"))
            {
                id = id.Replace("chat_", "");
            }
            var list = objCourse.GetMsgsfromGolfer(Convert.ToInt64(id), LoginInfo.UserId, Convert.ToInt32(PgNo), PgSize, ref totalrecords, MsgTo, false, "G", false);

            var jsonData = new
            {
                pages = Math.Ceiling(Convert.ToDecimal(totalrecords / PgSize)),
                rows = list
            };

            return Json(jsonData, JsonRequestBehavior.AllowGet);
            //string url = ConfigClass.GolferApiService + GolferApiName.MsgGolfers + "GetMessageListing";
            //string data = "PgNo=" + PgNo + "&MsgFrom=" + Convert.ToInt64(id) + "&MsgTo=" + LoginInfo.UserId + "&Timezone=0&Offset=1:30&IsMessagesFromGolfer=1";
            //MyWebRequest myRequest = new MyWebRequest(url, "POST", data);
            //var jsonString = myRequest.GetResponse();
            //return Json(jsonString, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Created By: Veera
        /// Creation On: 20 May 2015
        /// Description: method for Refresh Golfers On Map
        /// </summary>
        [SessionExpireFilterAttribute]
        public ActionResult RefreshGolfersOnMap()
        {
            CheckIdealUsers();

            ViewBag.courseLoginId = LoginInfo.UserId;
            ViewBag.CourseLoginName = LoginInfo.FirstName;
            GF_Golfer obj = new GF_Golfer();
            var totalRecords = 0;
            var list = obj.GetGolferByCourseIdList(Convert.ToString(LoginInfo.CourseId), "", "", "", 0, 0, ref totalRecords);
            string jsonstring = "";
            Int64 resultcount = 0;
            #region Golfer
            if (list != null)
            {
                foreach (var item in list)
                {
                    string CurrentHole = "";
                    string TotalTimeSpend = "";
                    string CurrentHoleTime = "";
                    string AverageTime = "";
                    string Round = "";
                    Int64 CurrentHoleTimeInMins = 0;
                    Dictionary<string, string> HoleTimings = new Dictionary<string, string>();

                    //#region Golfer Logout Status check
                    //try
                    //{
                    //    CommonFunctions.GolferAutoLogout(LoginInfo.CourseId);
                    //}
                    //catch (Exception ex)
                    //{
                    //    ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
                    //} 

                    //#endregion

                    CommonFunctions.GetGolferHoleInformation(Convert.ToInt64(LoginInfo.CourseId), Convert.ToInt64(item.Id), ref CurrentHole, ref CurrentHoleTime, ref CurrentHoleTimeInMins, ref TotalTimeSpend, ref HoleTimings, ref Round, ref AverageTime);

                    if (Round == "No round started yet.")
                    {
                        if (CurrentHole == "" || CurrentHole == "0")
                        {
                            CurrentHole = "Hole: " + "n.a." + " (No round started yet)";
                        }
                        else
                        {
                            CurrentHole = "Hole: " + CurrentHole + " (No round started yet)";
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
                        if (CurrentHole == "")
                        {
                            CurrentHole = "Hole: ";
                            CurrentHole = CurrentHole + "n.a.";
                        }
                        else
                        {
                            CurrentHole = "Hole: " + CurrentHole;
                            // CurrentHole = CurrentHole + ", Time: " + CurrentHoleTime;
                        }
                        //if (AverageTime == "")
                        //{
                        //    TotalTimeSpend = "Average Time: " + "n.a.";
                        //}
                        //else
                        //{
                        //    TotalTimeSpend = "Average Time: " + AverageTime;
                        //}

                        if (CurrentHoleTime == "")
                        {
                            TotalTimeSpend = "Current Hole Time: " + "n.a.";
                        }
                        else
                        {
                            TotalTimeSpend = "Current Hole Time: " + CurrentHoleTime;
                        }
                    }

                    string strNewType = "blue";

                    if (CurrentHole.Contains("No round started yet"))
                    {
                        strNewType = "blue";
                    }
                    else
                    {
                        if (CurrentHoleTimeInMins > 20)
                        {
                            strNewType = "red";
                        }
                        else
                        {
                            if (CurrentHoleTimeInMins > 15)
                            {
                                strNewType = "orange";
                            }
                            else
                            {
                                strNewType = "green";
                            }
                        }
                    }


                    string title = item.Admin_Name.Replace("\"", "") + "<br>" + CurrentHole + "<br>" + TotalTimeSpend;

                    if (jsonstring == "")
                    {
                        jsonstring = "{\"golfers\":[{\"Title\": \"" + title + "\",\"Id\": " + item.Id + ", \"Name\": " + item.Admin_Name.Replace(" ", "") + ", \"lat\": " + item.Admin_Latitude + ", \"lng\": " + item.Admin_Longitude + ", \"type\": \"" + strNewType + "\" }";
                    }
                    else
                    {
                        jsonstring = jsonstring + ",{\"Title\": \"" + title + "\",\"Id\": " + item.Id + ", \"Name\": " + item.Admin_Name.Replace(" ", "") + ", \"lat\": " + item.Admin_Latitude + ", \"lng\": " + item.Admin_Longitude + ", \"type\": \"" + strNewType + "\" }";

                    }



                }
                //  jsonstring = jsonstring + "]}";
                // ViewBag.viewGolferLengthRefresh = list.Count();
            }
            else
            {
                //  ViewBag.viewGolferLengthRefresh = 0;
            }
            #endregion

            #region Course Users
            Order ob = new Order();
            var activeUser = ob.getActiveCourseUser();

            if (activeUser.Count > 0)
            {
                var objAdminUser = CommonFunctions.GetCourseAdmin();
                foreach (var item in activeUser)
                {
                    if (item.Id != objAdminUser.ID)
                    {
                        string title = item.Name.Replace("\"", "");

                        if (jsonstring == "")
                        {
                            jsonstring = "{\"golfers\":[{\"Title\": \"" + title + "\",\"Id\": " + item.Id + ", \"Name\": \"" + item.Name.Replace(" ", "") + "\", \"lat\": \"" + item.Latitude + "\", \"lng\": \"" + item.Longitude + "\", \"type\": \"" + item.UserType + "\" }";
                        }
                        else
                        {
                            jsonstring = jsonstring + ",{\"Title\": \"" + title + "\",\"Id\": " + item.Id + ", \"Name\": \"" + item.Name.Replace(" ", "") + "\", \"lat\": \"" + item.Latitude + "\", \"lng\": \"" + item.Longitude + "\", \"type\": \"" + item.UserType + "\" }";
                        }
                    }
                }
                //ViewBag.viewGolferLengthRefresh = list.Count();
            }
            else
            {
                //ViewBag.viewGolferLengthRefresh = 0;
            }
            #endregion

            Int64 golfercount = 0;
            if (list != null)
            {
                golfercount = list.Count();
            }

            Int64 usercount = 0;
            if (activeUser != null)
            {
                usercount = activeUser.Count();
            }
            resultcount = golfercount + usercount;

            if (resultcount > 0)
            {
                jsonstring = jsonstring + "]}";
            }
            ViewBag.viewGolferLengthRefresh = resultcount;
            ViewBag.viewEmployeeRefresh = jsonstring;
            ViewBag.CourseLogin = LoginInfo.FirstName;
            var jsonData = new
            {

                status = "true"
            };
            //
            var course = Db.GF_CourseInfo.Where(x => x.ID == LoginInfo.CourseId);
            string CourseLatitude = "";
            string CourseLongitude = "";
            if (course != null)
            {
                foreach (var item in course)
                {
                    CourseLatitude = item.LATITUDE;
                    CourseLongitude = item.LONGITUDE;
                }
            }
            ViewBag.CourseLatitude = CourseLatitude;
            ViewBag.CourseLongitude = CourseLongitude;
            //

            //Int32 intjlength = 0;
            //if (list != null)
            //{
            //    intjlength = list.Count();
            //}
            return Json(new { jsonData = jsonData, jsonstring = jsonstring, jlength = resultcount }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Manage Course Builder

        [HttpGet]
        [SessionExpireFilterAttribute]
        public ActionResult CourseBuilder()
        {
            var courseID = SelectedCourseID == 0 ? LoginInfo.CourseId : SelectedCourseID;

            AccessModule(ModuleValues.CourseBuilder);
            var courseLocation = Db.GF_CourseInfo.FirstOrDefault(x => x.ID == courseID);//LoginInfo.CourseId);
            if (!(string.IsNullOrEmpty(Convert.ToString(courseLocation.LATITUDE))))
            {
                ViewBag.Lattitude = courseLocation.LATITUDE;
            }
            else
            {
                ViewBag.Lattitude = "";
            }
            if (!(string.IsNullOrEmpty(Convert.ToString(courseLocation.LONGITUDE))))
            {
                ViewBag.Longitude = courseLocation.LONGITUDE;
            }
            else
            {
                ViewBag.Longitude = "";
            }

            ViewBag.CourseId = courseID;// LoginInfo.CourseId;
            ViewBag.Status = new SelectList(ApproveStatusType.GetStatusTypeList(), "Tag", "Name", "P");

            string RecDateMsg = "";
            string RecDates = "";
            string RecDescripton = "";
            string coursebuilderid = "";
            if (!(string.IsNullOrEmpty(Convert.ToString(Session["RecurringTempMsg"]))))
            {
                RecDateMsg = Convert.ToString(Session["RecurringTempMsg"]);
            }

            if (!(string.IsNullOrEmpty(Convert.ToString(Session["RecurringTempDates"]))))
            {
                Session["RecurringTempDatesForSubmit"] = Session["RecurringTempDates"];
                RecDates = "Dates in session";
            }

            if (!(string.IsNullOrEmpty(Convert.ToString(Session["RecurringDescriptionForSubmit"]))))
            {
                RecDescripton = Convert.ToString(Session["RecurringDescriptionForSubmit"]);
            }
            if (!(string.IsNullOrEmpty(Convert.ToString(Session["RecurringCourseBuilderId"]))))
            {
                coursebuilderid = Convert.ToString(Session["RecurringCourseBuilderId"]);
            }

            Session["RecurringTempMsg"] = null;
            Session["RecurringTempDates"] = null;
            Session["RecurringCourseBuilderId"] = null;

            if (RecDateMsg.Length > 0)
            {
                ViewBag.RecDateMsg = RecDateMsg;
            }
            else
            {
                ViewBag.RecDateMsg = "";
            }
            if (RecDates.Length > 0)
            {
                ViewBag.RecDates = RecDates;
            }
            else
            {
                ViewBag.RecDates = "";
            }
            if (RecDescripton.Length > 0)
            {
                ViewBag.RecDescripton = RecDescripton;
            }
            else
            {
                ViewBag.RecDescripton = "";
            }
            if (coursebuilderid.Length > 0)
            {
                bool isOrg = false;
                // CHECK WHETHER coursebuilderid is Original Or Not
                Db = new GolflerEntities();
                var OrgCoOrdinate = Db.GF_CourseBuilder.Where(x => x.CoordinateType == "O" && x.CourseID == courseID &&//LoginInfo.CourseId &&
                    x.Status == StatusType.Active && x.BuildBy == "CA").FirstOrDefault();
                if (OrgCoOrdinate != null)
                {
                    if (OrgCoOrdinate.ID == Convert.ToInt64(coursebuilderid))
                    {
                        isOrg = true;
                    }
                }
                if (!isOrg)
                {
                    ViewBag.CourseBuilderId = coursebuilderid;
                }
                else
                {
                    ViewBag.CourseBuilderId = "0000";
                }
            }
            else
            {
                ViewBag.CourseBuilderId = "0";
            }


            return View();
        }

        [HttpPost]
        [SessionExpireFilterAttribute]
        public ActionResult CheckCourseBuildTitle(string title, string CourseBuilderId = "0")
        {
            var courseID = SelectedCourseID == 0 ? LoginInfo.CourseId : SelectedCourseID;
            bool boolResult = false;
            string Message = "";
            try
            {
                Int64 coursebuildid = 0;
                var chkTitle = new List<GF_CourseBuilder>();
                Db = new GolflerEntities();
                try
                {
                    coursebuildid = Convert.ToInt64(CourseBuilderId);
                }
                catch
                {
                    coursebuildid = 0;
                }
                if (coursebuildid > 0)
                {
                    // update case

                    chkTitle = Db.GF_CourseBuilder.Where(x => x.CourseID == courseID &&//LoginInfo.CourseId &&
                        x.Status != StatusType.Delete && x.Title == title && x.ID != coursebuildid).ToList();

                }
                else
                {
                    // new title
                    chkTitle = Db.GF_CourseBuilder.Where(x => x.CourseID == courseID && //LoginInfo.CourseId &&
                        x.Status != StatusType.Delete && x.Title == title).ToList();
                }
                if (chkTitle.Count > 0)
                {
                    boolResult = false;
                    Message = "Title already exists.";
                }
                else
                {
                    boolResult = true;
                }
            }
            catch (Exception ex)
            {
                boolResult = false;
                ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
                Message = ex.InnerException.ToString();
            }
            return Json(new { result = boolResult, Message = Message });
        }

        [HttpPost]
        [SessionExpireFilterAttribute]
        public ActionResult CheckCourseBuildOrignal()
        {
            var courseID = SelectedCourseID == 0 ? LoginInfo.CourseId : SelectedCourseID;
            bool boolResult = false;
            string Message = "";
            try
            {
                Db = new GolflerEntities();
                var chkOrg = Db.GF_CourseBuilder.Where(x => x.CourseID == courseID &&//LoginInfo.CourseId &&
                    x.Status != StatusType.Delete &&
                    x.CoordinateType == CoordinateType.LoadOriginal).ToList();

                if (chkOrg.Count > 0)
                {
                    boolResult = true;
                }
            }
            catch (Exception ex)
            {
                boolResult = false;
                ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
                Message = ex.InnerException.ToString();
            }
            return Json(new { result = boolResult });
        }

        [HttpPost]
        [SessionExpireFilterAttribute]
        public ActionResult CourseBuilder(string buildBy, Int64 courseid, string golferComments, string title,
            string coordinateType, string dateFrom, string dateTo, string CourseBuilderId, string status, string holesDetail, string callfrom)
        {
            bool boolResult = false;
            string Message = "";
            Int64 intNewCourseBuilderId = 0;
            try
            {
                //if (coordinateType.Contains(Golfler.Models.CoordinateType.SetTemporary))
                //{

                //}
                //if (!(string.IsNullOrEmpty(Convert.ToString(Session["RecurringTempDatesForSubmit"]))))
                //{

                //}
                //else
                //{
                //    boolResult = false;
                //    Message = "Recurring dates are not available.";
                //}
                GF_CourseBuilder obj = new GF_CourseBuilder();
                List<courseHoleDetail> objHoleDetail = (List<courseHoleDetail>)JsonConvert.DeserializeObject(holesDetail, typeof(List<courseHoleDetail>));


                string msg = "";

                var isSave = obj.SaveCourseBuilder(buildBy, courseid, golferComments, title, coordinateType, dateFrom, dateTo, CourseBuilderId, status, objHoleDetail, callfrom, ref msg, ref intNewCourseBuilderId);
                boolResult = true;
                Message = msg;

            }
            catch (Exception ex)
            {
                boolResult = false;
                ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
                Message = ex.InnerException.ToString();
            }

            return Json(new { result = boolResult, Message = Message, intNewCourseBuilderId = intNewCourseBuilderId });

            //var courseLocation = Db.GF_CourseInfo.FirstOrDefault(x => x.ID == courseid);
            //if (!(string.IsNullOrEmpty(Convert.ToString(courseLocation.LATITUDE))))
            //{
            //    ViewBag.Lattitude = courseLocation.LATITUDE;
            //}
            //else
            //{
            //    ViewBag.Lattitude = "";
            //}
            //if (!(string.IsNullOrEmpty(Convert.ToString(courseLocation.LONGITUDE))))
            //{
            //    ViewBag.Longitude = courseLocation.LONGITUDE;
            //}
            //else
            //{
            //    ViewBag.Longitude = "";
            //}

            //ViewBag.CourseId = courseid;
            //ViewBag.RecDateMsg = "";
            //ViewBag.RecDates = "";


        }

        [HttpGet]
        [SessionExpireFilterAttribute]
        public ActionResult CheckRecurringDates(string RecurringDates = "", string RecDateDescription = "",
            string courseBuilderId = "0", string coursebuildertitle = "")
        {
            var courseID = SelectedCourseID == 0 ? LoginInfo.CourseId : SelectedCourseID;
            Int64 intCourseBuilderId = 0;

            //if (Session["sessionRecurringDates"] != null)
            //{
            //    RecurringDates = Convert.ToString(Session["sessionRecurringDates"]);
            //}
            //if (Session["sessionRecDateDescription"] != null)
            //{
            //    RecDateDescription = Convert.ToString(Session["sessionRecDateDescription"]);
            //}
            //if (Session["sessioncoursebuildertitle"] != null)
            //{
            //    coursebuildertitle = Convert.ToString(Session["sessioncoursebuildertitle"]);
            //}

            string Message = "";
            try
            {

                if (RecurringDates != "Close")
                {
                    Session["RecurringTempMsg"] = null;
                    Session["RecurringTempDates"] = null;
                    Session["RecurringTempDatesForSubmit"] = null;
                    Session["RecurringDescriptionForSubmit"] = null;
                    Session["RecurringCourseBuilderId"] = null;
                    Session["coursebuildertitle"] = null;
                    Db = new GolflerEntities();


                    #region Check CourseBuilderID
                    if (!string.IsNullOrEmpty(Convert.ToString(courseBuilderId)))
                    {
                        if (Convert.ToString(courseBuilderId) != "0")
                        {
                            try
                            {
                                intCourseBuilderId = Convert.ToInt64(courseBuilderId);
                            }
                            catch
                            {
                                intCourseBuilderId = 0;
                            }
                        }
                    }



                    // CHECK WHETHER coursebuilderid is Original Or Not
                    bool isOrg = false;
                    if (intCourseBuilderId > 0)
                    {
                        var OrgCoOrdinate = Db.GF_CourseBuilder.Where(x => x.CoordinateType == "O" && x.CourseID == courseID &&//LoginInfo.CourseId &&
                            x.Status == StatusType.Active && x.BuildBy == "CA").FirstOrDefault();
                        if (OrgCoOrdinate != null)
                        {
                            if (OrgCoOrdinate.ID == Convert.ToInt64(intCourseBuilderId))
                            {
                                isOrg = true;
                            }
                        }
                    }
                    #endregion


                    // Check if dates already exists in database.
                    List<string> lstDates = RecurringDates.Split(',').ToList();

                    if (lstDates.Count > 0)
                    {
                        List<DateTime?> lstRecDates = new List<DateTime?>();
                        foreach (var date in lstDates)
                        {
                            lstRecDates.Add(Convert.ToDateTime(date.Replace("_", "/")));
                        }

                        var existDates = new List<GF_CourseBuilderRecDates>();

                        if (intCourseBuilderId > 0)
                        {
                            existDates = Db.GF_CourseBuilderRecDates.Where(x => lstRecDates.Contains(x.RecDate) && x.Status == StatusType.Active &&
                                x.CourseId == courseID &&//LoginInfo.CourseId &&
                                x.CourseBuilderId != intCourseBuilderId).ToList();
                        }
                        else
                        {
                            existDates = Db.GF_CourseBuilderRecDates.Where(x => lstRecDates.Contains(x.RecDate) && x.Status == StatusType.Active &&
                                x.CourseId == courseID//LoginInfo.CourseId
                                ).ToList();
                        }

                        if (existDates.Count <= 0) // means not exists
                        {
                            Session["RecurringTempDates"] = lstRecDates;
                            Session["RecurringTempDatesForSubmit"] = lstRecDates;
                            RecDateDescription = RecDateDescription.Replace("_", "/");
                            Session["RecurringDescriptionForSubmit"] = RecDateDescription;

                            if (!isOrg)
                            {
                                GF_CourseBuilder objCourseBuilder = new GF_CourseBuilder();
                                objCourseBuilder.UpdateRecDates(coursebuildertitle, ref intCourseBuilderId);
                            }
                        }
                        else
                        {
                            // duplicate dates found
                            Message = "You can not proceed because following dates are already occupied:\n";
                            foreach (var dat in existDates)
                            {
                                Message += "\n" + dat.RecDate.Value.Day + "/" + dat.RecDate.Value.Month + "/" + dat.RecDate.Value.Year;
                            }
                        }
                    }
                    else
                    {
                        Message = "Please select dates.";
                    }
                }
                else
                {
                    Message = "";
                }
            }
            catch (Exception ex)
            {
                Message = ex.Message;
            }

            Session["coursebuildertitle"] = coursebuildertitle;
            Session["RecurringCourseBuilderId"] = intCourseBuilderId;
            Session["RecurringTempMsg"] = Message;
            return RedirectToAction("CourseBuilder");
        }

        [HttpGet]
        [SessionExpireFilterAttribute]
        public ActionResult GetCourseBuilder(string id, string strCourseId, string holenumber, string coordinatetype)
        {
            AccessModule(ModuleValues.CourseBuilder);
            try
            {
                if (coordinatetype == "onlyCourseName")
                {
                    string coursename = "-";
                    try
                    {
                        var DbEntity = new GolflerEntities();
                        Int64 intCid = Convert.ToInt64(strCourseId);
                        coursename = Convert.ToString(DbEntity.GF_CourseInfo.FirstOrDefault(x => x.ID == intCid).COURSE_NAME);

                        return Json(new { coursename = coursename }, JsonRequestBehavior.AllowGet);
                    }
                    catch (Exception ex)
                    {
                        ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
                        return Json(new { coursename = "-" }, JsonRequestBehavior.AllowGet);
                    }

                }
                else
                {
                    if (coordinatetype.ToString().ToLower().Contains("new"))
                    {
                        Session["RecurringTempMsg"] = null;
                        Session["RecurringTempDates"] = null;
                        Session["RecurringTempDatesForSubmit"] = null;
                        Session["RecurringDescriptionForSubmit"] = null;
                        Session["RecurringCourseBuilderId"] = null;
                        Session["coursebuildertitle"] = null;
                    }

                    Int16 intholenumber = 0;
                    try
                    {
                        intholenumber = Convert.ToInt16(holenumber);
                    }
                    catch { intholenumber = 0; }

                    Int64 courseid = Convert.ToInt64(strCourseId);
                    long cid = 0;

                    try
                    {
                        cid = Convert.ToInt32(id);
                    }
                    catch
                    {
                        cid = 0;
                    }

                    if (cid == 0)
                    {
                        if (coordinatetype == "org") // first check
                        {
                            var lst = Db.GF_CourseBuilder.FirstOrDefault(x => (x.CourseID ?? 0) == courseid &&
                                x.CoordinateType == CoordinateType.LoadOriginal && x.Status == StatusType.Active);

                            if (lst != null)
                                cid = lst.ID;
                        }
                        if (coordinatetype == "GolferCoordinate") // second check // if coordinatetype == "GolferCoordinate" than it will overright cid
                        {
                            Int64 golferid = 0;
                            try
                            {
                                if (!string.IsNullOrEmpty(Convert.ToString(LoginInfo.GolferUserId)))
                                {
                                    golferid = LoginInfo.GolferUserId;
                                }
                                else
                                {
                                    golferid = 0;
                                }
                            }
                            catch { golferid = 0; }

                            var lst = Db.GF_CourseBuilder.FirstOrDefault(x => (x.CourseID ?? 0) == courseid &&
                                x.BuildBy == UserType.Golfer && x.CreatedBy == golferid);

                            if (lst != null)
                                cid = lst.ID;
                        }
                    }

                    IEnumerable<courseBuilder> lstCourse;
                    GF_CourseBuilder obj = new GF_CourseBuilder();
                    lstCourse = obj.getCourseBuilder(cid, courseid, intholenumber, coordinatetype);

                    return Json(new { data = lstCourse, result = true, Message = "Success" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
                return Json(new { data = "", result = false, Message = ex.InnerException.ToString() }, JsonRequestBehavior.AllowGet);
            }

        }

        /// <summary>
        /// Created By: Amit Kumar
        /// Creation On: 28 March 2015
        /// Description: method to bind grid
        /// </summary>
        [SessionExpireFilterAttribute]
        public ActionResult GetCourseBuilderList(string searchText, Int64 CourseId, string buildby, string sidx, string sord, int? page, int? rows)
        {
            try
            {
                // Int64 courseid = LoginInfo.CourseId;
                var CourseObj = new GF_CourseBuilder();
                var totalRecords = 0;
                if (buildby == "CA")
                {
                    var rec = CourseObj.GetCourseBuilderList(searchText, CourseId, buildby, sidx,
                                               sord, page ?? 1, rows ?? 10,
                                               ref totalRecords).AsEnumerable().Select((x =>
                                                                         new
                                                                         {
                                                                             x.ID,
                                                                             x.Title,
                                                                             x.CreatedDate,
                                                                             x.DateFrom,
                                                                             x.DateTo,
                                                                             x.CoordinateType
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
                else
                {
                    var rec = CourseObj.GetCourseBuilderList(searchText, CourseId, buildby, sidx,
                                                 sord, page ?? 1, rows ?? 10,
                                                 ref totalRecords).AsEnumerable().Select((x =>
                                                                           new
                                                                           {
                                                                               x.ID,
                                                                               // x.Title,
                                                                               x.ModifyDate,
                                                                               x.Status,
                                                                               x.Comments
                                                                               // x.DateFrom,
                                                                               // x.DateTo
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
            }
            catch (Exception ex)
            {
                ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
                return null;
            }
        }

        /// <summary>
        /// Created By: Amit Kumar
        /// Creation On: 22 April 2015
        /// Description: method to delete
        /// </summary>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult DeleteCourseBuilder(List<long> ids)
        {
            try
            {
                if (ids != null)
                {
                    var obj = new GF_CourseBuilder();
                    return obj.DeleteCourseBuilder(ids)
                               ? Json(new { statusText = "success", module = "Co-ordinate(s)", task = "delete", errormessage = obj.Message })
                               : Json(new { statusText = "error", module = "Co-ordinate(s)", task = "delete", message = obj.Message });
                }
                return Json(new { statusText = "error", module = "Co-ordinate(s)", task = "select" });
            }
            catch (Exception ex)
            {
                ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
                return null;
            }

        }

        public ActionResult SelectCourse(long CourseID)
        {
            SelectedCourseID = CourseID;
            return Json(new { success = "1" });
        }

        #endregion

        #region Manage Course

        /// <summary>
        /// Created By:Amit Kumar
        /// Creation On:15 March 2016
        /// Description: Get all course listing
        /// </summary>
        [SessionExpireFilterAttribute]
        public ActionResult GetCourseListing(string searchText, string cityName, string sidx, string sord, int? page, int? rows)
        {
            try
            {
                Course objCourse = new Course();
                var totalRecords = 0;
                var rec = objCourse.GetCourseListInfo(searchText, cityName, sidx,
                                           sord, page ?? 1, rows ?? 10,
                                           ref totalRecords).AsEnumerable().Select((x =>
                                                                     new
                                                                     {
                                                                         ID = x.ID,
                                                                         x.COURSE_NAME,
                                                                         x.CLUB_HOUSE,
                                                                         x.STATUS,
                                                                         x.HOLE,
                                                                         EID = CommonFunctions.EncryptUrlParam(x.ID),
                                                                         x.SCORECARD,
                                                                         x.ClubHouseID,
                                                                         DoActive = x.ID != 0,
                                                                         IsRead = x.ID == (SelectedCourseID == 0 ? LoginInfo.CourseId : SelectedCourseID)
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
        /// Created By: Amit Kumar
        /// Created Date: 15 March 2016
        /// Purpose: Course Score Card Data
        /// </summary>
        /// <param name="eid"></param>
        /// <returns></returns>
        [SessionExpireFilterAttribute]
        public ActionResult CourseScoreCard(string eid)
        {
            AccessModule(ModuleValues.CourseBuilder);

            ViewBag.eid = eid;
            TempData["CourseEID"] = eid;

            long id = CommonFunctions.DecryptUrlParam(eid);
            Course obj = new Course();

            var objCOurse = obj.GetCourseByID(id);
            objCOurse.UserID = obj.GetUserByCourseID(id);
            objCOurse.Active = objCOurse.Status == StatusType.Active ? true : false;

            ViewBag.Users = new SelectList(obj.GetCourseUserByCourseID(objCOurse.ID), "ID", "Name", objCOurse.UserID);
            ViewBag.Partner = new SelectList(PartershipStatus.GetPartnerShipStatus(), "Tag", "Name", objCOurse.PartnershipStatus);
            ViewBag.Country = new SelectList(obj.GetAllActiveCountiesCourse(), "COUNTRY_NAME", "COUNTRY_NAME", objCOurse.Country);
            ViewBag.County = new SelectList(obj.GetAllActiveCounties(), "COUNTY_NAME", "COUNTY_NAME", objCOurse.COUNTY);
            ViewBag.ActiveDive = TempData["ActiveDive"];

            if (!string.IsNullOrEmpty(Convert.ToString(TempData["CourseMessage"])))
            {
                ViewBag.CourseMessage = TempData["CourseMessage"];
            }
            else
            {
                ViewBag.CourseMessage = "";
            }
            if (!string.IsNullOrEmpty(Convert.ToString(TempData["courseSettings"])))
            {
                ViewBag.CourseSetting = TempData["courseSettings"];
            }
            else
            {
                ViewBag.CourseSetting = "";
            }

            return View(objCOurse);
        }

        [SessionExpireFilterAttribute]
        [HttpPost]
        public ActionResult CourseScoreCard(GF_CourseInfo objCourse)
        {
            try
            {
                Course obj = new Course();
                objCourse.PartnershipStatus = PartershipStatus.Partner;
                if (obj.updateCourseSettingsbyCourseAdmin(objCourse))
                {
                    TempData["courseSettings"] = "Course Setting saved successfully.";
                    ViewBag.Users = new SelectList(obj.GetCourseUser(), "ID", "Name");
                    ViewBag.Partner = new SelectList(PartershipStatus.GetPartnerShipStatus(), "Tag", "Name");
                    ViewBag.Country = new SelectList(obj.GetAllActiveCountiesCourse(), "COUNTRY_NAME", "COUNTRY_NAME");
                    ViewBag.County = new SelectList(obj.GetAllActiveCounties(), "COUNTY_NAME", "COUNTY_NAME");
                    ViewBag.State = new SelectList(obj.GetState(), "ID", "Name");
                }
            }
            catch (DbEntityValidationException e)
            {
                foreach (var eve in e.EntityValidationErrors)
                {
                    ErrorLibrary.ErrorClass.WriteLog(e.GetBaseException(), System.Web.HttpContext.Current.Request);
                }

                ViewBag.ErrorMessage = e.Message;
            }
            TempData["ActiveDive"] = "coursesetting";
            string eid = Convert.ToString(Request.Form["hdnEIDHole"]);
            return RedirectToAction("CourseScoreCard/" + eid);
        }

        #endregion

        #region Delete selected Golfer
        /// <summary>
        /// Created By: Arun
        /// Creation On: 31 March 2015
        /// Description: method to delete
        /// </summary>
        [AcceptVerbs(HttpVerbs.Post)]
        [SessionExpireFilterAttribute]
        public ActionResult DeleteCoursesInfo(long[] ids)
        {
            AccessModule(ModuleValues.AllRights);
            try
            {
                if (ids != null)
                {
                    Course obj = new Course();
                    return obj.DeletCourseInfo(ids)
                               ? Json(new { statusText = "success", module = "Course Info", task = "delete" })
                               : Json(new { statusText = "error", module = "Course Info", task = "delete", message = obj.Message });
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

        #region Manage Process Refund

        /// 
        /// Created By: Veera Verma
        /// Created on: 14 April 2015
        /// Description: method for list
        /// 
        [SessionExpireFilterAttribute]
        public ActionResult ProcessRefundList()
        {
            //if (LoginInfo.IsSuper)
            //{
            AccessModule(ModuleValues.ProcessRefund);
            //}
            //else
            //{
            //    AccessModule(ModuleValues.ProcessRefund);
            //}

            ViewBag.Message = TempData["Message"];
            Course objCourse = new Course();
            if (LoginInfo.Type == "SA")
            {
                ViewBag.CourseId = "";
            }
            else
            {
                ViewBag.CourseId = LoginInfo.CourseId;
            }
            ViewBag.CourseIds = new SelectList(objCourse.GetAllCourses(), "ID", "COURSE_NAME");
            return View();
        }

        /// 
        /// Created By: Veera Verma
        /// Created on: 14 April 2015
        /// Description: method to bind grid
        /// 
        [SessionExpireFilterAttribute]
        public ActionResult GetOrdersList(string searchText, string sidx, string sord, int? page, int? rows, string fromDate, string toDate, string CourseId)
        {
            try
            {
                var OrderObj = new Order();
                var totalRecords = 0;
                var rec = OrderObj.GetOrdersForRefund(searchText, sidx,
                                           sord, page ?? 1, rows ?? 10,
                                           ref totalRecords, fromDate, toDate, CourseId).AsEnumerable().Select((x =>
                                                                     new
                                                                     {
                                                                         x.ID,
                                                                         Date = x.CreatedDate,
                                                                         GolferName = Db.GF_Golfer.FirstOrDefault(y => y.GF_ID == x.GolferID).FirstName,
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

        [SessionExpireFilterAttribute]
        public ActionResult GetRefundOrdersList(string searchText, string sidx, string sord, int? page, int? rows, string fromDate, string toDate, string CourseId)
        {
            try
            {
                var OrderObj = new Order();
                var totalRecords = 0;
                var rec = OrderObj.GetRefundedOrders(searchText, sidx,
                                           sord, page ?? 1, rows ?? 10,
                                           ref totalRecords, fromDate, toDate, CourseId).AsEnumerable().Select((x =>
                                                                     new
                                                                     {
                                                                         x.ID,
                                                                         Date = x.CreatedDate,
                                                                         OrderID = x.OrderId,
                                                                         GolferName = Db.GF_Golfer.FirstOrDefault(y => y.GF_ID == (x.GF_Order.GolferID)).FirstName,
                                                                         RefundType = x.RefundType,
                                                                         RefundAmt = x.RefundAmount,
                                                                         RefundedByName = Db.GF_AdminUsers.FirstOrDefault(y => y.ID == x.CreatedBy).FirstName,
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


        [SessionExpireFilterAttribute]
        [HttpGet]
        public ActionResult ProcessRefundadd(string orderId, string refundId)
        {
            try
            {
                long id = Convert.ToInt64(orderId);
                //if (LoginInfo.IsSuper)
                AccessModule(ModuleValues.ProcessRefund);
                //else
                //    AccessModule(ModuleValues.ProcessRefund);

                GF_Order obj = new GF_Order();
                obj = Db.GF_Order.FirstOrDefault(x => x.ID == id);
                if (obj != null)
                {
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
                }
                //GF_PromoCode obj = promoCode.GetPromoCodeByID(id);

                // ViewBag.MenuItemList = new SelectList(promoCode.GetMenuItemList(), "ID", "Name", obj.ReferenceID);
                // ViewBag.IsLoginUser = (id != 0 && id == LoginInfo.UserId);
                ViewBag.refundId = refundId;
                return View(obj);
            }
            catch (DbEntityValidationException e)
            {
                foreach (var eve in e.EntityValidationErrors)
                {
                    ErrorLibrary.ErrorClass.WriteLog(e.GetBaseException(), System.Web.HttpContext.Current.Request);
                }

                ViewBag.ErrorMessage = e.Message;
            }
            return View();
        }

        public ActionResult OrderRefund(string refundAmount, string refundType, string refundDesc, string orderId)
        {
            bool result = true; string errorMsg = "";
            try
            {
                if (refundType == "" || refundDesc == "" || orderId == "")
                {
                    result = false;
                    return Json(new { result }, JsonRequestBehavior.AllowGet);
                }

                long id = Convert.ToInt64(orderId);


                GF_Order obj = new GF_Order();
                obj = Db.GF_Order.FirstOrDefault(x => x.ID == id);
                if (obj != null)
                {


                    // StripePayments objStripe = new StripePayments();
                    BrainTreePayments objBT = new BrainTreePayments();
                    bool refundResult = objBT.RefundCharge(refundDesc, Convert.ToInt64(orderId), refundType, Convert.ToDecimal(refundAmount), ref errorMsg);
                    if (refundResult)
                    {
                        result = true;
                    }
                    else
                    {
                        result = false;

                    }

                }
                else
                {
                    result = false;
                }


            }
            catch (DbEntityValidationException e)
            {
                foreach (var eve in e.EntityValidationErrors)
                {
                    ErrorLibrary.ErrorClass.WriteLog(e.GetBaseException(), System.Web.HttpContext.Current.Request);
                }

                errorMsg = e.Message;
                result = false;
            }
            return Json(new { result, error = errorMsg }, JsonRequestBehavior.AllowGet);
        }


        #endregion

        #region Email Template

        /// <summary>
        /// Created By:
        /// Creation On:
        /// Description: method for list
        /// </summary>
        [SessionExpireFilterAttribute]
        public ActionResult EmailTemplates()
        {
            try
            {
                //if (LoginInfo.IsSuper)
                AccessModule(ModuleValues.EmailTemplates);
                //else
                //    AccessModule(ModuleValues.EmailTemplates);

                ViewBag.Message = TempData["Message"];
                return View();
            }
            catch (Exception ex)
            {
                ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
                return null;
            }

        }

        /// <summary>
        /// Created By:
        /// Creation On:
        /// Description:
        /// </summary>
        [SessionExpireFilterAttribute]
        public ActionResult GetEmailTemplates(long orgid, string searchText, string sidx, string sord, int? page, int? rows)
        {
            try
            {
                var templateObj = new EmailTemplate();
                var totalRecords = 0;
                var rec = templateObj.GetAdminEmailTemplates(searchText, sidx,
                                           sord, page ?? 1, rows ?? 10,
                                           ref totalRecords).AsEnumerable().Select(x =>
                                                                     new
                                                                     {
                                                                         ID = x.ID,
                                                                         TemplateName = x.TemplateName,
                                                                         ModifiedOn = x.ModifiedOn,
                                                                         EID = CommonFunctions.EncryptUrlParam(x.ID)
                                                                     }
                                                                    );

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
        /// Created By:
        /// Creation On:
        /// Description: method to get add/edit
        /// </summary>
        [SessionExpireFilterAttribute]
        public ActionResult EditTemplate(string eid)
        {
            try
            {
                long id = CommonFunctions.DecryptUrlParam(eid);

                //if (LoginInfo.IsSuper)
                // AccessModule(ModuleValues.AllRights);
                //else
                AccessModule(ModuleValues.EmailTemplates);

                var emailObj = new EmailTemplate();
                var lst = new List<GF_CourseEmailTemplates>();
                var lstfields = new List<GF_EmailTemplatesFields>();
                lst = emailObj.GetAdminTemplatesDetail(id, ref lstfields);
                ViewBag.fields = lstfields;
                var objModel = new GF_EmailTemplates();
                objModel.MessageBody = lst[0].MessageBody;
                objModel.TemplateName = lst[0].TemplateName;
                objModel.MessageBodyOriginal = lst[0].MessageBody;
                objModel.ID = lst[0].ID;
                return View(objModel);
            }
            catch (Exception ex)
            {
                ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
                return null;
            }

        }

        [ValidateInput(false)]
        public ActionResult UpdateEmailTemplate(long id, string content)
        {
            //if (LoginInfo.IsSuper)
            AccessModule(ModuleValues.EmailTemplates);
            //else
            //    AccessModule(ModuleValues.EmailTemplates);

            var temp = new EmailTemplate();
            var status = temp.UpdateEmailTemplate(id, content);
            if (status)
                TempData["Message"] = string.Format("Email Template update successfully.");
            return Json(new { msg = status });
        }

        #endregion

        #region Resolution center


        [SessionExpireFilterAttribute]
        public ActionResult ResolutionMessages()
        {
            AccessModule(ModuleValues.Inbox);
            var UserObj = new GF_ResolutionCenter();
            ViewBag.CourseName = new SelectList(UserObj.GetCourseInfo(), "ID", "Course_name", "0");
            ViewBag.ResolutionType = new SelectList(ResolutionType.GetResolutionType(), "Tag", "Name");
            return View();
        }

        [SessionExpireFilterAttribute]
        public ActionResult ResolutionMessagereply(string eid)
        {
            AccessModule(ModuleValues.Inbox);
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
                @ViewBag.SentByName = (MainRes.GolferID ?? 0) != 0 ? MainRes.GF_Golfer.FirstName + " " + MainRes.GF_Golfer.LastName : ((MainRes.SenderID ?? 0) == LoginInfo.UserId ? "me" : MainRes.GetAdminName(MainRes.SenderID ?? 0, MainRes.SenderType, Db));
                @ViewBag.CourseName = MainRes.GF_CourseInfo.COURSE_NAME;
                @ViewBag.eidReply = eid;
            }

            MainRes.ReadStatusResolutionMessage(msgid);

            //return View();
            return View(new GF_ResolutionMessageHistory());
        }

        [HttpGet]
        [SessionExpireFilterAttribute]
        public ActionResult AddResolutionMessages()
        {
            ViewBag.ResolutionCenterType = new SelectList(ResolutionCenterType.GetResolutionCenterType(), "Tag", "Name");
            ViewBag.ResolutionSendToType = new SelectList(UserType.GetResolutionSendToType(), "Tag", "Name");

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

        #region Settings
        /// <summary>
        /// Created By: Veera
        /// Created Date:21 April 2015
        /// Purpose: Get Settings
        /// </summary>
        /// <returns></returns>
        [SessionExpireFilterAttribute]

        public ActionResult SettingAddEdit()
        {
            AccessModule(ModuleValues.ManageSettings);

            GF_Settings objSettings = Db.GF_Settings.FirstOrDefault(x => x.CourseID == LoginInfo.CourseUserId);

            #region Time Zone Settings

            var timeZone = objSettings.lstSettings.FirstOrDefault(x => x.Name.ToLower() == WebSetting.CourseTimeZone.ToLower());

            string value = "0";
            if (timeZone != null)
            {
                try
                {
                    if (Convert.ToInt64(timeZone.Value) >= 1)
                    {
                        value = timeZone.Value;
                    }
                }
                catch { }
            }

            //var lstTimeZone = TimeZoneInfo.GetSystemTimeZones().ToList();
            //ViewBag.TimeZone = new SelectList(lstTimeZone, "Id", "DisplayName", value);
            ViewBag.TimeZone = new SelectList(CommonFunctions.GetTimeZone(), "ID", "TimeZone_Name", value);
            ViewBag.TimeZoneValue = value;

            #endregion

            return View(objSettings);
        }


        /// <summary>
        /// Created By: Veera
        /// Created Date:21 April 2015
        /// Purpose: Update Settings
        /// </summary>
        /// <returns></returns>
        [SessionExpireFilterAttribute]
        [HttpPost]
        public ActionResult SettingAddEdit(GF_Settings obj, FormCollection frm)
        {
            AccessModule(ModuleValues.ManageSettings);
            try
            {
                var lstName = string.IsNullOrEmpty(frm["txtName"]) ? new List<string>() : Convert.ToString(frm["txtName"]).Split(',').Select(x => x).ToList();
                var lstValue = string.IsNullOrEmpty(frm["txtValue"]) ? new List<string>() : Convert.ToString(frm["txtValue"]).Split(',').Select(x => x).ToList();
                obj.UpdateSettings(lstName, lstValue, LoginInfo.CourseUserId);
                obj.CourseID = LoginInfo.CourseUserId;

                #region Time Zone Settings

                var timeZone = obj.lstSettings.FirstOrDefault(x => x.Name.ToLower() == WebSetting.CourseTimeZone.ToLower());

                string value = "0";
                if (timeZone != null)
                {
                    try
                    {
                        if (Convert.ToInt64(timeZone.Value) >= 1)
                        {
                            value = timeZone.Value;
                        }
                    }
                    catch { }
                }

                //var lstTimeZone = TimeZoneInfo.GetSystemTimeZones().ToList();
                //ViewBag.TimeZone = new SelectList(lstTimeZone, "Id", "DisplayName", value);
                ViewBag.TimeZone = new SelectList(CommonFunctions.GetTimeZone(), "ID", "TimeZone_Name", value);
                ViewBag.TimeZoneValue = value;

                #endregion

                TempData["Message"] = "Setting(s) updated sucessfully.";
            }
            catch (Exception ex)
            {
                ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
            }
            return View(obj);
        }

        #endregion

        #region SMTP details

        [HttpGet]
        [SessionExpireFilterAttribute]
        public ActionResult SMTPDetails()
        {
            try
            {
                //if (LoginInfo.IsSuper)
                //else

                AccessModule(ModuleValues.Smtp);

                GF_SMTPSettings objModel = new GF_SMTPSettings();
                long id = LoginInfo.CourseId;
                Templates objTemp = new Templates();
                List<GF_SMTPSettings> lstSmtp = new List<GF_SMTPSettings>();
                lstSmtp = objTemp.GetSMTPSettings(id);
                ViewBag.orgEditId = id;
                ViewBag.isSMTPDetails = true;
                ViewBag.Isedit = "0";

                if (lstSmtp.Count > 0)
                {
                    objModel.AdminEmail = lstSmtp[0].AdminEmail;
                    objModel.FromEmail = lstSmtp[0].FromEmail;
                    objModel.SMTPHost = lstSmtp[0].SMTPHost;
                    objModel.SMTPPassword = lstSmtp[0].SMTPPassword;
                    objModel.SMTPPort = lstSmtp[0].SMTPPort;
                    objModel.ReplyEmail = lstSmtp[0].ReplyEmail;
                    objModel.SMTPUserName = lstSmtp[0].SMTPUserName;
                    objModel.EnableSsl = lstSmtp[0].EnableSsl;
                    objModel.EnableTls = lstSmtp[0].EnableTls;
                }


                //---------new


                //AccessModule(ModuleValues.Smtp);
                //if (ViewBag.ReadFlag=="True")
                //{//if have Read access
                //    if (lstSmtp.Count > 0)
                //    {
                //        objModel.AdminEmail = lstSmtp[0].AdminEmail;
                //        objModel.FromEmail = lstSmtp[0].FromEmail;
                //        objModel.SMTPHost = lstSmtp[0].SMTPHost;
                //        objModel.SMTPPassword = lstSmtp[0].SMTPPassword;
                //        objModel.SMTPPort = lstSmtp[0].SMTPPort;
                //        objModel.ReplyEmail = lstSmtp[0].ReplyEmail;
                //        objModel.SMTPUserName = lstSmtp[0].SMTPUserName;
                //        objModel.EnableSsl = lstSmtp[0].EnableSsl;
                //        objModel.EnableTls = lstSmtp[0].EnableTls;
                //    }
                //}
                //else
                //{
                //    objModel.AdminEmail = "";
                //    objModel.FromEmail = "";
                //    objModel.SMTPHost = "";
                //    objModel.SMTPPassword = "";
                //    objModel.SMTPPort = "";
                //    objModel.ReplyEmail = "";
                //    objModel.SMTPUserName = "";
                //    objModel.EnableSsl = null;
                //    objModel.EnableTls = null;
                //}



                ViewBag.CourseId = objModel.CourseID;
                return View(objModel);
            }
            catch (Exception ex)
            {
                ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
                return null;
            }
        }

        [SessionExpireFilterAttribute]
        [HttpPost]
        public ActionResult SMTPDetails(GF_SMTPSettings obj, FormCollection frm)
        {
            AccessModule(ModuleValues.Smtp);
            try
            {
                Templates objTemplates = new Templates();
                obj.CourseID = LoginInfo.CourseId;
                var status = objTemplates.SaveSMTP(obj);
                ViewBag.Message = string.Format("SMTP Details update successfully.");

            }
            catch (Exception ex)
            {
                ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
            }
            return View(obj);
        }

        #endregion

        #region LoadEmail PopUp

        public ActionResult LoadEmail(string partialView, string from, string host, string password, int port, string username, string EnableSsl, string EnableTls = "")
        {
            AccessModule(ModuleValues.Smtp);
            ViewBag.Host = host;
            ViewBag.Password = password;
            ViewBag.Port = port;
            ViewBag.From = from;
            ViewBag.UserName = username;
            ViewBag.EnableSsl = EnableSsl;
            ViewBag.EnableTls = EnableTls;

            return PartialView(partialView);
        }

        public ActionResult SendEmail(string username, string from, string to, string subject, string body, string host, string password, int port, bool EnableSsl, bool EnableTls = false)
        {
            AccessModule(ModuleValues.Smtp);
            var status = false;

            try
            {
                var smtp = new System.Net.Mail.SmtpClient();
                {
                    smtp.Host = host;
                    if (port != null && port != 0)
                        smtp.Port = port;

                    if (EnableTls)
                        smtp = new System.Net.Mail.SmtpClient(host, port);

                    smtp.DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network;
                    smtp.Credentials = new NetworkCredential(username, password);
                    smtp.Timeout = Params.TimeOut;
                    //smtp.EnableSsl = EnableSsl;
                    if (EnableSsl || EnableTls)
                    {
                        smtp.EnableSsl = true;
                    }
                }


                // Passing values to smtp object
                smtp.Send(from, to, subject, body);
                status = true;
            }
            catch (Exception ex)
            {
                ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
                status = false;
            }

            return Json(new { msg = status });
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

            AccessModule(ModuleValues.MessageCenter);
            //else
            //    AccessModule(ModuleValues.MessagingCenter);

            ViewBag.Message = TempData["Message"];

            return View();
        }

        /// <summary>
        /// Created By:
        /// Creation On:
        /// Description: method to bind grid
        /// </summary>
        [SessionExpireFilterAttribute]
        public ActionResult GetOnlineGolferList(string searchText, string type, string status, string sidx, string sord, int? page, int? rows)
        {

            try
            {
                GolflerEntities Db = new GolflerEntities();
                GF_Golfer obj = new GF_Golfer();

                var totalRecords = 0;
                var list = obj.GetOnlineGolferListByCourseIdList(Convert.ToString(LoginInfo.CourseId), LoginInfo.UserId, "0", type, status, searchText, sidx, sord, page ?? 1, rows ?? 10, ref totalRecords);


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
        #endregion

        #region Manage Food Items

        /// <summary>
        /// Created By: Amit Kumar
        /// Created on: 23 April 2015
        /// Description: method for list
        /// </summary>
        [SessionExpireFilterAttribute]
        public ActionResult MenuItemList(string eid)
        {
            AccessModule(ModuleValues.ManageFoodItems);

            if (eid != null)
            {
                var catID = Db.GF_Category.FirstOrDefault(x => x.Name.ToLower() == eid.ToLower());

                if (catID != null)
                    TempData["MenuItemFilter"] = catID.ID;
                else
                    TempData["MenuItemFilter"] = "0";
            }
            else
            {
                TempData["MenuItemFilter"] = "0";
            }

            return View();
        }

        /// <summary>
        /// Created By: Amit Kumar
        /// Creation On: 23 April 2015
        /// Description: Get all Menu User listing
        /// </summary>
        [SessionExpireFilterAttribute]
        public ActionResult GetSubCategoryList(string searchText, long category, long subCategory,
            string sidx, string sord, int? page, int? rows)
        {
            try
            {
                GF_SubCategory objCat = new GF_SubCategory();
                var totalRecords = 0;
                var rec = objCat.GetSubCategory(searchText, category, subCategory, sidx,
                                           sord, page ?? 1, rows ?? 10,
                                           ref totalRecords).AsEnumerable().Select((x =>
                                                                     new
                                                                     {
                                                                         ID = x.ID,
                                                                         x.Name,
                                                                         x.Status,
                                                                         x.IsActive,
                                                                         x.Category,
                                                                         EID = CommonFunctions.EncryptUrlParam(x.ID)

                                                                         //Status = x.GF_CourseFoodItem.FirstOrDefault(y => y.CourseID == LoginInfo.CourseId) == null ? StatusType.InActive : x.GF_CourseFoodItem.FirstOrDefault(y => y.CourseID == LoginInfo.CourseId).AllowStatus,
                                                                         //IsActive = x.GF_CourseFoodItem.FirstOrDefault(y => y.CourseID == LoginInfo.CourseId) == null ? false : x.GF_CourseFoodItem.FirstOrDefault(y => y.CourseID == LoginInfo.CourseId).IsAllow,
                                                                         //Category = x.GF_Category.Name,
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
        /// Created By: Amit Kumar
        /// Created date: 23 April 2015
        /// Purpose: Update Status
        /// </summary>
        /// <param name="id"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        [SessionExpireFilterAttribute]
        public ActionResult UpdateFoodItemStatus(long id, string status)
        {
            AccessModule(ModuleValues.ManageFoodItems);
            try
            {
                GF_SubCategory objCat = new GF_SubCategory();

                return objCat.ChangeFoodItemStatus(id, status)
                           ? Json(new { statusText = "success", module = "Food item", task = "update", message = objCat.Message })
                           : Json(new { statusText = "error", module = "Food item", task = "update", message = objCat.Message });
            }
            catch (Exception ex)
            {
                ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
                return null;
            }
        }

        /// <summary>
        /// Created By: Amit Kumar
        /// Creation On: 22 April 2015
        /// Description: method to get add/edit
        /// </summary>
        [SessionExpireFilterAttribute]
        public ActionResult MenuItemAddEdit(string eid, string type)
        {
            AccessModule(ModuleValues.ManageFoodItems);
            long id = CommonFunctions.DecryptUrlParam(eid);

            var subCategory = new GF_SubCategory();
            GF_CourseFoodItem obj = new GF_CourseFoodItem();
            obj = subCategory.GetCourseFoodItemByID(id);

            if (obj == null)
                return RedirectToAction("MenuItemList");

            obj.CategoryID = obj.GF_SubCategory.GF_Category.ID;
            obj.CatName = obj.GF_SubCategory.GF_Category.Name;

            obj.FoodItemDetail = Db.GF_MenuItems.Where(x => x.Status == StatusType.Active && x.SubCategoryID == obj.SubCategoryID).ToList()
                                                .Select(x => new GF_CourseFoodItemDetail
                                                {
                                                    ID = x.GF_CourseFoodItemDetail.FirstOrDefault(y => y.GF_CourseFoodItem.CourseID == LoginInfo.CourseId) == null ? 0 : x.GF_CourseFoodItemDetail.FirstOrDefault(y => y.GF_CourseFoodItem.CourseID == LoginInfo.CourseId).ID,
                                                    Active = x.GF_CourseFoodItemDetail.FirstOrDefault(y => y.GF_CourseFoodItem.CourseID == LoginInfo.CourseId) == null ? false : x.GF_CourseFoodItemDetail.FirstOrDefault(y => y.GF_CourseFoodItem.CourseID == LoginInfo.CourseId).IsActive ?? false,
                                                    MenuItemID = x.ID,
                                                    ItemName = x.Name,
                                                    Quantity = x.GF_CourseFoodItemDetail.FirstOrDefault(y => y.GF_CourseFoodItem.CourseID == LoginInfo.CourseId) == null ? 0 : x.GF_CourseFoodItemDetail.FirstOrDefault(y => y.GF_CourseFoodItem.CourseID == LoginInfo.CourseId).Quantity ?? 0,
                                                    CostPrice = x.GF_CourseFoodItemDetail.FirstOrDefault(y => y.GF_CourseFoodItem.CourseID == LoginInfo.CourseId) == null ? 0M : x.GF_CourseFoodItemDetail.FirstOrDefault(y => y.GF_CourseFoodItem.CourseID == LoginInfo.CourseId).CostPrice ?? 0M,
                                                    Price = x.GF_CourseFoodItemDetail.FirstOrDefault(y => y.GF_CourseFoodItem.CourseID == LoginInfo.CourseId) == null ? x.Amount : x.GF_CourseFoodItemDetail.FirstOrDefault(y => y.GF_CourseFoodItem.CourseID == LoginInfo.CourseId).Price,
                                                    Itemoption = x.GF_CourseFoodItemDetail.FirstOrDefault(y => y.GF_CourseFoodItem.CourseID == LoginInfo.CourseId) == null ?
                                                            x.GF_MenuItemOption.Where(q => q.Status == StatusType.Active).ToList() :
                                                            x.GF_CourseFoodItemDetail.FirstOrDefault(y => y.GF_CourseFoodItem.CourseID == LoginInfo.CourseId).GF_MenuItems.GF_MenuItemOption.Where(q => q.Status == StatusType.Active)
                                                            .Select(z => new GF_MenuItemOption
                                                            {
                                                                ID = z.ID,
                                                                MenuItemID = z.MenuItemID,
                                                                Name = z.Name,
                                                                Status = z.Status,
                                                                CourseID = z.CourseID,
                                                                IsSelected = z.GF_CourseFoodItemOption.FirstOrDefault(w => w.MenuItemOptionID == z.ID) == null ? false : z.GF_CourseFoodItemOption.FirstOrDefault().IsActive ?? false
                                                            }).ToList(),
                                                    //string.Join(",", x.GF_MenuItemOption.Where(q => q.Status == StatusType.Active).Select(q => q.Name)) :
                                                    //string.Join(",", x.GF_CourseFoodItemDetail.FirstOrDefault(y => y.GF_CourseFoodItem.CourseID == LoginInfo.CourseId).GF_MenuItems.GF_MenuItemOption.Where(q => q.Status == StatusType.Active).Select(q => q.Name)),
                                                }).ToList();

            ViewBag.IsLoginUser = (id != 0 && id == LoginInfo.UserId);

            return View(obj);
        }

        /// <summary>
        /// Created By: Amit Kumar
        /// Creation On: 22 April 2015
        /// Description: method to get add/edit
        /// </summary>
        [HttpPost]
        [SessionExpireFilterAttribute]
        public ActionResult MenuItemAddEdit(string eid, GF_CourseFoodItem objFood)
        {
            AccessModule(ModuleValues.ManageFoodItems);
            long id = CommonFunctions.DecryptUrlParam(eid);

            var isSaved = false;
            var obj = new GF_SubCategory();

            ViewBag.CategoryList = new SelectList(obj.GetCategoryList(), "ID", "Name");
            ViewBag.IsLoginUser = (id != 0 && id == LoginInfo.UserId);

            //if (ModelState.IsValid)
            //{
            isSaved = obj.SaveCourseFoodItem(objFood);
            //}
            if (isSaved)
            {
                string module = "Menu Item";
                //TempData["Message"] = CommonFunctions.Message(menuItems.Message, module);
                //return RedirectToAction("MenuItemList");
                //ViewBag.Message = CommonFunctions.Message(obj.Message, module);
                TempData["Message"] = CommonFunctions.Message(obj.Message, module);

                return RedirectToAction("MenuItemAddEdit"); //View(objFood);
            }
            ViewBag.ErrorMessage = obj.Message;

            return View(objFood);
        }

        /// <summary>
        /// Created By: Amit Kumar
        /// Creation On: 27 May 2015
        /// Description: method to get add/edit
        /// </summary>
        [HttpGet]
        [SessionExpireFilterAttribute]
        public ActionResult GetCategory()
        {
            var obj = new GF_SubCategory();
            var catData = obj.GetCategoryList();

            return Json(new { data = catData, msg = "success" }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Created By: Amit Kumar
        /// Creation On: 27 May 2015
        /// Description: method to get sub category
        /// </summary>
        [HttpGet]
        [SessionExpireFilterAttribute]
        public ActionResult GetSubCategory(long catID)
        {
            var obj = new GF_SubCategory();
            var subCatData = obj.GetSubCategoryList(catID);

            return Json(new { data = subCatData, msg = "success" }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Created By: Amit Kumar
        /// Creation On: 20 August 2015
        /// Description: method to get menu item
        /// </summary>
        [HttpGet]
        [SessionExpireFilterAttribute]
        public ActionResult GetMenuItems(long subCatID)
        {
            var obj = new GF_SubCategory();
            var menuItemData = obj.GetMenuItemsList(subCatID);

            return Json(new { data = menuItemData, msg = "success" }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Created By: Amit Kumar
        /// Created date: 23 June 2015
        /// Purpose: Update Menu Item Status
        /// </summary>
        /// <param name="id"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        [HttpPost]
        [SessionExpireFilterAttribute]
        public ActionResult AddMenuItemOption(MenuItemOption obj)
        {
            try
            {
                AccessModule(ModuleValues.ManageFoodItems);

                GF_SubCategory objCat = new GF_SubCategory();

                return objCat.AddMenuItemOption(obj)
                           ? Json(new { statusText = "success", module = "Menu Item Option", task = "update", message = objCat.Message }, JsonRequestBehavior.AllowGet)
                           : Json(new { statusText = "error", module = "Menu Item Option", task = "update", message = objCat.Message }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
                return null;
            }
        }

        /// <summary>
        /// Created By: Amit Kumar
        /// Created date: 23 June 2015
        /// Purpose: Update Menu Item Status
        /// </summary>
        /// <param name="id"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        [HttpGet]
        [SessionExpireFilterAttribute]
        public ActionResult RemoveMenuItemOption(long optionID)
        {
            try
            {
                AccessModule(ModuleValues.ManageFoodItems);

                GF_SubCategory objCat = new GF_SubCategory();

                return objCat.RemoveMenuItemOption(optionID)
                           ? Json(new { statusText = "success", module = "Menu Item Option", task = "update", message = objCat.Message }, JsonRequestBehavior.AllowGet)
                           : Json(new { statusText = "error", module = "Menu Item Option", task = "update", message = objCat.Message }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
                return null;
            }
        }

        /// <summary>
        /// Created By: Amit Kumar
        /// Created date: 24 June 2015
        /// Purpose: Get Menu Item Option added by Course
        /// </summary>
        /// <param name="menuID"></param>
        /// <returns></returns>
        [HttpGet]
        [SessionExpireFilterAttribute]
        public ActionResult GetCourseMenuItemOption(long menuID)
        {
            try
            {
                AccessModule(ModuleValues.ManageFoodItems);

                GF_SubCategory objCat = new GF_SubCategory();

                var menuOption = objCat.GetCourseMenuItemOption(menuID);

                var json = new
                {
                    status = menuOption != null,
                    data = menuOption.Select(x => new
                    {
                        ID = x.ID,
                        Name = x.Name
                    })
                };

                return Json(new { json }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
                return null;
            }
        }

        [HttpGet]
        public ActionResult UpdateAdminStatus(string status)
        {
            if (status == StatusType.Active || status == StatusType.InActive)
            {
                bool result = CommonFunctions.UpdateAdminStatus(status);

                return Json(new { Result = result ? "Success" : "Failed" }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { Result = "Invalid Request Parameter" }, JsonRequestBehavior.AllowGet);
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
        ///// <summary>
        ///// Created By: Veera
        ///// Creation On: 27 April, 2015
        ///// Description: forgot password post method
        ///// </summary>
        //[HttpPost]
        //public ActionResult Forgot(ForgotModelAdmin obj)
        //{
        //    var rUser = new Users();
        //    if (ModelState.IsValid)
        //    {

        //        obj.Type = UserType.Admin;
        //        if (rUser.ResetPassword(obj))
        //        {

        //            ViewBag.Message = String.Format(Resources.Resources.pwdreset);
        //            return View();
        //        }
        //        ViewBag.ErrorMessage = rUser.Message;
        //    }
        //    return View(obj);
        //}
        #endregion

        #region Manage Ratings

        /// 
        /// Created By: Veera Verma
        /// Created on: 7 May 2015
        /// Description: method for list
        /// 
        [SessionExpireFilterAttribute]
        public ActionResult ManageRatings()
        {

            AccessModule(ModuleValues.Ratting);
            ViewBag.Message = TempData["Message"];
            return View();
        }


        //Created By: Veera Verma
        //Created on: 7 May 2015
        //Description: method to bind rating grid

        [SessionExpireFilterAttribute]
        public ActionResult GetRatingList(string searchText, string sidx, string sord, int? page, int? rows, string fromDate, string toDate)
        {
            try
            {
                var OrderObj = new Order();
                var totalRecords = 0;

                string timeZone = CommonFunctions.GetCourseTimeZone(LoginInfo.CourseId);

                var rec = OrderObj.GetRatingsList(searchText, sidx,
                                           sord, page ?? 1, rows ?? 10,
                                           ref totalRecords, fromDate, toDate).AsEnumerable().Select((x =>
                                                                     new
                                                                     {
                                                                         x.ID,
                                                                         Name = Db.GF_AdminUsers.FirstOrDefault(k => k.ID == x.ReferenceID).FirstName + " " + Db.GF_AdminUsers.FirstOrDefault(k => k.ID == x.ReferenceID).LastName,
                                                                         EmailAddress = Db.GF_AdminUsers.FirstOrDefault(k => k.ID == x.ReferenceID).Email,
                                                                         x.OrderNo,
                                                                         x.Rating,
                                                                         EID = CommonFunctions.EncryptUrlParam(x.ReferenceID ?? 0),
                                                                         Date = x.CreatedDate,
                                                                         GolferName = x.GF_Golfer.FirstName + " " + x.GF_Golfer.LastName,
                                                                         OrderDetails = new JavaScriptSerializer().Serialize(Db.GF_Order.Where(y => y.ID == x.OrderNo).ToList().Select(y =>
                                                                                            new
                                                                                            {
                                                                                                y.GolferPlatformFee,
                                                                                                y.TaxAmount,
                                                                                                OrderType = y.OrderType == "CO" ? "Cart Order" : "Turn Order",
                                                                                                CourseInfo = y.GF_CourseInfo.COURSE_NAME,
                                                                                                CourseAddress = y.GF_CourseInfo.ADDRESS,
                                                                                                PaymentMode = (y.PaymentType == "1" ? "Payment by Card" : "Payment by MembershipId"),
                                                                                                itemOrdered = new JavaScriptSerializer().Serialize(y.GF_OrderDetails.ToList().Select(z =>
                                                                                                    new
                                                                                                    {
                                                                                                        z.GF_MenuItems.Name,
                                                                                                        UnitPrice = z.GF_MenuItems.Amount,
                                                                                                        z.Quantity,
                                                                                                        Amount = (z.GF_MenuItems.Amount * z.Quantity)
                                                                                                    })),
                                                                                                PromoCodeDiscount = y.PromoCodeID == null ? 0 : y.DiscountAmt,
                                                                                                //CreatedDate = y.CreatedDate.Value.ToShortDateString(),
                                                                                                CreatedDate = CommonFunctions.DateByTimeZone(timeZone, y.OrderDate ?? DateTime.UtcNow),
                                                                                                GrandTotal = ((y.GF_OrderDetails.Sum(k => (k.UnitPrice * k.Quantity))) + y.TaxAmount + y.GolferPlatformFee).ToString(),
                                                                                                time = y.CreatedDate.Value.ToShortTimeString(),
                                                                                                billAmount = y.GF_OrderDetails.Sum(k => (k.UnitPrice * k.Quantity))
                                                                                            })),
                                                                         GolferEmail = x.GF_Golfer.Email,
                                                                         DateOfBirth = x.GF_Golfer.DateOfBirth == null ? "" : x.GF_Golfer.DateOfBirth.Value.ToShortDateString(),
                                                                         Gender = x.GF_Golfer.Gender,
                                                                         Status = x.GF_Golfer.IsOnline == true ? "Online" : "Offline",
                                                                         Phone = x.GF_Golfer.MobileNo,
                                                                         GolferCourse = ""
                                                                         //Db.GF_GolferUser.Where(l => l.GolferID == x.GF_Golfer.GF_ID).Select(
                                                                         //l => new {
                                                                         //   Db.GF_CourseInfo.FirstOrDefault(k=>k.ID==(l.CourseID ?? 0)).COURSE_NAME
                                                                         //})


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
        public ActionResult GetAdminUserByID(string eid)
        {
            AccessModule(ModuleValues.AllRights);

            long userID = CommonFunctions.DecryptUrlParam(eid);

            Users ob = new Users();
            var lst = ob.GetAdminUserByID(userID);

            if (lst.Count() > 0)
            {
                var lstUser = lst.Select(x => new
                {
                    UserName = x.UserName,
                    Name = x.FirstName + " " + x.LastName,
                    Email = x.Email,
                    Status = x.Status == StatusType.Active ? "Active" : "Inactive",
                    UserType = UserType.GetFullUserType(x.Type),
                    Image = string.IsNullOrEmpty(x.Image) ? "/images/noprofile.png" : x.Image,
                    Phone = x.Phone,
                    RoleName = (x.RoleId ?? 0) > 0 ? x.GF_Roles.Name : "No Role"
                }).FirstOrDefault();

                return Json(new { data = lstUser, status = 1 }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { data = "", status = 0 }, JsonRequestBehavior.AllowGet);
        }

        [SessionExpireFilterAttribute]
        public ActionResult GetRoleUserByID(string eid)
        {
            AccessModule(ModuleValues.AllRights);

            long userID = CommonFunctions.DecryptUrlParam(eid);

            Users ob = new Users();
            var lst = ob.GetRoleUserByID(userID);

            if (lst.Count() > 0)
            {
                var lstUser = lst.Select(x => new
                {
                    ModuelGroupID = x.GF_Modules.GroupId,
                    ModuleGroup = Db.GF_Modules_Group.FirstOrDefault(y => y.MD_Id == x.GF_Modules.GroupId).GroupName,
                    ModuelName = x.GF_Modules.Name,
                    Read = x.ReadFlag ? "Yes" : "No",
                    Edit = x.AddFlag ? "Yes" : "No"
                }).OrderBy(x => x.ModuelGroupID);

                return Json(new { data = lstUser, status = 1 }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { data = "", status = 0 }, JsonRequestBehavior.AllowGet);
        }

        //
        #endregion

        #region Mass Messages


        [SessionExpireFilterAttribute]
        public ActionResult MessageDetails(string eid)
        {
            AccessModule(ModuleValues.MassMessage);
            long messageid = 0;
            if (!string.IsNullOrEmpty(eid))
            {
                try
                {
                    messageid = Convert.ToInt64(CommonFunctions.DecryptUrlParam(eid));
                }
                catch
                {
                    messageid = 0;
                }
            }
            else
            {
                RedirectToAction("MassMessages");
            }
            if (messageid <= 0)
            {
                RedirectToAction("MassMessages");
            }

            ViewBag.MessageId = messageid;
            var DB = new GolflerEntities();
            GF_MassMessages MsgDetails = new GF_MassMessages();
            Int64 msgid = Convert.ToInt64(messageid);
            MsgDetails = Db.GF_MassMessages.Where(x => x.MassMsgId == msgid).FirstOrDefault();

            return View(MsgDetails);
        }

        [SessionExpireFilterAttribute]
        public ActionResult MessageHistory()
        {
            AccessModule(ModuleValues.MassMessage);
            Course objCourse = new Course();
            ViewBag.CourseIds = new SelectList(objCourse.GetAllActivePartnerCourses(), "ID", "COURSE_NAME");
            ViewBag.Type = new SelectList(UserType.GetSystemUsersForMassMsgs(), "Tag", "Name");
            return View();
        }

        [SessionExpireFilterAttribute]
        public ActionResult MassMessages()
        {
            AccessModule(ModuleValues.MassMessage);
            Course objCourse = new Course();
            ViewBag.CourseIds = new SelectList(objCourse.GetAllActivePartnerCourses(), "ID", "COURSE_NAME");
            ViewBag.Type = new SelectList(UserType.GetSystemUsersForMassMsgs(), "Tag", "Name");
            return View();
        }
        #endregion

        #region Main Email Template

        /// <summary>
        /// Created By:veera
        /// Creation On: 12 may 2015
        /// Description: method for add n view main email templates -- for personal use --no link is given 
        /// </summary>
        [SessionExpireFilterAttribute]
        public ActionResult MainEmailTemplates()
        {
            try
            {

                AccessModule(ModuleValues.EmailTemplates);
                ViewBag.Message = TempData["Message"];
                return View();
            }
            catch (Exception ex)
            {
                ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
                return null;
            }

        }

        /// <summary>
        /// Created By: veera
        /// Creation On: 12 may 2015
        /// Description: Get main template List
        /// </summary>
        [SessionExpireFilterAttribute]
        public ActionResult GetMainEmailTemplates(long orgid, string searchText, string sidx, string sord, int? page, int? rows)
        {
            try
            {
                var templateObj = new EmailTemplate();
                var totalRecords = 0;
                var rec = templateObj.GetMainEmailTemplates(searchText, sidx,
                                           sord, page ?? 1, rows ?? 10,
                                           ref totalRecords).AsEnumerable().Select(x =>
                                                                     new
                                                                     {
                                                                         ID = x.ID,
                                                                         TemplateName = x.TemplateName,
                                                                         ModifiedOn = x.ModifiedOn,
                                                                         EID = CommonFunctions.EncryptUrlParam(x.ID)
                                                                     }
                                                                    );

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
        /// Created By:
        /// Creation On:
        /// Description: method to get add/edit
        /// </summary>
        [SessionExpireFilterAttribute]
        public ActionResult EditMainTemplate(string eid)
        {
            try
            {
                long id = CommonFunctions.DecryptUrlParam(eid);


                AccessModule(ModuleValues.EmailTemplates);

                var emailObj = new EmailTemplate();
                var lst = new List<GF_EmailTemplates>();
                var lstfields = new List<GF_EmailTemplatesFields>();
                lst = emailObj.GetMainTemplatesDetail(id, ref lstfields);
                ViewBag.fields = lstfields;
                var objModel = new GF_EmailTemplates();
                objModel.MessageBody = lst[0].MessageBody;
                objModel.TemplateName = lst[0].TemplateName;
                objModel.MessageBodyOriginal = lst[0].MessageBody;
                objModel.ID = lst[0].ID;
                return View(objModel);
            }
            catch (Exception ex)
            {
                ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
                return null;
            }

        }

        [ValidateInput(false)]
        public ActionResult UpdateMainEmailTemplate(long id, string content)
        {

            AccessModule(ModuleValues.EmailTemplates);

            var temp = new EmailTemplate();
            var status = temp.UpdateMainEmailTemplate(id, content);
            if (status)
                TempData["Message"] = string.Format("Email Template update successfully.");
            return Json(new { msg = status });
        }

        #endregion

        #region Manage Missing Orders List

        /// <summary>
        /// Created By: Amit Kumar
        /// Creation On: 28 March 2015
        /// Description: method for list
        /// </summary>
        [SessionExpireFilterAttribute]
        public ActionResult MissingOrdersList()
        {
            AccessModule(ModuleValues.MissedOrders);
            //if (Golfler.Models.LoginInfo.Type == Golfler.Models.UserType.Kitchen || Golfler.Models.LoginInfo.Type == Golfler.Models.UserType.Cartie)
            //{
            //    AccessModule(ModuleValues.AllRights);
            //}
            //else
            //{
            //    AccessModule(ModuleValues.OrderHistory);
            //}
            ViewBag.Message = TempData["Message"];
            return View();
        }

        /// <summary>
        /// Created By: veera
        /// Creation On: 18 May- 2015
        /// Description: method to bind grid
        /// </summary>
        [SessionExpireFilterAttribute]
        public ActionResult GetMissingOrdersList(string searchText, string fromDate, string toDate, string type, string sidx, string sord, int? page, int? rows)
        {
            try
            {
                var OrderObj = new Order();
                var totalRecords = 0;

                string timeZone = CommonFunctions.GetCourseTimeZone(LoginInfo.CourseId);


                var rec = OrderObj.GetMissingOrders(searchText, fromDate, toDate, type, sidx,
                                           sord, page ?? 1, rows ?? 10,
                                           ref totalRecords).AsEnumerable();

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

        #region Employee Analytics

        /// <summary>
        /// Created By: Veera
        /// Creation On: 25 May, 2015
        /// Description: method for Employee Analytics
        /// </summary>
        [SessionExpireFilter]
        public ActionResult EmployeeAnalytics()
        {
            AccessModule(ModuleValues.EmployeeReport);

            return View();
        }
        public ActionResult EmployeeAnalyticsSearch(string type, string name, string fromdate, string todate)
        {
            AccessModule(ModuleValues.AllRights);
            try
            {

                Course obj = new Course();
                EmployeeAnalyticsSearchResult objResult = obj.EmployeeAnalyticsSearch(LoginInfo.CourseId, type, name, fromdate, todate);
                return Json(new { result = objResult });


            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                return Json(new { result = "0" });
            }
        }
        public bool ExportToExcelEmployeeReport(string type, string name, string fromdate, string todate, string reportType)
        {
            Course obj = new Course();

            if (reportType == "0")
            {
                var rptExport = obj.EmployeeAnalyticsSearch(LoginInfo.CourseId, type, name, fromdate, todate).EmployeeSearch;
                var fileName = "EmployeeComparativeReport_" + DateTime.Now.Ticks.ToString();

                var data = from x in rptExport
                           select new
                           {
                               x.PriceMe,
                               x.PriceCourse,
                               x.NoOfOrdersMe,
                               x.NoOfOrdersCourse,
                               x.RatingMe,
                               x.RatingCourse
                           };

                WebGrid grid = new WebGrid(source: data, canPage: false, canSort: false);

                string gridData = grid.GetHtml(
                    columns: grid.Columns(

                            grid.Column("PriceMe", "Price Me"),
                            grid.Column("PriceCourse", "Price Course"),
                            grid.Column("NoOfOrdersMe", "No Of Orders Me"),
                            grid.Column("NoOfOrdersCourse", "No Of Orders Course"),
                            grid.Column("RatingMe", "Rating Me"),
                            grid.Column("RatingCourse", "Rating Course")
                            )
                        ).ToString();

                Response.ClearContent();
                Response.AddHeader("content-disposition", "attachment; filename=" + fileName + ".xls");
                Response.ContentType = "application/ms-excel";
                Response.Write(gridData);
                Response.End();
                return true;
            }

            else if (reportType == "1")
            {
                var rptExport = obj.EmployeeAnalyticsSearch(LoginInfo.CourseId, type, name, fromdate, todate).EmployeePersonalSearch;
                var fileName = "EmployeePersonalReport_" + DateTime.Now.Ticks.ToString();

                var data = from x in rptExport
                           select new
                           {
                               x.PriceMe,

                               x.NoOfOrdersMe,

                               x.RatingMe,
                               Title = x.InnerTitle
                           };

                WebGrid grid = new WebGrid(source: data, canPage: false, canSort: false);

                string gridData = grid.GetHtml(
                    columns: grid.Columns(

                            grid.Column("PriceMe", "Price Me"),

                            grid.Column("NoOfOrdersMe", "No Of Orders Me"),

                            grid.Column("RatingMe", "Rating Me"),
                            grid.Column("Title", "Title")
                            )
                        ).ToString();

                Response.ClearContent();
                Response.AddHeader("content-disposition", "attachment; filename=" + fileName + ".xls");
                Response.ContentType = "application/ms-excel";
                Response.Write(gridData);
                Response.End();
                return true;
            }

            else
            {
                return false;
            }
        }

        #endregion

        #region Golfer Playing History
        /// <summary>
        /// Created By: Ramesh Kalra
        /// Creation On: 26 May, 2015
        /// Description: Golfer playing history Report
        /// </summary>
        /// <returns></returns>
        [SessionExpireFilter]
        public ActionResult PlayingHistory()
        {
            AccessModule(ModuleValues.GolfPlayingHistoryReport);

            return View();
        }

        [SessionExpireFilterAttribute]
        public ActionResult GetGolpherforplayinghistory(string searchText, string CourseId, string HistoryFrom, string HistoryTo, string CompareParameter, string RangeParameter, string sidx, string sord, int? page, int? rows)
        {
            try
            {
                GolflerEntities Db = new GolflerEntities();
                GF_Golfer obj = new GF_Golfer();

                Int64 intCourseId = Convert.ToInt64(CourseId);
                //try
                //{
                //    if (!string.IsNullOrEmpty(Convert.ToString(CourseId)))
                //    {
                //        intCourseId = Convert.ToInt64(CourseId);
                //    }
                //}
                //catch
                //{
                //    intCourseId = 0;
                //}
                //if (intCourseId == 0)
                //{
                //    intCourseId = LoginInfo.CourseId;
                //}


                var totalRecords = 0;
                var list = obj.GetGolphersbyCourseForPlayingHistory(searchText, intCourseId, HistoryFrom, HistoryTo, CompareParameter, RangeParameter, "", "", sidx, sord, page ?? 1, rows ?? 10, ref totalRecords);

                if (list != null)
                {
                    var jsonData = new
                    {
                        total = (totalRecords + rows - 1) / rows,
                        page,
                        records = totalRecords,
                        rows = list.ToList(),
                        Id = "GolpherId"
                    };
                    return Json(jsonData, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
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
        [SessionExpireFilter]
        public ActionResult GolferPlayingHistory(Int64 GolferId, Int64 CourseId)
        {
            AccessModule(ModuleValues.AllRights);
            Db = new GolflerEntities();

            string GolferName = "";
            string GolferEmail = "";
            if (!string.IsNullOrEmpty(Convert.ToString(GolferId)))
            {
                var objGolfer = Db.GF_Golfer.Where(x => x.GF_ID == GolferId).FirstOrDefault();
                if (objGolfer != null)
                {
                    GolferName = objGolfer.FirstName + " " + objGolfer.LastName;
                    GolferEmail = objGolfer.Email;
                }
            }
            ViewBag.GolferName = GolferName;
            ViewBag.GolferEmail = GolferEmail;
            ViewBag.GolferId = GolferId;
            ViewBag.CourseId = CourseId;

            return PartialView("~/Views/Shared/_GolferPlayingHistory.cshtml");
        }


        [SessionExpireFilterAttribute]
        public ActionResult GetGolpherPlayingHistory(string searchText, string HistoryFrom, string HistoryTo, Int64 golferid, string CourseId, string sidx, string sord, int? page, int? rows)
        {
            try
            {
                GolflerEntities Db = new GolflerEntities();
                GF_Golfer obj = new GF_Golfer();

                Int64 intCourseId = Convert.ToInt64(CourseId);
                //try
                //{
                //    if (!string.IsNullOrEmpty(Convert.ToString(CourseId)))
                //    {
                //        intCourseId = Convert.ToInt64(CourseId);
                //    }
                //}
                //catch
                //{
                //    intCourseId = 0;
                //}
                //if (intCourseId == 0)
                //{
                //    intCourseId = LoginInfo.CourseId;
                //}

                var totalRecords = 0;
                var list = obj.GetGolpherPlayingHistory(searchText, HistoryFrom, HistoryTo, intCourseId, golferid, "", sidx, sord, page ?? 1, rows ?? 10, ref totalRecords, false);

                if (list != null)
                {
                    var jsonData = new
                    {
                        total = (totalRecords + rows - 1) / rows,
                        page,
                        records = totalRecords,
                        rows = list.ToList(),
                        Id = "CourseId"
                    };
                    return Json(jsonData, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
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

        [SessionExpireFilterAttribute]
        public ActionResult GetGolpherPlayingHistoryForCourse(string searchText, string HistoryFrom, string HistoryTo, Int64 golferid, string CourseId, string sidx, string sord, int? page, int? rows)
        {
            try
            {
                GolflerEntities Db = new GolflerEntities();
                GF_Golfer obj = new GF_Golfer();

                Int64 intCourseId = Convert.ToInt64(CourseId);

                var totalRecords = 0;
                var list = obj.GetGolpherPlayingHistoryForCourse(searchText, HistoryFrom, HistoryTo, intCourseId, golferid, "", sidx, sord, page ?? 1, rows ?? 10, ref totalRecords, false);

                if (list != null)
                {
                    var jsonData = new
                    {
                        total = (totalRecords + rows - 1) / rows,
                        page,
                        records = totalRecords,
                        rows = list.ToList(),
                        Id = "CourseId"
                    };
                    return Json(jsonData, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
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


        [SessionExpireFilterAttribute]
        public bool ExportToExcelPlayingHistory(Int64 golferid, Int64 CourseId, string fromdate, string todate, string reportType)
        {
            #region old code
            try
            {

                GolflerEntities Db = new GolflerEntities();
                GF_Golfer obj = new GF_Golfer();



                var totalRecords = 0;
                var list = obj.GetGolpherPlayingHistory("", fromdate, todate, CourseId, golferid, "", "GolpherId", "desc", 1, 10, ref totalRecords, true);

                var fileName = "GolferPlayingHistoryReport_" + DateTime.Now.Ticks.ToString();

                var data = from x in list
                           select new
                           {
                               x.GolpherName,
                               x.CourseName,
                               x.TotalPlay,
                               x.OneHoleAvgTime,
                               x.NineHoleAvgTime,
                               x.EighteenHoleAvgTime,
                               x.LastGameDate
                           };

                WebGrid grid = new WebGrid(source: data, canPage: false, canSort: false);

                if (reportType == "1")
                {
                    string gridData = grid.GetHtml(
                        columns: grid.Columns(

                                grid.Column("GolpherName", "Golfer Name"),
                                grid.Column("CourseName", "Course Name"),
                                grid.Column("TotalPlay", "No. of time played"),
                                grid.Column("OneHoleAvgTime", "Avg. time for one hole"),
                                grid.Column("NineHoleAvgTime", "Avg. time for 9 holes"),
                                grid.Column("EighteenHoleAvgTime", "Avg. time for 18 holes"),
                                  grid.Column("LastGameDate", "Last game date")
                                )
                            ).ToString();
                    Response.ClearContent();
                    Response.AddHeader("content-disposition", "attachment; filename=" + fileName + ".xls");
                    Response.ContentType = "application/ms-excel";
                    Response.Write(gridData);
                    Response.End();

                }
                else
                {
                    string gridData = grid.GetHtml(
                           columns: grid.Columns(

                                   grid.Column("GolpherName", "Golfer Name"),
                                   grid.Column("CourseName", "Course Name"),
                                   grid.Column("TotalPlay", "No. of time played")
                                   )
                               ).ToString();
                    Response.ClearContent();
                    Response.AddHeader("content-disposition", "attachment; filename=" + fileName + ".xls");
                    Response.ContentType = "application/ms-excel";
                    Response.Write(gridData);
                    Response.End();
                }



                return true;

            }
            catch (Exception ex)
            {
                ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
                return false;
            }
            #endregion
        }

        [HttpPost]
        [SessionExpireFilterAttribute]
        public ActionResult GetPlayingHistoryForGraph(string golferid, string HistoryFrom, string HistoryTo)
        {
            AccessModule(ModuleValues.AllRights);
            string msg = "";
            List<object[]> data = new List<object[]>();
            try
            {

                data.Add(new object[] { "Course", "Number of times played" });

                CommonFunctions.GetGolferPlayGraph(golferid, HistoryFrom, HistoryTo, ref data, ref msg);


            }
            catch (Exception ex)
            {
                msg = ex.Message;
            }

            JavaScriptSerializer objJS = new JavaScriptSerializer();
            return Json(new { result = objJS.Serialize(data), msg = msg });
        }

        public ActionResult ShowGolferHistoryForCourseAdmin(Int64 courseid, Int64 golferid, string HistoryFrom, string HistoryTo)
        {
            //ViewBag.Host = host;
            //ViewBag.Password = password;
            //ViewBag.Port = port;
            //ViewBag.From = from;
            //ViewBag.UserName = username;
            //ViewBag.EnableSsl = EnableSsl;
            //ViewBag.EnableTls = EnableTls;
            Db = new GolflerEntities();
            ViewBag.golferid = golferid;
            ViewBag.courseid = courseid;
            ViewBag.coursename = Db.GF_CourseInfo.FirstOrDefault(x => x.ID == courseid).COURSE_NAME;
            ViewBag.golfername = Db.GF_Golfer.FirstOrDefault(x => x.GF_ID == golferid).FirstName + " " + Db.GF_Golfer.FirstOrDefault(x => x.GF_ID == golferid).LastName;
            ViewBag.HistoryFrom = HistoryFrom;
            ViewBag.HistoryTo = HistoryTo;

            return PartialView("~/Views/Shared/_GolferPlayingHistoryInCourse.cshtml");
        }
        #endregion

        #region  Food items Report
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [SessionExpireFilterAttribute]
        public ActionResult FoodItemsreport()
        {
            AccessModule(ModuleValues.FoodItemsReport);
            var subCategory = new GF_SubCategory();

            ViewBag.CategoryList = new SelectList(subCategory.GetCategoryList(), "ID", "Name");
            return View();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="trend"></param>
        /// <param name="sidx"></param>
        /// <param name="sord"></param>
        /// <param name="page"></param>
        /// <param name="rows"></param>
        /// <returns></returns>
        [SessionExpireFilterAttribute]
        public ActionResult GetHotItems(string trend, string trendin, string catID, string sidx, string sord, int? page, int? rows)
        {
            try
            {
                GolflerEntities Db = new GolflerEntities();
                Course obj = new Course();


                var totalRecords = 0;
                var list = obj.GetHotMenuItems(trend, trendin, catID, sidx, sord, page ?? 1, rows ?? 10, ref totalRecords);

                if (list != null)
                {
                    var jsonData = new
                    {
                        total = (totalRecords + rows - 1) / rows,
                        page,
                        records = totalRecords,
                        rows = list.ToList(),
                        Id = "ID"
                    };
                    return Json(jsonData, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(null, JsonRequestBehavior.AllowGet);
                }
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="range"></param>
        /// <param name="fromdate"></param>
        /// <param name="todate"></param>
        /// <returns></returns>
        [SessionExpireFilterAttribute]
        public ActionResult GetFoodItemGraphData(int range, string fromdate, string todate, string category, string subcategory)
        {

            category = string.IsNullOrEmpty(category) ? "0" : category;
            subcategory = string.IsNullOrEmpty(subcategory) ? "0" : subcategory;


            var objCourse = new Course();
            var column = new string[100];
            var column1 = new string[100];
            var courseName = new string[100];
            var courseName1 = new string[100];
            JavaScriptSerializer obj = new JavaScriptSerializer();
            var lstresult = objCourse.GetFoodItemsData(range, fromdate, todate, ref column, ref courseName, Convert.ToInt64(category), Convert.ToInt64(subcategory));
            var lstresult1 = objCourse.GetFoodItemsDataLineChart(range, fromdate, todate, ref column1, ref courseName1, Convert.ToInt64(category), Convert.ToInt64(subcategory));

            var result = "";
            List<object[]> data = new List<object[]>();
            data.Add(column);
            foreach (object[] arr in lstresult)
            {
                data.Add(arr);
            }
            List<object[]> data1 = new List<object[]>();

            data1.Add(column1);

            foreach (object[] arr in lstresult1)
            {
                data1.Add(arr);
            }
            if (lstresult.Count == 0)
                result = "failed";
            else
            {
                result = obj.Serialize(data);
            }

            return Json(new { result, lineresult = obj.Serialize(data1) });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="range"></param>
        /// <param name="fromdate"></param>
        /// <param name="todate"></param>
        /// <param name="category"></param>
        /// <param name="subcategory"></param>
        /// <returns></returns>
        [SessionExpireFilterAttribute]
        public ActionResult ExportToExcelGraphReport(int range, string fromdate, string todate, string category, string subcategory)
        {

            category = string.IsNullOrEmpty(category) ? "0" : category;
            subcategory = string.IsNullOrEmpty(subcategory) ? "0" : subcategory;

            var objCourse = new Course();
            var column = new string[100];
            var courseName = new string[100];

            var result = objCourse.GetFoodItemsDataExcelExport(range, fromdate, todate, ref column, ref courseName, Convert.ToInt64(category), Convert.ToInt64(subcategory));


            StringBuilder sb = new StringBuilder();
            //static file name, can be changes as per requirement
            string sFileName = "FoodItemreport_" + DateTime.Now.Ticks + ".xls";
            //Bind data list from edmx
            var Data = result.ToList();
            if (Data != null && Data.Any())
            {
                sb.Append("<table style='font-size:16px;fornt-weight:bold;' Border='1'>");
                sb.Append("<tr>");
                for (int i = 0; i < column.Length; i++)
                {
                    sb.Append("<td style='width:120px;'><b>" + column[i] + "</b></td>");
                }


                sb.Append("</tr>");
                foreach (object[] item in result)
                {
                    sb.Append("<tr>");
                    for (int i = 0; i < item.Length; i++)
                    {
                        sb.Append("<td>" + item[i].ToString() + "</td>");

                    }
                    sb.Append("</tr>");
                }
            }
            HttpContext.Response.AddHeader("content-disposition", "attachment; filename=" + sFileName);
            this.Response.ContentType = "application/vnd.ms-excel";
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
            return File(buffer, "application/vnd.ms-excel");

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="range"></param>
        /// <param name="fromdate"></param>
        /// <param name="todate"></param>
        /// <param name="category"></param>
        /// <param name="subcategory"></param>
        /// <returns></returns>
        [SessionExpireFilterAttribute]
        public ActionResult ExportToExcelLineGraphReport(int range, string fromdate, string todate, string category, string subcategory)
        {

            category = string.IsNullOrEmpty(category) ? "0" : category;
            subcategory = string.IsNullOrEmpty(subcategory) ? "0" : subcategory;

            var objCourse = new Course();
            var column = new string[100];
            var courseName = new string[100];

            var result = objCourse.GetFoodItemsDataLineChart(range, fromdate, todate, ref column, ref courseName, Convert.ToInt64(category), Convert.ToInt64(subcategory));


            StringBuilder sb = new StringBuilder();
            //static file name, can be changes as per requirement
            string sFileName = "FoodItemreportLine_" + DateTime.Now.Ticks + ".xls";
            //Bind data list from edmx
            var Data = result.ToList();
            if (Data != null && Data.Any())
            {
                sb.Append("<table style='font-size:16px;fornt-weight:bold;' Border='1'>");
                sb.Append("<tr>");
                for (int i = 0; i < column.Length; i++)
                {
                    sb.Append("<td style='width:120px;'><b>" + column[i] + "</b></td>");
                }

                sb.Append("</tr>");
                foreach (object[] item in result)
                {
                    sb.Append("<tr>");
                    for (int i = 0; i < item.Length; i++)
                    {
                        sb.Append("<td>" + item[i].ToString() + "</td>");
                    }
                    sb.Append("</tr>");
                }
            }
            HttpContext.Response.AddHeader("content-disposition", "attachment; filename=" + sFileName);
            this.Response.ContentType = "application/vnd.ms-excel";
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
            return File(buffer, "application/vnd.ms-excel");

        }

        #endregion

        public ActionResult BlockUsers(string BlockedUserId, bool IsBlockedGolfer, string Block)
        {

            AccessModule(ModuleValues.AllRights);
            Course objCourse = new Course();
            try
            {
                long _userId = LoginInfo.UserId;
                long _blockedUserId = Convert.ToInt64(BlockedUserId.Trim());
                string result = objCourse.BlockNUnblockUser(_userId, _blockedUserId, IsBlockedGolfer, Block);
                return Json(new { result = result });



            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                return Json(new { result = "-1", });
            }
            finally
            {
                objCourse = null;
            }
        }

        #region Manage MembershipIds
        /// <summary>
        /// Created By:veera
        /// Created on: 4 June 2015
        /// Description: method for manage membership
        /// </summary>
        [SessionExpireFilterAttribute]
        public ActionResult MembershipList()
        {
            AccessModule(ModuleValues.managemembershipidnumber);
            ViewBag.Message = TempData["Message"];

            return View();
        }
        /// <summary>
        /// Created By: veera
        /// Creation On:  4 June 2015
        /// Description: method to bind grid
        /// </summary>
        [SessionExpireFilterAttribute]
        public ActionResult GetMembershipList(string searchText, string sidx, string sord, int? page, int? rows)
        {

            try
            {
                GolflerEntities Db = new GolflerEntities();
                Course obj = new Course();

                var totalRecords = 0;
                var list = obj.GetCourseMembershipList(searchText, sidx, sord, page ?? 1, rows ?? 10, ref totalRecords).
                  Select((x =>
                                                                     new
                                                                     {
                                                                         ID = x.ID,
                                                                         Name = x.FirstName + " " + x.LastName,
                                                                         MemberShipId = x.MemberShipId,
                                                                         x.Email,
                                                                         x.CreatedDate,
                                                                         EID = CommonFunctions.EncryptUrlParam(x.ID)
                                                                     }));
                var jsonData = new
                {
                    total = (totalRecords + rows - 1) / rows,
                    page,
                    records = totalRecords,
                    rows = list.ToList(),
                    ID = "ID"

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

        /// <summary>
        /// Created By:
        /// Creation On:
        /// Description: method to get add/edit
        /// </summary>
        [SessionExpireFilterAttribute]
        public ActionResult MembershipAddEdit(string eid)
        {
            long id = CommonFunctions.DecryptUrlParam(eid);

            AccessModule(ModuleValues.managemembershipidnumber);
            Course objCourse = new Course();
            GF_CourseMemberShip obj = objCourse.GetMemberShip(id);
            if (id > 0)
            {
                ViewBag.Module = "Edit Membership";
            }
            else
            {
                ViewBag.Module = "Add Membership";
            }
            return View(obj);
        }

        /// <summary>
        /// Created By:
        /// Creation On:
        /// Description: method to get add/edit
        /// </summary>
        [HttpPost]
        [SessionExpireFilterAttribute]
        public ActionResult MembershipAddEdit(string eid, GF_CourseMemberShip obj)
        {
            long id = CommonFunctions.DecryptUrlParam(eid);



            AccessModule(ModuleValues.managemembershipidnumber);
            var isSaved = false;
            var Obj = new Course();

            if (ModelState.IsValid)
            {
                isSaved = Obj.SaveMembership(obj);
            }
            if (isSaved)
            {
                string module = "Membership";
                TempData["Message"] = CommonFunctions.Message(Obj.Message, module);
                return RedirectToAction("MembershipList");
            }

            ViewBag.ErrorMessage = Obj.Message;

            return View(obj);
        }

        /// <summary>
        /// Created By:VEERA
        /// Creation On: 4 JUNE 2015
        /// Description: method to delete
        /// </summary>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult DeleteMemberShip(List<long> ids)
        {
            try
            {
                AccessModule(ModuleValues.managemembershipidnumber);

                if (ids != null)
                {
                    var Obj = new Course();
                    return Obj.DeleteMembership(ids)
                               ? Json(new { statusText = "success", module = "Membership", task = "delete", errormessage = Obj.Message })
                               : Json(new { statusText = "error", module = "Membership", task = "delete", message = Obj.Message });
                }
                return Json(new { statusText = "error", module = "Membership", task = "select" });
            }
            catch (Exception ex)
            {
                ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
                return null;
            }

        }
        #endregion

        #region BT SubMerchantID Creation

        public ActionResult CreateBTSubMerchantId(string IndividualFirstName,
            string IndividualLastName,
            string IndividualEmail,
            string IndividualPhone,
            string IndividualDateOfBirth,
            string IndividualSsn,
            string IndividualStreetAddress,
            string IndividualLocality,
            string IndividualRegion,
            string IndividualPostalCode,
            string BusinessLegalName,
            string BusinessDbaName,
            string BusinessTaxId,
            string BusinessStreetAddress,
            string BusinessLocality,
            string BusinessRegion,
            string BusinessPostalCode,
            string FundingDescriptor,
            string FundingEmail,
            string FundingMobilePhone,
            string FundingAccountNumber,
            string FundingRoutingNumber

            )
        {

            AccessModule(ModuleValues.ManageSettings);

            try
            {
                Int64 courseid = Convert.ToInt64(LoginInfo.CourseId);
                if (courseid > 0)
                {
                    var dbCheck = new GolflerEntities();
                    //var courseSettings = dbCheck.GF_Settings.Where(x => x.CourseID == courseid && x.x.Name == "BTSubMerchantId").FirstOrDefault();

                    string strBTSubMerchantId = Convert.ToString(dbCheck.GF_Settings.Where(x => x.CourseID == courseid && x.Name == "BTSubMerchantId").FirstOrDefault().Value);
                    if (!string.IsNullOrEmpty(Convert.ToString(strBTSubMerchantId)))
                    {
                        return Json(new { result = "0", msg = "Sub-Merchant ID is already exists in database." });
                    }
                    else
                    {
                        string strPublicKey = "";
                        string strPrivateKey = "";
                        string strMerchantId = "";
                        string strMasterMerchantAccountId = "";
                        Braintree.Environment strEnviroment = Braintree.Environment.SANDBOX;
                        if (Convert.ToString(System.Web.Configuration.WebConfigurationManager.AppSettings["PAYMENT_MODE"]) == "L")
                        {
                            strEnviroment = Braintree.Environment.PRODUCTION;
                            strPublicKey = Convert.ToString(ConfigurationManager.AppSettings["BTPublicKey_Live"]);
                            strPrivateKey = Convert.ToString(ConfigurationManager.AppSettings["BTPrivateKey_Live"]);
                            strMerchantId = Convert.ToString(ConfigurationManager.AppSettings["BTMerchantId_Live"]);
                            strMasterMerchantAccountId = Convert.ToString(ConfigurationManager.AppSettings["BTMasterMerchantAccountId_Live"]);
                        }
                        else
                        {
                            strEnviroment = Braintree.Environment.SANDBOX;
                            strPublicKey = Convert.ToString(ConfigurationManager.AppSettings["BTPublicKey"]);
                            strPrivateKey = Convert.ToString(ConfigurationManager.AppSettings["BTPrivateKey"]);
                            strMerchantId = Convert.ToString(ConfigurationManager.AppSettings["BTMerchantId"]);
                            strMasterMerchantAccountId = Convert.ToString(ConfigurationManager.AppSettings["BTMasterMerchantAccountId"]);
                        }

                        BraintreeGateway Gateway = new BraintreeGateway
                        {
                            Environment = strEnviroment,
                            PublicKey = strPublicKey,
                            PrivateKey = strPrivateKey,
                            MerchantId = strMerchantId
                        };

                        string result = "";
                        MerchantAccountRequest request = new MerchantAccountRequest
                        {
                            Individual = new IndividualRequest
                            {
                                FirstName = IndividualFirstName,
                                LastName = IndividualLastName,
                                Email = IndividualEmail,
                                Phone = IndividualPhone,
                                DateOfBirth = IndividualDateOfBirth,
                                Ssn = IndividualSsn,
                                Address = new AddressRequest
                                {
                                    StreetAddress = IndividualStreetAddress,
                                    Locality = IndividualLocality,
                                    Region = IndividualRegion,
                                    PostalCode = IndividualPostalCode
                                }
                            },
                            Business = new BusinessRequest
                            {
                                LegalName = BusinessLegalName,
                                DbaName = BusinessDbaName,
                                TaxId = BusinessTaxId,
                                Address = new AddressRequest
                                {
                                    StreetAddress = BusinessStreetAddress,
                                    Locality = BusinessLocality,
                                    Region = BusinessRegion,
                                    PostalCode = BusinessPostalCode
                                }
                            },
                            //

                            //
                            Funding = new FundingRequest
                            {
                                //Destination=   FundingDestination.EMAIL,
                                // Email= "amitkumar@cogniter.com"

                                Descriptor = FundingDescriptor,
                                Destination = FundingDestination.BANK,
                                Email = FundingEmail,
                                MobilePhone = FundingMobilePhone,
                                AccountNumber = FundingAccountNumber,
                                RoutingNumber = FundingRoutingNumber
                            },
                            TosAccepted = true,
                            MasterMerchantAccountId = strMasterMerchantAccountId
                            //  Id = "Golfler_store"
                        };

                        Result<MerchantAccount> resultSubmId = Gateway.MerchantAccount.Create(request);

                        if (resultSubmId != null)
                        {
                            MerchantAccount ma = resultSubmId.Target;
                            if (ma != null)
                            {
                                if (ma.Status == MerchantAccountStatus.SUSPENDED)
                                {
                                    // update database
                                    var lstBTSubMerchantId = dbCheck.GF_Settings.Where(x => x.CourseID == courseid && x.Name == "BTSubMerchantId").ToList();
                                    foreach (var objMerchant in lstBTSubMerchantId)
                                    {
                                        objMerchant.Value = "";
                                        objMerchant.ModifyBy = Convert.ToString(LoginInfo.UserId);
                                        objMerchant.ModifyDate = DateTime.Now;
                                        dbCheck.SaveChanges();
                                    }

                                    return Json(new { result = "0", msg = "Your account has been suspended from Braintree." });
                                }
                                else
                                {
                                    bool isInsert = false;
                                    // update database 
                                    var lstBTSubMerchantId = dbCheck.GF_Settings.Where(x => x.CourseID == courseid && x.Name == "BTSubMerchantId").ToList();
                                    if (lstBTSubMerchantId != null)
                                    {
                                        if (lstBTSubMerchantId.Count > 0)
                                        {
                                            isInsert = true;
                                            foreach (var objMerchant in lstBTSubMerchantId)
                                            {
                                                objMerchant.Value = ma.Id;
                                                objMerchant.ModifyBy = Convert.ToString(LoginInfo.UserId);
                                                objMerchant.ModifyDate = DateTime.Now;
                                                dbCheck.SaveChanges();
                                            }
                                        }
                                    }
                                    if (!isInsert)
                                    {
                                        GF_Settings objSettings = new GF_Settings();
                                        objSettings.Name = "BTSubMerchantId";
                                        objSettings.Value = ma.Id;
                                        objSettings.Status = StatusType.Active;
                                        objSettings.IsActive = true;
                                        objSettings.CreatedBy = Convert.ToString(LoginInfo.UserId);
                                        objSettings.CreatedDate = DateTime.Now;
                                        objSettings.CourseID = courseid;

                                        dbCheck.GF_Settings.Add(objSettings);
                                        dbCheck.SaveChanges();
                                    }

                                    return Json(new { result = "1", msg = ma.Id });
                                }
                            }
                            else
                            {
                                string msg = Convert.ToString(resultSubmId.Message);
                                if (string.IsNullOrEmpty(Convert.ToString(msg)))
                                {
                                    msg = "Error returned from Braintree.";
                                }
                                return Json(new { result = "0", msg = msg });
                            }
                        }
                        else
                        {
                            string msg = Convert.ToString(resultSubmId.Message);
                            if (string.IsNullOrEmpty(Convert.ToString(msg)))
                            {
                                msg = "Error returned from Braintree.";
                            }
                            return Json(new { result = "0", msg = msg });
                        }

                        //  return Json(new { result = result, msg = msg });
                    }
                }
                else
                {
                    return Json(new { result = "0", msg = "Course not available." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { result = "0", msg = ex.Message });
            }
        }

        #endregion

        public ActionResult GetChartResult(int Type)
        {
            AccessModule(ModuleValues.GopherView);
            try
            {

                #region Golfer Chart

                Course objCourse = new Course();
                CourseVisitNRevenueResult objResult = new CourseVisitNRevenueResult();
                objResult = objCourse.GetCourseSiteVisitsNRevenueById(LoginInfo.CourseId, Type, 1);
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


            }
        }
        public ActionResult GetRevenueResult(int Type)
        {
            AccessModule(ModuleValues.AllRights);
            try
            {

                #region Golfer Chart

                Course objCourse = new Course();
                CourseVisitNRevenueResult objResult = new CourseVisitNRevenueResult();
                objResult = objCourse.GetCourseSiteVisitsNRevenueById(LoginInfo.CourseId, Type, 2);
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


            }
        }

        #region Golfer Order History Report

        /// <summary>
        /// Created By: Amit Kumar
        /// Created Date: 20 August 2015
        /// Description: Golfer order history report which include:
        ///                 - Order History of Golfer User
        ///                 - Comparative analysis of Game Play and Food Order
        ///                 - Rating and Complaints
        /// </summary>
        /// <returns></returns>
        [SessionExpireFilterAttribute]
        public ActionResult GolferOrderHistoryReport()
        {
            AccessModule(ModuleValues.GolferOrderHistoryReport);

            return View();
        }

        /// <summary>
        /// Created By: Amit Kumar
        /// Creation On: 20 August 2015
        /// Description: method to bind grid
        /// </summary>
        [SessionExpireFilterAttribute]
        public ActionResult GetGolferOrderList(string searchText, string fromDate, string toDate, string category, string subCategory,
            string menuItem, string viewIn, string sidx, string sord, int? page, int? rows)
        {
            try
            {
                AccessModule(ModuleValues.GolferOrderHistoryReport);

                var OrderObj = new Order();
                var totalRecords = 0;

                string timeZone = CommonFunctions.GetCourseTimeZone(LoginInfo.CourseId);

                var rec = OrderObj.GetGolferOrdersByCourse(searchText, fromDate, toDate, category, subCategory, menuItem, viewIn,
                                                           sidx, sord, page ?? 1, rows ?? 10,
                                                           ref totalRecords).AsEnumerable();

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
        /// Created By: Amit Kumar
        /// Creation On: 21 August 2015
        /// </summary>
        [SessionExpireFilterAttribute]
        [HttpGet]
        public ActionResult GetComparativeOrderGame(string golferEmail, DateTime DateFrom, DateTime DateTo, string viewIN)
        {
            AccessModule(ModuleValues.GolferOrderHistoryReport);

            Order ob = new Order();
            string golferName = "";
            var lstOrderGame = ob.GetComparativeOrderGame(golferEmail, DateFrom, DateTo, viewIN, ref golferName);

            TempData["GolferName"] = golferName;

            return Json(lstOrderGame, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Created By: Amit Kumar
        /// Creation On: 21 August 2015
        /// </summary>
        [SessionExpireFilterAttribute]
        [HttpGet]
        public ActionResult GetComparativeRatingComplaints(string golferEmail, DateTime DateFrom, DateTime DateTo, string viewIN)
        {
            AccessModule(ModuleValues.GolferOrderHistoryReport);

            Order ob = new Order();
            var lstRatingComplaints = ob.GetComparativeRatingComplaints(golferEmail, DateFrom, DateTo, viewIN);

            return Json(lstRatingComplaints, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Created By: Amit Kumar
        /// Creation On: 21 Sep 2015
        /// </summary>
        [SessionExpireFilterAttribute]
        [HttpGet]
        public ActionResult GetComparativeOrderWeather(string golferEmail, DateTime DateFrom, DateTime DateTo, string viewIN)
        {
            AccessModule(ModuleValues.GolferOrderHistoryReport);

            Order ob = new Order();
            var lstRatingComplaints = ob.GetComparativeOrderWeather(golferEmail, DateFrom, DateTo, viewIN);

            return Json(lstRatingComplaints, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Created By: Amit Kumar
        /// Creation On: 24 August 2015
        /// </summary>
        [SessionExpireFilterAttribute]
        public ActionResult ExportComparativeOrderGame(string golferEmail, DateTime DateFrom, DateTime DateTo, string viewIN)
        {
            AccessModule(ModuleValues.GolferOrderHistoryReport);

            Order ob = new Order();
            var column = new string[3];
            var courseName = new string[3];
            string golferName = "";
            var result = ob.GetComparativeOrderGame(golferEmail, DateFrom, DateTo, viewIN, ref golferName);

            StringBuilder sb = new StringBuilder();
            //static file name, can be changes as per requirement
            string sFileName = "ComparativeOrderGame_" + DateTime.Now.Ticks + ".xls";
            //Bind data list from edmx
            var Data = result.ToList();

            if (Data != null && Data.Any())
            {
                sb.Append("<table style='font-size:16px;fornt-weight:bold;' Border='1'>");

                int headRow = 0;
                sb.Append("</tr>");
                foreach (object[] item in result)
                {
                    sb.Append("<tr>");

                    if (headRow == 0)
                    {
                        for (int i = 0; i < item.Length; i++)
                        {
                            sb.Append("<th>" + item[i].ToString() + "</th>");
                        }
                        headRow++;
                    }
                    else
                    {
                        for (int i = 0; i < item.Length; i++)
                        {
                            sb.Append("<td>" + item[i].ToString() + "</td>");
                        }
                    }

                    sb.Append("</tr>");
                }
                sb.Append("</table>");
            }

            HttpContext.Response.AddHeader("content-disposition", "attachment; filename=" + sFileName);
            this.Response.ContentType = "application/vnd.ms-excel";
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
            return File(buffer, "application/vnd.ms-excel");

        }

        /// <summary>
        /// Created By: Amit Kumar
        /// Creation On: 24 August 2015
        /// </summary>
        [SessionExpireFilterAttribute]
        public ActionResult ExportComparativeRatingComplaints(string golferEmail, DateTime DateFrom, DateTime DateTo, string viewIN)
        {
            AccessModule(ModuleValues.GolferOrderHistoryReport);

            Order ob = new Order();
            var column = new string[3];
            var courseName = new string[3];

            var result = ob.GetComparativeRatingComplaints(golferEmail, DateFrom, DateTo, viewIN);

            StringBuilder sb = new StringBuilder();
            //static file name, can be changes as per requirement
            string sFileName = "ComparativeRatingComplaints_" + DateTime.Now.Ticks + ".xls";
            //Bind data list from edmx
            var Data = result.ToList();

            if (Data != null && Data.Any())
            {
                sb.Append("<table style='font-size:16px;fornt-weight:bold;' Border='1'>");

                int headRow = 0;
                sb.Append("</tr>");
                foreach (object[] item in result)
                {
                    sb.Append("<tr>");

                    if (headRow == 0)
                    {
                        for (int i = 0; i < item.Length; i++)
                        {
                            sb.Append("<th>" + item[i].ToString() + "</th>");
                        }
                        headRow++;
                    }
                    else
                    {
                        for (int i = 0; i < item.Length; i++)
                        {
                            sb.Append("<td>" + item[i].ToString() + "</td>");
                        }
                    }

                    sb.Append("</tr>");
                }
                sb.Append("</table>");
            }

            HttpContext.Response.AddHeader("content-disposition", "attachment; filename=" + sFileName);
            this.Response.ContentType = "application/vnd.ms-excel";
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
            return File(buffer, "application/vnd.ms-excel");

        }

        /// <summary>
        /// Created By: Amit Kumar
        /// Creation On: 24 August 2015
        /// </summary>
        [SessionExpireFilterAttribute]
        [HttpGet]
        public ActionResult GetGolferAverageSpending(string golferEmail, DateTime DateFrom, DateTime DateTo, string viewIN)
        {
            AccessModule(ModuleValues.GolferOrderHistoryReport);

            Order ob = new Order();

            string strCourseAvgSpend = "";
            string strOtherAvgSpend = "";

            var lstRatingComplaints = ob.GetGolferAverageSpending(golferEmail, DateFrom, DateTo, viewIN, ref strCourseAvgSpend, ref strOtherAvgSpend);

            return Json(new { course = strCourseAvgSpend, other = strOtherAvgSpend }, JsonRequestBehavior.AllowGet);
        }

        #endregion
    }
}
