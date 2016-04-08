using System.Configuration;
using System;

namespace Golfler.Models
{
    public static class Params
    {
        static Params()
        {
            TimeOut = Convert.ToInt32(ConfigurationManager.AppSettings["TimeOut"]);

            CurrencySymbol = ConfigurationManager.AppSettings["CURRENCY_SYMBOL"];
            CurrencyName = ConfigurationManager.AppSettings["CURRENCY_NAME"];

            SitePrefix = ConfigurationManager.AppSettings["SITE_PREFIX"];
            SiteName = ConfigurationManager.AppSettings["SITE_NAME"];
            SiteUrl = ConfigurationManager.AppSettings["APPSITEURL"];
            SiteUrlAdminSection = SiteUrl + "appstore/admin";

            PageSize = Convert.ToInt32(ConfigurationManager.AppSettings["PageSize"]);
        }

        public static int TimeOut { get; private set; }

        public static string CurrencySymbol { get; private set; }
        public static string CurrencyName { get; private set; }

        public static string SitePrefix { get; private set; }
        public static string SiteName { get; private set; }
        public static string SiteUrl { get; private set; }
        public static string SiteUrlAdminSection { get; private set; }

        private static string _emailTemplatePath;
        public static string EmailTemplatePath
        {
            get
            {
                if (string.IsNullOrEmpty(_emailTemplatePath))
                    _emailTemplatePath = ConfigurationManager.AppSettings["EMAIL_TEMPLATE_PATH"];
                return _emailTemplatePath;
            }
        }

        private static string _emailFrom;
        public static string EmailFrom
        {
            get
            {
                if (string.IsNullOrEmpty(_emailFrom))
                    _emailFrom = ConfigurationManager.AppSettings["FROM_EMAILID"];
                return _emailFrom;
            }
        }

        private static bool _enableSsl;
        public static bool EnableSsl
        {
            get
            {
                if (_enableSsl == null)
                    _enableSsl = Convert.ToBoolean(ConfigurationManager.AppSettings["SSL_MODE"]);
                return _enableSsl;
            }
        }

        private static string _adminEmail;
        public static string AdminEmail
        {
            get
            {
                if (string.IsNullOrEmpty(_adminEmail))
                    _adminEmail = ConfigurationManager.AppSettings["ADMIN_EMAILID"];
                return _adminEmail;
            }
        }

        private static string _mailImagePath;
        public static string MailImagePath
        {
            get
            {
                if (string.IsNullOrEmpty(_mailImagePath))
                {
                    _mailImagePath = SiteUrl;
                    _mailImagePath = _mailImagePath.Substring(0, _mailImagePath.Length - 1);
                }
                return _mailImagePath;
            }
        }

        private static string _categoryImagesPath;
        public static string CategoryImagesPath
        {
            get
            {
                if (string.IsNullOrEmpty(_categoryImagesPath))
                    _categoryImagesPath = ConfigurationManager.AppSettings["CategoryImagesPath"];
                return _categoryImagesPath;
            }
        }
        private static string _SuperAdminUrl;
        public static string SuperAdminUrl
        {
            get
            {
                if (string.IsNullOrEmpty(_SuperAdminUrl))
                    _SuperAdminUrl = ConfigurationManager.AppSettings["SuperAdminUrl"];
                return _SuperAdminUrl;
            }
        }
        private static string _GolflerUrl;
        public static string GolflerUrl
        {
            get
            {
                if (string.IsNullOrEmpty(_GolflerUrl))
                    _GolflerUrl = ConfigurationManager.AppSettings["GolflerUrl"];
                return _GolflerUrl;
            }
        }
        private static string _AppStoreAdminUrl;
        public static string AppStoreAdminUrl
        {
            get
            {
                if (string.IsNullOrEmpty(_AppStoreAdminUrl))
                    _AppStoreAdminUrl = ConfigurationManager.AppSettings["AppStoreAdminUrl"];
                return _AppStoreAdminUrl;
            }
        }
        public static string GetCategoryImagesPath
        {
            get
            {
                return SiteUrl + CategoryImagesPath.Replace("~/", "");
            }
        }

