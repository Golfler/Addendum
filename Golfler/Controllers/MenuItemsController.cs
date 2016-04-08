using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Golfler.Models;
namespace Golfler.Controllers
{
    public class MenuItemsController : Controller
    {
        GF_SubCategory objCat = null;

        public MenuItemsController()
        {
            objCat = new GF_SubCategory();
        }

        [SessionExpireFilterAttribute]
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Created By:Arun
        /// Created Date:30 March 2015
        /// Purpose"
        /// </summary>
        /// <param name="eid"></param>
        /// <returns></returns>
        [SessionExpireFilterAttribute]
        public ActionResult MenuItemsAddUpdate(string eid)
        {
            //if (LoginInfo.IsSuper)
            //    AccessModule(ModuleValues.AllRights);
            //else
            //    AccessModule(ModuleValues.User);


            ViewBag.Message = TempData["Message"];

            long id = CommonFunctions.DecryptUrlParam(eid);
            if (id > 0)
            {
                objCat = objCat.GetSubBuID(id);
                objCat.Active = objCat.Status==StatusType.Active?true:false;
                objCat.lstItems = objCat.GetAllMenuItems(id);
            }
            else
            {

                if (string.IsNullOrEmpty(eid))
                {
                    var obj = new Items
                    {
                        Name = "",
                        Commission = 0,
                        Amount = 0
                    };
                    objCat.lstItems = new List<Items>();
                    objCat.lstItems.Add(obj);
                }
            }
            ViewBag.Category = new SelectList(objCat.GetAllCategories(), "ID", "Name", objCat.ID);
            return View(objCat);

        }

        /// <summary>
        /// Created By:Arun
        /// Created Date:30 March 2015
        /// Purpose:Add or Update Menu items Info
        /// </summary>
        /// <param name="eid"></param>
        /// <returns></returns>

        [SessionExpireFilterAttribute]
        [HttpPost]
        public ActionResult MenuItemsAddUpdate(GF_SubCategory objSub)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    objSub.AddOrUpdate(objSub);
                    ViewBag.Message = objSub.Message;
                    
                }
            }
            catch (DbEntityValidationException ex)
            {
                foreach (var eve in ex.EntityValidationErrors)
                {
                    ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
                }
                ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
               
            }
            ViewBag.Category = new SelectList(objCat.GetAllCategories(), "ID", "Name", objCat.ID);
            return View(objSub);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [SessionExpireFilterAttribute]
        public ActionResult MenuItemsList()
        {
            return View();
        }


        /// <summary>
        /// Created By:Arun
        /// Creation On:26 March 2015
        /// Description: Get all Menu User listing
        /// </summary>
        [SessionExpireFilterAttribute]
        public ActionResult GetSubCategoryList(string searchText, string sidx, string sord, int? page, int? rows)
        {
            try
            {
                var totalRecords = 0;
                var rec = objCat.GetSubCatory(searchText, 0, 0, sidx,
                                           sord, page ?? 1, rows ?? 10,
                                           ref totalRecords).AsEnumerable().Select((x =>
                                                                     new
                                                                     {
                                                                         ID = x.ID,
                                                                         x.Name,
                                                                         x.Status,
                                                                         x.IsActive,
                                                                         Category = x.GF_Category.Name,
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
                if (ids != null)
                {

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

        #region Access Function

        /// <summary>
        /// Created By:
        /// Creation On:
        /// Description: method for list
        /// </summary>
        private void AccessModule(string module)
        {
            GF_RoleModules m = CommonFunctions.GetAccessModule(module);
            ViewBag.AddFlag = m.AddFlag;
            ViewBag.UpdateFlag = m.UpdateFlag;
            ViewBag.DeleteFlag = m.DeleteFlag;
            ViewBag.ReadFlag = m.ReadFlag;
        }

        #endregion
    }
}
