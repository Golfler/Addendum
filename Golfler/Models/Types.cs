using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System;
using System.ComponentModel;

namespace Golfler.Models
{
    public class StatusType
    {
        public static string Active { get { return "A"; } }
        public static string InActive { get { return "I"; } }
        public static string Delete { get { return "D"; } }
        public static string Copied { get { return "C"; } }//In Static Pages.
    }

    public class WebSetting
    {
        public static string LoginLockTime { get { return "Login Lock Time"; } }
        public static string StripeAccount { get { return "StripeAccount"; } }
        public static string StripeApiKey { get { return "StripeApiKey"; } }
        public static string StripeSecretApiKey { get { return "StripeSecretApiKey"; } }
        public static string OrderAutoCancelTime { get { return "OrderAutoCancelTime"; } }
        public static string GolflerPlatformFee { get { return "GolflerPlatformFee"; } }
        public static string OrderPlaceLimit { get { return "Order Limit"; } }
        public static string HoleRadius { get { return "Hole Radius"; } }
        public static string CourseTimeZone { get { return "Course Time Zone"; } }
        public static string TimeZone { get { return "Time Zone"; } }
    }

    public class AppScreenName
    {
        public static string ResolutionCenter { get { return "resolutionCenter"; } }
        public static string ActiveOrder { get { return "activeOrder"; } }
        public static string EnRoute { get { return "enroute"; } }
    }

    public class PushnoficationMsgFrom
    {
        public static string Course { get { return "C"; } }
        public static string Golfer { get { return "G"; } }
    }

    public class PromoCodeType
    {
        public static string CategoryWise { get { return "C"; } }
        public static string MenuItemWise { get { return "M"; } }
        public static string AmountWise { get { return "A"; } }
        public string Tag { get; set; }
        public string Name { get; set; }

        public static string GetFullPromoType(string type)
        {
            if (type == CategoryWise)
                return "Category";
            else if (type == MenuItemWise)
                return "Menu";
            else if (type == AmountWise)
                return "Amount";
            return "";
        }

        public static IEnumerable<PromoCodeType> GetPromoCodeType()
        {
            List<PromoCodeType> uList = new List<PromoCodeType>();

            uList.Add(new PromoCodeType() { Name = GetFullPromoType(CategoryWise), Tag = CategoryWise });
            uList.Add(new PromoCodeType() { Name = GetFullPromoType(MenuItemWise), Tag = MenuItemWise });

            return uList.AsEnumerable();
        }
    }

    public class ResolutionCenterType
    {
        public static string Praise { get { return "P"; } }
        public static string Complaint { get { return "C"; } }
        public static string Others { get { return "O"; } }
        public string Tag { get; set; }
        public string Name { get; set; }

        public static string GetFullResolutionCenterType(string type)
        {
            if (type == Praise)
                return "Praise";
            else if (type == Complaint)
                return "Complaint";
            else if (type == Others)
                return "Others";
            return "";
        }

        public static IEnumerable<PromoCodeType> GetResolutionCenterType()
        {
            List<PromoCodeType> uList = new List<PromoCodeType>();

            uList.Add(new PromoCodeType() { Name = GetFullResolutionCenterType(Praise), Tag = Praise });
            uList.Add(new PromoCodeType() { Name = GetFullResolutionCenterType(Complaint), Tag = Complaint });
            uList.Add(new PromoCodeType() { Name = GetFullResolutionCenterType(Others), Tag = Others });

            return uList.AsEnumerable();
        }
    }

    public class Preferences
    {
        public string Tag { get; set; }
        public string Name { get; set; }

        public static IEnumerable<Preferences> GetMeasurement()
        {
            List<Preferences> uList = new List<Preferences>();

            uList.Add(new Preferences() { Name = "Yards", Tag = "yards" });
            uList.Add(new Preferences() { Name = "Meters", Tag = "meters" });

            return uList.AsEnumerable();
        }

        public static IEnumerable<Preferences> GetTemperature()
        {
            List<Preferences> uList = new List<Preferences>();

            uList.Add(new Preferences() { Name = "Fahrenheit", Tag = "fahrenheit" });
            uList.Add(new Preferences() { Name = "Celsius", Tag = "celsius" });

            return uList.AsEnumerable();
        }

        public static IEnumerable<Preferences> GetSpeed()
        {
            List<Preferences> uList = new List<Preferences>();

            uList.Add(new Preferences() { Name = "MPH", Tag = "mph" });
            uList.Add(new Preferences() { Name = "KMPH", Tag = "kmph" });

            return uList.AsEnumerable();
        }
    }

