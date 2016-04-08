using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Golfler.Models;


namespace Golfler.Repositories
{
    public class EmailTemplate
    {
        protected GolflerEntities Db;
        public string Message { get; private set; }

        public EmailTemplate()
        {
            Db = new GolflerEntities();
        }

        public IQueryable<GF_EmailTemplates> GetOrganizationEmailTemplates(long courseid, string filterExpression, string sortExpression, string sortDirection, int pageIndex, int pageSize, ref int totalRecords)
        {
            IQueryable<GF_EmailTemplates> list;

            if (!String.IsNullOrWhiteSpace(filterExpression))
                list = Db.GF_EmailTemplates;//.Where(x => x.TemplateName.ToLower().Contains(filterExpression.ToLower()) && x.CourseID == courseid);
            else
                list = Db.GF_EmailTemplates;//.Where(x => x.CourseID == courseid);

            totalRecords = list.Count();

            return list.OrderBy(sortExpression + " " + sortDirection).Skip((pageIndex - 1) * pageSize).Take(pageSize);
        }
        public List<GF_EmailTemplates> GetTemplatesDetail(long id, ref List<GF_EmailTemplatesFields> fields)
        {
            var lst = new List<GF_EmailTemplates>();
            lst = Db.GF_EmailTemplates.Where(i => i.ID == id).ToList();
            fields = new List<GF_EmailTemplatesFields>();
            //var emailid = lst[0].EmailTemplateId;
            //fields = Db.GF_EmailTemplatesFields.Where(i => i.EmailTemplateId == emailid).ToList();
            return lst;
        }
        public bool UpdateTemplate(long id, string content)
        {
           // GF_RoleModules objModuleRole = CommonFunctions.GetAccessModule(ModuleValues.EmailTemplates);

            var lst = Db.GF_EmailTemplates.Where(i => i.ID == id).FirstOrDefault();
            if (lst != null)
            {
                //if (!objModuleRole.UpdateFlag)
                //{
                //    Message = "unaccess";
                //    return false;
                //}
                lst.MessageBody = content;
                lst.ModifiedOn = DateTime.Now;
                lst.ModifiedBy = LoginInfo.UserId;
                Db.SaveChanges();
                return true;
            }
            else
            {
                return false;
            }
        }
        public bool InsertEmailTemplate(long id)
        {

            var lst = Db.GF_EmailTemplates.FirstOrDefault();
            if (lst == null)
            {
                //Db.ExecuteStoreCommand(@"Insert Into dbo.GF_EmailTemplates Select '" + id + "' as courseid, e.TemplateName,e.MessageBody ,e.MessageBody as orginal,e.Id,NULL as moddate,NULL as modby from dbo.GF_EmailTemplates e");
                return true;
            }
            else
            {
                return true;
            }
        }

        public IQueryable<GF_CourseEmailTemplates> GetAdminEmailTemplates(string filterExpression, string sortExpression, string sortDirection, int pageIndex, int pageSize, ref int totalRecords)
        {
            IQueryable<GF_CourseEmailTemplates> list;

            if (!String.IsNullOrWhiteSpace(filterExpression))
                list = Db.GF_CourseEmailTemplates.Where(x => x.TemplateName.ToLower().Contains(filterExpression.ToLower()) && x.CourseID==LoginInfo.CourseId);
            else
                list = Db.GF_CourseEmailTemplates.Where(x =>x.CourseID==LoginInfo.CourseId);

            totalRecords = list.Count();

            return list.OrderBy(sortExpression + " " + sortDirection).Skip((pageIndex - 1) * pageSize).Take(pageSize);
        }
        public bool UpdateEmailTemplate(long id, string content)
        {
           // GF_RoleModules objModuleRole = CommonFunctions.GetAccessModule(ModuleValues.EmailTemplates);

            var lst = Db.GF_CourseEmailTemplates.Where(i => i.ID == id).FirstOrDefault();
            if (lst != null)
            {
                //if (!objModuleRole.UpdateFlag)
                //{
                //    Message = "unaccess";
                //    return false;
                //}
                lst.MessageBody = content;
                lst.ModifiedOn = DateTime.Now;
                lst.ModifiedBy = LoginInfo.UserId;
                Db.SaveChanges();
                return true;
            }
            else
            {
                return false;
            }
        }
        public List<GF_CourseEmailTemplates> GetAdminTemplatesDetail(long id, ref List<GF_EmailTemplatesFields> fields)
        {
            var lst = new List<GF_CourseEmailTemplates>();
            lst = Db.GF_CourseEmailTemplates.Where(i => i.ID == id).ToList();
            fields = new List<GF_EmailTemplatesFields>();
            var emailid = Db.GF_CourseEmailTemplates.FirstOrDefault(x=>x.ID==id).EmailTemplateId;

            fields = Db.GF_EmailTemplatesFields.Where(i => i.EmailTemplateId == emailid).ToList();
            return lst;
        }

        public IQueryable<GF_EmailTemplates> GetMainEmailTemplates(string filterExpression, string sortExpression, string sortDirection, int pageIndex, int pageSize, ref int totalRecords)
        {
            IQueryable<GF_EmailTemplates> list;

            if (!String.IsNullOrWhiteSpace(filterExpression))
                list = Db.GF_EmailTemplates.Where(x => x.TemplateName.ToLower().Contains(filterExpression.ToLower()));
            else
                list = Db.GF_EmailTemplates;

            totalRecords = list.Count();

            return list.OrderBy(sortExpression + " " + sortDirection).Skip((pageIndex - 1) * pageSize).Take(pageSize);
        }
        public bool UpdateMainEmailTemplate(long id, string content)
        {
          

            var lst = Db.GF_EmailTemplates.Where(i => i.ID == id).FirstOrDefault();
            if (lst != null)
            {
               
                lst.MessageBody = content;
                lst.ModifiedOn = DateTime.Now;
                lst.ModifiedBy = LoginInfo.UserId;
                Db.SaveChanges();
                return true;
            }
            else
            {
                return false;
            }
        }
        public List<GF_EmailTemplates> GetMainTemplatesDetail(long id, ref List<GF_EmailTemplatesFields> fields)
        {
            var lst = new List<GF_EmailTemplates>();
            lst = Db.GF_EmailTemplates.Where(i => i.ID == id).ToList();
            fields = new List<GF_EmailTemplatesFields>();
            var emailid = Db.GF_EmailTemplates.FirstOrDefault(x => x.ID == id).ID;

            fields = Db.GF_EmailTemplatesFields.Where(i => i.EmailTemplateId == emailid).ToList();
            return lst;
        }
    }
}