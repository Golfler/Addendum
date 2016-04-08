using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Golfler.Models
{
    [MetadataType(typeof(PageMetaData))]
    public partial class GF_StaticPages
    {
        public GF_StaticPages PageObj { get; private set; }

        public bool Active { get { return Status == StatusType.Active; } set { Status = (value) ? Status = StatusType.Active : Status = StatusType.InActive; } }
        public string Link { get { return CommonFunctions.GetCourseLink(LoginInfo.CourseId).ToLower().Replace("home", "page") + "/" + PageName.Replace(" ", "_"); } }

        protected GolflerEntities Db;

        public string Message { get; private set; }

        #region Constructors

        public GF_StaticPages()
        {
            Db = new GolflerEntities();
        }

        public GF_StaticPages(long? id)
        {
            Db = new GolflerEntities();
            PageObj = Db.GF_StaticPages.FirstOrDefault(u => u.ID == id);
        }

        #endregion

        public bool Save(GF_StaticPages obj)
        {
            try
            {
                //GF_RoleModules objModuleRole = CommonFunctions.GetAccessModule(ModuleValues.StaticPage);

                //if (obj.ID > 0)
                //{
                //    if (!objModuleRole.UpdateFlag)
                //    {
                //        Message = "unaccess";
                //        return false;
                //    }
                //}
                //else
                //{
                //    if (!objModuleRole.AddFlag)
                //    {
                //        Message = "unaccess";
                //        return false;
                //    }
                //}
                if (ValidPage(obj))
                {
                    if (obj.ID > 0)
                    {
                        PageObj = Db.GF_StaticPages.FirstOrDefault(x => x.ID == obj.ID);

                        if (PageObj != null)
                        {
                            PageObj.IsActive = obj.IsActive;
                            PageObj.Status = obj.Status;
                            PageObj.MetaDescription = obj.MetaDescription;
                            PageObj.MetaKeywords = obj.MetaKeywords;
                            PageObj.MetaTitle = obj.MetaTitle;
                            PageObj.PageHTML = obj.PageHTML;
                            PageObj.PageName = obj.PageName;
                            PageObj.ModifyBy = LoginInfo.UserId;
                            PageObj.ModifyDate = DateTime.Now;
                            Message = "update";
                        }
                    }
                    else
                    {
                        obj.CreatedBy = LoginInfo.UserId;
                        obj.CreatedDate = DateTime.Now;
                        Db.GF_StaticPages.Add(obj);
                        Message = "add";
                    }
                    Db.SaveChanges();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), HttpContext.Current.Request);
                return false;
            }
        }

        private bool ValidPage(GF_StaticPages obj)
        {
            Message = string.Empty;
            if (Db.GF_StaticPages.Count(x => x.PageName.ToLower() == obj.PageName.ToLower() &&
                x.ID != obj.ID && x.Status != StatusType.Delete) > 0)
                Message = "Name";
            return Message.Length == 0;
        }

        internal GF_StaticPages GetPage(long? id)
        {
            if (Convert.ToInt64(id) > 0)
                PageObj = Db.GF_StaticPages.FirstOrDefault(x => x.ID == id);
            return PageObj ?? new GF_StaticPages();
        }

        internal IQueryable<GF_StaticPages> GetPages(string filterExpression, string sortExpression, string sortDirection, int pageIndex, int pageSize, ref int totalRecords)
        {
            IQueryable<GF_StaticPages> list
                = !String.IsNullOrWhiteSpace(filterExpression)
                      ? Db.GF_StaticPages.Where(x => x.PageName.ToLower().Contains(filterExpression.ToLower()) && x.Status != StatusType.Delete)
                      : Db.GF_StaticPages.Where(x => x.Status != StatusType.Delete);

            totalRecords = list.Count();

            return list.AsEnumerable().AsQueryable().OrderBy(sortExpression + " " + sortDirection).Skip((pageIndex - 1) * pageSize).Take(pageSize);
        }

        internal bool ChangeStatus(bool status)
        {
            //GF_RoleModules objModuleRole = CommonFunctions.GetAccessModule(ModuleValues.StaticPage);
            //if (!objModuleRole.UpdateFlag)
            //{
            //    Message = "unaccess";
            //    return false;
            //}

            if (PageObj != null)
            {
                PageObj.IsActive = !status;
                PageObj.Status = (PageObj.IsActive ?? false) ? StatusType.Active : StatusType.InActive;
                PageObj.ModifyBy = LoginInfo.UserId;
                PageObj.ModifyDate = DateTime.Now;
                Db.SaveChanges();

                Active = !status;

                return true;
            }
            return false;
        }

        internal bool DeletePages(long[] ids)
        {
           // GF_RoleModules objModuleRole = CommonFunctions.GetAccessModule(ModuleValues.StaticPage);
            //if (!objModuleRole.DeleteFlag)
            //{
            //    Message = "unaccess";
            //    return false;
            //}

            var vouch = Db.GF_StaticPages.Where(x => ids.AsQueryable().Contains(x.ID));
            foreach (var v in vouch)
            {
                v.Status = StatusType.Delete;
                v.ModifyBy = LoginInfo.UserId;
                v.ModifyDate = DateTime.Now;
            }
            Db.SaveChanges();
            return true;
        }
    }

    class PageMetaData
    {
        [Required(ErrorMessage = "Required")]
        [DisplayName("Page Name")]
        [StringLength(100, ErrorMessage = "Page Name cannot be longer than 100 characters.")]
        public string PageName { get; set; }

        [DisplayName("Meta Title")]
        [Required(ErrorMessage = "Required")]
        [StringLength(500, ErrorMessage = "Meta Title cannot be longer than 500 characters.")]
        public string MetaTitle { get; set; }

        [DisplayName("Meta Description")]
        public string MetaDescription { get; set; }

        [DisplayName("Meta Keyword")]
        [Required(ErrorMessage = "Required")]
        [StringLength(500, ErrorMessage = "Meta Keyword cannot be longer than 500 characters.")]
        public string MetaKeywords { get; set; }

        [DisplayName("Page Content")]
        public string PageHTML { get; set; }

        [DisplayName("Is Active")]
        public bool Active { get; set; }
    }
}