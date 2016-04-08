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
using CourseWebApi.Models;
using GolferWebAPI.Models;
using System.Text.RegularExpressions;
using System.Device.Location;


namespace CourseWebAPI.Models
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
                        lstUsers = db.GF_AdminUsers.Where(x => x.DeviceType.ToLower().Equals(devicetype.ToLower()) && x.DeviceSerialNo == DeviceSerialNo && x.Status == StatusType.Active && x.IsOnline == true).ToList();

                    }
                    else  // android users
                    {
                        lstUsers = db.GF_AdminUsers.Where(x => x.DeviceType.ToLower().Equals(devicetype.ToLower()) && x.DeviceSerialNo == DeviceSerialNo && x.Status == StatusType.Active && x.IsOnline == true).ToList();

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
                        //    if (objUser.DeviceType.ToLower() == "ios")
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
                //else
                objUser.APNID = string.Empty;
                //}
                objUser.DeviceSerialNo = string.Empty;
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
        internal static bool SendAdminMailForgot(GF_AdminUsers objUser, string newpassword)
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
        internal static bool SendMailForgot(GF_AdminUsers objUser, string newpassword)
        {
            var htValues = new Hashtable();
            htValues.Add("LogoPath", ConfigClass.SiteUrl);
            htValues.Add("SiteName", ConfigClass.SiteName);
            htValues.Add("Fname", objUser.FirstName);
            htValues.Add("Lname", objUser.LastName);
            htValues.Add("UserName", objUser.Email);
            htValues.Add("Password", newpassword);
            return SendMail(objUser.Email, "Course - Forgot Password", ConfigClass.EmailTemplatePath, "forgotpassword.htm", htValues);
        }

        internal static bool ChangePassword(GF_AdminUsers objUser, string link)
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
                //  client.EnableSsl = true;
                //   client.DeliveryMethod = SmtpDeliveryMethod.Network;
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
                // ErrorLibrary.ErrorClass.WriteLog(ex.InnerException, HttpContext.Current.Request);
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

        public static long[] ConvertStringArrayToLongArray(string str)
        {
            return str.Split(",".ToCharArray()).Select(x => long.Parse(x.ToString())).ToArray();
        }

        public static long GetUserCurrentHoleNo(long courseID, string lat, string lng)
        {
            try
            {
                GolflerEntities db = new GolflerEntities();

                #region Course Holes Detail

                var courseBuilderID = db.GF_CourseBuilder.FirstOrDefault(x => x.CourseID == courseID && x.CoordinateType == "O");

                var courseRec = db.GF_CourseBuilderRecDates.FirstOrDefault(x => x.RecDate == DateTime.Now && x.Status == StatusType.Active);

                var lstCourseHole = db.GF_CourseBuilderHolesDetail.ToList().Where(x => x.CourseBuilderID == (courseRec != null ? courseRec.CourseBuilderId : (courseBuilderID != null ? courseBuilderID.ID : 0)))
                                    .Select(x => new
                                    {
                                        x.HoleNumber,
                                        x.Latitude,
                                        x.Longitude
                                    });

                #endregion

                if (lstCourseHole.Count() > 0)
                {
                    var metersForPaceOfPlay = db.GF_Settings.FirstOrDefault(x => x.Name.ToLower() == WebSetting.HoleRadius.ToLower() &&
                        x.CourseID == courseID);

                    double radius = Convert.ToDouble(ConfigClass.MetersForPaceOfPlay);

                    if (metersForPaceOfPlay != null)
                    {
                        try
                        {
                            radius = Convert.ToDouble(metersForPaceOfPlay.Value);
                        }
                        catch
                        {
                            radius = Convert.ToDouble(ConfigClass.MetersForPaceOfPlay);
                        }
                    }

                    var center = new GeoCoordinate(Convert.ToDouble(lat), Convert.ToDouble(lng));

                    var holeNumber = lstCourseHole.Select(x => new
                                 {
                                     holeNo = x.HoleNumber ?? 0,
                                     distance = center.GetDistanceTo(new GeoCoordinate(Convert.ToDouble(x.Latitude), Convert.ToDouble(x.Longitude)))
                                 }).Where(x => x.distance <= radius).OrderBy(x => x.distance);

                    if (holeNumber.Count() > 0)
                    {
                        return (holeNumber.FirstOrDefault() != null ? holeNumber.FirstOrDefault().holeNo : 0);
                    }
                    else
                    {
                        return 0;
                    }
                }
                else
                {
                    return 0;
                }
            }
            catch
            {
                return 0;
            }
        }

        public static Int64 GetIntervalForCourse(Int64 courseId)
        {
            Int64 timeInterval = Convert.ToInt64("0");
            try
            {
                GolflerEntities DbGolfer = new GolflerEntities();
                var courseSettings = DbGolfer.GF_Settings.Where(x => x.CourseID == courseId && x.Name == "Location Update Time Interval" &&
                    x.Status == StatusType.Active && x.IsActive == true).FirstOrDefault();
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
            //  TimeZoneInfo cstZone = TimeZoneInfo.FindSystemTimeZoneById(courseTimeZone);
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
            GolflerEntities db = new GolflerEntities();

            var webSetting = db.GF_Settings.FirstOrDefault(x => x.Name.ToLower() == WebSetting.CourseTimeZone.ToLower() &&
                x.CourseID == courseID);

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
                //courseTimeZone = webSetting.Value;
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

            string golferTimeZone = ConfigClass.DefaultTimeZone;

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
                TimeZoneInfo cstZone = TimeZoneInfo.FindSystemTimeZoneById(ConfigClass.DefaultTimeZone);
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

            string golferTimeZone = ConfigClass.DefaultTimeZone;

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