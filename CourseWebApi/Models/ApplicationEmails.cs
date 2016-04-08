using System;
using System.Collections;
using CourseWebApi.Models;
using System.Web;
using System.Linq;
using System.Collections.Generic;
using CourseWebAPI.Models;
using System.Configuration;

namespace CourseWebApi.Models
{
    public class ApplicationEmails
    {

        private static bool SendMail(EmailParams emailParams, ref string mailresult)
        {

            return MailClass.SendMail(emailParams, ref mailresult);
        }

        #region CourseUser Mails

        internal static bool CourseUserForGotMail(GF_AdminUsers objUser, string newpassword, EmailParams param, ref IQueryable<GF_EmailTemplatesFields> templateFields, long courseId)
        {

            param.Subject = "CourseUser" + " : Forgot Password";
            //string courseId = "";
            //string SMTPHost = "";
            //string SMTPPassword = "";
            //string SMTPUserName = "";
            //string FromEmail = "";
            //string SMTPPort = "";
            //string AdminEmail = "";
            //string EnableSsl = "";

          //  if (GetSMTPSettings(objUser, ref courseId, ref SMTPHost, ref SMTPPassword, ref  SMTPUserName, ref FromEmail, ref  SMTPPort, ref AdminEmail, ref EnableSsl,true))
           // {
             //   param.FromEmail = FromEmail;
              //  param.SmtpHost = SMTPHost;
              //  param.SmtpUserName = SMTPUserName;
             //   param.SmtpPassword = SMTPPassword;
              //  param.SmtpPort = Convert.ToInt32(SMTPPort);
             //   param.EnableSsl = Convert.ToBoolean(EnableSsl);
                param.ToEmail = objUser.Email;
                var htbl = new Hashtable();

                foreach (var fld in templateFields)
                {
                    switch (fld.FieldName.ToString())
                    {
                        case "##FullName##":
                            htbl[fld.FieldName] = objUser.FirstName + " " + objUser.LastName;
                            break;
                        case "##Password##":
                            htbl[fld.FieldName] = newpassword;
                            break;
                        case "##UserName##":
                            htbl[fld.FieldName] = objUser.Email;
                            break;
                        case "##Footer##":
                            htbl[fld.FieldName] = getCourseFooterForEmails(courseId);
                            break;
                        case "##Logo##":
                            htbl[fld.FieldName] = ConfigClass.LogoUrl;
                            break;

                    }
                }
                param.Htbl = htbl;
                string mailresult = "";
                return SendMail(param, ref mailresult);
           // }
            //else
            //{
            //    return false;
            //}
        }
        internal static bool CourseUserRegistrationMail(GF_AdminUsers objUser, EmailParams param, ref IQueryable<GF_EmailTemplatesFields> templateFields, string subject)
        {
            param.Subject = subject;
            //string courseId = "";
            //string SMTPHost = "";
            //string SMTPPassword = "";
            //string SMTPUserName = "";
            //string FromEmail = "";
            //string SMTPPort = "";
            //string AdminEmail = "";
            //string EnableSsl = "";

            //if (GetSMTPSettings(objUser, ref courseId, ref SMTPHost, ref SMTPPassword, ref  SMTPUserName, ref FromEmail, ref  SMTPPort, ref AdminEmail, ref EnableSsl,false))
            //{
                //param.FromEmail = FromEmail;
                //param.SmtpHost = SMTPHost;
                //param.SmtpUserName = SMTPUserName;
                //param.SmtpPassword = SMTPPassword;
                //param.SmtpPort = Convert.ToInt32(SMTPPort);
                //param.EnableSsl = false;
                param.ToEmail = objUser.Email;
                var htbl = new Hashtable();
                foreach (var fld in templateFields)
                {
                    switch (fld.FieldName.ToString())
                    {


                        case "##FullName##":
                            htbl[fld.FieldName] = objUser.FirstName + " " + objUser.LastName;
                            break;
                        case "##Password##":
                            htbl[fld.FieldName] = CommonFunctions.DecryptString(objUser.Password, objUser.SALT);
                            break;
                        case "##UserName##":
                            htbl[fld.FieldName] = objUser.UserName;
                            break;
                        case "##Footer##":
                            htbl[fld.FieldName] = getCourseFooterForEmails(0);
                            break;
                        case "##Logo##":
                            htbl[fld.FieldName] = ConfigClass.LogoUrl;
                            break;
                        case "##Email##":
                            htbl[fld.FieldName] = objUser.Email;
                            break;
                        case "##Phone##":
                            htbl[fld.FieldName] = objUser.Phone;
                            break;
                        case "##Name##":
                            htbl[fld.FieldName] = objUser.FirstName;
                            break;
                           
                    }
                }
                param.Htbl = htbl;
                string mailresult = "";
                return SendMail(param, ref mailresult);
            //}
            //else
            //{
            //    return true;
            //}
        }


