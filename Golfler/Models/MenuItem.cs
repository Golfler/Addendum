using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Golfler.Models
{
    [MetadataType(typeof(Metatdata))]
    public partial class GF_SubCategory
    {
        private GolflerEntities _db = null;
        public List<Items> lstItems { get; set; }
        public bool Active { get { return Status == StatusType.Active; } set { Status = (value) ? Status = StatusType.Active : Status = StatusType.InActive; } }
        public string Message { get; set; }
        //public string MenuName { get; set; }
        public string MenuName { get; set; }
        public decimal Amount { get; set; }
        public long MenuItemID { get; set; }

        /// <summary>
        /// Created By:Arun
        /// purpose: get all parent category Details
        /// </summary>
        /// <returns></returns>
        public List<GF_Category> GetAllCategories()
        {
            _db = new GolflerEntities();
            return _db.GF_Category.ToList();
        }

        /// <summary>
        /// Created By:Arun
        /// purpose: get all parent category Details
        /// </summary>
        /// <returns></returns>
        public List<Items> GetAllMenuItems(long Subcatid)
        {
            _db = new GolflerEntities();
            return _db.GF_MenuItems.Where(x => x.SubCategoryID == Subcatid).Select(y => new
            {
                y.Name,
                y.Commission,
                y.Amount
            }).ToList().Select(z => new Items
            {
                Name = z.Name,
                Commission = z.Commission ?? 0,
                Amount = z.Amount ?? 0
            }).ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public GF_SubCategory GetSubBuID(long id)
        {
            _db = new GolflerEntities();
            var obj = _db.GF_SubCategory.FirstOrDefault(x => x.ID == id);
            if (obj == null)
            {
                return obj;
            }
            return obj;
        }

        /// <summary>
        /// Created By:Arun
        /// //Created Date:30 March 2015
        /// Purpose: Add or Update menu items
        /// </summary>
        /// <param name="objsub"></param>
        /// <returns></returns>
        public bool AddOrUpdate(GF_SubCategory objsub)
        {

            GF_RoleModules objModuleRole = new GF_RoleModules();


            if (LoginInfo.IsSuper)
                objModuleRole = CommonFunctions.GetAccessModule(ModuleValues.AllRights);
            //////////////else
            //////////////    objModuleRole = CommonFunctions.GetAccessModule(ModuleValues.Golfer);

            if (!objModuleRole.DeleteFlag)
            {
                Message = "Not Access";// Resources.Resources.unaccess;
                return false;
            }
            _db = new GolflerEntities();
            if (objsub.ID == 0)//Add
            {
                var obj = _db.GF_SubCategory.FirstOrDefault(x => x.Name.Trim() == objsub.Name.Trim());
                if (obj != null)
                {
                    Message = "Sub category already exists.";
                    return false;
                }

                objsub.CreatedBy = Convert.ToString(LoginInfo.UserId);
                objsub.CreatedDate = DateTime.Now;
                objsub.Status = objsub.Active ? StatusType.Active : StatusType.InActive;
                objsub.IsActive = true;
                _db.GF_SubCategory.Add(objsub);
                _db.SaveChanges();

                var subID = objsub.ID;
                foreach (var item in objsub.lstItems)
                {
                    var objitem = new GF_MenuItems();
                    objitem.SubCategoryID = subID;
                    objitem.Name = item.Name;
                    objitem.Commission = item.Commission;
                    objitem.Amount = item.Amount;
                    objitem.Status = StatusType.Active;
                    objitem.IsActive = true;
                    objitem.CreatedBy = Convert.ToString(LoginInfo.UserId);
                    objitem.CreatedDate = DateTime.Now;
                    _db.GF_MenuItems.Add(objitem);
                }
                _db.SaveChanges();
                Message = "Menu Item added successfully.";
            }

            else   //Update
            {

                var obj = _db.GF_SubCategory.FirstOrDefault(x => x.ID == objsub.ID);

                if (obj != null)
                {
                    obj.Name = objsub.Name;
                    obj.CategoryID = objsub.CategoryID;
                    obj.Status = objsub.Active ? StatusType.Active : StatusType.InActive;
                    obj.IsActive = true;
                    obj.ModifyBy = Convert.ToString(LoginInfo.UserId);
                    obj.ModifyDate = DateTime.Now;

                    _db.GF_MenuItems.Where(x => x.SubCategoryID == objsub.ID).ToList().ForEach(x => _db.GF_MenuItems.Remove(x));
                    _db.SaveChanges();

                    foreach (var item in objsub.lstItems)
                    {
                        var objitem = new GF_MenuItems();
                        objitem.SubCategoryID = objsub.ID;
                        objitem.Name = item.Name;
                        objitem.Commission = item.Commission;
                        objitem.Amount = item.Amount;
                        objitem.Status = StatusType.Active;
                        objitem.IsActive = true;
                        objitem.ModifyBy = Convert.ToString(LoginInfo.UserId);
                        objitem.ModifyDate = DateTime.Now;
                        _db.GF_MenuItems.Add(objitem);
                    }
                    _db.SaveChanges();
                    Message = "Menu Item updated successfully.";
                }


            }
            return true;


        }

        public IQueryable<GF_SubCategory> GetSubCatoryOld(string filterExpression, string sortExpression, string sortDirection,
            int pageIndex, int pageSize, ref int totalRecords)
        {
            _db = new GolflerEntities();
            IQueryable<GF_SubCategory> list;

            if (!String.IsNullOrWhiteSpace(filterExpression))
                list = _db.GF_SubCategory.Where(x => (x.Name.ToLower().Contains(filterExpression.ToLower()) &&
                     x.Status != StatusType.Delete));
            else
                list = _db.GF_SubCategory.Where(x => x.Status != StatusType.Delete);

            totalRecords = list.Count();

            return list.OrderBy(sortExpression + " " + sortDirection).Skip((pageIndex - 1) * pageSize).Take(pageSize);
        }

        /// <summary>
        /// Created By:Arun
        /// Created Date:26 March 2015
        /// Purpose: Get Golfer listing
        /// </summary>
        /// <param name="filterExpression"></param>
        /// <param name="sortExpression"></param>
        /// <param name="sortDirection"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalRecords"></param>
        /// <returns></returns>
        public IQueryable<GF_SubCategory> GetSubCatory(string filterExpression, long category, long subCategory, string sortExpression,
            string sortDirection, int pageIndex, int pageSize, ref int totalRecords)
        {
            _db = new GolflerEntities();
            IQueryable<GF_SubCategory> list;

            if (!String.IsNullOrWhiteSpace(filterExpression))
                list = _db.GF_SubCategory.Where(x => x.Name.ToLower().Contains(filterExpression.ToLower()) &&
                     x.Status != StatusType.Delete).ToList()
                     .Select(x => new GF_SubCategory
                     {
                         ID = x.ID,
                         Name = x.Name,
                         Status = x.Status,
                         IsActive = x.IsActive,
                         Category = x.GF_Category.Name,
                         CategoryID = x.CategoryID,
                         IsOccupied = x.GF_CourseFoodItem.Where(y => y.AllowStatus == StatusType.Active).Count() > 0
                     }).AsQueryable();
            else
                list = _db.GF_SubCategory.Where(x => x.Status != StatusType.Delete).ToList()
                     .Select(x => new GF_SubCategory
                     {
                         ID = x.ID,
                         Name = x.Name,
                         Status = x.Status,
                         IsActive = x.IsActive,
                         Category = x.GF_Category.Name,
                         CategoryID = x.CategoryID,
                         IsOccupied = x.GF_CourseFoodItem.Where(y => y.AllowStatus == StatusType.Active).Count() > 0
                     }).AsQueryable();

            if (category > 0)
            {
                list = list.Where(x => x.CategoryID == category);
            }

            if (subCategory > 0)
            {
                list = list.Where(x => x.ID == subCategory);
            }

            totalRecords = list.Count();

            return list.OrderBy(sortExpression + " " + sortDirection).Skip((pageIndex - 1) * pageSize).Take(pageSize);
        }

        /// <summary>
        /// Created By:Arun
        /// Created Date:26 March 2015
        /// Purpose: Chnage Status
        /// </summary>
        /// <param name="id"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        internal bool ChangeStatus(long id, string status)
        {
            try
            {
                //GF_RoleModules objModuleRole = new GF_RoleModules();
                _db = new GolflerEntities();

                var objSubCat = _db.GF_SubCategory.FirstOrDefault(x => x.ID == id);

                //if (LoginInfo.IsSuper)
                //    objModuleRole = CommonFunctions.GetAccessModule(ModuleValues.AllRights);
                //else
                //    objModuleRole = CommonFunctions.GetAccessModule(ModuleValues.Golfer);

                //if (!objModuleRole.UpdateFlag)
                //{
                //    Message = "Not Access";//Resources.Resources.unaccess;
                //    return false;
                //}

                if (objSubCat != null)
                {
                    Message = status == StatusType.Active ? "Sub category deactivated successfully" : "Sub category activated successfully";
                    objSubCat.Status = status == StatusType.Active ? StatusType.InActive : StatusType.Active;
                    objSubCat.IsActive = status == StatusType.Active ? false : true;
                    _db.SaveChanges();

                    return true;
                }
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
                Message = "Error occured. Please try again.";
            }
            return false;
        }

        /// <summary>
        /// Created By:Arun
        /// Created Date:26 March 2015
        /// Purpose: Delete selected user(s).
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        internal bool DeleteSubCategory(long[] ids)
        {
            try
            {
                _db = new GolflerEntities();

                var objsub = _db.GF_SubCategory.Where(x => ids.AsQueryable().Contains(x.ID));
                foreach (var r in objsub)
                {
                    r.Status = StatusType.Delete;
                    r.IsActive = false;
                    //_db.GF_MenuItems.Where(x => x.SubCategoryID == r.ID).ToList().ForEach(y => y.Status = StatusType.Delete);
                }

                //Delete all menu items
                _db.GF_MenuItems.Where(x => ids.AsQueryable().Contains(x.SubCategoryID ?? 0)).ToList()
                    .ForEach(y => y.Status = StatusType.Delete);

                //Delere all course food item
                _db.GF_CourseFoodItem.Where(x => ids.AsQueryable().Contains(x.SubCategoryID ?? 0)).ToList()
                    .ForEach(y => y.AllowStatus = StatusType.Delete);

                _db.SaveChanges();
                return true;
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                return false;
            }
        }

        #region Manage Menu Item

        public GF_SubCategory GetSubCategoryByID(long? id)
        {
            _db = new GolflerEntities();
            GF_SubCategory subCategory = new GF_SubCategory();

            if (Convert.ToInt64(id) > 0)
                subCategory = _db.GF_SubCategory.FirstOrDefault(x => x.ID == id);

            subCategory.ArrMenuItemOption = new GF_MenuItemOption[1];

            return subCategory;
        }

        public GF_SubCategory GetMenuItemsByID(long? id)
        {
            _db = new GolflerEntities();
            GF_SubCategory subCategory = new GF_SubCategory();

            if (Convert.ToInt64(id) > 0)
            {
                var menuItem = _db.GF_MenuItems.FirstOrDefault(x => x.ID == id);

                if (menuItem != null)
                {
                    subCategory.ID = menuItem.SubCategoryID ?? 0;
                    subCategory.CategoryID = menuItem.GF_SubCategory.CategoryID;
                    subCategory.Name = menuItem.GF_SubCategory.Name;
                    subCategory.MenuItemID = menuItem.ID;
                    subCategory.MenuName = menuItem.Name;
                    subCategory.Amount = menuItem.Amount ?? 0;
                    subCategory.Active = menuItem.IsActive ?? false;

                    if (menuItem.GF_MenuItemOption.Where(x => x.Status == StatusType.Active).Count() > 0)
                    {
                        subCategory.ArrMenuItemOption = menuItem.GF_MenuItemOption.Where(x => x.Status == StatusType.Active &&
                            (x.CourseID ?? 0) <= 0).ToArray();

                        //Error: System.IndexOutOfRangeException: Index was outside the bounds of the array.
                        //Solution: If ArrMenuItemOption don't have any active records then assign a blank array
                        if (subCategory.ArrMenuItemOption.Count() <= 0)
                            subCategory.ArrMenuItemOption = new GF_MenuItemOption[1];
                    }
                    else
                        subCategory.ArrMenuItemOption = new GF_MenuItemOption[1];
                }
            }

            return subCategory;
        }

        /// <summary>
        /// Created By: Amit Kumar
        /// Created on: 22 April 2015
        /// </summary>
        /// <remarks>Get Category Listing</remarks>
        public IEnumerable<GF_Category> GetCategoryList()
        {
            _db = new GolflerEntities();
            var lstCat = _db.GF_Category.Where(x => x.Status == StatusType.Active)
                .Select(x => new { x.ID, Name = x.Name }).ToList()
                .Select(y => new GF_Category { ID = y.ID, Name = y.Name }).OrderBy(x => x.Name);

            return lstCat;
        }

        /// <summary>
        /// Created By: Amit Kumar
        /// Created on: 27 May 2015
        /// </summary>
        /// <remarks>Get Sub Category Listing</remarks>
        public IEnumerable<GF_Category> GetSubCategoryList(long catID)
        {
            _db = new GolflerEntities();
            var lstCat = _db.GF_SubCategory.Where(x => x.Status == StatusType.Active &&
                x.CategoryID == catID)
                .Select(x => new { x.ID, Name = x.Name }).ToList()
                .Select(y => new GF_Category { ID = y.ID, Name = y.Name }).OrderBy(x => x.Name);

            return lstCat;
        }

        /// <summary>
        /// Created By: Amit Kumar
        /// Created on: 27 May 2015
        /// </summary>
        /// <remarks>Get Sub Category Listing</remarks>
        public IEnumerable<GF_Category> GetMenuItemsList(long subCatID)
        {
            _db = new GolflerEntities();
            var lstMenuItem = _db.GF_CourseFoodItemDetail.Where(x => x.Status == StatusType.Active &&
                x.GF_CourseFoodItem.SubCategoryID == subCatID &&
                x.GF_CourseFoodItem.CourseID == LoginInfo.CourseId &&
                x.GF_CourseFoodItem.AllowStatus == StatusType.Active)
                .Select(x => new { x.MenuItemID, Name = x.GF_MenuItems.Name }).ToList()
                .Select(y => new GF_Category { ID = y.MenuItemID ?? 0, Name = y.Name }).OrderBy(x => x.Name);

            return lstMenuItem;
        }

        public bool SaveMenuItem(GF_SubCategory obj, ref long id)
        {
            try
            {
                _db = new GolflerEntities();
                GF_SubCategory subCategory = new GF_SubCategory();
                GF_MenuItems menuItems = new GF_MenuItems();

                if (obj.MenuItemID > 0)
                {
                    menuItems = _db.GF_MenuItems.FirstOrDefault(x => x.ID == obj.MenuItemID);

                    if (menuItems != null)
                    {
                        #region Sub Category Detail Update

                        var menuItemCount = _db.GF_MenuItems.Where(x => x.Status != StatusType.Delete &&
                            x.SubCategoryID == obj.ID).Count();

                        subCategory = new GF_SubCategory();
                        subCategory = _db.GF_SubCategory.FirstOrDefault(x => x.ID == obj.ID);
                        subCategory.Name = obj.Name;
                        if (menuItemCount == 0)
                        {
                            subCategory.Status = obj.Active ? StatusType.Active : StatusType.InActive;
                            subCategory.IsActive = obj.Active;
                        }
                        subCategory.ModifyBy = LoginInfo.UserId.ToString();
                        subCategory.ModifyDate = DateTime.Now;
                        subCategory.MenuName = obj.MenuName;
                        _db.SaveChanges();

                        id = subCategory.ID;

                        #endregion

                        menuItems.Name = obj.MenuName;
                        menuItems.Amount = obj.Amount;
                        menuItems.Status = obj.Active ? StatusType.Active : StatusType.InActive;
                        menuItems.IsActive = obj.Active;
                        menuItems.ModifyBy = LoginInfo.UserId.ToString();
                        menuItems.ModifyDate = DateTime.Now;
                    }

                    Message = "update";
                    _db.SaveChanges();

                    #region Add/Update Menu Item Options

                    GF_MenuItemOption menuItemOption = new GF_MenuItemOption();
                    foreach (var item in obj.ArrMenuItemOption)
                    {
                        if (!string.IsNullOrEmpty(item.Name))
                        {
                            menuItemOption = new GF_MenuItemOption();

                            if (item.ID > 0)
                            {
                                menuItemOption = _db.GF_MenuItemOption.FirstOrDefault(x => x.ID == item.ID);
                                menuItemOption.Name = item.Name;
                            }
                            else
                            {
                                menuItemOption.MenuItemID = menuItems.ID;
                                menuItemOption.Name = item.Name;
                                menuItemOption.Status = StatusType.Active;
                                menuItemOption.CourseID = 0;
                                _db.GF_MenuItemOption.Add(menuItemOption);
                            }
                        }
                    }
                    _db.SaveChanges();

                    #endregion
                }
                else
                {
                    #region Duplicate Check

                    var lstMenu = _db.GF_MenuItems.FirstOrDefault(x => x.SubCategoryID == obj.ID &&
                        x.Name.ToLower() == obj.MenuName.ToLower() && x.Status != StatusType.Delete);

                    if (lstMenu != null)
                    {
                        Message = "Duplicate menu item name, please enter different menu item name.";
                        return false;
                    }

                    #endregion

                    #region Sub Category Detail Add

                    subCategory = new GF_SubCategory();
                    subCategory = _db.GF_SubCategory.FirstOrDefault(x => x.ID == obj.ID);
                    if (subCategory == null)
                    {
                        #region Duplicate Check

                        var lstSubMenu = _db.GF_SubCategory.FirstOrDefault(x => x.Name.ToLower() == obj.Name.Trim().ToLower() &&
                            x.Status != StatusType.Delete);

                        if (lstSubMenu != null)
                        {
                            Message = "Duplicate sub category name, please enter different sub category name.";
                            return false;
                        }

                        #endregion

                        subCategory = new GF_SubCategory();
                        subCategory.CategoryID = obj.CategoryID;
                        subCategory.Name = obj.Name;
                        subCategory.Status = obj.Active ? StatusType.Active : StatusType.InActive;
                        subCategory.IsActive = obj.Active;
                        subCategory.CreatedBy = LoginInfo.UserId.ToString();
                        subCategory.CreatedDate = DateTime.Now;
                        subCategory.MenuName = obj.MenuName;
                        _db.GF_SubCategory.Add(subCategory);
                        _db.SaveChanges();

                        id = subCategory.ID;
                    }
                    else
                    {
                        subCategory.Name = obj.Name;
                        subCategory.MenuName = obj.MenuName;

                        var menuItemCount = _db.GF_MenuItems.Where(x => x.Status != StatusType.Delete &&
                            x.SubCategoryID == obj.ID).Count();

                        if (menuItemCount == 0)
                        {
                            subCategory.Status = obj.Active ? StatusType.Active : StatusType.InActive;
                            subCategory.IsActive = obj.Active;
                        }
                        subCategory.ModifyBy = LoginInfo.UserId.ToString();
                        subCategory.ModifyDate = DateTime.Now;
                        _db.SaveChanges();
                    }

                    #endregion

                    menuItems = new GF_MenuItems();
                    menuItems.SubCategoryID = subCategory.ID;
                    menuItems.Name = obj.MenuName;
                    menuItems.Amount = obj.Amount;
                    menuItems.Status = obj.Active ? StatusType.Active : StatusType.InActive;
                    menuItems.IsActive = obj.Active;
                    menuItems.CreatedBy = LoginInfo.UserId.ToString();
                    menuItems.CreatedDate = DateTime.Now;
                    _db.GF_MenuItems.Add(menuItems);
                    _db.SaveChanges();

                    #region Add Menu Item Options

                    GF_MenuItemOption menuItemOption = new GF_MenuItemOption();
                    foreach (var item in obj.ArrMenuItemOption)
                    {
                        if (!string.IsNullOrEmpty(item.Name))
                        {
                            menuItemOption = new GF_MenuItemOption();
                            menuItemOption.MenuItemID = menuItems.ID;
                            menuItemOption.Name = item.Name;
                            menuItemOption.Status = StatusType.Active;
                            menuItemOption.CourseID = 0;
                            _db.GF_MenuItemOption.Add(menuItemOption);
                        }
                    }
                    _db.SaveChanges();

                    #endregion

                    Message = "add";
                }

                return true;
            }
            //catch (Exception ex)
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
                Message = "Error occured. Please try again.";
                return false;
            }
        }

        /// <summary>
        /// Created By: Amit Kumar
        /// Created Date: 23 June 2015
        /// Purpose: Add new Menu Item Option
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        internal bool AddMenuItemOption(MenuItemOption menuItemOption)
        {
            try
            {
                _db = new GolflerEntities();
                GF_MenuItemOption itemOption = new GF_MenuItemOption();
                foreach (var item in menuItemOption.menuOption)
                {
                    if (!string.IsNullOrEmpty(item.Name))
                    {
                        itemOption = new GF_MenuItemOption();

                        if (item.ID > 0)
                        {
                            itemOption = _db.GF_MenuItemOption.FirstOrDefault(x => x.ID == item.ID && x.CourseID == LoginInfo.CourseId);
                            itemOption.Name = item.Name;
                        }
                        else
                        {
                            itemOption.MenuItemID = menuItemOption.menuID;
                            itemOption.Name = item.Name;
                            itemOption.CourseID = LoginInfo.CourseId;
                            itemOption.Status = StatusType.Active;
                            _db.GF_MenuItemOption.Add(itemOption);
                        }
                    }
                }

                Message = "Menu item option added successfully.";

                _db.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Created By: Amit Kumar
        /// Created Date: 21 May 2015
        /// Purpose: Remove Menu Item Option
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        internal bool RemoveMenuItemOption(long id)
        {
            try
            {
                _db = new GolflerEntities();

                var option = _db.GF_MenuItemOption.FirstOrDefault(x => x.ID == id);

                if (option != null)
                {
                    Message = option.Name + " removed successfully.";
                    option.Status = StatusType.Delete;

                    #region Changes Course Option Selected

                    var courseOption = _db.GF_CourseFoodItemOption.FirstOrDefault(x => x.MenuItemOptionID == id && x.CourseID == LoginInfo.CourseId);
                    if (courseOption != null)
                    {
                        courseOption.IsActive = false;
                    }

                    #endregion

                    _db.SaveChanges();

                    return true;
                }
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
                Message = "Error occured. Please try again.";
            }
            return false;
        }

        /// <summary>
        /// Created By: Amit Kumar
        /// Created Date: 24 June 2015
        /// Purpose: Get Menu Item Option added by Course
        /// </summary>
        /// <param name="menuID"></param>
        /// <returns></returns>
        public List<GF_MenuItemOption> GetCourseMenuItemOption(long menuID)
        {
            try
            {
                _db = new GolflerEntities();

                var option = _db.GF_MenuItemOption.Where(x => x.MenuItemID == menuID &&
                    x.Status == StatusType.Active &&
                    x.CourseID == LoginInfo.CourseId).ToList();

                return option;
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
                Message = "Error occured. Please try again.";
            }
            return null;
        }

        /// <summary>
        /// Created By: Amit Kumar
        /// Created on: 22 April 2015
        /// </summary>
        /// <remarks>Get Promo Code Listing</remarks>
        public IQueryable<GF_MenuItems> GetMenuItemSubCatWise(string filterExpression, long sCatID, string sortExpression, string sortDirection, int pageIndex,
            int pageSize, ref int totalRecords)
        {
            _db = new GolflerEntities();
            IQueryable<GF_MenuItems> list;

            if (!String.IsNullOrWhiteSpace(filterExpression))
                list = _db.GF_MenuItems.Where(x => x.Name.ToLower().Contains(filterExpression.ToLower()) &&
                    x.Status != StatusType.Delete &&
                    x.SubCategoryID == sCatID).OrderByDescending(x => x.ID);
            else
                list = _db.GF_MenuItems.Where(x => x.Status != StatusType.Delete &&
                    x.SubCategoryID == sCatID).OrderByDescending(x => x.ID);

            totalRecords = list.Count();

            return list.OrderBy(sortExpression + " " + sortDirection).Skip((pageIndex - 1) * pageSize).Take(pageSize);
        }

        /// <summary>
        /// Created By: Amit Kumar
        /// Created Date: 22 April 2015
        /// Purpose: Chnage Status
        /// </summary>
        /// <param name="id"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        internal bool ChangeMenuItemStatus(long id, string status)
        {
            try
            {
                _db = new GolflerEntities();

                var objMenuItem = _db.GF_MenuItems.FirstOrDefault(x => x.ID == id);

                if (objMenuItem != null)
                {
                    Message = status == StatusType.Active ? "Menu item deactivated successfully" : "Menu item activated successfully";
                    objMenuItem.Status = status == StatusType.Active ? StatusType.InActive : StatusType.Active;
                    objMenuItem.IsActive = status == StatusType.Active ? false : true;
                    _db.SaveChanges();

                    return true;
                }
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
                Message = "Error occured. Please try again.";
            }
            return false;
        }

        public bool RenameSubCategory(long id, string newName)
        {
            _db = new GolflerEntities();
            var subCategory = _db.GF_SubCategory.FirstOrDefault(x => x.ID == id);

            if (subCategory != null)
            {
                #region Duplicate Check

                var duplicate = _db.GF_SubCategory.FirstOrDefault(x => x.Name.Trim().ToLower() == newName.Trim().ToLower() &&
                    x.ID != id);

                if (duplicate != null)
                {
                    Message = "Duplicate sub category name, please enter different sub category name.";
                    return false;
                }

                #endregion

                subCategory.Name = newName;
                _db.Configuration.ValidateOnSaveEnabled = false;
                _db.SaveChanges();

                Message = "Sub category name changed successfully.";
                return true;
            }

            Message = "Error Occured: Please try again.";

            return false;
        }

        #endregion

        #region Manage Food Item in Course Admin

        /// <summary>
        /// Created By: Amit Kumar
        /// Created Date: 23 April 2015
        /// Purpose: Get Golfer listing
        /// </summary>
        /// <param name="filterExpression"></param>
        /// <param name="sortExpression"></param>
        /// <param name="sortDirection"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalRecords"></param>
        /// <returns></returns>
        public IQueryable<GF_SubCategory> GetSubCategory(string filterExpression, long category, long subCategory, string sortExpression,
            string sortDirection, int pageIndex, int pageSize, ref int totalRecords)
        {
            _db = new GolflerEntities();
            IQueryable<GF_SubCategory> list;

            if (!String.IsNullOrWhiteSpace(filterExpression))
            {
                //Inner Search
                var lstMenuItem = _db.GF_CourseFoodItemDetail.Where(x => //x.Status == StatusType.Active &&
                    x.GF_MenuItems.Name.ToLower().Contains(filterExpression.ToLower()) &&
                    x.GF_CourseFoodItem.CourseID == LoginInfo.CourseId &&
                    x.GF_CourseFoodItem.AllowStatus == StatusType.Active).ToList();

                string lstSubCatIDs = string.Join(",", lstMenuItem.Select(x => x.GF_CourseFoodItem.SubCategoryID));

                long[] subCatIDs = CommonFunctions.ConvertStringArrayToLongArray(lstSubCatIDs == "" ? "0" : lstSubCatIDs);

                var listSubCatFilter = string.Join(",", _db.GF_SubCategory.Where(x => (x.Name.ToLower().Contains(filterExpression.ToLower()) &&
                    x.Status != StatusType.Delete)).Select(x => x.ID));

                long[] subCatFilterIDs = CommonFunctions.ConvertStringArrayToLongArray(listSubCatFilter == "" ? "0" : listSubCatFilter);

                long[] conbineSubCatIDs = subCatIDs.Concat(subCatFilterIDs).ToArray();

                //list = _db.GF_SubCategory.Where(x => (x.Name.ToLower().Contains(filterExpression.ToLower()) &&
                list = _db.GF_SubCategory.Where(x => (conbineSubCatIDs.Contains(x.ID) &&
                     x.Status == StatusType.Active)).ToList()
                     .Select(x => new GF_SubCategory
                     {
                         ID = x.ID,
                         Name = x.Name,
                         Status = x.GF_CourseFoodItem.FirstOrDefault(y => y.CourseID == LoginInfo.CourseId) == null ? StatusType.InActive : x.GF_CourseFoodItem.FirstOrDefault(y => y.CourseID == LoginInfo.CourseId).AllowStatus,
                         IsActive = x.GF_CourseFoodItem.FirstOrDefault(y => y.CourseID == LoginInfo.CourseId) == null ? false : x.GF_CourseFoodItem.FirstOrDefault(y => y.CourseID == LoginInfo.CourseId).IsAllow,
                         Category = x.GF_Category.Name,
                         CategoryID = x.CategoryID
                     }).AsQueryable();
            }
            else
                list = _db.GF_SubCategory.Where(x => x.Status == StatusType.Active).ToList()
                     .Select(x => new GF_SubCategory
                     {
                         ID = x.ID,
                         Name = x.Name,
                         Status = x.GF_CourseFoodItem.FirstOrDefault(y => y.CourseID == LoginInfo.CourseId) == null ? StatusType.InActive : x.GF_CourseFoodItem.FirstOrDefault(y => y.CourseID == LoginInfo.CourseId).AllowStatus,
                         IsActive = x.GF_CourseFoodItem.FirstOrDefault(y => y.CourseID == LoginInfo.CourseId) == null ? false : x.GF_CourseFoodItem.FirstOrDefault(y => y.CourseID == LoginInfo.CourseId).IsAllow,
                         Category = x.GF_Category.Name,
                         CategoryID = x.CategoryID
                     }).AsQueryable();

            if (category > 0)
            {
                list = list.Where(x => x.CategoryID == category);
            }

            if (subCategory > 0)
            {
                list = list.Where(x => x.ID == subCategory);
            }

            totalRecords = list.Count();

            return list.OrderBy(sortExpression + " " + sortDirection).Skip((pageIndex - 1) * pageSize).Take(pageSize);
        }

        /// <summary>
        /// Created By: Amit Kumar
        /// Created Date: 23 April 2015
        /// Purpose: Change Status
        /// </summary>
        /// <param name="id"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        internal bool ChangeFoodItemStatus(long id, string status)
        {
            try
            {
                _db = new GolflerEntities();

                var objFoodItem = _db.GF_CourseFoodItem.FirstOrDefault(x => x.SubCategoryID == id && x.CourseID == LoginInfo.CourseId);

                if (objFoodItem != null)
                {
                    Message = status == StatusType.Active ? "Food item deactivated successfully" : "Food item activated successfully";
                    objFoodItem.AllowStatus = status == StatusType.Active ? StatusType.InActive : StatusType.Active;
                    objFoodItem.IsAllow = status == StatusType.Active ? false : true;
                    objFoodItem.ModifyBy = LoginInfo.UserId;
                    objFoodItem.ModifyDate = DateTime.Now;
                    _db.SaveChanges();

                    return true;
                }
                else
                {
                    #region Add Course Menu

                    var lstCourseMenu = _db.GF_CourseMenu.FirstOrDefault(x => x.CourseID == LoginInfo.CourseId);

                    if (lstCourseMenu == null)
                    {
                        var lstCategory = _db.GF_Category.Where(x => x.Status == StatusType.Active);

                        foreach (var item in lstCategory)
                        {
                            GF_CourseMenu courseMenu = new GF_CourseMenu();
                            courseMenu.CategoryID = item.ID;
                            courseMenu.CourseID = LoginInfo.CourseId;
                            courseMenu.CreatedBy = LoginInfo.UserId.ToString();
                            courseMenu.CreatedDate = DateTime.Now;
                            courseMenu.Status = StatusType.Active;
                            courseMenu.IsActive = true;

                            _db.GF_CourseMenu.Add(courseMenu);
                        }
                        _db.SaveChanges();
                    }

                    #endregion

                    var catID = _db.GF_SubCategory.FirstOrDefault(x => x.ID == id).CategoryID;

                    GF_CourseFoodItem obj = new GF_CourseFoodItem();
                    Message = status == StatusType.Active ? "Food item deactivated successfully" : "Food item activated successfully";
                    obj.CourseID = LoginInfo.CourseId;
                    obj.CourseMenuID = _db.GF_CourseMenu.FirstOrDefault(x => x.CategoryID == catID && x.CourseID == LoginInfo.CourseId).ID;
                    obj.SubCategoryID = id;
                    obj.AllowStatus = status == StatusType.Active ? StatusType.InActive : StatusType.Active;
                    obj.IsAllow = status == StatusType.Active ? false : true;
                    obj.CreatedBy = LoginInfo.UserId;
                    obj.CreatedDate = DateTime.Now;
                    _db.GF_CourseFoodItem.Add(obj);
                    _db.SaveChanges();

                    return true;
                }
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
                Message = "Error occured. Please try again.";
            }
            return false;
        }

        public GF_CourseFoodItem GetCourseFoodItemByID(long? id)
        {
            _db = new GolflerEntities();
            GF_CourseFoodItem courseFoodItem = new GF_CourseFoodItem();

            if (Convert.ToInt64(id) > 0)
                courseFoodItem = _db.GF_CourseFoodItem.FirstOrDefault(x => x.SubCategoryID == id && x.CourseID == LoginInfo.CourseId);

            return courseFoodItem;
        }

        public bool SaveCourseFoodItem(GF_CourseFoodItem obj)
        {
            try
            {
                _db = new GolflerEntities();
                GF_CourseFoodItem courseFoodItem = new GF_CourseFoodItem();

                var lstFoodItem = _db.GF_CourseFoodItemDetail.Where(x => x.CourseFoodItemID == obj.ID && x.GF_CourseFoodItem.CourseID == LoginInfo.CourseId).Count();

                if (lstFoodItem > 0)
                {
                    IQueryable<GF_CourseFoodItemDetail> lstDetail = _db.GF_CourseFoodItemDetail
                        .Where(x => x.CourseFoodItemID == obj.ID);

                    foreach (GF_CourseFoodItemDetail item in lstDetail.ToList())
                    {
                        var foodItemDetail = obj.FoodItemDetail.FirstOrDefault(x => x.MenuItemID == item.MenuItemID);
                        if (foodItemDetail != null)
                        {
                            item.Quantity = obj.FoodItemDetail.FirstOrDefault(x => x.MenuItemID == item.MenuItemID).Quantity ?? 0;
                            item.CostPrice = obj.FoodItemDetail.FirstOrDefault(x => x.MenuItemID == item.MenuItemID).CostPrice ?? 0;
                            item.Price = obj.FoodItemDetail.FirstOrDefault(x => x.MenuItemID == item.MenuItemID).Price;
                            item.Status = obj.FoodItemDetail.FirstOrDefault(x => x.MenuItemID == item.MenuItemID).Active ? StatusType.Active : StatusType.InActive;
                            item.IsActive = obj.FoodItemDetail.FirstOrDefault(x => x.MenuItemID == item.MenuItemID).Active;
                            item.ModifyBy = LoginInfo.UserId;
                            item.ModifyDate = DateTime.Now;

                            #region Update Course Selected Option

                            if (obj.FoodItemDetail.FirstOrDefault(x => x.MenuItemID == item.MenuItemID).Itemoption != null)
                            {
                                foreach (var option in obj.FoodItemDetail.FirstOrDefault(x => x.MenuItemID == item.MenuItemID).Itemoption.ToList())
                                {
                                    var itemOption = new GF_CourseFoodItemOption();

                                    itemOption = _db.GF_CourseFoodItemOption.FirstOrDefault(x => x.MenuItemOptionID == option.ID &&
                                        x.CourseID == LoginInfo.CourseId &&
                                        x.ItemDetailID == item.ID);

                                    //Update Old Entry
                                    if (itemOption != null)
                                    {
                                        itemOption.Name = option.Name;
                                        itemOption.IsActive = option.IsSelected;
                                    }
                                    else
                                    {
                                        //Add new entry
                                        if (option.IsSelected)
                                        {
                                            itemOption = new GF_CourseFoodItemOption();
                                            itemOption.ItemDetailID = item.ID;
                                            itemOption.MenuItemOptionID = option.ID;
                                            itemOption.Name = option.Name;
                                            itemOption.IsActive = option.IsSelected;
                                            itemOption.CourseID = LoginInfo.CourseId;
                                            _db.GF_CourseFoodItemOption.Add(itemOption);
                                        }
                                    }
                                    _db.SaveChanges();
                                }
                            }

                            #endregion
                        }
                    }
                    _db.SaveChanges();

                    #region Add new item

                    if (lstDetail.Count() < obj.FoodItemDetail.Count())
                    {
                        foreach (var item in obj.FoodItemDetail)
                        {
                            if (item.Active && item.ID == 0)
                            {
                                var objDetail = new GF_CourseFoodItemDetail();
                                objDetail.CourseFoodItemID = obj.ID;
                                objDetail.MenuItemID = item.MenuItemID;
                                objDetail.Price = item.Price;
                                objDetail.Status = item.Active ? StatusType.Active : StatusType.InActive;
                                objDetail.IsActive = item.Active;
                                objDetail.CreatedBy = LoginInfo.UserId;
                                objDetail.CreatedDate = DateTime.Now;
                                _db.GF_CourseFoodItemDetail.Add(objDetail);

                                #region Add new Course Selected Option while updating

                                if (item.Itemoption != null)
                                {
                                    foreach (var option in item.Itemoption)
                                    {
                                        var itemOption = new GF_CourseFoodItemOption();

                                        if (option.IsSelected)
                                        {
                                            itemOption.ItemDetailID = objDetail.ID;
                                            itemOption.MenuItemOptionID = option.ID;
                                            itemOption.Name = option.Name;
                                            itemOption.IsActive = option.IsSelected;
                                            itemOption.CourseID = LoginInfo.CourseId;
                                            _db.GF_CourseFoodItemOption.Add(itemOption);
                                        }
                                    }
                                }

                                #endregion

                                _db.SaveChanges();
                            }
                        }
                    }

                    #endregion

                    Message = "update";
                }
                else
                {
                    foreach (var item in obj.FoodItemDetail)
                    {
                        if (item.Active)
                        {
                            var objDetail = new GF_CourseFoodItemDetail();
                            objDetail.CourseFoodItemID = obj.ID;
                            objDetail.MenuItemID = item.MenuItemID;
                            objDetail.Quantity = item.Quantity;
                            objDetail.CostPrice = item.CostPrice;
                            objDetail.Price = item.Price;
                            objDetail.Status = item.Active ? StatusType.Active : StatusType.InActive;
                            objDetail.IsActive = item.Active;
                            objDetail.CreatedBy = LoginInfo.UserId;
                            objDetail.CreatedDate = DateTime.Now;
                            _db.GF_CourseFoodItemDetail.Add(objDetail);

                            #region Add Course Selected Option

                            if (item.Itemoption != null)
                            {
                                foreach (var option in item.Itemoption)
                                {
                                    var itemOption = new GF_CourseFoodItemOption();

                                    if (option.IsSelected)
                                    {
                                        itemOption.ItemDetailID = objDetail.ID;
                                        itemOption.MenuItemOptionID = option.ID;
                                        itemOption.Name = option.Name;
                                        itemOption.IsActive = option.IsSelected;
                                        itemOption.CourseID = LoginInfo.CourseId;
                                        _db.GF_CourseFoodItemOption.Add(itemOption);
                                    }
                                }
                            }

                            #endregion

                            _db.SaveChanges();
                        }
                    }


                    Message = "add";
                }

                return true;
            }
            //catch (Exception ex)
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
                Message = "Error occured. Please try again.";
                return false;
            }
        }

        #endregion
    }

    #region Metadata
    public class Metatdata
    {
        [Required(ErrorMessage = "Required")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Required")]
        public string CategoryID { get; set; }

        //[Required(ErrorMessage = "Required")]
        public string MenuName { get; set; }
        //public string MenuName { get { return Name == null ? null : Name; } set { } }

        //[Range(0.0, Double.MaxValue)]
        //[Range(1.0, 9999999999, ErrorMessage = "Menu price should be greater than $1.")]
        [Required(ErrorMessage = "Required")]
        public decimal Amount { get; set; }
    }
    #endregion

    #region Menu Items
    public class Items
    {
        [Required(ErrorMessage = "Required")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Required")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Required")]
        public decimal Commission { get; set; }

    }

    #endregion

    #region Menu Items Meta Data

    [MetadataType(typeof(MenuItemsMetatdata))]
    public partial class GF_MenuItems
    {
        public bool Active { get { return Status == StatusType.Active; } set { Status = (value) ? Status = StatusType.Active : Status = StatusType.InActive; } }
        public long CategoryID { get; set; }
        public string SubCatName { get; set; }
    }

    public class MenuItemsMetatdata
    {
        [Required(ErrorMessage = "Required")]
        public long CategoryID { get; set; }

        [Required(ErrorMessage = "Required")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Required")]
        public string Amount { get; set; }
    }

    #endregion

    #region Course Food Menu Item

    public partial class GF_CourseFoodItem
    {
        public long CategoryID { get; set; }
        public string CatName { get; set; }
        public string SubCatName { get; set; }
        public List<GF_CourseFoodItemDetail> FoodItemDetail { get; set; }
    }

    public partial class GF_CourseFoodItemDetail
    {
        public string ItemName { get; set; }
        public bool Active { get { return Status == StatusType.Active; } set { Status = (value) ? Status = StatusType.Active : Status = StatusType.InActive; } }
        //public string Itemoption { get; set; }
        public List<GF_MenuItemOption> Itemoption { get; set; }
    }


    public partial class GF_SubCategory
    {
        public string Category { get; set; }
        public GF_MenuItemOption[] ArrMenuItemOption { get; set; }
        public bool IsOccupied { get; set; }
    }

    public partial class GF_MenuItemOption
    {
        public bool IsSelected { get; set; }
        //public bool IsSelected { get { return IsSelected ?? false; } set { IsSelected = value; } }
    }

    public class MenuItemOption
    {
        public long menuID { get; set; }
        public IList<SubMenuItemOption> menuOption { get; set; }
    }

    public class SubMenuItemOption
    {
        public long ID { get; set; }
        public string Name { get; set; }
    }

    #endregion
}