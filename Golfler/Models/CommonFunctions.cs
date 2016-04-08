using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using ErrorLibrary;
using Golfler.Repositories;
using System.Text.RegularExpressions;
using System.Data.Objects;
using System.Globalization;
using System.IO;

namespace Golfler.Models
{
    public class CommonFunctions
    {
        static GolflerEntities DbStatic;

        public static void SetDb()
        {
            DbStatic = new GolflerEntities();
        }

        /// <summary>
        /// Function to get current year
        /// </summary>
        /// <returns></returns>
        public static string GetYear()
        {
            return Convert.ToString(DateTime.Now.Year);
        }

        #region Encryption/ Decryption
        /// <summary>
        /// Encrypting params here
        /// </summary>
        /// <param name="Password"></param>
        /// <param name="salt"></param>
        /// <returns></returns>
        public static string EncryptString(string Password, string salt)
        {

            if (Password == null)
                return null;
            if (Password == "")
                return "";

            byte[] Results;
            System.Text.UTF8Encoding UTF8 = new System.Text.UTF8Encoding();

            // Step 1. We hash the passphrase using MD5
            // We use the MD5 hash generator as the result is a 128 bit byte array
            // which is a valid length for the TripleDES encoder we use below

            MD5CryptoServiceProvider HashProvider = new MD5CryptoServiceProvider();
            byte[] TDESKey = HashProvider.ComputeHash(UTF8.GetBytes(salt));

            // Step 2. Create a new TripleDESCryptoServiceProvider object
            TripleDESCryptoServiceProvider TDESAlgorithm = new TripleDESCryptoServiceProvider();

            // Step 3. Setup the encoder
            TDESAlgorithm.Key = TDESKey;
            TDESAlgorithm.Mode = CipherMode.ECB;
            TDESAlgorithm.Padding = PaddingMode.PKCS7;

            // Step 4. Convert the input string to a byte[]
            byte[] DataToEncrypt = UTF8.GetBytes(Password);

            // Step 5. Attempt to encrypt the string
            try
            {
                ICryptoTransform Encryptor = TDESAlgorithm.CreateEncryptor();
                Results = Encryptor.TransformFinalBlock(DataToEncrypt, 0, DataToEncrypt.Length);
            }
            finally
            {
                // Clear the TripleDes and Hashprovider services of any sensitive information
                TDESAlgorithm.Clear();
                HashProvider.Clear();
            }

            // Step 6. Return the encrypted string as a base64 encoded string
            return Convert.ToBase64String(Results);
        }

        /// <summary>
        /// Decrypting string here
        /// </summary>
        /// <param name="EncryptedString"></param>
        /// <param name="salt"></param>
        /// <returns></returns>
        public static string DecryptString(string EncryptedString, string salt)
        {
            if (EncryptedString == null)
                return null;
            if (EncryptedString == "")
                return "";

            byte[] Results;
            System.Text.UTF8Encoding UTF8 = new System.Text.UTF8Encoding();

            // Step 1. We hash the passphrase using MD5
            // We use the MD5 hash generator as the result is a 128 bit byte array
            // which is a valid length for the TripleDES encoder we use below

            MD5CryptoServiceProvider HashProvider = new MD5CryptoServiceProvider();
            byte[] TDESKey = HashProvider.ComputeHash(UTF8.GetBytes(salt));

            // Step 2. Create a new TripleDESCryptoServiceProvider object
            TripleDESCryptoServiceProvider TDESAlgorithm = new TripleDESCryptoServiceProvider();

            // Step 3. Setup the decoder
            TDESAlgorithm.Key = TDESKey;
            TDESAlgorithm.Mode = CipherMode.ECB;
            TDESAlgorithm.Padding = PaddingMode.PKCS7;

            // Step 4. Convert the input string to a byte[]
            byte[] DataToDecrypt = Convert.FromBase64String(EncryptedString);

            // Step 5. Attempt to decrypt the string
            try
            {
                ICryptoTransform Decryptor = TDESAlgorithm.CreateDecryptor();
                Results = Decryptor.TransformFinalBlock(DataToDecrypt, 0, DataToDecrypt.Length);
            }
            finally
            {
                // Clear the TripleDes and Hashprovider services of any sensitive information
                TDESAlgorithm.Clear();
                HashProvider.Clear();
            }

            // Step 6. Return the decrypted string in UTF8 format
            return UTF8.GetString(Results);
        }

        public static string GetUserNameMessageHistory(long msgHistoryid)
        {
            string usernam = "";
            DbStatic = new GolflerEntities();
            try
            {
                //   name = x.UserType != "G" ? (Db.GF_AdminUsers.FirstOrDefault(y => y.ID == x.LogUserID).Name) : (Db.GF_Golfer.FirstOrDefault(y => y.GF_ID == x.LogUserID).FirstName)
                var objUser = DbStatic.GF_ResolutionMessageHistory.Where(x => x.ID == msgHistoryid).OrderByDescending(x => x.ID).FirstOrDefault();
                if (objUser != null)
                {
                    string utype = objUser.UserType;

                    if (utype == UserType.Kitchen || utype == UserType.Cartie || utype == UserType.Ranger || utype == UserType.CourseAdmin || utype == UserType.Proshop || utype == UserType.SuperAdmin || utype == UserType.Admin)
                    {
                        var clsUser = DbStatic.GF_AdminUsers.Where(x => x.ID == objUser.LogUserID).FirstOrDefault();
                        if (clsUser != null)
                            usernam = clsUser.FirstName + " " + clsUser.LastName;
                    }
                    else
                    {
                        var clsUser = DbStatic.GF_Golfer.Where(x => x.GF_ID == objUser.LogUserID).FirstOrDefault();
                        if (clsUser != null)
                            usernam = clsUser.FirstName + " " + clsUser.LastName;
                    }

                }
                else
                {
                    usernam = "-";
                }
            }
            catch (Exception ex)
            {
                ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
                return null;
            }
            return usernam;
        }

        public static void ResolutionReplyByAnyType(string comment, string status, Int64 msgID)
        {
            string mailresult = "";
            var Db = new GolflerEntities();

            try
            {
                if (LoginInfo.LoginUserType == UserType.Kitchen || LoginInfo.LoginUserType == UserType.Cartie || LoginInfo.LoginUserType == UserType.Ranger || LoginInfo.LoginUserType == UserType.CourseAdmin || LoginInfo.LoginUserType == UserType.Proshop)
                {
                    #region Reply by Course User...send mail to golfer or super admin

                    Int64 FromUserid = LoginInfo.UserId;
                    string toEmail = "";
                    string toUserName = "";

                    var mainRes = Db.GF_ResolutionCenter.Where(x => x.ID == msgID).FirstOrDefault();
                    if (mainRes != null)
                    {
                        bool isGolfer = false;
                        if ((mainRes.GolferID ?? 0) != 0)
                        {
                            toEmail = Convert.ToString(mainRes.GF_Golfer.Email);
                            toUserName = Convert.ToString(mainRes.GF_Golfer.FirstName) + " " + Convert.ToString(mainRes.GF_Golfer.LastName);
                            isGolfer = true;
                        }
                        else
                        {
                            var adminUser = Db.GF_AdminUsers.FirstOrDefault(x => x.Type == UserType.SuperAdmin);

                            if (adminUser != null)
                            {
                                toEmail = Convert.ToString(adminUser.Email);
                                toUserName = adminUser.FirstName + " " + adminUser.LastName;
                                isGolfer = false;
                            }
                        }

                        if (isGolfer ? (mainRes.GF_Golfer.IsReceiveEmail ?? true) : true)
                        {
                            #region send mail
                            try
                            {
                                IQueryable<GF_EmailTemplatesFields> templateFields = null;
                                var param = EmailParams.GetEmailParamsNew(ref Db, EmailTemplateName.ResolutionCenterReply, ref templateFields, LoginInfo.CourseId, LoginType.CourseAdmin, ref mailresult);

                                if (mailresult == "") // means Parameters are OK
                                {
                                    if (ApplicationEmails.ResolutionReply(ref Db, LoginInfo.CourseId, comment, status, toEmail, toUserName, param, ref templateFields, ref mailresult))
                                    {
                                        // Do steps for Mail Send successful
                                        mailresult = "Resolution Reply has been successfully sent.";
                                    }
                                    else
                                    {
                                        // Do steps for Mail Failure. Mail failure reason can be find on "mailresult"
                                    }
                                }
                                else
                                {
                                    // Do steps for Parameters not available.Reason can be find on "mailresult"
                                }
                            }
                            catch (Exception ex)
                            {
                                mailresult = ex.Message;
                            }
                            #endregion
                        }
                    }

                    #endregion
                }
                else if (LoginInfo.GolferType == UserType.Golfer)
                {
                    #region Reply by Golfer...send mail to course/super admin

                    Int64 FromUserid = LoginInfo.GolferUserId;
                    string toEmail = "";
                    string toUserName = "";

                    var mainRes = Db.GF_ResolutionCenter.Where(x => x.ID == msgID).FirstOrDefault();
                    if (mainRes != null)
                    {
                        GetLatestUserAndEmailForReply(msgID, ref toEmail, ref toUserName);
                        if ((!(string.IsNullOrEmpty(Convert.ToString(toEmail)))) && (!(string.IsNullOrEmpty(Convert.ToString(toUserName)))))
                        {
                            if (mainRes.SendTo.ToUpper() == "SA" || mainRes.SendTo.ToUpper() == "A")
                            {
                                #region send mail
                                try
                                {
                                    IQueryable<GF_EmailTemplatesFields> templateFields = null;
                                    var param = EmailParams.GetEmailParamsNew(ref Db, EmailTemplateName.ResolutionCenterReply, ref templateFields, 0, LoginType.SuperAdmin, ref mailresult);

                                    if (mailresult == "") // means Parameters are OK
                                    {
                                        if (ApplicationEmails.ResolutionReply(ref Db, 0, comment, status, toEmail, toUserName, param, ref templateFields, ref mailresult))
                                        {
                                            // Do steps for Mail Send successful
                                            mailresult = "Resolution Reply has been successfully sent.";

                                            #region Auto Response
                                            param = EmailParams.GetEmailParamsNew(ref Db, EmailTemplateName.GolferAutoResponse, ref templateFields, 0, LoginType.SuperAdmin, ref mailresult);
                                            ApplicationEmails.AutoResponseToGolfer(ref Db, 0, comment, status, toEmail, toUserName, param, ref templateFields, ref mailresult);
                                            #endregion
                                        }
                                        else
                                        {
                                            // Do steps for Mail Failure. Mail failure reason can be find on "mailresult"
                                        }
                                    }
                                    else
                                    {
                                        // Do steps for Parameters not available.Reason can be find on "mailresult"
                                    }
                                }
                                catch (Exception ex)
                                {
                                    mailresult = ex.Message;
                                }
                                #endregion
                            }
                            else
                            {
                                #region send mail
                                try
                                {
                                    IQueryable<GF_EmailTemplatesFields> templateFields = null;
                                    var param = EmailParams.GetEmailParamsNew(ref Db, EmailTemplateName.ResolutionCenterReply, ref templateFields, Convert.ToInt64(mainRes.CourseID), LoginType.CourseAdmin, ref mailresult);

                                    if (mailresult == "") // means Parameters are OK
                                    {
                                        if (ApplicationEmails.ResolutionReply(ref Db, Convert.ToInt64(mainRes.CourseID), comment, status, toEmail, toUserName, param, ref templateFields, ref mailresult))
                                        {
                                            // Do steps for Mail Send successful
                                            mailresult = "Resolution Reply has been successfully sent.";
                                        }
                                        else
                                        {
                                            // Do steps for Mail Failure. Mail failure reason can be find on "mailresult"
                                        }
                                    }
                                    else
                                    {
                                        // Do steps for Parameters not available.Reason can be find on "mailresult"
                                    }
                                }
                                catch (Exception ex)
                                {
                                    mailresult = ex.Message;
                                }
                                #endregion
                            }
                        }
                    }
                    #endregion

                }
                else if (LoginInfo.Type == UserType.Admin || LoginInfo.Type == UserType.SuperAdmin)
                {
                    #region Reply by Super Admin...send mail to golfer or course admin/proshop

                    Int64 FromUserid = LoginInfo.UserId;
                    string toEmail = "";
                    string toUserName = "";

                    var mainRes = Db.GF_ResolutionCenter.Where(x => x.ID == msgID).FirstOrDefault();
                    if (mainRes != null)
                    {
                        bool isGolfer = false;
                        if ((mainRes.GolferID ?? 0) != 0)
                        {
                            toEmail = Convert.ToString(mainRes.GF_Golfer.Email);
                            toUserName = Convert.ToString(mainRes.GF_Golfer.FirstName) + " " + Convert.ToString(mainRes.GF_Golfer.LastName);
                            isGolfer = true;
                        }
                        else
                        {
                            var adminUser = Db.GF_AdminUsers.FirstOrDefault(x => x.ID == (mainRes.SenderID ?? 0) &&
                                x.Type == mainRes.SenderType);

                            if (adminUser != null)
                            {
                                toEmail = Convert.ToString(adminUser.Email);
                                toUserName = adminUser.FirstName + " " + adminUser.LastName;
                                isGolfer = false;
                            }
                        }

                        if (isGolfer ? (mainRes.GF_Golfer.IsReceiveEmail ?? true) : true)
                        {
                            #region send mail
                            try
                            {
                                IQueryable<GF_EmailTemplatesFields> templateFields = null;
                                var param = EmailParams.GetEmailParamsNew(ref Db, EmailTemplateName.ResolutionCenterReply, ref templateFields, 0, LoginType.SuperAdmin, ref mailresult);

                                if (mailresult == "") // means Parameters are OK
                                {
                                    if (ApplicationEmails.ResolutionReply(ref Db, 0, comment, status, toEmail, toUserName, param, ref templateFields, ref mailresult))
                                    {
                                        // Do steps for Mail Send successful
                                        mailresult = "Resolution Reply has been successfully sent.";
                                    }
                                    else
                                    {
                                        // Do steps for Mail Failure. Mail failure reason can be find on "mailresult"
                                    }
                                }
                                else
                                {
                                    // Do steps for Parameters not available.Reason can be find on "mailresult"
                                }
                            }
                            catch (Exception ex)
                            {
                                mailresult = ex.Message;
                            }
                            #endregion
                        }
                    }

                    #endregion
                }
            }
            catch (Exception ex)
            {
                ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
            }
        }

        public static string GetResolutionType(string strType)
        {
            if (!(string.IsNullOrEmpty(Convert.ToString(strType))))
            {
                if (Convert.ToString(strType) == ResolutionType.Praise)
                {
                    return ResolutionType.GetFullResolutionType(ResolutionType.Praise);
                }
                if (Convert.ToString(strType) == ResolutionType.Complaint)
                {
                    return ResolutionType.GetFullResolutionType(ResolutionType.Complaint);
                }
                if (Convert.ToString(strType) == ResolutionType.Others)
                {
                    return ResolutionType.GetFullResolutionType(ResolutionType.Others);
                }
                return "-";
            }
            else
            {
                return "-";
            }
        }


        public static string GetLatestUserName(long id, string mainSender)
        {
            string usernam = "";
            DbStatic = new GolflerEntities();
            try
            {
                //   name = x.UserType != "G" ? (Db.GF_AdminUsers.FirstOrDefault(y => y.ID == x.LogUserID).Name) : (Db.GF_Golfer.FirstOrDefault(y => y.GF_ID == x.LogUserID).FirstName)
                var objUser = DbStatic.GF_ResolutionMessageHistory.Where(x => x.MessageID == id).OrderByDescending(x => x.ID).FirstOrDefault();
                if (objUser != null)
                {
                    string utype = objUser.UserType;

                    if (utype == UserType.Kitchen || utype == UserType.Cartie || utype == UserType.Ranger || utype == UserType.CourseAdmin || utype == UserType.Proshop || utype == UserType.SuperAdmin || utype == UserType.Admin)
                    {
                        var clsUser = DbStatic.GF_AdminUsers.Where(x => x.ID == objUser.LogUserID).FirstOrDefault();
                        if (clsUser != null)
                            usernam = clsUser.FirstName + " " + clsUser.LastName;
                    }
                    else
                    {
                        var clsUser = DbStatic.GF_Golfer.Where(x => x.GF_ID == objUser.LogUserID).FirstOrDefault();
                        if (clsUser != null)
                            usernam = clsUser.FirstName + " " + clsUser.LastName;
                    }

                }
                else
                {
                    usernam = mainSender;
                }
            }
            catch (Exception ex)
            {
                ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
                return null;
            }
            return usernam;
        }