    public class DiscountType
    {
        public static string Percentage { get { return "P"; } }
        public static string Amount { get { return "A"; } }
        public string Tag { get; set; }
        public string Name { get; set; }

        public static string GetFullDiscountType(string type)
        {
            if (type == Percentage)
                return "Percentage";
            else if (type == Amount)
                return "Amount";
            return "";
        }

        public static IEnumerable<PromoCodeType> GetDiscountType()
        {
            List<PromoCodeType> uList = new List<PromoCodeType>();

            uList.Add(new PromoCodeType() { Name = GetFullDiscountType(Percentage), Tag = Percentage });
            uList.Add(new PromoCodeType() { Name = GetFullDiscountType(Amount), Tag = Amount });

            return uList.AsEnumerable();
        }
    }

    public class CoordinateType
    {
        public static string LoadOriginal { get { return "O"; } }
        public static string SetTemporary { get { return "T"; } }

        public static string GetFullCoordinateType(string type)
        {
            if (type == LoadOriginal)
                return "Load Original Co-ordinates";
            else if (type == SetTemporary)
                return "Set Temporary Co-ordinates";
            return "";
        }

        public static IEnumerable<PromoCodeType> GetPromoCodeType()
        {
            List<PromoCodeType> uList = new List<PromoCodeType>();

            uList.Add(new PromoCodeType() { Name = GetFullCoordinateType(LoadOriginal), Tag = LoadOriginal });
            uList.Add(new PromoCodeType() { Name = GetFullCoordinateType(SetTemporary), Tag = SetTemporary });


            return uList.AsEnumerable();
        }
    }

    public class DragItemType
    {
        public static string Tee { get { return "TE"; } }
        public static string FemaleTee { get { return "FT"; } }
        public static string HandicapTee { get { return "HT"; } }
        public static string WhiteFlag { get { return "WF"; } }

        public static string WhiteTee { get { return "WT"; } }
        public static string RedTee { get { return "RT"; } }
        public static string BlueTee { get { return "BT"; } }

        public static string GetFullDragItemType(string type)
        {
            if (type == Tee)
                return "Tee";
            else if (type == FemaleTee)
                return "Female Tee";
            else if (type == HandicapTee)
                return "Handicap Tee";
            else if (type == WhiteFlag)
                return "White Flag";
            else if (type == WhiteTee)
                return "White Tee";
            else if (type == RedTee)
                return "Red Tee";
            else if (type == BlueTee)
                return "Blue Tee";

            return "";
        }
    }

    public class FoodCategoryType
    {
        public static string Cart { get { return "Cart"; } }
        public static string Kitchen { get { return "Kitchen"; } }
        public static string Proshop { get { return "Proshop"; } }
    }

    public class OrderStatus
    {
        public static string Placed { get { return "Order Placed"; } }
        public static string Process { get { return "Order Process"; } }
        public static string Ready { get { return "Order Ready"; } }
        public static string Delivered { get { return "Order Delivered"; } }
        public static string Picked { get { return "Order Picked up"; } }
        public static string Reject { get { return "Order Rejected"; } }
        public static string Cancel { get { return "Order Cancelled"; } }
        public static string PaymentFailed { get { return "Payment Failed"; } }
    }

    public class OrderType
    {
        public static string TurnOrder { get { return "TO"; } } //Turn Order
        public static string CartOrder { get { return "CO"; } } //Cart Order

        public static string GetFullOrderType(string type)
        {
            if (type == TurnOrder)
                return "Turn Order";
            else if (type == CartOrder)
                return "Cart Order";

            return "";
        }
    }

    public class PaymentStatusType
    {
        public static string Active { get { return "A"; } }
        public static string InActive { get { return "I"; } }


        public static string GetFullStatusType(string type)
        {
            if (type == Active)
                return "Active";
            else if (type == InActive)
                return "InActive";
            return "";
        }
    }

    public class ApproveStatusType
    {
        public static string Approve { get { return "A"; } }
        public static string Reject { get { return "R"; } }
        public static string Pending { get { return "P"; } }
        public static string Delete { get { return "D"; } }

        public static string GetFullStatusType(string type)
        {
            if (type == Approve)
                return "Approved";
            else if (type == Reject)
                return "Rejected";
            else if (type == Pending)
                return "Pending";
            else if (type == Delete)
                return "Delete";
            return "";
        }

