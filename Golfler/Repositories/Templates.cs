using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Golfler.Models;


namespace Golfler.Repositories
{
    public class Templates
    {
        public string navfontsize { get; set; }
        public string buttonfontsize { get; set; }

        public string textsize { get; set; }
        public string headingsize { get; set; }




        public GF_Templates ObjTemplates { get; private set; }
        protected GolflerEntities Db;
        public string Message { get; private set; }

        public Templates()
        {
            Db = new GolflerEntities();
        }
        public IQueryable<GF_Templates> GetAllTemplates()
        {
            IQueryable<GF_Templates> list;
            list = Db.GF_Templates.OrderBy(i => i.ID);
            return list;
        }
        public List<GF_Templates> GetTemplatesDetail(Int64 templateId)
        {
            List<GF_Templates> list;
            list = Db.GF_Templates.Where(i => i.ID == templateId).ToList();
            return list;
        }

        //public CSSEditor[] GetCssEditorValues()
        //{
        //    long SessionId = LoginInfo.OrganizationId;
        //    if (SessionId == 0)
        //    {
        //        SessionId = Params.SuperAdminID;
        //    }
        //    var cssEditor = CSSEditorTypes.GetAllKeywords();
        //    List<GF_CssEditorOrganization> listOrg = Db.GF_CssEditorOrganization.Where(i => i.OrganizationId == SessionId).ToList();
        //    var tempId = Db.GF_Organizations.FirstOrDefault(x => x.ID == SessionId).TemplateID;
        //    List<GF_CSSEditorDefault> listDefault = Db.GF_CSSEditorDefault.Where(i => i.TemplateId == tempId).ToList();

        //    foreach (var rec in listDefault)
        //    {
        //        if (listOrg.AsQueryable().Count(x => x.GF_CSSEditorDefault.Keyword == rec.Keyword) == 0)
        //            cssEditor.AsQueryable().FirstOrDefault(x => x.Tag == rec.Keyword).Value = rec.DefaultValue;
        //        else
        //            cssEditor.AsQueryable().FirstOrDefault(x => x.Tag == rec.Keyword).Value =
        //                listOrg.AsQueryable().FirstOrDefault(x => x.GF_CSSEditorDefault.Keyword == rec.Keyword).Value;
        //    }

        //    return cssEditor.ToArray();
        //}

        public List<GF_CourseInfo> GetCourseDetails(Int64 courseID)
        {
            List<GF_CourseInfo> list;
            list = Db.GF_CourseInfo.Where(i => i.ID == courseID).ToList();

            return list;
        }

        public List<GF_CourseInfo> GetOrganizationDescription(Int64 courseID)
        {
            List<GF_CourseInfo> list;
            long? Level = 1; //basic membership
            list = (from res
                    in Db.GF_CourseInfo
                    where res.ID == courseID
                    select res).ToList();

            if (list.Count == 0)
            {
                list = (from res
                    in Db.GF_CourseInfo
                        where res.ID == 3
                        select res).ToList();
            }


            return list;
        }


        public List<GF_SMTPSettings> GetSMTPSettings(Int64 courseID)
        {
            List<GF_SMTPSettings> list;
            list = Db.GF_SMTPSettings.Where(i => i.CourseID == courseID).ToList();
            return list;
        }

        public List<GF_CourseInfo> GetCourseDetails(int courseID)
        {
            List<GF_CourseInfo> list;
            list = Db.GF_CourseInfo.Where(i => i.ID == courseID).ToList();
            return list;
        }

        //public bool SaveTemplate(GF_OrganizationsTemplates org)
        //{
        //    var status = false;
        //    try
        //    {
        //        Db.AddToGF_OrganizationsTemplates(org);
        //        Db.SaveChanges();
        //        status = true;
        //    }
        //    catch (Exception ex)
        //    {
        //        ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
        //        status = false;
        //    }
        //    return status;
        //}

        //public bool UpdateTemplate(Int64 organizationId, string logo, string banner)
        //{
        //    var status = false;
        //    try
        //    {
        //        var org = Db.GF_Organizations.Where(i => i.ID == organizationId).FirstOrDefault();
        //        if (org != null)
        //        {


