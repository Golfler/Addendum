using System;
using System.Configuration;

namespace GolferWebAPI.Models
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
        public static string Distance = ConfigurationManager.AppSettings["Distance"];
        public static string LogoUrl = ConfigurationManager.AppSettings["LogoUrl"];
        public static string ImageSiteUrl = ConfigurationManager.AppSettings["IMAGE_SITE_URL"];
        public static string DefaultTimeZone = ConfigurationManager.AppSettings["DefaultTimeZone"];
        public static string GolferDefaultTimeZone = ConfigurationManager.AppSettings["GolferDefaultTimeZone"];

        #region Web Settings Default

        public static string OrderAutoCancelTime = ConfigurationManager.AppSettings["OrderAutoCancelTime"];
        public static string GolflerPlatformFee = ConfigurationManager.AppSettings["GolflerPlatformFee"];
        public static string OrderPlaceLimit = ConfigurationManager.AppSettings["OrderPlaceLimit"];

        #endregion
    }
}