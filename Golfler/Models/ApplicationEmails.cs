using System;
using System.Collections;
using System.Web;
using System.Linq;
using System.Collections.Generic;
using Golfler.Models;
using System.Configuration;

namespace Golfler.Models
{
    public class ApplicationEmails
    {

        private static bool SendMail(EmailParams emailParams, ref string mailresult)
        {

            return MailClass.SendMail(emailParams, ref mailresult);
        }

        internal static bool MassMessage(string messageTitle, string messagebody, string sendtoUser, string toemail, EmailParams param, ref IQueryable<GF_EmailTemplatesFields> templateFields, ref string mailresult)
        {
            param.Subject = Convert.ToString(System.Web.Configuration.WebConfigurationManager.AppSettings["SITE_NAME"]) + ": " + messageTitle;
            string SMTPHost = "";
            string SMTPPassword = "";
            string SMTPUserName = "";
            string FromEmail = "";
            string SMTPPort = "";
            string AdminEmail = "";
            string EnableSsl = "";

            if (GetSMTPSettingsForSuperAdmin(ref SMTPHost, ref SMTPPassword, ref  SMTPUserName, ref FromEmail, ref  SMTPPort, ref AdminEmail, ref EnableSsl, ref mailresult))
            {
                param.FromEmail = FromEmail;
                param.SmtpHost = SMTPHost;
                param.SmtpUserName = SMTPUserName;
                param.SmtpPassword = SMTPPassword;
                param.SmtpPort = Convert.ToInt32(SMTPPort);
                param.EnableSsl = Convert.ToBoolean(EnableSsl);
                param.ToEmail = toemail;
                var htbl = new Hashtable();

                foreach (var fld in templateFields)
                {
                    switch (fld.FieldName.ToString())
                    {

                        case "##name##":
                            htbl[fld.FieldName] = sendtoUser;
                            break;
                        case "##msgbody##":
                            htbl[fld.FieldName] = messagebody;
                            break;
                        case "##year##":
                            htbl[fld.FieldName] = DateTime.Now.Year;
                            break;
                        case "##sitename##":
                            htbl[fld.FieldName] = ConfigClass.SiteName;
                            break;

                        case "##sitelink##":
                            htbl[fld.FieldName] = ConfigClass.SiteUrl;
                            break;
                        case "##logo##":
                            htbl[fld.FieldName] = ConfigClass.LogoUrl;
                            break;

                    }
                }
                param.Htbl = htbl;
                return SendMail(param, ref mailresult);
            }
            else
            {
                return false;
            }
        }