        #endregion

        public static bool GetSMTPSettings(GF_AdminUsers objUser, ref string courseId, ref string SMTPHost, ref string SMTPPassword, ref string SMTPUserName, ref string FromEmail, ref string SMTPPort, ref string AdminEmail, ref string EnableSsl, bool isForgotMail)
        {
            bool result = false;
            GolflerEntities _db = new GolflerEntities();
            try
            {
                if (objUser != null)
                {
                    //   var objCourse = _db.GF_CourseUsers.FirstOrDefault(x => x.UserID == objUser.ID);
                    var objCourse = _db.GF_AdminUsers.FirstOrDefault(x => x.ID == objUser.ID);
                    if (objCourse != null)
                    {
                        var objSMTP = _db.GF_SMTPSettings.FirstOrDefault(x => x.CourseID == objCourse.CourseId);
                        if (objSMTP != null)
                        {
                            courseId = Convert.ToString(objSMTP.CourseID);
                            SMTPHost = objSMTP.SMTPHost;
                            SMTPPassword = objSMTP.SMTPPassword;
                            SMTPUserName = objSMTP.SMTPUserName;
                            FromEmail = objSMTP.FromEmail;
                            SMTPPort = objSMTP.SMTPPort;
                            AdminEmail = objSMTP.AdminEmail;
                            EnableSsl = Convert.ToString(objSMTP.EnableSsl);
                            result = true;
                        }
                        else if (isForgotMail)
                        {
                            //
                            //get from super admin'

                            var objSMTPSet = _db.GF_SMTPSettings.FirstOrDefault(x => x.CourseID == 0);
                            if (objSMTP != null)
                            {
                                courseId = Convert.ToString(objSMTPSet.CourseID);
                                SMTPHost = objSMTPSet.SMTPHost;
                                SMTPPassword = objSMTPSet.SMTPPassword;
                                SMTPUserName = objSMTPSet.SMTPUserName;
                                FromEmail = objSMTPSet.FromEmail;
                                SMTPPort = objSMTPSet.SMTPPort;
                                AdminEmail = objSMTPSet.AdminEmail;
                                EnableSsl = Convert.ToString(objSMTPSet.EnableSsl);
                                result = true;
                            }
                            else
                            {
                                courseId = "";
                                SMTPHost = "";
                                SMTPPassword = "";
                                SMTPUserName = "";
                                FromEmail = "";
                                SMTPPort = "";
                                AdminEmail = "";
                                EnableSsl = "";
                                result = false;
                            }
                            //
                        }
                        else
                        {
                            courseId = "";
                            SMTPHost = "";
                            SMTPPassword = "";
                            SMTPUserName = "";
                            FromEmail = "";
                            SMTPPort = "";
                            AdminEmail = "";
                            EnableSsl = "";
                            result = false;
                        }
                    }
                    else
                    {
                        //get from super admin'

                        var objSMTP = _db.GF_SMTPSettings.FirstOrDefault(x => x.CourseID == 0);
                        if (objSMTP != null)
                        {
                            courseId = Convert.ToString(objSMTP.CourseID);
                            SMTPHost = objSMTP.SMTPHost;
                            SMTPPassword = objSMTP.SMTPPassword;
                            SMTPUserName = objSMTP.SMTPUserName;
                            FromEmail = objSMTP.FromEmail;
                            SMTPPort = objSMTP.SMTPPort;
                            AdminEmail = objSMTP.AdminEmail;
                            EnableSsl = Convert.ToString(objSMTP.EnableSsl);
                            result = true;
                        }
                        else
                        {
                            courseId = "";
                            SMTPHost = "";
                            SMTPPassword = "";
                            SMTPUserName = "";
                            FromEmail = "";
                            SMTPPort = "";
                            AdminEmail = "";
                            EnableSsl = "";
                            result = false;
                        }


                    }
                }

                else
                {

                    courseId = "";
                    SMTPHost = "";
                    SMTPPassword = "";
                    SMTPUserName = "";
                    FromEmail = "";
                    SMTPPort = "";
                    AdminEmail = "";
                    EnableSsl = "";
                    result = false;
                }

            }
            catch
            {
                courseId = "";
                SMTPHost = "";
                SMTPPassword = "";
                SMTPUserName = "";
                FromEmail = "";
                SMTPPort = "";
                AdminEmail = "";
                EnableSsl = "";
                result = false;
            }
            return result;
        }

