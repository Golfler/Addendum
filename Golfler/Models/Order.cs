using System;
using System.Collections.Generic;
using System.Data.Objects;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using Newtonsoft.Json.Linq;
using System.Data.SqlClient;
using System.Globalization;
using Newtonsoft.Json;

namespace Golfler.Models
{
    public class Order
    {
        public GF_Order OrderObj { get; private set; }

        protected GolflerEntities Db;

        public string Message { get; private set; }
        public decimal commissionFeeTotal { get; set; }
        public decimal plateformFeeTotal { get; set; }
        public decimal coursePlatformFee { get; set; }

        #region Constructors

        public Order()
        {
            Db = new GolflerEntities();
        }

        #endregion

        /// <summary>
        /// Created By: Amit Kumar
        /// Created on: 28 March 2015
        /// Modified By: Ramesh Kalra
        /// Modified On: 14 July 2015
        /// Modifiy Purpose: Previously this function was used for getting all orders of the course. Now We can get list of orders made by specific Golfer. So we add new parameter GolferId. 
        ///                  If GolferId=0 than all orders otherwise orders made by GolferId
        /// </summary>
        /// <remarks>Get Order Listing</remarks>
        public IQueryable<GF_Order> GetOrders(string filterExpression, string fromDate, string toDate, string type, string paymentType, Int64 GolferId, string sortExpression, string sortDirection, int pageIndex, int pageSize, ref int totalRecords)
        {
            IQueryable<GF_Order> list;

            if (!String.IsNullOrWhiteSpace(filterExpression))
            {
                bool isNoNumbers = System.Text.RegularExpressions.Regex.IsMatch(filterExpression, @"^[^0-9]+$");

                if (isNoNumbers)
                {
                    if (GolferId > 0)  // If golfer id is greater than 0; it means we are searching orders for specific Golfer; so don't need to search within Golfer name and email
                    {
                        // In this case; if filterexpression is not a number; than these should be no result; so we will response as no record found by ORDER ID=0
                        long orderID = Convert.ToInt32("0");
                        list = Db.GF_Order.Where(x => x.ID == orderID &&
                                                       x.CourseID == LoginInfo.CourseId &&
                                                       !(x.IsRejected ?? false)).OrderByDescending(x => x.ID);
                    }
                    else
                    {
                        list = Db.GF_Order.Where(x => ((x.GF_Golfer.FirstName + " " + x.GF_Golfer.LastName).ToLower().Contains(filterExpression.ToLower()) ||
                                                    x.GF_Golfer.Email.ToLower().StartsWith(filterExpression.ToLower()) ||
                                                    x.GF_OrderDetails.Where(y => y.GF_MenuItems.Name.ToLower().Contains(filterExpression.ToLower())).Count() > 0) &&
                                                    x.CourseID == LoginInfo.CourseId &&
                                                   !(x.IsRejected ?? false)).OrderByDescending(x => x.ID);
                    }
                }
                else
                {
                    long orderID = Convert.ToInt32(filterExpression);
                    list = Db.GF_Order.Where(x => x.ID == orderID &&
                                                   x.CourseID == LoginInfo.CourseId &&
                                                   !(x.IsRejected ?? false)).OrderByDescending(x => x.ID);
                }
            }
            else
            {
                list = Db.GF_Order.Where(x => x.CourseID == LoginInfo.CourseId &&
                                               !(x.IsRejected ?? false)).OrderByDescending(x => x.ID);
            }

            if (GolferId > 0)
            {
                list = list.Where(x => x.GolferID == GolferId && x.CourseID == LoginInfo.CourseId).AsQueryable();
            }

            #region Filters

            if (!string.IsNullOrEmpty(type))
            {
                if (type == "K")
                {
                    list = list.ToList().Where(x => (x.KitchenId ?? 0) > 0).AsQueryable();
                }
                else if (type == "P")
                {
                    list = list.ToList().Where(x => (x.ProShopID ?? 0) > 0).AsQueryable();
                }
                else if (type == "D")
                {
                    list = list.ToList().Where(x => (x.IsDelivered ?? false)).AsQueryable();
                }

            }
            if (!string.IsNullOrEmpty(fromDate) && !string.IsNullOrEmpty(toDate))
            {
                DateTime dtDate = DateTime.Parse(fromDate);
                DateTime dtToDate = DateTime.Parse(toDate);

                list = list.ToList().Where(x => x.OrderDate.Value.Date >= dtDate.Date
                    && x.OrderDate.Value.Date <= dtToDate.Date).AsQueryable();
            }
            else if (!string.IsNullOrEmpty(fromDate))
            {
                DateTime dtDate = DateTime.Parse(fromDate);

                list = list.ToList().Where(x => x.OrderDate.Value.Date >= dtDate.Date).AsQueryable();
            }
            else if (!string.IsNullOrEmpty(toDate))
            {
                DateTime dtToDate = DateTime.Parse(toDate);
                list = list.ToList().Where(x => x.OrderDate.Value.Date <= dtToDate.Date).AsQueryable();
            }

            if (!string.IsNullOrEmpty(paymentType))
            {
                if (paymentType == "C")
                {

                    list = list.ToList().Where(x => string.IsNullOrEmpty(x.MemberShipID)).AsQueryable();
                }
                else if (paymentType == "M")
                {
                    list = list.ToList().Where(x => !string.IsNullOrEmpty(x.MemberShipID)).AsQueryable();
                }
            }


            #endregion

            totalRecords = list.Count();

            return list.OrderBy(sortExpression + " " + sortDirection).Skip((pageIndex - 1) * pageSize).Take(pageSize);
        }

        /// <summary>
        /// Created By: Amit Kumar
        /// Created on: 07 August 2015
        /// </summary>
        /// <remarks>Get Order Listing</remarks>
        public IQueryable<object> GetOrders_New(string filterExpression, string fromDate, string toDate, string type, string paymentType, Int64 GolferId, string sortExpression, string sortDirection, int pageIndex, int pageSize, ref int totalRecords)
        {
            IQueryable<GF_Order> list;

            if (!String.IsNullOrWhiteSpace(filterExpression))
            {
                bool isNoNumbers = System.Text.RegularExpressions.Regex.IsMatch(filterExpression, @"^[^0-9]+$");

                if (isNoNumbers)
                {
                    if (GolferId > 0)  // If golfer id is greater than 0; it means we are searching orders for specific Golfer; so don't need to search within Golfer name and email
                    {
                        // In this case; if filterexpression is not a number; than these should be no result; so we will response as no record found by ORDER ID=0
                        long orderID = Convert.ToInt32("0");
                        list = Db.GF_Order.Where(x => x.ID == orderID &&
                                                       x.CourseID == LoginInfo.CourseId &&
                                                       !(x.IsRejected ?? false)).OrderByDescending(x => x.ID);
                    }
                    else
                    {
                        list = Db.GF_Order.Where(x => ((x.GF_Golfer.FirstName + " " + x.GF_Golfer.LastName).ToLower().Contains(filterExpression.ToLower()) ||
                                                    x.GF_Golfer.Email.ToLower().StartsWith(filterExpression.ToLower()) ||
                                                    x.GF_OrderDetails.Where(y => y.GF_MenuItems.Name.ToLower().Contains(filterExpression.ToLower())).Count() > 0) &&
                                                    x.CourseID == LoginInfo.CourseId &&
                                                   !(x.IsRejected ?? false)).OrderByDescending(x => x.ID);
                    }
                }
                else
                {
                    long orderID = Convert.ToInt32(filterExpression);
                    list = Db.GF_Order.Where(x => x.ID == orderID &&
                                                   x.CourseID == LoginInfo.CourseId &&
                                                   !(x.IsRejected ?? false)).OrderByDescending(x => x.ID);
                }
            }
            else
            {
                list = Db.GF_Order.Where(x => x.CourseID == LoginInfo.CourseId &&
                                               !(x.IsRejected ?? false)).OrderByDescending(x => x.ID);
            }

            if (GolferId > 0)
            {
                list = list.Where(x => x.GolferID == GolferId && x.CourseID == LoginInfo.CourseId).AsQueryable();
            }

            #region Filters

            if (!string.IsNullOrEmpty(type))
            {
                if (type == "K")
                {
                    list = list.ToList().Where(x => (x.KitchenId ?? 0) > 0).AsQueryable();
                }
                else if (type == "P")
                {
                    list = list.ToList().Where(x => (x.ProShopID ?? 0) > 0).AsQueryable();
                }
                else if (type == "D")
                {
                    list = list.ToList().Where(x => (x.IsDelivered ?? false)).AsQueryable();
                }

            }
            if (!string.IsNullOrEmpty(fromDate) && !string.IsNullOrEmpty(toDate))
            {
                DateTime dtDate = DateTime.Parse(fromDate);
                DateTime dtToDate = DateTime.Parse(toDate);

                list = list.ToList().Where(x => x.OrderDate.Value.Date >= dtDate.Date
                    && x.OrderDate.Value.Date <= dtToDate.Date).AsQueryable();
            }
            else if (!string.IsNullOrEmpty(fromDate))
            {
                DateTime dtDate = DateTime.Parse(fromDate);

                list = list.ToList().Where(x => x.OrderDate.Value.Date >= dtDate.Date).AsQueryable();
            }
            else if (!string.IsNullOrEmpty(toDate))
            {
                DateTime dtToDate = DateTime.Parse(toDate);
                list = list.ToList().Where(x => x.OrderDate.Value.Date <= dtToDate.Date).AsQueryable();
            }

            if (!string.IsNullOrEmpty(paymentType))
            {
                if (paymentType == "C")
                {

                    list = list.ToList().Where(x => string.IsNullOrEmpty(x.MemberShipID)).AsQueryable();
                }
                else if (paymentType == "M")
                {
                    list = list.ToList().Where(x => !string.IsNullOrEmpty(x.MemberShipID)).AsQueryable();
                }
            }


            #endregion

            totalRecords = list.Count();

            commissionFeeTotal = list.Sum(x => x.Commission ?? 0);
            plateformFeeTotal = list.Sum(x => x.GolferPlatformFee ?? 0);
            coursePlatformFee = list.Sum(x => x.CoursePlatformFee ?? 0);

            string timeZone = CommonFunctions.GetCourseTimeZone(LoginInfo.CourseId);
            IQueryable<object> lst = list.ToList().Select((x =>
                        new
                        {
                            x.ID,
                            //OrderDate = CommonFunctions.DateByTimeZone(timeZone, x.OrderDate ?? DateTime.UtcNow),
                            OrderDate = CommonFunctions.DateByTimeZone(timeZone, x.OrderDate ?? DateTime.UtcNow).ToString("MM/dd/yyyy hh:mm tt"),
                            time = CommonFunctions.DateByTimeZone(timeZone, x.OrderDate ?? DateTime.UtcNow).ToShortTimeString(),
                            golferName = x.GF_Golfer.FirstName + " " + x.GF_Golfer.LastName,
                            golferEmail = x.GF_Golfer.Email,
                            billAmount = x.GF_OrderDetails.Sum(y => ((y.UnitPrice ?? 0) * (y.Quantity ?? 0))).ToString("F"),
                            TaxAmount = (x.TaxAmount ?? 0).ToString("F"),
                            GolferPlatformFee = (x.GolferPlatformFee ?? 0).ToString("F"),
                            GrandTotal = x.GrandTotal ?? 0,
                            itemOrdered = new JavaScriptSerializer().Serialize(x.GF_OrderDetails.ToList().Select(y =>
                                                                            new
                                                                            {
                                                                                y.GF_MenuItems.Name,
                                                                                UnitPrice = y.UnitPrice,
                                                                                y.Quantity,
                                                                                Amount = (y.UnitPrice * y.Quantity),
                                                                                MenuOptionName = y.GF_OrderMenuOptionDetail.Select(q => q.MenuOptionName).ToList()
                                                                            })),
                            OrderType = OrderType.GetFullOrderType(x.OrderType),
                            PaymentMode = (x.PaymentType == "1" ? "Payment by Card" : "Payment by MembershipId"),
                            CourseInfo = x.GF_CourseInfo.COURSE_NAME,
                            CourseAddress = x.GF_CourseInfo.ADDRESS,
                            PromoCode = x.GF_PromoCode == null ? "0.00" : (x.DiscountAmt ?? 0).ToString("F"),
                            TransId = x.PaymentType == "0" ? x.MemberShipID : ((x.BT_TransId == null ? "" : x.BT_TransId)),
                            OrderStatus = ((x.IsDelivered ?? false) || (x.IsPickup ?? false)) ? "Paid" : "Pending",
                            Cartie = x.CartieId > 0 ? Db.GF_AdminUsers.FirstOrDefault(y => y.ID == x.CartieId).UserName : "N/A",
                            PreparedByType = (string.Join(",", x.GF_OrderDetails.ToList()
                                            .Select(y => y.GF_MenuItems.GF_SubCategory.GF_Category.Type)
                                            .Distinct().ToList())).ToLower() == FoodCategoryType.Proshop.ToLower() ? "Proshop" : "Kitchen",
                            PreparedBy = (string.Join(",", x.GF_OrderDetails.ToList()
                                        .Select(y => y.GF_MenuItems.GF_SubCategory.GF_Category.Type)
                                        .Distinct().ToList())).ToLower() == FoodCategoryType.Proshop.ToLower() ?
                                        ((Db.GF_AdminUsers.FirstOrDefault(k => k.ID == x.ProShopID) == null ? "N/A" : (Db.GF_AdminUsers.FirstOrDefault(k => k.ID == x.ProShopID).FirstName))) : ((Db.GF_AdminUsers.FirstOrDefault(k => k.ID == x.KitchenId) == null ? "N/A" : (Db.GF_AdminUsers.FirstOrDefault(k => k.ID == x.KitchenId).FirstName))),
                            commissionFeeTotal,
                            plateformFeeTotal,
                            coursePlatformFee,
                        }
                    )).AsQueryable();

            return lst.OrderBy(sortExpression + " " + sortDirection).Skip((pageIndex - 1) * pageSize).Take(pageSize);
        }