        public static DateTime GetLatestDate(long id, DateTime mainDateTime)
        {
            DateTime dtLatest;
            DbStatic = new GolflerEntities();
            var objUser = DbStatic.GF_ResolutionMessageHistory.Where(x => x.MessageID == id).OrderByDescending(x => x.ID).FirstOrDefault();
            if (objUser != null)
            {
                dtLatest = Convert.ToDateTime(objUser.CreatedDate);
            }
            else
            {
                dtLatest = mainDateTime;
            }
            return dtLatest;
        }

        public static string GetLatestStatus(long id, string mainStatus)
        {
            string latestStatus = "";
            DbStatic = new GolflerEntities();
            var objUser = DbStatic.GF_ResolutionMessageHistory.Where(x => x.MessageID == id).OrderByDescending(x => x.ID).FirstOrDefault();
            if (objUser != null)
            {
                latestStatus = objUser.Status;
            }
            else
            {
                latestStatus = mainStatus;
            }
            return latestStatus;
        }

        public static string GetLatestCommentsFromMessageHistory(long id, string mainComments)
        {
            string latestComments = "";
            DbStatic = new GolflerEntities();
            var objUser = DbStatic.GF_ResolutionMessageHistory.Where(x => x.MessageID == id).OrderByDescending(x => x.ID).FirstOrDefault();
            if (objUser != null)
            {
                latestComments = Convert.ToString(objUser.Message);
            }
            else
            {
                latestComments = mainComments;
            }
            return latestComments;
        }


        const string passphrase = "password";
        //Valid base64 characters are below.
        //ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/=
        public static string EncryptUrlParam(long id)
        {
            //return CommonLibClass.encryptStr(Convert.ToString(id)); 
            string Message = Convert.ToString(id);
            byte[] Results;
            System.Text.UTF8Encoding UTF8 = new System.Text.UTF8Encoding();
            MD5CryptoServiceProvider HashProvider = new MD5CryptoServiceProvider();
            byte[] TDESKey = HashProvider.ComputeHash(UTF8.GetBytes(passphrase));
            TripleDESCryptoServiceProvider TDESAlgorithm = new TripleDESCryptoServiceProvider();
            TDESAlgorithm.Key = TDESKey;
            TDESAlgorithm.Mode = CipherMode.ECB;
            TDESAlgorithm.Padding = PaddingMode.PKCS7;
            byte[] DataToEncrypt = UTF8.GetBytes(Message);
            try
            {
                ICryptoTransform Encryptor = TDESAlgorithm.CreateEncryptor();
                Results = Encryptor.TransformFinalBlock(DataToEncrypt, 0, DataToEncrypt.Length);
            }
            finally
            {
                TDESAlgorithm.Clear();
                HashProvider.Clear();
            }
            var baseStr = Convert.ToBase64String(Results);
            return baseStr.Replace("+", ",").Replace("/", "_").Replace("=", "-");
        }

        public static long DecryptUrlParam(string eid)
        {
            //return (eid != null) ? Convert.ToInt32(CommonLibClass.decryptStr(eid)) : 0;
            if (eid != null)
            {
                string Message = eid.Replace(",", "+").Replace("_", "/").Replace("-", "=");
                byte[] Results;
                System.Text.UTF8Encoding UTF8 = new System.Text.UTF8Encoding();
                MD5CryptoServiceProvider HashProvider = new MD5CryptoServiceProvider();
                byte[] TDESKey = HashProvider.ComputeHash(UTF8.GetBytes(passphrase));
                TripleDESCryptoServiceProvider TDESAlgorithm = new TripleDESCryptoServiceProvider();
                TDESAlgorithm.Key = TDESKey;
                TDESAlgorithm.Mode = CipherMode.ECB;
                TDESAlgorithm.Padding = PaddingMode.PKCS7;
                byte[] DataToDecrypt = Convert.FromBase64String(Message);
                try
                {
                    ICryptoTransform Decryptor = TDESAlgorithm.CreateDecryptor();
                    Results = Decryptor.TransformFinalBlock(DataToDecrypt, 0, DataToDecrypt.Length);
                }
                finally
                {
                    TDESAlgorithm.Clear();
                    HashProvider.Clear();
                }
                return Convert.ToInt64(UTF8.GetString(Results));
            }
            return 0;
        }
        #endregion

        #region Password
        /// <summary>
        /// Get password to match entered pwd
        /// </summary>
        /// <param name="p"></param>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string GetPassword(string p, string s)
        {
            return EncryptString(p, s);
        }

        /// <summary>
        /// password generation - forgot password
        /// </summary>
        /// <param name="p"></param>
        /// <param name="_salt"></param>
        /// <param name="_password"></param>
        public static void GeneratePassword(string p, ref string _salt, ref string _password)
        {
            _salt = CommonLibClass.FetchRandStr(3);
            _password = EncryptString(p, _salt);
        }

        /// <summary>
        /// Password decryption- log in
        /// </summary>
        /// <param name="p"></param>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string DecryptPassword(string p, string s)
        {
            return DecryptString(p, s);
        }
        #endregion

        #region mails
        public static bool IsValidEmailID(string emailAddress)
        {
            return Regex.IsMatch(emailAddress, @"^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$");
        }


        /// <summary>
        /// Bulding up the mail content for forgot email here
        /// </summary>
        /// <param name="objUser"></param>
        /// <param name="newpassword"></param>
        /// <returns></returns>
        //internal static bool SendAdminMailForgot(PS_AdminUser objUser, string newpassword)
        //{
        //    var htValues = new Hashtable();
        //    htValues.Add("LogoPath", ConfigClass.SiteUrl);
        //    htValues.Add("SiteName", ConfigClass.SiteName);
        //    htValues.Add("Fname", objUser.FirstName);
        //    htValues.Add("Lname", objUser.LastName);
        //    htValues.Add("UserName", objUser.Email);
        //    htValues.Add("Password", newpassword);
        //    return SendMail(objUser.Email, "Premier Services - Forgot Password", ConfigClass.EmailTemplatePath, "forgotpassword.htm", htValues);
        //}


        ///// <summary>
        ///// Bulding up the mail content for forgot email here
        ///// </summary>
        ///// <param name="objUser"></param>
        ///// <param name="newpassword"></param>
        ///// <returns></returns>
        //internal static bool SendMailForgot(PS_EmploymentApplication_1 objUser, string newpassword)
        //{
        //    var htValues = new Hashtable();
        //    htValues.Add("LogoPath", ConfigClass.SiteUrl);
        //    htValues.Add("SiteName", ConfigClass.SiteName);
        //    htValues.Add("Fname", CommonFunctions.DecryptString(objUser.FirstName, objUser.Salts));
        //    htValues.Add("Lname", CommonFunctions.DecryptString(objUser.LastName, objUser.Salts));
        //    htValues.Add("UserName", objUser.Email);
        //    htValues.Add("Password", newpassword);
        //    return SendMail(objUser.Email, "Premier Services - Forgot Password", ConfigClass.EmailTemplatePath, "forgotpassword.htm", htValues);
        //}

        ///// <summary>
        ///// Bulding up the mail content for Hire Congratulation email here
        ///// </summary>
        ///// <param name="objUser"></param>
        ///// <param name="newpassword"></param>
        ///// <returns></returns>
        //internal static bool SendMailHired(PS_EmploymentApplication_1 objUser)
        //{
        //    var htValues = new Hashtable();
        //    htValues.Add("LogoPath", ConfigClass.SiteUrl);
        //    htValues.Add("SiteName", ConfigClass.SiteName);
        //    htValues.Add("Fname", CommonFunctions.DecryptString(objUser.FirstName, objUser.Salts));
        //    htValues.Add("Lname", CommonFunctions.DecryptString(objUser.LastName, objUser.Salts));
        //    htValues.Add("UserName", objUser.Email);
        //    htValues.Add("Password", CommonFunctions.DecryptString(objUser.Password, objUser.Salts));
        //    htValues.Add("PostApplied", CommonFunctions.DecryptString(objUser.PositionApplied, objUser.Salts));
        //    return SendMail(objUser.Email, "Premier Services - Congratulations", ConfigClass.EmailTemplatePath, "hireGreeting.htm", htValues);
        //}

        ///// <summary>
        ///// Bulding up the mail content for Registration email here
        ///// </summary>
        ///// <param name="objUser"></param>
        ///// <param name="newpassword"></param>
        ///// <returns></returns>
        //internal static bool SendMailRegistration(EmploymentApplicationModel1 objUser)
        //{
        //    var htValues = new Hashtable();
        //    htValues.Add("LogoPath", ConfigClass.SiteUrl);
        //    htValues.Add("SiteName", ConfigClass.SiteName);
        //    htValues.Add("Fname", objUser.FirstName);
        //    htValues.Add("Lname", objUser.LastName);
        //    htValues.Add("UserName", objUser.Email);
        //    htValues.Add("Password", objUser.Password);
        //    return SendMail(objUser.Email, "Premier Services - Registration", ConfigClass.EmailTemplatePath, "registration.htm", htValues);
        //}

        internal static bool SendCustomerLead(string toMail, string subject, string template, Hashtable htValues)
        {
            var htVal = htValues;

            return SendMail(toMail, subject, ConfigClass.EmailTemplatePath, template, htVal);
        }

        /// <summary>
        /// Mail sending functionality implemented here
        /// </summary>
        /// <param name="email"></param>
        /// <param name="subject"></param>
        /// <param name="templatePath"></param>
        /// <param name="templateName"></param>
        /// <param name="hashVars"></param>
        /// <returns></returns>
        public static bool SendMail(string email, string subject, string templatePath, string templateName, Hashtable hashVars)
        {
            try
            {
                var footer = "Copyright &copy; " + GetYear();
                hashVars.Add("MailFooter", "Copyright @ " + GetYear());
                var mailParser = new Parser(HttpContext.Current.Server.MapPath("~/" + templatePath + "/" + templateName), hashVars);
                var message = new MailMessage();

                var fromAddress = new MailAddress(ConfigClass.FromMail);
                message.To.Add(email);
                message.From = fromAddress;
                message.Subject = subject;
                message.IsBodyHtml = true;
                message.Body = mailParser.Parse();
                message.BodyEncoding = Encoding.UTF8;

                var client = new SmtpClient(ConfigClass.SMTP_Host);
                var credential = new NetworkCredential();
                credential.UserName = ConfigClass.SMTP_Username;
                credential.Password = ConfigClass.SMTP_Password;
                client.Credentials = credential;
                client.Send(message);
                return true;
            }
            catch (Exception ex)
            {
                ErrorLibrary.ErrorClass.WriteLog(ex.InnerException, HttpContext.Current.Request);
                return false;
            }
        }

        #endregion

        public static string Message(string action, string module)
        {
            switch (action.ToLower())
            {
                case "add":
                    return "New " + module + " has been added.";
                case "update":
                    return module + " has been updated.";
                case "send":
                    return module + " send successfully.";
                case "unaccess":
                    return "You are not authorised to perform this action!";
                default:
                    return "";
            }
        }

        public static GF_Roles GetRolesforHeaderMenus(bool adminSec = false)
        {
            if ((LoginInfo.Type == UserType.Admin) || (LoginInfo.Type == UserType.SuperAdmin))
            {
                adminSec = true;
            }

            Role objRole = new Role();
            return (objRole.GetRoleByUserId(LoginInfo.UserId, adminSec));
        }

        public static void BindErrorLog()
        {
            if (ErrorClass.FilePath == null)
            {
                ErrorClass.LogMode = Convert.ToBoolean(Convert.ToInt32(ConfigurationManager.AppSettings["ERROR_LOG_MODE"]));
                ErrorClass.FilePath = HttpContext.Current.Request.PhysicalApplicationPath + Convert.ToString(ConfigurationManager.AppSettings["ERROR_LOG_DIR"]);
                ErrorClass.RequestLogMode = Convert.ToBoolean(Convert.ToInt32(ConfigurationManager.AppSettings["ERROR_LOG_MODE"]));
                ErrorClass.RequestFilePath = HttpContext.Current.Request.PhysicalApplicationPath + Convert.ToString(ConfigurationManager.AppSettings["ERROR_LOG_DIR"]);

            }
        }

        public static string GetCourseLink(long orgid)
        {
            return Params.GolflerUrl + GetGolfler(orgid);
        }

        internal static string GetGolfler(long courseID)
        {
            if (courseID == 0)
            {
                courseID = Convert.ToInt64(System.Configuration.ConfigurationManager.AppSettings["SuperAdminID"].ToString());
            }
            Templates objTemp = new Templates();
            List<GF_CourseInfo> lstCourse = new List<GF_CourseInfo>();
            lstCourse = objTemp.GetCourseDetails(courseID);
            return "#";
        }

        public static string GetCourseSuggestStatus(Int64 courseid)
        {
            string status = "-";
            DbStatic = new GolflerEntities();

            var varCourse = DbStatic.GF_CourseBuilder.Where(x => x.BuildBy == UserType.BuildByGolfer && x.CourseID == courseid && x.CreatedBy == LoginInfo.GolferUserId).FirstOrDefault();
            if (varCourse != null)
            {
                if (!string.IsNullOrEmpty(Convert.ToString(varCourse.Status)))
                {
                    if (varCourse.Status == StatusType.Active)
                    {
                        status = "Accepted";
                    }
                    if (varCourse.Status == StatusType.InActive)
                    {
                        status = "Pending";
                    }
                    if (varCourse.Status == StatusType.Delete)
                    {
                        status = "Rejected";
                    }
                }
            }
            return status;
        }

        public static string GetCourseNameById(Int64 courseid)
        {
            string name = "";
            DbStatic = new GolflerEntities();

            name = Convert.ToString(DbStatic.GF_CourseInfo.FirstOrDefault(x => x.ID == courseid).COURSE_NAME);

            return name;
        }