        public static IEnumerable<object> GetStatusTypeList()
        {
            List<object> uList = new List<object>();
            uList.Add(new { Name = GetFullStatusType(Approve), Tag = Approve });
            uList.Add(new { Name = GetFullStatusType(Pending), Tag = Pending });
            uList.Add(new { Name = GetFullStatusType(Reject), Tag = Reject });
            return uList.AsEnumerable();
        }

        public static IEnumerable<object> GetRateTypeList()
        {
            List<object> uList = new List<object>();
            uList.Add(new { Name = "Private", Tag = "True" });
            uList.Add(new { Name = "Public", Tag = "False" });
            return uList.AsEnumerable();
        }
    }

    public class UserTypeChar
    {
        public static string SuperAdminChar { get { return "S"; } }//Super Admin
        public static string AdminChar { get { return "A"; } }//Admin
        public static string GolferChar { get { return "G"; } }//Course Pro-Shop User
        public static string CourseAdminChar { get { return "C"; } }//Course Admin User
        public static string CourseKitchen { get { return "K"; } }//Course Kitchen User
        public static string CourseCartie { get { return "C"; } }//Course Cartie User
        public static string CourseRanger { get { return "R"; } }//Course Ranger User
        public static string CourseProShop { get { return "P"; } }//Course Pro-Shop User

    }

    public class UserType
    {
        public static string SuperAdmin { get { return UserTypeChar.SuperAdminChar + UserTypeChar.AdminChar; } } //SA
        public static string Admin { get { return UserTypeChar.AdminChar; } } //A
        public static string Golfer { get { return UserTypeChar.GolferChar; } } //G
        public static string CourseAdmin { get { return UserTypeChar.CourseAdminChar + UserTypeChar.AdminChar; } } //CA
        public static string Kitchen { get { return UserTypeChar.CourseAdminChar + UserTypeChar.CourseKitchen; } } //CK
        public static string Cartie { get { return UserTypeChar.CourseAdminChar + UserTypeChar.CourseCartie; } } //CC
        public static string Ranger { get { return UserTypeChar.CourseAdminChar + UserTypeChar.CourseRanger; } } //CR
        public static string Proshop { get { return UserTypeChar.CourseAdminChar + UserTypeChar.CourseProShop; } } //CP
        public static string PowerAdmin { get { return UserTypeChar.CourseProShop + UserTypeChar.AdminChar; } } //PA

        public static string BuildByCourseAdmin { get { return "CA"; } }
        public static string BuildByGolfer { get { return "G"; } }

        public static string GetFullUserType(string type)
        {
            if (type == SuperAdmin)
                return "Super Administrator";
            else if (type == Admin)
                return "Administrator";
            else if (type == Golfer)
                return "Golfer";
            else if (type == CourseAdmin)
                return "Course Administrator";
            else if (type == Kitchen)
                return "Kitchen User";
            else if (type == Cartie)
                return "Gophie User";
            else if (type == Ranger)
                return "Ranger User";
            else if (type == Proshop)
                return "Pro-Shop User";
            else if (type == PowerAdmin)
                return "Admin User";

            return "";
        }

        public static string GetFolder(string type)
        {
            if (type == SuperAdmin)
                return "admin";
            else if (type == Admin)
                return "admin";
            else if (type == CourseAdmin)
                return "course";
            else if (type == Kitchen)
                return "kitchen";
            else if (type == Cartie)
                return "Gophie";
            else if (type == Ranger)
                return "ranger";
            else if (type == Proshop)
                return "proshop";
            else if (type == PowerAdmin)
                return "course";

            return "";
        }

        public string Tag { get; set; }
        public string Name { get; set; }

        public static IEnumerable<UserType> GetSystemUsers()
        {
            List<UserType> uList = new List<UserType>();
            if (LoginInfo.Type == UserType.SuperAdmin || LoginInfo.Type == UserType.Admin)
            {
                uList.Add(new UserType() { Name = GetFullUserType(CourseAdmin), Tag = CourseAdmin });
                uList.Add(new UserType() { Name = GetFullUserType(Admin), Tag = Admin });
            }
            else
            {
                uList.Add(new UserType() { Name = GetFullUserType(PowerAdmin), Tag = PowerAdmin });
                uList.Add(new UserType() { Name = GetFullUserType(Cartie), Tag = Cartie });
                uList.Add(new UserType() { Name = GetFullUserType(Kitchen), Tag = Kitchen });
                uList.Add(new UserType() { Name = GetFullUserType(Proshop), Tag = Proshop });
                uList.Add(new UserType() { Name = GetFullUserType(Ranger), Tag = Ranger });
            }
            return uList.AsEnumerable();
        }

