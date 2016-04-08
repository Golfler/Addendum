using System;
using System.Linq;
using System.Web;
using System.Collections;
using System.Net;
using System.Net.Mail;
using System.IO;
using System.Text.RegularExpressions;
using Golfler;
using Golfler.Models;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

namespace Golfler.Models
{



    public class EmailParams
    {
        private GolflerEntities _db = null;
        public EmailParams()
        {
            _db = new GolflerEntities();
        }
        public Hashtable Htbl { get; set; }
        public string ToEmail { get; set; }
        public string CcEmail { get; set; }
        public string BccEmail { get; set; }
        public string Subject { get; set; }
        private string _templatePath;
        public string EmailTemplatePath
        {
            get
            {
                return _templatePath ?? Params.EmailTemplatePath;
            }
            set { _templatePath = value; }
        }

        public string EmailTemplateName { get; set; }
        private string _attch;
        public string Attachments
        {
            get { return _attch ?? ""; }
            set { _attch = value; }
        }
        private string _fromEmail;
        public string FromEmail
        {
            get
            {
                return _fromEmail ?? Params.EmailFrom;
            }
            set { _fromEmail = value; }
        }

        public string SmtpHost { get; set; }
        public string SmtpUserName { get; set; }
        public string SmtpPassword { get; set; }
        public int SmtpPort { get; set; }

        public string EmailBody { get; set; }

        private bool _enableSsl;
        public bool EnableSsl
        {
            get
            {
                if (_enableSsl == null)
                    _enableSsl = Params.EnableSsl;
                return _enableSsl;
            }
            set { _enableSsl = value; }
        }

        public string ReplyEmail { get; set; }
        public string Message { get; private set; }

        public object CourseName { get; set; }
        public object SiteName { get; set; }
        public string CourseLink { get; set; }

        public string CourseLoginUrl { get; set; }

        public string LogoUrl { get; set; }
        public string Logo { get; set; }