        private static long _superAdminId;
        public static long SuperAdminID
        {
            get
            {
                if (_superAdminId == 0)
                    _superAdminId = Convert.ToInt64(ConfigurationManager.AppSettings["SuperAdminID"]);
                return _superAdminId;
            }
        }

        private static string _templatePath;
        public static string TemplatePath
        {
            get
            {
                if (string.IsNullOrEmpty(_templatePath))
                    _templatePath = Convert.ToString(ConfigurationManager.AppSettings["TemplatePath"]);
                return _templatePath;
            }
        }

        private static string _HTMLFileName;
        public static string HTMLFileName
        {
            get
            {
                if (string.IsNullOrEmpty(_HTMLFileName))
                    _HTMLFileName = Convert.ToString(ConfigurationManager.AppSettings["HTMLFileName"]);
                return _HTMLFileName;
            }
        }

        private static string _CSSFileName;
        public static string CSSFileName
        {
            get
            {
                if (string.IsNullOrEmpty(_CSSFileName))
                    _CSSFileName = Convert.ToString(ConfigurationManager.AppSettings["CSSFileName"]);
                return _CSSFileName;
            }
        }

        private static string _LOGOFileName;
        public static string LOGOFileName
        {
            get
            {
                if (string.IsNullOrEmpty(_LOGOFileName))
                    _LOGOFileName = Convert.ToString(ConfigurationManager.AppSettings["LOGOFileName"]);
                return _LOGOFileName;
            }
        }

        private static string _NoLOGOFileName;
        public static string NoLOGOFileName
        {
            get
            {
                if (string.IsNullOrEmpty(_NoLOGOFileName))
                    _NoLOGOFileName = Convert.ToString(ConfigurationManager.AppSettings["NoLOGOFileName"]);
                return _NoLOGOFileName;
            }
        }

        private static string _previewFolder;
        public static string PreviewFolder
        {
            get
            {
                if (string.IsNullOrEmpty(_previewFolder))
                    _previewFolder = Convert.ToString(ConfigurationManager.AppSettings["PreviewFolder"]);
                return _previewFolder;
            }
        }

        private static string _DateFormat;
        public static string DateFormat
        {
            get
            {
                if (string.IsNullOrEmpty(_DateFormat))
                    _DateFormat = Convert.ToString(ConfigurationManager.AppSettings["DateFormat"]);
                return _DateFormat;
            }
        }

        private static string _JqDateFormat;
        public static string JqDateFormat
        {
            get
            {
                if (string.IsNullOrEmpty(_JqDateFormat))
                    _JqDateFormat = Convert.ToString(ConfigurationManager.AppSettings["JqDateFormat"]);
                return _JqDateFormat;
            }
        }

        private static int _pageSize;
        public static int FrontPageSize
        {
            get
            {
                if (_pageSize == 0)
                    _pageSize = Convert.ToInt32(ConfigurationManager.AppSettings["PageSizeEight"]);
                return _pageSize;
            }
        }

        public static int PageSize
        {
            get;
            set;
        }

        private static string _siteFooter;
        public static string SiteFooter
        {
            get
            {
                if (string.IsNullOrEmpty(_siteFooter))
                    _siteFooter = ConfigurationManager.AppSettings["SITE_FOOTER"];
                return _siteFooter;
            }
        }

        private static string _siteHeaderTitle;
        public static string SiteHeaderTitle
        {
            get
            {
                if (string.IsNullOrEmpty(_siteHeaderTitle))
                    _siteHeaderTitle = ConfigurationManager.AppSettings["SITE_TITLE_HEADER"];
                return _siteHeaderTitle;
            }
        }
    }
}