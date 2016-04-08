using System;
using System.Configuration;

namespace CourseWebApi.Models
{
    /// <summary>
    /// Created By: Arum
    /// Created By: 05 Nov. 2014 
    /// Purpose:Site prefix data
    /// </summary>
    public class ConfigClass
    {
        public static string SitePrefix = ConfigurationManager.AppSettings["SITE_PREFIX"];
        public static string SiteName = ConfigurationManager.AppSettings["SITE_NAME"];
        public static int PageSize = Convert.ToInt32(ConfigurationManager.AppSettings["PAGE_SIZE"]);
        public static int MessageListingPageSize = Convert.ToInt32(ConfigurationManager.AppSettings["MESSAGE_LISTING_PAGE_SIZE"]);
        public static string SiteUrl = ConfigurationManager.AppSettings["SITE_URL"];
        public static string SiteImageUrl = ConfigurationManager.AppSettings["SITE_URL_FOR_IMAGE"];
        public static string ImageSiteUrl = ConfigurationManager.AppSettings["IMAGE_SITE_URL"];
        
        public static string EmailTemplatePath = ConfigurationManager.AppSettings["EMAIL_TEMPLATE_PATH"];
        public static string FromMail = ConfigurationManager.AppSettings["FromMail"];
        public static string SMTP_Host = ConfigurationManager.AppSettings["SMTP_Host"];
        public static string SMTP_Username = ConfigurationManager.AppSettings["SMTP_Username"];
        public static string SMTP_Password = ConfigurationManager.AppSettings["SMTP_Password"];
        public static string ErrorFilePath = ConfigurationManager.AppSettings["Error_FilePath"];
        public static string ErrorMessagesFilePath = ConfigurationManager.AppSettings["ErrorMessage_FilePath"];

        public static string IPHONE_CERT = ConfigurationManager.AppSettings["IPHONE_CERT"];
        public static string CERT_PASSWORD = ConfigurationManager.AppSettings["CERT_PASSWORD"];
        public static string GOOGLE_API_KEY = ConfigurationManager.AppSettings["GOOGLE_API_KEY"];
        public static string GOOGLE_PROJECT_ID = ConfigurationManager.AppSettings["GOOGLE_PROJECT_ID"];
        public static string Gophie20MinsMapColor = ConfigurationManager.AppSettings["Gophie20MinsMapColor"];
        public static string Gophie10MinsMapColor = ConfigurationManager.AppSettings["Gophie10MinsMapColor"];
        public static string Gophie5MinsMapColor = ConfigurationManager.AppSettings["Gophie5MinsMapColor"];

        public static string GolflerSiteURL = ConfigurationManager.AppSettings["GolflerSiteURL"];
        public static string LogoUrl = ConfigurationManager.AppSettings["LogoUrl"];
        public static string DefaultTimeZone = ConfigurationManager.AppSettings["DefaultTimeZone"];

        #region Web Settings Default

        public static string OrderAutoCancelTime = ConfigurationManager.AppSettings["OrderAutoCancelTime"];
        public static string GolflerPlatformFee = ConfigurationManager.AppSettings["GolflerPlatformFee"];
        public static string MetersForPaceOfPlay = ConfigurationManager.AppSettings["MetersForPaceOfPlay"];

        #endregion
    }
}