        /// <summary>
        /// Created By: Amit Kumar
        /// Created on: 06 April 2015
        /// </summary>
        /// <remarks>Get Order History Listing of golfer user</remarks>
        public IQueryable<GF_Order> GetOrderHistory(string filterExpression, string fromDate, string toDate, string orderAmount,
            string sortExpression, string sortDirection, int pageIndex, int pageSize, string orderType, long? courseID, ref int totalRecords)
        {
            IQueryable<GF_Order> list;
            bool isFilled = false;
            if (!String.IsNullOrWhiteSpace(filterExpression))
            {
                //var cartieID = Db.GF_AdminUsers.FirstOrDefault(x => (x.FirstName + " " + x.LastName).ToLower().Contains(filterExpression.ToLower()) &&
                //    x.Type.Contains(UserType.Cartie));
                //if (cartieID != null)
                //    list = Db.GF_Order.Where(x => x.CartieId == cartieID.ID &&
                //         x.GolferID == LoginInfo.GolferUserId).OrderByDescending(x => x.ID);
                //else
                //    list = Db.GF_Order.Where(x => x.CartieId < 0 &&
                //         x.GolferID == LoginInfo.GolferUserId).OrderByDescending(x => x.ID);

                long searchedOrderNo = Convert.ToInt64(filterExpression);
                list = Db.GF_Order.Where(x => x.GolferID == LoginInfo.GolferUserId &&
                    (x.ID == searchedOrderNo || x.MemberShipID.Trim().ToLower() == filterExpression.Trim().ToLower())).OrderByDescending(x => x.ID);
                if (orderType == "M")
                {
                    if (list.Count() == 0)
                    {
                        list = Db.GF_Order.Where(x => x.GolferID == LoginInfo.GolferUserId && x.MemberShipID == filterExpression.Trim()).OrderByDescending(x => x.ID);
                        isFilled = true;
                    }
                    else
                    {

                        list = from item in list
                               where (!string.IsNullOrEmpty(item.MemberShipID) && string.IsNullOrEmpty(item.StripeToken))
                               select item;
                        isFilled = true;
                    }
                }
            }
            else
            {
                list = Db.GF_Order.Where(x => x.GolferID == LoginInfo.GolferUserId).OrderByDescending(x => x.ID);
            }

            #region Filters

            if (!string.IsNullOrEmpty(fromDate) && !string.IsNullOrEmpty(toDate))
            {
                DateTime dtDate = DateTime.Parse(fromDate);
                DateTime dtToDate = DateTime.Parse(toDate);

                list = list.ToList().Where(x => x.OrderDate.Value.Date >= dtDate.Date
                    && x.OrderDate.Value.Date <= dtToDate.Date).AsQueryable();
            }
            else if (!string.IsNullOrEmpty(fromDate))
            {
                DateTime dtDate = DateTime.Parse(fromDate);

                list = list.ToList().Where(x => x.OrderDate.Value.Date >= dtDate.Date).AsQueryable();
            }
            else if (!string.IsNullOrEmpty(toDate))
            {
                DateTime dtToDate = DateTime.Parse(toDate);
                list = list.ToList().Where(x => x.OrderDate.Value.Date <= dtToDate.Date).AsQueryable();
            }
            else if (!string.IsNullOrEmpty(orderAmount))
            {
                decimal oAmt = 0;
                try
                {
                    oAmt = Convert.ToDecimal(orderAmount);
                }
                catch
                {
                    oAmt = 0;
                }

                list = list.ToList().Where(x => (x.GrandTotal ?? 0) <= oAmt).AsQueryable();
            }

            if (orderType != "")
            {
                if (orderType == "R")
                {
                    //  var listtemp = list.Where(x => x.GF_OrderRefund.ToList().Count > 0).ToList();
                    list = list.Where(x => x.GF_OrderRefund.Any());
                }
                if (orderType == "M")
                {
                    //  var listtemp = list.Where(x => x.GF_OrderRefund.ToList().Count > 0).ToList();
                    if (!isFilled)
                    {
                        list = from item in list
                               where (!string.IsNullOrEmpty(item.MemberShipID) && string.IsNullOrEmpty(item.StripeToken))
                               select item;
                    }
                }
            }

            if ((courseID ?? 0) > 0)
            {
                list = list.Where(x => (x.CourseID ?? 0) == (courseID ?? 0));
            }

            #endregion

            totalRecords = list.Count();

            return list.OrderBy(sortExpression + " " + sortDirection).Skip((pageIndex - 1) * pageSize).Take(pageSize);
        }

        /// <summary>
        /// Created By: Amit Kumar
        /// Created on: 06 April 2015
        /// </summary>
        /// <remarks>Get Order History Listing of golfer user</remarks>
        public IQueryable<GF_Order> GetOrderHistoryNew(string filterExpression, string fromDate, string toDate, string orderAmount,
            string sortExpression, string sortDirection, int pageIndex, int pageSize, string orderType, long? courseID, string ccNumber,
            ref int totalRecords)
        {
            IQueryable<GF_Order> list;
            bool isFilled = false;
            if (!String.IsNullOrWhiteSpace(filterExpression))
            {
                //var cartieID = Db.GF_AdminUsers.FirstOrDefault(x => (x.FirstName + " " + x.LastName).ToLower().Contains(filterExpression.ToLower()) &&
                //    x.Type.Contains(UserType.Cartie));
                //if (cartieID != null)
                //    list = Db.GF_Order.Where(x => x.CartieId == cartieID.ID &&
                //         x.GolferID == LoginInfo.GolferUserId).OrderByDescending(x => x.ID);
                //else
                //    list = Db.GF_Order.Where(x => x.CartieId < 0 &&
                //         x.GolferID == LoginInfo.GolferUserId).OrderByDescending(x => x.ID);

                long searchedOrderNo = Convert.ToInt64(filterExpression);
                list = Db.GF_Order.Where(x => x.GolferID == LoginInfo.GolferUserId &&
                    (x.ID == searchedOrderNo || x.MemberShipID.Trim().ToLower() == filterExpression.Trim().ToLower())).OrderByDescending(x => x.ID);
                if (orderType == "M")
                {
                    if (list.Count() == 0)
                    {
                        list = Db.GF_Order.Where(x => x.GolferID == LoginInfo.GolferUserId && x.MemberShipID == filterExpression.Trim())
                            .OrderByDescending(x => x.ID);

                        isFilled = true;
                    }
                    else
                    {
                        //list = from item in list
                        //       where (!string.IsNullOrEmpty(item.MemberShipID) && string.IsNullOrEmpty(item.StripeToken))
                        //       select item;

                        list = list.Where(x => !string.IsNullOrEmpty(x.MemberShipID) && x.PaymentType == "0");

                        isFilled = true;
                    }
                }
                else if (orderType == "P")
                {
                    if (!isFilled)
                    {
                        list = list.Where(x => string.IsNullOrEmpty(x.MemberShipID) && x.PaymentType == "1");

                        isFilled = true;
                    }
                }
            }
            else
            {
                list = Db.GF_Order.Where(x => x.GolferID == LoginInfo.GolferUserId).OrderByDescending(x => x.ID);
            }

            #region Filters

            if (!string.IsNullOrEmpty(fromDate) && !string.IsNullOrEmpty(toDate))
            {
                DateTime dtDate = DateTime.Parse(fromDate);
                DateTime dtToDate = DateTime.Parse(toDate);

                list = list.ToList().Where(x => x.OrderDate.Value.Date >= dtDate.Date
                    && x.OrderDate.Value.Date <= dtToDate.Date).AsQueryable();
            }
            else if (!string.IsNullOrEmpty(fromDate))
            {
                DateTime dtDate = DateTime.Parse(fromDate);

                list = list.ToList().Where(x => x.OrderDate.Value.Date >= dtDate.Date).AsQueryable();
            }
            else if (!string.IsNullOrEmpty(toDate))
            {
                DateTime dtToDate = DateTime.Parse(toDate);
                list = list.ToList().Where(x => x.OrderDate.Value.Date <= dtToDate.Date).AsQueryable();
            }
            else if (!string.IsNullOrEmpty(orderAmount))
            {
                decimal oAmt = 0;
                try
                {
                    oAmt = Convert.ToDecimal(orderAmount);
                }
                catch
                {
                    oAmt = 0;
                }

                list = list.ToList().Where(x => (x.GrandTotal ?? 0) <= oAmt).AsQueryable();
            }
            else if (!string.IsNullOrEmpty(ccNumber))
            {
                list = list.Where(x => !string.IsNullOrEmpty(x.CardNumber)).ToList()
                    .Where(x => Convert.ToString(CommonFunctions.DecryptUrlParam(x.CardNumber)).EndsWith(ccNumber)).AsQueryable();
            }

            if (orderType != "")
            {
                //if (orderType == "R")
                //{
                //    //  var listtemp = list.Where(x => x.GF_OrderRefund.ToList().Count > 0).ToList();
                //    list = list.Where(x => x.GF_OrderRefund.Any());
                //}
                if (orderType == "M")
                {
                    //  var listtemp = list.Where(x => x.GF_OrderRefund.ToList().Count > 0).ToList();
                    if (!isFilled)
                    {
                        //list = from item in list
                        //       where (!string.IsNullOrEmpty(item.MemberShipID) && string.IsNullOrEmpty(item.StripeToken))
                        //       select item;

                        list = list.Where(x => !string.IsNullOrEmpty(x.MemberShipID) && x.PaymentType == "0");
                    }
                }
                else if (orderType == "P")
                {
                    if (!isFilled)
                    {
                        list = list.Where(x => string.IsNullOrEmpty(x.MemberShipID) && x.PaymentType == "1");
                    }
                }
            }

            if ((courseID ?? 0) > 0)
            {
                list = list.Where(x => (x.CourseID ?? 0) == (courseID ?? 0));
            }

            #endregion

            totalRecords = list.Count();

            return list.OrderBy(sortExpression + " " + sortDirection).Skip((pageIndex - 1) * pageSize).Take(pageSize);
        }