        public static string GetAvgTimePlay(Int64 RoundId, Int64 courseid, Int64 golferid, Int16 forHoles, List<Int64> validRoundForCalculations)
        {
            string AvgTime = "";

            DbStatic = new GolflerEntities();
            try
            {
                if (forHoles == 1) // Average for 1 Hole by valid rounds
                {
                    #region for 1 hour
                    var allData = DbStatic.GF_GolferPaceofPlay.Where(x => x.CourseId == courseid && x.GolferId == golferid && x.Status == StatusType.Delete).ToList();
                    foreach (var objData in allData)
                    {
                        if (!(validRoundForCalculations.Contains(Convert.ToInt64(objData.GameRoundID))))
                        {
                            Int64? grid = Convert.ToInt64(objData.GameRoundID);
                            allData = allData.Where(x => x.GameRoundID != grid).ToList();
                        }
                    }
                    var GetAllRounds = allData.GroupBy(x => x.GameRoundID).Select(x => x.First()).ToList();

                    Int64 cntRound = 0;
                    Int64 intMinutesCurrentHoleTime = 0;
                    foreach (var objROund in GetAllRounds)
                    {
                        var GetAllDataForRound = allData.Where(x => x.GameRoundID == objROund.GameRoundID).ToList().OrderBy(x => x.Time);

                        if (GetAllDataForRound.Count() > 0)
                        {
                            //if (GetAllDataForRound.Where(x => x.HoleNumber == 18 && x.Position == "Flag").ToList().Count > 0)
                            //{
                            //    if (GetAllDataForRound.Where(x => x.HoleNumber == 1 && x.Position == "Tee").ToList().Count > 0)
                            //    {
                            // cntRound = cntRound + 1;
                            DateTime startTime = Convert.ToDateTime(GetAllDataForRound.FirstOrDefault().Time);
                            DateTime endTime = Convert.ToDateTime(GetAllDataForRound.LastOrDefault().Time);

                            TimeSpan spanCurrentHoleTime = Convert.ToDateTime(endTime) - Convert.ToDateTime(startTime);
                            intMinutesCurrentHoleTime = intMinutesCurrentHoleTime + Convert.ToInt64(spanCurrentHoleTime.TotalMinutes);
                            if (Convert.ToInt64(spanCurrentHoleTime.TotalMinutes) > 0)
                            {
                                Int32 TotalHolesInThisRound = GetAllDataForRound.Count() / 2; // Convert.ToInt32(GetAllDataForRound.LastOrDefault().HoleNumber);
                                cntRound = cntRound + TotalHolesInThisRound;
                            }
                            //    }
                            //}
                        }
                    }
                    Int64 intAvgTime = intMinutesCurrentHoleTime / cntRound;

                    int totalMinutes = Convert.ToInt32(intAvgTime);
                    TimeSpan spnTotal = TimeSpan.FromMinutes(totalMinutes);

                    int days = spnTotal.Days;
                    int hours = spnTotal.Hours;
                    int totalMins = spnTotal.Minutes;
                    int secs = spnTotal.Seconds;
                    #region Day
                    if (days > 0)
                    {
                        if (AvgTime.Length > 0)
                        {
                            AvgTime = AvgTime + "," + days + " Day(s)";
                        }
                        else
                        {
                            AvgTime = days + " Day(s)";
                        }
                    }
                    #endregion

                    #region Hours
                    if (hours > 0)
                    {
                        if (AvgTime.Length > 0)
                        {
                            AvgTime = AvgTime + "," + hours + " Hour(s)";
                        }
                        else
                        {
                            AvgTime = hours + " Hour(s)";
                        }
                    }
                    #endregion

                    #region Mins
                    if (AvgTime.Length > 0)
                    {
                        AvgTime = AvgTime + "," + totalMins + " Minute(s)";
                    }
                    else
                    {
                        AvgTime = totalMins + " Minute(s)";
                    }
                    #endregion

                    #region seconds
                    if (secs > 0)
                    {
                        if (AvgTime.Length > 0)
                        {
                            AvgTime = AvgTime + "," + secs + " Second(s)";
                        }
                        else
                        {
                            AvgTime = secs + " Second(s)";
                        }
                    }
                    #endregion
                    #endregion
                }
                if (forHoles == 9) // Average for 9 Holes by valid rounds
                {
                    #region for 9 hours
                    var allData = DbStatic.GF_GolferPaceofPlay.Where(x => x.CourseId == courseid && x.GolferId == golferid && x.Status == StatusType.Delete).ToList();
                    foreach (var objData in allData)
                    {
                        if (!(validRoundForCalculations.Contains(Convert.ToInt64(objData.GameRoundID))))
                        {
                            Int64? grid = Convert.ToInt64(objData.GameRoundID);
                            allData = allData.Where(x => x.GameRoundID != grid).ToList();
                        }
                    }
                    var GetAllRounds = allData.GroupBy(x => x.GameRoundID).Select(x => x.First()).ToList();

                    Int64 cntRound = 0;
                    Int64 intMinutesCurrentHoleTime = 0;
                    foreach (var objROund in GetAllRounds)
                    {
                        var GetAllDataForRound = allData.Where(x => x.GameRoundID == objROund.GameRoundID).ToList().OrderBy(x => x.Time);

                        if (GetAllDataForRound.Count() > 0)
                        {
                            // Hole Count should be equal to 9
                            Int32 TotalHolesInThisRound = GetAllDataForRound.Count() / 2;
                            if (TotalHolesInThisRound == 9)
                            {
                                //if (GetAllDataForRound.Where(x => x.HoleNumber == 9 && x.Position == "Flag").ToList().Count > 0)
                                //{
                                //    if (GetAllDataForRound.Where(x => x.HoleNumber == 1 && x.Position == "Tee").ToList().Count > 0)
                                //    {
                                cntRound = cntRound + 1;
                                DateTime startTime = Convert.ToDateTime(GetAllDataForRound.FirstOrDefault().Time);
                                DateTime endTime = Convert.ToDateTime(GetAllDataForRound.LastOrDefault().Time);

                                TimeSpan spanCurrentHoleTime = Convert.ToDateTime(endTime) - Convert.ToDateTime(startTime);
                                intMinutesCurrentHoleTime = intMinutesCurrentHoleTime + Convert.ToInt64(spanCurrentHoleTime.TotalMinutes);

                                //    }
                                //}
                            }
                        }
                    }
                    Int64 intAvgTime = intMinutesCurrentHoleTime / cntRound;

                    int totalMinutes = Convert.ToInt32(intAvgTime);
                    TimeSpan spnTotal = TimeSpan.FromMinutes(totalMinutes);

                    int days = spnTotal.Days;
                    int hours = spnTotal.Hours;
                    int totalMins = spnTotal.Minutes;
                    int secs = spnTotal.Seconds;
                    #region Day
                    if (days > 0)
                    {
                        if (AvgTime.Length > 0)
                        {
                            AvgTime = AvgTime + "," + days + " Day(s)";
                        }
                        else
                        {
                            AvgTime = days + " Day(s)";
                        }
                    }
                    #endregion

                    #region Hours
                    if (hours > 0)
                    {
                        if (AvgTime.Length > 0)
                        {
                            AvgTime = AvgTime + "," + hours + " Hour(s)";
                        }
                        else
                        {
                            AvgTime = hours + " Hour(s)";
                        }
                    }
                    #endregion

                    #region Mins
                    if (AvgTime.Length > 0)
                    {
                        AvgTime = AvgTime + "," + totalMins + " Minute(s)";
                    }
                    else
                    {
                        AvgTime = totalMins + " Minute(s)";
                    }
                    #endregion

                    #region seconds
                    if (secs > 0)
                    {
                        if (AvgTime.Length > 0)
                        {
                            AvgTime = AvgTime + "," + secs + " Second(s)";
                        }
                        else
                        {
                            AvgTime = secs + " Second(s)";
                        }
                    }
                    #endregion
                    #endregion
                }
                if (forHoles == 18) // Average for 18 Holes by valid rounds
                {
                    #region for 18 hours
                    var allData = DbStatic.GF_GolferPaceofPlay.Where(x => x.CourseId == courseid && x.GolferId == golferid && x.Status == StatusType.Delete).ToList();
                    foreach (var objData in allData)
                    {
                        if (!(validRoundForCalculations.Contains(Convert.ToInt64(objData.GameRoundID))))
                        {
                            Int64? grid = Convert.ToInt64(objData.GameRoundID);
                            allData = allData.Where(x => x.GameRoundID != grid).ToList();
                        }
                    }
                    var GetAllRounds = allData.GroupBy(x => x.GameRoundID).Select(x => x.First()).ToList();

                    Int64 cntRound = 0;
                    Int64 intMinutesCurrentHoleTime = 0;
                    foreach (var objROund in GetAllRounds)
                    {
                        var GetAllDataForRound = allData.Where(x => x.GameRoundID == objROund.GameRoundID).ToList().OrderBy(x => x.Time);

                        if (GetAllDataForRound.Count() > 0)
                        {
                            // Hole Count should be equal to 18
                            Int32 TotalHolesInThisRound = GetAllDataForRound.Count() / 2;
                            if (TotalHolesInThisRound == 18)
                            {
                                //if (GetAllDataForRound.Where(x => x.HoleNumber == 18 && x.Position == "Flag").ToList().Count > 0)
                                //{
                                //    if (GetAllDataForRound.Where(x => x.HoleNumber == 1 && x.Position == "Tee").ToList().Count > 0)
                                //    {
                                cntRound = cntRound + 1;
                                DateTime startTime = Convert.ToDateTime(GetAllDataForRound.FirstOrDefault().Time);
                                DateTime endTime = Convert.ToDateTime(GetAllDataForRound.LastOrDefault().Time);

                                TimeSpan spanCurrentHoleTime = Convert.ToDateTime(endTime) - Convert.ToDateTime(startTime);
                                intMinutesCurrentHoleTime = intMinutesCurrentHoleTime + Convert.ToInt64(spanCurrentHoleTime.TotalMinutes);

                                //    }
                                //}
                            }
                        }
                    }
                    Int64 intAvgTime = intMinutesCurrentHoleTime / cntRound;

                    int totalMinutes = Convert.ToInt32(intAvgTime);
                    TimeSpan spnTotal = TimeSpan.FromMinutes(totalMinutes);

                    int days = spnTotal.Days;
                    int hours = spnTotal.Hours;
                    int totalMins = spnTotal.Minutes;
                    int secs = spnTotal.Seconds;
                    #region Day
                    if (days > 0)
                    {
                        if (AvgTime.Length > 0)
                        {
                            AvgTime = AvgTime + "," + days + " Day(s)";
                        }
                        else
                        {
                            AvgTime = days + " Day(s)";
                        }
                    }
                    #endregion

                    #region Hours
                    if (hours > 0)
                    {
                        if (AvgTime.Length > 0)
                        {
                            AvgTime = AvgTime + "," + hours + " Hour(s)";
                        }
                        else
                        {
                            AvgTime = hours + " Hour(s)";
                        }
                    }
                    #endregion

                    #region Mins
                    if (AvgTime.Length > 0)
                    {
                        AvgTime = AvgTime + "," + totalMins + " Minute(s)";
                    }
                    else
                    {
                        AvgTime = totalMins + " Minute(s)";
                    }
                    #endregion

                    #region seconds
                    if (secs > 0)
                    {
                        if (AvgTime.Length > 0)
                        {
                            AvgTime = AvgTime + "," + secs + " Second(s)";
                        }
                        else
                        {
                            AvgTime = secs + " Second(s)";
                        }
                    }
                    #endregion
                    #endregion
                }
                if (forHoles == 0) // TOTAL TIME OF PLAY for a specific Round
                {
                    #region
                    var GetAllDataForRound = DbStatic.GF_GolferPaceofPlay.Where(x => x.GameRoundID == RoundId && x.GolferId==golferid).OrderBy(x => x.Time).ToList();

                    //Int64 cntRound = 0;
                    Int64 intMinutesCurrentHoleTime = 0;
                    //foreach (var objROund in GetAllRounds)
                    //{
                    //    var GetAllDataForRound = allData.Where(x => x.GameRoundID == objROund.GameRoundID).ToList().OrderBy(x => x.Time);

                    if (GetAllDataForRound.Count() > 0)
                    {
                        //if (GetAllDataForRound.Where(x => x.HoleNumber == 18 && x.Position == "Flag").ToList().Count > 0)
                        //{
                        //    if (GetAllDataForRound.Where(x => x.HoleNumber == 1 && x.Position == "Tee").ToList().Count > 0)
                        //    {
                        // cntRound = cntRound + 1;
                        DateTime startTime = Convert.ToDateTime(GetAllDataForRound.FirstOrDefault().Time);
                        DateTime endTime = Convert.ToDateTime(GetAllDataForRound.LastOrDefault().Time);

                        TimeSpan spanCurrentHoleTime = Convert.ToDateTime(endTime) - Convert.ToDateTime(startTime);
                        intMinutesCurrentHoleTime = intMinutesCurrentHoleTime + Convert.ToInt64(spanCurrentHoleTime.TotalMinutes);
                        //if (Convert.ToInt64(spanCurrentHoleTime.TotalMinutes) > 0)
                        //{
                        //    Int32 TotalHolesInThisRound = Convert.ToInt32(GetAllDataForRound.LastOrDefault().HoleNumber);
                        //    cntRound = cntRound + TotalHolesInThisRound;
                        //}
                        //    }
                        //}
                    }
                    //}
                    // Int64 intAvgTime = intMinutesCurrentHoleTime / cntRound;

                    int totalMinutes = Convert.ToInt32(intMinutesCurrentHoleTime);
                    TimeSpan spnTotal = TimeSpan.FromMinutes(totalMinutes);

                    int days = spnTotal.Days;
                    int hours = spnTotal.Hours;
                    int totalMins = spnTotal.Minutes;
                    int secs = spnTotal.Seconds;
                    #region Day
                    if (days > 0)
                    {
                        if (AvgTime.Length > 0)
                        {
                            AvgTime = AvgTime + "," + days + " Day(s)";
                        }
                        else
                        {
                            AvgTime = days + " Day(s)";
                        }
                    }
                    #endregion

                    #region Hours
                    if (hours > 0)
                    {
                        if (AvgTime.Length > 0)
                        {
                            AvgTime = AvgTime + "," + hours + " Hour(s)";
                        }
                        else
                        {
                            AvgTime = hours + " Hour(s)";
                        }
                    }
                    #endregion

                    #region Mins
                    if (AvgTime.Length > 0)
                    {
                        AvgTime = AvgTime + "," + totalMins + " Minute(s)";
                    }
                    else
                    {
                        AvgTime = totalMins + " Minute(s)";
                    }
                    #endregion

                    #region seconds
                    if (secs > 0)
                    {
                        if (AvgTime.Length > 0)
                        {
                            AvgTime = AvgTime + "," + secs + " Second(s)";
                        }
                        else
                        {
                            AvgTime = secs + " Second(s)";
                        }
                    }
                    #endregion
                    #endregion
                }
                if (forHoles == 2) // Total Holes in a specific Round
                {
                    #region
                    var GetAllDataForRound = DbStatic.GF_GolferPaceofPlay.Where(x => x.GameRoundID == RoundId && x.GolferId == golferid).ToList();

                    Int64 cntRound = 0;


                    if (GetAllDataForRound.Count() > 0)
                    {

                        cntRound = GetAllDataForRound.Count() / 2;
                        //DateTime startTime = Convert.ToDateTime(GetAllDataForRound.FirstOrDefault().Time);
                        //DateTime endTime = Convert.ToDateTime(GetAllDataForRound.LastOrDefault().Time);

                        //TimeSpan spanCurrentHoleTime = Convert.ToDateTime(endTime) - Convert.ToDateTime(startTime);
                        //if (Convert.ToInt64(spanCurrentHoleTime.TotalMinutes) > 0)
                        //{
                        //    Int32 TotalHolesInThisRound = Convert.ToInt32(GetAllDataForRound.LastOrDefault().HoleNumber);
                        //    cntRound = TotalHolesInThisRound;
                        //}
                    }
                    AvgTime = Convert.ToString(cntRound);


                    #endregion
                }
                if (forHoles == 11) // Average for 1 Hole for specific round
                {
                    #region for 1 hole
                    var GetAllDataForRound = DbStatic.GF_GolferPaceofPlay.Where(x => x.GameRoundID == RoundId && x.GolferId==golferid).ToList().OrderBy(x => x.Time);


                    Int64 cntRound = 0;
                    Int64 intMinutesCurrentHoleTime = 0;

                    // var GetAllDataForRound = allData.Where(x => x.GameRoundID == objROund.GameRoundID).ToList().OrderBy(x => x.Time);

                    if (GetAllDataForRound.Count() > 0)
                    {
                        //if (GetAllDataForRound.Where(x => x.HoleNumber == 18 && x.Position == "Flag").ToList().Count > 0)
                        //{
                        //    if (GetAllDataForRound.Where(x => x.HoleNumber == 1 && x.Position == "Tee").ToList().Count > 0)
                        //    {
                        // cntRound = cntRound + 1;
                        DateTime startTime = Convert.ToDateTime(GetAllDataForRound.FirstOrDefault().Time);
                        DateTime endTime = Convert.ToDateTime(GetAllDataForRound.LastOrDefault().Time);

                        TimeSpan spanCurrentHoleTime = Convert.ToDateTime(endTime) - Convert.ToDateTime(startTime);
                        intMinutesCurrentHoleTime = intMinutesCurrentHoleTime + Convert.ToInt64(spanCurrentHoleTime.TotalMinutes);
                        if (Convert.ToInt64(spanCurrentHoleTime.TotalMinutes) > 0)
                        {
                            Int32 TotalHolesInThisRound = GetAllDataForRound.Count() / 2; //Convert.ToInt32(GetAllDataForRound.LastOrDefault().HoleNumber);
                            cntRound = cntRound + TotalHolesInThisRound;
                        }
                        //    }
                        //}
                    }

                    Int64 intAvgTime = intMinutesCurrentHoleTime / cntRound;

                    int totalMinutes = Convert.ToInt32(intAvgTime);
                    TimeSpan spnTotal = TimeSpan.FromMinutes(totalMinutes);

                    int days = spnTotal.Days;
                    int hours = spnTotal.Hours;
                    int totalMins = spnTotal.Minutes;
                    int secs = spnTotal.Seconds;
                    #region Day
                    if (days > 0)
                    {
                        if (AvgTime.Length > 0)
                        {
                            AvgTime = AvgTime + "," + days + " Day(s)";
                        }
                        else
                        {
                            AvgTime = days + " Day(s)";
                        }
                    }
                    #endregion

                    #region Hours
                    if (hours > 0)
                    {
                        if (AvgTime.Length > 0)
                        {
                            AvgTime = AvgTime + "," + hours + " Hour(s)";
                        }
                        else
                        {
                            AvgTime = hours + " Hour(s)";
                        }
                    }
                    #endregion

                    #region Mins
                    if (AvgTime.Length > 0)
                    {
                        AvgTime = AvgTime + "," + totalMins + " Minute(s)";
                    }
                    else
                    {
                        AvgTime = totalMins + " Minute(s)";
                    }
                    #endregion

                    #region seconds
                    if (secs > 0)
                    {
                        if (AvgTime.Length > 0)
                        {
                            AvgTime = AvgTime + "," + secs + " Second(s)";
                        }
                        else
                        {
                            AvgTime = secs + " Second(s)";
                        }
                    }
                    #endregion
                    #endregion
                }
                if (forHoles == 99) // Average for 9 Holes for specific round
                {
                    #region for 9 hours
                    var GetAllDataForRound = DbStatic.GF_GolferPaceofPlay.Where(x => x.GameRoundID == RoundId && x.GolferId==golferid).ToList().OrderBy(x => x.Time);

                    //   Int64 cntRound = 0;
                    Int64 intMinutesCurrentHoleTime = 0;

                    if (GetAllDataForRound.Count() > 0)
                    {
                        // Hole Count should be equal to 9
                        Int32 TotalHolesInThisRound = GetAllDataForRound.Count() / 2;
                        if (TotalHolesInThisRound == 9)
                        {
                            //if (GetAllDataForRound.Where(x => x.HoleNumber == 9 && x.Position == "Flag").ToList().Count > 0)
                            //{
                            //    if (GetAllDataForRound.Where(x => x.HoleNumber == 1 && x.Position == "Tee").ToList().Count > 0)
                            //    {
                            //     cntRound = cntRound + 1;
                            DateTime startTime = Convert.ToDateTime(GetAllDataForRound.FirstOrDefault().Time);
                            DateTime endTime = Convert.ToDateTime(GetAllDataForRound.LastOrDefault().Time);

                            TimeSpan spanCurrentHoleTime = Convert.ToDateTime(endTime) - Convert.ToDateTime(startTime);
                            intMinutesCurrentHoleTime = intMinutesCurrentHoleTime + Convert.ToInt64(spanCurrentHoleTime.TotalMinutes);

                            //    }
                            //}
                        }
                    }

                    Int64 intAvgTime = intMinutesCurrentHoleTime;

                    int totalMinutes = Convert.ToInt32(intAvgTime);
                    TimeSpan spnTotal = TimeSpan.FromMinutes(totalMinutes);

                    int days = spnTotal.Days;
                    int hours = spnTotal.Hours;
                    int totalMins = spnTotal.Minutes;
                    int secs = spnTotal.Seconds;
                    #region Day
                    if (days > 0)
                    {
                        if (AvgTime.Length > 0)
                        {
                            AvgTime = AvgTime + "," + days + " Day(s)";
                        }
                        else
                        {
                            AvgTime = days + " Day(s)";
                        }
                    }
                    #endregion

                    #region Hours
                    if (hours > 0)
                    {
                        if (AvgTime.Length > 0)
                        {
                            AvgTime = AvgTime + "," + hours + " Hour(s)";
                        }
                        else
                        {
                            AvgTime = hours + " Hour(s)";
                        }
                    }
                    #endregion

                    #region Mins
                    if (AvgTime.Length > 0)
                    {
                        AvgTime = AvgTime + "," + totalMins + " Minute(s)";
                    }
                    else
                    {
                        AvgTime = totalMins + " Minute(s)";
                    }
                    #endregion

                    #region seconds
                    if (secs > 0)
                    {
                        if (AvgTime.Length > 0)
                        {
                            AvgTime = AvgTime + "," + secs + " Second(s)";
                        }
                        else
                        {
                            AvgTime = secs + " Second(s)";
                        }
                    }
                    #endregion
                    #endregion
                }
                if (forHoles == 188) // Average for 18 Holes for specific round
                {
                    #region for 18 hours
                    var GetAllDataForRound = DbStatic.GF_GolferPaceofPlay.Where(x => x.GameRoundID == RoundId && x.GolferId==golferid).ToList().OrderBy(x => x.Time);

                    Int64 intMinutesCurrentHoleTime = 0;

                    if (GetAllDataForRound.Count() > 0)
                    {
                        // Hole Count should be equal to 18
                        Int32 TotalHolesInThisRound = GetAllDataForRound.Count() / 2;
                        if (TotalHolesInThisRound == 18)
                        {
                            //if (GetAllDataForRound.Where(x => x.HoleNumber == 18 && x.Position == "Flag").ToList().Count > 0)
                            //{
                            //    if (GetAllDataForRound.Where(x => x.HoleNumber == 1 && x.Position == "Tee").ToList().Count > 0)
                            //    {
                            DateTime startTime = Convert.ToDateTime(GetAllDataForRound.FirstOrDefault().Time);
                            DateTime endTime = Convert.ToDateTime(GetAllDataForRound.LastOrDefault().Time);

                            TimeSpan spanCurrentHoleTime = Convert.ToDateTime(endTime) - Convert.ToDateTime(startTime);
                            intMinutesCurrentHoleTime = intMinutesCurrentHoleTime + Convert.ToInt64(spanCurrentHoleTime.TotalMinutes);
                            //    }
                            //}
                        }
                    }

                    Int64 intAvgTime = intMinutesCurrentHoleTime;

                    int totalMinutes = Convert.ToInt32(intAvgTime);
                    TimeSpan spnTotal = TimeSpan.FromMinutes(totalMinutes);

                    int days = spnTotal.Days;
                    int hours = spnTotal.Hours;
                    int totalMins = spnTotal.Minutes;
                    int secs = spnTotal.Seconds;
                    #region Day
                    if (days > 0)
                    {
                        if (AvgTime.Length > 0)
                        {
                            AvgTime = AvgTime + "," + days + " Day(s)";
                        }
                        else
                        {
                            AvgTime = days + " Day(s)";
                        }
                    }
                    #endregion

                    #region Hours
                    if (hours > 0)
                    {
                        if (AvgTime.Length > 0)
                        {
                            AvgTime = AvgTime + "," + hours + " Hour(s)";
                        }
                        else
                        {
                            AvgTime = hours + " Hour(s)";
                        }
                    }
                    #endregion

                    #region Mins
                    if (AvgTime.Length > 0)
                    {
                        AvgTime = AvgTime + "," + totalMins + " Minute(s)";
                    }
                    else
                    {
                        AvgTime = totalMins + " Minute(s)";
                    }
                    #endregion

                    #region seconds
                    if (secs > 0)
                    {
                        if (AvgTime.Length > 0)
                        {
                            AvgTime = AvgTime + "," + secs + " Second(s)";
                        }
                        else
                        {
                            AvgTime = secs + " Second(s)";
                        }
                    }
                    #endregion
                    #endregion
                }

            }
            catch
            {
            }

            if (AvgTime == "")
            {
                AvgTime = "-";
            }
            return AvgTime;
        }

