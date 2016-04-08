using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Web;


namespace GolferWebAPI.Models
{
    public class CommonFunctions
    {
        /// <summary>
        /// Function to get current year
        /// </summary>
        /// <returns></returns>
        public static string GetYear()
        {
            return Convert.ToString(DateTime.Now.Year);
        }

        public static decimal GetRadiusInMeters(Int64 courseId)
        {
            decimal meterSearch = Convert.ToDecimal("0");
            try
            {
                GolflerEntities DbGolfer = new GolflerEntities();
                var courseSettings = DbGolfer.GF_Settings.Where(x => x.CourseID == courseId && x.Name == "Hole Radius" && x.Status == StatusType.Active && x.IsActive == true).FirstOrDefault();
                if (courseSettings != null)
                {
                    if (!(string.IsNullOrEmpty(Convert.ToString(courseSettings.Value))))
                    {
                        meterSearch = Convert.ToDecimal(courseSettings.Value);
                    }
                }
            }
            catch
            {
                meterSearch = Convert.ToDecimal("0");
            }

            try
            {
                if (meterSearch <= 0)
                {
                    meterSearch = Convert.ToDecimal(System.Web.Configuration.WebConfigurationManager.AppSettings["MetersForPaceOfPlay"]);
                }
            }
            catch
            {
                meterSearch = Convert.ToDecimal("0");
            }
            if (meterSearch <= 0)
            {
                meterSearch = 1;
            }
            return meterSearch;
        }

        public static Int64 GetIntervalForCourse(Int64 courseId)
        {
            Int64 timeInterval = Convert.ToInt64("0");
            try
            {
                GolflerEntities DbGolfer = new GolflerEntities();
                var courseSettings = DbGolfer.GF_Settings.Where(x => x.CourseID == courseId && x.Name == "Location Update Time Interval" && x.Status == StatusType.Active && x.IsActive == true).FirstOrDefault();
                if (courseSettings != null)
                {
                    if (!(string.IsNullOrEmpty(Convert.ToString(courseSettings.Value))))
                    {
                        timeInterval = Convert.ToInt64(courseSettings.Value);
                    }
                }
            }
            catch
            {
                timeInterval = Convert.ToInt64("0");
            }

            try
            {
                if (timeInterval <= 0)
                {
                    timeInterval = Convert.ToInt64(System.Web.Configuration.WebConfigurationManager.AppSettings["LocationUpdateTimeInterval"]);
                }
            }
            catch
            {
                timeInterval = Convert.ToInt64("0");
            }
            if (timeInterval < 10)
            {
                timeInterval = 10;
            }
            return timeInterval;
        }