        /// <summary>
        /// Created By: Amit Kumar
        /// Created on: 31 March 2015
        /// </summary>
        /// <remarks>Get Latest Order Listing</remarks>
        public IQueryable<OrderApiResult> viewOrders(string filterExpression, string orderType, string orderInclucde, string sortExpression, string sortDirection,
            int pageIndex, int pageSize, ref int totalRecords)
        {
            try
            {
                string url = ConfigClass.CourseApiService;
                string data = "";

                if (LoginInfo.LoginUserType == UserType.CourseAdmin ||
                    LoginInfo.LoginUserType == UserType.Proshop)
                {
                    url = url + CourseApiName.ProshopOrders;
                    data = "ProShopID=" + LoginInfo.UserId;
                }
                else if (LoginInfo.LoginUserType == UserType.Kitchen)
                {
                    url = url + CourseApiName.KitcenOrders;
                    data = "KitchenId=" + LoginInfo.UserId;
                }
                else if (LoginInfo.LoginUserType == UserType.PowerAdmin)
                {
                    url = url + CourseApiName.PowerAdminOrders;
                    data = "ProShopID=" + LoginInfo.UserId;
                }
                else
                {
                    url = url + CourseApiName.CartieOrders;
                    data = "CartieId=" + LoginInfo.UserId;
                }

                //url = ConfigClass.CourseApiService + CourseApiName.CartieOrders;
                //data = "CartieId=130348";

                //string refType = LoginInfo.LoginUserType == UserType.CourseAdmin ? UserType.Proshop : LoginInfo.LoginUserType;
                //string data = "CourseID=" + LoginInfo.CourseId.ToString() + "&referenceID=" + LoginInfo.UserId + "&referenceType=" + refType + "&pageIndex=" + pageIndex + "&pageSize=" + pageSize;
                MyWebRequest myRequest = new MyWebRequest(url, "POST", data);
                var jsonString = myRequest.GetResponse();
                resultSet result = JsonHelper.JsonDeserialize<resultSet>(jsonString);

                List<OrderApiResult> blank = new List<OrderApiResult>();
                IQueryable<OrderApiResult> list = result.record != null ? result.record.AsQueryable() : blank.AsQueryable();

                //if (!String.IsNullOrWhiteSpace(filterExpression))
                //    list = list.Where(x => (x.golferName).ToLower().Contains(filterExpression.ToLower())).OrderByDescending(x => x.orderID);
                //else

                if (string.IsNullOrEmpty(orderInclucde))
                {
                    list = list.Where(x => x.OrderType.Contains(orderType)).OrderByDescending(x => x.orderID);
                }
                else
                {
                    list = list.Where(x => x.OrderType.Contains(orderType) && x.orderInclude.Contains(orderInclucde))
                               .OrderByDescending(x => x.orderID);
                }

                totalRecords = list.Count();

                return list.OrderBy(sortExpression + " " + sortDirection).Skip((pageIndex - 1) * pageSize).Take(pageSize);
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                return null;
            }
        }

