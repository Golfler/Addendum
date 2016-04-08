using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Golfler.Models;
using Golfler.Repositories;
using System.Data.Entity.Validation;
using System.Threading;
using System.IO;
using System.Data.SqlClient;
using System.Web.Helpers;
using System.Web.Script.Serialization;

namespace Golfler.Controllers
{
    public class AdminController : Controller
    {
        GolflerEntities Db = new GolflerEntities();

        public ActionResult Index()
        {
            return View("LogIn");
        }

        #region Version

        /// <summary>
        /// Created By: veera
        /// Creation On: 3 march, 2015
        /// Description: method for version info.
        /// </summary>
        public ActionResult Version()
        {
            return View();
        }

        #endregion

        #region LogIn

        /// <summary>
        /// Created By: Amit KUmar
        /// Description: Get method for Login Admin
        /// </summary>
        public ActionResult LogIn()
        {
            Session.Clear();

            var obj = new LogInModel() { UserType = UserType.Admin };
            Session["ForLogin"] = "Global Admin";
            if (Request.Cookies["loginsuper"] != null)
            {
                if (!string.IsNullOrEmpty(Request.Cookies["loginsuper"].Values["uname"]) &&
                !string.IsNullOrEmpty(Request.Cookies["loginsuper"].Values["pwd"]))
                {
                    obj.UserName = Request.Cookies["loginsuper"].Values["uname"];
                    obj.Password = Request.Cookies["loginsuper"].Values["pwd"];
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
                        obj.UserType = UserType.Admin;
                    if (rUser.Login(obj))
                    {
                        LoginInfo.CreateLoginSession(LoginType.SuperAdmin, rUser.UserObj.ID, rUser.CourseId, rUser.CourseName, rUser.CourseEmail,
                                                    rUser.UserObj.UserName, obj.Password, rUser.UserObj.FirstName, rUser.UserObj.LastName,
                                                    rUser.UserObj.Type, rUser.UserObj.LastLogin, rUser.UserObj.LastLoginIP, obj.KeepMeLogin,
                                                    0, "", false, "", rUser.UserObj.Image);

                        Session["LoginHitCount"] = null;

                        return RedirectToAction("Dashboard");
                    }
                    else
                    {
                        #region Login Lock Functionality

                        const int triesRemain = 4;
                        string msg = "Invalid User";

                        var userData = Db.GF_AdminUsers.FirstOrDefault(x => x.UserName == obj.UserName && x.Status == StatusType.Active &&
                                (x.Type == UserType.SuperAdmin || x.Type == UserType.Admin));

                        if (userData != null)
                        {
                            var lockTime = Db.GF_Settings.FirstOrDefault(x => x.Name.ToLower().Contains("login lock time") && x.CourseID == 0);

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
        /// Created By: Amit Kumar
        /// Description: User Dashboard.
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
                int roleCount = 0;
                int courseCount = 0;
                int staticpageCount = 0;
                int golferUserCount = 0;
                int suggCourseCount = 0;

                GF_Golfer.GetSuperAdminDashBoardInfo(LoginInfo.UserId, ref userCount, ref roleCount, ref courseCount,
                    ref staticpageCount, ref golferUserCount, ref suggCourseCount);

                ViewBag.UserCount = userCount;
                ViewBag.RoleCount = roleCount;
                ViewBag.CourseCount = courseCount;
                ViewBag.StaticpageCount = staticpageCount;
                ViewBag.GolferUserCount = golferUserCount;
                ViewBag.SuggCourseCount = suggCourseCount;

                #endregion

                return View(objRole.GetRoleByUserId(LoginInfo.UserId, true));
            }
            catch (Exception ex)
            {

                ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
                return null;
            }
        }

        #endregion

        #region Manage Admin/Course User

        /// <summary>
        /// Created By:
        /// Creation On:
        /// Description: method to get add/edit
        /// </summary>
        [SessionExpireFilterAttribute]
        public ActionResult UserAddEdit(string eid)
        {
            AccessModule(ModuleValues.UsersAdmin);
            long id = CommonFunctions.DecryptUrlParam(eid);
            //if (LoginInfo.IsSuper)
            //     AccessModule(ModuleValues.AllRights);
            //else
            //  AccessModule(ModuleValues.User);

            var UserObj = new Users(id);
            GF_AdminUsers obj = UserObj.GetUser(id);

            if (obj.ID == 0)
                obj.Type = LoginInfo.Type;

            ViewBag.RoleId = new SelectList(UserObj.GetUserRoles(StatusType.Active, id), "Id", "Name", obj.RoleId);
            ViewBag.Type = new SelectList(UserType.GetSystemUsers(), "Tag", "Name", obj.Type);
            ViewBag.IsLoginUser = (id != 0 && id == LoginInfo.UserId);
            obj.LastLogin = Convert.ToDateTime(obj.LastLogin).ToLocalTime();
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

            AccessModule(ModuleValues.UsersAdmin);

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
            AccessModule(ModuleValues.UsersAdmin);

            ViewBag.Message = TempData["Message"];

            ViewBag.TypeFrom = "SuperAdmin";

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
                string timeZone = CommonFunctions.GetCourseTimeZone(0);

                var UserObj = new Users();
                var totalRecords = 0;
                var rec = UserObj.GetAdminUsers(searchText, sidx,
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
                                                                         CreatedOn = CommonFunctions.DateByTimeZone(timeZone, x.CreatedOn),
                                                                         LastLogin = x.LastLogin == null ? x.LastLogin : CommonFunctions.DateByTimeZone(timeZone, x.LastLogin ?? DateTime.UtcNow)
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
            var UserObj = new Users(id);
            return UserObj.ChangeStatus(status)
                       ? Json(new { statusText = "success", module = "Admin User", task = "update" })
                       : Json(new { statusText = "error", module = "Admin User", task = "update", message = UserObj.Message });
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

        [AcceptVerbs(HttpVerbs.Post)]
        [SessionExpireFilterAttribute]
        public ActionResult UpdateSuggestedCourseStatus(long id, bool status)
        {
            var SuggestedCourse = new SuggestedCourse(id);
            return SuggestedCourse.ChangeStatus(status)
                       ? Json(new { statusText = "success", module = "Suggested Course status", task = "update" })
                       : Json(new { statusText = "error", module = "Suggested Course status", task = "update", message = SuggestedCourse.Message });
        }

        #region Access Function

        /// <summary>
        /// Created By:
        /// Creation On:
        /// Description: method for list
        /// </summary>
        private void AccessModule(string module)
        {
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
                if (m != null)
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
                else
                {
                    ViewBag.AddFlag = "False";
                    ViewBag.UpdateFlag = "False";
                    ViewBag.DeleteFlag = "False";
                    ViewBag.ReadFlag = "False";
                }
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
                //if (LoginInfo.IsSuper)
                AccessModule(ModuleValues.RolesAdmin);
                //else
                //    AccessModule(ModuleValues.Role);

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

            try
            {
                long id = CommonFunctions.DecryptUrlParam(eid);

                //if (LoginInfo.IsSuper)
                AccessModule(ModuleValues.RolesAdmin);
                //else
                //    AccessModule(ModuleValues.Role);

                var RoleObj = new Role(id);
                GF_Roles obj = RoleObj.GetRole(id, true);
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
                //if (LoginInfo.IsSuper)
                AccessModule(ModuleValues.RolesAdmin);
                //else
                //    AccessModule(ModuleValues.Role);

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
                AccessModule(ModuleValues.EmailTemplatesAdmin);
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
                AccessModule(ModuleValues.AllRights);
                //else
                //    AccessModule(ModuleValues.EmailTemplates);

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
            AccessModule(ModuleValues.AllRights);
            //else
            //    AccessModule(ModuleValues.EmailTemplates);

            var temp = new EmailTemplate();
            var status = temp.UpdateEmailTemplate(id, content);
            if (status)
                TempData["Message"] = string.Format("Email Template update successfully.");
            return Json(new { msg = status });
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
                AccessModule(ModuleValues.SmtpAdmin);

                GF_SMTPSettings objModel = new GF_SMTPSettings();
                long id = LoginInfo.CourseId;
                Templates objTemp = new Templates();
                List<GF_SMTPSettings> lstSmtp = new List<GF_SMTPSettings>();
                lstSmtp = objTemp.GetSMTPSettings(id);
                ViewBag.orgEditId = id;
                ViewBag.isSMTPDetails = true;
                ViewBag.Isedit = "0";

                //if (lstSmtp.Count > 0)
                //{
                //    objModel.AdminEmail = lstSmtp[0].AdminEmail;
                //    objModel.FromEmail = lstSmtp[0].FromEmail;
                //    objModel.SMTPHost = lstSmtp[0].SMTPHost;
                //    objModel.SMTPPassword = lstSmtp[0].SMTPPassword;
                //    objModel.SMTPPort = lstSmtp[0].SMTPPort;
                //    objModel.ReplyEmail = lstSmtp[0].ReplyEmail;
                //    objModel.SMTPUserName = lstSmtp[0].SMTPUserName;
                //    objModel.EnableSsl = lstSmtp[0].EnableSsl;
                //    objModel.EnableTls = lstSmtp[0].EnableTls;
                //}
                //---------new

                //AccessModule(ModuleValues.SmtpAdmin);
                //GF_RoleModules m = CommonFunctions.GetAccessModule(ModuleValues.SmtpAdmin);
                //if (ViewBag.ReadFlag=="True")
                //{
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
                //    objModel.EnableSsl =null;
                //    objModel.EnableTls = null;
                //}
                //----------------------------------------------



                return View(objModel);
            }
            catch (Exception ex)
            {
                ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
                return null;
            }
        }



        [HttpPost]
        [SessionExpireFilterAttribute]
        public ActionResult SMTPDetails(GF_SMTPSettings obj, FormCollection frm)
        {
            // AccessModule(ModuleValues.AllRights);
            try
            {
                Templates objTemplates = new Templates();
                obj.CourseID = 0;
                var status = objTemplates.SaveSMTP(obj);


            }
            catch (Exception ex)
            {
                ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
            }
            return View(obj);
        }
        #endregion

        #region Manage Club House

        [ActionName("ManageClubHouse")]
        [SessionExpireFilterAttribute]
        public ActionResult ManageCourses()
        {
            AccessModule(ModuleValues.CourseAdmin);
            ViewBag.Partner = new SelectList(PartershipStatus.GetPartnerShipStatus(), "Tag", "Name");
            return View("ManageCourses");
        }

        /// <summary>
        /// Created By:Arun
        /// Creation On:26 March 2015
        /// Description: Get all Menu User listing
        /// </summary>
        [SessionExpireFilterAttribute]
        public ActionResult GetCourseInfo(string searchText, string cityName, string partnerType, string sidx, string sord, int? page, int? rows)
        {
            try
            {
                Course objCourse = new Course();
                var totalRecords = 0;
                var rec = objCourse.GetCoursesInfo(searchText, cityName, partnerType, sidx,
                                           sord, page ?? 1, rows ?? 10,
                                           ref totalRecords).AsEnumerable().Select((x =>
                                                                     new
                                                                     {
                                                                         //ID = x.ID,
                                                                         //x.COURSE_NAME,
                                                                         //PartnershipStatus = x.PartnershipStatus == PartershipStatus.Partner ? "Partner" : "Non-Partner",
                                                                         //x.STATE,
                                                                         //x.CITY,
                                                                         //Status = x.Status ?? StatusType.InActive,
                                                                         //EID = CommonFunctions.EncryptUrlParam(x.ID),
                                                                         //UserName = GetCourseAdmin(x.ID),
                                                                         //Coordinate = string.IsNullOrEmpty(x.LATITUDE) ? "No" : "Yes"
                                                                         ID = x.ID,
                                                                         x.COURSE_NAME,
                                                                         x.PartnershipStatus,
                                                                         x.STATE,
                                                                         x.CITY,
                                                                         x.Status,
                                                                         EID = CommonFunctions.EncryptUrlParam(x.ID),
                                                                         x.UserName,
                                                                         x.Coordinate,
                                                                         x.ScoreCard
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

        public string GetCourseAdmin(long CourseID)
        {
            try
            {
                var lstCourseAdmin = Db.GF_AdminUsers.FirstOrDefault(q => (q.CourseId ?? 0) == CourseID &&
                    q.Type.ToLower().Contains(UserType.CourseAdmin.ToLower()));

                if (lstCourseAdmin == null)
                {
                    lstCourseAdmin = Db.GF_AdminUsers.FirstOrDefault(q => (q.CourseId ?? 0) == CourseID &&
                    q.Type.ToLower().Contains(UserType.Proshop.ToLower()));
                }

                if (lstCourseAdmin != null)
                {
                    return lstCourseAdmin.UserName;
                }

                return "";
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        /// Created By:Arun
        /// </summary>
        /// <returns></returns>
        [ActionName("ClubHouseAddUpdate")]
        [SessionExpireFilterAttribute]
        public ActionResult CoursesAddUpdate(string eid)
        {
            AccessModule(ModuleValues.CourseAdmin);

            ViewBag.eid = eid;
            TempData["CourseEID"] = eid;

            long id = CommonFunctions.DecryptUrlParam(eid);
            Course obj = new Course();
            if (id > 0)
            {
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

                #region Comment

                //if (!string.IsNullOrEmpty(Convert.ToString(TempData["HoleMessage"])))
                //{
                //    ViewBag.HoleMessage = TempData["HoleMessage"];
                //}
                //else
                //{
                //    ViewBag.HoleMessage = "";
                //}
                //if (!string.IsNullOrEmpty(Convert.ToString(TempData["HandicapMessage"])))
                //{
                //    ViewBag.HandicapMessage = TempData["HandicapMessage"];
                //}
                //else
                //{
                //    ViewBag.HandicapMessage = "";
                //}
                //if (!string.IsNullOrEmpty(Convert.ToString(TempData["ParMessage"])))
                //{
                //    ViewBag.ParMessage = TempData["ParMessage"];
                //}
                //else
                //{
                //    ViewBag.ParMessage = "";
                //}

                #endregion

                return View("CoursesAddUpdate", objCOurse);
            }

            var courseInfo = new GF_CourseInfo();

            courseInfo.FoodCommission = Db.GF_Category.Where(x => x.Status == StatusType.Active).ToList().Select(x => new GF_FoodCommission
            {
                CategoryID = x.ID,
                CategoryName = x.Name,
                Commission = 0,
                CourseID = LoginInfo.CourseId
            }).ToList();

            ViewBag.Users = new SelectList(obj.GetCourseUserByCourseID(null), "ID", "Name", courseInfo.UserID);
            ViewBag.Partner = new SelectList(PartershipStatus.GetPartnerShipStatus(), "Tag", "Name");

            ViewBag.Country = new SelectList(obj.GetAllActiveCountiesCourse(), "COUNTRY_NAME", "COUNTRY_NAME");
            ViewBag.County = new SelectList(obj.GetAllActiveCounties(), "COUNTY_NAME", "COUNTY_NAME");



            ViewBag.ActiveDive = TempData["ActiveDive"];
            //TempData["HoleMessage"] = "Hole detail updated successfully.";
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

            #region Comment
            //if (!string.IsNullOrEmpty(Convert.ToString(TempData["HoleMessage"])))
            //{
            //    ViewBag.HoleMessage = TempData["HoleMessage"];
            //}
            //else
            //{
            //    ViewBag.HoleMessage = "";
            //}
            //if (!string.IsNullOrEmpty(Convert.ToString(TempData["HandicapMessage"])))
            //{
            //    ViewBag.HandicapMessage = TempData["HandicapMessage"];
            //}
            //else
            //{
            //    ViewBag.HandicapMessage = "";
            //}
            //if (!string.IsNullOrEmpty(Convert.ToString(TempData["ParMessage"])))
            //{
            //    ViewBag.ParMessage = TempData["ParMessage"];
            //}
            //else
            //{
            //    ViewBag.ParMessage = "";
            //}
            #endregion

            return View("CoursesAddUpdate", courseInfo);
        }

        [SessionExpireFilterAttribute]
        public ActionResult LoadNewCourseAdmin()
        {
            AccessModule(ModuleValues.UsersAdmin);


            return PartialView("~/Views/Shared/_QuickCourseAdminUser.cshtml");
        }

        [HttpPost]
        [SessionExpireFilterAttribute]
        public ActionResult QuickAddCourseAdmin(string UserName, string FirstName, string LastName, string Email, string Password)
        {
            var isSaved = false;
            if (UserName.Length < 5)
            {
                return Json(new { newid = 0, uname = "Minimum length for Username is 5 characters." });
            }
            else
            {
                if (!(CommonFunctions.IsValidEmailID(Email)))
                {
                    return Json(new { newid = 0, uname = "Please enter valid email address." });
                }
                else
                {
                    var UserObj = new Users(0);

                    GF_AdminUsers obj = new GF_AdminUsers();
                    obj.UserName = UserName;
                    obj.FirstName = FirstName;
                    obj.LastName = LastName;
                    obj.Email = Email;
                    obj.Password = Password;
                    obj.Type = UserType.CourseAdmin;
                    obj.CreatedBy = LoginInfo.UserId;
                    obj.CreatedOn = DateTime.Now;
                    obj.Status = StatusType.Active;
                    obj.RoleId = 0;
                    obj.IsCourseUser = true;
                    obj.Active = true;
                    obj.Phone = "";

                    isSaved = UserObj.Save(obj);

                    if (isSaved)
                    {
                        if (Convert.ToInt64(UserObj.IdForQuickAdd) > 0)
                        {
                            return Json(new { newid = UserObj.IdForQuickAdd, uname = UserObj.FullNameForQuickAdd });
                        }
                        else
                        {
                            return Json(new { newid = 0, uname = UserObj.Message });
                        }
                    }
                    else
                    {
                        return Json(new { newid = 0, uname = UserObj.Message });
                    }
                }
            }
        }

        /// <summary>
        /// Created By:Arun
        /// </summary>
        /// <returns></returns>
        [ActionName("ClubHouseAddUpdate")]
        [SessionExpireFilterAttribute]
        [HttpPost]
        public ActionResult CoursesAddUpdate(GF_CourseInfo objCourse)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    Course obj = new Course();
                    long courseID = 0;

                    obj.updateCourseInfo(objCourse, (objCourse.UserID ?? 0), ref courseID);

                    string eid = CommonFunctions.EncryptUrlParam(courseID);
                    TempData["CourseMessage"] = "Club house details submitted successfully.";
                    //TempData["CourseMessage"] = "Course details submitted successfully.";

                    return RedirectToAction("ClubHouseAddUpdate/" + eid);
                    //return RedirectToAction("CoursesAddUpdate/" + eid);
                }
            }
            catch (DbEntityValidationException e)
            {
                foreach (var eve in e.EntityValidationErrors)
                {
                    ErrorLibrary.ErrorClass.WriteLog(e.GetBaseException(), System.Web.HttpContext.Current.Request);
                }
                return null;
            }
            return View("CoursesAddUpdate", objCourse);
        }

        [SessionExpireFilterAttribute]
        [HttpPost]
        public ActionResult CourseSetting(GF_CourseInfo objCourse)
        {
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
            // ViewBag.ActiveDive =
            TempData["ActiveDive"] = "coursesetting";
            string eid = Convert.ToString(Request.Form["hdnEIDHole"]); // TempData["CourseEID"].ToString();
            return RedirectToAction("ClubHouseAddUpdate/" + eid);
            // return RedirectToAction("CoursesAddUpdate");// View(objCourse);
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
        //            TempData["HoleMessage"] = "Hole details updated successfully.";// CommonFunctions.Message(obj.Message, module);

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
        //    string eid = Convert.ToString(Request.Form["hdnEIDHole"]); // TempData["CourseEID"].ToString();

        //    return RedirectToAction("CoursesAddUpdate/" + eid);// View(objCourse);
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
        //            TempData["HandicapMessage"] = "Handicapped details updated successfully."; //CommonFunctions.Message(obj.Message, module);

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
        //    string eid = Convert.ToString(Request.Form["hdnEIDHandicapped"]);


        //    return RedirectToAction("CoursesAddUpdate/" + eid);// View(objCourse);
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
        //            TempData["ParMessage"] = "Par details updated successfully.";// CommonFunctions.Message(obj.Message, module);

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

        //    string eid = Convert.ToString(Request.Form["hdnEIDPar"]);
        //    return RedirectToAction("CoursesAddUpdate/" + eid);// View(objCourse);
        //}

        #region Update user Status

        /// <summary>
        /// Created By: Arun
        /// Created date: 31 March 2015
        /// Purpose: Update Status
        /// </summary>
        /// <param name="id"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        [SessionExpireFilterAttribute]
        public ActionResult UpdateCourseStatus(long id, string status)
        {
            AccessModule(ModuleValues.AllRights);
            try
            {
                Course obj = new Course();
                return obj.ChangeStatus(id, status)
                           ? Json(new { statusText = "success", module = "Course Info", task = "update", message = obj.Message })
                           : Json(new { statusText = "error", module = "Course Info", task = "update", message = obj.Message });
            }
            catch (DbEntityValidationException e)
            {
                foreach (var eve in e.EntityValidationErrors)
                {
                    ErrorLibrary.ErrorClass.WriteLog(e.GetBaseException(), System.Web.HttpContext.Current.Request);
                }
                return null;
            }

        }

        #endregion

        # region Delete selected Golfer
        /// <summary>
        /// Created By: Arun
        /// Creation On: 31 March 2015
        /// Description: method to delete
        /// </summary>
        [AcceptVerbs(HttpVerbs.Post)]
        [SessionExpireFilterAttribute]
        public ActionResult DeleteCoursesInfo(long[] ids)
        {
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

        /// <summary>
        /// Created By: Amit Kumar
        /// Creation On: 28 March 2015
        /// Description: method for list
        /// </summary>
        [SessionExpireFilterAttribute]
        public ActionResult CourseSuggestList()
        {
            //if (LoginInfo.IsSuper)
            AccessModule(ModuleValues.SuggestedCourseAdmin);
            //else
            //    AccessModule(ModuleValues.Order);

            ViewBag.Message = TempData["Message"];

            return View();
        }

        [SessionExpireFilterAttribute]
        public ActionResult GetGolpherListForSuggestedCourse(string searchText, string sidx, string sord, int? page, int? rows)
        {
            try
            {
                Course CourseObj = new Course();
                var totalRecords = 0;
                var rec = CourseObj.GetGolferListForSuggestCourse(searchText, sidx,
                                           sord, page ?? 1, rows ?? 10,
                                           ref totalRecords).AsEnumerable().Select((x =>
                                                                     new
                                                                     {
                                                                         SubmitedBy = Db.GF_Golfer.FirstOrDefault(y => y.GF_ID == x.UserId).FirstName + " " + Db.GF_Golfer.FirstOrDefault(y => y.GF_ID == x.UserId).LastName,
                                                                         Email = Db.GF_Golfer.FirstOrDefault(y => y.GF_ID == x.UserId).Email,
                                                                         x.Date
                                                                     }
                                                                    ));

                var jsonData = new
                {
                    total = (totalRecords + rows - 1) / rows,
                    page,
                    records = totalRecords,
                    rows = rec.ToList(),
                    id = "Date"
                };
                return Json(jsonData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
                return null;
            }
        }

        public ActionResult ShowGolferListforSuggestedCourse(string coursename)
        {
            Db = new GolflerEntities();

            ViewBag.coursename = coursename;

            return PartialView("~/Views/Shared/_GolferListForSuggestedCourse.cshtml");
        }

        /// <summary>
        /// Created By: Amit Kumar
        /// Creation On: 28 March 2015
        /// Description: method to bind grid
        /// </summary>
        [SessionExpireFilterAttribute]
        public ActionResult GetCourseSuggestList(string searchText, string sidx, string sord, int? page, int? rows)
        {
            try
            {
                Course CourseObj = new Course();
                var totalRecords = 0;
                var rec = CourseObj.GetSuggestedCourseList(searchText, sidx,
                                           sord, page ?? 1, rows ?? 10,
                                           ref totalRecords).AsEnumerable().Select((x =>
                                                                     new
                                                                     {
                                                                         x.coursename,
                                                                         x.NoOfSuggestions
                                                                         //x.ID,
                                                                         //NearByCourseId = (x.NearByCourseId == null || x.NearByCourseId == 0) ? "0" :
                                                                         //                 CommonFunctions.EncryptUrlParam(x.NearByCourseId ?? 0),
                                                                         //EID = CommonFunctions.EncryptUrlParam(x.UserId ?? 0),
                                                                         //x.Name,
                                                                         //x.Date,
                                                                         //SubmitedBy = Db.GF_Golfer.FirstOrDefault(y => y.GF_ID == x.UserId).FirstName + " " + Db.GF_Golfer.FirstOrDefault(y => y.GF_ID == x.UserId).LastName,
                                                                         //Email= Db.GF_Golfer.FirstOrDefault(y => y.GF_ID == x.UserId).Email,
                                                                         //x.Status,
                                                                         //Active = (x.Status == StatusType.Active)  ,
                                                                         //DoActive = true
                                                                     }
                                                                    ));

                var jsonData = new
                {
                    total = (totalRecords + rows - 1) / rows,
                    page,
                    records = totalRecords,
                    rows = rec.ToList(),
                    id = "coursename"
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

        [SessionExpireFilterAttribute]
        public ActionResult CoursesList()
        {
            AccessModule(ModuleValues.CourseAdmin);
            ViewBag.Partner = new SelectList(PartershipStatus.GetPartnerShipStatus(), "Tag", "Name");
            return View();
        }

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
                                                                         DoActive = x.ID != 0
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
        /// Purpose: Save new course information
        /// </summary>
        /// <param name="ClubHouseID"></param>
        /// <param name="CourseName"></param>
        /// <param name="Hole"></param>
        /// <param name="Status"></param>
        /// <returns></returns>
        [HttpPost]
        [SessionExpireFilterAttribute]
        public ActionResult AddCourseInfo(string EID, long ClubHouseID, string CourseName, string Hole, bool Status)
        {
            Course CourseObj = new Course();
            string message = string.Empty;
            long id = !string.IsNullOrEmpty(EID) ? CommonFunctions.DecryptUrlParam(EID) : 0;
            bool isSaved = CourseObj.AddEditCourse(id, ClubHouseID, CourseName, Hole, Status, ref message);

            if (isSaved)
            {
                return Json(new { status = 1, msg = message });
            }
            else
            {
                return Json(new { status = 0, msg = message });
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
            AccessModule(ModuleValues.CourseAdmin);

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

        #region Settings
        /// <summary>
        /// Created By: Arun
        /// Created Date:01 April 2015
        /// Purpose: Get Settings
        /// </summary>
        /// <returns></returns>
        [SessionExpireFilterAttribute]
        public ActionResult SettingAddEdit()
        {
            AccessModule(ModuleValues.SettingAdmin);
            GF_Settings objSettings = Db.GF_Settings.FirstOrDefault(x => x.CourseID == 0);

            #region Time Zone Settings

            var timeZone = objSettings.lstSettings.FirstOrDefault(x => x.Name.ToLower() == WebSetting.TimeZone.ToLower());

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

            ViewBag.TimeZone = new SelectList(CommonFunctions.GetTimeZone(), "ID", "TimeZone_Name", value);
            ViewBag.TimeZoneValue = value;

            #endregion

            return View(objSettings);
        }


        /// <summary>
        /// Created By: Arun
        /// Created Date:01 April 2015
        /// Purpose: Update Settings
        /// </summary>
        /// <returns></returns>
        [SessionExpireFilterAttribute]
        [HttpPost]
        public ActionResult SettingAddEdit(GF_Settings obj, FormCollection frm)
        {
            AccessModule(ModuleValues.SettingAdmin);
            try
            {
                var lstName = string.IsNullOrEmpty(frm["txtName"]) ? new List<string>() : Convert.ToString(frm["txtName"]).Split(',').Select(x => x).ToList();
                var lstValue = string.IsNullOrEmpty(frm["txtValue"]) ? new List<string>() : Convert.ToString(frm["txtValue"]).Split(',').Select(x => x).ToList();
                obj.UpdateSettings(lstName, lstValue, 0);

                #region Time Zone Settings

                var timeZone = obj.lstSettings.FirstOrDefault(x => x.Name.ToLower() == WebSetting.TimeZone.ToLower());

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

        #region Manage Static Pages

        /// <summary>
        /// Modified By: Amit Kumar
        /// Modified On: 10 April 2015
        /// Description: method for list
        /// </summary>
        [SessionExpireFilterAttribute]
        public ActionResult StaticPageList()
        {
            try
            {
                AccessModule(ModuleValues.StaticPageAdmin);

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
        /// Modified By: Amit Kumar
        /// Modified On: 10 April 2015
        /// Description: method to bind grid
        /// </summary>
        [SessionExpireFilterAttribute]
        public ActionResult GetStaticPageList(string searchText, string sidx, string sord, int? page, int? rows)
        {
            try
            {
                var PageObj = new GF_StaticPages();
                var totalRecords = 0;
                var rec = PageObj.GetPages(searchText, sidx,
                                           sord, page ?? 1, rows ?? 10,
                                           ref totalRecords).AsEnumerable().Select(x =>
                                                                     new
                                                                     {
                                                                         x.ID,
                                                                         x.PageName,
                                                                         Active = (x.Status == StatusType.Active),
                                                                         DoActive = (x.Status != StatusType.Copied),
                                                                         Type = (x.Status != StatusType.Copied),
                                                                         Link = "/Golfler/Home/" + x.PageCode,
                                                                         CreatedDate = x.ModifyDate == null ? x.CreatedDate : x.ModifyDate,
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
        /// Modified By: Amit Kumar
        /// Modified On: 10 April 2015
        /// Description: method to active/inactive
        /// </summary>
        [AcceptVerbs(HttpVerbs.Post)]
        [SessionExpireFilterAttribute]
        public ActionResult UpdateStaticPageStatus(long id, bool status)
        {
            try
            {
                AccessModule(ModuleValues.StaticPageAdmin);

                var PageObj = new GF_StaticPages(id);
                return PageObj.ChangeStatus(status)
                    ? Json(new { statusText = "success", module = "Pages", task = "update", message = PageObj.Active ? "Pages Activated Successfully" : "Pages Deactivated Successfully" })
                           : Json(new { statusText = "error", module = "Pages", task = "update", message = PageObj.Message });

            }
            catch (Exception ex)
            {
                ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
                return null;
            }

        }

        /// <summary>
        /// Modified By: Amit Kumar
        /// Modified On: 10 April 2015
        /// Description: method to delete
        /// </summary>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult DeleteStaticPages(long[] ids)
        {
            try
            {
                AccessModule(ModuleValues.StaticPageAdmin);

                if (ids != null)
                {
                    var PageObj = new GF_StaticPages();
                    return PageObj.DeletePages(ids)
                               ? Json(new { statusText = "success", module = "Pages", task = "delete" })
                               : Json(new { statusText = "error", module = "Pages", task = "delete", message = PageObj.Message });
                }
                return Json(new { statusText = "error", module = "Pages", task = "select" });
            }
            catch (Exception ex)
            {
                ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
                return null;
            }

        }

        /// <summary>
        /// Modified By: Amit Kumar
        /// Modified On: 10 April 2015
        /// Description: method to get add/edit
        /// </summary>
        [HttpGet]
        [SessionExpireFilterAttribute]
        public ActionResult StaticPageAddEdit(string eid)
        {
            try
            {
                AccessModule(ModuleValues.StaticPageAdmin);

                if (eid == null || eid == "")
                    return RedirectToAction("StaticPageList");

                long id = CommonFunctions.DecryptUrlParam(eid);

                //AccessModule(ModuleValues.StaticPage);
                var PageObj = new GF_StaticPages(id);
                GF_StaticPages obj = PageObj.GetPage(id);

                return View(obj);
            }
            catch (Exception ex)
            {
                ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
                return null;
            }

        }

        /// <summary>
        /// Modified By: Amit Kumar
        /// Modified On: 10 April 2015
        /// Description: method to save/update
        /// </summary>
        [HttpPost]
        [SessionExpireFilterAttribute]
        [ValidateInput(false)]
        public ActionResult StaticPageAddEdit(long? id, GF_StaticPages obj)
        {
            try
            {
                AccessModule(ModuleValues.StaticPageAdmin);

                var PageObj = new GF_StaticPages(id);
                var isSaved = false;
                if (ModelState.IsValid)
                {
                    isSaved = PageObj.Save(obj);
                }

                if (isSaved)
                {
                    string module = "StaticPage";
                    TempData["Message"] = CommonFunctions.Message(PageObj.Message, module);
                    return RedirectToAction(module + "List");
                }
                if (PageObj.Message == "Name")
                    ViewBag.ErrorMessage = String.Format("exist", PageObj.Message);
                else
                    ViewBag.ErrorMessage = PageObj.Message;

                return View(obj);
            }
            catch (Exception ex)
            {
                ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
                return null;
            }

        }

        #endregion

        #region Manage Process Refund

        /// <summary>
        /// Created By: Veera Verma
        /// Created on: 14 April 2015
        /// Description: method for list
        /// </summary>
        [SessionExpireFilterAttribute]
        public ActionResult AdminProcessRefundList()
        {
            //if (LoginInfo.IsSuper)
            //{
            AccessModule(ModuleValues.ProcessRefundAdmin);
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

        /// <summary>
        /// Created By: Veera Verma
        /// Created on: 14 April 2015
        /// Description: method to bind grid
        /// </summary>
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
        public ActionResult AdminProcessRefundadd(string orderId, string refundId)
        {
            try
            {
                long id = Convert.ToInt64(orderId);
                //if (LoginInfo.IsSuper)
                // AccessModule(ModuleValues.AllRights);
                //else
                AccessModule(ModuleValues.ProcessRefundAdmin);

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
                    //if (refundType == "F")
                    // { 
                    //

                    //   StripePayments objStripe = new StripePayments();
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

        #region Manage Login as Course Admin

        public ActionResult SuperAdminLogin(long courseID)
        {
            AccessModule(ModuleValues.AllRights);
            var rUser = new Users();
            var status = false;
            string reason = "You are not authorised to login any course.";
            try
            {
                //if (LoginInfo.IsSuper)
                if (LoginInfo.Type == UserType.SuperAdmin || LoginInfo.Type == UserType.Admin)
                {
                    #region old code
                    //var admin = Db.GF_AdminUsers.FirstOrDefault(x => x.ID == LoginInfo.UserId);
                    //var course = Db.GF_CourseInfo.FirstOrDefault(x => x.ID == courseID);

                    //LoginInfo.CreateLoginSession(admin.ID, course.ID, course.COURSE_NAME, admin.Email,
                    //        admin.UserName, "", admin.FirstName, admin.LastName,
                    //        admin.Type, admin.LastLogin, admin.LastLoginIP, false, LoginInfo.UserId, LoginInfo.Type);
                    #endregion

                    #region new code
                    // Changes by Ramesh Kalra
                    // Date: 20 April 2015
                    // Changes: 1. Super Admin can not login into Course which have not its Course Admin.
                    // 2. Super admin will login into Course as a "Course Admin", not as "Super Admin".
                    var course = Db.GF_CourseInfo.FirstOrDefault(x => x.ID == courseID);

                    // Get Admin User of Course
                    var admin_user_course = Db.GF_AdminUsers.Where(x => x.Status == StatusType.Active && x.Type == UserType.CourseAdmin && x.CourseId == courseID).FirstOrDefault();
                    if (admin_user_course != null)
                    {
                        var admin = Db.GF_AdminUsers.FirstOrDefault(x => x.ID == admin_user_course.ID);

                        LoginInfo.CreateLoginSession(LoginType.CourseAdmin, admin.ID, course.ID, course.COURSE_NAME, admin.Email,
                                admin.UserName, "", admin.FirstName, admin.LastName,
                                admin.Type, admin.LastLogin, admin.LastLoginIP, false, LoginInfo.UserId, LoginInfo.Type,
                                false, "", admin.Image);

                        status = true;
                    }
                    else
                    {
                        status = false;
                        reason = "Login failure: You need to set Admin User for this course before Login.";
                    }
                    #endregion
                }
            }
            catch (Exception ex)
            {
                status = false;
                reason = "Login failure. Exception: " + ex.Message;
            }

            // if successful login -- status =true and reason =""
            // if not              -- status =false and reason=[description]
            return Json(new { msg = status, reason = reason });
        }

        public ActionResult SuperAdminGolferLogin(long golferID)
        {
            AccessModule(ModuleValues.AllRights);
            var rUser = new Users();
            var status = false;
            string reason = "";
            try
            {
                if (LoginInfo.IsSuper)
                {
                    var golfer = Db.GF_Golfer.FirstOrDefault(x => x.GF_ID == golferID);

                    if (golfer != null)
                    {
                        LogInModel obj = new LogInModel();
                        obj.UserName = golfer.Email;
                        obj.IpAddress = Request.ServerVariables["REMOTE_ADDR"];
                        if (obj.UserType == null)
                            obj.UserType = UserType.Golfer;

                        LoginInfo.CreateGolferLoginSession(LoginType.Golfer, golfer.GF_ID, golfer.Email, golfer.Password, golfer.FirstName,
                            golfer.LastName, UserType.Golfer, golfer.LastLogin, golfer.LastLoginIP, false, LoginInfo.UserId, LoginInfo.Type, true,
                            golfer.Image);

                        status = true;
                    }
                    else
                    {
                        status = false;
                        reason = "Login failure: You need to set Admin User for this course before Login.";
                    }
                }
            }
            catch (Exception ex)
            {
                status = false;
                reason = "Login failure. Exception: " + ex.Message;
            }

            // if successful login -- status =true and reason =""
            // if not              -- status =false and reason=[description]
            return Json(new { msg = status, reason = reason });
        }

        public ActionResult SuperAdminLoginByID(long adminId = 0)
        {
            AccessModule(ModuleValues.AllRights);
            var rUser = new Users();
            var status = false;

            try
            {
                if (rUser.LoginWithID(adminId))
                {
                    if (rUser.UserObj.Type.Contains(UserType.SuperAdmin) || rUser.UserObj.Type.Contains(UserType.Admin))
                        LoginInfo.CreateLoginSession(LoginType.SuperAdmin, rUser.UserObj.ID, rUser.CourseId, rUser.CourseName, rUser.CourseEmail,
                                                    rUser.UserObj.UserName, "", rUser.UserObj.FirstName, rUser.UserObj.LastName,
                                                    rUser.UserObj.Type, rUser.UserObj.LastLogin, rUser.UserObj.LastLoginIP,
                                                    false, 0, "", true, "", rUser.UserObj.Image);
                    else
                    {
                        status = false;
                    }
                    status = true;
                }
            }
            catch (Exception ex)
            {
                status = false;
            }


            if (rUser.UserObj != null)
            {
                return RedirectToAction("Dashboard", "Admin");
            }
            else
            {
                return RedirectToAction("LogIn", "Admin");
            }
        }

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
            AccessModule(ModuleValues.AllRights);
            Course objCourse = new Course();
            ViewBag.CourseIds = new SelectList(objCourse.GetAllActivePartnerCourses(), "ID", "COURSE_NAME");
            ViewBag.Type = new SelectList(UserType.GetSystemUsersForMassMsgs(), "Tag", "Name");
            return View();
        }

        [SessionExpireFilterAttribute]
        public ActionResult MassMessages()
        {
            AccessModule(ModuleValues.MassMessageAdmin);
            Course objCourse = new Course();
            ViewBag.CourseIds = new SelectList(objCourse.GetAllActivePartnerCourses(), "ID", "COURSE_NAME");
            ViewBag.Type = new SelectList(UserType.GetSystemUsersForMassMsgs(), "Tag", "Name");
            return View();
        }

        [SessionExpireFilterAttribute]
        public ActionResult GetUserListForMassMsgs(string searchText, string CourseId, string strUserType, string strRequestFrom, string RangeParameter, string sidx, string sord, int? page, int? rows)
        {
            try
            {
                var UserObj = new MassMessageClass();
                var totalRecords = 0;
                var rec = UserObj.GetUsersForMassMessages(searchText, CourseId, strUserType, strRequestFrom, RangeParameter, sidx,
                                           sord, page ?? 1, rows ?? 10,
                                           ref totalRecords).AsEnumerable().Select((x =>
                                                                     new
                                                                     {
                                                                         ID = x.Id,
                                                                         x.UserName,
                                                                         x.Email,
                                                                         UserType = x.Type,
                                                                         EID = CommonFunctions.EncryptUrlParam(x.Id),
                                                                         x.CourseName,
                                                                         Active = true,
                                                                         DoActive = true,
                                                                         x.userid
                                                                         //CourseName = string.Join(",<br />  ", ((from a in Db.GF_CourseInfo
                                                                         //                                        join b in Db.GF_CourseUsers on a.ID equals b.CourseID
                                                                         //                                        where b.ID == x.ID
                                                                         //                                        select a.COURSE_NAME).ToList()))
                                                                     }
                                                                    ));

                var jsonData = new
                {
                    total = (totalRecords + rows - 1) / rows,
                    page,
                    records = totalRecords,
                    rows = rec.ToList(),
                    id = "Id"
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
        public ActionResult GetMessageHistory(string searchText, string CourseId, string strUserType, string strRequestFrom, string fromDate, string toDate, string sidx, string sord, int? page, int? rows)
        {
            try
            {
                var UserObj = new MassMessageClass();
                var totalRecords = 0;
                var rec = UserObj.GetMessageHistory(searchText, CourseId, strUserType, strRequestFrom, fromDate, toDate, sidx,
                                           sord, page ?? 1, rows ?? 10,
                                           ref totalRecords).AsEnumerable().Select((x =>
                                                                     new
                                                                     {
                                                                         ID = x.MassMsgId,
                                                                         x.MessageTitle,
                                                                         x.CreatedByUser,
                                                                         x.SendToUserName,
                                                                         x.SendToUserEmail,
                                                                         x.SendToCourseName,
                                                                         x.CreatedDate,
                                                                         x.SendToUserType,
                                                                         x.EmailStatus,
                                                                         x.EmailStatusReason,
                                                                         EID = CommonFunctions.EncryptUrlParam(x.MassMsgId)
                                                                         //EID = CommonFunctions.EncryptUrlParam(x.Id),
                                                                         //x.CourseName,
                                                                         //Active = true,
                                                                         //DoActive = true,
                                                                         //x.userid
                                                                         //CourseName = string.Join(",<br />  ", ((from a in Db.GF_CourseInfo
                                                                         //                                        join b in Db.GF_CourseUsers on a.ID equals b.CourseID
                                                                         //                                        where b.ID == x.ID
                                                                         //                                        select a.COURSE_NAME).ToList()))
                                                                     }
                                                                    ));

                var jsonData = new
                {
                    total = (totalRecords + rows - 1) / rows,
                    page,
                    records = totalRecords,
                    rows = rec.ToList(),
                    id = "MassMsgId"
                };
                return Json(jsonData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
                return null;
            }
        }


        public delegate void delSendMassMessages(List<GF_MassMessages> lstMsgs);
        public static delSendMassMessages invSendMassMessages;

        public static void CallbackDelegatedFunctions(IAsyncResult t)
        {
            try
            {
                invSendMassMessages.EndInvoke(t);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public static void CallDelegatedFunctions(List<GF_MassMessages> lstMsgs)
        {
            var Db = new GolflerEntities();
            List<string> msgs = new List<string>();
            msgs.Add("Total Messages: " + lstMsgs.Count);
            string EmailBy = "";
            foreach (var objMsg in lstMsgs)
            {
                if (EmailBy == "")
                {
                    EmailBy = Convert.ToString(Db.GF_AdminUsers.Where(x => x.ID == objMsg.CreatedBy).Select(x => x.Email).FirstOrDefault());
                }
                if (!string.IsNullOrEmpty(Convert.ToString(EmailBy)))
                {
                    if (CommonFunctions.IsValidEmailID(EmailBy))
                    {
                        if (CommonFunctions.IsValidEmailID(objMsg.SendToUserEmail))
                        {

                            #region send Mass message email
                            try
                            {
                                string mailresult = "";
                                string logintype = "";
                                if (objMsg.CourseId == 0)
                                {
                                    logintype = LoginType.SuperAdmin;
                                }

                                IQueryable<GF_EmailTemplatesFields> templateFields = null;
                                var param = EmailParams.GetEmailParamsNew(ref Db, EmailTemplateName.MassMessages, ref templateFields, Convert.ToInt64(objMsg.CourseId), logintype, ref mailresult);

                                if (mailresult == "") // means Parameters are OK
                                {
                                    if (ApplicationEmails.SendMassMessages(Convert.ToInt64(objMsg.CourseId), ref Db, objMsg, EmailBy, param, ref templateFields, ref mailresult))
                                    {
                                        // Do steps for Mail Send successful
                                        objMsg.EmailStatus = "Success";
                                        objMsg.EmailStatusReason = Convert.ToString(mailresult);
                                        objMsg.ModifiedBy = 0;
                                        objMsg.ModifiedDate = DateTime.Now;
                                    }
                                    else
                                    {
                                        // Do steps for Mail Failure. Mail failure reason can be find in "mailresult"
                                        objMsg.EmailStatus = "Fail";
                                        objMsg.EmailStatusReason = Convert.ToString(mailresult);
                                        objMsg.ModifiedBy = 0;
                                        objMsg.ModifiedDate = DateTime.Now;
                                    }
                                }
                                else
                                {
                                    // Do steps for Parameters not available.Reason can be find in "mailresult"
                                    objMsg.EmailStatus = "Fail";
                                    objMsg.EmailStatusReason = Convert.ToString(mailresult);
                                    objMsg.ModifiedBy = 0;
                                    objMsg.ModifiedDate = DateTime.Now;
                                }
                            }
                            catch (Exception ex)
                            {
                                //ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
                                objMsg.EmailStatus = "Fail";
                                objMsg.EmailStatusReason = Convert.ToString(ex.Message);
                                objMsg.ModifiedBy = 0;
                                objMsg.ModifiedDate = DateTime.Now;
                            }
                            #endregion
                        }
                        else
                        {
                            //   objMsgs.Add("Email not valid: " + email.strEmail);
                            objMsg.EmailStatus = "Fail";
                            objMsg.EmailStatusReason = "To Email id not valid.";
                            objMsg.ModifiedBy = 0;
                            objMsg.ModifiedDate = DateTime.Now;
                        }
                    }
                    else
                    {
                        objMsg.EmailStatus = "Fail";
                        objMsg.EmailStatusReason = "From Email id not valid.";
                        objMsg.ModifiedBy = 0;
                        objMsg.ModifiedDate = DateTime.Now;
                    }
                }
                else
                {
                    objMsg.EmailStatus = "Fail";
                    objMsg.EmailStatusReason = "From Email id not found.";
                    objMsg.ModifiedBy = 0;
                    objMsg.ModifiedDate = DateTime.Now;
                }

                GF_MassMessages updateMsg = Db.GF_MassMessages.FirstOrDefault(x => x.MassMsgId == objMsg.MassMsgId);
                if (updateMsg != null)
                {
                    updateMsg.EmailStatus = objMsg.EmailStatus;
                    updateMsg.EmailStatusReason = objMsg.EmailStatusReason;
                    updateMsg.ModifiedBy = objMsg.ModifiedBy;
                    updateMsg.ModifiedDate = objMsg.ModifiedDate;
                    Db.SaveChanges();
                }

                msgs.Add("====================================");
                msgs.Add("Email To: " + objMsg.SendToUserEmail);
                msgs.Add("Email From: " + EmailBy);
                msgs.Add("Status: " + objMsg.EmailStatus);
                msgs.Add("Reason: " + objMsg.EmailStatusReason);
                msgs.Add("Request From: " + objMsg.MessageBy);
                msgs.Add("Mass Message in Course Id: " + objMsg.CourseId);
                msgs.Add("====================================");

                Thread.Sleep(Convert.ToInt32(System.Web.Configuration.WebConfigurationManager.AppSettings["MailDelayMiliSec"]));

            }
            LogClass.MassMessageLog(msgs);
        }

        /// <summary>
        /// Created By: Ramesh Kalra
        /// Creation On: 21st April, 2015
        /// Description: Function to send Mass Messages
        /// </summary>
        [AcceptVerbs(HttpVerbs.Post)]
        [SessionExpireFilterAttribute]
        public ActionResult SendMassMessages(List<long?> Ids, string msgtitle, string msg, long courseid, string strRequestFrom)
        {
            string strResult = "";
            try
            {
                if (Ids.Count <= 0)
                {
                    return Json(new { statusText = "Error: No user selected." });
                }
                else
                {
                    IQueryable<MassMessages> qryList = (IQueryable<MassMessages>)Session["UsersForMassMessages"];
                    List<GF_MassMessages> lstMsgsSync = new List<GF_MassMessages>();

                    List<MassMessages> lstUsers = qryList.Where(x => Ids.Contains(x.Id)).ToList();
                    Db = new GolflerEntities();
                    foreach (var objUser in lstUsers)
                    {
                        GF_MassMessages objMsg = new GF_MassMessages();
                        objMsg.MessageTitle = msgtitle;
                        objMsg.Message = msg;
                        objMsg.SendToUserID = objUser.userid;
                        objMsg.SendToUserName = objUser.UserName;
                        objMsg.SendToUserEmail = objUser.Email;
                        objMsg.SendToCourseId = courseid;
                        objMsg.SendToCourseName = objUser.CourseName;
                        objMsg.EmailStatus = "Pending";
                        objMsg.SendToUserType = objUser.Type;
                        objMsg.CreatedBy = LoginInfo.UserId;
                        objMsg.CreatedByUser = LoginInfo.FirstName + " " + LoginInfo.LastName;
                        objMsg.CreatedDate = DateTime.Now;
                        objMsg.Status = StatusType.Active;
                        objMsg.MessageBy = strRequestFrom;
                        objMsg.CourseId = LoginInfo.CourseId;
                        Db.GF_MassMessages.Add(objMsg);
                        Db.SaveChanges();

                        lstMsgsSync.Add(objMsg);
                    }

                    #region Async call
                    invSendMassMessages = new delSendMassMessages(CallDelegatedFunctions);
                    invSendMassMessages.BeginInvoke(lstMsgsSync, new AsyncCallback(CallbackDelegatedFunctions), null);

                    #endregion

                    strResult = "success";
                    return Json(new { statusText = strResult });
                }
            }
            catch (Exception ex)
            {
                ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
                return Json(new { statusText = "Exception: " + Convert.ToString(ex.Message) });
            }
        }

        /// <summary>
        /// Created By: Ramesh Kalra
        /// Creation On: 21st April, 2015
        /// Description: Function to Delete Mass Messages
        /// </summary>
        [AcceptVerbs(HttpVerbs.Post)]
        [SessionExpireFilterAttribute]
        public ActionResult DeleteMassMessages(List<long?> Ids)
        {
            string strResult = "";
            try
            {
                if (Ids.Count <= 0)
                {
                    return Json(new { statusText = "Error: No Message(s) selected." });
                }
                else
                {
                    Db = new GolflerEntities();
                    var lstMsgs = Db.GF_MassMessages.Where(x => Ids.Contains(x.MassMsgId)).ToList();


                    foreach (var objMsg in lstMsgs)
                    {
                        objMsg.Status = StatusType.Delete;
                        objMsg.ModifiedBy = LoginInfo.UserId;
                        objMsg.ModifiedDate = DateTime.Now;
                        Db.SaveChanges();
                    }
                    strResult = "success";
                    return Json(new { statusText = strResult });
                }
            }
            catch (Exception ex)
            {
                ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
                return Json(new { statusText = "Exception: " + Convert.ToString(ex.Message) });
            }
        }

        #endregion

        #region Resolution center


        [SessionExpireFilterAttribute]
        public ActionResult ResolutionMessages()
        {
            AccessModule(ModuleValues.inboxAdmin);
            Course objCourse = new Course();
            ViewBag.ResolutionType = new SelectList(ResolutionType.GetResolutionType(), "Tag", "Name");
            //  ViewBag.CourseName = new SelectList(objCourse.GetAllActiveCourses(), "ID", "Course_name", "0");
            ViewBag.CourseName = new SelectList(objCourse.GetCourses("", PartershipStatus.Partner, 0), "ID", "Course_name", "0");

            return View();
        }

        /// <summary>
        /// Created By: Kiran Bala
        /// Creation On: 2 April 2015
        /// Description: Get all resolution messages
        /// </summary>
        [SessionExpireFilterAttribute]
        public ActionResult GetResolutionMessages(string Courseid, string golferID, string status, string fromdate, string todate, string username, string strResolutionType, string sidx, string sord, int? page, int? rows)
        {
            //AccessModule(ModuleValues.AllRights);
            try
            {
                if (Convert.ToString(Courseid) == "") Courseid = "";

                if (Convert.ToString(username) == "") username = "";

                username = string.IsNullOrEmpty(username) ? "" : username;

                var UserObj = new GF_ResolutionCenter();
                var totalRecords = 0;
                DateTime from = string.IsNullOrEmpty(fromdate) ? DateTime.MinValue : Convert.ToDateTime(fromdate);
                DateTime to = string.IsNullOrEmpty(todate) ? DateTime.MaxValue : Convert.ToDateTime(todate);

                var timeZone = CommonFunctions.GetCourseTimeZone(0);

                var rec = UserObj.GetResolutionMessagesForAdmin(Courseid, golferID, status, Convert.ToString(from), Convert.ToString(to), username, strResolutionType, sidx,
                                           sord, page ?? 1, rows ?? 10,
                                           ref totalRecords).AsEnumerable().Select((x =>
                                                    new
                                                    {
                                                        ID = x.ID,
                                                        Status = CommonFunctions.GetLatestStatus(x.ID, x.Status),
                                                        //sentBy = (x.GF_Golfer.FirstName + " " + x.GF_Golfer.LastName),
                                                        x.sentBy,
                                                        // Name =x.GF_Golfer.FirstName+" "+x.GF_Golfer.LastName, //x.GF_CourseInfo.COURSE_NAME,
                                                        LatestReplyBy = CommonFunctions.GetLatestUserName(x.ID, Convert.ToString(x.LatestReplyBy)),
                                                        //courseName = x.GF_CourseInfo.COURSE_NAME,
                                                        x.courseName,
                                                        DoActive = (x.ID != LoginInfo.UserId),
                                                        EID = CommonFunctions.EncryptUrlParam(x.ID),
                                                        CreatedDate = x.CreatedDate != null ? CommonFunctions.DateByTimeZone(timeZone, x.CreatedDate ?? DateTime.UtcNow).ToString("MM/dd/yyyy hh:mm tt") : (x.CreatedDate ?? DateTime.UtcNow).ToString("MM/dd/yyyy hh:mm tt"),
                                                        LatestCreatedDate = CommonFunctions.DateByTimeZone(timeZone, x.LatestCreatedDate).ToString("MM/dd/yyyy hh:mm tt"),
                                                        x.LatestComments,
                                                        x.comment,
                                                        x.strResolutionType,
                                                        SenderType = UserType.GetFullUserType(x.SenderType),
                                                        IsRead = LoginInfo.IsGolferLoginUser ? !x.IsRead : !(x.IsReadByAdmin ?? false)
                                                        //CreatedDate = CommonFunctions.GetLatestDate(x.ID, Convert.ToDateTime(x.CreatedDate))
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
        public ActionResult ResolutionMessagereply(string eid)
        {
            AccessModule(ModuleValues.inboxAdmin);
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

            //return View();
            return View(new GF_ResolutionMessageHistory());
        }

        /// <summary>
        /// Created By: Kiran Bala
        /// Creation On: 2 April 2015
        /// Description: Get all resolution messages replies
        /// </summary>
        [SessionExpireFilterAttribute]
        public ActionResult GetResolutionMessagesHistory(string messageID, string sidx, string sord, int? page, int? rows)
        {
            try
            {

                var UserObj = new GF_ResolutionCenter();
                var totalRecords = 0;
                var DB = new GolflerEntities();
                var rec = UserObj.GetResolutionMessagesHistoryAdmin(Convert.ToInt64(messageID), sidx,
                                           sord, page ?? 1, rows ?? 10,
                                           ref totalRecords).AsEnumerable().Select((x =>
                                                                     new
                                                                     {
                                                                         ID = x.ID,
                                                                         x.Status,
                                                                         x.Message,
                                                                         x.CreatedDate,
                                                                         //courseName=Db.GF_CourseInfo.FirstOrDefault(y => y.ID = x.GF_ResolutionCenter.CourseID).COURSE_NAME,
                                                                         name = x.UserType != "G" ? (Db.GF_AdminUsers.FirstOrDefault(y => y.ID == x.LogUserID).Name) : (Db.GF_Golfer.FirstOrDefault(y => y.GF_ID == x.LogUserID).FirstName)
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

                ResolutionReply(obj.Message, statusReply(obj.Status));

                //string cntrollername = "";
                //if (LoginInfo.LoginUserType == "CK" || LoginInfo.LoginUserType == "CC" || LoginInfo.LoginUserType == "CR" || LoginInfo.LoginUserType == "CA" || LoginInfo.LoginUserType == "CP")
                //{//course....................Golfler/CourseAdmin/
                //    cntrollername = "CourseAdmin";
                //}
                //else if (LoginInfo.LoginUserType == "")
                //{//Golfler/golfer/
                //    cntrollername = "golfer";
                //}

                //return RedirectToAction("ResolutionMessages");

                ViewBag.ErrorMessage = "Reply has been sent successfully.";
                return RedirectToAction("ResolutionMessagereply", new { eid = eidSTR });

            }
            return View(obj);

        }

        public string statusReply(string statusCode)
        {
            string status = "";
            if (statusCode.Trim() == MessageStatusType.Open)
                status = "Open";
            else if (statusCode.Trim() == MessageStatusType.Replyed)
                status = "Replyed";
            else
                status = "Closed";

            return status;
        }

        public void ResolutionReply(string comment, string status)
        {
            try
            {

                #region send email
                IQueryable<GF_EmailTemplatesFields> templateFields = null;
                var param = EmailParams.GetEmailParams(ref Db, "Resolution_Reply", ref templateFields);

                if (param != null)
                {
                    if (!ApplicationEmails.GolferResolutionReply(LoginInfo.UserId, comment, status, LoginInfo.GolferEmail, LoginInfo.UserName, param, ref templateFields))
                    {
                        // Message = String.Format(Resources.Resources.mailerror);
                        // return new Result { Status = 0, Error = "An error occured while sending mail.", record = null };
                    }
                    else
                    {
                        //   return new Result { Status = 1, Error = "Your new password is sent on your email address. Please login with new password.", record = null };
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
        public ActionResult GetUnReadResolutionMsg()
        {
            GF_ResolutionCenter resolutionCenter = new GF_ResolutionCenter();
            long count = resolutionCenter.GetUnReadResolutionMessages();

            return Json(new { status = "success", unReadCount = count }, JsonRequestBehavior.AllowGet);
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

            if (obj.AddCourseResolutionCenter(resolutionCenter))
            {
                return RedirectToAction("ResolutionMessages");
            }

            return View(obj);
        }

        #endregion

        #region Manage Menu Item

        /// <summary>
        /// Created By: Amit Kumar
        /// Created on: 22 April 2015
        /// Description: method for list
        /// </summary>
        [SessionExpireFilterAttribute]
        public ActionResult MenuItemList(string eid)
        {
            AccessModule(ModuleValues.ItemAdmin);

            ViewBag.Message = TempData["Message"];

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
        /// Created By:Arun
        /// Creation On:26 March 2015
        /// Description: Get all Menu User listing
        /// </summary>
        [SessionExpireFilterAttribute]
        public ActionResult GetSubCategoryList(string searchText, long category, long subCategory, string sidx, string sord, int? page, int? rows)
        {
            try
            {
                GF_SubCategory objCat = new GF_SubCategory();
                var totalRecords = 0;
                var rec = objCat.GetSubCatory(searchText, category, subCategory, sidx,
                                           sord, page ?? 1, rows ?? 10,
                                           ref totalRecords).AsEnumerable().Select((x =>
                                                                     new
                                                                     {
                                                                         ID = x.ID,
                                                                         x.Name,
                                                                         x.Status,
                                                                         x.IsActive,
                                                                         x.Category,
                                                                         EID = CommonFunctions.EncryptUrlParam(x.ID),
                                                                         DoActive = "true"//x.IsOccupied ? "false" : "true"
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
        /// Creation On: 22 April 2015
        /// Description: method to get add/edit
        /// </summary>
        [SessionExpireFilterAttribute]
        public ActionResult MenuItemAddEdit(string eid, string type)
        {
            AccessModule(ModuleValues.ItemAdmin);

            long id = CommonFunctions.DecryptUrlParam(eid);

            var subCategory = new GF_SubCategory();

            if (string.IsNullOrEmpty(type))
                subCategory = subCategory.GetSubCategoryByID(id);
            else
                subCategory = subCategory.GetMenuItemsByID(id);

            ViewBag.CategoryList = new SelectList(subCategory.GetCategoryList(), "ID", "Name");
            ViewBag.IsLoginUser = (id != 0 && id == LoginInfo.UserId);

            return View(subCategory);
        }

        /// <summary>
        /// Created By: Amit Kumar
        /// Creation On: 22 April 2015
        /// Description: method to get add/edit
        /// </summary>
        [HttpPost]
        [SessionExpireFilterAttribute]
        public ActionResult MenuItemAddEdit(string eid, GF_SubCategory obj)
        {
            AccessModule(ModuleValues.ItemAdmin);

            long id = CommonFunctions.DecryptUrlParam(eid);

            var isSaved = false;
            var menuItems = new GF_SubCategory();

            ViewBag.CategoryList = new SelectList(menuItems.GetCategoryList(), "ID", "Name");
            ViewBag.IsLoginUser = (id != 0 && id == LoginInfo.UserId);

            long subCatID = 0;

            if (ModelState.IsValid)
            {
                isSaved = menuItems.SaveMenuItem(obj, ref subCatID);
            }
            if (isSaved)
            {
                string module = "Menu Item";
                //TempData["Message"] = CommonFunctions.Message(menuItems.Message, module);
                //return RedirectToAction("MenuItemList");
                ViewBag.Message = CommonFunctions.Message(menuItems.Message, module);

                if (subCatID > 0)
                    return RedirectToAction("MenuItemAddEdit/" + CommonFunctions.EncryptUrlParam(subCatID));
                else
                    return View(obj);
            }
            ViewBag.ErrorMessage = menuItems.Message;

            return View(obj);
        }

        /// <summary>
        /// Created By: Amit Kumar
        /// Created date: 22 APril 2015
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
                AccessModule(ModuleValues.ItemAdmin);

                GF_SubCategory objCat = new GF_SubCategory();

                return objCat.RemoveMenuItemOption(optionID)
                           ? Json(new { statusText = "success", module = "Sub Category", task = "update", message = objCat.Message }, JsonRequestBehavior.AllowGet)
                           : Json(new { statusText = "error", module = "Sub Category", task = "update", message = objCat.Message }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
                return null;
            }
        }

        /// <summary>
        /// Created By: Amit Kumar
        /// Created date: 22 APril 2015
        /// Purpose: Update Menu Item Status
        /// </summary>
        /// <param name="id"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        [SessionExpireFilterAttribute]
        public ActionResult UpdateMenuItemStatus(long id, string status)
        {
            try
            {
                AccessModule(ModuleValues.ItemAdmin);

                GF_SubCategory objCat = new GF_SubCategory();

                return objCat.ChangeMenuItemStatus(id, status)
                           ? Json(new { statusText = "success", module = "Menu Item", task = "update", message = objCat.Message })
                           : Json(new { statusText = "error", module = "Menu Item", task = "update", message = objCat.Message });
            }
            catch (Exception ex)
            {
                ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
                return null;
            }
        }

        /// <summary>
        /// Created By: Amit Kumar
        /// Created on: 22 April 2015
        /// Description: method to bind grid
        /// </summary>
        [SessionExpireFilterAttribute]
        public ActionResult GetMenuItemSubCatWiseList(string searchText, long sCatID, string sidx, string sord, int? page, int? rows)
        {
            try
            {
                var OrderObj = new GF_SubCategory();
                var totalRecords = 0;
                var rec = OrderObj.GetMenuItemSubCatWise(searchText, sCatID, sidx,
                                           sord, page ?? 1, rows ?? 10,
                                           ref totalRecords).AsEnumerable().Select((x =>
                                                                     new
                                                                     {
                                                                         x.ID,
                                                                         x.Name,
                                                                         x.Amount,
                                                                         x.CreatedDate,
                                                                         x.Status,
                                                                         Active = (x.Status == StatusType.Active),
                                                                         DoActive = (x.ID != LoginInfo.UserId),
                                                                         EID = CommonFunctions.EncryptUrlParam(x.ID),
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
        /// Created date: 30 March 2015
        /// Purpose: Update Status
        /// </summary>
        /// <param name="id"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        [SessionExpireFilterAttribute]
        public ActionResult UpdateSubCateogryStatus(long id, string status)
        {
            try
            {
                AccessModule(ModuleValues.AllRights);

                GF_SubCategory objCat = new GF_SubCategory();

                return objCat.ChangeStatus(id, status)
                           ? Json(new { statusText = "success", module = "Sub Category", task = "update", message = objCat.Message })
                           : Json(new { statusText = "error", module = "Sub Category", task = "update", message = objCat.Message });
            }
            catch (Exception ex)
            {
                ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
                return null;
            }
        }

        /// <summary>
        /// Created By: Arun
        /// Creation On: 30 March 2015
        /// Description: method to delete
        /// </summary>
        [AcceptVerbs(HttpVerbs.Post)]
        [SessionExpireFilterAttribute]
        public ActionResult DeleteMenuItems(long[] ids)
        {
            try
            {
                AccessModule(ModuleValues.AllRights);

                if (ids != null)
                {
                    GF_SubCategory objCat = new GF_SubCategory();

                    return objCat.DeleteSubCategory(ids)
                               ? Json(new { statusText = "success", module = "Sub Category", task = "delete" })
                               : Json(new { statusText = "error", module = "Sub Category", task = "delete", message = objCat.Message });
                }
                return Json(new { statusText = "error", module = "Sub Category", task = "select" });
            }
            catch (Exception ex)
            {
                ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
                return null;
            }

        }

        /// <summary>
        /// Created By: Amit Kumar
        /// Created date: 31 July 2015
        /// Purpose: Rename sub category name
        /// </summary>
        /// <param name="eid"></param>
        /// <param name="newName"></param>
        /// <returns></returns>
        [HttpGet]
        [SessionExpireFilterAttribute]
        public ActionResult RenameSubCateogry(string eid, string newName)
        {
            try
            {
                AccessModule(ModuleValues.AllRights);

                GF_SubCategory objCat = new GF_SubCategory();

                long id = CommonFunctions.DecryptUrlParam(eid);

                bool status = false;

                if (id > 0)
                {
                    status = objCat.RenameSubCategory(id, newName);
                }
                else
                {
                    objCat.Message = "Error Occured: Please try again.";
                }

                return Json(new { status = status ? "success" : "fail", message = objCat.Message }, JsonRequestBehavior.AllowGet);
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

        #endregion

        #region Suggested Co-ordinates

        [SessionExpireFilterAttribute]
        public ActionResult SuggestedCoordinates()
        {
            AccessModule(ModuleValues.SuggestedGolfCoordinates);
            return View();
        }

        [SessionExpireFilterAttribute]
        public ActionResult CourseCoordinateSuggestion(string Eid)
        {
            AccessModule(ModuleValues.SuggestedGolfCoordinates);
            Db = new GolflerEntities();
            Int64 intSuggestionId = CommonFunctions.DecryptUrlParam(Eid);
            var objSuggestion = Db.GF_CourseBuilder.FirstOrDefault(x => x.ID == intSuggestionId);

            Int64 intCourseId = Convert.ToInt64(objSuggestion.CourseID);
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
            GF_Golfer objGolfer = new GF_Golfer();
            ViewBag.submittedby = objGolfer.GetGolferNameById(Convert.ToInt64(objSuggestion.CreatedBy));
            ViewBag.submittedOn = objSuggestion.ModifyDate;
            ViewBag.CommentsbyGolfer = Convert.ToString(objSuggestion.Comments);
            ViewBag.CourseBuilderId = intSuggestionId;
            //ViewBag.CoOrdStatus = objSuggestion.Status;
            string coordStatus = "I";
            if (objSuggestion.Status == StatusType.Active)
            {
                coordStatus = ApproveStatusType.Approve;
            }
            if (objSuggestion.Status == StatusType.InActive)
            {
                coordStatus = ApproveStatusType.Pending;
            }
            if (objSuggestion.Status == StatusType.Delete)
            {
                coordStatus = ApproveStatusType.Reject;
            }

            ViewBag.Status = new SelectList(ApproveStatusType.GetStatusTypeList(), "Tag", "Name", coordStatus);

            return View();
        }


        [SessionExpireFilterAttribute]
        public ActionResult GetSuggestedCoordinates(string searchText, string partnerType, string sidx, string sord, int? page, int? rows)
        {
            try
            {
                // Int64 courseid = LoginInfo.CourseId;
                GF_Golfer objGolfer = new GF_Golfer();
                var CourseObj = new GF_CourseBuilder();
                var totalRecords = 0;

                var rec = CourseObj.GetCourseBuilderList(searchText, 0, "forSuperAdmin", sidx,
                                                 sord, page ?? 1, rows ?? 10,
                                                 ref totalRecords).AsEnumerable().Select((x =>
                                                                           new
                                                                           {
                                                                               x.ID,
                                                                               x.GF_CourseInfo.COURSE_NAME,
                                                                               x.GF_CourseInfo.COUNTY,
                                                                               x.GF_CourseInfo.CITY,
                                                                               username = objGolfer.GetGolferNameById(Convert.ToInt64(x.CreatedBy)),
                                                                               x.CreatedDate,
                                                                               EID = CommonFunctions.EncryptUrlParam(x.ID),
                                                                               x.Status,
                                                                               number = CommonFunctions.NumberOfSuggestions(Convert.ToInt64(x.CourseID)),
                                                                               Course_EID = CommonFunctions.EncryptUrlParam(x.GF_CourseInfo.ID),
                                                                               Golfer_EID = CommonFunctions.EncryptUrlParam(Convert.ToInt64(x.CreatedBy)),
                                                                               x.GF_CourseInfo.Country,
                                                                               x.GF_CourseInfo.STATE
                                                                               //x.Comments
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
            catch (Exception ex)
            {
                ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
                return null;
            }
        }

        #endregion

        #region Reports

        #region Commission Report
        CommissionReport objCommisionRpt = new CommissionReport();

        [SessionExpireFilterAttribute]
        public ActionResult CommissionReport()
        {
            AccessModule(ModuleValues.CommissionReportAdmin);
            return View();
        }

        [SessionExpireFilterAttribute]
        public ActionResult ShowCommissionReport(string Courseid, string status, string fromdate, string todate, string username, string paymentType, string sidx, string sord, int? page, int? rows)
        {

            try
            {
                if (Convert.ToString(Courseid) == "undefined" || Courseid == "0")
                    Courseid = "-1";
                Courseid = string.IsNullOrEmpty(Courseid) ? "-1" : Courseid;

                if (!string.IsNullOrEmpty(fromdate) || !string.IsNullOrEmpty(todate) || !string.IsNullOrEmpty(paymentType))
                    Courseid = (string.IsNullOrEmpty(Courseid) || Courseid == "-1") ? "0" : Courseid;

                var totalRecords = 0;

                var rec = objCommisionRpt.GetCommissionReport(Convert.ToInt64(Courseid), fromdate, todate, paymentType, sidx,
                                            sord, page ?? 1, rows ?? 10,
                                            ref totalRecords).AsEnumerable().Select((x =>
                                                                      new
                                                                      {
                                                                          x.OrderID,
                                                                          x.date,
                                                                          x.commissionFee,
                                                                          x.plateformFee,
                                                                          x.CourseName,
                                                                          x.CoursePlatformFee,
                                                                          x.PaymentType,
                                                                          commissionFeeTotal = objCommisionRpt.commissionFeeTotal,
                                                                          plateformFeeTotal = objCommisionRpt.plateformFeeTotal,
                                                                          coursePlatformFeeTotal = objCommisionRpt.coursePlatformFee
                                                                      }
                                                                     ));


                ViewBag.sumCommission = objCommisionRpt.commissionFeeTotal;
                ViewBag.sumPlateForm = objCommisionRpt.plateformFeeTotal;
                ViewBag.sumCoursePlatformFee = objCommisionRpt.coursePlatformFee;

                var jsonData = new
                {
                    total = (totalRecords + rows - 1) / rows,
                    page,
                    records = totalRecords,
                    rows = rec.ToList(),
                    id = ""
                };
                return Json(jsonData, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
                return null;
            }
        }

        public void ExportToExcelCommissionReport(string Courseid, string fromdate, string todate)
        {
            if (Convert.ToString(Courseid) == "undefined")
                Courseid = "";
            Courseid = string.IsNullOrEmpty(Courseid) ? "0" : Courseid;

            var rptExport = objCommisionRpt.GetCommissionReportExport(Convert.ToInt64(Courseid), fromdate, todate);

            var fileName = "CommissionReport_" + DateTime.Now.Ticks.ToString();

            var data = from x in rptExport
                       select new
                       {
                           date = x.date.ToString(),
                           x.commissionFee,
                           x.plateformFee,
                           x.CoursePlatformFee,
                           x.CourseName
                       };

            #region Get Commission Total Result

            List<CommissionReportFromat> lstTotal = new List<CommissionReportFromat>();
            CommissionReportFromat commissionReport = new CommissionReportFromat();
            commissionReport.date = DateTime.Now;
            commissionReport.commissionFeeTotal = objCommisionRpt.commissionFeeTotal;
            commissionReport.plateformFeeTotal = objCommisionRpt.plateformFeeTotal;
            commissionReport.CoursePlatformFee = objCommisionRpt.coursePlatformFee;
            lstTotal.Add(commissionReport);

            var dataTotal = from x in lstTotal
                            select new
                            {
                                date = "Total",
                                commissionFee = x.commissionFeeTotal,
                                plateformFee = x.plateformFeeTotal,
                                x.CoursePlatformFee,
                                CourseName = ""
                            };

            var resultData = data.Union(dataTotal.ToList().AsQueryable());

            #endregion

            WebGrid grid = new WebGrid(source: resultData, canPage: false, canSort: false);

            string gridData = grid.GetHtml(
                columns: grid.Columns(
                        grid.Column("date", "Date", format: (item) => string.Format("{0:g}", item.date)),
                        grid.Column("commissionFee", "Commission Amt"),
                        grid.Column("plateformFee", "Golfer Platform Fee"),
                        grid.Column("CoursePlatformFee", "Course Platform Fee"),
                        grid.Column("CourseName", "Course Name")
                        )
                    ).ToString();

            Response.ClearContent();
            Response.AddHeader("content-disposition", "attachment; filename=" + fileName + ".xls");
            Response.ContentType = "application/ms-excel";
            Response.Write(gridData);
            Response.End();
        }

        #endregion

        #region Analytics

        [SessionExpireFilterAttribute]
        public ActionResult AnalyticsReport()
        {
            AccessModule(ModuleValues.AnalyticsReportAdmin);
            var subCategory = new GF_SubCategory();

            ViewBag.CategoryList = new SelectList(subCategory.GetCategoryList(), "ID", "Name");
            return View();
        }

        /// <summary>
        /// Created By:Arun
        /// Created Date: 25 May 2015
        /// Purpose: Show Analytical report
        /// </summary>
        /// <param name="email"></param>
        /// <param name="fromdate"></param>
        /// <param name="todate"></param>
        /// <param name="category"></param>
        /// <param name="subcategory"></param>
        /// <param name="courseid"></param>
        /// <param name="sidx"></param>
        /// <param name="sord"></param>
        /// <param name="page"></param>
        /// <param name="rows"></param>
        /// <returns></returns>
        [SessionExpireFilterAttribute]
        public ActionResult ShowAnalyticsReport(string email, string fromdate, string todate, string category, string subcategory, string courseid, string sidx, string sord, int? page, int? rows)
        {

            try
            {
                courseid = string.IsNullOrEmpty(courseid) ? "0" : courseid;
                category = string.IsNullOrEmpty(category) ? "0" : category;
                subcategory = string.IsNullOrEmpty(subcategory) ? "0" : subcategory;

                var totalRecords = 0;

                var rec = objCommisionRpt.GetAnalyticsReport(email, fromdate, todate, Convert.ToInt64(category), Convert.ToInt64(subcategory), Convert.ToInt64(courseid), sidx,
                                            sord, page ?? 1, rows ?? 10,
                                            ref totalRecords).AsEnumerable().Select((x =>
                                                                      new
                                                                      {
                                                                          date = CommonFunctions.DateByCourseTimeZone(x.CourseID, x.date).ToString("MM/dd/yyyy hh:mm tt"),
                                                                          //x.CourseID,
                                                                          x.commissionFee,
                                                                          x.plateformFee,
                                                                          x.CourseName,
                                                                          x.CoursePlatformFee,
                                                                          x.Amount,
                                                                          x.OrderID,
                                                                          x.TransactionID,
                                                                          analyticAverageAmt = Math.Round(objCommisionRpt.analyticAverageAmt, 2),
                                                                          analyticPageTotalAmt = Math.Round(objCommisionRpt.analyticPageTotalAmt, 2),
                                                                          analyticTotalAmt = Math.Round(objCommisionRpt.analyticTotalAmt, 2)
                                                                      }
                                                                     ));

                ViewBag.AverageAmt = objCommisionRpt.analyticAverageAmt;
                ViewBag.PageTotalAmt = objCommisionRpt.analyticPageTotalAmt;
                ViewBag.TotalAmt = objCommisionRpt.analyticTotalAmt;

                var jsonData = new
                {
                    total = (totalRecords + rows - 1) / rows,
                    page,
                    records = totalRecords,
                    rows = rec.ToList(),
                    id = ""
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
        /// Created Date: 25 May 2015
        /// Purpose: Export Analytical data
        /// </summary>
        /// <param name="email"></param>
        /// <param name="fromdate"></param>
        /// <param name="todate"></param>
        /// <param name="category"></param>
        /// <param name="subcategory"></param>
        /// <param name="courseid"></param>
        public void ExportToExcelAnalyticalReport(string email, string fromdate, string todate, string category, string subcategory, string courseid)
        {
            courseid = string.IsNullOrEmpty(courseid) ? "0" : courseid;
            category = string.IsNullOrEmpty(category) ? "0" : category;
            subcategory = string.IsNullOrEmpty(subcategory) ? "0" : subcategory;

            var rptExport = objCommisionRpt.GetAnalyticsReportExport(email, fromdate, todate, Convert.ToInt64(category), Convert.ToInt64(subcategory), Convert.ToInt64(courseid));

            var fileName = "AnalyticalReport_" + DateTime.Now.Ticks.ToString();

            var data = from x in rptExport
                       select new
                       {
                           x.date,
                           x.OrderID,
                           x.Amount,
                           x.CourseName,
                           x.TransactionID
                       };

            WebGrid grid = new WebGrid(source: data, canPage: false, canSort: false);

            string gridData = grid.GetHtml(
                columns: grid.Columns(
                        grid.Column("date", "Date", format: (item) => string.Format("{0:g}", item.date)),
                        grid.Column("OrderID", "Order ID"),
                        grid.Column("Amount", "Order Total"),
                        grid.Column("CourseName", "Course Name"),
                        grid.Column("TransactionID", "Transaction ID")
                        )
                    ).ToString();

            Response.ClearContent();
            Response.AddHeader("content-disposition", "attachment; filename=" + fileName + ".xls");
            Response.ContentType = "application/ms-excel";
            Response.Write(gridData);
            Response.End();
        }

        /// <summary>
        /// Created By:Arun
        /// Created Date: 27 May 2015
        /// Purpose: Get Food ordered and game played data for Graph
        /// </summary>
        /// <param name="email"></param>
        /// <param name="fromdate"></param>
        /// <param name="todate"></param>
        /// <param name="courseid"></param>
        /// <returns></returns>

        public ActionResult GetGraphData(string email, string fromdate, string todate, string courseid)
        {
            string type = "";
            JavaScriptSerializer obj = new JavaScriptSerializer();

            courseid = string.IsNullOrEmpty(courseid) ? "0" : courseid;

            var rptExport = objCommisionRpt.GetGraphData(Convert.ToInt64(courseid), fromdate, todate, email, ref type);
            List<object[]> data = new List<object[]>();
            data.Add(new object[] { type, "Food ordered", "Game Played" });
            foreach (var item in rptExport)
            {
                data.Add(new object[] { item.hType, item.Foodordered, item.GamePlay });
            }
            return Json(new { result = obj.Serialize(data), type = type });

        }
        /// <summary>
        /// Created By:Arun
        /// Created Date: 27 May 2015
        /// Purpose: Get rating and Complaint data for Graph
        /// </summary>
        /// <param name="email"></param>
        /// <param name="fromdate"></param>
        /// <param name="todate"></param>
        /// <param name="courseid"></param>
        /// <returns></returns>
        public ActionResult GetRatingGraphData(string email, string fromdate, string todate, string courseid)
        {
            string type = "";
            JavaScriptSerializer obj = new JavaScriptSerializer();
            courseid = string.IsNullOrEmpty(courseid) ? "0" : courseid;


            var rptExport = objCommisionRpt.GetRatingGraphData(Convert.ToInt64(courseid), fromdate, todate, email, ref type);
            List<object[]> data = new List<object[]>();
            data.Add(new object[] { type, "Rating", "Complaint" });
            foreach (var item in rptExport)
            {
                data.Add(new object[] { item.hType, item.Rating, item.Complaint });
            }
            return Json(new { result = obj.Serialize(data), type = type });

        }

        /// <summary>
        /// Created By: Arun
        /// Created date: 27 may 2015
        /// Purpose: Export Game played and food ordered data to Excel file.
        /// </summary>
        /// <param name="email"></param>
        /// <param name="fromdate"></param>
        /// <param name="todate"></param>
        /// <param name="courseid"></param>
        public void ExportToExcelAnalyticalGameplayReport(string email, string fromdate, string todate, string courseid)
        {
            courseid = string.IsNullOrEmpty(courseid) ? "0" : courseid;

            string type = "";
            var rptExport = objCommisionRpt.GetGraphData(Convert.ToInt64(courseid), fromdate, todate, email, ref type);

            var fileName = "AnalyticalGraphGameplayedReport_" + DateTime.Now.Ticks.ToString();

            var data = from x in rptExport
                       select new
                       {
                           x.hType,
                           x.Foodordered,
                           x.GamePlay
                       };

            WebGrid grid = new WebGrid(source: data, canPage: false, canSort: false);

            string gridData = grid.GetHtml(
                columns: grid.Columns(
                        grid.Column("hType", type),
                        grid.Column("Foodordered", "Food Ordered"),
                        grid.Column("GamePlay", "Game Played")

                        )
                    ).ToString();

            Response.ClearContent();
            Response.AddHeader("content-disposition", "attachment; filename=" + fileName + ".xls");
            Response.ContentType = "application/ms-excel";
            Response.Write(gridData);
            Response.End();
        }


        /// <summary>
        /// Created By: Arun
        /// Created date: 27 may 2015
        /// Purpose: Export Rating and Complaint data to Excel file.
        ///
        public void ExportToExcelAnalyticalRatingReport(string email, string fromdate, string todate, string courseid)
        {
            courseid = string.IsNullOrEmpty(courseid) ? "0" : courseid;

            string type = "";
            var rptExport = objCommisionRpt.GetRatingGraphData(Convert.ToInt64(courseid), fromdate, todate, email, ref type);

            var fileName = "AnalyticalGraphRatingReport_" + DateTime.Now.Ticks.ToString();

            var data = from x in rptExport
                       select new
                       {
                           x.hType,
                           x.Rating,
                           x.Complaint
                       };

            WebGrid grid = new WebGrid(source: data, canPage: false, canSort: false);

            string gridData = grid.GetHtml(
                columns: grid.Columns(
                        grid.Column("hType", type),
                        grid.Column("Rating", "Rating"),
                        grid.Column("Complaint", "Complaint(s)")

                        )
                    ).ToString();

            Response.ClearContent();
            Response.AddHeader("content-disposition", "attachment; filename=" + fileName + ".xls");
            Response.ContentType = "application/ms-excel";
            Response.Write(gridData);
            Response.End();
        }

        #endregion

        #endregion

        #region Messaging Center

        /// <summary>
        /// Created By: Amit Kumar
        /// Created Date: 14 Oct 2015
        /// Description: Method to get listing of course admin's
        /// </summary>
        /// <returns></returns>
        [SessionExpireFilterAttribute]
        public ActionResult MessagingCenterList()
        {
            AccessModule(ModuleValues.MessageCenter);

            ViewBag.Message = TempData["Message"];

            return View();
        }

        /// <summary>
        /// Created By: Amit Kumar
        /// Creation On: 14 Oct 2015
        /// Description: method to get list of online course user
        /// </summary>
        [SessionExpireFilterAttribute]
        public ActionResult GetOnlineCourseList(string searchText, string courseID, string status, string userType, string sidx, string sord, int? page, int? rows)
        {
            try
            {
                GF_Golfer obj = new GF_Golfer();

                var totalRecords = 0;

                var list = obj.GetOnlineCourseUserList(searchText, courseID, LoginInfo.UserId, status, userType,
                    sidx, sord, page ?? 1, rows ?? 10, ref totalRecords);

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
        }

        #endregion
    }
}