        /// <summary>
        /// Created By: Ramesh Kalra
        /// Created On: 7 May 2015
        /// Purpose: Get Parameters and Template Fields for Course. 
        /// </summary>
        /// <param name="Db">DB Reference </param>
        /// <param name="TemplateName">Template Name for which mail need to be sent. E.g. Forgot Password, Registration etc. Value of this field should be match with Database column name. TemplateName in GF_CourseEmailTemplates table </param>
        /// <param name="templateFields">This parameter is reference type which contains Email template fields.</param>
        /// <param name="courseId">Course Id -- ID in case of Couse, 0 in case of Super Admin or Golfer</param>
        /// <param name="logintype">Login Type would be "SA" for Super Admin, "CA" for Course Admin and "G" for Golfer. These values should be call from "LoginType" class in TYPES.CS</param>
        /// <param name="emailResult">This parameter would contains Result of any exception.</param>
        /// <returns>Return type is "EmailParams" class</returns>
        public static EmailParams GetEmailParamsNew(ref GolflerEntities Db, string TemplateName, ref IQueryable<GF_EmailTemplatesFields> templateFields,
            long courseId, string logintype, ref string mailresult)
        {
            EmailParams param = new EmailParams();
            try
            {
                var courseDetails = Db.GF_CourseInfo.FirstOrDefault(x => x.ID == courseId);
                if (courseDetails != null) // Course Details
                {
                    param.SiteName = ConfigClass.SiteName;
                    param.CourseName = courseDetails.COURSE_NAME;
                    param.CourseLink = ConfigClass.SiteUrl;
                    string siteurl = ConfigClass.SiteUrl;
                    if (!(siteurl.EndsWith("/")))
                    {
                        siteurl = siteurl + "/";
                    }
                    param.CourseLoginUrl = siteurl + "Golfler/courseadmin";
                    param.Logo = ConfigClass.LogoUrl;
                }
                else
                {
                    if (courseId == 0)  // Super Admin OR Golfer
                    {
                        param.SiteName = ConfigClass.SiteName;
                        param.CourseName = ConfigClass.SiteName;
                        param.CourseLink = ConfigClass.SiteUrl;
                        param.Logo = ConfigClass.LogoUrl;

                        string siteurl = ConfigClass.SiteUrl;
                        if (!(siteurl.EndsWith("/")))
                        {
                            siteurl = siteurl + "/";
                        }

                        if (logintype == LoginType.SuperAdmin)  // Super Admin
                        {
                            param.CourseLoginUrl = siteurl + "Golfler";
                        }
                        else // Golfer
                        {
                            param.CourseLoginUrl = siteurl + "Golfler/golfer";
                        }
                    }
                    else // Course Does not exists
                    {
                        mailresult = "Course does not exists.";
                    }
                }

                if (mailresult == "")
                {
                    var smtpSettings = Db.GF_SMTPSettings.FirstOrDefault(x => x.CourseID == courseId);


                    if (smtpSettings != null)
                    {
                        param.FromEmail = smtpSettings.FromEmail;
                        param.ReplyEmail = smtpSettings.ReplyEmail;
                        param.SmtpHost = smtpSettings.SMTPHost;
                        param.SmtpPassword = smtpSettings.SMTPPassword;
                        param.SmtpUserName = smtpSettings.SMTPUserName;

                        bool enableSSL = false;
                        if (Convert.ToBoolean(smtpSettings.EnableSsl) || Convert.ToBoolean(smtpSettings.EnableTls))
                            enableSSL = true;

                        param.EnableSsl = enableSSL;

                        if (!string.IsNullOrEmpty(smtpSettings.SMTPPort))
                            param.SmtpPort = Convert.ToInt32(smtpSettings.SMTPPort);

                        if (string.IsNullOrEmpty(param.SmtpHost) ||
                          string.IsNullOrEmpty(param.SmtpUserName) || string.IsNullOrEmpty(param.SmtpPassword) ||
                          string.IsNullOrEmpty(param.FromEmail))
                        {
                            mailresult = "SMTP information is not correct.";
                        }
                        else
                        {
                            long templateId = 0;

                            var courseTemp = Db.GF_CourseEmailTemplates.FirstOrDefault(x => x.CourseID == courseId && x.TemplateName == TemplateName);
                            if (courseTemp != null)
                            {
                                param.EmailBody = courseTemp.MessageBody;
                                param.EmailTemplateName = courseTemp.TemplateName;
                                templateId = Convert.ToInt64(courseTemp.EmailTemplateId);
                            }
                            else
                            {
                                var maintemplate = Db.GF_EmailTemplates.FirstOrDefault(x => x.TemplateName == TemplateName);
                                param.EmailBody = maintemplate.MessageBody;
                                param.EmailTemplateName = maintemplate.TemplateName;
                                templateId = Convert.ToInt64(maintemplate.ID);
                            }

                            templateFields = Db.GF_EmailTemplatesFields.Where(x => x.EmailTemplateId == templateId);
                        }
                    }
                    else
                    {
                        mailresult = "SMTP information not available.";
                    }
                }
            }
            catch (Exception ex)
            {
                mailresult = ex.Message;
            }
            return param;

        }

        public static EmailParams GetEmailParams(ref GolflerEntities Db, string TemplateName, ref IQueryable<GF_EmailTemplatesFields> templateFields)
        {
            EmailParams param = new EmailParams();
            var orgTemp = Db.GF_EmailTemplates.FirstOrDefault(x => x.TemplateName == TemplateName);
            var orgtemplate = Db.GF_EmailTemplates.FirstOrDefault(x => x.TemplateName == TemplateName);
            param.EmailBody = orgtemplate.MessageBody;
            param.EmailTemplateName = orgtemplate.TemplateName;
            var templateId = Convert.ToInt64(orgtemplate.ID);
            templateFields = Db.GF_EmailTemplatesFields.Where(x => x.EmailTemplateId == templateId);
            return param;
        }
    }