        public static bool GetSMTPSettingsForSuperAdmin(ref string SMTPHost, ref string SMTPPassword, ref string SMTPUserName, ref string FromEmail, ref string SMTPPort, ref string AdminEmail, ref string EnableSsl, ref string mailresult)
        {
            bool result = false;
            GolflerEntities objnewdb = new GolflerEntities();
            try
            {
                var objSMTP = objnewdb.GF_SMTPSettings.FirstOrDefault(x => x.CourseID == 0);
                if (objSMTP != null)
                {
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
                    mailresult = "SMTP not available.";
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
                mailresult = "Exception during SMTP information fetch from database.";
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


        public static bool GetSMTPSettings(GF_AdminUsers objUser, ref string courseId, ref string SMTPHost, ref string SMTPPassword, ref string SMTPUserName, ref string FromEmail, ref string SMTPPort, ref string AdminEmail, ref string EnableSsl)
        {
            bool result = false;
            GolflerEntities _db = new GolflerEntities();
            try
            {
                if (objUser != null)
                {
                    var objCourse = _db.GF_CourseUsers.FirstOrDefault(x => x.UserID == objUser.ID);
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

        
        public static bool GetSMTPSettingsForGolfer(long golferUserID, ref string courseId, ref string SMTPHost, ref string SMTPPassword, ref string SMTPUserName, ref string FromEmail, ref string SMTPPort, ref string AdminEmail, ref string EnableSsl)
        {
            bool result = false;
            GolflerEntities _db = new GolflerEntities();
            try
            {
                var objCourse = _db.GF_GolferUser.FirstOrDefault(x => x.GolferID == golferUserID);
                if (objCourse != null)
                {
                    var objSMTP = _db.GF_SMTPSettings.FirstOrDefault(x => x.CourseID == (objCourse.CourseID ?? 0));
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

        /// <summary>
        /// Created By: Ramesh Kalra
        /// Created Date: 19 May 2015
        /// Purpose: Send Mass message
        /// </summary>
        /// <param name="Db"></param>
        /// <param name="objMailDetails"></param>
        /// <param name="EmailBy"></param>
        /// <param name="param"></param>
        /// <param name="templateFields"></param>
        /// <param name="mailresult"></param>
        /// <returns></returns>
        internal static bool SendMassMessages(Int64 courseid, ref GolflerEntities Db, GF_MassMessages objMailDetails, string EmailBy, EmailParams param, ref IQueryable<GF_EmailTemplatesFields> templateFields, ref string mailresult)
        {
            param.Subject = param.CourseName + ": " + objMailDetails.MessageTitle;
            param.ToEmail = objMailDetails.SendToUserEmail;
            param.FromEmail = EmailBy;

            var htbl = new Hashtable();

            foreach (var fld in templateFields)
            {
                switch (fld.FieldName.ToString())
                {
                    case "##name##":
                        htbl[fld.FieldName] = objMailDetails.SendToUserName;
                        break;
                    case "##msgbody##":
                        htbl[fld.FieldName] = objMailDetails.Message;
                        break;
                    case "##year##":
                        htbl[fld.FieldName] = DateTime.Now.Year;
                        break;
                    case "##sitename##":
                        htbl[fld.FieldName] = ConfigClass.SiteName;
                        break;
                    case "##FullName##":
                        htbl[fld.FieldName] = objMailDetails.SendToUserName;
                        break;
                    case "##sitelink##":
                        htbl[fld.FieldName] = ConfigClass.SiteUrl;
                        break;
                    case "##Logo##":
                        htbl[fld.FieldName] = param.Logo;
                        break;
                    case "##Footer##":
                        htbl[fld.FieldName] = getCourseFooterForEmails(courseid);
                        break;
                }
            }
            param.Htbl = htbl;
            return MailClass.SendMail(param, ref mailresult); ;
        }

        /// <summary>
        /// Created By: Ramesh Kalra
        /// Created On: 7 May 2015
        /// Purpose: Email send to Super Admin when Golfer will send Co-ordinate status for any course
        /// </summary>
        /// <param name="Db"></param>
        /// <param name="objCourseDetails"></param>
        /// <param name="param"></param>
        /// <param name="templateFields"></param>
        /// <param name="mailresult"></param>
        /// <returns></returns>
        internal static bool SuggestedCoordinateSend(ref GolflerEntities Db, GF_CourseBuilder objCourseDetails, EmailParams param, ref IQueryable<GF_EmailTemplatesFields> templateFields, ref string mailresult)
        {
            string username = Convert.ToString(Db.GF_AdminUsers.Where(x => x.ID == objCourseDetails.CreatedBy).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault());
            param.Subject = param.CourseName + ": Suggested Co-ordinates Submitted successfully for " + objCourseDetails.GF_CourseInfo.COURSE_NAME;
            param.ToEmail = CommonFunctions.GetSuperAdminEmail();

            var htbl = new Hashtable();
            foreach (var fld in templateFields)
            {
                switch (fld.FieldName.ToString())
                {
                    case "##logo##":
                        htbl[fld.FieldName] = param.Logo;
                        break;
                    case "##name##":
                        htbl[fld.FieldName] = username;
                        break;
                    case "##msgbody##":
                        htbl[fld.FieldName] = objCourseDetails.Comments;
                        break;
                    case "##sitename##":
                        htbl[fld.FieldName] = param.SiteName;
                        break;
                    case "##year##":
                        htbl[fld.FieldName] = CommonFunctions.GetCurrentYear();
                        break;
                }
            }
            param.Htbl = htbl;
            return MailClass.SendMail(param, ref mailresult); ;
        }

        /// <summary>  /// <summary>
        /// Created By: Ramesh Kalra
        /// Created On: 7 May 2015
        /// Purpose: Send status to Golfer when Admin Approve/Reject Golfer's suggestion
        /// </summary>
        /// <param name="Db"></param>
        /// <param name="objCourseDetails"></param>
        /// <param name="param"></param>
        /// <param name="templateFields"></param>
        /// <param name="mailresult"></param>
        /// <returns></returns>
        internal static bool SuggestedCoordinateStatus(ref GolflerEntities Db, GF_CourseBuilder objCourseDetails, string StatusForMail, EmailParams param, ref IQueryable<GF_EmailTemplatesFields> templateFields, ref string mailresult)
        {
            string username = Convert.ToString(Db.GF_AdminUsers.Where(x => x.ID == objCourseDetails.CreatedBy).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault());
            param.Subject = param.CourseName + ": Suggested Co-ordinates Status for " + objCourseDetails.GF_CourseInfo.COURSE_NAME;
            param.ToEmail = Convert.ToString(Db.GF_AdminUsers.Where(x => x.ID == objCourseDetails.CreatedBy).Select(x => x.Email).FirstOrDefault());

            var htbl = new Hashtable();
            foreach (var fld in templateFields)
            {
                switch (fld.FieldName.ToString())
                {
                    case "##logo##":
                        htbl[fld.FieldName] = param.Logo;
                        break;
                    case "##name##":
                        htbl[fld.FieldName] = username;
                        break;
                    case "##msgbody##":
                        htbl[fld.FieldName] = "Status: " + StatusForMail;
                        break;
                    case "##sitename##":
                        htbl[fld.FieldName] = param.SiteName;
                        break;
                    case "##year##":
                        htbl[fld.FieldName] = CommonFunctions.GetCurrentYear();
                        break;
                }
            }
            param.Htbl = htbl;
            return MailClass.SendMail(param, ref mailresult); ;
        }

        internal static bool CoordinateReminder(ref GolflerEntities Db, Int64 courseid, string forDate, EmailParams param, ref IQueryable<GF_EmailTemplatesFields> templateFields, ref string mailresult)
        {
            string toemail = Convert.ToString(Db.GF_AdminUsers.Where(x => x.CourseId == courseid && x.Type == "CA").Select(x => x.Email).FirstOrDefault());
            string username = Convert.ToString(Db.GF_AdminUsers.Where(x => x.CourseId == courseid && x.Type == "CA").Select(x => x.FirstName + " " + x.LastName).FirstOrDefault());

            param.Subject = param.CourseName + ": Golf course co-ordinates reminder";
            param.ToEmail = toemail;

            var htbl = new Hashtable();
            foreach (var fld in templateFields)
            {
                switch (fld.FieldName.ToString())
                {
                    case "##Logo##":
                        htbl[fld.FieldName] = param.Logo;
                        break;
                    case "##FullName##":
                        htbl[fld.FieldName] = username;
                        break;
                    case "##msgDate##":
                        htbl[fld.FieldName] = forDate.Replace("12:00:00 AM", "").Trim();
                        break;
                    case "##sitename##":
                        htbl[fld.FieldName] = param.SiteName;
                        break;
                    case "##year##":
                        htbl[fld.FieldName] = CommonFunctions.GetCurrentYear();
                        break;
                    case "##Footer##":
                        htbl[fld.FieldName] = getCourseFooterForEmails(courseid);
                        break;
                }
            }
            param.Htbl = htbl;
            return MailClass.SendMail(param, ref mailresult); ;
        }


        /// Created By: Ramesh Kalra
        /// Created On: 9 May 2015
        /// Purpose: Send mail to Course Admin from super admin that he is assigned as a course admin for new course
        /// </summary>
        /// <param name="Db"></param>
        /// <param name="objCourseDetails"></param>
        /// <param name="param"></param>
        /// <param name="templateFields"></param>
        /// <param name="mailresult"></param>
        /// <returns></returns>
        internal static bool AssignCourseAdmin(ref GolflerEntities Db, GF_CourseInfo objCourseDetails, EmailParams param, ref IQueryable<GF_EmailTemplatesFields> templateFields, ref string mailresult)
        {
            var userdetails = Db.GF_AdminUsers.Where(x => x.ID == objCourseDetails.UserID).FirstOrDefault();
            param.Subject = param.CourseName + ": You have assigned as Course Admin.";
            param.ToEmail = Convert.ToString(Db.GF_AdminUsers.Where(x => x.ID == objCourseDetails.UserID).Select(x => x.Email).FirstOrDefault());

            var htbl = new Hashtable();
            foreach (var fld in templateFields)
            {
                switch (fld.FieldName.ToString())
                {
                    case "##logo##":
                        htbl[fld.FieldName] = param.Logo;
                        break;
                    case "##name##":
                        htbl[fld.FieldName] = Convert.ToString(userdetails.FirstName + " " + userdetails.LastName);
                        break;
                    case "##sitename##":
                        htbl[fld.FieldName] = param.SiteName;
                        break;
                    case "##year##":
                        htbl[fld.FieldName] = CommonFunctions.GetCurrentYear();
                        break;
                    case "##coursename##":
                        htbl[fld.FieldName] = objCourseDetails.COURSE_NAME;
                        break;
                    case "##loginurl##":
                        string siteurl = ConfigClass.SiteUrl;
                        if (!(siteurl.EndsWith("/")))
                        {
                            siteurl = siteurl + "/";
                        }

                        htbl[fld.FieldName] = siteurl + "Golfler/courseadmin";
                        break;
                    case "##username##":
                        htbl[fld.FieldName] = Convert.ToString(userdetails.UserName);
                        break;
                    case "##password##":
                        htbl[fld.FieldName] = CommonFunctions.DecryptPassword(userdetails.Password, userdetails.SALT);
                        break;
                }
            }
            param.Htbl = htbl;
            return MailClass.SendMail(param, ref mailresult); ;
        }


        /// <summary>
        /// Created By: Veera Verma
        /// Created On: 8 May 2015
        /// Purpose: Email send to golfer when his order is refunded
        /// </summary>
        /// <param name="Db"></param>
        /// <param name="objCourseDetails"></param>
        /// <param name="param"></param>
        /// <param name="templateFields"></param>
        /// <param name="mailresult"></param>
        /// <returns></returns>
        internal static bool OrderRefund(ref GolflerEntities Db, EmailParams param, GF_OrderRefund objRefund, string ccmail, ref IQueryable<GF_EmailTemplatesFields> templateFields, ref string mailresult)
        {
            param.Subject = "Your order has been Refunded : ";
            param.ToEmail = Db.GF_Golfer.FirstOrDefault(x => x.GF_ID == objRefund.GF_Order.GolferID).Email;
            param.CcEmail = ccmail;
            long? _refundedBy = objRefund.CreatedBy;
            string name = Db.GF_Golfer.FirstOrDefault(x => x.GF_ID == objRefund.GF_Order.GolferID).FirstName;
            string refundedBy = "";
            if (_refundedBy > 0)
            {
                refundedBy = Db.GF_AdminUsers.FirstOrDefault(x => x.ID == _refundedBy).FirstName;
            }
            var htbl = new Hashtable();
            //
            GF_Order objOrder = Db.GF_Order.FirstOrDefault(x => x.ID == objRefund.OrderId);
            string itemDeatilsHtml = "";
            try
            {

                //GF_Order objOrderInner = Db.GF_Order.FirstOrDefault(x => x.ID == objRefund.OrderId);
                //foreach (var item in objOrderInner)
                //{
                //    string itemName =  Db.GF_MenuItems.FirstOrDefault(x => x.ID == item.ID).Name;
                //    itemDeatilsHtml = itemDeatilsHtml + "<tr><td style='border:1px solid #ccc; border-top:none; padding:5px;'>" + itemName + "</td>";
                //    itemDeatilsHtml = itemDeatilsHtml + "<td style='border-right:1px solid #ccc; border-bottom:1px solid #ccc; padding:5px; text-align:right;'>" + item.q + "</td>";
                //    itemDeatilsHtml = itemDeatilsHtml + "<td style='border-right:1px solid #ccc; border-bottom:1px solid #ccc; padding:5px; text-align:right;'>$" + item.UnitPrice + "</td>";
                //    itemDeatilsHtml = itemDeatilsHtml + "<td style='border-right:1px solid #ccc; border-bottom:1px solid #ccc; padding:5px; text-align:right;'>$" + (item.UnitPrice * item.Quantity) + "</td>";
                //    itemDeatilsHtml = itemDeatilsHtml + "</tr>";


                //}

            }
            catch { }
         

           
            foreach (var fld in templateFields)
            {
                switch (fld.FieldName.ToString())
                {
                    case "##Logo##":
                        htbl[fld.FieldName] = ConfigClass.LogoUrl;
                        break;
                    case "##FullName##":
                        htbl[fld.FieldName] = objOrder.GF_Golfer.FirstName + " " + objOrder.GF_Golfer.LastName;
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
                        htbl[fld.FieldName] = objOrder.GF_Golfer.FirstName + " " + objOrder.GF_Golfer.LastName;
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
                        htbl[fld.FieldName] = "$ " + objOrder.TaxAmount;
                        break;
                    case "##PromoCodeDiscount##":
                        htbl[fld.FieldName] = "$ " + 0;
                        break;
                    case "##Footer##":
                        htbl[fld.FieldName] = "";
                        break;
                    case "##Refundamount##":
                        htbl[fld.FieldName] = "$ " + objRefund.RefundAmount;
                        break;
                    case "##Refunddesc##":
                        htbl[fld.FieldName] = objRefund.Description;
                        break;
                    case "##Refundedby##":
                        htbl[fld.FieldName] = refundedBy;
                        break;
                    case "##Refunddate##":
                        htbl[fld.FieldName] = objRefund.CreatedDate;
                        break;
                    case "##Refundtype##":
                        htbl[fld.FieldName] = objRefund.RefundType == "P" ? "Partial" : "Full";
                        break;
                  
                }
            }
            param.Htbl = htbl;
            return MailClass.SendMail(param, ref mailresult); ;
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
                    //FooterHtml = "<td style='font-size:11pt; color:#000; font-family:calibri; padding: 10px; line-height: 20px; text-align: justify;'>";
                    //FooterHtml = FooterHtml + "Best,<br><br>";
                    //FooterHtml = FooterHtml + "<strong>" + ConfigurationManager.AppSettings["EmailAdminName"] + "</strong><br>";
                    //FooterHtml = FooterHtml + ConfigurationManager.AppSettings["EmailAdminType"];
                    //FooterHtml = FooterHtml + "<br>";
                    //FooterHtml = FooterHtml + "<strong style='color:#43b34a'>" + ConfigurationManager.AppSettings["EmailAdminAddress"] + "</strong>";
                    //FooterHtml = FooterHtml + "<br>";
                    //FooterHtml = FooterHtml + ConfigurationManager.AppSettings["EmailAdminPhone"];
                    //FooterHtml = FooterHtml + "<br>";
                    //FooterHtml = FooterHtml + "</td>";

                    FooterHtml = "<td style='font-size:11pt; color:#000; font-family:calibri; padding: 10px; line-height: 20px; text-align: justify;'>";
                    FooterHtml = FooterHtml + "Best,<br><br>";
                    FooterHtml = FooterHtml + "<strong>" + LoginInfo.Name + "</strong><br>";
                    FooterHtml = FooterHtml + UserType.GetFullUserType(LoginInfo.LoginUserType);
                    FooterHtml = FooterHtml + "<br>";
                    FooterHtml = FooterHtml + "<strong style='color:#43b34a'>" + LoginInfo.CourseEmail + "</strong>";
                    FooterHtml = FooterHtml + "<br>";
                    FooterHtml = FooterHtml + "</td>";
                }
            }
            catch
            {
                //FooterHtml = "<td style='font-size:11pt; color:#000; font-family:calibri; padding: 10px; line-height: 20px; text-align: justify;'>";
                //FooterHtml = FooterHtml + "Best,<br><br>";
                //FooterHtml = FooterHtml + "<strong>" + ConfigurationManager.AppSettings["EmailAdminName"] + "</strong><br>";
                //FooterHtml = FooterHtml + ConfigurationManager.AppSettings["EmailAdminType"];
                //FooterHtml = FooterHtml + "<br>";
                //FooterHtml = FooterHtml + "<strong style='color:#43b34a'>" + ConfigurationManager.AppSettings["EmailAdminAddress"] + "</strong>";
                //FooterHtml = FooterHtml + "<br>";
                //FooterHtml = FooterHtml + ConfigurationManager.AppSettings["EmailAdminPhone"];
                //FooterHtml = FooterHtml + "<br>";
                //FooterHtml = FooterHtml + "</td>";

                FooterHtml = "<td style='font-size:11pt; color:#000; font-family:calibri; padding: 10px; line-height: 20px; text-align: justify;'>";
                FooterHtml = FooterHtml + "Best,<br><br>";
                FooterHtml = FooterHtml + "<strong>" + LoginInfo.Name + "</strong><br>";
                FooterHtml = FooterHtml + UserType.GetFullUserType(LoginInfo.LoginUserType);
                FooterHtml = FooterHtml + "<br>";
                FooterHtml = FooterHtml + "<strong style='color:#43b34a'>" + LoginInfo.CourseEmail + "</strong>";
                FooterHtml = FooterHtml + "<br>";
                FooterHtml = FooterHtml + "</td>";
            }
            return FooterHtml;
        }

        internal static bool EndUserRegistrationMail(GF_Golfer objUser, EmailParams param, ref IQueryable<GF_EmailTemplatesFields> templateFields)
        {
            param.Subject = "Golfer" + ": Your UserName has been changed";
          
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
            
        }

        internal static bool ResolutionReply(ref GolflerEntities Db, Int64 courseid,string message,string status,string toemail,string username, EmailParams param, ref IQueryable<GF_EmailTemplatesFields> templateFields, ref string mailresult)
        {
            param.Subject = param.CourseName + ": Resolution Center Reply";
            param.ToEmail = toemail;

            var htbl = new Hashtable();
            foreach (var fld in templateFields)
            {
                switch (fld.FieldName.ToString())
                {
                    case "##Logo##":
                        htbl[fld.FieldName] = param.Logo;
                        break;
                    case "##FullName##":
                        htbl[fld.FieldName] = username;
                        break;
                    case "##Message##":
                        htbl[fld.FieldName] = message;
                        break;
                    case "##Replystatus##":
                        htbl[fld.FieldName] = status;
                        break;                    
                    case "##Footer##":
                        htbl[fld.FieldName] = getCourseFooterForEmails(courseid);
                        break;
                }
            }
            param.Htbl = htbl;
            return MailClass.SendMail(param, ref mailresult); ;
        }

        internal static bool PromoCodeIssue(ref GolflerEntities Db, Int64 courseid, string orderNo, string promoCode, string toemail, string name, EmailParams param, ref IQueryable<GF_EmailTemplatesFields> templateFields, ref string mailresult)
        {
            param.Subject = param.CourseName + ": Resolution Center Reply";
            param.ToEmail = toemail;

            var htbl = new Hashtable();
            foreach (var fld in templateFields)
            {
                switch (fld.FieldName.ToString())
                {
                    case "##Logo##":
                        htbl[fld.FieldName] = param.Logo;
                        break;
                    case "##FullName##":
                        htbl[fld.FieldName] = name;
                        break;
                    case "##OrderNo##":
                        htbl[fld.FieldName] = orderNo;
                        break;
                    case "##PromoCode##":
                        htbl[fld.FieldName] = promoCode;
                        break;
                    case "##Footer##":
                        htbl[fld.FieldName] = getCourseFooterForEmails(courseid);
                        break;
                }
            }
            param.Htbl = htbl;
            return MailClass.SendMail(param, ref mailresult); ;
        }

        internal static bool AutoResponseToGolfer(ref GolflerEntities Db, Int64 courseid, string message, string status, string toemail, string username, EmailParams param, ref IQueryable<GF_EmailTemplatesFields> templateFields, ref string mailresult)
        {
            param.Subject = param.CourseName + ": Auto Response";
            param.ToEmail = toemail;

            var htbl = new Hashtable();
            foreach (var fld in templateFields)
            {
                switch (fld.FieldName.ToString())
                {
                    case "##Logo##":
                        htbl[fld.FieldName] = param.Logo;
                        break;
                    case "##FullName##":
                        htbl[fld.FieldName] = username;
                        break;
                    case "##Message##":
                        htbl[fld.FieldName] = message;
                        break;
                    //case "##Replystatus##":
                    //    htbl[fld.FieldName] = status;
                    //    break;
                    case "##Footer##":
                        htbl[fld.FieldName] = getCourseFooterForEmails(courseid);
                        break;
                }
            }
            param.Htbl = htbl;
            return MailClass.SendMail(param, ref mailresult); ;
        }


        internal static bool GolferResolutionReply(long golferUserID, string Comment, string status, string email, string name, EmailParams param, ref IQueryable<GF_EmailTemplatesFields> templateFields)
        {

            param.Subject =Convert.ToString(System.Web.Configuration.WebConfigurationManager.AppSettings["SITE_NAME"]) + ": Resolution center reply";
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


        internal static bool ResolutionCenterReply(ref GolflerEntities Db, GF_ResolutionCenter objCourseDetails, EmailParams param, ref IQueryable<GF_EmailTemplatesFields> templateFields, ref string mailresult)
        {
            string username = Convert.ToString(Db.GF_AdminUsers.Where(x => x.ID == objCourseDetails.CreatedBy).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault());
            param.Subject = param.CourseName + ": Suggested Co-ordinates Submitted successfully for " + objCourseDetails.GF_CourseInfo.COURSE_NAME;
            param.ToEmail = CommonFunctions.GetSuperAdminEmail();

            var htbl = new Hashtable();
            foreach (var fld in templateFields)
            {
                switch (fld.FieldName.ToString())
                {
                    case "##logo##":
                        htbl[fld.FieldName] = param.Logo;
                        break;
                    case "##name##":
                        htbl[fld.FieldName] = username;
                        break;
                    case "##msgbody##":
                      //  htbl[fld.FieldName] = objCourseDetails.Comments;
                        break;
                    case "##sitename##":
                        htbl[fld.FieldName] = param.SiteName;
                        break;
                    case "##year##":
                        htbl[fld.FieldName] = CommonFunctions.GetCurrentYear();
                        break;
                }
            }
            param.Htbl = htbl;
            return MailClass.SendMail(param, ref mailresult); ;
        }

        #region Feedback

        internal static bool SendResolutionMsgEMail(long golferUserID, string Comment, string email, string name, EmailParams param, ref IQueryable<GF_EmailTemplatesFields> templateFields)
        {

            param.Subject = "Golfer" + " : Reply";
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

        #endregion

        internal static bool AdminUserRegistrationMail(ref GolflerEntities Db, GF_AdminUsers objUser, EmailParams param, ref IQueryable<GF_EmailTemplatesFields> templateFields)
        {
            param.Subject = (LoginInfo.LoginUserType == UserType.CourseAdmin || LoginInfo.LoginUserType == UserType.Proshop) ? 
                "Course: New User Registration" : "Admin: New User Registration";

            string courseName = "";
            var course = Db.GF_CourseInfo.FirstOrDefault(x => x.ID == (objUser.CourseId ?? 0));
            courseName = course != null ? course.COURSE_NAME : "Golfler";

            if (templateFields != null)
            {
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
                        case "##UserName##":
                            htbl[fld.FieldName] = objUser.Email;
                            break;
                        case "##Password##":
                            htbl[fld.FieldName] = CommonFunctions.DecryptString(objUser.Password, objUser.SALT);
                            break;
                        case "##Footer##":
                            htbl[fld.FieldName] = getCourseFooterForEmails(objUser.CourseId ?? 0);
                            break;
                        case "##CourseName##":
                            htbl[fld.FieldName] = courseName;
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
    }
}