        public static string getCourseFooterForEmails(long courseId)
        {
            string FooterHtml = "";
            try
            {

                GolflerEntities _db = new GolflerEntities();
                if (courseId > 0)
                {
                    string CourseAdminName = _db.GF_AdminUsers.Where(x => x.CourseId == courseId && x.Type == "CA").Select(x => x.FirstName + " " + x.LastName).FirstOrDefault();

                    var objCourse = _db.GF_CourseInfo.FirstOrDefault(x => x.ID == courseId);
                    string address = objCourse.ADDRESS + ", " + objCourse.CITY + ", " + objCourse.STATE + ", " + objCourse.COUNTY + ", " + objCourse.ZIPCODE;
                    string CourseAdminPhone = objCourse.PHONE;

                    FooterHtml = "<td style='font-size:11pt; color:#000; font-family:calibri; padding: 10px; line-height: 20px; text-align: justify;'>";
                    FooterHtml = FooterHtml + "Best,<br><br>";
                    FooterHtml = FooterHtml + "<strong>" + CourseAdminName + "</strong><br>";
                    FooterHtml = FooterHtml + "Course Admin";
                    FooterHtml = FooterHtml + "<br>";
                    FooterHtml = FooterHtml + "<strong style='color:#43b34a'>" + address + "</strong>";
                    FooterHtml = FooterHtml + "<br>";
                    FooterHtml = FooterHtml + CourseAdminPhone;
                    FooterHtml = FooterHtml + "<br>";
                    FooterHtml = FooterHtml + "</td>";
                }
                else
                {
                    FooterHtml = "<td style='font-size:11pt; color:#000; font-family:calibri; padding: 10px; line-height: 20px; text-align: justify;'>";
                    FooterHtml = FooterHtml + "Best,<br><br>";
                    FooterHtml = FooterHtml + "<strong>" + ConfigurationManager.AppSettings["EmailAdminName"] + "</strong><br>";
                    FooterHtml = FooterHtml + ConfigurationManager.AppSettings["EmailAdminType"];
                    FooterHtml = FooterHtml + "<br>";
                    FooterHtml = FooterHtml + "<strong style='color:#43b34a'>" + ConfigurationManager.AppSettings["EmailAdminAddress"] + "</strong>";
                    FooterHtml = FooterHtml + "<br>";
                    FooterHtml = FooterHtml + ConfigurationManager.AppSettings["EmailAdminPhone"];
                    FooterHtml = FooterHtml + "<br>";
                    FooterHtml = FooterHtml + "</td>";

                }

            }
            catch
            {
                FooterHtml = "<td style='font-size:11pt; color:#000; font-family:calibri; padding: 10px; line-height: 20px; text-align: justify;'>";
                FooterHtml = FooterHtml + "Best,<br><br>";
                FooterHtml = FooterHtml + "<strong>" + ConfigurationManager.AppSettings["EmailAdminName"] + "</strong><br>";
                FooterHtml = FooterHtml + ConfigurationManager.AppSettings["EmailAdminType"];
                FooterHtml = FooterHtml + "<br>";
                FooterHtml = FooterHtml + "<strong style='color:#43b34a'>" + ConfigurationManager.AppSettings["EmailAdminAddress"] + "</strong>";
                FooterHtml = FooterHtml + "<br>";
                FooterHtml = FooterHtml + ConfigurationManager.AppSettings["EmailAdminPhone"];
                FooterHtml = FooterHtml + "<br>";
                FooterHtml = FooterHtml + "</td>";
            }
            return FooterHtml;
        }
    }
}