        public static Int64 NumberOfSuggestions(Int64 courseid)
        {
            DbStatic = new GolflerEntities();

            var intcount = DbStatic.GF_CourseBuilder.Where(x => x.BuildBy == UserType.BuildByGolfer && x.Status != StatusType.Delete && x.CourseID == courseid).Count();
            return intcount;
        }

        #region Users Related

        public static GF_RoleModules GetAccessModule(string module)
        {
            var role = new Role();
            return role.GetRoleModuleByUserId(LoginInfo.UserId, module, LoginInfo.LoginUserType == UserTypeChar.AdminChar);
        }

        public static GF_RoleModules GetGolferAccessModule(string module)
        {
            var role = new Role();
            return role.GetRoleModuleByUserId(LoginInfo.GolferUserId, module, LoginInfo.GolferType == UserType.Golfer);
        }

        public static object UserCount
        {
            get
            {
                return DbStatic.GF_AdminUsers.Count(x =>
                    x.CourseId == LoginInfo.CourseId &&
                    x.Type.Contains(LoginInfo.LoginUserType) &&
                    x.Type != UserType.SuperAdmin &&
                    x.Status != StatusType.Delete) + 1;
            }
        }

        public static object AdminUserCount
        {
            get
            {
                return DbStatic.GF_AdminUsers.Count(x =>
                    x.CourseId == LoginInfo.CourseId &&
                    x.Type.Contains(LoginInfo.LoginUserType) &&
                    x.Status != StatusType.Delete);
            }
        }

