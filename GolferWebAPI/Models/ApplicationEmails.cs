using System;
using System.Collections;

using System.Web;
using System.Linq;
using System.Collections.Generic;
using GolferWebAPI.Models;
using System.Configuration;

namespace GolferWebAPI.Models
{
    public class ApplicationEmails
    {
        public ApplicationEmails()
        {
            GolflerEntities _db = new GolflerEntities();
        }
        private static bool SendMail(EmailParams emailParams, ref string mailresult)
        {


            return MailClass.SendMail(emailParams, ref mailresult);
        }


        #region Golfer User Mails

        internal static bool GolferUserForGotMail(GF_Golfer objUser, string newpassword, EmailParams param, ref IQueryable<GF_EmailTemplatesFields> templateFields, long courseId)
        {
            param.Subject = "Golfer" + " : Forgot Password";
            //string courseId = "";
            //string SMTPHost = "";
            //string SMTPPassword = "";
            //string SMTPUserName = "";
            //string FromEmail = "";
            //string SMTPPort = "";
            //string AdminEmail = "";
            //string EnableSsl = "";

            //if (GetSMTPSettings(objUser, ref courseId, ref SMTPHost, ref SMTPPassword, ref  SMTPUserName, ref FromEmail, ref  SMTPPort, ref AdminEmail, ref EnableSsl))
            //{
            //param.FromEmail = FromEmail;
            //param.SmtpHost = SMTPHost;
            //param.SmtpUserName = SMTPUserName;
            //param.SmtpPassword = SMTPPassword;
            //param.SmtpPort = Convert.ToInt32(SMTPPort);
            //param.EnableSsl = Convert.ToBoolean(EnableSsl);
            param.ToEmail = objUser.Email;
            var htbl = new Hashtable();

            try
            { }
            catch
            {

            }
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

        }
        internal static bool EndUserRegistrationMail(GF_Golfer objUser, EmailParams param, ref IQueryable<GF_EmailTemplatesFields> templateFields)
        {
            param.Subject = "Golfer" + ": Registration";
            //string courseId = "";
            //string SMTPHost = "";
            //string SMTPPassword = "";
            //string SMTPUserName = "";
            //string FromEmail = "";
            //string SMTPPort = "";
            //string AdminEmail = "";
            //string EnableSsl = "";

            //if (GetSMTPSettings(objUser, ref courseId, ref SMTPHost, ref SMTPPassword, ref  SMTPUserName, ref FromEmail, ref  SMTPPort, ref AdminEmail, ref EnableSsl))
            //{
            //param.FromEmail = FromEmail;
            //param.SmtpHost = SMTPHost;
            //param.SmtpUserName = SMTPUserName;
            //param.SmtpPassword = SMTPPassword;
            //param.SmtpPort = Convert.ToInt32(SMTPPort);
            //param.EnableSsl = false;
            if (templateFields != null)
            {
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
                            htbl[fld.FieldName] = CommonFunctions.DecryptString(objUser.Password, objUser.Salt);
                            break;
                        case "##UserName##":
                            htbl[fld.FieldName] = objUser.Email;
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
                            htbl[fld.FieldName] = objUser.MobileNo;
                            break;
                        case "##Name##":
                            htbl[fld.FieldName] = objUser.FirstName;
                            break;


                    }
                }
                param.Htbl = htbl;
                string mailresult = "";
                return SendMail(param, ref mailresult);
            }
            else
            {
                return false;
            }
            //}
            //else
            //{
            //    return true;
            //}
        }
        internal static bool EndUserPlaceOrderMail(GF_Golfer objUser, EmailParams param, ref IQueryable<GF_EmailTemplatesFields> templateFields, string orderNumber, GF_Order objOrder)
        {
            GolflerEntities _db = new GolflerEntities();
            param.Subject = "Golfer" + ": Order Place";
            //string courseId = "";
            //string SMTPHost = "";
            //string SMTPPassword = "";
            //string SMTPUserName = "";
            //string FromEmail = "";
            //string SMTPPort = "";
            //string AdminEmail = "";
            //string EnableSsl = "";

            //if (GetSMTPSettings(objUser, ref courseId, ref SMTPHost, ref SMTPPassword, ref  SMTPUserName, ref FromEmail, ref  SMTPPort, ref AdminEmail, ref EnableSsl))
            //{
            //    param.FromEmail = FromEmail;
            //    param.SmtpHost = SMTPHost;
            //    param.SmtpUserName = SMTPUserName;
            //    param.SmtpPassword = SMTPPassword;
            //    param.SmtpPort = Convert.ToInt32(SMTPPort);
            //    param.EnableSsl = Convert.ToBoolean(EnableSsl);
            string CourseAdminName = "Course Admin";
            string itemDeatilsHtml = "";
            try
            {
                CourseAdminName = _db.GF_AdminUsers.FirstOrDefault(x => x.CourseId == objOrder.GF_CourseInfo.ID && x.Type == "CA").UserName;

                foreach (var item in objOrder.MenuItems)
                {
                    string itemName = _db.GF_MenuItems.FirstOrDefault(x => x.ID == item.ItemId).Name;
                    itemDeatilsHtml = itemDeatilsHtml + "<tr><td style='border:1px solid #ccc; border-top:none; padding:5px;'>" + itemName + "</td>";
                    itemDeatilsHtml = itemDeatilsHtml + "<td style='border-right:1px solid #ccc; border-bottom:1px solid #ccc; padding:5px; text-align:right;'>" + item.Quantity + "</td>";
                    itemDeatilsHtml = itemDeatilsHtml + "<td style='border-right:1px solid #ccc; border-bottom:1px solid #ccc; padding:5px; text-align:right;'>$" + item.UnitPrice + "</td>";
                    itemDeatilsHtml = itemDeatilsHtml + "<td style='border-right:1px solid #ccc; border-bottom:1px solid #ccc; padding:5px; text-align:right;'>$" + (item.UnitPrice * item.Quantity) + "</td>";
                    itemDeatilsHtml = itemDeatilsHtml + "</tr>";


                }

            }
            catch { }
            //objOrder.GF_CourseInfo.ID
            param.ToEmail = objUser.Email;
            var htbl = new Hashtable();

            foreach (var fld in templateFields)
            {
                switch (fld.FieldName.ToString())
                {
                    case "##Logo##":
                        htbl[fld.FieldName] = ConfigClass.LogoUrl;
                        break;
                    case "##FullName##":
                        htbl[fld.FieldName] = objUser.FirstName + " " + objUser.LastName;
                        break;
                    case "##Course##":
                        htbl[fld.FieldName] = objOrder.GF_CourseInfo.COURSE_NAME;
                        break;
                    case "##OrderNo##":
                        htbl[fld.FieldName] = objOrder.ID;
                        break;
                    case "##OrderDate##":
                        htbl[fld.FieldName] = objOrder.CreatedDate;
                        break;

                    case "##GolferName##":
                        htbl[fld.FieldName] = objUser.FirstName + " " + objUser.LastName;
                        break;

                    case "##OrderType##":
                        htbl[fld.FieldName] = (objOrder.OrderType == "CO" ? "Cart Order" : "Turn Order");
                        break;

                    case "##PaymentMode##":
                        htbl[fld.FieldName] = (objOrder.PaymentType == "1" ? "Payment by Card" : "Payment by MembershipId");
                        break;
                    case "##ItemDetails##":
                        htbl[fld.FieldName] = itemDeatilsHtml;
                        break;
                    case "##ToalAmount##":
                        htbl[fld.FieldName] = "$ " + objOrder.GrandTotal;
                        break;
                    case "##Tax##":
                        htbl[fld.FieldName] = "$ " + Math.Round(objOrder.TaxAmount ?? 0, 2);
                        break;
                    case "##PromoCodeDiscount##":
                        htbl[fld.FieldName] = "$ " + 0;
                        break;
                    case "##CourseAdmin##":
                        htbl[fld.FieldName] = CourseAdminName;
                        break;
                    case "##CourseAddressPhone##":
                        htbl[fld.FieldName] = objOrder.GF_CourseInfo.PHONE;
                        break;
                    case "##CourseAddress##":
                        htbl[fld.FieldName] = objOrder.GF_CourseInfo.ADDRESS;
                        break;






                }
            }
            param.Htbl = htbl;
            string mailresult = "";
            //param.CcEmail = "amitkumar@cogniter.com";
            return SendMail(param, ref mailresult);
            //}
            //else
            //{
            //    return true;
            //}
        }
        internal static bool GolferResolutionReply(long golferUserID, string Comment, string status, string email, string name, EmailParams param, ref IQueryable<GF_EmailTemplatesFields> templateFields)
        {

            param.Subject = "Golfer" + " :Reply";
            string courseId = "";
            string SMTPHost = "";
            string SMTPPassword = "";
            string SMTPUserName = "";
            string FromEmail = "";
            string SMTPPort = "";
            string AdminEmail = "";
            string EnableSsl = "";

            if (GetSMTPSettingsForGolfer(golferUserID, ref courseId, ref SMTPHost, ref SMTPPassword, ref  SMTPUserName, ref FromEmail, ref  SMTPPort, ref AdminEmail, ref EnableSsl))
            {
                param.FromEmail = FromEmail;
                param.SmtpHost = SMTPHost;
                param.SmtpUserName = SMTPUserName;
                param.SmtpPassword = SMTPPassword;
                param.SmtpPort = Convert.ToInt32(SMTPPort);
                param.EnableSsl = Convert.ToBoolean(EnableSsl);
                param.ToEmail = email;// objUser.Email;
                var htbl = new Hashtable();

                foreach (var fld in templateFields)
                {
                    switch (fld.FieldName.ToString())
                    {
                        case "##FullName##":
                            htbl[fld.FieldName] = name;
                            break;
                        case "##Message##":
                            htbl[fld.FieldName] = Comment;
                            break;
                        case "##Replystatus##":
                            htbl[fld.FieldName] = status;
                            break;
                        case "##Footer##":
                            htbl[fld.FieldName] = getCourseFooterForEmails(Convert.ToInt64(courseId));
                            break;

                        case "##Logo##":
                            htbl[fld.FieldName] = ConfigClass.LogoUrl;
                            break;
                    }
                }
                param.Htbl = htbl;
                string mailresult = "";
                return SendMail(param, ref mailresult);
            }
            else
            {
                return false;
            }
        }


        #endregion


        public static bool GetSMTPSettings(GF_Golfer objgolfer, ref string courseId, ref string SMTPHost, ref string SMTPPassword, ref string SMTPUserName, ref string FromEmail, ref string SMTPPort, ref string AdminEmail, ref string EnableSsl)
        {
            bool result = false;
            GolflerEntities _db = new GolflerEntities();
            try
            {
                if (objgolfer != null)
                {
                    var objCourse = _db.GF_GolferUser.FirstOrDefault(x => x.GolferID == objgolfer.GF_ID);
                    if (objCourse != null)
                    {
                        var objSMTP = _db.GF_SMTPSettings.FirstOrDefault(x => x.CourseID == objCourse.CourseID);
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
                            //get from web config'
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
                        // No Confirm Course
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

        #region Feedback

        internal static bool SendResolutionMsgEMail(string sendername, string senderemail, string sendercourse, long golferUserID, string Comment, string resolutionType, string email, string name, EmailParams param, ref IQueryable<GF_EmailTemplatesFields> templateFields)
        {

            param.Subject = "Golfer" + " : Message (" + resolutionType + ") submitted";
            string courseId = "";
            string SMTPHost = "";
            string SMTPPassword = "";
            string SMTPUserName = "";
            string FromEmail = "";
            string SMTPPort = "";
            string AdminEmail = "";
            string EnableSsl = "";

            if (GetSMTPSettingsForGolfer(golferUserID, ref courseId, ref SMTPHost, ref SMTPPassword, ref  SMTPUserName, ref FromEmail, ref  SMTPPort, ref AdminEmail, ref EnableSsl))
            {
                param.FromEmail = FromEmail;
                param.SmtpHost = SMTPHost;
                param.SmtpUserName = SMTPUserName;
                param.SmtpPassword = SMTPPassword;
                param.SmtpPort = Convert.ToInt32(SMTPPort);
                param.EnableSsl = Convert.ToBoolean(EnableSsl);
                param.ToEmail = email;// objUser.Email;
                var htbl = new Hashtable();

                foreach (var fld in templateFields)
                {
                    switch (fld.FieldName.ToString())
                    {
                        case "##FullName##":
                            htbl[fld.FieldName] = name;
                            break;
                        case "##Message##":
                            htbl[fld.FieldName] = Comment;
                            break;

                        case "##Footer##":
                            htbl[fld.FieldName] = getCourseFooterForEmails(Convert.ToInt64(courseId));
                            break;
                        case "##Replystatus##":
                            htbl[fld.FieldName] = "";
                            break;
                        case "##Logo##":
                            htbl[fld.FieldName] = ConfigClass.LogoUrl;
                            break;

                        case "##SenderName##":
                            htbl[fld.FieldName] = sendername;
                            break;
                        case "##SenderEmail##":
                            htbl[fld.FieldName] = senderemail;
                            break;
                        case "##SenderCourse##":
                            htbl[fld.FieldName] = sendercourse;
                            break;
                    }
                }
                param.Htbl = htbl;
                string mailresult = "";
                return SendMail(param, ref mailresult);
            }
            else
            {
                return false;
            }
        }



        internal static bool SendAutoResponseToGolfer(long golferUserID, string Comment, string email, string name, EmailParams param, ref IQueryable<GF_EmailTemplatesFields> templateFields)
        {

            param.Subject = "Golfer" + " : Message";
            string courseId = "";
            string SMTPHost = "";
            string SMTPPassword = "";
            string SMTPUserName = "";
            string FromEmail = "";
            string SMTPPort = "";
            string AdminEmail = "";
            string EnableSsl = "";

            if (GetSMTPSettingsForGolfer(golferUserID, ref courseId, ref SMTPHost, ref SMTPPassword, ref  SMTPUserName, ref FromEmail, ref  SMTPPort, ref AdminEmail, ref EnableSsl))
            {
                param.FromEmail = FromEmail;
                param.SmtpHost = SMTPHost;
                param.SmtpUserName = SMTPUserName;
                param.SmtpPassword = SMTPPassword;
                param.SmtpPort = Convert.ToInt32(SMTPPort);
                param.EnableSsl = Convert.ToBoolean(EnableSsl);
                param.ToEmail = email;// objUser.Email;
                var htbl = new Hashtable();

                foreach (var fld in templateFields)
                {
                    switch (fld.FieldName.ToString())
                    {
                        case "##FullName##":
                            htbl[fld.FieldName] = name;
                            break;
                        case "##Message##":
                            htbl[fld.FieldName] = Comment;
                            break;
                        case "##Footer##":
                            htbl[fld.FieldName] = getCourseFooterForEmails(Convert.ToInt64(courseId));
                            break;

                        case "##Logo##":
                            htbl[fld.FieldName] = ConfigClass.LogoUrl;
                            break;
                    }
                }
                param.Htbl = htbl;
                string mailresult = "";
                return SendMail(param, ref mailresult);
            }
            else
            {
                return false;
            }
        }


        public static bool GetSMTPSettingsForGolfer(long golferUserID, ref string courseId, ref string SMTPHost, ref string SMTPPassword, ref string SMTPUserName, ref string FromEmail, ref string SMTPPort, ref string AdminEmail, ref string EnableSsl)
        {
            bool result = false;
            GolflerEntities _db = new GolflerEntities();
            try
            {
                var objCourse = _db.GF_GolferUser.FirstOrDefault(x => x.GolferID == golferUserID);
                if (objCourse != null)
                {
                    var objSMTP = _db.GF_SMTPSettings.FirstOrDefault(x => x.CourseID == objCourse.CourseID);
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


        #endregion

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
                    string address = objCourse.ADDRESS + ", " + objCourse.CITY + ", " + objCourse.STATE + ", " + objCourse.Country + ", " + objCourse.ZIPCODE;
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