        public List<OrderData> getOrderByID(long orderID)
        {
            try
            {
                string timeZone = CommonFunctions.GetCourseTimeZone(LoginInfo.CourseId);

                var lstOrder = Db.GF_Order.Where(y => y.ID == orderID)
                    .ToList()
                    .Select(x =>
                        new OrderData
                        {
                            orderID = x.ID,
                            courseID = x.CourseID,
                            golferID = x.GolferID,
                            golferName = x.GF_Golfer.FirstName + " " + x.GF_Golfer.LastName,
                            time = CommonFunctions.DateByCourseTimeZone(x.CourseID ?? 0, x.OrderDate ?? DateTime.UtcNow).ToShortTimeString(),
                            //time = CommonFunctions.DateByTimeZone(timeZone, x.OrderDate ?? DateTime.UtcNow).ToShortTimeString(),
                            itemsOrdered = x.GF_OrderDetails.ToList().Select(y =>
                                new GF_OrderDetails
                                {
                                    Name = y.GF_MenuItems.Name,
                                    UnitPrice = y.UnitPrice,
                                    Quantity = y.Quantity,
                                    Amount = ((y.UnitPrice ?? 0) * (y.Quantity ?? 0)),
                                    MenuOptionName = string.Join(",", y.GF_OrderMenuOptionDetail.Select(q => q.MenuOptionName).ToList())
                                    //MenuOptionName = y.GF_OrderMenuOptionDetail.Select(q => q.MenuOptionName).ToList()
                                }).ToList(),
                            billAmount = x.GF_OrderDetails.Sum(y => (y.UnitPrice * y.Quantity)),
                            TaxPercentage = x.GF_CourseInfo.Tax ?? 0,
                            TaxAmount = x.TaxAmount ?? 0,
                            GolferPlatformFee = (x.GolferPlatformFee ?? 0),
                            Total = x.GrandTotal ?? 0,
                            OrderType = x.OrderType,
                            OrderDate = CommonFunctions.DateByCourseTimeZone(x.CourseID ?? 0, x.OrderDate ?? DateTime.UtcNow).ToString("MM/dd/yyyy"),
                            //OrderDate = CommonFunctions.DateByTimeZone(timeZone, x.OrderDate ?? DateTime.UtcNow).ToString("MM/dd/yyyy"),
                            ReadyStatus = GetOrderReadyStatus(x.ID, LoginInfo.Type),
                            HEXColor = x.HEXColor,
                            RGBColor = x.RGBColor,
                            HUEColor = x.HUEColor,
                            orderInclude = x.OrderType == OrderType.CartOrder ?
                                            string.Join(",", x.GF_OrderDetails.ToList()
                                                            .Select(y => y.GF_MenuItems.GF_SubCategory.GF_Category.CartType)
                                                            .Distinct().ToList()) + (x.OrderType == OrderType.CartOrder ? ",Cart" : "") :
                                            string.Join(",", x.GF_OrderDetails.ToList()
                                                            .Select(y => y.GF_MenuItems.GF_SubCategory.GF_Category.Type)
                                                            .Distinct().ToList()) + (x.OrderType == OrderType.CartOrder ? ",Cart" : ""),
                            //orderInclude = string.Join(",", x.GF_OrderDetails.ToList()
                            //                                .Select(y => y.GF_MenuItems.GF_SubCategory.GF_Category.Type)
                            //                                .Distinct().ToList()) + (x.OrderType == OrderType.CartOrder ? ",Cart" : ""),
                            DiscountAmt = x.DiscountAmt ?? 0,
                            TimeElapsed = ((long)DateTime.UtcNow.Subtract(Convert.ToDateTime(x.OrderDate)).TotalMinutes).ToString(),
                            IsNew = false
                        }).ToList();

                if (lstOrder.Count() > 0)
                {
                    return lstOrder;
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }

        public List<ActiveCourseUser> getActiveCourseUser()
        {
            var lstActiveCourseUser = (from x in Db.GF_AdminUsers
                                       join y in Db.GF_UserCurrentPosition on x.ID equals y.ReferenceID
                                       where x.Type == y.ReferenceType &&
                                       x.CourseId == LoginInfo.CourseId &&
                                       (x.IsOnline ?? false)
                                       select new { x, y })
                                       .ToList()
                                       .Select(z => new ActiveCourseUser
                                       {
                                           Id = z.x.ID,
                                           Name = z.x.FirstName + " " + z.x.LastName,
                                           UserType = z.x.Type, //UserType.GetFullUserType(z.x.Type),
                                           Latitude = z.y.Latitude,
                                           Longitude = z.y.Longitude
                                       }).ToList();

            return lstActiveCourseUser;
        }

        /// <summary>
        /// Created By: Amit Kumar
        /// Created on: 16 April 2015
        /// </summary>
        /// <remarks>Get Latest Order Listing</remarks>
        public IQueryable<OrderApiResult> viewIncommingOrders(string filterExpression, string sortExpression, string sortDirection,
            int pageIndex, int pageSize, ref int totalRecords)
        {
            try
            {
                string url = ConfigClass.CourseApiService + CourseApiName.CourseOrders;
                string refType = LoginInfo.LoginUserType == UserType.CourseAdmin ? UserType.Proshop : LoginInfo.LoginUserType;
                string data = "CourseID=" + LoginInfo.CourseId.ToString() + "&referenceID=" + LoginInfo.UserId + "&referenceType=" + refType + "&pageIndex=" + pageIndex + "&pageSize=" + pageSize;
                MyWebRequest myRequest = new MyWebRequest(url, "POST", data);
                var jsonString = myRequest.GetResponse();
                resultSet result = JsonHelper.JsonDeserialize<resultSet>(jsonString);

                List<OrderApiResult> blank = new List<OrderApiResult>();
                IQueryable<OrderApiResult> list = result.record != null ? result.record.AsQueryable() : blank.AsQueryable();

                //if (!String.IsNullOrWhiteSpace(filterExpression))
                //    list = list.Where(x => (x.golferName).ToLower().Contains(filterExpression.ToLower())).OrderByDescending(x => x.orderID);
                //else
                list = list.OrderByDescending(x => x.orderID);

                totalRecords = list.Count();

                return list.OrderBy(sortExpression + " " + sortDirection).Skip((pageIndex - 1) * pageSize).Take(pageSize);
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                return null;
            }
        }

        /// <summary>
        /// Created By: Amit Kumar
        /// Created on: 16 April 2015
        /// </summary>
        /// <remarks>Get Latest Order Listing</remarks>
        public long getCurrentDayMissedOrder()
        {
            try
            {
                DateTime TodaysDate = Convert.ToDateTime(DateTime.UtcNow).Date;

                var lstMissedOrder = Db.GF_Order.Where(x => EntityFunctions.TruncateTime(x.OrderDate) == TodaysDate &&
                    (x.IsRejected ?? false));

                return lstMissedOrder.Count();
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                return 0;
            }
        }

        /// <summary>
        /// Created By: Amit Kumar
        /// Created on: 16 April 2015
        /// </summary>
        /// <remarks>Accept/Reject orders by Kitchen/Cartie/Proshop</remarks>
        public resultSet AcceptRejectOrdersStatus(long orderID, string status)
        {
            try
            {
                #region Power Admin

                if (LoginInfo.LoginUserType == UserType.PowerAdmin)
                {
                    return AcceptRejectOrdersByPowerAdmin(orderID, status);
                }

                #endregion

                string url = ConfigClass.CourseApiService + CourseApiName.AcceptRejectOrders;
                string refType = LoginInfo.LoginUserType == UserType.CourseAdmin ? UserType.Proshop : LoginInfo.LoginUserType;
                string data = "OrderID=" + orderID + "&ReferenceID=" + LoginInfo.UserId + "&ReferenceType=" + refType + "&Status=" + status;
                MyWebRequest myRequest = new MyWebRequest(url, "POST", data);
                var jsonString = myRequest.GetResponse();
                resultSet result = JsonHelper.JsonDeserialize<resultSet>(jsonString);

                return result;
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                return new resultSet();
            }
        }

        /// <summary>
        /// Created By: Amit Kumar
        /// Created Date: 02 March 2016
        /// Purpose: Accept/Reject orders by Power Admin (Order Type: Turn Orders(Kitchen/Proshop))
        /// </summary>
        /// <param name="orderID"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public resultSet AcceptRejectOrdersByPowerAdmin(long orderID, string status)
        {
            try
            {
                var order = Db.GF_Order.FirstOrDefault(x => x.ID == orderID);

                string foodCategoryType;

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

                var orderAcceptReject = Db.GF_OrderAcceptReject.FirstOrDefault(x => x.OrderID == orderID &&
                x.ReferenceID == LoginInfo.UserId &&
                x.ReferenceType == LoginInfo.LoginUserType);

                if (orderAcceptReject != null)
                {
                    return new resultSet
                    {
                        Id = LoginInfo.UserId,
                        Status = 1,
                        record = null,
                        Error = (orderAcceptReject.Status ?? false) ? "Order is already accepted." : "Order is already rejected."
                    };
                }
                else
                {
                    if (Convert.ToBoolean(status))
                    {
                        if (foodCategoryType.ToLower() == FoodCategoryType.Proshop.ToLower())
                        {
                            if (order.OrderType == OrderType.CartOrder)
                            {
                                order.CartieId = LoginInfo.UserId;
                            }

                            order.ProShopID = LoginInfo.UserId;
                        }
                        else if (foodCategoryType.ToLower() == FoodCategoryType.Kitchen.ToLower())
                        {
                            if (order.OrderType == OrderType.CartOrder)
                            {
                                order.CartieId = LoginInfo.UserId;
                            }

                            order.KitchenId = LoginInfo.UserId;
                        }
                        else if (foodCategoryType.ToLower() == FoodCategoryType.Cart.ToLower())
                        {
                            order.CartieId = LoginInfo.UserId;
                        }
                        else if (foodCategoryType.ToLower().Contains(FoodCategoryType.Proshop.ToLower()) &&
                                 foodCategoryType.ToLower().Contains(FoodCategoryType.Kitchen.ToLower()))
                        {
                            if (order.OrderType == OrderType.CartOrder)
                            {
                                order.CartieId = LoginInfo.UserId;
                            }
                            order.ProShopID = LoginInfo.UserId;
                            order.KitchenId = LoginInfo.UserId;
                        }
                        else if (foodCategoryType.ToLower().Contains(FoodCategoryType.Cart.ToLower()) &&
                                 foodCategoryType.ToLower().Contains(FoodCategoryType.Kitchen.ToLower()))
                        {
                            order.CartieId = LoginInfo.UserId;
                            order.KitchenId = LoginInfo.UserId;
                        }
                        else if (foodCategoryType.ToLower().Contains(FoodCategoryType.Proshop.ToLower()) &&
                                 foodCategoryType.ToLower().Contains(FoodCategoryType.Cart.ToLower()))
                        {
                            order.ProShopID = LoginInfo.UserId;
                            order.CartieId = LoginInfo.UserId;
                        }
                        else if (foodCategoryType.ToLower().Contains(FoodCategoryType.Proshop.ToLower()) &&
                                 foodCategoryType.ToLower().Contains(FoodCategoryType.Cart.ToLower()) &&
                                 foodCategoryType.ToLower().Contains(FoodCategoryType.Kitchen.ToLower()))
                        {
                            order.ProShopID = LoginInfo.UserId;
                            order.CartieId = LoginInfo.UserId;
                            order.KitchenId = LoginInfo.UserId;
                        }

                        order.OrderStatus = OrderStatus.Process;
                        order.ModifyDate = DateTime.UtcNow;
                    }
                    else
                    {
                        order.OrderStatus = OrderStatus.Reject;
                        order.IsRejected = true;
                        order.ModifyDate = DateTime.UtcNow;
                    }

                    Db.SaveChanges();

                    #region Push Notification

                    bool IsMessageToGolfer = true;

                    PushNotications pushNotications = new PushNotications();

                    var senderName = Db.GF_AdminUsers.FirstOrDefault(x => x.ID == LoginInfo.UserId);// && x.Type == objOrder.referenceType);

                    pushNotications.SenderId = LoginInfo.UserId;
                    pushNotications.SenderName = senderName.FirstName + " " + senderName.LastName;

                    pushNotications.ReceiverId = order.GolferID ?? 0;
                    pushNotications.ReceiverName = order.GF_Golfer.FirstName + " " + order.GF_Golfer.LastName;
                    pushNotications.pushMsgFrom = PushnoficationMsgFrom.Course;
                    pushNotications.DeviceType = order.GF_Golfer.DeviceType;

                    if ((order.GF_Golfer.AppVersion ?? 0) > 0)
                    {
                        var jString = new
                        {
                            ScreenName = AppScreenName.EnRoute,
                            Message = Convert.ToBoolean(status) ? "Your order has gone for processing." : "Your order is cancelled due to item is not avaiable.",
                            Data = new { OrderID = order.ID.ToString() }
                        };
                        string jsonString = JsonConvert.SerializeObject(jString);
                        if (pushNotications.DeviceType.ToLower() == "ios")
                        {
                            pushNotications.Message = Convert.ToBoolean(status) ? "Your order has gone for processing." : "Your order is cancelled due to item is not avaiable.";
                            pushNotications.iosMessageJson = jsonString;
                        }
                        else
                        {
                            pushNotications.Message = jsonString;
                        }
                    }
                    else
                    {
                        if (pushNotications.DeviceType.ToLower() == "ios")
                        {
                            pushNotications.Message = Convert.ToBoolean(status) ? "Your order has gone for processing." : "Your order is cancelled due to item is not avaiable.";
                        }
                        else
                        {
                            pushNotications.Message = Convert.ToBoolean(status) ? "\"Your order has gone for processing.\"" : "\"Your order is cancelled due to item is not avaiable.\"";
                        }
                    }
                    IsMessageToGolfer = true;

                    SendRecieveNotification.callingPushNotification(pushNotications, IsMessageToGolfer);

                    #endregion

                    return new resultSet
                    {
                        Id = LoginInfo.UserId,
                        Status = 1,
                        Error = (Convert.ToBoolean(status)) ? "Order is accepted successfully." : "Order is rejected successfully.",
                        record = null
                    };
                }

                resultSet result = new resultSet()
                {
                    Id = 0,
                    Error = "",
                    record = null,
                    Status = 1
                };

                return result;
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                return new resultSet();
            }
        }

        /// <summary>
        /// Created By: Amit Kumar
        /// Created on: 18 May 2015
        /// </summary>
        /// <remarks>Pickedup orders status by Kitchen/Proshop</remarks>
        public resultSet PickupOrdersStatus(long orderID)
        {
            try
            {
                string url = ConfigClass.CourseApiService + CourseApiName.OrderPickupStatus;
                string refType = LoginInfo.LoginUserType == UserType.CourseAdmin ? UserType.Proshop : LoginInfo.LoginUserType;
                string data = "OrderId=" + orderID;
                MyWebRequest myRequest = new MyWebRequest(url, "POST", data);
                var jsonString = myRequest.GetResponse();
                resultSet result = JsonHelper.JsonDeserialize<resultSet>(jsonString);

                return result;
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                return new resultSet();
            }
        }

        /// <summary>
        /// Created By: Amit Kumar
        /// Created on: 18 May 2015
        /// </summary>
        /// <remarks>Delivery status orders by Cartie</remarks>
        public resultSet DeliveryOrdersStatus(long orderID)
        {
            try
            {
                string url = ConfigClass.CourseApiService + CourseApiName.OrderDeliveryStatusByCartie;
                string refType = LoginInfo.LoginUserType == UserType.CourseAdmin ? UserType.Proshop : LoginInfo.LoginUserType;
                string data = "OrderId=" + orderID + "&CartieId=" + LoginInfo.UserId;
                MyWebRequest myRequest = new MyWebRequest(url, "POST", data);
                var jsonString = myRequest.GetResponse();
                resultSet result = JsonHelper.JsonDeserialize<resultSet>(jsonString);

                return result;
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                return new resultSet();
            }
        }

        /// <summary>
        /// Created By: Amit Kumar
        /// Created on: 16 April 2015
        /// </summary>
        /// <remarks>Orders Ready status by Kitchen/Cartie/Proshop</remarks>
        public resultSet changeOrderStatus(long orderID)
        {
            try
            {
                #region Power Admin

                if (LoginInfo.LoginUserType == UserType.PowerAdmin)
                {
                    return changeOrderStatusByPowerAdmin(orderID);
                }

                #endregion

                string url = ConfigClass.CourseApiService + CourseApiName.OrderReadyStatus;
                string refType = LoginInfo.LoginUserType == UserType.CourseAdmin ? UserType.Proshop : LoginInfo.LoginUserType;
                string data = "OrderId=" + orderID + "&referenceID=" + LoginInfo.UserId + "&referenceType=" + refType;
                MyWebRequest myRequest = new MyWebRequest(url, "POST", data);
                var jsonString = myRequest.GetResponse();
                resultSet result = JsonHelper.JsonDeserialize<resultSet>(jsonString);

                return result;
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                return new resultSet();
            }
        }

        /// <summary>
        /// Created By: Amit Kumar
        /// Created on: 09 March 2015
        /// </summary>
        /// <remarks>Orders Ready status by Power Admin</remarks>
        public resultSet changeOrderStatusByPowerAdmin(long orderID)
        {
            try
            {
                var order = Db.GF_Order.FirstOrDefault(x => x.ID == orderID);

                string foodCategoryType;

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

                var chkOrder = Db.GF_Order.FirstOrDefault(x => x.ID == orderID &&
                           ((x.IsReadyByKitchen ?? false) || (x.IsReadyByProshop ?? false)));

                if (chkOrder != null)
                {
                    return new resultSet
                    {
                        Id = orderID,
                        Status = 1,
                        record = null,
                        Error = "Order is already ready."
                    };
                }
                else
                {
                    if (foodCategoryType.ToLower() == FoodCategoryType.Proshop.ToLower())
                    {
                        order.ProShopID = LoginInfo.UserId;
                        order.IsReadyByProshop = true;
                    }
                    else if (foodCategoryType.ToLower() == FoodCategoryType.Kitchen.ToLower())
                    {
                        order.KitchenId = LoginInfo.UserId;
                        order.IsReadyByKitchen = true;
                    }
                    else if (foodCategoryType.ToLower() == FoodCategoryType.Cart.ToLower())
                    {
                        order.CartieId = LoginInfo.UserId;
                    }
                    else if (foodCategoryType.ToLower().Contains(FoodCategoryType.Proshop.ToLower()) &&
                             foodCategoryType.ToLower().Contains(FoodCategoryType.Kitchen.ToLower()))
                    {
                        order.ProShopID = LoginInfo.UserId;
                        order.IsReadyByProshop = true;

                        order.KitchenId = LoginInfo.UserId;
                        order.IsReadyByKitchen = true;
                    }
                    else if (foodCategoryType.ToLower().Contains(FoodCategoryType.Proshop.ToLower()) &&
                             foodCategoryType.ToLower().Contains(FoodCategoryType.Cart.ToLower()))
                    {
                        order.ProShopID = LoginInfo.UserId;
                        order.IsReadyByProshop = true;

                        order.CartieId = LoginInfo.UserId;
                    }
                    else if (foodCategoryType.ToLower().Contains(FoodCategoryType.Cart.ToLower()) &&
                             foodCategoryType.ToLower().Contains(FoodCategoryType.Kitchen.ToLower()))
                    {
                        order.CartieId = LoginInfo.UserId;

                        order.KitchenId = LoginInfo.UserId;
                        order.IsReadyByKitchen = true;
                    }
                    else if (foodCategoryType.ToLower().Contains(FoodCategoryType.Cart.ToLower()) &&
                             foodCategoryType.ToLower().Contains(FoodCategoryType.Kitchen.ToLower()) &&
                             foodCategoryType.ToLower().Contains(FoodCategoryType.Proshop.ToLower()))
                    {
                        order.CartieId = LoginInfo.UserId;

                        order.KitchenId = LoginInfo.UserId;
                        order.IsReadyByKitchen = true;

                        order.ProShopID = LoginInfo.UserId;
                        order.IsReadyByProshop = true;
                    }

                    order.OrderStatus = OrderStatus.Ready;
                    order.ModifyDate = DateTime.UtcNow;
                    Db.SaveChanges();

                    #region Push Notification

                    bool IsMessageToGolfer = true;

                    PushNotications pushNotications = new PushNotications();

                    var senderName = Db.GF_AdminUsers.FirstOrDefault(x => x.ID == LoginInfo.UserId);// && x.Type == objOrder.referenceType);

                    pushNotications.SenderId = LoginInfo.UserId;
                    pushNotications.SenderName = senderName.FirstName + " " + senderName.LastName;

                    pushNotications.ReceiverId = order.GolferID ?? 0;
                    pushNotications.ReceiverName = order.GF_Golfer.FirstName + " " + order.GF_Golfer.LastName;
                    pushNotications.pushMsgFrom = PushnoficationMsgFrom.Course;
                    pushNotications.DeviceType = order.GF_Golfer.DeviceType;

                    if ((order.GF_Golfer.AppVersion ?? 0) > 0)
                    {
                        var jString = new
                        {
                            ScreenName = AppScreenName.EnRoute,
                            Message = "Order is ready to be picked up.",
                            Data = new { OrderID = order.ID.ToString() }
                        };
                        string jsonString = JsonConvert.SerializeObject(jString);
                        if (pushNotications.DeviceType.ToLower() == "ios")
                        {
                            pushNotications.Message = "Order is ready to be picked up.";
                            pushNotications.iosMessageJson = jsonString;
                        }
                        else
                        {
                            pushNotications.Message = jsonString;
                        }
                    }
                    else
                    {
                        if (pushNotications.DeviceType.ToLower() == "ios")
                        {
                            pushNotications.Message = "Order is ready to be picked up.";
                        }
                        else
                        {
                            pushNotications.Message = "\"Order is ready to be picked up.\"";
                        }
                    }
                    IsMessageToGolfer = true;

                    SendRecieveNotification.callingPushNotification(pushNotications, IsMessageToGolfer);

                    #endregion

                    return new resultSet
                    {
                        Id = order.ID,
                        Status = 1,
                        Error = "Order ready successfully."
                    };
                }

                resultSet result = new resultSet()
                {
                    Id = 0,
                    Error = "",
                    record = null,
                    Status = 1
                };

                return result;
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                return new resultSet();
            }
        }

        //public List<GF_Order> GetOrderDetails(long orderID)
        //{
        //    try
        //    {
        //        var lstOrder = Db.GF_Order.Where(x => x.ID == orderID).ToList();

        //        if (lstOrder.Count() > 0)
        //        {
        //            lstOrder = lstOrder.Select(x => new
        //            {
        //                x.ID

        //            });

        //            return lstOrder;
        //        }

        //        return new List<GF_Order>();
        //    }
        //    catch
        //    {
        //        return new List<GF_Order>();
        //    }
        //}

        public bool changeOrderStatus(long orderID, ref string Message)
        {
            if (orderID > 0)
            {
                GF_Order order = new GF_Order();
                order = Db.GF_Order.FirstOrDefault(x => x.ID == orderID);

                if (LoginInfo.LoginUserType == UserType.CourseAdmin || LoginInfo.LoginUserType == UserType.Proshop)
                {
                    if (order.ProShopID > 0)
                    {
                        Message = "This order is already taken by another Pro-Shop user.";
                        return false;
                    }
                    else
                    {
                        order.ProShopID = LoginInfo.UserId;
                        Message = "Order has been successfully selected.";
                    }
                }
                else if (LoginInfo.LoginUserType == UserType.Cartie)
                {
                    if (order.ProShopID > 0)
                    {
                        Message = "This order is already taken by another cartie user.";
                        return false;
                    }
                    else
                    {
                        order.CartieId = LoginInfo.UserId;
                        Message = "Order has been successfully selected.";
                    }
                }
                else if (LoginInfo.LoginUserType == UserType.Kitchen)
                {
                    if (order.ProShopID > 0)
                    {
                        Message = "This order is already taken by another kitchen user.";
                        return false;
                    }
                    else
                    {
                        order.KitchenId = LoginInfo.UserId;
                        Message = "Order has been successfully selected.";
                    }
                }
                else
                {
                    Message = "invalid Request.";
                    return false;
                }

                order.ModifyBy = LoginInfo.UserId.ToString();
                order.ModifyDate = DateTime.Now;
                Db.SaveChanges();

                return true;
            }
            else
            {
                Message = "invalid Request.";
                return false;
            }
        }

        /// <summary>
        /// Created By: Veera
        /// Created on: 14 April 2015
        /// </summary>
        /// <remarks>Get Order Listing</remarks>
        public IQueryable<GF_Order> GetOrdersForRefund(string filterExpression, string sortExpression, string sortDirection, int pageIndex, int pageSize, ref int totalRecords, string fromDate, string toDate, string courseId)
        {
            IQueryable<GF_Order> list = null;
            long _courseid = Convert.ToInt64(courseId);
            if (!String.IsNullOrWhiteSpace(filterExpression))
            {
                //
                bool IsOrderId = false;
                try
                {
                    long searchedByOrderId = Convert.ToInt64(filterExpression.Trim());
                    var objOrder = Db.GF_Order.FirstOrDefault(x => x.ID == searchedByOrderId);
                    if (objOrder != null)
                    {
                        list = Db.GF_Order.Where(x => x.ID == searchedByOrderId && x.CourseID == _courseid && string.IsNullOrEmpty(x.BT_TransId) == false).OrderByDescending(x => x.ID);
                        IsOrderId = true;
                    }
                    else
                    {
                        list = null;
                    }
                }
                catch
                {
                    IsOrderId = false;
                }
                if (!IsOrderId)
                {
                    list = Db.GF_Order.Where(x => ((x.GF_Golfer.FirstName).ToLower().Contains(filterExpression.ToLower()) || (x.GF_Golfer.LastName).ToLower().Contains(filterExpression.ToLower())) &&
                                     x.CourseID == _courseid && string.IsNullOrEmpty(x.BT_TransId) == false).OrderByDescending(x => x.ID);
                }

            }
            else
            {
                list = Db.GF_Order.Where(x => x.CourseID == _courseid && string.IsNullOrEmpty(x.BT_TransId) == false).OrderByDescending(x => x.ID);
            }
            if (fromDate != "" && toDate == "")
            {
                DateTime dtFrom = Convert.ToDateTime(fromDate);

                list = list.Where(i => EntityFunctions.TruncateTime(i.OrderDate) >= EntityFunctions.TruncateTime(dtFrom));

            }
            else if (fromDate == "" && toDate != "")
            {

                DateTime dtTo = Convert.ToDateTime(toDate);
                list = list.Where(i => EntityFunctions.TruncateTime(i.OrderDate) <= EntityFunctions.TruncateTime(dtTo));

            }
            else if (fromDate != "" && toDate != "")
            {
                DateTime dtFrom = Convert.ToDateTime(fromDate);
                DateTime dtTo = Convert.ToDateTime(toDate);
                list = list.Where(i => (EntityFunctions.TruncateTime(i.OrderDate) >= EntityFunctions.TruncateTime(dtFrom)) && (EntityFunctions.TruncateTime(i.OrderDate) <= EntityFunctions.TruncateTime(dtTo)));

            }
            totalRecords = list.Count();


            return list.OrderBy(sortExpression + " " + sortDirection).Skip((pageIndex - 1) * pageSize).Take(pageSize);
        }

        /// <summary>
        /// Created By: Veera
        /// Created on: 14 April 2015
        /// </summary>
        /// <remarks>Get Order Listing</remarks>
        public IQueryable<GF_OrderRefund> GetRefundedOrders(string filterExpression, string sortExpression, string sortDirection, int pageIndex, int pageSize, ref int totalRecords, string fromDate, string toDate, string courseId)
        {
            IQueryable<GF_OrderRefund> list = null;
            long _courseid = Convert.ToInt64(courseId);
            if (!String.IsNullOrWhiteSpace(filterExpression))
            {

                list = Db.GF_OrderRefund.Where(x => (x.GF_Order.CourseID == _courseid)).OrderByDescending(x => x.ID);
                //
                bool IsOrderId = false;
                try
                {
                    long searchedByOrderId = Convert.ToInt64(filterExpression.Trim());
                    var objOrder = Db.GF_Order.FirstOrDefault(x => x.ID == searchedByOrderId);
                    if (objOrder != null)
                    {
                        list = Db.GF_OrderRefund.Where(x => (x.GF_Order.CourseID == _courseid && x.OrderId == searchedByOrderId)).OrderByDescending(x => x.ID);
                        IsOrderId = true;
                    }
                    else
                    {
                        list = null;
                    }
                }
                catch
                {
                    IsOrderId = false;
                }
                if (!IsOrderId)
                {
                    list = Db.GF_OrderRefund.Where(x => (x.GF_Order.CourseID == _courseid)
                            && ((x.GF_Order.GF_Golfer.FirstName).ToLower().Contains(filterExpression.ToLower()) || ((x.GF_Order.GF_Golfer.LastName).ToLower().Contains(filterExpression.ToLower()))))
                          .OrderByDescending(x => x.ID);
                }
            }
            else
            {
                list = Db.GF_OrderRefund.Where(x => (x.GF_Order.CourseID == _courseid)).OrderByDescending(x => x.ID);
            }
            if (fromDate != "" && toDate == "")
            {
                DateTime dtFrom = Convert.ToDateTime(fromDate);

                list = list.Where(i => EntityFunctions.TruncateTime(i.CreatedDate) >= EntityFunctions.TruncateTime(dtFrom));

            }
            else if (fromDate == "" && toDate != "")
            {

                DateTime dtTo = Convert.ToDateTime(toDate);
                list = list.Where(i => EntityFunctions.TruncateTime(i.CreatedDate) <= EntityFunctions.TruncateTime(dtTo));

            }
            else if (fromDate != "" && toDate != "")
            {
                DateTime dtFrom = Convert.ToDateTime(fromDate);
                DateTime dtTo = Convert.ToDateTime(toDate);
                list = list.Where(i => (EntityFunctions.TruncateTime(i.CreatedDate) >= EntityFunctions.TruncateTime(dtFrom)) && (EntityFunctions.TruncateTime(i.CreatedDate) <= EntityFunctions.TruncateTime(dtTo)));
            }
            totalRecords = list.Count();

            return list.OrderBy(sortExpression + " " + sortDirection).Skip((pageIndex - 1) * pageSize).Take(pageSize);

        }

        /// <summary>
        /// Created By: Veera
        /// Created on: 9 May 2015
        /// </summary>
        /// <remarks>Get Rating Listing</remarks>
        public IQueryable<GF_GolferRating> GetRatingsList(string filterExpression, string sortExpression, string sortDirection, int pageIndex, int pageSize, ref int totalRecords, string fromDate, string toDate)
        {
            IQueryable<GF_GolferRating> list = null;

            #region Get Course IDs

            string cIds = string.Join(",", Db.GF_CourseInfo.Where(x => x.Status != StatusType.Delete && (x.ClubHouseID == LoginInfo.CourseId ||
                x.ID == LoginInfo.CourseId)).Select(v => v.ID).ToArray());
            long[] courseIDs = CommonFunctions.ConvertStringArrayToLongArray(cIds);

            #endregion

            if (!String.IsNullOrWhiteSpace(filterExpression))
            {
                long _refID = 0;
                if (filterExpression.Contains("@"))
                {
                    try
                    {
                        _refID = Db.GF_AdminUsers.FirstOrDefault(x => x.Email == filterExpression.Trim()).ID;
                    }
                    catch
                    {
                        _refID = 0;
                    }
                    list = Db.GF_GolferRating.Where(x => courseIDs.Contains(x.CourseId ?? 0) &&// == LoginInfo.CourseId &&
                        x.ReferenceID == _refID).OrderByDescending(x => x.ID);
                }
                else
                {
                    try
                    {
                        var values = Db.GF_AdminUsers.Where(x => x.FirstName.Contains(filterExpression.Trim())).Select(x => x.ID).Distinct().ToList();
                        string ids = string.Join(",", values.Select(v => v.ToString()).ToArray());
                        long[] IDs = CommonFunctions.ConvertStringArrayToLongArray(ids);
                        list = Db.GF_GolferRating.Where(x => IDs.Contains(x.ReferenceID ?? 0) && courseIDs.Contains(x.CourseId ?? 0));//x.CourseId == LoginInfo.CourseId);
                    }
                    catch
                    {
                        list = null;
                    }

                }

            }
            else
            {
                list = Db.GF_GolferRating.Where(x => courseIDs.Contains(x.CourseId ?? 0))//x.CourseId == LoginInfo.CourseId)
                    .OrderByDescending(x => x.ID);
            }
            if (fromDate != "" && toDate == "")
            {
                DateTime dtFrom = Convert.ToDateTime(fromDate);

                list = list.Where(i => EntityFunctions.TruncateTime(i.CreatedDate) >= EntityFunctions.TruncateTime(dtFrom));

            }
            else if (fromDate == "" && toDate != "")
            {

                DateTime dtTo = Convert.ToDateTime(toDate);
                list = list.Where(i => EntityFunctions.TruncateTime(i.CreatedDate) <= EntityFunctions.TruncateTime(dtTo));

            }
            else if (fromDate != "" && toDate != "")
            {
                DateTime dtFrom = Convert.ToDateTime(fromDate);
                DateTime dtTo = Convert.ToDateTime(toDate);
                list = list.Where(i => (EntityFunctions.TruncateTime(i.CreatedDate) >= EntityFunctions.TruncateTime(dtFrom)) &&
                    (EntityFunctions.TruncateTime(i.CreatedDate) <= EntityFunctions.TruncateTime(dtTo)));

            }
            totalRecords = list.Count();


            return list.OrderBy(sortExpression + " " + sortDirection).Skip((pageIndex - 1) * pageSize).Take(pageSize);
        }

        /// <summary>
        /// Created By: Veera
        /// Created on: 18 May 2015
        /// </summary>
        /// <remarks>Get Missing Order Listing</remarks>
        public IQueryable<object> GetMissingOrders(string filterExpression, string fromDate, string toDate, string type, string sortExpression, string sortDirection, int pageIndex, int pageSize, ref int totalRecords)
        {
            IQueryable<GF_Order> list;

            if (!String.IsNullOrWhiteSpace(filterExpression))
            {
                list = Db.GF_Order.Where(x => (x.GF_Golfer.FirstName + " " + x.GF_Golfer.LastName).ToLower().Contains(filterExpression.ToLower()) &&
                                               x.CourseID == LoginInfo.CourseId && (x.IsRejected ?? false)).OrderByDescending(x => x.ID);

            }
            else
            {
                list = Db.GF_Order.Where(x => x.CourseID == LoginInfo.CourseId && (x.IsRejected ?? false)).OrderByDescending(x => x.ID);
            }

            #region Filters

            if (!string.IsNullOrEmpty(type))
            {
                if (type != "0")
                {
                    if (type == "R")
                    {
                        list = list.ToList().Where(x => x.OrderStatus.ToLower() == "order rejected").AsQueryable();
                    }
                    if (type == "C")
                    {
                        list = list.ToList().Where(x => x.OrderStatus.ToLower() == "order cancelled").AsQueryable();
                    }
                }
            }

            if (!string.IsNullOrEmpty(fromDate) && !string.IsNullOrEmpty(toDate))
            {
                DateTime dtDate = DateTime.Parse(fromDate);
                DateTime dtToDate = DateTime.Parse(toDate);

                list = list.ToList().Where(x => x.OrderDate.Value.Date >= dtDate.Date
                    && x.OrderDate.Value.Date <= dtToDate.Date).AsQueryable();
            }
            else if (!string.IsNullOrEmpty(fromDate))
            {
                DateTime dtDate = DateTime.Parse(fromDate);

                list = list.ToList().Where(x => x.OrderDate.Value.Date >= dtDate.Date).AsQueryable();
            }
            else if (!string.IsNullOrEmpty(toDate))
            {
                DateTime dtToDate = DateTime.Parse(toDate);
                list = list.ToList().Where(x => x.OrderDate.Value.Date <= dtToDate.Date).AsQueryable();
            }

            #endregion


            totalRecords = list.Count();

            string timeZone = CommonFunctions.GetCourseTimeZone(LoginInfo.CourseId);
            IQueryable<object> lst = list.ToList().Select((x =>
                        new
                        {
                            x.ID,
                            OrderDate = CommonFunctions.DateByTimeZone(timeZone, x.OrderDate ?? DateTime.UtcNow).ToString("MM/dd/yyyy hh:mm tt"),
                            time = CommonFunctions.DateByTimeZone(timeZone, x.OrderDate ?? DateTime.UtcNow).ToShortTimeString(),//x.OrderDate.Value.ToShortTimeString(),
                            golferName = x.GF_Golfer.FirstName + " " + x.GF_Golfer.LastName,
                            billAmount = x.GF_OrderDetails.Sum(y => ((y.UnitPrice ?? 0) * (y.Quantity ?? 0))).ToString("F"),
                            TaxAmount = (x.TaxAmount ?? 0).ToString("F"),
                            GolferPlatformFee = (x.GolferPlatformFee ?? 0).ToString("F"),
                            GrandTotal = x.GrandTotal ?? 0,
                            itemOrdered = new JavaScriptSerializer().Serialize(x.GF_OrderDetails.ToList().Select(y =>
                                                                               new
                                                                               {
                                                                                   y.GF_MenuItems.Name,
                                                                                   UnitPrice = y.GF_MenuItems.Amount,
                                                                                   y.Quantity,
                                                                                   Amount = (y.GF_MenuItems.Amount * y.Quantity),
                                                                                   MenuOptionName = y.GF_OrderMenuOptionDetail.Select(q => q.MenuOptionName).ToList()
                                                                               })),
                            OrderType = OrderType.GetFullOrderType(x.OrderType),
                            PaymentMode = (x.PaymentType == "1" ? "Payment by Card" : "Payment by MembershipId"),
                            CourseInfo = x.GF_CourseInfo.COURSE_NAME,
                            CourseAddress = x.GF_CourseInfo.ADDRESS,
                            OrderStatus = x.OrderStatus,
                            TransId = x.PaymentType == "0" ? x.MemberShipID : ((x.BT_TransId == null ? "" : x.BT_TransId)),
                            PromoCode = x.GF_PromoCode == null ? "0.00" : (x.DiscountAmt ?? 0).ToString("F"),
                            golferEmail = x.GF_Golfer.Email
                        }
                    )).AsQueryable();

            return lst.OrderBy(sortExpression + " " + sortDirection).Skip((pageIndex - 1) * pageSize).Take(pageSize);
        }

        #region Order Ready Status By ID

        public int GetOrderReadyStatus(long orderID, string type)
        {
            var order = Db.GF_Order.FirstOrDefault(x => x.ID == orderID);

            string foodCategoryType;

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
                                                         .Distinct().ToList());// +(order.OrderType == OrderType.CartOrder ? ",Cart" : "");
            }

            if (type == UserType.Proshop)
            {
                if (foodCategoryType.ToLower() == FoodCategoryType.Proshop.ToLower())
                {
                    return (order.ProShopID > 0 && (order.IsReadyByProshop ?? false) ? 1 : 0);
                }

                if (foodCategoryType.ToLower().Contains(FoodCategoryType.Proshop.ToLower()) &&
                    foodCategoryType.ToLower().Contains(FoodCategoryType.Cart.ToLower()) &&
                    foodCategoryType.ToLower().Contains(FoodCategoryType.Kitchen.ToLower()))
                {
                    return (order.ProShopID > 0 && (order.IsReadyByProshop ?? false) ? 1 : 0);
                }
                else if (foodCategoryType.ToLower().Contains(FoodCategoryType.Proshop.ToLower()) &&
                         foodCategoryType.ToLower().Contains(FoodCategoryType.Cart.ToLower()))
                {
                    return (order.ProShopID > 0 && (order.IsReadyByProshop ?? false) ? 1 : 0);
                }
                else if (foodCategoryType.ToLower().Contains(FoodCategoryType.Proshop.ToLower()) &&
                         foodCategoryType.ToLower().Contains(FoodCategoryType.Kitchen.ToLower()))
                {
                    return (order.ProShopID > 0 && (order.IsReadyByProshop ?? false) ? 1 : 0);
                }
                else
                {
                    return 0;
                }
            }
            else if (type == UserType.Cartie)
            {
                if (foodCategoryType.ToLower() == FoodCategoryType.Cart.ToLower())
                {
                    //return 1;
                    return ((order.CartieId ?? 0) > 0 ? 1 : 0);
                }

                foodCategoryType = foodCategoryType + (order.OrderType == OrderType.CartOrder ? ",Cart" : "");

                if (foodCategoryType.ToLower().Contains(FoodCategoryType.Proshop.ToLower()) &&
                    foodCategoryType.ToLower().Contains(FoodCategoryType.Cart.ToLower()) &&
                    foodCategoryType.ToLower().Contains(FoodCategoryType.Kitchen.ToLower()))
                {
                    int ready = ((order.ProShopID > 0 && (order.IsReadyByProshop ?? false)) &&
                                 (order.KitchenId > 0 && (order.IsReadyByKitchen ?? false))) ? 1 : 0;
                    return ready;
                }
                else if (foodCategoryType.ToLower().Contains(FoodCategoryType.Proshop.ToLower()) &&
                         foodCategoryType.ToLower().Contains(FoodCategoryType.Cart.ToLower()))
                {
                    return (order.ProShopID > 0 && (order.IsReadyByProshop ?? false) ? 1 : 0);
                }
                else if (foodCategoryType.ToLower().Contains(FoodCategoryType.Cart.ToLower()) &&
                         foodCategoryType.ToLower().Contains(FoodCategoryType.Kitchen.ToLower()))
                {
                    return (order.KitchenId > 0 && (order.IsReadyByKitchen ?? false) ? 1 : 0);
                }
                else
                {
                    return 0;
                }
            }
            else if (type == UserType.Kitchen)
            {
                if (foodCategoryType.ToLower() == FoodCategoryType.Kitchen.ToLower())
                {
                    return (order.KitchenId > 0 && (order.IsReadyByKitchen ?? false) ? 1 : 0);
                }

                if (foodCategoryType.ToLower().Contains(FoodCategoryType.Proshop.ToLower()) &&
                    foodCategoryType.ToLower().Contains(FoodCategoryType.Cart.ToLower()) &&
                    foodCategoryType.ToLower().Contains(FoodCategoryType.Kitchen.ToLower()))
                {
                    return (order.KitchenId > 0 && (order.IsReadyByKitchen ?? false) ? 1 : 0);
                }
                else if (foodCategoryType.ToLower().Contains(FoodCategoryType.Proshop.ToLower()) &&
                         foodCategoryType.ToLower().Contains(FoodCategoryType.Kitchen.ToLower()))
                {
                    return (order.KitchenId > 0 && (order.IsReadyByKitchen ?? false) ? 1 : 0);
                }
                else if (foodCategoryType.ToLower().Contains(FoodCategoryType.Cart.ToLower()) &&
                         foodCategoryType.ToLower().Contains(FoodCategoryType.Kitchen.ToLower()))
                {
                    return (order.KitchenId > 0 && (order.IsReadyByKitchen ?? false) ? 1 : 0);
                }
                else
                {
                    return 0;
                }
            }
            else if (type == UserType.PowerAdmin)
            {
                if (foodCategoryType.ToLower() == FoodCategoryType.Proshop.ToLower())
                {
                    return (order.ProShopID > 0 && (order.IsReadyByProshop ?? false) ? 1 : 0);
                }
                else if (foodCategoryType.ToLower() == FoodCategoryType.Kitchen.ToLower())
                {
                    return (order.KitchenId > 0 && (order.IsReadyByKitchen ?? false) ? 1 : 0);
                }
                else if (foodCategoryType.ToLower() == FoodCategoryType.Cart.ToLower())
                {
                    return ((order.CartieId ?? 0) > 0 ? 1 : 0);
                }
                else if (foodCategoryType.ToLower().Contains(FoodCategoryType.Proshop.ToLower()) &&
                         foodCategoryType.ToLower().Contains(FoodCategoryType.Kitchen.ToLower()))
                {
                    var kitchen = (order.KitchenId > 0 && (order.IsReadyByKitchen ?? false) ? 1 : 0);
                    var proshop = (order.ProShopID > 0 && (order.IsReadyByProshop ?? false) ? 1 : 0);
                    return (kitchen == proshop ? 1 : 0);
                }
                else if (foodCategoryType.ToLower().Contains(FoodCategoryType.Cart.ToLower()) &&
                         foodCategoryType.ToLower().Contains(FoodCategoryType.Kitchen.ToLower()))
                {
                    var kitchen = (order.KitchenId > 0 && (order.IsReadyByKitchen ?? false) ? 1 : 0);
                    var cart = ((order.CartieId ?? 0) > 0 ? 1 : 0);
                    return (kitchen == cart ? 1 : 0);
                }
                else if (foodCategoryType.ToLower().Contains(FoodCategoryType.Proshop.ToLower()) &&
                         foodCategoryType.ToLower().Contains(FoodCategoryType.Cart.ToLower()))
                {
                    var cart = ((order.CartieId ?? 0) > 0 ? 1 : 0);
                    var proshop = (order.ProShopID > 0 && (order.IsReadyByProshop ?? false) ? 1 : 0);
                    return (cart == proshop ? 1 : 0);
                }
                else if (foodCategoryType.ToLower().Contains(FoodCategoryType.Proshop.ToLower()) &&
                         foodCategoryType.ToLower().Contains(FoodCategoryType.Cart.ToLower()) &&
                         foodCategoryType.ToLower().Contains(FoodCategoryType.Kitchen.ToLower()))
                {
                    var cart = ((order.CartieId ?? 0) > 0 ? 1 : 0);
                    var proshop = (order.ProShopID > 0 && (order.IsReadyByProshop ?? false) ? 1 : 0);
                    var kitchen = (order.KitchenId > 0 && (order.IsReadyByKitchen ?? false) ? 1 : 0);
                    return ((cart == proshop) && (proshop == kitchen) ? 1 : 0);
                }
                else
                {
                    return 0;
                }
                //if (foodCategoryType.ToLower() == FoodCategoryType.Proshop.ToLower())
                //{
                //    return (order.ProShopID > 0 && (order.IsReadyByProshop ?? false) ? 1 : 0);
                //}
                //else if (foodCategoryType.ToLower() == FoodCategoryType.Kitchen.ToLower())
                //{
                //    return (order.KitchenId > 0 && (order.IsReadyByKitchen ?? false) ? 1 : 0);
                //}
                //else if (foodCategoryType.ToLower().Contains(FoodCategoryType.Proshop.ToLower()) &&
                //         foodCategoryType.ToLower().Contains(FoodCategoryType.Kitchen.ToLower()))
                //{
                //    var kitchen = (order.KitchenId > 0 && (order.IsReadyByKitchen ?? false) ? 1 : 0);
                //    var proshop = (order.ProShopID > 0 && (order.IsReadyByProshop ?? false) ? 1 : 0);
                //    return (kitchen == proshop ? 1 : 0);
                //}
                //else
                //{
                //    return 0;
                //}
            }
            else
            {
                return 0;
            }
        }