    public class MailClass
    {
        #region Private Methods
        // Methods
        private static MailMessage AddAttachments(string attachments, MailMessage message)
        {
            if ((attachments.IndexOf(",") != -1) || (attachments.IndexOf(";") != -1))
            {
                char ch = ',';
                if (attachments.IndexOf(",") != -1)
                {
                    ch = ',';
                }
                else
                {
                    ch = ';';
                }
                string[] strArray = attachments.Split(new char[] { ch });
                for (int i = 0; i < strArray.Length; i++)
                {
                    if (!File.Exists(Convert.ToString(strArray[i]).Trim()))
                    {
                        throw new IOException("Attachment File not found. Path: " + strArray[i]);
                    }
                    message.Attachments.Add(new Attachment(Convert.ToString(strArray[i]).Trim()));
                }
                return message;
            }
            if (attachments.Length > 0)
            {
                if (!File.Exists(attachments.Trim()))
                {
                    throw new IOException("Attachment File not found. Path: " + attachments);
                }
                message.Attachments.Add(new Attachment(attachments.Trim()));
            }
            return message;
        }

        private static MailMessage addEmailAddress(MailMessage message, string emailIDs, string code)
        {
            if ((emailIDs.IndexOf(",") != -1) || (emailIDs.IndexOf(";") != -1))
            {
                char ch = ',';
                if (emailIDs.IndexOf(",") != -1)
                {
                    ch = ',';
                }
                else
                {
                    ch = ';';
                }
                string[] strArray = emailIDs.Split(new char[] { ch });
                for (int i = 0; i < strArray.Length; i++)
                {
                    if (!IsValidEmailID(Convert.ToString(strArray[i]).Trim()))
                    {
                        throw new FormatException("Invalid email ID found in address list");
                    }
                    string str = code;
                    if (str == null)
                    {
                        goto Label_00F1;
                    }
                    if (!(str == "cc"))
                    {
                        if (str == "bcc")
                        {
                            goto Label_00CF;
                        }
                        goto Label_00F1;
                    }
                    message.CC.Add(new MailAddress(Convert.ToString(strArray[i]).Trim()));
                    continue;
                Label_00CF:
                    message.Bcc.Add(new MailAddress(Convert.ToString(strArray[i]).Trim()));
                    continue;
                Label_00F1:
                    message.To.Add(new MailAddress(Convert.ToString(strArray[i]).Trim()));
                }
                return message;
            }
            if (!IsValidEmailID(emailIDs))
            {
                throw new FormatException("Invalid email ID found in " + code);
            }
            switch (code)
            {
                case "cc":
                    message.CC.Add(new MailAddress(emailIDs));
                    return message;

                case "bcc":
                    message.Bcc.Add(new MailAddress(emailIDs));
                    return message;

                case "reply":
                    message.ReplyToList.Add(new MailAddress(emailIDs));
                    return message;
            }
            message.To.Add(new MailAddress(emailIDs));
            return message;
        }

        private static bool IsValidEmailID(string emailAddress)
        {
            return Regex.IsMatch(emailAddress, @"^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$");
        }