        public static IEnumerable<UserType> GetSystemUsersForMassMsgs()
        {
            List<UserType> uList = new List<UserType>();

            uList.Add(new UserType() { Name = GetFullUserType(PowerAdmin), Tag = PowerAdmin });
            uList.Add(new UserType() { Name = GetFullUserType(Golfer), Tag = Golfer });
            uList.Add(new UserType() { Name = GetFullUserType(Cartie), Tag = Cartie });
            uList.Add(new UserType() { Name = GetFullUserType(Kitchen), Tag = Kitchen });
            uList.Add(new UserType() { Name = GetFullUserType(Proshop), Tag = Proshop });
            uList.Add(new UserType() { Name = GetFullUserType(Ranger), Tag = Ranger });

            return uList.AsEnumerable();
        }

        public static IEnumerable<UserType> GetResolutionSendToType()
        {
            List<UserType> uList = new List<UserType>();

            uList.Add(new UserType() { Name = "Golfler App", Tag = SuperAdmin });
            uList.Add(new UserType() { Name = GetFullUserType(CourseAdmin), Tag = CourseAdmin });
            uList.Add(new UserType() { Name = "Both", Tag = "AL" });

            return uList.AsEnumerable();
        }
    }

    public class Gender
    {
        public static string Male { get { return "M"; } }
        public static string Female { get { return "F"; } }
        public static string Both { get { return "B"; } }

        public string Tag { get; set; }
        public string Name { get; set; }
        public bool Selected { get; set; }

        public static string GetFullGender(string type)
        {
            if (type == Male)
                return "Male";
            else if (type == Female)
                return "Female";
            else if (type == Both)
                return "Both";
            return "";
        }

        public static IEnumerable<Gender> GetGenders(bool isboth = false)
        {
            List<Gender> uList = new List<Gender>();
            uList.Add(new Gender() { Name = GetFullGender(Male), Tag = Male });
            uList.Add(new Gender() { Name = GetFullGender(Female), Tag = Female });
            if (isboth)
                uList.Add(new Gender() { Name = GetFullGender(Both), Tag = Both });
            return uList.AsEnumerable();
        }
    }

    public class MessageType
    {
        public static string Warning { get { return "W"; } }
        public static string Alert { get { return "A"; } }

        public static string GetFullName(string type)
        {
            if (type == Warning)
                return "Warning";
            else if (type == Alert)
                return "Special Alert";
            return "";
        }

        public static IEnumerable<object> GetMessageType()
        {
            List<object> uList = new List<object>();
            uList.Add(new { Name = GetFullName(Warning), Tag = Warning });
            uList.Add(new { Name = GetFullName(Alert), Tag = Alert });
            return uList.AsEnumerable();
        }
    }

    public class ModuleType
    {
        public static string Global { get { return "G"; } }
        public static string Organization { get { return "O"; } }
        public static string All { get { return "A"; } }
        public static string None { get { return ""; } }

        public static string GetFullName(string type)
        {
            if (type == Global)
                return "Global";
            else if (type == Organization)
                return "Specific Organization";
            return "";
        }

        public static IEnumerable<object> GetType()
        {
            List<object> uList = new List<object>();
            uList.Add(new { Name = GetFullName(Global), Tag = Global });
            uList.Add(new { Name = GetFullName(Organization), Tag = Organization });
            return uList.AsEnumerable();
        }
    }