        #endregion


        #region Golfer Comparative Order History Report

        /// <summary>
        /// Created By: Amit Kumar
        /// Created on: 20 August 2015
        /// </summary>
        /// <remarks>Get Order Listing</remarks>
        public IQueryable<object> GetGolferOrdersByCourse(string filterExpression, string fromDate, string toDate,
            string category, string subCategory, string menuItem, string viewIn,
            string sortExpression, string sortDirection, int pageIndex, int pageSize, ref int totalRecords)
        {
            IQueryable<GF_Order> list;

            #region Get Course IDs

            string cIds = string.Join(",", Db.GF_CourseInfo.Where(x => x.Status != StatusType.Delete && (x.ClubHouseID == LoginInfo.CourseId ||
                x.ID == LoginInfo.CourseId)).Select(v => v.ID).ToArray());
            long[] courseIDs = CommonFunctions.ConvertStringArrayToLongArray(cIds);

            #endregion

            //if (!String.IsNullOrWhiteSpace(filterExpression))
            //{
            //bool isNoNumbers = System.Text.RegularExpressions.Regex.IsMatch(filterExpression, @"^[^0-9]+$");

            //if (isNoNumbers)
            //{
            //list = Db.GF_Order.Where(x => ((x.GF_Golfer.FirstName + " " + x.GF_Golfer.LastName).ToLower().Contains(filterExpression.ToLower()) ||
            //                                x.GF_Golfer.Email.ToLower().StartsWith(filterExpression.ToLower())) &&
            //                                x.CourseID == LoginInfo.CourseId &&
            //                                !(x.IsRejected ?? false)).OrderByDescending(x => x.ID);

            list = Db.GF_Order.Where(x => courseIDs.Contains(x.CourseID ?? 0) &&// == LoginInfo.CourseId &&
                                          !(x.IsRejected ?? false)).OrderByDescending(x => x.ID);
            //}
            //else
            //{
            //    long orderID = Convert.ToInt32(filterExpression);
            //    list = Db.GF_Order.Where(x => x.ID == orderID &&
            //                                  x.CourseID == LoginInfo.CourseId &&
            //                                  !(x.IsRejected ?? false)).OrderByDescending(x => x.ID);
            //}
            //}
            //else
            //{
            //    list = Db.GF_Order.Where(x => x.CourseID == LoginInfo.CourseId &&
            //                                   !(x.IsRejected ?? false) &&
            //                                   (viewIn == "" ? x.ID < 0 : x.ID > 0)).OrderByDescending(x => x.ID);
            //}

            #region Filters

            //Golfer Email Filter
            if (!string.IsNullOrEmpty(filterExpression))
            {
                list = list.Where(x => x.GF_Golfer.Email.Trim().ToLower() == filterExpression.Trim().ToLower()).AsQueryable();
            }

            //Date Filter
            if (!string.IsNullOrEmpty(fromDate) && !string.IsNullOrEmpty(toDate))
            {
                DateTime dtDate = DateTime.Parse(fromDate);
                DateTime dtToDate = DateTime.Parse(toDate);

                list = list.ToList().Where(x => x.OrderDate.Value.Date >= dtDate.Date
                    && x.OrderDate.Value.Date <= dtToDate.Date).AsQueryable();
            }

            //Category Filter
            if (!string.IsNullOrEmpty(category))
            {
                long catID = Convert.ToInt32(category);
                list = list.ToList().Where(x => x.GF_OrderDetails.Where(y => y.GF_MenuItems.GF_SubCategory.CategoryID == catID).Count() > 0).AsQueryable();
            }

            //Sub Category Filter
            if (!string.IsNullOrEmpty(subCategory))
            {
                long subCatID = Convert.ToInt32(subCategory);
                list = list.ToList().Where(x => x.GF_OrderDetails.Where(y => y.GF_MenuItems.SubCategoryID == subCatID).Count() > 0).AsQueryable();
            }

            //Menu Item Filter
            if (!string.IsNullOrEmpty(menuItem))
            {
                long menuItemID = Convert.ToInt32(menuItem);
                list = list.ToList().Where(x => x.GF_OrderDetails.Where(y => y.MenuItemID == menuItemID).Count() > 0).AsQueryable();
            }

            #endregion

            totalRecords = list.Count();

            string timeZone = CommonFunctions.GetCourseTimeZone(LoginInfo.CourseId);
            IQueryable<object> lst = list.ToList().Select((x =>
                        new
                        {
                            x.ID,
                            //OrderDate = CommonFunctions.DateByTimeZone(timeZone, x.OrderDate ?? DateTime.UtcNow),
                            OrderDate = CommonFunctions.DateByTimeZone(timeZone, x.OrderDate ?? DateTime.UtcNow).ToString("MM/dd/yyyy hh:mm tt"),
                            time = CommonFunctions.DateByTimeZone(timeZone, x.OrderDate ?? DateTime.UtcNow).ToShortTimeString(),
                            golferName = x.GF_Golfer.FirstName + " " + x.GF_Golfer.LastName,
                            golferEmail = x.GF_Golfer.Email,
                            billAmount = x.GF_OrderDetails.Sum(y => ((y.UnitPrice ?? 0) * (y.Quantity ?? 0))).ToString("F"),
                            TaxAmount = (x.TaxAmount ?? 0).ToString("F"),
                            GolferPlatformFee = (x.GolferPlatformFee ?? 0).ToString("F"),
                            GrandTotal = "$" + (x.GrandTotal ?? 0).ToString("F"),
                            itemOrdered = new JavaScriptSerializer().Serialize(x.GF_OrderDetails.ToList().Select(y =>
                                                                            new
                                                                            {
                                                                                y.GF_MenuItems.Name,
                                                                                UnitPrice = y.UnitPrice,
                                                                                y.Quantity,
                                                                                Amount = (y.UnitPrice * y.Quantity),
                                                                                MenuOptionName = y.GF_OrderMenuOptionDetail.Select(q => q.MenuOptionName).ToList()
                                                                            })),
                            OrderType = OrderType.GetFullOrderType(x.OrderType),
                            PaymentMode = (x.PaymentType == "1" ? "Payment by Card" : "Payment by MembershipId"),
                            CourseInfo = x.GF_CourseInfo.COURSE_NAME,
                            CourseAddress = x.GF_CourseInfo.ADDRESS,
                            PromoCode = x.GF_PromoCode == null ? "0.00" : (x.DiscountAmt ?? 0).ToString("F"),
                            TransId = x.PaymentType == "0" ? x.MemberShipID : ((x.BT_TransId == null ? "" : x.BT_TransId)),
                            OrderStatus = ((x.IsDelivered ?? false) || (x.IsPickup ?? false)) ? "Paid" : "Pending",
                            Cartie = x.CartieId > 0 ? Db.GF_AdminUsers.FirstOrDefault(y => y.ID == x.CartieId).UserName : "N/A",
                            PreparedByType = (string.Join(",", x.GF_OrderDetails.ToList()
                                            .Select(y => y.GF_MenuItems.GF_SubCategory.GF_Category.Type)
                                            .Distinct().ToList())).ToLower() == FoodCategoryType.Proshop.ToLower() ? "Proshop" : "Kitchen",
                            PreparedBy = (string.Join(",", x.GF_OrderDetails.ToList()
                                        .Select(y => y.GF_MenuItems.GF_SubCategory.GF_Category.Type)
                                        .Distinct().ToList())).ToLower() == FoodCategoryType.Proshop.ToLower() ?
                                        ((Db.GF_AdminUsers.FirstOrDefault(k => k.ID == x.ProShopID) == null ? "N/A" : (Db.GF_AdminUsers.FirstOrDefault(k => k.ID == x.ProShopID).FirstName))) : ((Db.GF_AdminUsers.FirstOrDefault(k => k.ID == x.KitchenId) == null ? "N/A" : (Db.GF_AdminUsers.FirstOrDefault(k => k.ID == x.KitchenId).FirstName)))
                        }
                    )).AsQueryable();

            return lst.OrderBy(sortExpression + " " + sortDirection).Skip((pageIndex - 1) * pageSize).Take(pageSize);
        }