        private static string ReadTemplateFile(string filePath, string fileName)
        {
            string str = string.Empty;
            if (filePath == null)
            {
                filePath = string.Empty;
            }
            if (fileName == null)
            {
                fileName = string.Empty;
            }
            filePath = filePath.Trim();
            fileName = fileName.Trim();
            filePath = filePath.Replace(@"\", "/");
            fileName = fileName.Replace(@"\", "/");
            if ((filePath != string.Empty) && (filePath.IndexOf("/") != 0))
            {
                filePath = "/" + filePath;
            }
            if ((fileName != string.Empty) && (fileName.IndexOf("/") != 0))
            {
                fileName = "/" + fileName;
            }
            string path = HttpContext.Current.Server.MapPath(HttpContext.Current.Request.ApplicationPath + filePath + fileName);
            if (File.Exists(path))
            {
                try
                {
                    StreamReader reader = File.OpenText(path);
                    str = reader.ReadToEnd();
                    reader.Close();
                    return str;
                }
                catch (Exception)
                {
                    throw new IOException("Error!! File Not found in the specified location.<br />Path: " + path + "<br />");
                }
            }
            throw new IOException("Error!! File Not found in the specified location.<br />Path: " + path + "<br />");
        }

        private static string ReplaceFileVariables(Hashtable hashVars, string content)
        {
            IDictionaryEnumerator enumerator = hashVars.GetEnumerator();
            while (enumerator.MoveNext())
            {
                content = content.Replace(Convert.ToString(enumerator.Key), Convert.ToString(enumerator.Value));
            }
            return content;
        }

        private static bool SendMail(MailMessage mess, string smtpHost, string smtpUserName, string smtpPassword, int smtpPort, bool EnableSsl, ref string mailresult)
        {
            bool result = false;
            try
            {
                SmtpClient client = (smtpPort > 0) ? new SmtpClient(smtpHost, smtpPort) : new SmtpClient(smtpHost);
                if (smtpUserName != string.Empty)
                {

                    NetworkCredential credential = new NetworkCredential(smtpUserName, smtpPassword);
                    //  client.EnableSsl = EnableSsl;
                    // client.UseDefaultCredentials = false;
                    client.Credentials = credential;
                    //  client.DeliveryMethod = SmtpDeliveryMethod.Network;
                    // client.Timeout = Params.TimeOut;

                    if (EnableSsl)
                    {
                        client.EnableSsl = true;
                    }
                }

                ///Date: 28 July 2015
                ///Author: Amit 
                ///Error: The remote certificate is invalid according to the validation procedure.
                ///Solution: As a workaround, you can switch off certificate validation. Put this code somewhere before smtpclient.Send():
                ServicePointManager.ServerCertificateValidationCallback = delegate(object s, X509Certificate certificate,
                    X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };

                client.Send(mess);
                result = true;
            }
            catch (Exception ex)
            {
                mailresult = ex.Message;
                result = false;
            }
            return result;
        }
        #endregion

        public static bool SendMail(EmailParams param, ref string mailresult)
        {
            try
            {
                string content = string.Empty;
                string emailAddress = param.FromEmail;

                switch (param.SmtpHost)
                {
                    case "":
                    case null:
                        throw new ArgumentNullException("SMTP Server address found empty");
                }
                switch (emailAddress)
                {
                    case "":
                    case null:
                        throw new ArgumentNullException("From Email address found empty");
                }
                if ((param.ToEmail == string.Empty) || (param.ToEmail == null))
                {
                    throw new ArgumentNullException("To Email address found empty");
                }
                if (!IsValidEmailID(emailAddress))
                {
                    throw new FormatException("Invalid email ID found in From");
                }
                if ((param.Subject == string.Empty) || (param.Subject == null))
                {
                    throw new ArgumentNullException("Email subject found empty");
                }

                MailMessage message = new MailMessage();
                message.From = new MailAddress(emailAddress);
                if (param.Attachments != string.Empty)
                {
                    message = AddAttachments(param.Attachments, message);
                }
                message = addEmailAddress(message, param.ToEmail, "to");
                if (!string.IsNullOrEmpty(param.ReplyEmail))
                {
                    message = addEmailAddress(message, param.ReplyEmail, "reply");
                }
                if ((param.CcEmail != string.Empty) && (param.CcEmail != null))
                {
                    message = addEmailAddress(message, param.CcEmail, "cc");
                }
                if ((param.BccEmail != string.Empty) && (param.BccEmail != null))
                {
                    message = addEmailAddress(message, param.BccEmail, "bcc");
                }

                message.Subject = param.Subject;
                message.IsBodyHtml = true;
                content = param.EmailBody ?? ReadTemplateFile(param.EmailTemplatePath, param.EmailTemplateName);
                if (param.Htbl.Count > 0)
                {
                    content = ReplaceFileVariables(param.Htbl, content);
                }
                if ((content == string.Empty) || (content == null))
                {
                    throw new ArgumentNullException("Email body content found empty");
                }
                message.Body = content;
                bool mresult = SendMail(message, param.SmtpHost, param.SmtpUserName, param.SmtpPassword, param.SmtpPort, param.EnableSsl, ref mailresult);
                return mresult;
                //if (mresult)
                //{

                //    return true;
                //}
                //else
                //{
                //    return false;
                //}
            }
            catch (Exception ex)
            {
                mailresult = ex.Message;
                ////////////////////////////    ErrorRequestLog.WriteLog(ex.GetBaseException());
                // ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
                return false;
            }
        }
    }
}