        public static Int64 GetIdleStateHours()
        {
            Int64 idleHours = Convert.ToInt64("0");

            try
            {
                if (idleHours <= 0)
                {
                    idleHours = Convert.ToInt64(System.Web.Configuration.WebConfigurationManager.AppSettings["GolferIdleStateHours"]);
                }
            }
            catch
            {
                idleHours = Convert.ToInt64("0");
            }
            if (idleHours < 2)
            {
                idleHours = 2;
            }
            return idleHours;
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
        public static void GeneratePassword(string p, string userType, ref string _salt, ref string _password)
        {
            if (userType == "new")
            {
                _salt = CommonLibClass.FetchRandStr(3);
            }
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

        /// <summary>
        /// Bulding up the mail content for forgot email here
        /// </summary>
        /// <param name="objUser"></param>
        /// <param name="newpassword"></param>
        /// <returns></returns>
        internal static bool SendAdminMailForgot(GF_Golfer objUser, string newpassword)
        {
            var htValues = new Hashtable();
            htValues.Add("LogoPath", ConfigClass.SiteUrl);
            htValues.Add("SiteName", ConfigClass.SiteName);
            htValues.Add("Fname", objUser.FirstName);
            htValues.Add("Lname", objUser.LastName);
            htValues.Add("UserName", objUser.Email);
            htValues.Add("Password", newpassword);
            return SendMail(objUser.Email, "Golfer - Forgot Password", ConfigClass.EmailTemplatePath, "forgotpassword.htm", htValues);
        }


        /// <summary>
        /// Bulding up the mail content for forgot email here
        /// </summary>
        /// <param name="objUser"></param>
        /// <param name="newpassword"></param>
        /// <returns></returns>
        internal static bool SendMailForgot(GF_Golfer objUser, string newpassword)
        {
            var htValues = new Hashtable();
            htValues.Add("LogoPath", ConfigClass.SiteUrl);
            htValues.Add("SiteName", ConfigClass.SiteName);
            htValues.Add("Fname", objUser.FirstName);
            htValues.Add("Lname", objUser.LastName);
            htValues.Add("UserName", objUser.Email);
            htValues.Add("Password", newpassword);
            return SendMail(objUser.Email, "Golfer - Forgot Password", ConfigClass.EmailTemplatePath, "forgotpassword.htm", htValues);
        }

        internal static bool ChangePassword(GF_Golfer objUser, string link)
        {
            var htValues = new Hashtable();
            htValues.Add("LogoPath", ConfigClass.SiteUrl);
            htValues.Add("SiteName", ConfigClass.SiteName);
            htValues.Add("Fname", objUser.FirstName);
            htValues.Add("Lname", objUser.LastName);
            htValues.Add("link", link);

            return SendMail(objUser.Email, "Recruit Me - Change Password", ConfigClass.EmailTemplatePath, "changepassword.htm", htValues);
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
                // client.Port = 587;
                // ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;
                client.EnableSsl = true;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                var credential = new NetworkCredential();
                credential.UserName = ConfigClass.SMTP_Username;
                credential.Password = ConfigClass.SMTP_Password;
                client.UseDefaultCredentials = false;
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

        #region Pace of play
        public static void GetLatLong(Int64 courseid, Int64 holenumber, string dragitemtype, ref string Courselatitude, ref string Courselongitude)
        {
            try
            {
                GolflerEntities DbGolfer = new GolflerEntities();
                var coursedet = DbGolfer.GF_CourseBuilder.Where(x => x.CourseID == courseid && x.Status == StatusType.Active && x.CoordinateType == "O").FirstOrDefault();
                if (coursedet != null)
                {
                    var HoleDet = DbGolfer.GF_CourseBuilderHolesDetail.Where(x => x.CourseBuilderID == coursedet.ID && x.HoleNumber == holenumber && x.DragItemType == dragitemtype).FirstOrDefault();
                    if (HoleDet != null)
                    {
                        if (!string.IsNullOrEmpty(Convert.ToString(HoleDet.Latitude)))
                        {
                            Courselatitude = Convert.ToString(HoleDet.Latitude);
                        }
                        if (!string.IsNullOrEmpty(Convert.ToString(HoleDet.Longitude)))
                        {
                            Courselongitude = Convert.ToString(HoleDet.Longitude);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Courselatitude = "";
                Courselongitude = "";
            }
        }

        public static double ConvertMilesToMeters(double miles)
        {
            //
            // Multiply by this constant.
            //
            return (miles * 1609.344);
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
        public static void GetGolferHoleInformation_old(Int64 courseid, Int64 golferid, ref string CurrentHole, ref string CurrentHoleTime, ref string TotalTimeSpend, ref Dictionary<string, string> HoleTimings)
        {
            try
            {
                GolflerEntities DbGolfer = new GolflerEntities();
                var HoleDetails = DbGolfer.GF_GolferPaceofPlay.Where(x => x.CourseId == courseid && x.Status == StatusType.Active && x.GolferId == golferid).OrderBy(x => x.HoleNumber).ToList();
                if (HoleDetails.Count > 0)
                {
                    var startTime = HoleDetails.FirstOrDefault().Time;
                    // var endTime = HoleDetails.LastOrDefault().Time;

                    //if (startTime  != endTime)
                    //{
                    //    // TotalTimeSpend = Convert.ToString(endTime - startTime);
                    //    TimeSpan spnTotal = Convert.ToDateTime(endTime) - Convert.ToDateTime(startTime);
                    //    int days = spnTotal.Days;
                    //    int hours = spnTotal.Hours;
                    //    int totalMins = spnTotal.Minutes;
                    //    int secs = spnTotal.Seconds;
                    //    #region Day
                    //    if (days > 0)
                    //    {
                    //        if (TotalTimeSpend.Length > 0)
                    //        {
                    //            TotalTimeSpend = TotalTimeSpend + "," + days + " Day(s)";
                    //        }
                    //        else
                    //        {
                    //            TotalTimeSpend = days + " Day(s)";
                    //        }
                    //    }
                    //    #endregion

                    //    #region Hours
                    //    if (TotalTimeSpend.Length > 0)
                    //    {
                    //        TotalTimeSpend = TotalTimeSpend + "," + hours + " Hour(s)";
                    //    }
                    //    else
                    //    {
                    //        TotalTimeSpend = hours + " Hour(s)";
                    //    }
                    //    #endregion

                    //    #region Mins
                    //    if (TotalTimeSpend.Length > 0)
                    //    {
                    //        TotalTimeSpend = TotalTimeSpend + "," + totalMins + " Minute(s)";
                    //    }
                    //    else
                    //    {
                    //        TotalTimeSpend = totalMins + " Minute(s)";
                    //    }
                    //    #endregion

                    //    #region seconds
                    //    if (secs > 0)
                    //    {
                    //        if (TotalTimeSpend.Length > 0)
                    //        {
                    //            TotalTimeSpend = TotalTimeSpend + "," + secs + " Second(s)";
                    //        }
                    //        else
                    //        {
                    //            TotalTimeSpend = secs + " Second(s)";
                    //        }
                    //    }
                    //    #endregion
                    //}
                    //else
                    //{
                    var endTime = DateTime.Now;
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
                            TotalTimeSpend = TotalTimeSpend + "," + days + " Day(s)";
                        }
                        else
                        {
                            TotalTimeSpend = days + " Day(s)";
                        }
                    }
                    #endregion

                    #region Hours
                    if (TotalTimeSpend.Length > 0)
                    {
                        TotalTimeSpend = TotalTimeSpend + "," + hours + " Hour(s)";
                    }
                    else
                    {
                        TotalTimeSpend = hours + " Hour(s)";
                    }
                    #endregion

                    #region Mins
                    if (TotalTimeSpend.Length > 0)
                    {
                        TotalTimeSpend = TotalTimeSpend + "," + totalMins + " Minute(s)";
                    }
                    else
                    {
                        TotalTimeSpend = totalMins + " Minute(s)";
                    }
                    #endregion

                    #region seconds
                    if (secs > 0)
                    {
                        if (TotalTimeSpend.Length > 0)
                        {
                            TotalTimeSpend = TotalTimeSpend + "," + secs + " Second(s)";
                        }
                        else
                        {
                            TotalTimeSpend = secs + " Second(s)";
                        }
                    }
                    #endregion
                    //}

                    CurrentHole = Convert.ToString(HoleDetails.LastOrDefault().HoleNumber);

                    for (int cnt = 1; cnt <= Convert.ToInt16(CurrentHole); cnt++)
                    {
                        DateTime HoleStartTime = Convert.ToDateTime("01/10/1111");
                        DateTime NextHoleStartTime = Convert.ToDateTime("01/10/1111");
                        DateTime HoleEndTime = Convert.ToDateTime("01/11/1111");
                        int NextHoleNumber = cnt + 1;

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
                                    timeSpendInHole = Convert.ToString(intMinutesSpendInHole) + " Minute(s)";
                                }
                            }
                        }

                        #region Current Hole Time
                        if (Convert.ToInt16(CurrentHole) == cnt)
                        {
                            //if (timeSpendInHole != "N/A")
                            //{
                            //    CurrentHoleTime = timeSpendInHole;
                            //}
                            //else
                            //{
                            if (varHoleStartTime != null)
                            {
                                if (!string.IsNullOrEmpty(Convert.ToString(HoleStartTime)))
                                {
                                    if (HoleStartTime != Convert.ToDateTime("01/10/1111"))
                                    {
                                        try
                                        {
                                            var currentDate = DateTime.Now;
                                            if (Convert.ToInt16(CurrentHole) == 18)
                                            {
                                                if (varHoleEndTime != null)
                                                {
                                                    if (!string.IsNullOrEmpty(Convert.ToString(HoleEndTime)))
                                                    {
                                                        if (HoleEndTime != Convert.ToDateTime("01/10/1111"))
                                                        {
                                                            currentDate = Convert.ToDateTime(HoleEndTime);
                                                        }
                                                    }
                                                }
                                            }

                                            // var vartimespan = currentDate - HoleStartTime;
                                            // CurrentHoleTime = Convert.ToString(vartimespan.Value.TotalMinutes);
                                            TimeSpan spanCurrentHoleTime = Convert.ToDateTime(currentDate) - Convert.ToDateTime(HoleStartTime);
                                            Int64 intMinutesCurrentHoleTime = Convert.ToInt64(spanCurrentHoleTime.TotalMinutes);
                                            CurrentHoleTime = Convert.ToString(intMinutesCurrentHoleTime) + " Minute(s)";
                                        }
                                        catch
                                        {
                                            CurrentHoleTime = "";
                                        }
                                    }
                                }
                            }
                            //}
                        }
                        #endregion

                        HoleTimings.Add("Hole " + cnt, timeSpendInHole);
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

        public static void GetGolferHoleInformation(Int64 courseid, Int64 golferid, ref string CurrentHole, ref string CurrentHoleTime, ref string TotalTimeSpend, ref Dictionary<string, string> HoleTimings, ref string Round, ref string AverageTime)
        {
            try
            {
                //DateTime _localTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(serverTime, TimeZoneInfo.Local.Id, "Mountain Standard Time");

                //try
                //{
                //   DateTime dtEnd= DateTime.UtcNow;

                //   DateTime tst = DateTime.Now.AddHours(-4);
                //   DateTime star = Convert.ToDateTime(tst).ToUniversalTime();

                //   TimeSpan testspnTotal = Convert.ToDateTime(dtEnd) - Convert.ToDateTime(star);

                //    //DateTime tst = DateTime.Now;
                //    //DateTime uslasttest = TimeZoneInfo.ConvertTime(tst, TimeZoneInfo.Local, TimeZoneInfo.FindSystemTimeZoneById("Mountain Standard Time"));

                //    //DateTime indialasttest = TimeZoneInfo.ConvertTime(tst, TimeZoneInfo.Local, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));

                //    //DateTime hwlasttest = TimeZoneInfo.ConvertTime(tst, TimeZoneInfo.Local, TimeZoneInfo.FindSystemTimeZoneById("Hawaiian Standard Time"));

                //    //DateTime newtst = tst.ToUniversalTime().AddHours(+5.30);


                //    //DateTime hwTime = new DateTime(2015, 06, 23, 13, 49, 15);
                //    //TimeZoneInfo hwZone = TimeZoneInfo.FindSystemTimeZoneById("Indian Standard Time");   // Hawaiian Standard Time    DateTime.Now; // 

                //    //string first = Convert.ToString(hwTime);
                //    //string sec = hwZone.IsDaylightSavingTime(hwTime) ? hwZone.DaylightName : hwZone.StandardName;
                //    //DateTime third = Convert.ToDateTime(TimeZoneInfo.ConvertTime(hwTime, hwZone, TimeZoneInfo.Local));
                //}
                //catch
                //{

                //}

                GolflerEntities DbGolfer = new GolflerEntities();
                //courseid = 17674;
                //golferid = 1;

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
                    // var endTime = HoleDetails.LastOrDefault().Time;

                    //if (startTime  != endTime)
                    //{
                    //    // TotalTimeSpend = Convert.ToString(endTime - startTime);
                    //    TimeSpan spnTotal = Convert.ToDateTime(endTime) - Convert.ToDateTime(startTime);
                    //    int days = spnTotal.Days;
                    //    int hours = spnTotal.Hours;
                    //    int totalMins = spnTotal.Minutes;
                    //    int secs = spnTotal.Seconds;
                    //    #region Day
                    //    if (days > 0)
                    //    {
                    //        if (TotalTimeSpend.Length > 0)
                    //        {
                    //            TotalTimeSpend = TotalTimeSpend + "," + days + " Day(s)";
                    //        }
                    //        else
                    //        {
                    //            TotalTimeSpend = days + " Day(s)";
                    //        }
                    //    }
                    //    #endregion

                    //    #region Hours
                    //    if (TotalTimeSpend.Length > 0)
                    //    {
                    //        TotalTimeSpend = TotalTimeSpend + "," + hours + " Hour(s)";
                    //    }
                    //    else
                    //    {
                    //        TotalTimeSpend = hours + " Hour(s)";
                    //    }
                    //    #endregion

                    //    #region Mins
                    //    if (TotalTimeSpend.Length > 0)
                    //    {
                    //        TotalTimeSpend = TotalTimeSpend + "," + totalMins + " Minute(s)";
                    //    }
                    //    else
                    //    {
                    //        TotalTimeSpend = totalMins + " Minute(s)";
                    //    }
                    //    #endregion

                    //    #region seconds
                    //    if (secs > 0)
                    //    {
                    //        if (TotalTimeSpend.Length > 0)
                    //        {
                    //            TotalTimeSpend = TotalTimeSpend + "," + secs + " Second(s)";
                    //        }
                    //        else
                    //        {
                    //            TotalTimeSpend = secs + " Second(s)";
                    //        }
                    //    }
                    //    #endregion
                    //}
                    //else
                    //{


                    var endTime = DateTime.UtcNow;
                    //try
                    //{
                    //    Int64 grid = Convert.ToInt64(HoleDetails.FirstOrDefault().GameRoundID);
                    //    if (grid > 0)
                    //    {
                    //        string tzone = Convert.ToString(DbGolfer.GF_GamePlayRound.Where(x => x.ID == grid).FirstOrDefault().strTimeZone);
                    //        if (!string.IsNullOrEmpty(Convert.ToString(tzone)))
                    //        {
                    //            DateTime tst = DateTime.Now;
                    //            endTime = TimeZoneInfo.ConvertTime(tst, TimeZoneInfo.Local, TimeZoneInfo.FindSystemTimeZoneById(tzone));
                    //        }
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

                        //#region Current Hole Time
                        //if (!string.IsNullOrEmpty(CurrentHole))
                        //{
                        //    CurrentHole = "0";
                        //}
                        //if (Convert.ToInt16(CurrentHole) == cnt)
                        //{
                        //    //if (timeSpendInHole != "N/A")
                        //    //{
                        //    //    CurrentHoleTime = timeSpendInHole;
                        //    //}
                        //    //else
                        //    //{
                        //    if (varHoleStartTime != null)
                        //    {
                        //        if (!string.IsNullOrEmpty(Convert.ToString(HoleStartTime)))
                        //        {
                        //            if (HoleStartTime != Convert.ToDateTime("01/10/1111"))
                        //            {
                        //                try
                        //                {
                        //                    var currentDate = DateTime.Now;
                        //                    if (Convert.ToInt16(CurrentHole) == 18)
                        //                    {
                        //                        if (varHoleEndTime != null)
                        //                        {
                        //                            if (!string.IsNullOrEmpty(Convert.ToString(HoleEndTime)))
                        //                            {
                        //                                if (HoleEndTime != Convert.ToDateTime("01/10/1111"))
                        //                                {
                        //                                    currentDate = Convert.ToDateTime(HoleEndTime);
                        //                                }
                        //                            }
                        //                        }
                        //                    }

                        //                    // var vartimespan = currentDate - HoleStartTime;
                        //                    // CurrentHoleTime = Convert.ToString(vartimespan.Value.TotalMinutes);
                        //                    TimeSpan spanCurrentHoleTime = Convert.ToDateTime(currentDate) - Convert.ToDateTime(HoleStartTime);
                        //                    Int64 intMinutesCurrentHoleTime = Convert.ToInt64(spanCurrentHoleTime.TotalMinutes);
                        //                    CurrentHoleTime = Convert.ToString(intMinutesCurrentHoleTime) + " M";
                        //                }
                        //                catch
                        //                {
                        //                    CurrentHoleTime = "";
                        //                }
                        //            }
                        //        }
                        //    }
                        //    //}
                        //}
                        //#endregion

                        //  HoleTimings.Add("Hole " + cnt, timeSpendInHole);

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
                        DateTime startTime = LocationDetails.Select(x => x.TimeOfChange).LastOrDefault() ?? DateTime.Now;


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

        #endregion

        public static string GetLatestUserName(long id, string mainSender)
        {
            string usernam = "";
            GolflerEntities DbStatic = new GolflerEntities();
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
            GolflerEntities DbStatic = new GolflerEntities();
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
            GolflerEntities DbStatic = new GolflerEntities();
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
        public static string GetTeeValue(int tee)
        {
            if (tee == 1)
            {
                return "1";
            }
            else if (tee == 2)
            {
                return "2";
            }
            else if (tee == 3)
            {
                return "3";
            }
            else
            {
                return "2";
            }
        }

        public static void GolferAutoLogout(Int64 courseid)
        {
            var DbStatic = new GolflerEntities();
            var endtime = DateTime.UtcNow;

            int checkHours = 0;
            try
            {
                checkHours = Convert.ToInt32(System.Web.Configuration.WebConfigurationManager.AppSettings["checkLogoutHours"]);
            }
            catch
            {
                checkHours = 0;
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

                        int hours = Convert.ToInt32((Convert.ToDateTime(endtime) - Convert.ToDateTime(starttime)).TotalHours);
                        //int hours = spnTotal.Hours;

                        if (checkHours > 0)
                        {
                            if (hours > checkHours)
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
        public static bool LogoutOtherUsers(string usertype, Int64 userid, string devicetype, string deviceuniqueid, string DeviceSerialNo)
        {
            try
            {
                var db = new GolflerEntities();
                try
                {
                    #region Check Course Users
                    var lstUsers = new List<GF_AdminUsers>();
                    if (Convert.ToString(devicetype).ToLower() == "ios")  // apple users
                    {
                        lstUsers = db.GF_AdminUsers.Where(x => x.DeviceType.ToLower().Equals(devicetype.ToLower()) &&
                            x.DeviceSerialNo == DeviceSerialNo &&
                            x.Status == StatusType.Active &&
                            x.IsOnline == true).ToList();

                    }
                    else  // android users
                    {
                        lstUsers = db.GF_AdminUsers.Where(x => x.DeviceType.ToLower().Equals(devicetype.ToLower()) &&
                            x.DeviceSerialNo == DeviceSerialNo &&
                            x.Status == StatusType.Active &&
                            x.IsOnline == true).ToList();

                    }
                    if (usertype == "course")
                    {
                        lstUsers = lstUsers.Where(x => x.ID != userid).ToList();
                    }
                    foreach (var user in lstUsers)
                    {
                        //var objUser = db.GF_AdminUsers.FirstOrDefault(x => x.ID == userid);
                        //if (objUser != null)
                        //{
                        //if (objUser.DeviceType.ToLower() == "ios")
                        user.GCMID = string.Empty;
                        //else
                        user.APNID = string.Empty;
                        user.DeviceSerialNo = string.Empty;
                        user.IsOnline = false;
                        db.SaveChanges();
                        //}
                    }
                    #endregion
                }
                catch
                {

                }
                try
                {
                    #region Check Golfer Users
                    var lstUsers = new List<GF_Golfer>();
                    if (Convert.ToString(devicetype).ToLower() == "ios")  // apple users
                    {
                        lstUsers = db.GF_Golfer.Where(x => x.DeviceType.ToLower().Equals(devicetype.ToLower()) && x.DeviceSerialNo == DeviceSerialNo && x.Status == StatusType.Active && x.IsOnline == true).ToList();

                    }
                    else  // android users
                    {
                        lstUsers = db.GF_Golfer.Where(x => x.DeviceType.ToLower().Equals(devicetype.ToLower()) && x.DeviceSerialNo == DeviceSerialNo && x.Status == StatusType.Active && x.IsOnline == true).ToList();

                    }
                    if (usertype == "golfer")
                    {
                        lstUsers = lstUsers.Where(x => x.GF_ID != userid).ToList();
                    }
                    foreach (var user in lstUsers)
                    {
                        GolferLogout(user.GF_ID);
                    }
                    #endregion
                }
                catch
                {

                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void GolferLogout(Int64 userid)
        {
            try
            {
                var _db = new GolflerEntities();
                var objUser = _db.GF_Golfer.FirstOrDefault(x => x.GF_ID == userid);
                //if (objUser != null)
                //{
                //    if (objUser.DeviceType == "ios")
                objUser.GCMID = string.Empty;
                objUser.DeviceSerialNo = string.Empty;
                //    else
                objUser.APNID = string.Empty;
                //}
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
                //var deleteGolferChangingLocation = _db.GF_GolferChangingLocation.Where(y => y.GolferId == userid);
                //foreach (var golferChangingLocation in deleteGolferChangingLocation)
                //{
                //    _db.GF_GolferChangingLocation.Remove(golferChangingLocation);
                //}
                //_db.SaveChanges();
                _db.GF_SP_deleteGolferChangingLocation_unused(Convert.ToInt64(userid));
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

        public static void GolferLogoutExceptCurrentCourse(Int64 userid, Int64 courseid, ref GolflerEntities _db)
        {
            try
            {
               // var _db = new GolflerEntities();
                var objUser = _db.GF_Golfer.FirstOrDefault(x => x.GF_ID == userid);


                #region Quit from all Game Round
                var allActiveGameRound = _db.GF_GamePlayRound.Where(y => y.GolferID == userid && y.IsQuit == false && y.CourseID != courseid).ToList();
                foreach (var gameplay in allActiveGameRound)
                {
                    gameplay.IsQuit = true;
                    _db.SaveChanges();
                }
                #endregion

                #region delete relation with course
                var lstCourseRelation = _db.GF_GolferUser.Where(x => x.GolferID == userid && x.CourseID != courseid).ToList();
                foreach (var rel in lstCourseRelation)
                {
                    _db.GF_GolferUser.Remove(rel);
                    _db.SaveChanges();
                }
                #endregion

                #region delete from paceofplay
                var lstpaceofplay = _db.GF_GolferPaceofPlay.Where(x => x.GolferId == userid && x.Status != StatusType.Delete && x.CourseId != courseid).ToList();

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


        public static long[] ConvertStringArrayToLongArray(string str)
        {
            return str.Split(",".ToCharArray()).Select(x => long.Parse(x.ToString())).ToArray();
        }

        #region Course Time Zone

        /// <summary>
        /// Created By: Amit Kumar
        /// Created Date: 10 July 2015
        /// Discription: This function is used to get the time zone id which is
        ///              selected by course in web settings.
        /// </summary>
        /// <param name="courseID"></param>
        /// <returns></returns>
        public static string CourseTimeZone(long courseID)
        {
            GolflerEntities db = new GolflerEntities();

            var webSetting = db.GF_Settings.FirstOrDefault(x => x.Name.ToLower() == WebSetting.CourseTimeZone.ToLower() &&
                x.CourseID == courseID);

            string courseTimeZone = ConfigClass.DefaultTimeZone;

            if (webSetting != null)
            {
                // courseTimeZone = webSetting.Value;
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

        /// <summary>
        /// Created By: Amit Kumar
        /// Created Date: 10 July 2015
        /// Discription: This function is used to get the date time according to time zone id.
        /// </summary>
        /// <param name="courseTimeZone"></param>
        /// <param name="utcDateTime"></param>
        /// <returns></returns>
        public static DateTime CourseTimeZoneDateTime(string courseTimeZone, DateTime utcDateTime)
        {
            //TimeZoneInfo cstZone = TimeZoneInfo.FindSystemTimeZoneById(courseTimeZone);

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

        /// <summary>
        /// Created By: Amit Kumar
        /// Created Date: 10 July 2015
        /// Discription: This function is used to get the date time according to time zone id and course id.
        /// </summary>
        /// <param name="courseID"></param>
        /// <param name="utcDateTime"></param>
        /// <returns></returns>
        public static DateTime TimeZoneDateTimeByCourseID(long courseID, DateTime utcDateTime)
        {
            #region OLD
            //GolflerEntities db = new GolflerEntities();

            //var webSetting = db.GF_Settings.FirstOrDefault(x => x.Name.ToLower() == WebSetting.CourseTimeZone.ToLower() &&
            //    x.CourseID == courseID);

            //string courseTimeZone = ConfigClass.DefaultTimeZone;

            //if (webSetting != null)
            //{
            //    courseTimeZone = webSetting.Value;
            //}

            //TimeZoneInfo cstZone = TimeZoneInfo.FindSystemTimeZoneById(courseTimeZone);
            //DateTime tzDateTime = TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, cstZone);

            //return tzDateTime;
            #endregion

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
                        // courseTimeZone = Convert.ToString(db.GF_Timezone.FirstOrDefault(x => x.timezone_id == intTimeZone).timezone_standard_identifier);
                        timeForCalculation = Convert.ToDouble(db.GF_Timezone.FirstOrDefault(x => x.timezone_id == intTimeZone).gmt_value_forCalculation);
                    }
                }
                catch
                { }
            }


            //TimeZoneInfo cstZone = TimeZoneInfo.FindSystemTimeZoneById(courseTimeZone);
            //TimeZoneInfo cstZone = TimeZoneInfo.FindSystemTimeZoneById(ConfigClass.DefaultTimeZone);
            //try
            //{
            //    cstZone = TimeZoneInfo.FindSystemTimeZoneById(courseTimeZone);

            //}
            //catch
            //{

            //}
            //DateTime tzDateTime = TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, cstZone);

            int hours = (int)timeForCalculation;
            int min = (int)(Math.Round((timeForCalculation - hours), 2) * 100);
            int totalMinutes = (hours * 60) + min;

            //DateTime tzDateTime = utcDateTime.AddHours(timeForCalculation);
            DateTime tzDateTime = utcDateTime.AddMinutes(totalMinutes);
            return tzDateTime;
        }

        /// <summary>
        /// Created By: Amit Kumar
        /// Created Date: 10 July 2015
        /// Discription: This function is used to get the time zone id which is selected by golfer.
        /// </summary>
        /// <param name="courseID"></param>
        /// <returns></returns>
        public static string GolferTimeZone(long golferID)
        {
            GolflerEntities db = new GolflerEntities();

            var TimeZone = db.GF_Golfer.FirstOrDefault(x => x.GF_ID == golferID);

            string golferTimeZone = ConfigClass.GolferDefaultTimeZone;

            if (TimeZone != null)
            {
                golferTimeZone = TimeZone.strTimeZone;
            }

            return golferTimeZone;
        }

        /// <summary>
        /// Created By: Amit Kumar
        /// Created Date: 10 July 2015
        /// Discription: This function is used to get the date time according to time zone id.
        /// </summary>
        /// <param name="courseTimeZone"></param>
        /// <param name="utcDateTime"></param>
        /// <returns></returns>
        public static DateTime GolferTimeZoneDateTime(string golferTimeZone, DateTime utcDateTime)
        {
            try
            {
                TimeZoneInfo cstZone = TimeZoneInfo.FindSystemTimeZoneById(golferTimeZone);
                DateTime tzDateTime = TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, cstZone);
                return tzDateTime;
            }
            catch
            {
                TimeZoneInfo cstZone = TimeZoneInfo.FindSystemTimeZoneById(ConfigClass.GolferDefaultTimeZone);
                return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, cstZone);
            }
        }

        /// <summary>
        /// Created By: Amit Kumar
        /// Created Date: 10 July 2015
        /// Discription: This function is used to get the date time according to time zone id which is set by golfer.
        /// </summary>
        /// <param name="courseID"></param>
        /// <param name="utcDateTime"></param>
        /// <returns></returns>
        public static DateTime TimeZoneDateTimeByGolferID(long golferID, DateTime utcDateTime)
        {
            GolflerEntities db = new GolflerEntities();

            var TimeZone = db.GF_Golfer.FirstOrDefault(x => x.GF_ID == golferID);

            string golferTimeZone = ConfigClass.GolferDefaultTimeZone;

            if (TimeZone != null)
            {
                golferTimeZone = TimeZone.strTimeZone;
            }

            TimeZoneInfo cstZone = TimeZoneInfo.FindSystemTimeZoneById(golferTimeZone);
            DateTime tzDateTime = TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, cstZone);

            return tzDateTime;
        }

        #endregion Course Time Zone
    }
}