        /// <summary>
        /// Created By: Amit Kumar
        /// Created Date: 21 August 2015
        /// Description: Get comparative result of order and game
        /// </summary>
        /// <param name="golferEmail"></param>
        /// <param name="DateFrom"></param>
        /// <param name="DateTo"></param>
        /// <param name="viewIN"></param>
        /// <returns></returns>
        public List<object> GetComparativeOrderGame(string golferEmail, DateTime DateFrom, DateTime DateTo, string viewIN, ref string golferFullName)
        {
            List<object> orderGameList = new List<object>();
            orderGameList.Add(new object[] { " ", "Order", "Game Play" });

            try
            {
                long golferID = 0;
                if (!string.IsNullOrEmpty(golferEmail))
                {
                    var golferUser = Db.GF_Golfer.FirstOrDefault(x => x.Email.Trim().ToLower() == golferEmail.Trim().ToLower());
                    if (golferUser != null)
                    {
                        golferID = golferUser.GF_ID;
                        golferFullName = golferUser.FirstName + " " + golferUser.LastName;
                    }
                    //else
                    //{
                    //    orderGameList.Add(new object[] { " ", 0, 0 });
                    //    return orderGameList;
                    //}
                }
                //else
                //{
                //    orderGameList.Add(new object[] { " ", 0, 0 });
                //    return orderGameList;
                //}

                string reportType = ReportResultType.DateWise;

                #region Get Report Type

                var allDates = DateFrom.Range(DateTo);

                if (allDates.Count() <= 7)
                {
                    reportType = ReportResultType.DateWise;
                }
                else if (allDates.Count() <= 31)
                {
                    reportType = ReportResultType.WeekWise;
                }
                else if (allDates.Count() > 31 && allDates.Count() <= 366)
                {
                    reportType = ReportResultType.MonthWise;
                }
                else if (allDates.Count() > 366)
                {
                    reportType = ReportResultType.YearWise;
                }

                #endregion

                var cID = new SqlParameter
                {
                    ParameterName = "CourseID",
                    Value = LoginInfo.CourseId
                };

                var gID = new SqlParameter
                {
                    ParameterName = "GolferID",
                    Value = golferID
                };

                var dtFrom = new SqlParameter
                {
                    ParameterName = "DateFrom",
                    Value = DateFrom.ToString("yyyy-MM-dd")
                };

                var dtTo = new SqlParameter
                {
                    ParameterName = "DateTo",
                    Value = DateTo.ToString("yyyy-MM-dd")
                };

                var rpType = new SqlParameter
                {
                    ParameterName = "ReportType",
                    Value = reportType
                };

                var vIN = new SqlParameter
                {
                    ParameterName = "ViewIN",
                    Value = viewIN
                };

                var lstOrderAndGame = Db.Database.SqlQuery<ComparativeOrderAndGame>("exec GF_SP_OrderAndGameComparativeAnalysis @CourseID, @GolferID, @DateFrom, @DateTo, @ReportType, @ViewIN",
                    cID, gID, dtFrom, dtTo, rpType, vIN).ToList<ComparativeOrderAndGame>();

                if (lstOrderAndGame.Count() > 0)
                {
                    string dText = "";
                    Calendar calendar = CultureInfo.CurrentCulture.Calendar;
                    var lstWeekYear = allDates.ToList().Select(x => new
                    {
                        WeekNumber = calendar.GetWeekOfYear(x.Date, CalendarWeekRule.FirstDay, DayOfWeek.Sunday).ToString(),
                        Year = x.Year
                    }).Distinct();

                    foreach (var row in lstOrderAndGame)
                    {
                        if (reportType == ReportResultType.WeekWise)
                        {
                            if (lstWeekYear != null)
                            {
                                dText = lstWeekYear.FirstOrDefault(x => x.WeekNumber == row.displayText) != null ?
                                    CommonFunctions.FirstLastDayOfWeek(Convert.ToInt32(row.displayText), lstWeekYear.FirstOrDefault(x => x.WeekNumber == row.displayText).Year) : row.displayText;
                            }
                            else
                            {
                                dText = row.displayText;
                            }
                        }
                        else
                        {
                            dText = row.displayText;
                        }

                        orderGameList.Add(new object[] { dText, row.Order, row.GamePlay });
                    }
                }
                else
                {
                    orderGameList.Add(new object[] { " ", 0, 0 });
                }

                return orderGameList;
            }
            catch
            {
                orderGameList.Add(new object[] { " ", 0, 0 });
                return orderGameList;
            }
        }