        //            if (logo.Trim() != "")
        //            {
        //                org.TemplateLogo = logo;
        //            }

        //            if (banner.Trim() != "")
        //            {
        //                org.BannerImage = banner;
        //            }

        //            Db.SaveChanges();
        //            status = true;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
        //        status = false;
        //    }
        //    return status;
        //}

        public bool SaveSMTP(GF_SMTPSettings org)
        {
            var status = false;
            try
            {
                var smtp = Db.GF_SMTPSettings.Where(i => i.CourseID == org.CourseID).FirstOrDefault();
                if (smtp != null)
                {
                    smtp.AdminEmail = org.AdminEmail;
                    smtp.FromEmail = org.FromEmail;
                    smtp.ReplyEmail = org.ReplyEmail;
                    smtp.SMTPHost = org.SMTPHost;
                    smtp.SMTPPassword = org.SMTPPassword;
                    smtp.SMTPPort = org.SMTPPort;
                    smtp.SMTPUserName = org.SMTPUserName;
                    smtp.EnableSsl = org.EnableSsl;
                    smtp.EnableTls = org.EnableTls;
                }
                else
                {
                    Db.GF_SMTPSettings.Add(org);
                }

                Db.SaveChanges();
                status = true;
            }
            catch (Exception ex)
            {
                ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
                status = false;
            }
            return status;
        }
        
        //public bool SaveCssEditorValues(CSSEditor[] cssEditor)
        //{
        //    var status = false;
        //    try
        //    {
        //        long SessionId = LoginInfo.OrganizationId;
        //        if (SessionId == 0)
        //        {
        //            SessionId = Params.SuperAdminID;
        //        }
        //        var listOrg = Db.GF_CssEditorOrganization.Where(i => i.OrganizationId == SessionId);
        //        GF_CssEditorOrganization rec;
        //        foreach (var css in cssEditor)
        //        {
        //            rec = listOrg.FirstOrDefault(x => x.GF_CSSEditorDefault.Keyword == css.Tag);
        //            if (rec != null)
        //                rec.Value = css.Value;
        //            else
        //            {
        //                Db.GF_CssEditorOrganization.AddObject(new GF_CssEditorOrganization()
        //                {
        //                    KeywordId = Db.GF_CSSEditorDefault.FirstOrDefault(x => x.Keyword == css.Tag).ID,
        //                    OrganizationId = SessionId,
        //                    Value = css.Value
        //                });
        //            }
        //        }
        //        Db.SaveChanges();
        //        status = true;
        //    }
        //    catch (Exception ex)
        //    {
        //        ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
        //        status = false;
        //    }
        //    return status;
        //}

        //public bool ResetCssEditorValues(long templateid)
        //{
        //    var status = false;
        //    try
        //    {
        //        long SessionId = LoginInfo.OrganizationId;
        //        if (SessionId == 0)
        //        {
        //            SessionId = Params.SuperAdminID;
        //        }
        //        var listOrg = Db.GF_CssEditorOrganization.Where(i => i.OrganizationId == SessionId);
        //        GF_CssEditorOrganization rec;
        //        foreach (var css in Db.GF_CSSEditorDefault.Where(x => x.TemplateId == templateid).ToList())
        //        {
        //            rec = listOrg.FirstOrDefault(x => x.GF_CSSEditorDefault.Keyword == css.Keyword);
        //            if (rec != null)
        //                rec.Value = css.DefaultValue;
        //            else
        //            {
        //                Db.GF_CssEditorOrganization.AddObject(new GF_CssEditorOrganization()
        //                {
        //                    KeywordId = Db.GF_CSSEditorDefault.FirstOrDefault(x => x.Keyword == css.Keyword).ID,
        //                    OrganizationId = SessionId,
        //                    Value = css.DefaultValue
        //                });
        //            }
        //        }
        //        Db.SaveChanges();
        //        status = true;
        //    }
        //    catch (Exception ex)
        //    {
        //        ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
        //        status = false;
        //    }
        //    return status;
        //}


    }
}