    public class TagList
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public long? ValueNum { get; set; }
        public string Value { get; set; }
    }

    public class EmailTemplateName
    {
        public static string ForgotPassword { get { return "Forgot Password"; } }
        public static string Registration { get { return "Registration"; } }
        public static string VoucherInformation { get { return "Voucher Information"; } }

        public static string UpdateEndUser { get { return "Update End User"; } }
        public static string ApproveAdvertiser { get { return "Approve/Reject Advertiser"; } }

        public static string RegisterAdvertiser { get { return "Register Advertiser"; } }
        public static string NewAdvertiserCreated { get { return "Organization - new Advertiser Registered"; } }

        public static string RegisterDeveloper { get { return "Register Developer"; } }
        public static string ApproveDeveloper { get { return "Approve/Reject Developer"; } }

        public static string NewAdForApprovalAdvertiser { get { return "Advertiser - New Ad for Approval"; } }
        public static string NewAdForApprovalDeveloper { get { return "Developer - New Ad for Approval"; } }

        public static string AdForReApprovalAdvertiser { get { return "Advertiser - Ad for Re-Approval"; } }
        public static string AdForReApprovalDeveloper { get { return "Developer - Ad for Re-Approval"; } }

        public static string AdvertiserApproveRejectAd { get { return "Advertiser - Approve/Reject Ad"; } }
        public static string DeveloperApproveRejectAd { get { return "Developer - Approve/Reject Ad"; } }


        public static string UpdatePassword { get { return "Update Password"; } }
        public static string CaseApprove { get { return "Case Approved"; } }
        public static string AdminCaseApprove { get { return "Case Approved Admin"; } }
        public static string TicketCreate { get { return "Ticket Created"; } }
        public static string TicketAdmin { get { return "Ticket Admin"; } }
        public static string OrganizationCreated { get { return "Organization Created"; } }
        public static string ActiveOrganizationCreated { get { return "Active/Open Organization Created"; } }
        public static string SendMessageMail { get { return "Send Message to end user"; } }
        public static string SendOfferMail { get { return "Send Offer to end user"; } }

        public static string AdvertiserApproveRejectOffer { get { return "Advertiser - Approve/Reject Offer"; } }
        public static string DeveloperApproveRejectOffer { get { return "Developer - Approve/Reject Offer"; } }

        public static string MDMNotification { get { return "MDM Notification"; } }

        public static string PaymentFailure { get { return "Payment Failure"; } }

        public static string AdvertiserPaymentFailure { get { return "Advertiser - Ad Payment Failure"; } }
        public static string DeveloperPaymentFailure { get { return "Developer - Ad Payment Failure"; } }

        public static string AdvertiserNewOffer { get { return "Advertiser - New Offer for Approval"; } }
        public static string DeveloperNewOffer { get { return "Developer - New Offer for Approval"; } }

        public static string AdvertiserOfferStatus { get { return "Advertiser - Approve/Reject Offer"; } }
        public static string DeveloperOfferStatus { get { return "Developer - Approve/Reject Offer"; } }

        public static string AdvertiserInActiveAccount { get { return "Advertiser - In-Active Account"; } }
        public static string DeveloperInActiveAccount { get { return "Developer - In-Active Account"; } }

        public static string LandingPage { get { return "Get in touch with us"; } }

        public static string OrgUserRegistration { get { return "Organization - User Registration"; } }
        public static string OrgUserInviteURL { get { return "Organization - User Invitation URL"; } }

        public static string golferCoordinateSuggestion { get { return "Golfer: Suggested Co-ordinates"; } }
        public static string SuggestedCoordinateStatus { get { return "Suggested Co-ordinates Status"; } }

        public static string OrderRefund { get { return "Order Refund"; } }

        public static string AssignCourseAdmin { get { return "Assign Course Admin"; } }

        public static string MassMessages { get { return "Mass Messages"; } }
        public static string CoordinateReminder { get { return "Golf Coordinate Reminder"; } }

        public static string ResolutionCenterReply { get { return "Resolution Reply"; } }
        public static string GolferAutoResponse { get { return "Resolution Golfer Auto Response"; } }

        public static string PromoCode { get { return "Promo Code"; } }

        public static string AdminUserRegistration { get { return "Admin User Registration"; } }
        public static string CourseUserRegistration { get { return "Course User Registration"; } }
    }

    public class CardType
    {
        public static string Master { get { return "M"; } }
        public static string Visa { get { return "V"; } }
        public static string AmericanExpress { get { return "A"; } }

        public static string GetFullName(string type)
        {
            if (type == Master)
                return "Master";
            else if (type == Visa)
                return "Visa";
            else if (type == AmericanExpress)
                return "American Express";
            return "";
        }

        public static IEnumerable<object> GetCardType()
        {
            List<object> uList = new List<object>();
            uList.Add(new { Name = GetFullName(Master), Tag = Master });
            uList.Add(new { Name = GetFullName(Visa), Tag = Visa });
            uList.Add(new { Name = GetFullName(AmericanExpress), Tag = AmericanExpress });
            return uList.AsEnumerable();
        }
    }

    public class PaymentType
    {
        public static string CreditCard { get { return "R"; } }
        public static string Cheque { get { return "Q"; } }
        public static string Cash { get { return "C"; } }

        public static string GetFullName(string type)
        {
            if (type == CreditCard)
                return "Credit Card";
            else if (type == Cheque)
                return "Cheque";
            else if (type == Cash)
                return "Cash";
            return "";
        }

        public static IEnumerable<object> GetPaymentType(string usertype = "")
        {
            List<object> uList = new List<object>();
            uList.Add(new { Name = GetFullName(Cash), Tag = Cash });
            uList.Add(new { Name = GetFullName(Cheque), Tag = Cheque });
            uList.Add(new { Name = GetFullName(CreditCard), Tag = CreditCard });

            return uList.AsEnumerable();
        }
    }

    public class ModuleValues
    {

        #region old Types
        //////public static string Menu { get { return "menu"; } }
        //////public static string Category { get { return "category"; } }
        public static string StaticPage { get { return "StaticPage"; } }
        //////public static string User { get { return "user"; } }
        ////////  public static string Role { get { return "role"; } }
        ////////  public static string AllRights { get { return "allrights"; } }
        ////////  public static string Course { get { return "Course"; } }
        //////// public static string Smtp { get { return "smtp"; } }
        ////////  public static string EmailTemplates { get { return "emailtemplates"; } }
        //////public static string ContactUs { get { return "contactus"; } }
        //////public static string Golfer { get { return "contactus"; } }
        //////public static string Order { get { return "order"; } }
        ////////  public static string PromoCode { get { return "promocode"; } }
        //////// public static string ProcessRefund { get { return "ProcessRefund"; } }
        //////// public static string MessagingCenter { get { return "MessagingCenter"; } }

        #endregion


        public static string User { get { return "user"; } }
        public static string Role { get { return "role"; } }
        public static string Course { get { return "course"; } }
        public static string ManageFoodItems { get { return "food"; } }
        public static string GopherView { get { return "gopherview"; } }
        public static string OrderHistory { get { return "orderhistory"; } }
        public static string AppView { get { return "appview"; } }
        public static string ManageSettings { get { return "setting"; } }
        public static string EmailTemplates { get { return "email"; } }
        public static string Smtp { get { return "smtpdetail"; } }
        public static string Inbox { get { return "inbox"; } }
        public static string PromoCode { get { return "promocode"; } }
        public static string ProcessRefund { get { return "processRefund"; } }
        public static string MassMessage { get { return "massMsg"; } }
        public static string AllRights { get { return "allrights"; } }

        public static string ActiveOrders { get { return "ActiveOrders"; } }
        public static string managemembershipidnumber { get { return "managemembershipidnumber"; } }
        public static string MissedOrders { get { return "MissedOrders"; } }

        public static string MessageCenter { get { return "msgCenter"; } }
        public static string Ratting { get { return "rating"; } }
        public static string CourseBuilder { get { return "courseBuilder"; } }



        public static string UsersAdmin { get { return "user_admin"; } }
        public static string RolesAdmin { get { return "roles_admin"; } }
        public static string CourseAdmin { get { return "Course_Admin"; } }
        public static string SuggestedCourseAdmin { get { return "suggestedCourse_admin"; } }
        public static string SuggestedGolfCoordinates { get { return "SuggestedGolfCoordinates_admin"; } }
        public static string StaticPageAdmin { get { return "static_admin"; } }
        public static string GolferAdmin { get { return "golfer_admin"; } }
        public static string ItemAdmin { get { return "menuitem_admin"; } }
        public static string SettingAdmin { get { return "setting_admin"; } }
        public static string EmailTemplatesAdmin { get { return "email_admin"; } }
        public static string SmtpAdmin { get { return "smtpdetail_admin"; } }
        public static string inboxAdmin { get { return "inbox_admin"; } }
        public static string ProcessRefundAdmin { get { return "processRefund_admin"; } }
        public static string MassMessageAdmin { get { return "massMsg_admin"; } }
        public static string ManageClubHouse { get { return "manage_club_house"; } }


        public static string CommissionReportAdmin { get { return "commReportadmin"; } }
        public static string AnalyticsReportAdmin { get { return "analyticsReportadmin"; } }

        public static string EmployeeReport { get { return "EmployeeReport"; } }
        public static string GolfPlayingHistoryReport { get { return "GolfPlayingHistoryReport"; } }
        public static string FoodItemsReport { get { return "FoodItemsReport"; } }
        public static string GolferOrderHistoryReport { get { return "golferorderhistoryreport"; } }
    }

    public class RegularExp
    {
        public const string UserName = @"([a-zA-Z]+)";
        public const string Password = @"^[^<>]*$";
        public const string Email = @"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$";
        public const string Url = @"^http(s)?\://[a-zA-Z0-9\-\.]+\.[a-zA-Z]{2,3}(/\S*)?$";
        public const string AlphaNumeric = @"([a-zA-Z0-9 ]+)";
        public const string Numeric = @"([0-9]+)";
        public const string Decimal = @"[\d]{1,15}([.][\d]{1,2})?";
        public const string Alphabets = @"([a-zA-Z ]+)";
        public const string PhoneNumber = @"^[(+),(.),0-9 (,),-]+$";// if phone req
        public const string ZipCode = @"^[0-9]*$";
        public const string PhoneNotReq = @"^([(+),(.),0-9 (,),-]+){10,20}$";// if phone not req
    }

    public class PaymentMode
    {
        public static string Offline { get { return "True"; } }
        public static string Online { get { return "False"; } }

        public static string GetFullName(string type)
        {
            if (type == Offline)
                return "Offline";
            return "Online";
        }

        public static string GetStringValue(bool type)
        {
            if (type)
                return Offline;
            return Online;
        }

        public static IEnumerable<object> GetPaymentMode()
        {
            List<object> uList = new List<object>();
            uList.Add(new { Name = GetFullName(Offline), Tag = Offline });
            uList.Add(new { Name = GetFullName(Online), Tag = Online });
            return uList.AsEnumerable();
        }

        public static bool isOffline(string type = "")
        {
            if (type == Offline)
                return true;
            return false;
        }

    }

    public class MenuType
    {
        public static string Header { get { return "H"; } }
        public static string Footer { get { return "F"; } }
        public static string Both { get { return "B"; } }
    }

    public class LoginType
    {
        public static string SuperAdmin { get { return "SA"; } }
        public static string CourseAdmin { get { return "CA"; } }
        public static string Golfer { get { return "G"; } }
    }

    public class CourseApiName
    {
        [Description("List of course orders i.e. incomming orders")]
        public static string CourseOrders { get { return "Course/ListOfCourseOrders"; } }

        [Description("List of proshop orders which is accepted by proshop user")]
        public static string ProshopOrders { get { return "Course/ListOfProshopOrders"; } }

        [Description("List of proshop orders which is accepted by power admin user")]
        public static string PowerAdminOrders { get { return "Course/ListOfPowerAdminOrders"; } }

        [Description("List of kitchen orders which is accepted by kitchen user")]
        public static string KitcenOrders { get { return "Course/ListOfKitchenOrders"; } }

        [Description("List of cartie orders which is accepted by cartie user")]
        public static string CartieOrders { get { return "Course/ListOfOrders"; } }

        [Description("Accept/Reject orders by Kitchen/Cartie/Proshop")]
        public static string AcceptRejectOrders { get { return "Course/OrderAcceptReject"; } }

        [Description("Orders Pickup Status by Kitchen/Proshop")]
        public static string OrderPickupStatus { get { return "Course/OrderPickupStatus"; } }

        [Description("Orders Delivery Status by Cartie")]
        public static string OrderDeliveryStatusByCartie { get { return "Course/OrderDeliveryStatusByCartie"; } }

        [Description("Orders ready by Kitchen/Cartie/Proshop")]
        public static string OrderReadyStatus { get { return "Course/OrderReadyStatus"; } }

        [Description("Forgot Password")]
        public static string ForgotPassword { get { return "Course/ForgotPasswordService"; } }
    }

    public class GolferApiName
    {
        public static string MsgGolfers { get { return "Messages/"; } }
        public static string ForgotPasswordService { get { return "Golfer/ForgotPasswordService/"; } }
        public static string Friends { get { return "Friends/"; } }
    }

    public class PartershipStatus
    {
        public static string Partner { get { return "P"; } }
        public static string NonPartner { get { return "N"; } }
        public string Tag { get; set; }
        public string Name { get; set; }

        public static string GetFullPartnershipStatus(string type)
        {
            if (type == Partner)
                return "Partner";
            else if (type == NonPartner)
                return "Non-Partner";


            return "";
        }

        public static IEnumerable<PartershipStatus> GetPartnerShipStatus()
        {
            List<PartershipStatus> uList = new List<PartershipStatus>();

            uList.Add(new PartershipStatus() { Name = GetFullPartnershipStatus(Partner), Tag = Partner });
            uList.Add(new PartershipStatus() { Name = GetFullPartnershipStatus(NonPartner), Tag = NonPartner });

            return uList.AsEnumerable();
        }
        public static IEnumerable<PartershipStatus> GetSuggestionStatus()
        {
            List<PartershipStatus> uList = new List<PartershipStatus>();

            uList.Add(new PartershipStatus() { Name = GetFullPartnershipStatus(Partner), Tag = Partner });
            uList.Add(new PartershipStatus() { Name = GetFullPartnershipStatus(NonPartner), Tag = NonPartner });

            return uList.AsEnumerable();
        }
    }

    public class SuggestionType
    {
        public static string Suggested { get { return "S"; } }
        public static string NonSuggested { get { return "N"; } }
        public string Tag { get; set; }
        public string Name { get; set; }

        public static string GetFullSuggestionType(string type)
        {
            if (type == Suggested)
                return "Suggested Courses";
            else if (type == NonSuggested)
                return "Non-Suggested Courses";


            return "";
        }

        public static IEnumerable<SuggestionType> GetSuggestionType()
        {
            List<SuggestionType> uList = new List<SuggestionType>();

            uList.Add(new SuggestionType() { Name = GetFullSuggestionType(Suggested), Tag = Suggested });
            uList.Add(new SuggestionType() { Name = GetFullSuggestionType(NonSuggested), Tag = NonSuggested });

            return uList.AsEnumerable();
        }

    }

    public class ResolutionType
    {
        public static string Praise { get { return "P"; } }
        public static string Complaint { get { return "C"; } }
        public static string Others { get { return "O"; } }
        public string Tag { get; set; }
        public string Name { get; set; }

        public static string GetFullResolutionType(string type)
        {
            if (type == Praise)
                return "Praise";
            else if (type == Complaint)
                return "Complaint";
            else if (type == Others)
                return "Others";

            return "";
        }

        public static IEnumerable<ResolutionType> GetResolutionType()
        {
            List<ResolutionType> uList = new List<ResolutionType>();

            uList.Add(new ResolutionType() { Name = GetFullResolutionType(Praise), Tag = Praise });
            uList.Add(new ResolutionType() { Name = GetFullResolutionType(Complaint), Tag = Complaint });
            uList.Add(new ResolutionType() { Name = GetFullResolutionType(Others), Tag = Others });

            return uList.AsEnumerable();
        }

    }


    public class MessageStatusType
    {
        public static string Open { get { return "A"; } }
        public static string Closed { get { return "C"; } }
        public static string Replyed { get { return "R"; } }

    }

    public class TimeSpanResource
    {
        public static string DateRange { get { return "Date Range"; } }
        public static string Today { get { return "Today"; } }
        public static string Yesterday { get { return "Yesterday"; } }
        public static string ThisWeek { get { return "This Week"; } }
        public static string AllTime { get { return "All Time"; } }
        public static string LastWeek { get { return "Last Week"; } }
        public static string ThisMonth { get { return "This Month"; } }
        public static string LastMonth { get { return "Last Month"; } }
        public static string From { get { return "From"; } }
        public static string To { get { return "To"; } }
        public static string arrowleft { get { return "arrow-left"; } }
        public static string arrowright { get { return "arrow-right"; } }
        public static string Previous { get { return "Previous"; } }
        public static string Next { get { return "Next"; } }
        public static string SelectDate { get { return "Select Date"; } }
        public static string arrowbottom { get { return "arrow-bottom"; } }
    }

    public class ReportResultType
    {
        public static string DateWise { get { return "DateWise"; } }
        public static string WeekWise { get { return "WeekWise"; } }
        public static string MonthWise { get { return "MonthWise"; } }
        public static string YearWise { get { return "YearWise"; } }
    }

    public static class DateTimeExtensions
    {
        public enum Days
        {
            SUNDAY = 0,
            MONDAY = 1,
            TUESDAY = 2,
            WEDNESDAY = 3,
            THURSDAY = 4,
            FRIDAY = 5,
            SATURDAY = 6
        }

        public static DateTime StartOfWeek(this DateTime dt, Days startOfWeek)
        {
            int diff = (int)dt.DayOfWeek - (int)startOfWeek;
            if (diff < 0)
            {
                diff += 7;
            }

            return dt.AddDays(-1 * diff).Date;
        }

        public static IEnumerable<DateTime> Range(this DateTime startDate, DateTime endDate)
        {
            return Enumerable.Range(0, (endDate - startDate).Days + 1).Select(d => startDate.AddDays(d));
        }
    }
}