        /// <summary>
        /// Created By: Amit Kumar
        /// Created Date: 21 August 2015
        /// Description: Get comparative result of rating and complaints
        /// </summary>
        /// <param name="golferEmail"></param>
        /// <param name="DateFrom"></param>
        /// <param name="DateTo"></param>
        /// <param name="viewIN"></param>
        /// <returns></returns>
        public List<object> GetComparativeRatingComplaints(string golferEmail, DateTime DateFrom, DateTime DateTo, string viewIN)
        {
            List<object> ratingComplaintList = new List<object>();
            ratingComplaintList.Add(new object[] { " ", "Rating", "Complaint" });

            try
            {
                long golferID = 0;
                if (!string.IsNullOrEmpty(golferEmail))
                {
                    var golferUser = Db.GF_Golfer.FirstOrDefault(x => x.Email.Trim().ToLower() == golferEmail.Trim().ToLower());
                    if (golferUser != null)
                    {
                        golferID = golferUser.GF_ID;
                    }
                    //else
                    //{
                    //    ratingComplaintList.Add(new object[] { " ", 0, 0 });
                    //    return ratingComplaintList;
                    //}
                }
                //else
                //{
                //    ratingComplaintList.Add(new object[] { " ", 0, 0 });
                //    return ratingComplaintList;
                //}

                string reportType = ReportResultType.DateWise;

                #region Get Report Type

                var allDates = DateFrom.Range(DateTo);

                if (allDates.Count() <= 7)
                {
                    reportType = ReportResultType.DateWise;
                }
                else if (allDates.Count() <= 31)
                {
                    reportType = ReportResultType.WeekWise;
                }
                else if (allDates.Count() > 31 && allDates.Count() <= 366)
                {
                    reportType = ReportResultType.MonthWise;
                }
                else if (allDates.Count() > 366)
                {
                    reportType = ReportResultType.YearWise;
                }

                #endregion

                var cID = new SqlParameter
                {
                    ParameterName = "CourseID",
                    Value = LoginInfo.CourseId
                };

                var gID = new SqlParameter
                {
                    ParameterName = "GolferID",
                    Value = golferID
                };

                var dtFrom = new SqlParameter
                {
                    ParameterName = "DateFrom",
                    Value = DateFrom.ToString("yyyy-MM-dd")
                };

                var dtTo = new SqlParameter
                {
                    ParameterName = "DateTo",
                    Value = DateTo.ToString("yyyy-MM-dd")
                };

                var rpType = new SqlParameter
                {
                    ParameterName = "ReportType",
                    Value = reportType
                };

                var vIN = new SqlParameter
                {
                    ParameterName = "ViewIN",
                    Value = viewIN
                };

                var lstOrderAndGame = Db.Database.SqlQuery<ComparativeRatingAndComplaints>("exec GF_SP_RatingAndComplaintsAnalysis @CourseID, @GolferID, @DateFrom, @DateTo, @ReportType, @ViewIN",
                    cID, gID, dtFrom, dtTo, rpType, vIN).ToList<ComparativeRatingAndComplaints>();

                if (lstOrderAndGame.Count() > 0)
                {
                    string dText = "";
                    Calendar calendar = CultureInfo.CurrentCulture.Calendar;
                    var lstWeekYear = allDates.ToList().Select(x => new
                    {
                        WeekNumber = calendar.GetWeekOfYear(x.Date, CalendarWeekRule.FirstDay, DayOfWeek.Sunday).ToString(),
                        Year = x.Year
                    }).Distinct();

                    foreach (var row in lstOrderAndGame)
                    {
                        if (reportType == ReportResultType.WeekWise)
                        {
                            if (lstWeekYear != null)
                            {
                                dText = lstWeekYear.FirstOrDefault(x => x.WeekNumber == row.displayText) != null ?
                                    CommonFunctions.FirstLastDayOfWeek(Convert.ToInt32(row.displayText), lstWeekYear.FirstOrDefault(x => x.WeekNumber == row.displayText).Year) : row.displayText;
                            }
                            else
                            {
                                dText = row.displayText;
                            }
                        }
                        else
                        {
                            dText = row.displayText;
                        }

                        ratingComplaintList.Add(new object[] { dText, row.Rating, row.Complaint });
                    }
                }
                else
                {
                    ratingComplaintList.Add(new object[] { " ", 0, 0 });
                }

                return ratingComplaintList;
            }
            catch
            {
                ratingComplaintList.Add(new object[] { " ", 0, 0 });
                return ratingComplaintList;
            }
        }