        public static string GetUserName(long userid, string userType)
        {
            string result = "";
            try
            {
                if (!(string.IsNullOrEmpty(Convert.ToString(userType))))
                {
                    result = DbStatic.GF_AdminUsers.Where(m => m.ID == userid).Select(m => m.FirstName + " " + m.LastName).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
            }

            return result;
        }

        public static string GetUserTypeFullText(string userType)
        {
            string result = "";
            try
            {
                if (!(string.IsNullOrEmpty(Convert.ToString(userType))))
                {
                    if (Convert.ToString(userType) == "A")
                    {
                        result = "Admin User";
                    }
                    else
                    {
                        result = "End User";
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
            }

            return result;
        }

        #endregion

        public static string GetCurrentYear()
        {
            string currentYear = Convert.ToString(DateTime.Now.Year);
            return currentYear;
        }

        public static string GetSuperAdminEmail()
        {
            string email = "";

            try
            {
                DbStatic = new GolflerEntities();
                email = Convert.ToString(DbStatic.GF_AdminUsers.Where(m => m.Type == UserType.SuperAdmin).Select(m => m.Email).FirstOrDefault());
                if (string.IsNullOrEmpty(Convert.ToString(email)))
                {
                    email = "";
                }
            }
            catch (Exception ex)
            {
                ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
            }

            return email;

        }

        public static object CourseCount { get { return DbStatic.GF_CourseInfo.Count(); } }
        public static long[] ConvertStringArrayToLongArray(string str)
        {
            return str.Split(",".ToCharArray()).Select(x => long.Parse(x.ToString())).ToArray();
        }

        /// <summary>
        /// Created By: Ramesh Kalra
        ///  Created Date: 12 May 2015
        ///  Purpose: Get Golfers location with Hole Position
        /// </summary>
        /// <param name="courseid"></param>
        /// <param name="golferid"></param>
        /// <param name="CurrentHole"></param>
        /// <param name="TotalTimeSpend"></param>
        /// <param name="HoleTimings"></param>
        public static void GetGolferHoleInformation(Int64 courseid, Int64 golferid, ref string CurrentHole, ref string CurrentHoleTime, ref Int64 CurrentHoleTimeInMins, ref string TotalTimeSpend, ref Dictionary<string, string> HoleTimings, ref string Round, ref string AverageTime)
        {
            try
            {
                GolflerEntities DbGolfer = new GolflerEntities();

                var HoleDetails = DbGolfer.GF_GolferPaceofPlay.Where(x => x.CourseId == courseid && x.Status == StatusType.Active && x.GolferId == golferid).OrderByDescending(x => x.Time).ToList();
                if (HoleDetails.Count > 0)
                {
                    Round = "started";

                    CurrentHole = Convert.ToString(HoleDetails.FirstOrDefault().HoleNumber);
                    if (CurrentHole == "0" || CurrentHole == "")
                    {
                        CurrentHole = "N.A.";
                    }

                    var startTime = HoleDetails.LastOrDefault().Time;
                    var endTime = DateTime.UtcNow;

                    TimeSpan spnTotal = Convert.ToDateTime(endTime) - Convert.ToDateTime(startTime);
                    int days = spnTotal.Days;
                    int hours = spnTotal.Hours;
                    int totalMins = spnTotal.Minutes;
                    int secs = spnTotal.Seconds;
                    #region Day
                    if (days > 0)
                    {
                        if (TotalTimeSpend.Length > 0)
                        {
                            TotalTimeSpend = TotalTimeSpend + "," + days + " D";
                        }
                        else
                        {
                            TotalTimeSpend = days + " D";
                        }
                    }
                    #endregion

                    #region Hours
                    if (TotalTimeSpend.Length > 0)
                    {
                        TotalTimeSpend = TotalTimeSpend + "," + hours + " H";
                    }
                    else
                    {
                        TotalTimeSpend = hours + " H";
                    }
                    #endregion

                    #region Mins
                    if (TotalTimeSpend.Length > 0)
                    {
                        TotalTimeSpend = TotalTimeSpend + "," + totalMins + " M";
                    }
                    else
                    {
                        TotalTimeSpend = totalMins + " M";
                    }
                    #endregion

                    #region seconds
                    if (secs > 0)
                    {
                        if (TotalTimeSpend.Length > 0)
                        {
                            TotalTimeSpend = TotalTimeSpend + "," + secs + " S";
                        }
                        else
                        {
                            TotalTimeSpend = secs + " S";
                        }
                    }
                    #endregion
                    //}

                    //CurrentHole = Convert.ToString(HoleDetails.LastOrDefault().HoleNumber);

                    List<Int64> lstIntHoles = new List<Int64>();
                    List<GF_GolferPaceofPlay> lstHoleDetails = HoleDetails.OrderBy(X => X.Time).ToList();
                    foreach (var objHole in lstHoleDetails)
                    {
                        Int64 hole = Convert.ToInt64(objHole.HoleNumber);
                        bool isExists = false;
                        foreach (var chkHole in lstIntHoles)
                        {
                            if (chkHole == hole)
                                isExists = true;
                        }
                        if (!isExists)
                        {
                            bool isTee = false;
                            bool isFlag = false;

                            Int64? blHole = Convert.ToInt64(objHole.HoleNumber);
                            if (HoleDetails.Where(x => x.HoleNumber == blHole && x.Position == "Tee").Count() > 0)
                            {
                                isTee = true;
                            }

                            if (HoleDetails.Where(x => x.HoleNumber == blHole && x.Position == "Flag").Count() > 0)
                            {
                                isFlag = true;
                            }

                            if (isTee && isFlag)
                            {
                                lstIntHoles.Add(hole);
                            }
                        }
                    }

                    // for (int cnt = 1; cnt <= Convert.ToInt16(CurrentHole); cnt++)
                    int cnt = Convert.ToInt32(lstIntHoles.FirstOrDefault());
                    List<Int64> lstMinutes = new List<Int64>();
                    foreach (var objHole in lstIntHoles)
                    {
                        DateTime HoleStartTime = Convert.ToDateTime("01/10/1111");
                        DateTime NextHoleStartTime = Convert.ToDateTime("01/10/1111");
                        DateTime HoleEndTime = Convert.ToDateTime("01/11/1111");

                        int NextHoleNumber = cnt + 1;

                        //lstHoleDetails.next(x => x.HoleNumber = cnt && x.Position == "Flag").ToList();
                        if (cnt == 9)
                        {
                            if (HoleDetails.Where(x => x.HoleNumber == 10 && x.Position == "Tee").Count() <= 0)
                            {
                                NextHoleNumber = 1;
                            }
                        }
                        if (cnt == 18)
                        {
                            NextHoleNumber = 1;
                        }

                        var varHoleStartTime = HoleDetails.Where(x => x.HoleNumber == cnt && x.Position == "Tee").FirstOrDefault();

                        if (varHoleStartTime != null)
                        {
                            HoleStartTime = Convert.ToDateTime(varHoleStartTime.Time);
                        }

                        var varHoleEndTime = HoleDetails.Where(x => x.HoleNumber == cnt && x.Position == "Flag").FirstOrDefault();
                        if (varHoleEndTime != null)
                        {
                            HoleEndTime = Convert.ToDateTime(varHoleEndTime.Time);
                        }

                        var varNextHoleStartTime = HoleDetails.Where(x => x.HoleNumber == NextHoleNumber && x.Position == "Tee").FirstOrDefault();

                        if (varNextHoleStartTime != null)
                        {
                            NextHoleStartTime = Convert.ToDateTime(varNextHoleStartTime.Time);
                        }

                        string timeSpendInHole = "n.a.";

                        // if ((!string.IsNullOrEmpty(Convert.ToString(HoleStartTime))) && (!string.IsNullOrEmpty(Convert.ToString(HoleEndTime))))
                        if ((varHoleStartTime != null) && (NextHoleStartTime != null))
                        {
                            if ((!string.IsNullOrEmpty(Convert.ToString(HoleStartTime))) && (!string.IsNullOrEmpty(Convert.ToString(NextHoleStartTime))))
                            {
                                if ((HoleStartTime != Convert.ToDateTime("01/10/1111")) && (NextHoleStartTime != Convert.ToDateTime("01/10/1111")))
                                {
                                    //timeSpendInHole = Convert.ToString(HoleEndTime - HoleStartTime);
                                    TimeSpan spantimeSpendInHole = Convert.ToDateTime(NextHoleStartTime) - Convert.ToDateTime(HoleStartTime);
                                    int intMinutesSpendInHole = spantimeSpendInHole.Minutes;
                                    lstMinutes.Add(Convert.ToInt64(intMinutesSpendInHole));
                                    timeSpendInHole = Convert.ToString(intMinutesSpendInHole) + " Minute(s)";
                                }
                            }
                        }

                        if (cnt == 9)
                        {
                            if (HoleDetails.Where(x => x.HoleNumber == 10 && x.Position == "Tee").Count() <= 0)
                            {
                                cnt = 1;
                            }
                        }
                        else
                        {
                            if (cnt == 18)
                            {
                                NextHoleNumber = 1;
                            }
                            else
                            {
                                cnt = cnt + 1;
                            }
                        }
                    }

                    Int64 totalMinsFinal = Convert.ToInt64("0");
                    foreach (var mins in lstMinutes)
                    {
                        totalMinsFinal = Convert.ToInt64(totalMinsFinal) + Convert.ToInt64(mins);
                    }

                    if (lstMinutes.Count > 0)
                    {
                        AverageTime = Convert.ToString(totalMinsFinal / lstMinutes.Count) + " M";
                    }
                    else
                    {
                        AverageTime = "N.A.";
                    }

                    #region Current Hole Time
                    var lastRecord = HoleDetails.FirstOrDefault();
                    if (lastRecord != null)
                    {
                        if (lastRecord.Position == "Tee")
                        {

                            DateTime startCurrentTime = Convert.ToDateTime(lastRecord.Time);
                            DateTime endCurrentTime = DateTime.UtcNow;

                            TimeSpan spnCurrentTotal = Convert.ToDateTime(endCurrentTime) - Convert.ToDateTime(startCurrentTime);
                            int Currentdays = spnCurrentTotal.Days;
                            int Currenthours = spnCurrentTotal.Hours;
                            int CurrenttotalMins = spnCurrentTotal.Minutes;
                            int Currentsecs = spnCurrentTotal.Seconds;
                            #region Day
                            if (Currentdays > 0)
                            {
                                if (CurrentHoleTime.Length > 0)
                                {
                                    CurrentHoleTime = CurrentHoleTime + "," + Currentdays + " D";
                                }
                                else
                                {
                                    CurrentHoleTime = Currentdays + " D";
                                }
                            }
                            #endregion

                            #region Hours
                            if (Currenthours > 0)
                            {
                                if (CurrentHoleTime.Length > 0)
                                {
                                    CurrentHoleTime = CurrentHoleTime + "," + Currenthours + " H";
                                }
                                else
                                {
                                    CurrentHoleTime = Currenthours + " H";
                                }
                            }
                            #endregion

                            #region Mins
                            if (CurrenttotalMins > 0)
                            {
                                if (CurrentHoleTime.Length > 0)
                                {
                                    CurrentHoleTime = CurrentHoleTime + "," + CurrenttotalMins + " M";
                                }
                                else
                                {
                                    CurrentHoleTime = CurrenttotalMins + " M";
                                }
                            }
                            #endregion

                            #region seconds
                            if (Currentsecs > 0)
                            {
                                if (CurrentHoleTime.Length > 0)
                                {
                                    CurrentHoleTime = CurrentHoleTime + "," + Currentsecs + " S";
                                }
                                else
                                {
                                    CurrentHoleTime = Currentsecs + " S";
                                }
                            }
                            #endregion

                            CurrentHoleTimeInMins = Convert.ToInt64(spnCurrentTotal.TotalMinutes);
                        }
                        else
                        {
                            if (lastRecord.Position == "Flag")
                            { 
                                DateTime endCurrentTime = DateTime.UtcNow;

                                Int64 cur_hole = Convert.ToInt64(lastRecord.HoleNumber);
                                var PrevofLastRecords = HoleDetails.FirstOrDefault(x => x.HoleNumber == cur_hole && x.Position == "Tee");

                                if (PrevofLastRecords != null)
                                {
                                    DateTime startCurrentTime = Convert.ToDateTime(PrevofLastRecords.Time);
                                    TimeSpan spnCurrentTotal = Convert.ToDateTime(endCurrentTime) - Convert.ToDateTime(startCurrentTime);
                                    int Currentdays = spnCurrentTotal.Days;
                                    int Currenthours = spnCurrentTotal.Hours;
                                    int CurrenttotalMins = spnCurrentTotal.Minutes;
                                    int Currentsecs = spnCurrentTotal.Seconds;
                                    #region Day
                                    if (Currentdays > 0)
                                    {
                                        if (CurrentHoleTime.Length > 0)
                                        {
                                            CurrentHoleTime = CurrentHoleTime + "," + Currentdays + " D";
                                        }
                                        else
                                        {
                                            CurrentHoleTime = Currentdays + " D";
                                        }
                                    }
                                    #endregion

                                    #region Hours
                                    if (Currenthours > 0)
                                    {
                                        if (CurrentHoleTime.Length > 0)
                                        {
                                            CurrentHoleTime = CurrentHoleTime + "," + Currenthours + " H";
                                        }
                                        else
                                        {
                                            CurrentHoleTime = Currenthours + " H";
                                        }
                                    }
                                    #endregion

                                    #region Mins
                                    if (CurrenttotalMins > 0)
                                    {
                                        if (CurrentHoleTime.Length > 0)
                                        {
                                            CurrentHoleTime = CurrentHoleTime + "," + CurrenttotalMins + " M";
                                        }
                                        else
                                        {
                                            CurrentHoleTime = CurrenttotalMins + " M";
                                        }
                                    }
                                    #endregion

                                    #region seconds
                                    if (Currentsecs > 0)
                                    {
                                        if (CurrentHoleTime.Length > 0)
                                        {
                                            CurrentHoleTime = CurrentHoleTime + "," + Currentsecs + " S";
                                        }
                                        else
                                        {
                                            CurrentHoleTime = Currentsecs + " S";
                                        }
                                    }
                                    #endregion

                                    CurrentHoleTimeInMins = Convert.ToInt64(spnCurrentTotal.TotalMinutes);
                                }
                            }
                        }
                    }
                    else
                    {
                        CurrentHoleTime = "";
                    }
                    #endregion
                }
                else
                {
                    Round = "No round started yet.";
                    TotalTimeSpend = "";
                    var LocationDetails = DbGolfer.GF_GolferChangingLocation.Where(x => x.GolferId == golferid).OrderByDescending(x => x.TimeOfChange).ToList(); // && x.HoleNumber > 0
                    if (LocationDetails.Count > 0)
                    {
                        CurrentHole = Convert.ToString(LocationDetails.Select(x => x.HoleNumber).FirstOrDefault());
                        if (CurrentHole == "0" || CurrentHole == "")
                        {
                            CurrentHole = "N.A.";
                        }

                        // DateTime startTime = LocationDetails.Select(x => x.TimeOfChange).FirstOrDefault() ?? DateTime.Now;
                        DateTime startTime = LocationDetails.Select(x => x.TimeOfChange).LastOrDefault() ?? DateTime.UtcNow;


                        DateTime endTime = DateTime.UtcNow;
                        //try
                        //{
                        //    string tmZone = Convert.ToString(DbGolfer.GF_Golfer.Where(x => x.GF_ID == golferid).FirstOrDefault().strTimeZone);
                        //    if (!string.IsNullOrEmpty(Convert.ToString(tmZone)))
                        //    {
                        //        DateTime tst = DateTime.Now;
                        //        endTime = TimeZoneInfo.ConvertTime(tst, TimeZoneInfo.Local, TimeZoneInfo.FindSystemTimeZoneById(tmZone));
                        //    }
                        //}
                        //catch
                        //{
                        //    endTime = DateTime.Now;
                        //}


                        TimeSpan spnTotal = Convert.ToDateTime(endTime) - Convert.ToDateTime(startTime);
                        int days = spnTotal.Days;
                        int hours = spnTotal.Hours;
                        int totalMins = spnTotal.Minutes;
                        int secs = spnTotal.Seconds;
                        #region Day
                        if (days > 0)
                        {
                            if (TotalTimeSpend.Length > 0)
                            {
                                TotalTimeSpend = TotalTimeSpend + "," + days + " D";
                            }
                            else
                            {
                                TotalTimeSpend = days + " D";
                            }
                        }
                        #endregion

                        #region Hours
                        if (TotalTimeSpend.Length > 0)
                        {
                            TotalTimeSpend = TotalTimeSpend + "," + hours + " H";
                        }
                        else
                        {
                            TotalTimeSpend = hours + " H";
                        }
                        #endregion

                        #region Mins
                        if (TotalTimeSpend.Length > 0)
                        {
                            TotalTimeSpend = TotalTimeSpend + "," + totalMins + " M";
                        }
                        else
                        {
                            TotalTimeSpend = totalMins + " M";
                        }
                        #endregion

                        #region seconds
                        if (secs > 0)
                        {
                            if (TotalTimeSpend.Length > 0)
                            {
                                TotalTimeSpend = TotalTimeSpend + "," + secs + " S";
                            }
                            else
                            {
                                TotalTimeSpend = secs + " S";
                            }
                        }
                        #endregion

                        CurrentHoleTime = "";
                    }
                    else
                    {
                        CurrentHole = "";
                        CurrentHoleTime = "";
                        TotalTimeSpend = "";
                        HoleTimings = new Dictionary<string, string>();
                    }
                }
            }
            catch (Exception ex)
            {
                CurrentHole = "";
                CurrentHoleTime = "";
                TotalTimeSpend = "";
                HoleTimings = new Dictionary<string, string>();
                ErrorLibrary.ErrorClass.WriteLog(ex.InnerException, HttpContext.Current.Request);
            }
        }

        #region Daily Mail Schedular for Co-ordinate settings
        /// <summary>
        /// Send Mail notification for all courses for golf co-ordinates
        /// </summary>
        public static void SendCoordinateReminderMails()
        {
            List<string> msgs = new List<string>();
            msgs.Add("Reminder Mail schedular starts.");
            try
            {
                DbStatic = new GolflerEntities();
                var lstAllCourses = DbStatic.GF_CourseInfo.Where(x => x.Status == StatusType.Active && x.PartnershipStatus == "P").ToList();

                foreach (var course in lstAllCourses)
                {
                    msgs.Add("Reminder Mail schedular for Course: " + course.ID);
                    SendCoordinateReminderMailToCourse(course.ID);
                }
            }
            catch (Exception ex)
            {
                msgs.Add("Exception:" + ex.Message);
            }
            msgs.Add("Reminder Mail schedular ends.");
            LogClass.MassMessageLog(msgs, "ReminderMails.txt");
        }

        /// <summary>
        /// Send Mail notification to particular courses for golf co-ordinates
        /// </summary>
        /// <param name="courseid">Course Id</param> 
        public static void SendCoordinateReminderMailToCourse(Int64 courseid)
        {
            try
            {
                List<string> msgs = new List<string>();
                msgs.Add("Mail schedular starts for Course ID: " + courseid);

                var DbObj = new GolflerEntities();

                List<Int64> checkBuilderIds = DbObj.GF_CourseBuilder.Where(x => x.Status == StatusType.Active && x.CourseID == courseid && x.BuildBy == "CA" && x.CoordinateType == "T").Select(x => x.ID).ToList();

                DateTime dtToday = DateTime.Now;
                DateTime checkForOneDayBefore = dtToday; //.AddDays(1);
                DateTime checkForTwoDayBefore = dtToday.AddDays(1);

                var lstBuilderForOne = DbObj.GF_CourseBuilder.Where(x => x.Status == StatusType.Active && x.CourseID == courseid && x.BuildBy == "CA" && x.CoordinateType == "T" && (x.DateTo.Value.Year == checkForOneDayBefore.Year && x.DateTo.Value.Month == checkForOneDayBefore.Month && x.DateTo.Value.Day == checkForOneDayBefore.Day)).FirstOrDefault();

                var lstAllDates = new List<GF_CourseBuilderRecDates>();

                if (lstBuilderForOne != null)
                {
                    // Check for next day i.e. one day
                    DateTime checkForMailOneDayBefore = dtToday.AddDays(1);
                    var lstAllDatesForOne = DbObj.GF_CourseBuilderRecDates.Where(x => x.Status == StatusType.Active && checkBuilderIds.Contains(x.CourseBuilderId.Value) && x.CourseId == courseid && (x.RecDate.Value.Year == checkForMailOneDayBefore.Year && x.RecDate.Value.Month == checkForMailOneDayBefore.Month && x.RecDate.Value.Day == checkForMailOneDayBefore.Day)).ToList();

                    if (lstAllDatesForOne == null || lstAllDatesForOne.Count <= 0) // means coordinates not available... now send mail
                    {
                        string mailresult = "";
                        #region send mail for reminder one days before
                        try
                        {
                            IQueryable<GF_EmailTemplatesFields> templateFields = null;
                            var param = EmailParams.GetEmailParamsNew(ref DbObj, EmailTemplateName.CoordinateReminder, ref templateFields, courseid, LoginType.CourseAdmin, ref mailresult);

                            if (mailresult == "") // means Parameters are OK
                            {
                                if (ApplicationEmails.CoordinateReminder(ref DbObj, courseid, Convert.ToString(checkForMailOneDayBefore.Date), param, ref templateFields, ref mailresult))
                                {
                                    // Do steps for Mail Send successful
                                    mailresult = "Reminder mail has been successfully sent.";
                                }
                                else
                                {
                                    // Do steps for Mail Failure. Mail failure reason can be find on "mailresult"
                                }
                            }
                            else
                            {
                                // Do steps for Parameters not available.Reason can be find on "mailresult"
                            }
                        }
                        catch (Exception ex)
                        {
                            mailresult = ex.Message;
                        }
                        #endregion
                        msgs.Add("For 24 Hours mail result: " + mailresult);
                    }
                }

                var lstBuilderForTwo = DbObj.GF_CourseBuilder.Where(x => x.Status == StatusType.Active && x.CourseID == courseid && x.BuildBy == "CA" && x.CoordinateType == "T" && (x.DateTo.Value.Year == checkForTwoDayBefore.Year && x.DateTo.Value.Month == checkForTwoDayBefore.Month && x.DateTo.Value.Day == checkForTwoDayBefore.Day)).FirstOrDefault();
                if (lstBuilderForTwo != null)
                {
                    // Check for next two day i.e. two day
                    DateTime checkForMailTwoDayBefore = dtToday.AddDays(2);
                    var lstAllDatesForTwo = DbObj.GF_CourseBuilderRecDates.Where(x => x.Status == StatusType.Active && x.CourseId == courseid && checkBuilderIds.Contains(x.CourseBuilderId.Value) && (x.RecDate.Value.Year == checkForMailTwoDayBefore.Year && x.RecDate.Value.Month == checkForMailTwoDayBefore.Month && x.RecDate.Value.Day == checkForMailTwoDayBefore.Day)).ToList();

                    if (lstAllDatesForTwo == null || lstAllDatesForTwo.Count <= 0) // means coordinates not available... now send mail
                    {
                        string mailresult = "";
                        #region send mail for reminder one days before
                        try
                        {
                            IQueryable<GF_EmailTemplatesFields> templateFields = null;
                            var param = EmailParams.GetEmailParamsNew(ref DbObj, EmailTemplateName.CoordinateReminder, ref templateFields, courseid, LoginType.CourseAdmin, ref mailresult);

                            if (mailresult == "") // means Parameters are OK
                            {
                                if (ApplicationEmails.CoordinateReminder(ref DbObj, courseid, Convert.ToString(checkForMailTwoDayBefore.Date), param, ref templateFields, ref mailresult))
                                {
                                    // Do steps for Mail Send successful
                                    mailresult = "Reminder mail has been successfully sent.";
                                }
                                else
                                {
                                    // Do steps for Mail Failure. Mail failure reason can be find on "mailresult"
                                }
                            }
                            else
                            {
                                // Do steps for Parameters not available.Reason can be find on "mailresult"
                            }
                        }
                        catch (Exception ex)
                        {
                            mailresult = ex.Message;
                        }
                        #endregion
                        msgs.Add("For 48 Hours mail result: " + mailresult);
                    }
                }
                msgs.Add("Mail schedular Ends for Course ID: " + courseid);
                LogClass.MassMessageLog(msgs, "ReminderMails.txt");
            }
            catch (Exception ex)
            {
                List<string> exMsg = new List<string>();
                exMsg.Add("Exception during Send mail for Course: " + Convert.ToString(ex.Message));
                LogClass.MassMessageLog(exMsg, "ReminderMails.txt");
            }
        }

        public static void GetGolferPlayGraph(string golferid, string HistoryFrom, string HistoryTo, ref  List<object[]> data, ref  string msg)
        {
            Int64 intGolferId = 0;
            try
            {
                if (!string.IsNullOrEmpty(Convert.ToString(golferid)))
                {
                    intGolferId = Convert.ToInt64(golferid);
                }
                else
                {
                    intGolferId = 0;
                }
            }
            catch
            {
                intGolferId = 0;
            }

            if (intGolferId > 0)
            {
                DbStatic = new GolflerEntities();
                var lstRoundsEntity = DbStatic.GF_GamePlayRound.Where(x => x.IsQuit == true && x.GolferID == intGolferId);

                #region History dates
                List<Int64> idsRemove = new List<Int64>();
                // var listHistory = list.AsQueryable();
                if (!string.IsNullOrEmpty(HistoryFrom) && !string.IsNullOrEmpty(HistoryTo))
                {
                    string courseTimeZone = CommonFunctions.GetCourseTimeZone(Convert.ToInt64(LoginInfo.CourseId));

                    DateTime dtDate = DateTime.Parse(HistoryFrom);
                    DateTime dtToDate = DateTime.Parse(HistoryTo);

                    foreach (var obj in lstRoundsEntity)
                    {
                        DateTime objDate = CommonFunctions.DateByTimeZone(courseTimeZone, Convert.ToDateTime(obj.CreatedDate));
                        if (!((objDate.Date >= dtDate.Date) && (objDate.Date <= dtToDate.Date)))
                        {
                            idsRemove.Add(obj.ID);
                        }
                    }

                    //lstRoundsEntity = lstRoundsEntity.Where(x => EntityFunctions.TruncateTime(CommonFunctions.DateByTimeZone(courseTimeZone, Convert.ToDateTime(x.CreatedDate))) >= EntityFunctions.TruncateTime(dtDate)
                    //      && EntityFunctions.TruncateTime(CommonFunctions.DateByTimeZone(courseTimeZone, Convert.ToDateTime(x.CreatedDate))) <= EntityFunctions.TruncateTime(dtToDate));

                    //listHistory = listHistory.Where(x => x.CreatedDate.Value.Year >= dtDate.Year
                    //    && x.CreatedDate.Value.Month >= dtDate.Month
                    //    && x.CreatedDate.Value.Day >= dtDate.Day

                    //&& x.CreatedDate.Value.Year <= dtToDate.Year
                    //    && x.CreatedDate.Value.Month <= dtToDate.Month
                    //    && x.CreatedDate.Value.Day <= dtToDate.Day);
                }
                else if (!string.IsNullOrEmpty(HistoryFrom))
                {
                    string courseTimeZone = CommonFunctions.GetCourseTimeZone(Convert.ToInt64(LoginInfo.CourseId));

                    DateTime dtDate = DateTime.Parse(HistoryFrom);

                    foreach (var obj in lstRoundsEntity)
                    {
                        DateTime objDate = CommonFunctions.DateByTimeZone(courseTimeZone, Convert.ToDateTime(obj.CreatedDate));
                        if (!(objDate.Date >= dtDate.Date))
                        {
                            idsRemove.Add(obj.ID);
                        }
                    }

                    //lstRoundsEntity = lstRoundsEntity.Where(x => EntityFunctions.TruncateTime(CommonFunctions.DateByTimeZone(courseTimeZone, Convert.ToDateTime(x.CreatedDate))) >= EntityFunctions.TruncateTime(dtDate));

                    //listHistory = listHistory.Where(x => x.CreatedDate.Value.Year >= dtDate.Year
                    //    && x.CreatedDate.Value.Month >= dtDate.Month
                    //    && x.CreatedDate.Value.Day >= dtDate.Day
                    //    );
                }
                else if (!string.IsNullOrEmpty(HistoryTo))
                {
                    string courseTimeZone = CommonFunctions.GetCourseTimeZone(Convert.ToInt64(LoginInfo.CourseId));

                    DateTime dtToDate = DateTime.Parse(HistoryTo);


                    foreach (var obj in lstRoundsEntity)
                    {
                        DateTime objDate = CommonFunctions.DateByTimeZone(courseTimeZone, Convert.ToDateTime(obj.CreatedDate));
                        if (!(objDate.Date <= dtToDate.Date))
                        {
                            idsRemove.Add(obj.ID);
                        }
                    }


                    //lstRoundsEntity = lstRoundsEntity.Where(x => EntityFunctions.TruncateTime(CommonFunctions.DateByTimeZone(courseTimeZone, Convert.ToDateTime(x.CreatedDate))) <= EntityFunctions.TruncateTime(dtToDate));


                    //listHistory = listHistory.Where(x => x.CreatedDate.Value.Year <= dtToDate.Year
                    //    && x.CreatedDate.Value.Month <= dtToDate.Month
                    //    && x.CreatedDate.Value.Day <= dtToDate.Day
                    //    );
                }
                #endregion

                var lstRounds = new List<GF_GamePlayRound>(); // lstRoundsEntity.ToList();
                if (idsRemove.Count > 0)
                {
                    foreach (var obj in lstRoundsEntity)
                    {
                        if (!(idsRemove.Contains(obj.ID)))
                        {
                            lstRounds.Add(obj); // = listEntity.Where(x => idsRemove.Contains(x.ID)).ToList();
                        }
                    }
                }
                else
                {
                    lstRounds = lstRoundsEntity.ToList();
                }


                if (lstRounds.Count > 0)
                {
                    var RoundByCourse = lstRounds.GroupBy(x => x.CourseID).Select(x => x.First()).ToList();

                    foreach (var objCourse in RoundByCourse)
                    {
                        string strCourseName = DbStatic.GF_CourseInfo.FirstOrDefault(x => x.ID == objCourse.CourseID).COURSE_NAME;
                        Int64 intTotalPlay = lstRounds.Where(x => x.CourseID == objCourse.CourseID).ToList().Count;

                        data.Add(new object[] { strCourseName, intTotalPlay });
                    }
                }
                else
                {
                    msg = "No record found.";
                }
            }
            else
            {
                msg = "Golfer Id not found.";
            }
        }

        #endregion

        public static Dictionary<int, string> GetCoursePar(string courseid)
        {
            Dictionary<int, string> dctResult = new Dictionary<int, string>();
            try
            {
                Int64 cid = Convert.ToInt64(courseid);
                if (cid > 0)
                {
                    DbStatic = new GolflerEntities();
                    var courseDetails = DbStatic.GF_CourseInfo.Where(x => x.ID == cid).FirstOrDefault();
                    if (courseDetails != null)
                    {
                        for (int cnt = 1; cnt <= 18; cnt++)
                        {
                            switch (cnt)
                            {
                                case 1:
                                    dctResult.Add(1, Convert.ToString(courseDetails.Par_1 ?? 0));
                                    break;
                                case 2:
                                    dctResult.Add(2, Convert.ToString(courseDetails.Par_2 ?? 0));
                                    break;
                                case 3:
                                    dctResult.Add(3, Convert.ToString(courseDetails.Par_3 ?? 0));
                                    break;
                                case 4:
                                    dctResult.Add(4, Convert.ToString(courseDetails.Par_4 ?? 0));
                                    break;
                                case 5:
                                    dctResult.Add(5, Convert.ToString(courseDetails.Par_5 ?? 0));
                                    break;
                                case 6:
                                    dctResult.Add(6, Convert.ToString(courseDetails.Par_6 ?? 0));
                                    break;
                                case 7:
                                    dctResult.Add(7, Convert.ToString(courseDetails.Par_7 ?? 0));
                                    break;
                                case 8:
                                    dctResult.Add(8, Convert.ToString(courseDetails.Par_8 ?? 0));
                                    break;
                                case 9:
                                    dctResult.Add(9, Convert.ToString(courseDetails.Par_9 ?? 0));
                                    break;
                                case 10:
                                    dctResult.Add(10, Convert.ToString(courseDetails.Par_10 ?? 0));
                                    break;
                                case 11:
                                    dctResult.Add(11, Convert.ToString(courseDetails.Par_11 ?? 0));
                                    break;
                                case 12:
                                    dctResult.Add(12, Convert.ToString(courseDetails.Par_12 ?? 0));
                                    break;
                                case 13:
                                    dctResult.Add(13, Convert.ToString(courseDetails.Par_13 ?? 0));
                                    break;
                                case 14:
                                    dctResult.Add(14, Convert.ToString(courseDetails.Par_14 ?? 0));
                                    break;
                                case 15:
                                    dctResult.Add(15, Convert.ToString(courseDetails.Par_15 ?? 0));
                                    break;
                                case 16:
                                    dctResult.Add(16, Convert.ToString(courseDetails.Par_16 ?? 0));
                                    break;
                                case 17:
                                    dctResult.Add(17, Convert.ToString(courseDetails.Par_17 ?? 0));
                                    break;
                                case 18:
                                    dctResult.Add(18, Convert.ToString(courseDetails.Par_18 ?? 0));
                                    break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorLibrary.ErrorClass.WriteLog(ex.InnerException, HttpContext.Current.Request);
            }
            return dctResult;
        }

        public static bool IsCourseSettingsComplete(Int64 courseId)
        {
            DbStatic = new GolflerEntities();
            var objCourse = DbStatic.GF_CourseInfo.Where(x => x.ID == courseId).FirstOrDefault();
            if (objCourse != null)
            {
                string SettingsMsg = "";

                ///////// PAR INFORMATION START
                string FirstParFill = "";
                string SecParFill = "";
                string checkAllFirstParFill = "";
                string checkAllSecParFill = "";

                if ((!(string.IsNullOrEmpty(Convert.ToString(objCourse.Par_1).Trim()))) || (!(string.IsNullOrEmpty(Convert.ToString(objCourse.Par_2).Trim()))) || (!(string.IsNullOrEmpty(Convert.ToString(objCourse.Par_3).Trim()))) || (!(string.IsNullOrEmpty(Convert.ToString(objCourse.Par_4).Trim()))) || (!(string.IsNullOrEmpty(Convert.ToString(objCourse.Par_5).Trim()))) || (!(string.IsNullOrEmpty(Convert.ToString(objCourse.Par_6).Trim()))) || (!(string.IsNullOrEmpty(Convert.ToString(objCourse.Par_7).Trim()))) || (!(string.IsNullOrEmpty(Convert.ToString(objCourse.Par_8).Trim()))) || (!(string.IsNullOrEmpty(Convert.ToString(objCourse.Par_9).Trim()))))
                {
                    FirstParFill = "fill";
                }

                if ((!(string.IsNullOrEmpty(Convert.ToString(objCourse.Par_10).Trim()))) || (!(string.IsNullOrEmpty(Convert.ToString(objCourse.Par_11).Trim()))) || (!(string.IsNullOrEmpty(Convert.ToString(objCourse.Par_12).Trim()))) || (!(string.IsNullOrEmpty(Convert.ToString(objCourse.Par_13).Trim()))) || (!(string.IsNullOrEmpty(Convert.ToString(objCourse.Par_14).Trim()))) || (!(string.IsNullOrEmpty(Convert.ToString(objCourse.Par_15).Trim()))) || (!(string.IsNullOrEmpty(Convert.ToString(objCourse.Par_16).Trim()))) || (!(string.IsNullOrEmpty(Convert.ToString(objCourse.Par_17).Trim()))) || (!(string.IsNullOrEmpty(Convert.ToString(objCourse.Par_18).Trim()))))
                {
                    SecParFill = "fill";
                }



                if (FirstParFill == "" && SecParFill == "")
                {
                    SettingsMsg = SettingsMsg + "Please fill Par Information." + "\n";
                }
                else
                {
                    if (FirstParFill == "fill")
                    {
                        if ((string.IsNullOrEmpty(Convert.ToString(objCourse.Par_1))) || (string.IsNullOrEmpty(Convert.ToString(objCourse.Par_2))) || (string.IsNullOrEmpty(Convert.ToString(objCourse.Par_3))) || (string.IsNullOrEmpty(Convert.ToString(objCourse.Par_4))) || (string.IsNullOrEmpty(Convert.ToString(objCourse.Par_5))) || (string.IsNullOrEmpty(Convert.ToString(objCourse.Par_6))) || (string.IsNullOrEmpty(Convert.ToString(objCourse.Par_7))) || (string.IsNullOrEmpty(Convert.ToString(objCourse.Par_8))) || (string.IsNullOrEmpty(Convert.ToString(objCourse.Par_9))))
                        {
                            checkAllFirstParFill = "empty";
                        }

                    }
                    if (SecParFill == "fill")
                    {
                        if ((string.IsNullOrEmpty(Convert.ToString(objCourse.Par_10))) || (string.IsNullOrEmpty(Convert.ToString(objCourse.Par_11))) || (string.IsNullOrEmpty(Convert.ToString(objCourse.Par_12))) || (string.IsNullOrEmpty(Convert.ToString(objCourse.Par_13))) || (string.IsNullOrEmpty(Convert.ToString(objCourse.Par_14))) || (string.IsNullOrEmpty(Convert.ToString(objCourse.Par_15))) || (string.IsNullOrEmpty(Convert.ToString(objCourse.Par_16))) || (string.IsNullOrEmpty(Convert.ToString(objCourse.Par_17))) || (string.IsNullOrEmpty(Convert.ToString(objCourse.Par_18))))
                        {
                            checkAllSecParFill = "empty";
                        }

                    }
                    if (checkAllFirstParFill == "empty")
                    {
                        SettingsMsg = SettingsMsg + "Please fill Par information for all first nine holes." + "\n";
                    }
                    if (checkAllSecParFill == "empty")
                    {
                        SettingsMsg = SettingsMsg + "Please fill Par information for all  last nine holes." + "\n";
                    }
                }


                var checkAllTee = "";
                if ((!(string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_1_Blue).Trim()))) || (!(string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_2_Blue).Trim()))) || (!(string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_3_Blue).Trim()))) || (!(string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_4_Blue).Trim()))) || (!(string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_5_Blue).Trim()))) || (!(string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_6_Blue).Trim()))) || (!(string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_7_Blue).Trim()))) || (!(string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_8_Blue).Trim()))) || (!(string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_9_Blue).Trim()))) || (!(string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_10_Blue).Trim()))) || (!(string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_11_Blue).Trim()))) || (!(string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_12_Blue).Trim()))) || (!(string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_13_Blue).Trim()))) || (!(string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_14_Blue).Trim()))) || (!(string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_15_Blue).Trim()))) || (!(string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_16_Blue).Trim()))) || (!(string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_17_Blue).Trim()))) || (!(string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_18_Blue).Trim()))) || (!(string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_1).Trim()))) || (!(string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_2).Trim()))) || (!(string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_3).Trim()))) || (!(string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_4).Trim()))) || (!(string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_5).Trim()))) || (!(string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_6).Trim()))) || (!(string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_7).Trim()))) || (!(string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_8).Trim()))) || (!(string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_9).Trim()))) || (!(string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_10).Trim()))) || (!(string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_11).Trim()))) || (!(string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_12).Trim()))) || (!(string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_13).Trim()))) || (!(string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_14).Trim()))) || (!(string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_15).Trim()))) || (!(string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_16).Trim()))) || (!(string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_17).Trim()))) || (!(string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_18).Trim()))) || (!(string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_1_Red).Trim()))) || (!(string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_2_Red).Trim()))) || (!(string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_3_Red).Trim()))) || (!(string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_4_Red).Trim()))) || (!(string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_5_Red).Trim()))) || (!(string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_6_Red).Trim()))) || (!(string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_7_Red).Trim()))) || (!(string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_8_Red).Trim()))) || (!(string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_9_Red).Trim()))) || (!(string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_10_Red).Trim()))) || (!(string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_11_Red).Trim()))) || (!(string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_12_Red).Trim()))) || (!(string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_13_Red).Trim()))) || (!(string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_14_Red).Trim()))) || (!(string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_15_Red).Trim()))) || (!(string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_16_Red).Trim()))) || (!(string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_17_Red).Trim()))) || (!(string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_18_Red).Trim()))))
                {
                    checkAllTee = "fill";
                }


                if (checkAllTee == "fill")
                {
                    //////// blue information start
                    var FirstBlueFill = "";
                    var SecBlueFill = "";
                    var checkAllFirstBlueFill = "";
                    var checkAllSecBlueFill = "";

                    if ((!(string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_1_Blue).Trim()))) || (!(string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_2_Blue).Trim()))) || (!(string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_3_Blue).Trim()))) || (!(string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_4_Blue).Trim()))) || (!(string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_5_Blue).Trim()))) || (!(string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_6_Blue).Trim()))) || (!(string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_7_Blue).Trim()))) || (!(string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_8_Blue).Trim()))) || (!(string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_9_Blue).Trim()))))
                    {
                        FirstBlueFill = "fill";
                    }


                    if ((!(string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_10_Blue).Trim()))) || (!(string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_11_Blue).Trim()))) || (!(string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_12_Blue).Trim()))) || (!(string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_13_Blue).Trim()))) || (!(string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_14_Blue).Trim()))) || (!(string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_15_Blue).Trim()))) || (!(string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_16_Blue).Trim()))) || (!(string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_17_Blue).Trim()))) || (!(string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_18_Blue).Trim()))))
                    {
                        SecBlueFill = "fill";
                    }


                    if (FirstBlueFill == "" && SecBlueFill == "")
                    {
                        // SettingsMsg = SettingsMsg + "Please fill Blue Tee Information." + "\n";
                    }
                    else
                    {
                        if (FirstBlueFill == "fill")
                        {
                            if ((string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_1_Blue))) || (string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_2_Blue))) || (string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_3_Blue))) || (string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_4_Blue))) || (string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_5_Blue))) || (string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_6_Blue))) || (string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_7_Blue))) || (string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_8_Blue))) || (string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_9_Blue))))
                            {
                                checkAllFirstBlueFill = "empty";
                            }

                        }
                        if (SecBlueFill == "fill")
                        {
                            if ((string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_10_Blue))) || (string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_11_Blue))) || (string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_12_Blue))) || (string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_13_Blue))) || (string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_14_Blue))) || (string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_15_Blue))) || (string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_16_Blue))) || (string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_17_Blue))) || (string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_18_Blue))))
                            {
                                checkAllSecBlueFill = "empty";
                            }

                        }
                        if (checkAllFirstBlueFill == "empty")
                        {
                            SettingsMsg = SettingsMsg + "Please fill Blue Tee information for all first nine holes." + "\n";
                        }
                        if (checkAllSecBlueFill == "empty")
                        {
                            SettingsMsg = SettingsMsg + "Please fill Blue Tee information for all  last nine holes." + "\n";
                        }
                    }

                    //////// blue information end

                    //////// white information start
                    var FirstwhiteFill = "";
                    var SecwhiteFill = "";
                    var checkAllFirstwhiteFill = "";
                    var checkAllSecwhiteFill = "";

                    if ((!(string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_1).Trim()))) || (!(string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_2).Trim()))) || (!(string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_3).Trim()))) || (!(string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_4).Trim()))) || (!(string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_5).Trim()))) || (!(string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_6).Trim()))) || (!(string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_7).Trim()))) || (!(string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_8).Trim()))) || (!(string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_9).Trim()))))
                    {
                        FirstwhiteFill = "fill";
                    }


                    if ((!(string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_10).Trim()))) || (!(string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_11).Trim()))) || (!(string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_12).Trim()))) || (!(string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_13).Trim()))) || (!(string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_14).Trim()))) || (!(string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_15).Trim()))) || (!(string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_16).Trim()))) || (!(string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_17).Trim()))) || (!(string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_18).Trim()))))
                    {
                        SecwhiteFill = "fill";
                    }


                    if (FirstwhiteFill == "" && SecwhiteFill == "")
                    {
                        //  SettingsMsg = SettingsMsg + "Please fill White Tee Information." + "\n";
                    }
                    else
                    {
                        if (FirstwhiteFill == "fill")
                        {
                            if ((string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_1))) || (string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_2))) || (string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_3))) || (string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_4))) || (string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_5))) || (string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_6))) || (string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_7))) || (string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_8))) || (string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_9))))
                            {
                                checkAllFirstwhiteFill = "empty";
                            }

                        }
                        if (SecwhiteFill == "fill")
                        {
                            if ((string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_10))) || (string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_11))) || (string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_12))) || (string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_13))) || (string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_14))) || (string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_15))) || (string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_16))) || (string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_17))) || (string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_18))))
                            {
                                checkAllSecwhiteFill = "empty";
                            }

                        }
                        if (checkAllFirstwhiteFill == "empty")
                        {
                            SettingsMsg = SettingsMsg + "Please fill White Tee information for all first nine holes." + "\n";
                        }
                        if (checkAllSecwhiteFill == "empty")
                        {
                            SettingsMsg = SettingsMsg + "Please fill White Tee information for all  last nine holes." + "\n";
                        }
                    }
                    //////// white information end

                    //////// red information start
                    var FirstredFill = "";
                    var SecredFill = "";
                    var checkAllFirstredFill = "";
                    var checkAllSecredFill = "";

                    if ((!(string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_1_Red).Trim()))) || (!(string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_2_Red).Trim()))) || (!(string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_3_Red).Trim()))) || (!(string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_4_Red).Trim()))) || (!(string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_5_Red).Trim()))) || (!(string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_6_Red).Trim()))) || (!(string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_7_Red).Trim()))) || (!(string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_8_Red).Trim()))) || (!(string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_9_Red).Trim()))))
                    {
                        FirstredFill = "fill";
                    }


                    if ((!(string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_10_Red).Trim()))) || (!(string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_11_Red).Trim()))) || (!(string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_12_Red).Trim()))) || (!(string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_13_Red).Trim()))) || (!(string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_14_Red).Trim()))) || (!(string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_15_Red).Trim()))) || (!(string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_16_Red).Trim()))) || (!(string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_17_Red).Trim()))) || (!(string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_18_Red).Trim()))))
                    {
                        SecredFill = "fill";
                    }


                    if (FirstredFill == "" && SecredFill == "")
                    {
                        //  SettingsMsg = SettingsMsg + "Please fill Red Tee Information." + "\n";
                    }
                    else
                    {
                        if (FirstredFill == "fill")
                        {
                            if ((string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_1_Red))) || (string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_2_Red))) || (string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_3_Red))) || (string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_4_Red))) || (string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_5_Red))) || (string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_6_Red))) || (string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_7_Red))) || (string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_8_Red))) || (string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_9_Red))))
                            {
                                checkAllFirstredFill = "empty";
                            }

                        }
                        if (SecredFill == "fill")
                        {
                            if ((string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_10_Red))) || (string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_11_Red))) || (string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_12_Red))) || (string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_13_Red))) || (string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_14_Red))) || (string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_15_Red))) || (string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_16_Red))) || (string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_17_Red))) || (string.IsNullOrEmpty(Convert.ToString(objCourse.Hole_18_Red))))
                            {
                                checkAllSecredFill = "empty";
                            }

                        }
                        if (checkAllFirstredFill == "empty")
                        {
                            SettingsMsg = SettingsMsg + "Please fill Red Tee information for all first nine holes." + "\n";
                        }
                        if (checkAllSecredFill == "empty")
                        {
                            SettingsMsg = SettingsMsg + "Please fill Red Tee information for all  last nine holes." + "\n";
                        }
                    }
                    //////// red information end 
                }
                else
                {
                    SettingsMsg = SettingsMsg + "Please fill atleast one Tee information." + "\n";
                }

                if (SettingsMsg == "")
                {
                    return true;
                }
                else
                {

                    return false;

                }

            }
            else
            {
                return false;
            }
        }

        public static bool IsCourseBuilderInitiate(Int64 courseId)
        {
            bool result = false;

            DbStatic = new GolflerEntities();
            var objCourse = DbStatic.GF_CourseBuilder.Where(x => x.CourseID == courseId && x.BuildBy == "CA" && x.Status == StatusType.Active).ToList();
            if (objCourse.Count > 0)
            {
                foreach (var objCourseBuilder in objCourse)
                {
                    var holedetails = DbStatic.GF_CourseBuilderHolesDetail.Where(x => x.CourseBuilderID == objCourseBuilder.ID).ToList();
                    var groupHoleDetails = holedetails.GroupBy(x => x.HoleNumber).Select(x => x.First()).ToList();
                    foreach (var holeDet in groupHoleDetails)
                    {
                        List<string> objTee = new List<string>();
                        objTee.Add(DragItemType.BlueTee);
                        objTee.Add(DragItemType.WhiteTee);
                        objTee.Add(DragItemType.RedTee);

                        var chkwhiteflag = holedetails.Where(x => x.HoleNumber == holeDet.HoleNumber && x.DragItemType == DragItemType.WhiteFlag).ToList();
                        var chkTee = holedetails.Where(x => x.HoleNumber == holeDet.HoleNumber && objTee.Contains(x.DragItemType)).ToList();
                        if ((chkwhiteflag.Count > 0) && chkTee.Count > 0)
                        {
                            result = true;
                        }
                    }
                }
            }
            return result;
        }

        internal static bool Validate(string value, string strRegex, int maxLength = -1, int minLength = -1)
        {
            bool returnValue = true;

            Regex regex = new Regex(strRegex);

            if (maxLength > 0)
            {
                if (value.Length > maxLength)
                    returnValue = false;
            }

            if (minLength > 0)
            {
                if (value.Length < maxLength)
                    returnValue = false;
            }


            if (!regex.Match(value).Success)
                returnValue = false;

            return returnValue;



        }

        public static bool UpdateAdminStatus(string status)
        {
            try
            {
                DbStatic = new GolflerEntities();
                var adminUsers = DbStatic.GF_AdminUsers.FirstOrDefault(x => x.Type == UserType.SuperAdmin);

                if (adminUsers != null)
                {
                    adminUsers.Status = status;
                    DbStatic.SaveChanges();
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        #region Get Start/End Date of the Week/Month/Year

        public static string FirstLastDayOfWeek(int weekNumber, int year)
        {
            var thursdayInWeek01 = Enumerable.Range(1, 7).Select(i => new DateTime(year, 1, i)).First(d => d.DayOfWeek == DayOfWeek.Thursday);
            var thursdayInCorrectWeek = thursdayInWeek01.AddDays((weekNumber - 1) * 7);

            string first = thursdayInCorrectWeek.AddDays(-3).ToString("dd/MM/yy");
            string last = thursdayInCorrectWeek.AddDays(3).ToString("dd/MM/yy");

            return first + " - " + last;
        }

        public DateTime LastDayOfWeek(DateTime dateTime)
        {
            DateTime firstDayOfMonth = new DateTime(dateTime.Year, dateTime.Month, 1);
            return firstDayOfMonth.AddMonths(1).AddDays(-1);
        }

        public DateTime FirstDayOfMonth(DateTime dateTime)
        {
            return new DateTime(dateTime.Year, dateTime.Month, 1);
        }

        public DateTime LastDayOfMonth(DateTime dateTime)
        {
            DateTime firstDayOfMonth = new DateTime(dateTime.Year, dateTime.Month, 1);
            return firstDayOfMonth.AddMonths(1).AddDays(-1);
        }

        public DateTime FirstDayOfYear(DateTime dateTime)
        {
            return new DateTime(dateTime.Year, 1, 1);
        }

        public DateTime LastDayOfYear(DateTime dateTime)
        {
            DateTime firstDayOfMonth = new DateTime(dateTime.Year, 12, 1);
            return firstDayOfMonth.AddMonths(1).AddDays(-1);
        }

        #endregion

        public static string WebHooksLog(string msg)
        {
            string strResult = "";
            string RequestFilePath = "";
            bool RequestLogMode;
            try
            {
                // if (Convert.ToString(System.Web.Configuration.WebConfigurationManager.AppSettings["ERROR_LOG_MODE"]) == "1")
                // {
                RequestLogMode = true;
                // }
                //  else
                //  {
                //  RequestLogMode = false;
                // }
                RequestFilePath = Convert.ToString(System.Web.Configuration.WebConfigurationManager.AppSettings["WebHookFilePath"]);

                if (RequestLogMode)
                {
                    string fileName = string.Empty;
                    fileName = "Error.txt";
                    string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigClass.WebHookFilePath);
                    StreamWriter w;

                    w = File.Exists(filePath) ? File.AppendText(filePath) : File.CreateText(filePath);

                    w.WriteLine("---------------------------------------------------------------------");
                    w.WriteLine(DateTime.Now);
                    w.WriteLine("---------------------------------------------------------------------");

                    w.WriteLine("Message: " + msg);

                    w.WriteLine("---------------------------------------------------------------------");
                    w.Flush();
                    w.Close();
                    strResult = "Log file created successfully";

                }
            }
            catch (Exception exp)
            {
                strResult = "Exception: " + exp.InnerException;
                strResult += "Message: " + exp.Message;
            }
            return strResult;
        }

        public static string GetMonthName(int id)
        {
            string result = "";
            if (id == 1)
            {
                result = "JAN";
            }
            if (id == 2)
            {
                result = "FEB";
            }
            if (id == 3)
            {
                result = "MAR";
            }
            if (id == 4)
            {
                result = "APR";
            }
            if (id == 5)
            {
                result = "MAY";
            }
            if (id == 6)
            {
                result = "JUN";
            }
            if (id == 7)
            {
                result = "JUL";
            }
            if (id == 8)
            {
                result = "AUG";
            }
            if (id == 9)
            {
                result = "SEP";
            }
            if (id == 10)
            {
                result = "OCT";
            }
            if (id == 11)
            {
                result = "NOV";
            }
            if (id == 12)
            {
                result = "DEC";
            }

            return result;
        }

        public static void CourseUserAutoLogout(Int64 courseid)
        {
            DbStatic = new GolflerEntities();
            var endtime = DateTime.UtcNow;

            int checkMinutes = 0;
            try
            {
                checkMinutes = Convert.ToInt32(System.Web.Configuration.WebConfigurationManager.AppSettings["CourseUserAutoLogoutMinutes"]);
            }
            catch
            {
                checkMinutes = 0;
            }

            var lstCourseUsers = DbStatic.GF_AdminUsers.Where(x => x.CourseId == courseid && x.IsOnline == true).ToList();
            foreach (var objCourseUsers in lstCourseUsers)
            {
                try
                {
                    var objUserInfo = DbStatic.GF_UserCurrentPosition.Where(x => x.ReferenceID == objCourseUsers.ID).OrderByDescending(x => x.ModifyDate).FirstOrDefault();
                    if (objUserInfo != null)
                    {
                        //if (lstGolfersLocation.Count > 0)
                        //{
                        var starttime = objUserInfo.ModifyDate;

                        int mins = Convert.ToInt32((Convert.ToDateTime(endtime) - Convert.ToDateTime(starttime)).TotalMinutes);
                        Int64 chekMins = Convert.ToInt64(checkMinutes);
                        //int hours = spnTotal.Hours;

                        if (chekMins > 0)
                        {
                            if (mins >= chekMins)
                            {
                                CourseUserLogout(Convert.ToInt64(objCourseUsers.ID));
                            }
                        }
                        //}
                    }
                    else
                    {
                        CourseUserLogout(Convert.ToInt64(objCourseUsers.ID));
                    }
                }
                catch
                { }
            }
        }

        public static void CourseUserLogout(Int64 userid)
        {
            try
            {
                var _db = new GolflerEntities();
                var objUser = _db.GF_AdminUsers.FirstOrDefault(x => x.ID == userid);
                if (objUser != null)
                {
                    if (objUser.DeviceType.ToLower() == "ios")
                        objUser.GCMID = string.Empty;
                    else
                        objUser.APNID = string.Empty;

                    objUser.IsOnline = false;
                    _db.SaveChanges();
                }
            }
            catch
            {
            }
        }

        public static void GolferAutoLogout(Int64 courseid)
        {
            DbStatic = new GolflerEntities();
            var endtime = DateTime.UtcNow;

            int checkMinutes = 0;
            try
            {
                checkMinutes = Convert.ToInt32(System.Web.Configuration.WebConfigurationManager.AppSettings["GolferAutoLogoutMinutes"]);
            }
            catch
            {
                checkMinutes = 0;
            }

            var lstGolfers = DbStatic.GF_GolferUser.Where(x => x.CourseID == courseid).ToList();
            foreach (var objGolfer in lstGolfers)
            {
                try
                {
                    var lstGolfersLocation = DbStatic.GF_GolferChangingLocation.Where(x => x.GolferId == objGolfer.GolferID).OrderByDescending(x => x.TimeOfChange).FirstOrDefault();
                    if (lstGolfersLocation != null)
                    {
                        //if (lstGolfersLocation.Count > 0)
                        //{
                        var starttime = lstGolfersLocation.TimeOfChange;

                        int mins = Convert.ToInt32((Convert.ToDateTime(endtime) - Convert.ToDateTime(starttime)).TotalMinutes);
                        Int64 chekMins = Convert.ToInt64(checkMinutes);
                        //int hours = spnTotal.Hours;

                        if (chekMins > 0)
                        {
                            if (mins >= chekMins)
                            {
                                GolferLogout(Convert.ToInt64(objGolfer.GolferID));
                            }
                        }
                        //}
                    }
                    else
                    {
                        GolferLogout(Convert.ToInt64(objGolfer.GolferID));
                    }
                }
                catch
                { }
            }
        }

        public static void GolferLogout(Int64 userid)
        {
            try
            {
                var _db = new GolflerEntities();
                var objUser = _db.GF_Golfer.FirstOrDefault(x => x.GF_ID == userid);
                if (objUser != null)
                {
                    if (objUser.DeviceType == "ios")
                        objUser.GCMID = string.Empty;
                    else
                        objUser.APNID = string.Empty;
                }
                objUser.IsOnline = false;
                _db.SaveChanges();

                #region Quit from all Game Round
                var allActiveGameRound = _db.GF_GamePlayRound.Where(y => y.GolferID == userid && y.IsQuit == false).ToList();
                foreach (var gameplay in allActiveGameRound)
                {
                    gameplay.IsQuit = true;
                    _db.SaveChanges();
                }
                #endregion

                #region remove data for changing location
                //Clear GF_GolferChangingLocation table for golfer
                var deleteGolferChangingLocation = _db.GF_GolferChangingLocation.Where(y => y.GolferId == userid);
                foreach (var golferChangingLocation in deleteGolferChangingLocation)
                {
                    _db.GF_GolferChangingLocation.Remove(golferChangingLocation);
                }
                _db.SaveChanges();
                #endregion

                #region delete relation with course
                var lstCourseRelation = _db.GF_GolferUser.Where(x => x.GolferID == userid).ToList();
                foreach (var rel in lstCourseRelation)
                {
                    _db.GF_GolferUser.Remove(rel);
                    _db.SaveChanges();
                }
                #endregion

                #region delete from paceofplay
                var lstpaceofplay = _db.GF_GolferPaceofPlay.Where(x => x.GolferId == userid && x.Status != StatusType.Delete).ToList();

                foreach (var rel in lstpaceofplay)
                {
                    rel.Status = StatusType.Delete;
                    _db.SaveChanges();
                }
                #endregion

                #region delete from paceofplay Temp
                //var lstpaceofplayTemp = _db.GF_GolferPaceofPlay_Temp.Where(x => x.GolferId == userid).ToList();
                //foreach (var rel in lstpaceofplayTemp)
                //{
                //    _db.GF_GolferPaceofPlay_Temp.Remove(rel);
                //}
                //_db.SaveChanges();
                #endregion

            }
            catch
            {

            }
        }

        public static bool LogoutAdminUsers()
        {
            try
            {
                DbStatic = new GolflerEntities();
                bool isLogin = LoginInfo.IsLoginUser;

                if (isLogin)
                {
                    var user = DbStatic.GF_AdminUsers.FirstOrDefault(x => x.ID == LoginInfo.UserId);
                    if (user != null)
                    {
                        user.IsOnline = false;
                        DbStatic.SaveChanges();
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool LogoutGolferUsers()
        {
            try
            {
                DbStatic = new GolflerEntities();
                bool isLogin = LoginInfo.IsGolferLoginUser;

                if (isLogin)
                {
                    var user = DbStatic.GF_Golfer.FirstOrDefault(x => x.GF_ID == LoginInfo.GolferUserId);
                    if (user != null)
                    {
                        user.IsOnline = false;
                        DbStatic.SaveChanges();
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        #region Time Zone
        public static IEnumerable<clsTimeZone> GetTimeZone()
        {
            DbStatic = new GolflerEntities();
            List<clsTimeZone> lstTimeZone = new List<clsTimeZone>();

            var templstTimeZone = DbStatic.GF_Timezone.Select(x => new { ID = x.timezone_id, Name = x.display_name }).OrderBy(x => x.ID).AsQueryable();
            foreach (var items in templstTimeZone)
            {
                clsTimeZone objTimeZone = new clsTimeZone();
                objTimeZone.ID = items.ID;
                objTimeZone.TimeZone_Name = items.Name;
                lstTimeZone.Add(objTimeZone);
            }
            return lstTimeZone;
        }

        public static string GetCourseTimeZone(long courseID)
        {
            GolflerEntities db = new GolflerEntities();

            var webSetting = db.GF_Settings.FirstOrDefault(x => x.Name.ToLower() == (courseID == 0 ? WebSetting.TimeZone : WebSetting.CourseTimeZone.ToLower())
                && x.CourseID == courseID);

            string courseTimeZone = ConfigClass.DefaultTimeZone;

            if (webSetting != null)
            {
                try
                {
                    if (Convert.ToInt64(webSetting.Value) >= 1)
                    {
                        Int64 intTimeZone = Convert.ToInt64(webSetting.Value);
                        courseTimeZone = Convert.ToString(db.GF_Timezone.FirstOrDefault(x => x.timezone_id == intTimeZone).gmt_value_forCalculation);
                    }
                }
                catch
                { }
            }
            return courseTimeZone;
        }

        public static DateTime DateByCourseTimeZone(long courseID, DateTime utcDateTime)
        {
            GolflerEntities db = new GolflerEntities();

            var webSetting = db.GF_Settings.FirstOrDefault(x => x.Name.ToLower() == WebSetting.CourseTimeZone.ToLower() && x.CourseID == courseID);

            string courseTimeZone = ConfigClass.DefaultTimeZone;
            Double timeForCalculation = Convert.ToDouble("0.0");
            try
            {
                timeForCalculation = Convert.ToDouble(ConfigClass.DefaultTimeZone);
            }
            catch
            {

            }

            if (webSetting != null)
            {
                try
                {
                    if (Convert.ToInt64(webSetting.Value) >= 1)
                    {
                        Int64 intTimeZone = Convert.ToInt64(webSetting.Value);
                        //courseTimeZone = Convert.ToString(db.GF_Timezone.FirstOrDefault(x => x.timezone_id == intTimeZone).timezone_standard_identifier);
                        timeForCalculation = Convert.ToDouble(db.GF_Timezone.FirstOrDefault(x => x.timezone_id == intTimeZone).gmt_value_forCalculation);
                    }
                }
                catch
                { }
            }

            //TimeZoneInfo cstZone = TimeZoneInfo.FindSystemTimeZoneById(ConfigClass.DefaultTimeZone);
            //try
            //{
            //      cstZone = TimeZoneInfo.FindSystemTimeZoneById(courseTimeZone);

            //}
            //catch
            //{ 

            //}
            //DateTime tzDateTime = TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, cstZone);

            int hours = (int)timeForCalculation;
            int min = (int)(Math.Round((timeForCalculation - hours), 2) * 100);
            int totalMinutes = (hours * 60) + min;

            DateTime tzDateTime = utcDateTime.AddMinutes(totalMinutes);
            return tzDateTime;
        }

        public static DateTime DateByTimeZone(string courseTimeZone, DateTime utcDateTime)
        {

            //TimeZoneInfo cstZone = TimeZoneInfo.FindSystemTimeZoneById(ConfigClass.DefaultTimeZone);
            //try
            //{
            //    cstZone = TimeZoneInfo.FindSystemTimeZoneById(courseTimeZone);

            //}
            //catch
            //{

            //}
            //DateTime tzDateTime = TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, cstZone);
            Double timeForCalculation = Convert.ToDouble("0.0");

            try
            {
                timeForCalculation = Convert.ToDouble(courseTimeZone);
            }
            catch
            { }

            int hours = (int)timeForCalculation;
            int min = (int)(Math.Round((timeForCalculation - hours), 2) * 100);
            int totalMinutes = (hours * 60) + min;

            //DateTime tzDateTime = utcDateTime.AddHours(timeForCalculation);
            DateTime tzDateTime = utcDateTime.AddMinutes(totalMinutes);
            return tzDateTime;
        }

        public static DateTime DateByGolferTimeZone(long golferID, DateTime utcDateTime)
        {
            GolflerEntities db = new GolflerEntities();

            var golfer = db.GF_Golfer.FirstOrDefault(x => x.GF_ID == golferID);

            Double timeForCalculation = Convert.ToDouble("0.0");
            try
            {
                timeForCalculation = Convert.ToDouble(ConfigClass.DefaultTimeZone);
            }
            catch
            {

            }

            if (golfer != null)
            {
                try
                {
                    if (Convert.ToInt64(golfer.TimeZoneId) >= 1)
                    {
                        Int64 intTimeZone = Convert.ToInt64(golfer.TimeZoneId);
                        timeForCalculation = Convert.ToDouble(db.GF_Timezone.FirstOrDefault(x => x.timezone_id == intTimeZone).gmt_value_forCalculation);
                    }
                }
                catch
                { }
            }

            int hours = (int)timeForCalculation;
            int min = (int)(Math.Round((timeForCalculation - hours), 2) * 100);
            int totalMinutes = (hours * 60) + min;

            //DateTime tzDateTime = utcDateTime.AddHours(timeForCalculation);
            DateTime tzDateTime = utcDateTime.AddMinutes(totalMinutes);
            return tzDateTime;
        }

        public static string GetGolferTimeZone(long golferID)
        {
            GolflerEntities db = new GolflerEntities();

            var golfer = db.GF_Golfer.FirstOrDefault(x => x.GF_ID == golferID);

            Double timeForCalculation = Convert.ToDouble("0.0");
            try
            {
                timeForCalculation = Convert.ToDouble(ConfigClass.DefaultTimeZone);
            }
            catch
            {

            }

            if (golfer != null)
            {
                try
                {
                    if (Convert.ToInt64(golfer.TimeZoneId) >= 1)
                    {
                        Int64 intTimeZone = golfer.TimeZoneId ?? 0;
                        timeForCalculation = Convert.ToDouble(db.GF_Timezone.FirstOrDefault(x => x.timezone_id == intTimeZone).gmt_value_forCalculation);
                    }
                }
                catch
                { }
            }

            return timeForCalculation.ToString();
        }

        #endregion

        public static void GetLatestUserAndEmailForReply(Int64 mainMsgId, ref string toEmail, ref string toUserName)
        {
            DbStatic = new GolflerEntities();
            var objMsgHistory = DbStatic.GF_ResolutionMessageHistory.Where(x => x.MessageID == mainMsgId && x.UserType != UserType.Golfer).OrderByDescending(x => x.CreatedDate).FirstOrDefault();
            if (objMsgHistory != null)
            {
                var objMsgBy = DbStatic.GF_AdminUsers.Where(x => x.ID == objMsgHistory.LogUserID).FirstOrDefault();
                if (objMsgBy != null)
                {
                    toEmail = Convert.ToString(objMsgBy.Email);
                    toUserName = Convert.ToString(objMsgBy.FirstName + " " + objMsgBy.LastName);
                }
            }
        }

        public static GF_AdminUsers GetCourseAdmin()
        {
            DbStatic = new GolflerEntities();
            var objtUser = DbStatic.GF_AdminUsers.Where(x => x.Type == UserType.CourseAdmin && x.CourseId == LoginInfo.CourseId && x.Status == StatusType.Active).FirstOrDefault();
            return objtUser;
        }

        #region Prepared By

        public static string GetPreparedByType(GF_Order order)
        {
            string foodCategoryType = "";

            if (order.OrderType == OrderType.TurnOrder)
            {
                foodCategoryType = string.Join(",", order.GF_OrderDetails
                                                         .Select(y => y.GF_MenuItems.GF_SubCategory.GF_Category.Type)
                                                         .Distinct().ToList());
            }
            else
            {
                foodCategoryType = string.Join(",", order.GF_OrderDetails
                                                         .Select(y => y.GF_MenuItems.GF_SubCategory.GF_Category.CartType)
                                                         .Distinct().ToList());
            }

            if (foodCategoryType.ToLower() == FoodCategoryType.Proshop.ToLower())
            {
                foodCategoryType = FoodCategoryType.Proshop;
            }
            else if (foodCategoryType.ToLower().Contains(FoodCategoryType.Kitchen.ToLower()))
            {
                foodCategoryType = FoodCategoryType.Kitchen;
            }
            else
            {
                foodCategoryType = "Gophie";
            }

            return foodCategoryType;
        }

        public static string GetPreparedBy(GF_Order order)
        {
            string foodCategoryType = "";

            if (order.OrderType == OrderType.TurnOrder)
            {
                foodCategoryType = string.Join(",", order.GF_OrderDetails
                                                         .Select(y => y.GF_MenuItems.GF_SubCategory.GF_Category.Type)
                                                         .Distinct().ToList());
            }
            else
            {
                foodCategoryType = string.Join(",", order.GF_OrderDetails
                                                         .Select(y => y.GF_MenuItems.GF_SubCategory.GF_Category.CartType)
                                                         .Distinct().ToList());
            }

            long userID = 0;
            if (foodCategoryType.ToLower() == FoodCategoryType.Proshop.ToLower())
            {
                userID = order.ProShopID ?? 0;
            }
            else if (foodCategoryType.ToLower().Contains(FoodCategoryType.Kitchen.ToLower()))
            {
                userID = order.KitchenId ?? 0;
            }
            else
            {
                userID = order.CartieId ?? 0;
            }

            DbStatic = new GolflerEntities();
            var lstUser = DbStatic.GF_AdminUsers.FirstOrDefault(x => x.ID == userID);

            if (lstUser != null)
            {
                return lstUser.FirstName + " " + lstUser.LastName;
            }
            else
            {
                return "N/A";
            }
        }

        #endregion

        #region Conversion

        public static double ConvertMilesToKilometers(double miles)
        {
            //
            // Multiply by this constant and return the result.
            //
            return miles * 1.609344;
        }

        public static double ConvertKilometersToMiles(double kilometers)
        {
            //
            // Multiply by this constant.
            //
            return kilometers * 0.621371192;
        }

        public static double ConvertMilesToMeters(double meters)
        {
            //
            // Multiply by this constant.
            //
            return (meters * 1609.344);
        }

        #endregion
    }
}