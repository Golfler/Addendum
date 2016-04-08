using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System;

namespace GolferWebAPI.Models
{
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

    public class RoundStartType
    {
        public static string Front { get { return "F"; } } //Turn Order
        public static string Back { get { return "B"; } } //Cart Order

        public static string GetFullRoundStartType(string type)
        {
            if (type == Front)
                return "Front";
            else if (type == Back)
                return "Back";

            return "";
        }
    }

    public class AppScreenName
    {
        public static string Messaging { get { return "messaging"; } }
        public static string CourseMessaging { get { return "courseMessaging"; } }
        public static string ActiveOrder { get { return "activeOrder"; } }
    }

    public class PushnoficationMsgFrom
    {
        public static string Course { get { return "C"; } }
        public static string Golfer { get { return "G"; } }
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

    public class WebSetting
    {
        public static string LoginLockTime { get { return "Login Lock Time"; } }
        public static string StripeAccount { get { return "StripeAccount"; } }
        public static string StripeApiKey { get { return "StripeApiKey"; } }
        public static string StripeSecretApiKey { get { return "StripeSecretApiKey"; } }
        public static string OrderAutoCancelTime { get { return "OrderAutoCancelTime"; } }
        public static string GolflerPlatformFee { get { return "GolflerPlatformFee"; } }
        public static string OrderPlaceLimit { get { return "Order Limit"; } }
        public static string CourseTimeZone { get { return "Course Time Zone"; } }
        public static string EnableDeliveryOrder { get { return "Enable Delivery Order"; } }
        public static string EnablePickupOrder { get { return "Enable Pickup Order"; } }
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
    }

    public class StatusType
    {
        public static string Active { get { return "A"; } }
        public static string InActive { get { return "I"; } }
        public static string Delete { get { return "D"; } }
        public static string Copied { get { return "C"; } }//In Static Pages.
    }

    public class ApproveStatusType
    {
        public static string Accept { get { return "A"; } }
        public static string Reject { get { return "R"; } }
        public static string Pending { get { return "P"; } }
        public static string Delete { get { return "D"; } }

        public static string GetFullStatusType(string type)
        {
            if (type == Accept)
                return "Accepted";
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
            uList.Add(new { Name = GetFullStatusType(Accept), Tag = Accept });
            uList.Add(new { Name = GetFullStatusType(Pending), Tag = Pending });
            uList.Add(new { Name = GetFullStatusType(Reject), Tag = Reject });
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

    public class UserTypeChar
    {
        public static string SuperAdminChar { get { return "S"; } }//Super Admin
        public static string AdminChar { get { return "A"; } }//Admin
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
        public static string CourseAdmin { get { return UserTypeChar.CourseAdminChar + UserTypeChar.AdminChar; } } //CA
        public static string Kitchen { get { return UserTypeChar.CourseAdminChar + UserTypeChar.CourseKitchen; } } //CK
        public static string Cartie { get { return UserTypeChar.CourseAdminChar + UserTypeChar.CourseCartie; } } //CC
        public static string Ranger { get { return UserTypeChar.CourseAdminChar + UserTypeChar.CourseRanger; } } //CR
        public static string Proshop { get { return UserTypeChar.CourseAdminChar + UserTypeChar.CourseProShop; } } //CP
        public static string PowerAdmin { get { return UserTypeChar.CourseProShop + UserTypeChar.AdminChar; } } //PA

        public static string Golfer { get { return "G"; } } //G

        public static string GetFullUserType(string type)
        {
            if (type == SuperAdmin)
                return "Super Administrator";
            else if (type == Admin)
                return "Administrator";
            else if (type == CourseAdmin)
                return "Course Administrator";
            else if (type == Kitchen)
                return "Course Kitchen User";
            else if (type == Cartie)
                return "Course Cartie Head";
            else if (type == Ranger)
                return "Course Ranger User";
            else if (type == Proshop)
                return "Course Pro Shop User";
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
                return "cartie";
            else if (type == Ranger)
                return "ranger";
            else if (type == Proshop)
                return "proshop";

            return "";
        }

        public string Tag { get; set; }
        public string Name { get; set; }

        public static IEnumerable<UserType> GetSystemUsers()
        {
            List<UserType> uList = new List<UserType>();
            uList.Add(new UserType() { Name = GetFullUserType(CourseAdmin), Tag = CourseAdmin });
            uList.Add(new UserType() { Name = GetFullUserType(Admin), Tag = Admin });
            return uList.AsEnumerable();
        }
    }
}