        /// <summary>
        /// Created By: Amit Kumar
        /// Created Date: 24 August 2015
        /// </summary>
        /// <param name="golferEmail"></param>
        /// <param name="DateFrom"></param>
        /// <param name="DateTo"></param>
        /// <param name="viewIN"></param>
        /// <returns></returns>
        public int GetGolferAverageSpending(string golferEmail, DateTime DateFrom, DateTime DateTo, string viewIN,
            ref string strCourseAvgSpend, ref string strOtherAvgSpend)
        {
            try
            {
                #region Get Course IDs

                string cIds = string.Join(",", Db.GF_CourseInfo.Where(x => x.Status != StatusType.Delete && (x.ClubHouseID == LoginInfo.CourseId ||
                    x.ID == LoginInfo.CourseId)).Select(v => v.ID).ToArray());
                long[] courseIDs = CommonFunctions.ConvertStringArrayToLongArray(cIds);

                #endregion

                long golferID = 0;
                if (!string.IsNullOrEmpty(golferEmail))
                {
                    var golferUser = Db.GF_Golfer.FirstOrDefault(x => x.Email.Trim().ToLower() == golferEmail.Trim().ToLower());
                    if (golferUser != null)
                    {
                        golferID = golferUser.GF_ID;
                    }
                }

                string reportType = "Daily";

                #region Get Report Type

                var allDates = DateFrom.Range(DateTo);

                if (allDates.Count() <= 7)
                {
                    reportType = "Daily";
                }
                else if (allDates.Count() <= 31)
                {
                    reportType = "Weekly";
                }
                else if (allDates.Count() > 31 && allDates.Count() <= 366)
                {
                    reportType = "Monthly";
                }
                else if (allDates.Count() > 366)
                {
                    reportType = "Yearly";
                }

                #endregion

                var lstOrder = Db.GF_Order.Where(x => x.GolferID == golferID && !(x.IsRejected ?? false)).ToList();

                if (lstOrder.Count() > 0)
                {
                    decimal courseAvgSpend = lstOrder.Where(x => courseIDs.Contains(x.CourseID ?? 0) &&//== LoginInfo.CourseId &&
                        (x.OrderDate.Value.Date >= DateFrom.Date && x.OrderDate.Value.Date <= DateTo.Date)).Average(x => (x.GrandTotal ?? 0));
                    strCourseAvgSpend = reportType + " Average spending: $" + Math.Round(courseAvgSpend, 2).ToString();

                    decimal otherAvgSpend = lstOrder.Where(x => !courseIDs.Contains(x.CourseID ?? 0) && //!= LoginInfo.CourseId &&
                        (x.OrderDate.Value.Date >= DateFrom.Date && x.OrderDate.Value.Date <= DateTo.Date)).Average(x => (x.GrandTotal ?? 0));
                    strOtherAvgSpend = reportType + " Average spending on other course: $" + Math.Round(otherAvgSpend, 2).ToString();

                    return 1;
                }
                else
                {
                    strCourseAvgSpend = reportType + " Average spending: $0.00";
                    strOtherAvgSpend = reportType + " Average spending on other course: $0.00";
                    return 0;
                }
            }
            catch (Exception ex)
            {
                string errMsg = ex.Message;
                strCourseAvgSpend = "";
                strOtherAvgSpend = "";
                return 0;
            }
        }

        /// <summary>
        /// Created By: Amit Kumar
        /// Created Date: 21 Sep 2015
        /// Description: Get weather re[port
        /// </summary>
        /// <param name="golferEmail"></param>
        /// <param name="DateFrom"></param>
        /// <param name="DateTo"></param>
        /// <param name="viewIN"></param>
        /// <returns></returns>
        public List<object> GetComparativeOrderWeather(string golferEmail, DateTime DateFrom, DateTime DateTo, string viewIN)
        {
            List<object> orderGameList = new List<object>();
            orderGameList.Add(new object[] { " ", "Temperature", "Humidity", "Wind Speed" });

            try
            {
                long golferID = 0;
                if (!string.IsNullOrEmpty(golferEmail))
                {
                    var golferUser = Db.GF_Golfer.FirstOrDefault(x => x.Email.Trim().ToLower() == golferEmail.Trim().ToLower());
                    if (golferUser != null)
                    {
                        golferID = golferUser.GF_ID;
                    }
                    else
                    {
                        //orderGameList.Add(new object[] { " ", 0, 0, 0 });
                        //return orderGameList;
                        golferID = 0;
                    }
                }
                else
                {
                    orderGameList.Add(new object[] { " ", 0, 0, 0 });
                    return orderGameList;
                }

                var cID = new SqlParameter
                {
                    ParameterName = "CourseID",
                    Value = LoginInfo.CourseId
                };

                var gID = new SqlParameter
                {
                    ParameterName = "GolferID",
                    Value = golferID
                };

                var dtFrom = new SqlParameter
                {
                    ParameterName = "DateFrom",
                    Value = DateFrom.ToString("yyyy-MM-dd")
                };

                var dtTo = new SqlParameter
                {
                    ParameterName = "DateTo",
                    Value = DateTo.ToString("yyyy-MM-dd")
                };

                var vIN = new SqlParameter
                {
                    ParameterName = "ViewIN",
                    Value = viewIN
                };

                var lstOrderAndGame = Db.Database.SqlQuery<ComparativeWeather>("exec GF_SP_GetOrderWeatherAvgReport @CourseID, @GolferID, @DateFrom, @DateTo, @ViewIN",
                    cID, gID, dtFrom, dtTo, vIN).ToList<ComparativeWeather>();

                if (lstOrderAndGame.Count() > 0)
                {
                    string dText = "";

                    foreach (var row in lstOrderAndGame)
                    {
                        dText = row.displayText;
                        orderGameList.Add(new object[] { dText, row.Temperature, row.Humidity, row.WindSpeed });
                    }
                }
                else
                {
                    orderGameList.Add(new object[] { " ", 0, 0, 0 });
                }

                return orderGameList;
            }
            catch
            {
                orderGameList.Add(new object[] { " ", 0, 0, 0 });
                return orderGameList;
            }
        }

        #endregion
    }

    public class resultSet
    {
        public long Id { get; set; }
        public int Status { get; set; }
        public List<OrderApiResult> record { get; set; }
        public string Error { get; set; }
    }

    public class OrderApiResult
    {
        public long orderID { get; set; }
        public long golferID { get; set; }
        public string golferName { get; set; }
        public string Longitude { get; set; }
        public string Latitude { get; set; }
        public string time { get; set; }
        public List<itemsOrderedDetaul> itemsOrdered { get; set; }
        public decimal? billAmount { get; set; }
        public decimal? TaxAmount { get; set; }
        public decimal? GolferPlatformFee { get; set; }
        public decimal? Total { get; set; }
        public string OrderType { get; set; }
        public string OrderDate { get; set; }
        public string HEXColor { get; set; }
        public string RGBColor { get; set; }
        public string HUEColor { get; set; }
        public string orderInclude { get; set; }
        public decimal? DiscountAmt { get; set; }
        public string TimeElapsed { get; set; }
        public int ReadyStatus { get; set; }
        public bool IsNew { get; set; }
    }

    public class itemsOrderedDetaul
    {
        public string Name { get; set; }
        public decimal? UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal? Amount { get; set; }
    }

    public class OrderData
    {
        public long orderID { get; set; }
        public long? courseID { get; set; }
        public long? golferID { get; set; }
        public string golferName { get; set; }
        public string Longitude { get; set; }
        public string Latitude { get; set; }
        public string time { get; set; }
        public List<GF_OrderDetails> itemsOrdered { get; set; }
        public decimal? billAmount { get; set; }
        public decimal? TaxPercentage { get; set; }
        public decimal? TaxAmount { get; set; }
        public decimal GolferPlatformFee { get; set; }
        public decimal? Total { get; set; }
        public string OrderType { get; set; }
        public string OrderDate { get; set; }
        public int ReadyStatus { get; set; }
        public string HEXColor { get; set; }
        public string RGBColor { get; set; }
        public string HUEColor { get; set; }
        public string orderInclude { get; set; }
        public decimal? DiscountAmt { get; set; }
        public string TimeElapsed { get; set; }
        public bool IsNew { get; set; }
    }

    public partial class GF_OrderDetails
    {
        public string Name { get; set; }
        public decimal Amount { get; set; }
        public string MenuOptionName { get; set; }
    }

    public class ActiveCourseUser
    {
        public Int64 Id { get; set; }
        public string Name { get; set; }
        public string UserType { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
    }

    public class ComparativeOrderAndGame
    {
        public string displayText { get; set; }
        public Int32 Order { get; set; }
        public Int32 GamePlay { get; set; }
    }

    public class ComparativeRatingAndComplaints
    {
        public string displayText { get; set; }
        public Int32 Rating { get; set; }
        public Int32 Complaint { get; set; }
    }

    public class ComparativeWeather
    {
        public string displayText { get; set; }
        public decimal Temperature { get; set; }
        public decimal Humidity { get; set; }
        public decimal Pressure { get; set; }
        public decimal WindSpeed { get; set; }
    }
}