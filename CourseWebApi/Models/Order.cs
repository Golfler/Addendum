using CourseWebAPI.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Objects;
using System.Globalization;
using System.Linq;
using System.Web;

namespace CourseWebApi.Models
{
    public partial class Order
    {
        protected GolflerEntities db;
        public DateTime TodaysDate = Convert.ToDateTime(DateTime.UtcNow).Date;

        #region Constructors

        public Order()
        {
            db = new GolflerEntities();
        }

        #endregion

        #region Cartie

        #region Old Commented Code

        ///// <summary>
        ///// Created By: Amit Kumar
        ///// Created Date: 25 March 2015
        ///// Purpose: List of those Orders which is accepted by cartie
        ///// </summary>
        ///// <param name="cartieID"></param>
        ///// <returns></returns>
        //public Result GetOrderList(long? cartieID)
        //{
        //    try
        //    {
        //        var lstOrders = db.GF_Order.Where(x => x.CartieId == cartieID &&
        //            (x.OrderType == OrderType.CartOrder ? (x.IsDelivered ?? false) == false : (x.IsPickup ?? false) == false) &&
        //            x.OrderType == OrderType.CartOrder &&
        //            EntityFunctions.TruncateTime(x.OrderDate) == TodaysDate &&
        //            (x.IsActive ?? false) &&
        //            !(x.IsRejected ?? false)).ToList()
        //            .Select(x =>
        //                new
        //                {
        //                    orderID = x.ID,
        //                    courseID = x.CourseID,
        //                    golferID = x.GolferID,
        //                    golferName = x.GF_Golfer.FirstName + " " + x.GF_Golfer.LastName,
        //                    //x.Longitude,
        //                    //x.Latitude,
        //                    Longitude = db.GF_GolferChangingLocation.FirstOrDefault(q => q.GolferId == x.GolferID) == null ? x.Longitude :
        //                                    db.GF_GolferChangingLocation.OrderByDescending(q => q.TimeOfChange).FirstOrDefault(q => q.GolferId == x.GolferID).Longitude,
        //                    Latitude = db.GF_GolferChangingLocation.FirstOrDefault(q => q.GolferId == x.GolferID) == null ? x.Latitude :
        //                                    db.GF_GolferChangingLocation.OrderByDescending(q => q.TimeOfChange).FirstOrDefault(q => q.GolferId == x.GolferID).Latitude,
        //                    time = x.OrderDate.Value.ToShortTimeString(),
        //                    itemsOrdered = x.GF_OrderDetails.ToList().Select(y =>
        //                        new
        //                        {
        //                            y.GF_MenuItems.Name,
        //                            y.UnitPrice,
        //                            y.Quantity,
        //                            Amount = (y.UnitPrice * y.Quantity)
        //                        }),
        //                    billAmount = x.GF_OrderDetails.Sum(y => (y.UnitPrice * y.Quantity)),
        //                    TaxPercentage = x.GF_CourseInfo.Tax,
        //                    TaxAmount = x.TaxAmount,//((x.GF_OrderDetails.Sum(y => (y.UnitPrice * y.Quantity))) * x.GF_CourseInfo.Tax / 100),
        //                    GolferPlatformFee = (x.GolferPlatformFee ?? 0),
        //                    Total = x.GrandTotal,
        //                    //Total = ((x.GF_OrderDetails.Sum(y => (y.UnitPrice * y.Quantity))) +
        //                    //                                     ((x.GF_OrderDetails.Sum(y => (y.UnitPrice * y.Quantity))) * x.GF_CourseInfo.Tax / 100) +
        //                    //                                     (x.GolferPlatformFee ?? 0)),
        //                    //x.TaxAmount,
        //                    //x.GolferPlatformFee,
        //                    //Total = ((x.GF_OrderDetails.Sum(y => (y.GF_MenuItems.Amount * y.Quantity))) + x.TaxAmount + x.GolferPlatformFee),
        //                    x.OrderType,
        //                    x.OrderDate,
        //                    //ReadyStatus = (x.KitchenId > 0) ? 1 : 0,
        //                    ReadyStatus = (string.Join(",", x.GF_OrderDetails.ToList()
        //                                                    .Select(y => y.GF_MenuItems.GF_SubCategory.GF_Category.Type)
        //                                                    .Distinct().ToList())).ToLower() == FoodCategoryType.Proshop.ToLower() ?
        //                                                    (x.ProShopID > 0 && (x.IsReadyByProshop ?? false) ? 1 : 0) :
        //                                                    (x.KitchenId > 0 && (x.IsReadyByKitchen ?? false) ? 1 : 0),
        //                    x.HEXColor,
        //                    x.RGBColor,
        //                    x.HUEColor,
        //                    orderInclude = string.Join(",", x.GF_OrderDetails.ToList()
        //                                                    .Select(y => y.GF_MenuItems.GF_SubCategory.GF_Category.Type)
        //                                                    .Distinct().ToList()) + (x.OrderType == OrderType.CartOrder ? ",Cart" : ""),
        //                    x.DiscountAmt,
        //                    TimeElapsed = ((long)DateTime.Now.Subtract(Convert.ToDateTime(x.OrderDate)).TotalMinutes).ToString()
        //                }).OrderByDescending(x => x.orderID);

        //        if (lstOrders.Count() > 0)
        //        {
        //            return new Result
        //                {
        //                    Id = cartieID ?? 0,
        //                    Status = 1,
        //                    Error = "Success",
        //                    record = lstOrders
        //                };
        //        }
        //        else
        //        {
        //            return new Result
        //            {
        //                Id = cartieID ?? 0,
        //                Status = 0,
        //                Error = "No order(s) has been placed.",
        //                record = null
        //            };
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return new Result
        //        {
        //            Id = cartieID ?? 0,
        //            Status = 0,
        //            Error = ex.Message,
        //            record = null
        //        };
        //    }
        //}

        #endregion

        /// <summary>
        /// Created By: Amit Kumar
        /// Created Date: 25 April 2015
        /// Purpose: List of those Orders which is accepted by cartie
        /// </summary>
        /// <param name="cartieID"></param>
        /// <returns></returns>
        public CourseOrderResult GetOrderList(long? cartieID)
        {
            try
            {
                //Get the course id of kitchen user
                var courseID = db.GF_AdminUsers.FirstOrDefault(x => x.ID == cartieID && x.Type.Contains(UserType.Cartie));

                //Check wheather the courseID is null, if null then user is invalid
                if (courseID == null)
                {
                    return new CourseOrderResult
                    {
                        Id = cartieID ?? 0,
                        Status = 0,
                        Error = "Invalid Cartie user",
                        record = null,
                        CourseUser = null
                    };
                }

                string courseTimeZone = CommonFunctions.CourseTimeZone(courseID.CourseId ?? 0); //Get Time Zone of Course

                var lstOrders = db.GF_Order.Where(x => x.CartieId == cartieID &&
                    (x.OrderType == OrderType.CartOrder ? !(x.IsDelivered ?? false) : !(x.IsPickup ?? false)) &&
                    x.OrderType == OrderType.CartOrder &&
                    EntityFunctions.TruncateTime(x.OrderDate) == TodaysDate &&
                    !(x.IsRejected ?? false)).ToList()
                    .Select(x =>
                        new OrderData
                        {
                            orderID = x.ID,
                            courseID = x.CourseID,
                            golferID = x.GolferID,
                            golferName = x.GF_Golfer.FirstName + " " + x.GF_Golfer.LastName,
                            Longitude = x.Longitude,
                            Latitude = x.Latitude,
                            //Longitude = db.GF_GolferChangingLocation.FirstOrDefault(q => q.GolferId == x.GolferID) == null ? x.Longitude :
                            //                db.GF_GolferChangingLocation.OrderByDescending(q => q.TimeOfChange).FirstOrDefault(q => q.GolferId == x.GolferID).Longitude,
                            //Latitude = db.GF_GolferChangingLocation.FirstOrDefault(q => q.GolferId == x.GolferID) == null ? x.Latitude :
                            //                db.GF_GolferChangingLocation.OrderByDescending(q => q.TimeOfChange).FirstOrDefault(q => q.GolferId == x.GolferID).Latitude,
                            time = CommonFunctions.CourseTimeZoneDateTime(courseTimeZone, x.OrderDate.Value).ToShortTimeString(),//x.OrderDate.Value.ToShortTimeString(),
                            itemsOrdered = x.GF_OrderDetails.ToList().Select(y =>
                                new OrderDetails
                                {
                                    Name = y.GF_MenuItems.Name,
                                    UnitPrice = y.UnitPrice ?? 0,
                                    Quantity = y.Quantity ?? 0,
                                    Amount = ((y.UnitPrice ?? 0) * (y.Quantity ?? 0)),
                                    MenuOptionName = y.GF_OrderMenuOptionDetail.Select(q => new MenuOptionName { ID = (q.MenuOptionID ?? 0), Name = q.MenuOptionName }).ToList()
                                }).ToList(),
                            billAmount = x.GF_OrderDetails.Sum(y => (y.UnitPrice * y.Quantity)),
                            TaxPercentage = x.GF_CourseInfo.Tax,
                            TaxAmount = x.TaxAmount,
                            GolferPlatformFee = (x.GolferPlatformFee ?? 0),
                            Total = x.GrandTotal,
                            OrderType = x.OrderType,
                            OrderDate = CommonFunctions.CourseTimeZoneDateTime(courseTimeZone, x.OrderDate.Value),//x.OrderDate,
                            //OrderDate = x.OrderDate.Value.ToString("MM/dd/yyyy"),
                            ReadyStatus = GetOrderReadyStatus(x.ID, UserType.Cartie),
                            //ReadyStatus = (string.Join(",", x.GF_OrderDetails.ToList()
                            //                    .Select(y => y.GF_MenuItems.GF_SubCategory.GF_Category.Type)
                            //                    .Distinct().ToList())).ToLower() == FoodCategoryType.Proshop.ToLower() ?
                            //                        (x.ProShopID > 0 && (x.IsReadyByProshop ?? false) ? 1 : 0) :

                            //                        (string.Join(",", x.GF_OrderDetails.ToList()
                            //                            .Select(y => y.GF_MenuItems.GF_SubCategory.GF_Category.CartType)
                            //                            .Distinct().ToList())).ToLower() == FoodCategoryType.Cart.ToLower() ? 1 :

                            //                                (x.KitchenId > 0 && (x.IsReadyByKitchen ?? false) ? 1 : 0),
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
                            DiscountAmt = x.DiscountAmt,
                            //TimeElapsed = ((long)DateTime.Now.Subtract(Convert.ToDateTime(x.OrderDate)).TotalMinutes).ToString(),
                            TimeElapsed = ((long)CommonFunctions.CourseTimeZoneDateTime(courseTimeZone, DateTime.UtcNow)
                                                .Subtract(CommonFunctions.CourseTimeZoneDateTime(courseTimeZone, x.OrderDate.Value)).TotalMinutes).ToString(),
                            IsNew = false,
                            HoleNo = x.HoleNo ?? 0,
                            //HoleNo = CourseWebAPI.Models.CommonFunctions.GetUserCurrentHoleNo(x.CourseID ?? 0,
                            //                             db.GF_GolferChangingLocation.FirstOrDefault(q => q.GolferId == x.GolferID) == null ? x.Latitude :
                            //                                            db.GF_GolferChangingLocation.OrderByDescending(q => q.TimeOfChange).FirstOrDefault(q => q.GolferId == x.GolferID).Latitude,
                            //                             db.GF_GolferChangingLocation.FirstOrDefault(q => q.GolferId == x.GolferID) == null ? x.Longitude :
                            //                                            db.GF_GolferChangingLocation.OrderByDescending(q => q.TimeOfChange).FirstOrDefault(q => q.GolferId == x.GolferID).Longitude),
                            IsGolphieOrder = string.Join(",", x.GF_OrderDetails.ToList()
                                                            .Select(y => y.GF_MenuItems.GF_SubCategory.GF_Category.CartType)
                                                            .Distinct().ToList()).ToLower() == FoodCategoryType.Cart.ToLower()
                        }).OrderByDescending(x => x.orderID);


                GF_Order obj = new GF_Order();
                obj.CourseID = courseID.CourseId;
                obj.referenceID = cartieID;
                obj.referenceType = UserType.Cartie;
                var lstIncommingOrders = GetAllCourseOrderList(obj);

                var lstOrder = lstOrders.Union(lstIncommingOrders);

                var lstCourseUser = getActiveCourseUser(courseID.CourseId ?? 0, cartieID ?? 0);

                if (lstOrder.Count() > 0)
                {
                    return new CourseOrderResult
                    {
                        Id = cartieID ?? 0,
                        Status = 1,
                        Error = "Success",
                        record = lstOrder,
                        CourseUser = lstCourseUser
                    };
                }
                else
                {
                    return new CourseOrderResult
                    {
                        Id = cartieID ?? 0,
                        Status = 0,
                        //Error = "No order(s) has been placed.",
                        Error = "There are currently no active orders.",
                        record = null,
                        CourseUser = lstCourseUser
                    };
                }
            }
            catch (Exception ex)
            {
                return new CourseOrderResult
                {
                    Id = cartieID ?? 0,
                    Status = 0,
                    Error = ex.Message,
                    record = null,
                    CourseUser = null
                };
            }
        }

        #endregion Cartie

        #region Kitchen

        #region Old Commented Code

        ///// <summary>
        ///// Created By: Amit Kumar
        ///// Created Date: 25 March 2015
        ///// Purpose: List of those Orders which is accepted by kitchen
        ///// </summary>
        ///// <param name="kitchenID"></param>
        ///// <returns></returns>
        //public Result GetKitchenOrderList(long? kitchenID)
        //{
        //    try
        //    {
        //        //Get the course id of kitchen user
        //        var courseID = db.GF_AdminUsers.FirstOrDefault(x => x.ID == kitchenID && x.Type.Contains(UserType.Kitchen));

        //        //Check wheather the courseID is null, if null then user is invalid
        //        if (courseID == null)
        //        {
        //            return new Result
        //            {
        //                Id = kitchenID ?? 0,
        //                Status = 0,
        //                Error = "Invalid kitchen user",
        //                record = null
        //            };
        //        }

        //        //Get the list of those cartie lies in above resultant course
        //        var lstCartie = string.Join(",", db.GF_AdminUsers.Where(x => x.CourseId == courseID.CourseId &&
        //            x.Type == UserType.Cartie).Select(x => x.ID).ToList());

        //        long[] cartie = ConvertStringArrayToLongArray(lstCartie);

        //        //Get those orders which is accepted by cartie of resultant course
        //        var lstCartieOrder = db.GF_Order.Where(y => y.OrderType == OrderType.CartOrder &&
        //                                                    !(y.IsDelivered ?? false) &&
        //                                                    !(y.IsRejected ?? false) &&
        //                                                    EntityFunctions.TruncateTime(y.OrderDate) == TodaysDate &&
        //                                                    cartie.Contains(y.CartieId ?? 0));

        //        var lstOrders = db.GF_Order.Where(x => x.KitchenId == kitchenID &&
        //                    x.OrderType == OrderType.TurnOrder &&
        //                    EntityFunctions.TruncateTime(x.OrderDate) == TodaysDate &&
        //                    (x.IsPickup ?? false) == false &&
        //                    (x.IsActive ?? false) &&
        //                    !(x.IsRejected ?? false)).ToList()
        //            .Union(lstCartieOrder).ToList()
        //            .Select(x =>
        //                new
        //                {
        //                    orderID = x.ID,
        //                    courseID = x.CourseID,
        //                    golferID = x.GolferID,
        //                    golferName = x.GF_Golfer.FirstName + " " + x.GF_Golfer.LastName,
        //                    //x.Longitude,
        //                    //x.Latitude,
        //                    Longitude = db.GF_GolferChangingLocation.FirstOrDefault(q => q.GolferId == x.GolferID) == null ? x.Longitude :
        //                                    db.GF_GolferChangingLocation.OrderByDescending(q => q.TimeOfChange).FirstOrDefault(q => q.GolferId == x.GolferID).Longitude,
        //                    Latitude = db.GF_GolferChangingLocation.FirstOrDefault(q => q.GolferId == x.GolferID) == null ? x.Latitude :
        //                                    db.GF_GolferChangingLocation.OrderByDescending(q => q.TimeOfChange).FirstOrDefault(q => q.GolferId == x.GolferID).Latitude,
        //                    time = x.OrderDate.Value.ToShortTimeString(),
        //                    itemsOrdered = x.GF_OrderDetails.ToList().Select(y =>
        //                        new
        //                        {
        //                            y.GF_MenuItems.Name,
        //                            y.UnitPrice,
        //                            y.Quantity,
        //                            Amount = (y.UnitPrice * y.Quantity)
        //                        }),
        //                    billAmount = x.GF_OrderDetails.Sum(y => (y.UnitPrice * y.Quantity)),
        //                    TaxPercentage = x.GF_CourseInfo.Tax,
        //                    TaxAmount = x.TaxAmount,//((x.GF_OrderDetails.Sum(y => (y.UnitPrice * y.Quantity))) * x.GF_CourseInfo.Tax / 100),
        //                    GolferPlatformFee = (x.GolferPlatformFee ?? 0),
        //                    //Total = ((x.GF_OrderDetails.Sum(y => (y.UnitPrice * y.Quantity))) +
        //                    //                                     ((x.GF_OrderDetails.Sum(y => (y.UnitPrice * y.Quantity))) * x.GF_CourseInfo.Tax / 100) +
        //                    //                                     (x.GolferPlatformFee ?? 0)),
        //                    Total = x.GrandTotal,
        //                    x.OrderType,
        //                    x.OrderDate,
        //                    //ReadyStatus = (x.KitchenId > 0) ? 1 : 0,
        //                    ReadyStatus = (string.Join(",", x.GF_OrderDetails.ToList()
        //                                                    .Select(y => y.GF_MenuItems.GF_SubCategory.GF_Category.Type)
        //                                                    .Distinct().ToList())).ToLower() == FoodCategoryType.Proshop.ToLower() ?
        //                                                    (x.ProShopID > 0 && (x.IsReadyByProshop ?? false) ? 1 : 0) :
        //                                                    (x.KitchenId > 0 && (x.IsReadyByKitchen ?? false) ? 1 : 0),
        //                    x.HEXColor,
        //                    x.RGBColor,
        //                    x.HUEColor,
        //                    orderInclude = string.Join(",", x.GF_OrderDetails.ToList()
        //                                                    .Select(y => y.GF_MenuItems.GF_SubCategory.GF_Category.Type)
        //                                                    .Distinct().ToList()) + (x.OrderType == OrderType.CartOrder ? ",Cart" : ""),
        //                    x.DiscountAmt,
        //                    TimeElapsed = ((long)DateTime.Now.Subtract(Convert.ToDateTime(x.OrderDate)).TotalMinutes).ToString()
        //                }).OrderByDescending(x => x.orderID);

        //        if (lstOrders.Count() > 0)
        //        {
        //            return new Result
        //            {
        //                Id = kitchenID ?? 0,
        //                Status = 1,
        //                Error = "Success",
        //                record = lstOrders
        //            };
        //        }
        //        else
        //        {
        //            return new Result
        //            {
        //                Id = kitchenID ?? 0,
        //                Status = 0,
        //                Error = "No order(s) has been placed.",
        //                record = null
        //            };
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return new Result
        //        {
        //            Id = kitchenID ?? 0,
        //            Status = 0,
        //            Error = ex.Message,
        //            record = null
        //        };
        //    }
        //}

        #endregion

        /// <summary>
        /// Created By: Amit Kumar
        /// Created Date: 25 April 2015
        /// Purpose: List of those Orders which is accepted by kitchen
        /// </summary>
        /// <param name="kitchenID"></param>
        /// <returns></returns>
        public CourseOrderResult GetKitchenOrderList(long? kitchenID)
        {
            try
            {
                //Get the course id of kitchen user
                var courseID = db.GF_AdminUsers.FirstOrDefault(x => x.ID == kitchenID && x.Type.Contains(UserType.Kitchen));

                //Check wheather the courseID is null, if null then user is invalid
                if (courseID == null)
                {
                    return new CourseOrderResult
                    {
                        Id = kitchenID ?? 0,
                        Status = 0,
                        Error = "Invalid kitchen user",
                        record = null,
                        CourseUser = null
                    };
                }

                string courseTimeZone = CommonFunctions.CourseTimeZone(courseID.CourseId ?? 0); //Get Time Zone of Course

                //Get the list of those cartie lies in above resultant course
                var lstCartie = string.Join(",", db.GF_AdminUsers.Where(x => x.CourseId == courseID.CourseId &&
                    x.Type == UserType.Cartie).Select(x => x.ID).ToList());

                long[] cartie = ConvertStringArrayToLongArray(lstCartie);

                //Get those orders which is accepted by cartie of resultant course
                var lstCartieOrder = db.GF_Order.Where(y => y.OrderType == OrderType.CartOrder &&
                                                            !(y.IsDelivered ?? false) &&
                                                            !(y.IsRejected ?? false) &&
                                                            EntityFunctions.TruncateTime(y.OrderDate) == TodaysDate &&
                                                            cartie.Contains(y.CartieId ?? 0));

                var lstOrders = db.GF_Order.Where(x => x.KitchenId == kitchenID &&
                            x.OrderType == OrderType.TurnOrder &&
                            EntityFunctions.TruncateTime(x.OrderDate) == TodaysDate &&
                            (x.IsPickup ?? false) == false &&
                            (x.IsActive ?? false) &&
                            !(x.IsRejected ?? false)).ToList()
                    .Union(lstCartieOrder).ToList()
                    .Select(x =>
                        new OrderData
                        {
                            orderID = x.ID,
                            courseID = x.CourseID,
                            golferID = x.GolferID,
                            golferName = x.GF_Golfer.FirstName + " " + x.GF_Golfer.LastName,
                            Longitude = x.Longitude,
                            Latitude = x.Latitude,
                            //Longitude = db.GF_GolferChangingLocation.FirstOrDefault(q => q.GolferId == x.GolferID) == null ? x.Longitude :
                            //                db.GF_GolferChangingLocation.OrderByDescending(q => q.TimeOfChange).FirstOrDefault(q => q.GolferId == x.GolferID).Longitude,
                            //Latitude = db.GF_GolferChangingLocation.FirstOrDefault(q => q.GolferId == x.GolferID) == null ? x.Latitude :
                            //                db.GF_GolferChangingLocation.OrderByDescending(q => q.TimeOfChange).FirstOrDefault(q => q.GolferId == x.GolferID).Latitude,
                            time = CommonFunctions.CourseTimeZoneDateTime(courseTimeZone, x.OrderDate.Value).ToShortTimeString(),//x.OrderDate.Value.ToShortTimeString(),
                            itemsOrdered = x.GF_OrderDetails.ToList().Select(y =>
                                new OrderDetails
                                {
                                    Name = y.GF_MenuItems.Name,
                                    UnitPrice = y.UnitPrice ?? 0,
                                    Quantity = y.Quantity ?? 0,
                                    Amount = ((y.UnitPrice ?? 0) * (y.Quantity ?? 0)),
                                    MenuOptionName = y.GF_OrderMenuOptionDetail.Select(q => new MenuOptionName { ID = (q.MenuOptionID ?? 0), Name = q.MenuOptionName }).ToList()
                                }).ToList(),
                            billAmount = x.GF_OrderDetails.Sum(y => (y.UnitPrice * y.Quantity)),
                            TaxPercentage = x.GF_CourseInfo.Tax,
                            TaxAmount = x.TaxAmount,
                            GolferPlatformFee = (x.GolferPlatformFee ?? 0),
                            Total = x.GrandTotal,
                            OrderType = x.OrderType,
                            OrderDate = CommonFunctions.CourseTimeZoneDateTime(courseTimeZone, x.OrderDate.Value),//x.OrderDate,
                            //OrderDate = x.OrderDate.Value.ToString("MM/dd/yyyy"),
                            ReadyStatus = GetOrderReadyStatus(x.ID, UserType.Kitchen),
                            //ReadyStatus = (string.Join(",", x.GF_OrderDetails.ToList()
                            //                                .Select(y => y.GF_MenuItems.GF_SubCategory.GF_Category.Type)
                            //                                .Distinct().ToList())).ToLower() == FoodCategoryType.Proshop.ToLower() ?
                            //                                (x.ProShopID > 0 && (x.IsReadyByProshop ?? false) ? 1 : 0) :
                            //                                (x.KitchenId > 0 && (x.IsReadyByKitchen ?? false) ? 1 : 0),
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
                            DiscountAmt = x.DiscountAmt,
                            //TimeElapsed = ((long)DateTime.Now.Subtract(Convert.ToDateTime(x.OrderDate)).TotalMinutes).ToString(),
                            TimeElapsed = ((long)CommonFunctions.CourseTimeZoneDateTime(courseTimeZone, DateTime.UtcNow)
                                                .Subtract(CommonFunctions.CourseTimeZoneDateTime(courseTimeZone, x.OrderDate.Value)).TotalMinutes).ToString(),
                            IsNew = false,
                            HoleNo = x.HoleNo ?? 0,
                            //HoleNo = CourseWebAPI.Models.CommonFunctions.GetUserCurrentHoleNo(x.CourseID ?? 0,
                            //                             db.GF_GolferChangingLocation.FirstOrDefault(q => q.GolferId == x.GolferID) == null ? x.Latitude :
                            //                                            db.GF_GolferChangingLocation.OrderByDescending(q => q.TimeOfChange).FirstOrDefault(q => q.GolferId == x.GolferID).Latitude,
                            //                             db.GF_GolferChangingLocation.FirstOrDefault(q => q.GolferId == x.GolferID) == null ? x.Longitude :
                            //                                            db.GF_GolferChangingLocation.OrderByDescending(q => q.TimeOfChange).FirstOrDefault(q => q.GolferId == x.GolferID).Longitude),
                            IsGolphieOrder = string.Join(",", x.GF_OrderDetails.ToList()
                                                            .Select(y => y.GF_MenuItems.GF_SubCategory.GF_Category.CartType)
                                                            .Distinct().ToList()).ToLower() == FoodCategoryType.Cart.ToLower()
                        }).Where(x => x.orderInclude.ToLower().Contains(FoodCategoryType.Kitchen.ToLower())).OrderByDescending(x => x.orderID);

                GF_Order obj = new GF_Order();
                obj.CourseID = courseID.CourseId;
                obj.referenceID = kitchenID;
                obj.referenceType = UserType.Kitchen;
                var lstIncommingOrders = GetAllCourseOrderList(obj);

                var lstOrder = lstOrders.Union(lstIncommingOrders);

                var lstCourseUser = getActiveCourseUser(courseID.CourseId ?? 0, kitchenID ?? 0);

                if (lstOrder.Count() > 0)
                {
                    return new CourseOrderResult
                    {
                        Id = kitchenID ?? 0,
                        Status = 1,
                        Error = "Success",
                        record = lstOrder,
                        CourseUser = lstCourseUser
                    };
                }
                else
                {
                    return new CourseOrderResult
                    {
                        Id = kitchenID ?? 0,
                        Status = 0,
                        //Error = "No order(s) has been placed.",
                        Error = "There are currently no active orders.",
                        record = null,
                        CourseUser = lstCourseUser
                    };
                }
            }
            catch (Exception ex)
            {
                return new CourseOrderResult
                {
                    Id = kitchenID ?? 0,
                    Status = 0,
                    Error = ex.Message,
                    record = null,
                    CourseUser = null
                };
            }
        }

        #endregion

        #region Proshop

        #region Old Commented Code

        ///// <summary>
        ///// Created By: Amit Kumar
        ///// Created Date: 08 April 2015
        ///// Purpose: List of those Orders which is accepted by Proshop
        ///// </summary>
        ///// <param name="ProshopID"></param>
        ///// <returns></returns>
        //public Result GetProshopOrderList(long? ProshopID)
        //{
        //    try
        //    {
        //        //Get the course id of proshop user
        //        var courseID = db.GF_AdminUsers.FirstOrDefault(x => x.ID == ProshopID &&
        //            (x.Type.Contains(UserType.Proshop) || x.Type.Contains(UserType.CourseAdmin)));

        //        //Check wheather the courseID is null, if null then user is invalid
        //        if (courseID == null)
        //        {
        //            return new Result
        //            {
        //                Id = ProshopID ?? 0,
        //                Status = 0,
        //                Error = "Invalid proshop user",
        //                record = null
        //            };
        //        }

        //        //Get the list of those cartie lies in above resultant course
        //        var lstCartie = string.Join(",", db.GF_AdminUsers.Where(x => x.CourseId == courseID.CourseId &&
        //            x.Type == UserType.Cartie).Select(x => x.ID).ToList());

        //        long[] cartie = ConvertStringArrayToLongArray(lstCartie);

        //        //Get those orders which is accepted by cartie of resultant course
        //        var lstCartieOrder = db.GF_Order.Where(y => y.OrderType == OrderType.CartOrder &&
        //                                                    !(y.IsDelivered ?? false) &&
        //                                                    !(y.IsRejected ?? false) &&
        //                                                    EntityFunctions.TruncateTime(y.OrderDate) == TodaysDate &&
        //                                                    cartie.Contains(y.CartieId ?? 0));

        //        var lstOrders = db.GF_Order.Where(x => x.ProShopID == ProshopID &&
        //                    x.OrderType == OrderType.TurnOrder &&
        //                    EntityFunctions.TruncateTime(x.OrderDate) == TodaysDate &&
        //                    !(x.IsPickup ?? false) &&
        //                    !(x.IsRejected ?? false) &&
        //                    x.IsActive == true).ToList()
        //            .Union(lstCartieOrder).ToList()
        //            .Select(x =>
        //                new
        //                {
        //                    orderID = x.ID,
        //                    courseID = x.CourseID,
        //                    golferID = x.GolferID,
        //                    golferName = x.GF_Golfer.FirstName + " " + x.GF_Golfer.LastName,
        //                    //x.Longitude,
        //                    //x.Latitude,
        //                    Longitude = db.GF_GolferChangingLocation.FirstOrDefault(q => q.GolferId == x.GolferID) == null ? x.Longitude :
        //                                    db.GF_GolferChangingLocation.OrderByDescending(q => q.TimeOfChange).FirstOrDefault(q => q.GolferId == x.GolferID).Longitude,
        //                    Latitude = db.GF_GolferChangingLocation.FirstOrDefault(q => q.GolferId == x.GolferID) == null ? x.Latitude :
        //                                    db.GF_GolferChangingLocation.OrderByDescending(q => q.TimeOfChange).FirstOrDefault(q => q.GolferId == x.GolferID).Latitude,
        //                    time = x.OrderDate.Value.ToShortTimeString(),
        //                    itemsOrdered = x.GF_OrderDetails.ToList().Select(y =>
        //                        new
        //                        {
        //                            y.GF_MenuItems.Name,
        //                            y.UnitPrice,
        //                            y.Quantity,
        //                            Amount = (y.UnitPrice * y.Quantity)
        //                        }),
        //                    billAmount = x.GF_OrderDetails.Sum(y => (y.UnitPrice * y.Quantity)),
        //                    TaxPercentage = x.GF_CourseInfo.Tax,
        //                    TaxAmount = x.TaxAmount,//((x.GF_OrderDetails.Sum(y => (y.UnitPrice * y.Quantity))) * x.GF_CourseInfo.Tax / 100),
        //                    GolferPlatformFee = (x.GolferPlatformFee ?? 0),
        //                    //Total = ((x.GF_OrderDetails.Sum(y => (y.UnitPrice * y.Quantity))) +
        //                    //                                     ((x.GF_OrderDetails.Sum(y => (y.UnitPrice * y.Quantity))) * x.GF_CourseInfo.Tax / 100) +
        //                    //                                     (x.GolferPlatformFee ?? 0)),
        //                    Total = x.GrandTotal,
        //                    x.OrderType,
        //                    x.OrderDate,
        //                    //ReadyStatus = (x.KitchenId > 0) ? 1 : 0,
        //                    ReadyStatus = (string.Join(",", x.GF_OrderDetails.ToList()
        //                                                    .Select(y => y.GF_MenuItems.GF_SubCategory.GF_Category.Type)
        //                                                    .Distinct().ToList())).ToLower() == FoodCategoryType.Proshop.ToLower() ?
        //                                                    (x.ProShopID > 0 && (x.IsReadyByProshop ?? false) ? 1 : 0) :
        //                                                    (x.KitchenId > 0 && (x.IsReadyByKitchen ?? false) ? 1 : 0),
        //                    x.HEXColor,
        //                    x.RGBColor,
        //                    x.HUEColor,
        //                    orderInclude = string.Join(",", x.GF_OrderDetails.ToList()
        //                                                    .Select(y => y.GF_MenuItems.GF_SubCategory.GF_Category.Type)
        //                                                    .Distinct().ToList()) + (x.OrderType == OrderType.CartOrder ? ",Cart" : ""),
        //                    x.DiscountAmt,
        //                    TimeElapsed = ((long)DateTime.Now.Subtract(Convert.ToDateTime(x.OrderDate)).TotalMinutes).ToString()
        //                }).Where(x => x.orderInclude.ToLower().Contains(FoodCategoryType.Proshop.ToLower())).OrderByDescending(x => x.orderID);

        //        if (lstOrders.Count() > 0)
        //        {
        //            return new Result
        //            {
        //                Id = ProshopID ?? 0,
        //                Status = 1,
        //                Error = "Success",
        //                record = lstOrders
        //            };
        //        }
        //        else
        //        {
        //            return new Result
        //            {
        //                Id = ProshopID ?? 0,
        //                Status = 0,
        //                Error = "No order(s) has been placed.",
        //                record = null
        //            };
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return new Result
        //        {
        //            Id = ProshopID ?? 0,
        //            Status = 0,
        //            Error = ex.Message,
        //            record = null
        //        };
        //    }
        //}

        #endregion

        /// <summary>
        /// Created By: Amit Kumar
        /// Created Date: 25 April 2015
        /// Purpose: List of those Orders which is accepted by Proshop
        /// </summary>
        /// <param name="ProshopID"></param>
        /// <returns></returns>
        public CourseOrderResult GetProshopOrderList(long? ProshopID)
        {
            //List<string> processmsg = new List<string>();
            //processmsg.Add("start");
            //LogClass.WriteLog(processmsg, "ProshopOrder.txt");
            try
            {
                //Get the course id of proshop user
                var courseID = db.GF_AdminUsers.FirstOrDefault(x => x.ID == ProshopID &&
                    (x.Type.Contains(UserType.Proshop) || x.Type.Contains(UserType.CourseAdmin) || x.Type.Contains(UserType.PowerAdmin)));

                //Check wheather the courseID is null, if null then user is invalid
                if (courseID == null)
                {
                    return new CourseOrderResult
                    {
                        Id = ProshopID ?? 0,
                        Status = 0,
                        Error = "Invalid proshop user",
                        record = null,
                        CourseUser = null
                    };
                }

                string courseTimeZone = CommonFunctions.CourseTimeZone(courseID.CourseId ?? 0); //Get Time Zone of Course

                #region When Course Admin

                if (courseID.Type == UserType.CourseAdmin || courseID.Type == UserType.PowerAdmin)
                {
                    return GetCourseAllOrderList(ProshopID);
                }

                #endregion

                //Get the list of those cartie lies in above resultant course
                var lstCartie = string.Join(",", db.GF_AdminUsers.Where(x => x.CourseId == courseID.CourseId &&
                    x.Type == UserType.Cartie).Select(x => x.ID).ToList());

                long[] cartie = ConvertStringArrayToLongArray(lstCartie);

                //Get those orders which is accepted by cartie of resultant course
                var lstCartieOrder = db.GF_Order.Where(y => y.OrderType == OrderType.CartOrder &&
                                                            !(y.IsDelivered ?? false) &&
                                                            !(y.IsRejected ?? false) &&
                                                            EntityFunctions.TruncateTime(y.OrderDate) == TodaysDate &&
                                                            cartie.Contains(y.CartieId ?? 0));

                var lstOrders = db.GF_Order.Where(x => x.ProShopID == ProshopID &&
                            x.OrderType == OrderType.TurnOrder &&
                            EntityFunctions.TruncateTime(x.OrderDate) == TodaysDate &&
                            !(x.IsPickup ?? false) &&
                            !(x.IsRejected ?? false) &&
                            x.IsActive == true).ToList()
                    .Union(lstCartieOrder).ToList()
                    .Select(x =>
                        new OrderData
                        {
                            orderID = x.ID,
                            courseID = x.CourseID,
                            golferID = x.GolferID,
                            golferName = x.GF_Golfer.FirstName + " " + x.GF_Golfer.LastName,
                            Longitude = x.Longitude,
                            Latitude = x.Latitude,
                            //Longitude = db.GF_GolferChangingLocation.FirstOrDefault(q => q.GolferId == x.GolferID) == null ? x.Longitude :
                            //                db.GF_GolferChangingLocation.OrderByDescending(q => q.TimeOfChange).FirstOrDefault(q => q.GolferId == x.GolferID).Longitude,
                            //Latitude = db.GF_GolferChangingLocation.FirstOrDefault(q => q.GolferId == x.GolferID) == null ? x.Latitude :
                            //                db.GF_GolferChangingLocation.OrderByDescending(q => q.TimeOfChange).FirstOrDefault(q => q.GolferId == x.GolferID).Latitude,
                            time = CommonFunctions.CourseTimeZoneDateTime(courseTimeZone, x.OrderDate.Value).ToShortTimeString(),//x.OrderDate.Value.ToShortTimeString(),
                            itemsOrdered = x.GF_OrderDetails.ToList().Select(y =>
                                new OrderDetails
                                {
                                    Name = y.GF_MenuItems.Name,
                                    UnitPrice = y.UnitPrice ?? 0,
                                    Quantity = y.Quantity ?? 0,
                                    Amount = ((y.UnitPrice ?? 0) * (y.Quantity ?? 0)),
                                    MenuOptionName = y.GF_OrderMenuOptionDetail.Select(q => new MenuOptionName { ID = (q.MenuOptionID ?? 0), Name = q.MenuOptionName }).ToList()
                                }).ToList(),
                            billAmount = x.GF_OrderDetails.Sum(y => (y.UnitPrice * y.Quantity)),
                            TaxPercentage = x.GF_CourseInfo.Tax,
                            TaxAmount = x.TaxAmount,
                            GolferPlatformFee = (x.GolferPlatformFee ?? 0),
                            Total = x.GrandTotal,
                            OrderType = x.OrderType,
                            OrderDate = CommonFunctions.CourseTimeZoneDateTime(courseTimeZone, x.OrderDate.Value),//x.OrderDate,
                            //OrderDate = x.OrderDate.Value.ToString("MM/dd/yyyy"),
                            //ReadyStatus = (string.Join(",", x.GF_OrderDetails.ToList()
                            //                                .Select(y => y.GF_MenuItems.GF_SubCategory.GF_Category.Type)
                            //                                .Distinct().ToList())).ToLower() == FoodCategoryType.Proshop.ToLower() ?
                            //                                (x.ProShopID > 0 && (x.IsReadyByProshop ?? false) ? 1 : 0) :
                            //                                (x.KitchenId > 0 && (x.IsReadyByKitchen ?? false) ? 1 : 0),
                            ReadyStatus = GetOrderReadyStatus(x.ID, UserType.Proshop),
                            //ReadyStatus = (string.Join(",", x.GF_OrderDetails.ToList()
                            //                                .Select(y => y.GF_MenuItems.GF_SubCategory.GF_Category.Type)
                            //                                .Distinct().ToList())).ToLower().Contains(FoodCategoryType.Proshop.ToLower()) ?
                            //                                (x.ProShopID > 0 && (x.IsReadyByProshop ?? false) ? 1 : 0) :
                            //                                (x.KitchenId > 0 && (x.IsReadyByKitchen ?? false) ? 1 : 0),
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
                            DiscountAmt = x.DiscountAmt,
                            //TimeElapsed = ((long)DateTime.Now.Subtract(Convert.ToDateTime(x.OrderDate)).TotalMinutes).ToString(),
                            TimeElapsed = ((long)CommonFunctions.CourseTimeZoneDateTime(courseTimeZone, DateTime.UtcNow)
                                                .Subtract(CommonFunctions.CourseTimeZoneDateTime(courseTimeZone, x.OrderDate.Value)).TotalMinutes).ToString(),
                            IsNew = false,
                            HoleNo = x.HoleNo ?? 0,
                            //HoleNo = CourseWebAPI.Models.CommonFunctions.GetUserCurrentHoleNo(x.CourseID ?? 0,
                            //                             db.GF_GolferChangingLocation.FirstOrDefault(q => q.GolferId == x.GolferID) == null ? x.Latitude :
                            //                                            db.GF_GolferChangingLocation.OrderByDescending(q => q.TimeOfChange).FirstOrDefault(q => q.GolferId == x.GolferID).Latitude,
                            //                             db.GF_GolferChangingLocation.FirstOrDefault(q => q.GolferId == x.GolferID) == null ? x.Longitude :
                            //                                            db.GF_GolferChangingLocation.OrderByDescending(q => q.TimeOfChange).FirstOrDefault(q => q.GolferId == x.GolferID).Longitude),
                            IsGolphieOrder = string.Join(",", x.GF_OrderDetails.ToList()
                                                            .Select(y => y.GF_MenuItems.GF_SubCategory.GF_Category.CartType)
                                                            .Distinct().ToList()).ToLower() == FoodCategoryType.Cart.ToLower()
                        }).Where(x => x.orderInclude.ToLower().Contains(FoodCategoryType.Proshop.ToLower())).OrderByDescending(x => x.orderID);

                GF_Order obj = new GF_Order();
                obj.CourseID = courseID.CourseId;
                obj.referenceID = ProshopID;
                obj.referenceType = UserType.Proshop;
                var lstIncommingOrders = GetAllCourseOrderList(obj);

                var lstOrder = lstOrders.Union(lstIncommingOrders);

                var lstCourseUser = getActiveCourseUser(courseID.CourseId ?? 0, ProshopID ?? 0);

                if (lstOrder.Count() > 0)
                {
                    return new CourseOrderResult
                    {
                        Id = ProshopID ?? 0,
                        Status = 1,
                        Error = "Success",
                        record = lstOrder,
                        CourseUser = lstCourseUser
                    };
                }
                else
                {
                    return new CourseOrderResult
                    {
                        Id = ProshopID ?? 0,
                        Status = 0,
                        //Error = "No order(s) has been placed.",
                        Error = "There are currently no active orders.",
                        record = null,
                        CourseUser = lstCourseUser
                    };
                }
            }
            catch (Exception ex)
            {
                return new CourseOrderResult
                {
                    Id = ProshopID ?? 0,
                    Status = 0,
                    Error = ex.Message,
                    record = null,
                    CourseUser = null
                };
            }
        }

        #endregion

        #region Power Admin

        /// <summary>
        /// Created By: Amit Kumar
        /// Created Date: 11 March 2016
        /// Purpose: List of those Orders which is accepted by Power admin
        /// </summary>
        /// <param name="ProshopID"></param>
        /// <returns></returns>
        public CourseOrderResult GetPowerAdminOrderList(long? PowerAdminID)
        {
            try
            {
                //Get the course id of proshop user
                var courseID = db.GF_AdminUsers.FirstOrDefault(x => x.ID == PowerAdminID && x.Type.Contains(UserType.PowerAdmin));

                //Check wheather the courseID is null, if null then user is invalid
                if (courseID == null)
                {
                    return new CourseOrderResult
                    {
                        Id = PowerAdminID ?? 0,
                        Status = 0,
                        Error = "Invalid admin user",
                        record = null,
                        CourseUser = null
                    };
                }

                string courseTimeZone = CommonFunctions.CourseTimeZone(courseID.CourseId ?? 0); //Get Time Zone of Course

                return GetCourseAllOrderList(PowerAdminID);
            }
            catch (Exception ex)
            {
                return new CourseOrderResult
                {
                    Id = PowerAdminID ?? 0,
                    Status = 0,
                    Error = ex.Message,
                    record = null,
                    CourseUser = null
                };
            }
        }

        #endregion

        #region Course Admin

        /// <summary>
        /// Created By: Amit Kumar
        /// Created Date: 22 June 2015
        /// Purpose: List of All orders placed in Course
        /// </summary>
        /// <param name="CourseAdminID"></param>
        /// <returns></returns>
        public CourseOrderResult GetCourseAllOrderList(long? CourseAdminID)
        {
            try
            {
                //Get the course id of proshop user
                var courseID = db.GF_AdminUsers.FirstOrDefault(x => x.ID == CourseAdminID &&
                    (x.Type.Contains(UserType.CourseAdmin) || x.Type.Contains(UserType.PowerAdmin)));

                //Check wheather the courseID is null, if null then user is invalid
                if (courseID == null)
                {
                    return new CourseOrderResult
                    {
                        Id = CourseAdminID ?? 0,
                        Status = 0,
                        Error = "Invalid course admin",
                        record = null,
                        CourseUser = null
                    };
                }

                string courseTimeZone = CommonFunctions.CourseTimeZone(courseID.CourseId ?? 0); //Get Time Zone of Course

                #region Get Course IDs

                string cIds = string.Join(",", db.GF_CourseInfo.Where(x => x.Status != StatusType.Delete && (x.ClubHouseID == courseID.CourseId ||
                    x.ID == courseID.CourseId)).Select(v => v.ID).ToArray());
                long[] courseIDs = CommonFunctions.ConvertStringArrayToLongArray(cIds);

                #endregion

                var lstOrders = db.GF_Order.Where(x => courseIDs.Contains(x.CourseID ?? 0) &&// == courseID.CourseId &&
                            EntityFunctions.TruncateTime(x.OrderDate) == TodaysDate &&
                            !(x.IsPickup ?? false) &&
                            !(x.IsDelivered ?? false) &&
                            !(x.IsRejected ?? false)).ToList()
                    .Select(x =>
                        new OrderData
                        {
                            orderID = x.ID,
                            courseID = x.CourseID,
                            golferID = x.GolferID,
                            golferName = x.GF_Golfer.FirstName + " " + x.GF_Golfer.LastName,
                            Longitude = x.Longitude,
                            Latitude = x.Latitude,
                            //Longitude = db.GF_GolferChangingLocation.FirstOrDefault(q => q.GolferId == x.GolferID) == null ? x.Longitude :
                            //                db.GF_GolferChangingLocation.OrderByDescending(q => q.TimeOfChange).FirstOrDefault(q => q.GolferId == x.GolferID).Longitude,
                            //Latitude = db.GF_GolferChangingLocation.FirstOrDefault(q => q.GolferId == x.GolferID) == null ? x.Latitude :
                            //                db.GF_GolferChangingLocation.OrderByDescending(q => q.TimeOfChange).FirstOrDefault(q => q.GolferId == x.GolferID).Latitude,
                            time = CommonFunctions.CourseTimeZoneDateTime(courseTimeZone, x.OrderDate.Value).ToShortTimeString(),//x.OrderDate.Value.ToShortTimeString(),
                            itemsOrdered = x.GF_OrderDetails.ToList().Select(y =>
                                new OrderDetails
                                {
                                    Name = y.GF_MenuItems.Name,
                                    UnitPrice = y.UnitPrice ?? 0,
                                    Quantity = y.Quantity ?? 0,
                                    Amount = ((y.UnitPrice ?? 0) * (y.Quantity ?? 0)),
                                    MenuOptionName = y.GF_OrderMenuOptionDetail.Select(q => new MenuOptionName { ID = (q.MenuOptionID ?? 0), Name = q.MenuOptionName }).ToList()
                                }).ToList(),
                            billAmount = x.GF_OrderDetails.Sum(y => (y.UnitPrice * y.Quantity)),
                            TaxPercentage = x.GF_CourseInfo.Tax,
                            TaxAmount = x.TaxAmount,
                            GolferPlatformFee = (x.GolferPlatformFee ?? 0),
                            Total = x.GrandTotal,
                            OrderType = x.OrderType,
                            OrderDate = CommonFunctions.CourseTimeZoneDateTime(courseTimeZone, x.OrderDate.Value),//x.OrderDate,
                            ReadyStatus = GetOrderReadyStatus(x.ID, UserType.PowerAdmin),
                            //ReadyStatus = (string.Join(",", x.GF_OrderDetails.ToList()
                            //                                .Select(y => y.GF_MenuItems.GF_SubCategory.GF_Category.Type)
                            //                                .Distinct().ToList())).ToLower() == FoodCategoryType.Proshop.ToLower() ?
                            //                                (x.ProShopID > 0 && (x.IsReadyByProshop ?? false) ? 1 : 0) :
                            //                                (x.KitchenId > 0 && (x.IsReadyByKitchen ?? false) ? 1 : 0),
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
                            DiscountAmt = x.DiscountAmt,
                            //TimeElapsed = ((long)DateTime.Now.Subtract(Convert.ToDateTime(x.OrderDate)).TotalMinutes).ToString(),
                            TimeElapsed = ((long)CommonFunctions.CourseTimeZoneDateTime(courseTimeZone, DateTime.UtcNow)
                                                .Subtract(CommonFunctions.CourseTimeZoneDateTime(courseTimeZone, x.OrderDate.Value)).TotalMinutes).ToString(),
                            IsNew = (((x.KitchenId ?? 0) <= 0) && ((x.CartieId ?? 0) <= 0) && ((x.ProShopID ?? 0) <= 0)),
                            HoleNo = x.HoleNo ?? 0,
                            //HoleNo = CourseWebAPI.Models.CommonFunctions.GetUserCurrentHoleNo(x.CourseID ?? 0,
                            //                             db.GF_GolferChangingLocation.FirstOrDefault(q => q.GolferId == x.GolferID) == null ? x.Latitude :
                            //                                            db.GF_GolferChangingLocation.OrderByDescending(q => q.TimeOfChange).FirstOrDefault(q => q.GolferId == x.GolferID).Latitude,
                            //                             db.GF_GolferChangingLocation.FirstOrDefault(q => q.GolferId == x.GolferID) == null ? x.Longitude :
                            //                                            db.GF_GolferChangingLocation.OrderByDescending(q => q.TimeOfChange).FirstOrDefault(q => q.GolferId == x.GolferID).Longitude)
                        }).OrderByDescending(x => x.orderID);

                var lstCourseUser = getActiveCourseUser(courseID.CourseId ?? 0, CourseAdminID ?? 0);

                if (lstOrders.Count() > 0)
                {
                    return new CourseOrderResult
                    {
                        Id = CourseAdminID ?? 0,
                        Status = 1,
                        Error = "Success",
                        record = lstOrders,
                        CourseUser = lstCourseUser
                    };
                }
                else
                {
                    return new CourseOrderResult
                    {
                        Id = CourseAdminID ?? 0,
                        Status = 0,
                        //Error = "No order(s) has been placed.",
                        Error = "There are currently no active orders.",
                        record = null,
                        CourseUser = lstCourseUser
                    };
                }
            }
            catch (Exception ex)
            {
                return new CourseOrderResult
                {
                    Id = CourseAdminID ?? 0,
                    Status = 0,
                    Error = ex.Message,
                    record = null,
                    CourseUser = null
                };
            }
        }

        #endregion

        #region Course Orders

        /// <summary>
        /// Created By: Amit Kumar
        /// Created Date: 26 March 2015
        /// Purpose: List of those Orders which is placed in course
        /// </summary>
        /// <param name="courseID"></param>
        /// <returns></returns>
        public Result GetCourseOrderList(GF_Order objOrder)
        {
            try
            {
                #region Order Wait

                //Check order wait time weather the it cross wait time limit or not
                OrderCancellationCheck((objOrder.CourseID ?? 0));

                #endregion

                if (objOrder.referenceType == UserType.Kitchen ||
                    objOrder.referenceType == UserType.Cartie ||
                    objOrder.referenceType == UserType.Proshop)
                {

                    #region Comments
                    //var lstOrders = db.GF_Order.Where(x => x.CourseID == objOrder.CourseID &&
                    //                                       x.IsDelivered == false &&
                    //                                       x.IsActive == true).ToList()
                    //    .Select(x =>
                    //        new
                    //        {
                    //            orderID = x.ID,
                    //            courseID = x.CourseID,
                    //            golferName = x.GF_Golfer.FirstName + " " + x.GF_Golfer.LastName,
                    //            x.Longitude,
                    //            x.Latitude,
                    //            time = x.OrderDate.Value.ToShortTimeString(),
                    //            itemsOrdered = x.GF_OrderDetails.ToList().Select(y =>
                    //                new
                    //                {
                    //                    y.GF_MenuItems.Name,
                    //                    UnitPrice = y.GF_MenuItems.Amount,
                    //                    y.Quantity,
                    //                    Amount = (y.GF_MenuItems.Amount * y.Quantity)
                    //                }),
                    //            billAmount = x.GF_OrderDetails.Sum(y => (y.GF_MenuItems.Amount * y.Quantity)),
                    //            TaxPercentage = x.GF_CourseInfo.Tax,
                    //            TaxAmount = ((x.GF_OrderDetails.Sum(y => (y.UnitPrice * y.Quantity))) * x.GF_CourseInfo.Tax / 100),
                    //            GolferPlatformFee = x.GolferPlatformFee,
                    //            Total = ((x.GF_OrderDetails.Sum(y => (y.UnitPrice * y.Quantity))) +
                    //                                                 ((x.GF_OrderDetails.Sum(y => (y.UnitPrice * y.Quantity))) * x.GF_CourseInfo.Tax / 100) +
                    //                                                 x.GolferPlatformFee),
                    //            //x.TaxAmount,
                    //            //x.GolferPlatformFee,
                    //            //Total = ((x.GF_OrderDetails.Sum(y => (y.GF_MenuItems.Amount * y.Quantity))) + x.TaxAmount + x.GolferPlatformFee),
                    //            OrderType = x.OrderType,
                    //            OrderDate = x.OrderDate.Value.ToString("MM/dd/yyyy"),
                    //            ReadyStatus = (x.KitchenId > 0) ? 1 : 0,
                    //            x.HEXColor,
                    //            x.RGBColor,
                    //            x.HUEColor
                    //        }).OrderByDescending(x => x.orderID);
                    #endregion

                    #region Get Course Club House ID

                    var ClubHouse = db.GF_CourseInfo.FirstOrDefault(x => x.ID == objOrder.CourseID && !(x.IsClubHouse ?? true));
                    long newCourseID = objOrder.CourseID ?? 0;
                    if (ClubHouse != null)
                    {
                        newCourseID = ClubHouse.ClubHouseID ?? 0;
                    }

                    #endregion

                    string courseTimeZone = CommonFunctions.CourseTimeZone(newCourseID);//objOrder.CourseID ?? 0); //Get Time Zone of Course

                    var lstRejectedOrders = db.GF_OrderAcceptReject.Where(x => x.ReferenceID == objOrder.referenceID &&
                        x.ReferenceType.ToLower().Contains(objOrder.referenceType.ToLower()));

                    var lstOrders = (from x in db.GF_Order
                                     join y in lstRejectedOrders on x.ID equals y.OrderID into z
                                     from p in z.DefaultIfEmpty()
                                     where x.CourseID == objOrder.CourseID && p.OrderID == null &&
                                           EntityFunctions.TruncateTime(x.OrderDate) == TodaysDate &&
                                           !(x.IsDelivered ?? false) &&
                                           !(x.IsRejected ?? false) &&
                                           (objOrder.referenceType == UserType.Cartie ? x.OrderType.Contains(OrderType.CartOrder) :
                                                x.OrderType.Contains(OrderType.TurnOrder)) &&
                                           (objOrder.referenceType == UserType.Cartie ? (x.CartieId ?? 0) <= 0 :
                                                (objOrder.referenceType == UserType.Proshop ? (x.ProShopID ?? 0) <= 0 :
                                                    (x.KitchenId ?? 0) <= 0))
                                     orderby x.ID
                                     select x).ToList()
                                    .Select(x =>
                                            new
                                            {
                                                orderID = x.ID,
                                                courseID = x.CourseID,
                                                golferID = x.GolferID,
                                                golferName = x.GF_Golfer.FirstName + " " + x.GF_Golfer.LastName,
                                                x.Longitude,
                                                x.Latitude,
                                                //Longitude = db.GF_GolferChangingLocation.FirstOrDefault(q => q.GolferId == x.GolferID) == null ? x.Longitude :
                                                //                db.GF_GolferChangingLocation.OrderByDescending(q => q.TimeOfChange).FirstOrDefault(q => q.GolferId == x.GolferID).Longitude,
                                                //Latitude = db.GF_GolferChangingLocation.FirstOrDefault(q => q.GolferId == x.GolferID) == null ? x.Latitude :
                                                //                db.GF_GolferChangingLocation.OrderByDescending(q => q.TimeOfChange).FirstOrDefault(q => q.GolferId == x.GolferID).Latitude,
                                                time = CommonFunctions.CourseTimeZoneDateTime(courseTimeZone, x.OrderDate.Value).ToShortTimeString(),//x.OrderDate.Value.ToShortTimeString(),
                                                itemsOrdered = x.GF_OrderDetails.ToList().Select(y =>
                                                    new
                                                    {
                                                        y.GF_MenuItems.Name,
                                                        y.UnitPrice,
                                                        y.Quantity,
                                                        Amount = (y.UnitPrice * y.Quantity),
                                                        MenuOptionName = y.GF_OrderMenuOptionDetail.Select(q => q.MenuOptionName).ToList()
                                                    }),
                                                billAmount = x.GF_OrderDetails.Sum(y => (y.UnitPrice * y.Quantity)),
                                                TaxPercentage = x.GF_CourseInfo.Tax,
                                                TaxAmount = x.TaxAmount,//((x.GF_OrderDetails.Sum(y => (y.UnitPrice * y.Quantity))) * x.GF_CourseInfo.Tax / 100),
                                                GolferPlatformFee = (x.GolferPlatformFee ?? 0),
                                                Total = x.GrandTotal,
                                                //Total = ((x.GF_OrderDetails.Sum(y => (y.UnitPrice * y.Quantity))) +
                                                //                                     ((x.GF_OrderDetails.Sum(y => (y.UnitPrice * y.Quantity))) * x.GF_CourseInfo.Tax / 100) +
                                                //                                     (x.GolferPlatformFee ?? 0)),
                                                OrderType = x.OrderType,
                                                OrderDate = CommonFunctions.CourseTimeZoneDateTime(courseTimeZone, x.OrderDate.Value),//x.OrderDate,
                                                //OrderDate = x.OrderDate.Value.ToString("MM/dd/yyyy"),
                                                //ReadyStatus = (x.KitchenId > 0) ? 1 : 0,
                                                //ReadyStatus = (objOrder.referenceType == UserType.Kitchen ? (x.KitchenId > 0 && (x.IsReadyByKitchen ?? false) ? 1 : 0) :
                                                //              (objOrder.referenceType == UserType.Proshop ? (x.ProShopID > 0 && (x.IsReadyByProshop ?? false) ? 1 : 0) :
                                                //                                                            ((x.IsReadyByKitchen ?? false) ? 1 : 0))),
                                                ReadyStatus = GetOrderReadyStatus(x.ID, UserType.Proshop),
                                                //ReadyStatus = (string.Join(",", x.GF_OrderDetails.ToList()
                                                //                                .Select(y => y.GF_MenuItems.GF_SubCategory.GF_Category.Type)
                                                //                                .Distinct().ToList())).ToLower() == FoodCategoryType.Proshop.ToLower() ?
                                                //                                (x.ProShopID > 0 && (x.IsReadyByProshop ?? false) ? 1 : 0) :
                                                //                                (x.KitchenId > 0 && (x.IsReadyByKitchen ?? false) ? 1 : 0),
                                                x.HEXColor,
                                                x.RGBColor,
                                                x.HUEColor,
                                                orderInclude = string.Join(",", x.GF_OrderDetails.ToList()
                                                                                .Select(y => y.GF_MenuItems.GF_SubCategory.GF_Category.Type)
                                                                                .Distinct().ToList()) + (x.OrderType == OrderType.CartOrder ? ",Cart" : ""),
                                                x.DiscountAmt,
                                                //TimeElapsed = ((long)DateTime.Now.Subtract(Convert.ToDateTime(x.OrderDate)).TotalMinutes).ToString()
                                                TimeElapsed = ((long)CommonFunctions.CourseTimeZoneDateTime(courseTimeZone, DateTime.UtcNow)
                                                                    .Subtract(CommonFunctions.CourseTimeZoneDateTime(courseTimeZone, x.OrderDate.Value)).TotalMinutes).ToString()
                                            }).OrderByDescending(x => x.orderID).ToList();

                    if (objOrder.referenceType == UserType.Kitchen)
                    {
                        lstOrders = lstOrders.Where(x => x.orderInclude != FoodCategoryType.Proshop).ToList();
                    }
                    else if (objOrder.referenceType == UserType.Proshop)
                    {
                        lstOrders = lstOrders.Where(x => (x.orderInclude != FoodCategoryType.Kitchen || x.orderInclude != FoodCategoryType.Cart)).ToList();
                        lstOrders = lstOrders.Where(x => x.orderInclude.ToLower().Contains(FoodCategoryType.Proshop.ToLower())).ToList();
                    }
                    else
                    {
                    }

                    if (lstOrders.Count() > 0)
                    {
                        long totalRec = lstOrders.Count();
                        long page = totalRec / (objOrder.pageSize ?? 10);

                        return new PageResult
                        {
                            Id = objOrder.CourseID ?? 0,
                            Status = 1,
                            Error = "Success",
                            record = lstOrders.Skip(((objOrder.pageIndex ?? 1) - 1) * (objOrder.pageSize ?? 10)).Take(objOrder.pageSize ?? 10),
                            totalRecords = totalRec,
                            pageCount = page + 1
                        };
                    }
                    else
                    {
                        return new PageResult
                        {
                            Id = objOrder.CourseID ?? 0,
                            Status = 0,
                            //Error = "No order(s) has been placed.",
                            Error = "There are currently no active orders.",
                            record = null,
                            totalRecords = 0,
                            pageCount = 0
                        };
                    }
                }
                else
                {
                    return new PageResult
                    {
                        Id = objOrder.CourseID ?? 0,
                        Status = 0,
                        Error = "Invalid user type",
                        record = null,
                        totalRecords = 0,
                        pageCount = 0
                    };
                }
            }
            catch (Exception ex)
            {
                return new PageResult
                {
                    Id = objOrder.CourseID ?? 0,
                    Status = 0,
                    Error = ex.Message,
                    record = null,
                    totalRecords = 0,
                    pageCount = 0
                };
            }
        }

        #endregion

        #region Get All Course Orders

        /// <summary>
        /// Created By: Amit Kumar
        /// Created Date: 25 April 2015
        /// Purpose: List of those Orders which is placed in course
        /// </summary>
        /// <param name="courseID"></param>
        /// <returns></returns>
        public List<OrderData> GetAllCourseOrderList(GF_Order objOrder)
        {
            try
            {
                #region Order Wait

                //Check order wait time weather it cross wait time limit or not
                OrderCancellationCheck((objOrder.CourseID ?? 0));

                #endregion

                if (objOrder.referenceType == UserType.Kitchen ||
                    objOrder.referenceType == UserType.Cartie ||
                    objOrder.referenceType == UserType.Proshop ||
                    objOrder.referenceType == UserType.CourseAdmin)
                {

                    #region Get Course Club House ID

                    var ClubHouse = db.GF_CourseInfo.FirstOrDefault(x => x.ID == objOrder.CourseID && !(x.IsClubHouse ?? true));
                    long newCourseID = objOrder.CourseID ?? 0;
                    if (ClubHouse != null)
                    {
                        newCourseID = ClubHouse.ClubHouseID ?? 0;
                    }

                    #endregion

                    string courseTimeZone = CommonFunctions.CourseTimeZone(newCourseID);//objOrder.CourseID ?? 0); //Get Time Zone of Course

                    var lstRejectedOrders = db.GF_OrderAcceptReject.Where(x => x.ReferenceID == objOrder.referenceID &&
                        x.ReferenceType.ToLower().Contains(objOrder.referenceType.ToLower()));

                    var lstOrders = (from x in db.GF_Order
                                     join y in lstRejectedOrders on x.ID equals y.OrderID into z
                                     from p in z.DefaultIfEmpty()
                                     where x.CourseID == objOrder.CourseID && p.OrderID == null &&
                                           EntityFunctions.TruncateTime(x.OrderDate) == TodaysDate &&
                                           !(x.IsDelivered ?? false) &&
                                           !(x.IsRejected ?? false) &&
                                           (objOrder.referenceType == UserType.Cartie ? x.OrderType.Contains(OrderType.CartOrder) :
                                                x.OrderType.Contains(OrderType.TurnOrder)) &&
                                           (objOrder.referenceType == UserType.Cartie ? (x.CartieId ?? 0) <= 0 :
                                                (objOrder.referenceType == UserType.Proshop ? (x.ProShopID ?? 0) <= 0 :
                                                    (objOrder.referenceType == UserType.CourseAdmin ? (x.ProShopID ?? 0) <= 0 :
                                                        (x.KitchenId ?? 0) <= 0)))
                                     orderby x.ID
                                     select x).ToList()
                                    .Select(x =>
                                            new OrderData
                                            {
                                                orderID = x.ID,
                                                courseID = x.CourseID,
                                                golferID = x.GolferID,
                                                golferName = x.GF_Golfer.FirstName + " " + x.GF_Golfer.LastName,
                                                Longitude = x.Longitude,
                                                Latitude = x.Latitude,
                                                //Longitude = db.GF_GolferChangingLocation.FirstOrDefault(q => q.GolferId == x.GolferID) == null ? x.Longitude :
                                                //                db.GF_GolferChangingLocation.OrderByDescending(q => q.TimeOfChange).FirstOrDefault(q => q.GolferId == x.GolferID).Longitude,
                                                //Latitude = db.GF_GolferChangingLocation.FirstOrDefault(q => q.GolferId == x.GolferID) == null ? x.Latitude :
                                                //                db.GF_GolferChangingLocation.OrderByDescending(q => q.TimeOfChange).FirstOrDefault(q => q.GolferId == x.GolferID).Latitude,
                                                time = CommonFunctions.CourseTimeZoneDateTime(courseTimeZone, x.OrderDate.Value).ToShortTimeString(),//x.OrderDate.Value.ToShortTimeString(),
                                                itemsOrdered = x.GF_OrderDetails.ToList().Select(y =>
                                                    new OrderDetails
                                                    {
                                                        Name = y.GF_MenuItems.Name,
                                                        UnitPrice = y.UnitPrice ?? 0,
                                                        Quantity = y.Quantity ?? 0,
                                                        Amount = ((y.UnitPrice ?? 0) * (y.Quantity ?? 0)),
                                                        MenuOptionName = y.GF_OrderMenuOptionDetail.Select(q => new MenuOptionName { ID = (q.MenuOptionID ?? 0), Name = q.MenuOptionName }).ToList()
                                                    }).ToList(),
                                                billAmount = x.GF_OrderDetails.Sum(y => (y.UnitPrice * y.Quantity)),
                                                TaxPercentage = x.GF_CourseInfo.Tax,
                                                TaxAmount = x.TaxAmount,
                                                GolferPlatformFee = (x.GolferPlatformFee ?? 0),
                                                Total = x.GrandTotal,
                                                OrderType = x.OrderType,
                                                OrderDate = CommonFunctions.CourseTimeZoneDateTime(courseTimeZone, x.OrderDate.Value),//x.OrderDate,
                                                //OrderDate = x.OrderDate.Value.ToString("MM/dd/yyyy"),
                                                ReadyStatus = (string.Join(",", x.GF_OrderDetails.ToList()
                                                                                .Select(y => y.GF_MenuItems.GF_SubCategory.GF_Category.Type)
                                                                                .Distinct().ToList())).ToLower() == FoodCategoryType.Proshop.ToLower() ?
                                                                                (x.ProShopID > 0 && (x.IsReadyByProshop ?? false) ? 1 : 0) :
                                                                                (x.KitchenId > 0 && (x.IsReadyByKitchen ?? false) ? 1 : 0),
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
                                                DiscountAmt = x.DiscountAmt,
                                                //TimeElapsed = ((long)DateTime.Now.Subtract(Convert.ToDateTime(x.OrderDate)).TotalMinutes).ToString(),
                                                TimeElapsed = ((long)CommonFunctions.CourseTimeZoneDateTime(courseTimeZone, DateTime.UtcNow)
                                                                    .Subtract(CommonFunctions.CourseTimeZoneDateTime(courseTimeZone, x.OrderDate.Value)).TotalMinutes).ToString(),
                                                IsNew = true,
                                                HoleNo = x.HoleNo ?? 0,
                                                //HoleNo = CourseWebAPI.Models.CommonFunctions.GetUserCurrentHoleNo(x.CourseID ?? 0,
                                                //         db.GF_GolferChangingLocation.FirstOrDefault(q => q.GolferId == x.GolferID) == null ? x.Latitude :
                                                //                        db.GF_GolferChangingLocation.OrderByDescending(q => q.TimeOfChange).FirstOrDefault(q => q.GolferId == x.GolferID).Latitude,
                                                //         db.GF_GolferChangingLocation.FirstOrDefault(q => q.GolferId == x.GolferID) == null ? x.Longitude :
                                                //                        db.GF_GolferChangingLocation.OrderByDescending(q => q.TimeOfChange).FirstOrDefault(q => q.GolferId == x.GolferID).Longitude),
                                                IsGolphieOrder = string.Join(",", x.GF_OrderDetails.ToList()
                                                                                .Select(y => y.GF_MenuItems.GF_SubCategory.GF_Category.CartType)
                                                                                .Distinct().ToList()).ToLower() == FoodCategoryType.Cart.ToLower()
                                            }).OrderByDescending(x => x.orderID).ToList();

                    if (objOrder.referenceType == UserType.Kitchen)
                    {
                        lstOrders = lstOrders.Where(x => x.orderInclude != FoodCategoryType.Proshop).ToList();
                    }
                    else if (objOrder.referenceType == UserType.Proshop)
                    {
                        lstOrders = lstOrders.Where(x => (x.orderInclude != FoodCategoryType.Kitchen || x.orderInclude != FoodCategoryType.Cart)).ToList();
                        lstOrders = lstOrders.Where(x => x.orderInclude.ToLower().Contains(FoodCategoryType.Proshop.ToLower())).ToList();
                    }
                    else
                    {
                    }

                    return lstOrders;
                }
                else
                {
                    return new List<OrderData>();
                }
            }
            catch (Exception ex)
            {
                return new List<OrderData>();
            }
        }

        #endregion

        /// <summary>
        /// Created By: Amit Kumar
        /// Created Date: 26 March 2015
        /// Purpose: Order is accept/reject by cartie/kitchen
        /// </summary>
        /// <param name="objOrder"></param>
        /// <returns></returns>
        public Result OrderAcceptReject(GF_OrderAcceptReject objOrder)
        {
            try
            {
                #region Check wheather user is Course Admin or not, then he is not authorised to perform requested action

                var lstCourseAdmin = db.GF_AdminUsers.FirstOrDefault(x => x.ID == objOrder.ReferenceID);

                if (lstCourseAdmin != null)
                {
                    if (lstCourseAdmin.Type == UserType.CourseAdmin)
                    {
                        return new Result
                        {
                            Id = objOrder.OrderID ?? 0,
                            Status = 0,
                            Error = "You are not authorised to accept/reject any order(s)."
                        };
                    }
                }

                #endregion

                #region Check for Order is already taken by another cartie/kitchen

                ///Comment: This area is used check if the order is taken by another cartie/kitchen.
                ///         If the result is true then return relevant message or proceeds to further process.

                if (objOrder.ReferenceType == UserType.Cartie)
                {
                    var chkOrder = db.GF_Order.FirstOrDefault(x => x.ID == objOrder.OrderID && x.CartieId == objOrder.ReferenceID);
                    if (chkOrder != null)
                    {
                        return new Result
                        {
                            Id = chkOrder.ID,
                            Status = 0,
                            Error = "Requested order is accepted by another cartie."
                        };
                    }
                }
                else if (objOrder.ReferenceType == UserType.Kitchen)
                {
                    var chkOrder = db.GF_Order.FirstOrDefault(x => x.ID == objOrder.OrderID && x.KitchenId == objOrder.ReferenceID);
                    if (chkOrder != null)
                    {
                        return new Result
                        {
                            Id = chkOrder.ID,
                            Status = 0,
                            Error = "Requested order is accepted by another kitchen."
                        };
                    }
                }
                else if (objOrder.ReferenceType == UserType.Proshop)
                {
                    var chkOrder = db.GF_Order.FirstOrDefault(x => x.ID == objOrder.OrderID && x.ProShopID == objOrder.ReferenceID);
                    if (chkOrder != null)
                    {
                        return new Result
                        {
                            Id = chkOrder.ID,
                            Status = 0,
                            Error = "Requested order is accepted by another proshop."
                        };
                    }
                }

                #endregion

                var order = db.GF_OrderAcceptReject.FirstOrDefault(x => x.OrderID == objOrder.OrderID &&
                    x.ReferenceID == objOrder.ReferenceID &&
                    x.ReferenceType == objOrder.ReferenceType);

                GF_Order obj = new GF_Order();

                if (order != null)
                {
                    return new Result
                    {
                        Id = objOrder.ReferenceID ?? 0,
                        Status = 1,
                        Error = (order.Status ?? false) ? "Order is already accepted." : "Order is already rejected."
                    };
                }
                else
                {
                    objOrder.CreatedDate = DateTime.UtcNow;
                    db.GF_OrderAcceptReject.Add(objOrder);
                    db.SaveChanges();

                    string pushMessage = "";
                    long SenderId = 0;
                    string SenderName = "";

                    #region Update Cartie/Kitchen/Proshop info in Order Table

                    ///Comment: Update the Cartie or Kitchen Id corresponding to orderID in GF_Order table.

                    obj = db.GF_Order.FirstOrDefault(x => x.ID == objOrder.OrderID);

                    if (objOrder.Status ?? false)
                    {
                        if (objOrder.ReferenceType == UserType.Cartie)
                            obj.CartieId = objOrder.ReferenceID;
                        else if (objOrder.ReferenceType == UserType.Kitchen)
                            obj.KitchenId = objOrder.ReferenceID;
                        else if (objOrder.ReferenceType == UserType.Proshop)
                            obj.ProShopID = objOrder.ReferenceID;

                        obj.OrderStatus = OrderStatus.Process;
                        obj.ModifyDate = DateTime.UtcNow;
                        db.SaveChanges();

                        #region Remove All rejected entries

                        ///Comment: Remove all rejected entries from the table GF_OrderAcceptReject only when accepted entry came.
                        ///Reason: After the acceptance of order then there is no longer use to hold rejected entry.

                        List<GF_OrderAcceptReject> objAccept = new List<GF_OrderAcceptReject>();
                        objAccept = db.GF_OrderAcceptReject.Where(x => x.OrderID == objOrder.OrderID &&
                            x.ReferenceID == objOrder.ReferenceID &&
                            x.ReferenceType == objOrder.ReferenceType).ToList();

                        foreach (var item in objAccept)
                            db.GF_OrderAcceptReject.Remove(item);
                        db.SaveChanges();

                        #endregion

                        pushMessage = "Your order has gone for processing.";
                        var senderName = db.GF_AdminUsers.FirstOrDefault(x => x.ID == (objOrder.ReferenceID ?? 1));
                        SenderId = objOrder.ReferenceID ?? 0;
                        SenderName = senderName.FirstName + " " + senderName.LastName;

                        #region Order Ready

                        //Order should be ready if the order is Cart Order and menu item involves on cart items

                        var lstOrder = db.GF_Order.FirstOrDefault(x => x.ID == obj.ID && x.OrderType == OrderType.CartOrder);
                        if (lstOrder != null)
                        {
                            bool onlyCart = (string.Join(",", lstOrder.GF_OrderDetails.ToList()
                                                                 .Select(y => y.GF_MenuItems.GF_SubCategory.GF_Category.CartType)
                                                                 .Distinct().ToList())).ToLower() == FoodCategoryType.Cart.ToLower();

                            if (onlyCart && (objOrder.Status ?? false))
                            {
                                lstOrder.OrderStatus = OrderStatus.Ready;
                                db.SaveChanges();

                                //return new Result
                                //{
                                //    Id = objOrder.ReferenceID ?? 0,
                                //    Status = 1,
                                //    Error = "Order is accepted successfully.",
                                //    record = null
                                //};
                            }
                        }

                        #endregion
                    }
                    else
                    {
                        //When order is rejected by all user according to reference type then send a notifcation
                        if (rejectOrder(obj.CourseID ?? 0, objOrder.OrderID ?? 0, objOrder.ReferenceType, objOrder.ReferenceID ?? 0))
                        {
                            obj.OrderStatus = OrderStatus.Reject;
                            obj.IsRejected = true;
                            db.SaveChanges();

                            //Call Reset quantity of those orders which is rejected
                            resetMenuItemQuantity(objOrder.OrderID ?? 0);

                            //pushMessage = "Your order is not able to processed. Please contact course administrator. Order status is : Rejected.";
                            pushMessage = "Your order is cancelled due to " + UserType.GetFullUserType(objOrder.ReferenceType) + " item is not avaiable.";

                            #region Get Course Club House ID

                            var ClubHouse = db.GF_CourseInfo.FirstOrDefault(x => x.ID == obj.CourseID && !(x.IsClubHouse ?? true));
                            long newCourseID = obj.CourseID ?? 0;
                            if (ClubHouse != null)
                            {
                                newCourseID = ClubHouse.ClubHouseID ?? 0;
                            }

                            #endregion

                            var sender = db.GF_AdminUsers.FirstOrDefault(x => x.CourseId == newCourseID && //(obj.CourseID ?? 0) &&
                                (x.Type == UserType.CourseAdmin || x.Type == UserType.Proshop));
                            SenderId = sender == null ? 0 : sender.ID;
                            SenderName = sender == null ? "Support Team" : sender.FirstName + " " + sender.LastName;
                        }
                    }

                    #endregion

                    #region Push Notification



                    //Case : Cart Orders
                    //Service  :- /Course/OrderAcceptReject
                    //Sender :- Cartie 
                    //Receiver :- Golfer 

                    //Case  Turn Orders
                    //Service  :- /Course/OrderAcceptReject
                    //Sender :- Kitchen , Pro shop
                    //Receiver :- Golfer

                    bool IsMessageToGolfer = true;

                    PushNotications pushNotications = new PushNotications();

                    pushNotications.SenderId = SenderId;
                    pushNotications.SenderName = SenderName;

                    pushNotications.ReceiverId = obj.GolferID ?? 0;
                    pushNotications.ReceiverName = obj.GF_Golfer.FirstName + " " + obj.GF_Golfer.LastName;
                    pushNotications.pushMsgFrom = PushnoficationMsgFrom.Course;
                    pushNotications.DeviceType = obj.GF_Golfer.DeviceType;

                    if ((obj.GF_Golfer.AppVersion ?? 0) > 0)
                    {
                        var jString = new
                        {
                            ScreenName = AppScreenName.EnRoute,
                            Message = pushMessage,
                            Data = new { OrderID = obj.ID.ToString() }
                        };
                        string jsonString = JsonConvert.SerializeObject(jString);
                        if (pushNotications.DeviceType.ToLower() == "ios")
                        {
                            pushNotications.Message = pushMessage;
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
                            pushNotications.Message = pushMessage;
                        }
                        else
                        {
                            pushNotications.Message = "\"" + pushMessage + "\"";
                        }
                    }

                    SendRecieveNotification.callingPushNotification(pushNotications, IsMessageToGolfer);

                    #endregion

                    return new Result
                    {
                        Id = objOrder.ReferenceID ?? 0,
                        Status = 1,
                        Error = (objOrder.Status ?? false) ? "Order is accepted successfully." : "Order is rejected successfully.",
                        record = null
                    };
                }
            }
            catch (Exception ex)
            {
                return new Result
                {
                    Id = objOrder.ReferenceID ?? 0,
                    Status = 0,
                    Error = ex.Message,
                    record = null
                };
            }
        }

        /// <summary>
        /// Created By: Amit Kumar
        /// Created Date: 26 March 2015
        /// Purpose: Update the order delivery status
        /// </summary>
        /// <param name="objOrder"></param>
        /// <returns></returns>
        public Result SaveOrderDeliveryStatus(GF_OrderAcceptReject objOrder)
        {
            try
            {
                if (objOrder.ReferenceType == UserType.Cartie)
                {
                    var lstOrder = db.GF_Order.Where(x => x.ID == objOrder.OrderID && x.CartieId == objOrder.ReferenceID).ToList();
                    if (lstOrder.Count() <= 0)
                    {
                        return new Result
                        {
                            Id = objOrder.OrderID ?? 0,
                            Status = 0,
                            Error = "No cartie user found against requested order."
                        };
                    }

                    var chkOrder = db.GF_Order.FirstOrDefault(x => x.ID == objOrder.OrderID &&
                        x.CartieId == objOrder.ReferenceID &&
                        (x.IsDelivered ?? false) == true);
                    if (chkOrder != null)
                    {
                        return new Result
                        {
                            Id = objOrder.OrderID ?? 0,
                            Status = 0,
                            Error = "Order is already delivered."
                        };
                    }
                    else
                    {
                        GF_Order obj = new GF_Order();
                        obj = db.GF_Order.FirstOrDefault(x => x.ID == objOrder.OrderID && x.CartieId == objOrder.ReferenceID);
                        obj.IsDelivered = true;
                        obj.OrderStatus = OrderStatus.Delivered;
                        obj.ModifyDate = DateTime.Now;
                        db.SaveChanges();

                        return new Result
                        {
                            Id = obj.ID,
                            Status = ((obj.IsDelivered ?? false) ? 1 : 0),
                            Error = ((obj.IsDelivered ?? false) ? "Order is delivered successfully." : "Order is not delivered.")
                        };
                    }
                }
                else if (objOrder.ReferenceType == UserType.Kitchen)
                {
                    var lstOrder = db.GF_Order.Where(x => x.ID == objOrder.OrderID && x.KitchenId == objOrder.ReferenceID).ToList();
                    if (lstOrder.Count() <= 0)
                    {
                        return new Result
                        {
                            Id = objOrder.OrderID ?? 0,
                            Status = 0,
                            Error = "No kitchen user found against requested order."
                        };
                    }

                    var chkOrder = db.GF_Order.FirstOrDefault(x => x.ID == objOrder.OrderID &&
                        x.KitchenId == objOrder.ReferenceID &&
                        (x.IsDelivered ?? false) == true);
                    if (chkOrder != null)
                    {
                        return new Result
                        {
                            Id = objOrder.OrderID ?? 0,
                            Status = 0,
                            Error = "Order is already delivered."
                        };
                    }
                    else
                    {
                        GF_Order obj = new GF_Order();
                        obj = db.GF_Order.FirstOrDefault(x => x.ID == objOrder.OrderID && x.KitchenId == objOrder.ReferenceID);
                        obj.IsDelivered = true;
                        obj.ModifyDate = DateTime.Now;
                        db.SaveChanges();

                        return new Result
                        {
                            Id = obj.ID,
                            Status = ((obj.IsDelivered ?? false) ? 1 : 0),
                            Error = ((obj.IsDelivered ?? false) ? "Order is delivered successfully." : "Order is not delivered.")
                        };
                    }
                }
                else
                {
                    return new Result
                    {
                        Id = objOrder.ReferenceID ?? 0,
                        Status = 0,
                        Error = "No order found."
                    };
                }
            }
            catch (Exception ex)
            {
                return new Result
                {
                    Id = objOrder.ReferenceID ?? 0,
                    Status = 0,
                    Error = ex.Message,
                    record = null
                };
            }
        }

        #region Order History

        /// <summary>
        /// Created By: Amit Kumar
        /// Created Date: 30 March 2015
        /// Purpose: List of Orders History
        /// </summary>
        /// <param name="orderHistory"></param>
        /// <returns></returns>
        public Result GetOrderHistoryList(OrderHistory orderHistory)
        {
            try
            {
                #region Course Admin

                var lstCouresAdmin = db.GF_AdminUsers.FirstOrDefault(x => x.ID == orderHistory.referenceID);

                if (lstCouresAdmin != null)
                {
                    if (lstCouresAdmin.Type == UserType.CourseAdmin)
                        orderHistory.referenceType = UserType.CourseAdmin;
                }

                long courseID = lstCouresAdmin != null ? (lstCouresAdmin.CourseId ?? 0) : 0;

                #endregion

                if (orderHistory.referenceType == UserType.Kitchen ||
                    orderHistory.referenceType == UserType.Cartie ||
                    orderHistory.referenceType == UserType.Proshop ||
                    orderHistory.referenceType == UserType.CourseAdmin)
                {
                    string courseTimeZone = CommonFunctions.CourseTimeZone(courseID); //Get Time Zone of Course

                    string noImage = "/Uploads/ProfileImage/no_profile_image.png";

                    var lstOrders = db.GF_Order.Where(x =>
                        (orderHistory.referenceType == UserType.Cartie ? x.CartieId == orderHistory.referenceID :
                            (orderHistory.referenceType == UserType.Proshop ? x.ProShopID == orderHistory.referenceID :
                                (orderHistory.referenceType == UserType.CourseAdmin ? (x.CourseID ?? 0) == courseID :
                                    x.KitchenId == orderHistory.referenceID))) &&
                                    !(x.IsRejected ?? false)).ToList()
                        //&& (x.OrderType == OrderType.CartOrder ? (x.IsDelivered ?? false) == true : (x.IsPickup ?? false) == true)
                        .Select(x =>
                            new
                            {
                                orderID = x.ID,
                                courseID = x.CourseID ?? 0,
                                //OrderDate = x.OrderDate.Value.ToString("MM/dd/yyyy"),
                                OrderDate = CommonFunctions.CourseTimeZoneDateTime(courseTimeZone, x.OrderDate.Value).ToString("MM/dd/yyyy"),//x.OrderDate,
                                golferID = x.GolferID ?? 0,
                                golferName = x.GF_Golfer.FirstName + " " + x.GF_Golfer.LastName,
                                x.Longitude,
                                x.Latitude,
                                time = CommonFunctions.CourseTimeZoneDateTime(courseTimeZone, x.OrderDate.Value).ToShortTimeString(),//x.OrderDate.Value.ToShortTimeString(),
                                itemsOrdered = x.GF_OrderDetails.ToList().Select(y =>
                                    new OrderDetails
                                    {
                                        Name = y.GF_MenuItems.Name,
                                        UnitPrice = y.UnitPrice ?? 0,
                                        Quantity = y.Quantity ?? 0,
                                        Amount = ((y.UnitPrice ?? 0) * (y.Quantity ?? 0)),
                                        MenuOptionName = y.GF_OrderMenuOptionDetail.Select(q => new MenuOptionName { ID = (q.MenuOptionID ?? 0), Name = q.MenuOptionName }).ToList()
                                    }).ToList(),
                                billAmount = x.GF_OrderDetails.Sum(y => ((y.UnitPrice ?? 0) * (y.Quantity ?? 0))),
                                TaxPercentage = x.GF_CourseInfo.Tax,
                                TaxAmount = x.TaxAmount ?? 0,//((x.GF_OrderDetails.Sum(y => (y.UnitPrice * y.Quantity))) * x.GF_CourseInfo.Tax / 100),
                                GolferPlatformFee = (x.GolferPlatformFee ?? 0),
                                //Total = ((x.GF_OrderDetails.Sum(y => (y.UnitPrice * y.Quantity))) +
                                //                                     ((x.GF_OrderDetails.Sum(y => (y.UnitPrice * y.Quantity))) * x.GF_CourseInfo.Tax / 100) +
                                //                                     (x.GolferPlatformFee ?? 0)),
                                Total = x.GrandTotal ?? 0,
                                cartieName = (x.CartieId != null ? (db.GF_AdminUsers.FirstOrDefault(y => y.ID == (x.CartieId ?? 0)) != null ? ((db.GF_AdminUsers.FirstOrDefault(y => y.ID == (x.CartieId ?? 0)).FirstName + " " +
                                    db.GF_AdminUsers.FirstOrDefault(y => y.ID == (x.CartieId ?? 0)).LastName)) : "") : ""),
                                cartieImage = ConfigClass.SiteImageUrl + (x.CartieId != null ? (db.GF_AdminUsers.FirstOrDefault(y => y.ID == (x.CartieId ?? 0)) != null ? (db.GF_AdminUsers.FirstOrDefault(y => y.ID == (x.CartieId ?? 0)).Image ?? noImage) : noImage) : noImage),
                                cartieRating = (db.GF_GolferRating.Where(y => (y.ReferenceID ?? 0) == (x.CartieId ?? 0) && y.ReferenceType == UserType.Cartie)
                                                                        .Average(y => y.Rating)) ?? -1,
                                x.OrderType,
                                orderInclude = string.Join(",", x.GF_OrderDetails.ToList()
                                                                .Select(y => y.GF_MenuItems.GF_SubCategory.GF_Category.Type)
                                                                .Distinct().ToList()) + (x.OrderType == OrderType.CartOrder ? ",Cart" : ""),
                                orderStatus = x.OrderStatus,
                                DiscountAmt = x.DiscountAmt ?? 0,
                                //TimeElapsed = ((long)DateTime.Now.Subtract(Convert.ToDateTime(x.OrderDate)).TotalMinutes).ToString(),
                                TimeElapsed = ((long)CommonFunctions.CourseTimeZoneDateTime(courseTimeZone, DateTime.UtcNow)
                                                .Subtract(CommonFunctions.CourseTimeZoneDateTime(courseTimeZone, x.OrderDate.Value)).TotalMinutes).ToString(),
                                PaymentMode = x.PaymentType == "0" ? "Membership" : "Card",
                                MemberhsipID = x.PaymentType == "0" ? (x.MemberShipID ?? "0") : "0"
                            });

                    if (!string.IsNullOrEmpty(orderHistory.name))
                    {
                        lstOrders = lstOrders.Where(x => x.golferName.ToLower().Contains(orderHistory.name.ToLower())).OrderByDescending(x => x.orderID);
                    }

                    if (!string.IsNullOrEmpty(orderHistory.orderDate))
                    {
                        string orderDate = Convert.ToDateTime(orderHistory.orderDate).ToString("MM/dd/yyyy");
                        lstOrders = lstOrders.Where(x => x.OrderDate.Contains(orderDate)).OrderByDescending(x => x.orderID);
                    }

                    if (orderHistory.OrderId > 0)
                    {
                        lstOrders = lstOrders.Where(x => x.orderID == orderHistory.OrderId).OrderByDescending(x => x.orderID);
                    }

                    if (lstOrders.Count() > 0)
                    {
                        long totalRec = lstOrders.Count();
                        long page = totalRec / (orderHistory.pageSize ?? 10);

                        return new PageResult
                        {
                            Id = orderHistory.referenceID,
                            Status = 1,
                            Error = "Success",
                            record = lstOrders.OrderByDescending(x => x.orderID).Skip(((orderHistory.pageIndex ?? 1) - 1) * (orderHistory.pageSize ?? 10)).Take(orderHistory.pageSize ?? 10),
                            totalRecords = totalRec,
                            pageCount = page + 1
                        };
                    }
                    else
                    {
                        return new PageResult
                        {
                            Id = orderHistory.referenceID,
                            Status = 0,
                            Error = "No order(s) has been placed.",
                            record = null,
                            totalRecords = 0,
                            pageCount = 0
                        };
                    }
                }
                else
                {
                    return new PageResult
                    {
                        Id = orderHistory.referenceID,
                        Status = 0,
                        Error = "Invalid user type",
                        record = null,
                        totalRecords = 0,
                        pageCount = 0
                    };
                }
            }
            catch (Exception ex)
            {
                return new PageResult
                {
                    Id = orderHistory.referenceID,
                    Status = 0,
                    Error = ex.Message,
                    record = null,
                    totalRecords = 0,
                    pageCount = 0
                };
            }
        }

        #endregion Order History

        #region Order Ready/Delivery/Pickup

        /// <summary>
        /// Created By: Amit Kumar
        /// Created Date: 02 April 2015
        /// Purpose: Update the order ready status
        /// </summary>
        /// <param name="objOrder"></param>
        /// <returns></returns>
        public Result UpdateOrderReadyStatus(GF_Order objOrder)
        {
            try
            {
                #region Check wheather user is Course Admin or not, then he is not authorised to perform requested action

                var lstCourseAdmin = db.GF_AdminUsers.FirstOrDefault(x => x.ID == objOrder.referenceID);

                if (lstCourseAdmin != null)
                {
                    if (lstCourseAdmin.Type == UserType.CourseAdmin)
                    {
                        return new Result
                        {
                            Id = objOrder.OrderId ?? 0,
                            Status = 0,
                            Error = "You are not authorised to ready any order."
                        };
                    }
                }

                #endregion

                #region Check wheather the order is exist or not

                var lstOrder = db.GF_Order.Where(x => x.ID == objOrder.OrderId).ToList();
                if (lstOrder.Count() <= 0)
                {
                    return new Result
                    {
                        Id = objOrder.OrderId ?? 0,
                        Status = 0,
                        Error = "No order found."
                    };
                }

                #endregion

                var chkOrder = db.GF_Order.FirstOrDefault(x => x.ID == objOrder.OrderId &&
                               (objOrder.referenceType == UserType.Kitchen ? ((x.KitchenId ?? 0) > 0 && (x.IsReadyByKitchen ?? false)) :
                               ((x.ProShopID ?? 0) > 0 && (x.IsReadyByProshop ?? false))));
                if (chkOrder != null)
                {
                    return new Result
                    {
                        Id = objOrder.OrderId ?? 0,
                        Status = 1,
                        Error = "Order is already ready."
                    };
                }
                else
                {
                    GF_Order obj = new GF_Order();
                    obj = db.GF_Order.FirstOrDefault(x => x.ID == objOrder.OrderId);

                    if (objOrder.referenceType == UserType.Kitchen)
                    {
                        obj.KitchenId = objOrder.referenceID;
                        obj.IsReadyByKitchen = true;
                    }
                    else
                    {
                        obj.ProShopID = objOrder.referenceID;
                        obj.IsReadyByProshop = true;
                    }


                    obj.OrderStatus = OrderStatus.Ready;
                    obj.ModifyDate = DateTime.UtcNow;
                    db.SaveChanges();

                    #region Push Notification

                    string orderInclude = string.Join(",", obj.GF_OrderDetails.ToList()
                                                                   .Select(y => y.GF_MenuItems.GF_SubCategory.GF_Category.Type)
                                                                   .Distinct().ToList());

                    bool IsMessageToGolfer = true;

                    PushNotications pushNotications = new PushNotications();

                    var senderName = db.GF_AdminUsers.FirstOrDefault(x => x.ID == (objOrder.referenceID ?? 1));// && x.Type == objOrder.referenceType);

                    if (orderInclude == FoodCategoryType.Proshop && obj.OrderType == OrderType.TurnOrder && objOrder.referenceType == UserType.Proshop)
                    {
                        //Case : Turn Order (If order contains only Pro shop items)
                        //Service :- Course/OrderReadyStatus
                        //Sender : Pro Shop
                        //Receiver : Golfer
                        //Message : Order is ready to be picked up.

                        pushNotications.SenderId = objOrder.referenceID ?? 0;
                        pushNotications.SenderName = senderName.FirstName + " " + senderName.LastName;

                        pushNotications.ReceiverId = obj.GolferID ?? 0;
                        pushNotications.ReceiverName = obj.GF_Golfer.FirstName + " " + obj.GF_Golfer.LastName;
                        pushNotications.pushMsgFrom = PushnoficationMsgFrom.Course;
                        pushNotications.DeviceType = obj.GF_Golfer.DeviceType;

                        if ((obj.GF_Golfer.AppVersion ?? 0) > 0)
                        {
                            var jString = new
                            {
                                ScreenName = AppScreenName.EnRoute,
                                Message = "Order is ready to be picked up.",
                                Data = new { OrderID = obj.ID.ToString() }
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
                    }

                    if (orderInclude == FoodCategoryType.Proshop && obj.OrderType == OrderType.CartOrder && objOrder.referenceType == UserType.Proshop)
                    {
                        //Case : Cart Order (If order contains only Pro shop items)
                        //Service :- Course/OrderReadyStatus
                        //Sender : Pro Shop
                        //Receiver : Cartie
                        //Message : Order is ready for delivery.

                        pushNotications.SenderId = objOrder.referenceID ?? 0;
                        pushNotications.SenderName = senderName.FirstName + " " + senderName.LastName;

                        pushNotications.ReceiverId = obj.CartieId ?? 0;
                        var recieverName = db.GF_AdminUsers.FirstOrDefault(x => x.ID == pushNotications.ReceiverId && x.Type == UserType.Cartie);
                        pushNotications.ReceiverName = recieverName != null ? recieverName.FirstName + " " + recieverName.LastName : "Support Team";

                        pushNotications.DeviceType = recieverName != null ? recieverName.DeviceType : "";

                        if ((recieverName.AppVersion ?? 0) > 0)
                        {
                            var jString = new
                            {
                                ScreenName = AppScreenName.ActiveOrder,
                                Message = "Order is ready for delivery.",
                                Data = new { OrderID = obj.ID.ToString() }
                            };
                            string jsonString = JsonConvert.SerializeObject(jString);

                            if (pushNotications.DeviceType.ToLower() == "ios")
                            {
                                pushNotications.Message = "Order is ready for delivery.";
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
                                pushNotications.Message = "Order is ready for delivery.";
                            }
                            else
                            {
                                pushNotications.Message = "\"Order is ready for delivery.\"";
                            }
                        }

                        IsMessageToGolfer = false;

                        SendRecieveNotification.callingPushNotification(pushNotications, IsMessageToGolfer);
                    }

                    if (orderInclude.Contains(FoodCategoryType.Proshop) && orderInclude.Contains(FoodCategoryType.Kitchen) &&
                             obj.OrderType == OrderType.TurnOrder && objOrder.referenceType == UserType.Proshop)
                    {
                        //Case : Turn Order (If order contains Pro shop and Kitchen items)
                        //Service :- Course/OrderReadyStatus
                        //Sender : Pro Shop
                        //Receiver : Kitchen
                        //Message : Order is Ready.

                        pushNotications.SenderId = objOrder.referenceID ?? 0;
                        pushNotications.SenderName = senderName.FirstName + " " + senderName.LastName;

                        pushNotications.ReceiverId = obj.KitchenId ?? 0;
                        var recieverName = db.GF_AdminUsers.FirstOrDefault(x => x.ID == pushNotications.ReceiverId && x.Type == UserType.Kitchen);

                        pushNotications.ReceiverName = recieverName != null ? recieverName.FirstName + " " + recieverName.LastName : "Support Team";

                        pushNotications.DeviceType = recieverName != null ? recieverName.DeviceType : "";

                        if ((recieverName.AppVersion ?? 0) > 0)
                        {
                            var jString = new
                            {
                                ScreenName = AppScreenName.ActiveOrder,
                                Message = "Order is ready.",
                                Data = new { OrderID = obj.ID.ToString() }
                            };
                            string jsonString = JsonConvert.SerializeObject(jString);

                            if (pushNotications.DeviceType.ToLower() == "ios")
                            {
                                pushNotications.Message = "Order is Ready.";
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
                                pushNotications.Message = "Order is Ready.";
                            }
                            else
                            {
                                pushNotications.Message = "\"Order is Ready.\"";
                            }
                        }

                        IsMessageToGolfer = false;

                        SendRecieveNotification.callingPushNotification(pushNotications, IsMessageToGolfer);
                    }

                    if (orderInclude.Contains(FoodCategoryType.Proshop) && orderInclude.Contains(FoodCategoryType.Kitchen) &&
                             obj.OrderType == OrderType.CartOrder && objOrder.referenceType == UserType.Proshop)
                    {
                        //Case : Cart Order (If order contains Pro shop and Kitchen items)
                        //Service :- Course/OrderReadyStatus
                        //Sender : Pro Shop
                        //Receiver : Kitchen
                        //Message : Order is Ready.

                        pushNotications.SenderId = objOrder.referenceID ?? 0;
                        pushNotications.SenderName = senderName.FirstName + " " + senderName.LastName;

                        pushNotications.ReceiverId = obj.KitchenId ?? 0;
                        var recieverName = db.GF_AdminUsers.FirstOrDefault(x => x.ID == pushNotications.ReceiverId && x.Type == UserType.Kitchen);

                        pushNotications.ReceiverName = recieverName != null ? recieverName.FirstName + " " + recieverName.LastName : "Support Team";

                        IsMessageToGolfer = false;

                        pushNotications.DeviceType = recieverName != null ? recieverName.DeviceType : "";

                        if ((recieverName.AppVersion ?? 0) > 0)
                        {
                            var jString = new
                            {
                                ScreenName = AppScreenName.ActiveOrder,
                                Message = "Order is ready.",
                                Data = new { OrderID = obj.ID.ToString() }
                            };
                            string jsonString = JsonConvert.SerializeObject(jString);

                            if (pushNotications.DeviceType.ToLower() == "ios")
                            {
                                pushNotications.Message = "Order is Ready.";
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
                                pushNotications.Message = "Order is Ready.";
                            }
                            else
                            {
                                pushNotications.Message = "\"Order is Ready.\"";
                            }
                        }

                        SendRecieveNotification.callingPushNotification(pushNotications, IsMessageToGolfer);
                    }

                    if (orderInclude.Contains(FoodCategoryType.Proshop) && orderInclude.Contains(FoodCategoryType.Kitchen) &&
                             obj.OrderType == OrderType.CartOrder && objOrder.referenceType == UserType.Kitchen)
                    {
                        //Case : Cart Order (If order contains Pro shop and Kitchen items)
                        //Service :- Course/OrderReadyStatus
                        //Sender : Kitchen
                        //Receiver : Cartie
                        //Message : Order is ready for delivery.

                        pushNotications.SenderId = objOrder.referenceID ?? 0;
                        pushNotications.SenderName = senderName.FirstName + " " + senderName.LastName;

                        pushNotications.ReceiverId = obj.CartieId ?? 0;
                        var recieverName = db.GF_AdminUsers.FirstOrDefault(x => x.ID == pushNotications.ReceiverId && x.Type == UserType.Cartie);
                        pushNotications.ReceiverName = recieverName != null ? recieverName.FirstName + " " + recieverName.LastName : "Support Team";

                        IsMessageToGolfer = false;

                        pushNotications.DeviceType = recieverName != null ? recieverName.DeviceType : "";

                        if ((recieverName.AppVersion ?? 0) > 0)
                        {
                            var jString = new
                            {
                                ScreenName = AppScreenName.ActiveOrder,
                                Message = "Order is ready for delivery.",
                                Data = new { OrderID = obj.ID.ToString() }
                            };
                            string jsonString = JsonConvert.SerializeObject(jString);

                            if (pushNotications.DeviceType.ToLower() == "ios")
                            {
                                pushNotications.Message = "Order is Ready for delivery.";
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
                                pushNotications.Message = "Order is Ready for delivery.";
                            }
                            else
                            {
                                pushNotications.Message = "\"Order is Ready for delivery.\"";
                            }
                        }

                        SendRecieveNotification.callingPushNotification(pushNotications, IsMessageToGolfer);
                    }

                    if (orderInclude.Contains(FoodCategoryType.Proshop) && orderInclude.Contains(FoodCategoryType.Kitchen) &&
                             obj.OrderType == OrderType.TurnOrder && objOrder.referenceType == UserType.Kitchen)
                    {
                        //Case : Turn Order (If order contains Pro shop and Kitchen items)
                        //Service :- Course/OrderReadyStatus
                        //Sender : Kitchen
                        //Receiver : Golfer
                        //Message : Order is ready to be picked up.

                        pushNotications.SenderId = objOrder.referenceID ?? 0;
                        pushNotications.SenderName = senderName.FirstName + " " + senderName.LastName;

                        pushNotications.ReceiverId = obj.GolferID ?? 0;
                        pushNotications.ReceiverName = obj.GF_Golfer.FirstName + " " + obj.GF_Golfer.LastName;
                        pushNotications.pushMsgFrom = PushnoficationMsgFrom.Course;
                        IsMessageToGolfer = true;

                        pushNotications.DeviceType = obj.GF_Golfer.DeviceType;

                        if ((obj.GF_Golfer.AppVersion ?? 0) > 0)
                        {
                            var jString = new
                            {
                                ScreenName = AppScreenName.EnRoute,
                                Message = "Order is ready to be picked up.",
                                Data = new { OrderID = obj.ID.ToString() }
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

                        SendRecieveNotification.callingPushNotification(pushNotications, IsMessageToGolfer);
                    }

                    if (orderInclude == FoodCategoryType.Kitchen && obj.OrderType == OrderType.CartOrder && objOrder.referenceType == UserType.Kitchen)
                    {
                        //Case : Cart Order (If order contains only Kitchen items)
                        //Service :- Course/OrderReadyStatus
                        //Sender : Kitchen
                        //Receiver : Cartie
                        //Message : Order is ready for delivery.

                        pushNotications.SenderId = objOrder.referenceID ?? 0;
                        pushNotications.SenderName = senderName.FirstName + " " + senderName.LastName;

                        pushNotications.ReceiverId = obj.CartieId ?? 0;
                        var recieverName = db.GF_AdminUsers.FirstOrDefault(x => x.ID == pushNotications.ReceiverId && x.Type == UserType.Cartie);
                        pushNotications.ReceiverName = recieverName != null ? recieverName.FirstName + " " + recieverName.LastName : "Support Team";

                        IsMessageToGolfer = false;

                        pushNotications.DeviceType = recieverName != null ? recieverName.DeviceType : "";

                        if ((recieverName.AppVersion ?? 0) > 0)
                        {
                            var jString = new
                            {
                                ScreenName = AppScreenName.ActiveOrder,
                                Message = "Order is ready for delivery.",
                                Data = new { OrderID = obj.ID.ToString() }
                            };
                            string jsonString = JsonConvert.SerializeObject(jString);

                            if (pushNotications.DeviceType.ToLower() == "ios")
                            {
                                pushNotications.Message = "Order is Ready for delivery.";
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
                                pushNotications.Message = "Order is Ready for delivery.";
                            }
                            else
                            {
                                pushNotications.Message = "\"Order is Ready for delivery.\"";
                            }
                        }

                        SendRecieveNotification.callingPushNotification(pushNotications, IsMessageToGolfer);
                    }

                    if (orderInclude == FoodCategoryType.Kitchen && obj.OrderType == OrderType.TurnOrder && objOrder.referenceType == UserType.Kitchen)
                    {
                        //Case : Turn Order (If order contains only Kitchen items)
                        //Service :- Course/OrderReadyStatus
                        //Sender : Kitchen
                        //Receiver : Golfer
                        //Message : Order is ready to be picked up.

                        pushNotications.SenderId = objOrder.referenceID ?? 0;
                        pushNotications.SenderName = senderName.FirstName + " " + senderName.LastName;

                        pushNotications.ReceiverId = obj.GolferID ?? 0;
                        pushNotications.ReceiverName = obj.GF_Golfer.FirstName + " " + obj.GF_Golfer.LastName;
                        pushNotications.pushMsgFrom = PushnoficationMsgFrom.Course;
                        IsMessageToGolfer = true;

                        pushNotications.DeviceType = obj.GF_Golfer.DeviceType;

                        if ((obj.GF_Golfer.AppVersion ?? 0) > 0)
                        {
                            var jString = new
                            {
                                ScreenName = AppScreenName.EnRoute,
                                Message = "Order is ready to be picked up.",
                                Data = new { OrderID = obj.ID.ToString() }
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

                        SendRecieveNotification.callingPushNotification(pushNotications, IsMessageToGolfer);
                    }

                    #endregion

                    return new Result
                    {
                        Id = obj.ID,
                        Status = 1,
                        Error = "Order ready successfully."
                    };
                }
            }
            catch (Exception ex)
            {
                return new Result
                {
                    Id = objOrder.OrderId ?? 0,
                    Status = 0,
                    Error = ex.Message,
                    record = null
                };
            }
        }

        /// <summary>
        /// Created By: Amit Kumar
        /// Created Date: 02 April 2015
        /// Purpose: Update the order delivery status
        /// </summary>
        /// <param name="objOrder"></param>
        /// <returns></returns>
        public Result UpdateOrderDeliveryStatus(GF_Order objOrder)
        {
            List<string> lstMsg = new List<string>();
            lstMsg.Add("UpdateOrderDeliveryStatus start");
            LogClass.WriteLog(lstMsg, "");
            try
            {
                #region Check wheather user is Course Admin or not, then he is not authorised to perform requested action

                var lstCourseAdmin = db.GF_AdminUsers.FirstOrDefault(x => x.ID == objOrder.referenceID);

                if (lstCourseAdmin != null)
                {
                    if (lstCourseAdmin.Type == UserType.CourseAdmin)
                    {
                        return new Result
                        {
                            Id = objOrder.OrderId ?? 0,
                            Status = 0,
                            Error = "You are not authorised to deliver any order."
                        };
                    }
                }

                #endregion

                #region Check wheather the order is exist or not

                var lstOrder = db.GF_Order.FirstOrDefault(x => x.ID == objOrder.OrderId);
                if (lstOrder == null)
                {
                    return new Result
                    {
                        Id = objOrder.OrderId ?? 0,
                        Status = 0,
                        Error = "No order found."
                    };
                }

                #endregion

                //var chkOrder = db.GF_Order.FirstOrDefault(x => x.ID == objOrder.OrderId && (x.IsDelivered ?? false) == true);
                var chkOrder = lstOrder.IsDelivered ?? false;
                //if (chkOrder != null)
                if (chkOrder)
                {
                    return new Result
                    {
                        Id = objOrder.OrderId ?? 0,
                        Status = 1,
                        Error = "Order is already delivered."
                    };
                }
                else
                {

                    lstMsg.Add("Payment start");
                    LogClass.WriteLog(lstMsg, "");
                    DelieverNPayment delieverNPayment = new DelieverNPayment();
                    string msg = "";
                    string transId = "";
                    string BTResponse = "";

                    bool pSuccess = delieverNPayment.ChargePayments(objOrder.OrderId ?? 0, ref msg, ref transId, ref BTResponse, ref lstMsg); //Payment Succes/Failure
                    lstMsg.Add("Payment result: " + Convert.ToString(pSuccess));
                    LogClass.WriteLog(lstMsg, "");
                    long id = 0;
                    int status = 0;
                    string errMsg = "";
                    lstMsg.Add("transaction id before save: " + Convert.ToString(transId));
                    lstMsg.Add("msg before save: " + Convert.ToString(msg));
                    LogClass.WriteLog(lstMsg, "");
                    if (pSuccess)
                    {
                        GF_Order obj = new GF_Order();
                        obj = db.GF_Order.FirstOrDefault(x => x.ID == objOrder.OrderId);
                        obj.CartieId = objOrder.CartieId;
                        obj.IsDelivered = true;
                        obj.OrderStatus = OrderStatus.Delivered;
                        obj.ModifyDate = DateTime.UtcNow;
                        obj.BT_TransId = transId;
                        obj.BT_ResponseText = BTResponse;
                        db.SaveChanges();

                        id = obj.ID;

                        lstMsg.Add("transaction id after save: " + Convert.ToString(obj.BT_TransId));
                        lstMsg.Add("msg after save: " + Convert.ToString(obj.BT_ResponseText));
                        LogClass.WriteLog(lstMsg, "");
                        status = ((obj.IsDelivered ?? false) ? 1 : 0);
                        errMsg = ((obj.IsDelivered ?? false) ? "Order is delivered successfully." : "Order is not delivered.");
                    }
                    else
                    {
                        GF_Order obj = new GF_Order();
                        obj = db.GF_Order.FirstOrDefault(x => x.ID == objOrder.OrderId);
                        obj.CartieId = objOrder.CartieId;
                        obj.IsRejected = true;
                        obj.OrderStatus = OrderStatus.PaymentFailed;
                        obj.ModifyDate = DateTime.UtcNow;
                        db.SaveChanges();

                        id = objOrder.OrderId ?? 0;
                        status = 0;
                        errMsg = msg;
                    }

                    LogClass.WriteLog(lstMsg, "");

                    #region Push Notification

                    bool IsMessageToGolfer = false;

                    PushNotications pushNotications = new PushNotications();

                    if (lstOrder.OrderType == OrderType.CartOrder)
                    {
                        //Case : Cart Order
                        //Service :- Course/OrderDeliveryStatus
                        //Sender : Cartie
                        //Receiver :Golfer
                        //Message : Order has been delivered.

                        GF_AdminUsers adminUsers = new GF_AdminUsers();

                        adminUsers = db.GF_AdminUsers.FirstOrDefault(x => x.ID == (objOrder.CartieId ?? 0));

                        pushNotications.SenderId = objOrder.CartieId ?? 0;
                        pushNotications.SenderName = adminUsers.FirstName + " " + adminUsers.LastName;

                        pushNotications.ReceiverId = lstOrder.GolferID ?? 0;
                        pushNotications.ReceiverName = lstOrder.GF_Golfer.FirstName + " " + lstOrder.GF_Golfer.LastName;
                        pushNotications.pushMsgFrom = PushnoficationMsgFrom.Course;
                        pushNotications.DeviceType = lstOrder.GF_Golfer.DeviceType;

                        if ((lstOrder.GF_Golfer.AppVersion ?? 0) > 0)
                        {
                            var jString = new
                            {
                                ScreenName = AppScreenName.EnRoute,
                                Message = errMsg,
                                Data = new { OrderID = lstOrder.ID }
                            };
                            string jsonString = JsonConvert.SerializeObject(jString);

                            if (pushNotications.DeviceType.ToLower() == "ios")
                            {
                                pushNotications.Message = errMsg;
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
                                pushNotications.Message = errMsg;
                            }
                            else
                            {
                                pushNotications.Message = "\"" + errMsg + "\"";
                            }
                        }

                        SendRecieveNotification.callingPushNotification(pushNotications, IsMessageToGolfer);
                    }

                    #endregion

                    return new Result
                    {
                        Id = id,
                        Status = status,
                        Error = errMsg
                    };
                }
            }
            catch (Exception ex)
            {
                lstMsg.Add("Main Exception: " + ex.Message);
                LogClass.WriteLog(lstMsg, "");
                return new Result
                {
                    Id = objOrder.OrderId ?? 0,
                    Status = 0,
                    Error = ex.Message,
                    record = null
                };
            }
        }

        /// <summary>
        /// Created By: Amit Kumar
        /// Created Date: 02 April 2015
        /// Purpose: Update the order pickup status
        /// </summary>
        /// <param name="objOrder"></param>
        /// <returns></returns>
        public Result UpdateOrderPickupStatus(GF_Order objOrder)
        {
            try
            {
                #region Check wheather user is Course Admin or not, then he is not authorised to perform requested action

                var lstCourseAdmin = db.GF_AdminUsers.FirstOrDefault(x => x.ID == objOrder.referenceID);

                if (lstCourseAdmin != null)
                {
                    if (lstCourseAdmin.Type == UserType.CourseAdmin)
                    {
                        return new Result
                        {
                            Id = objOrder.OrderId ?? 0,
                            Status = 0,
                            Error = "You are not authorised to picked up any order."
                        };
                    }
                }

                #endregion

                #region Check wheather the order is exist or not

                var lstOrder = db.GF_Order.FirstOrDefault(x => x.ID == objOrder.OrderId);
                if (lstOrder == null)
                {
                    return new Result
                    {
                        Id = objOrder.OrderId ?? 0,
                        Status = 0,
                        Error = "No order found."
                    };
                }

                #endregion

                var chkOrder = db.GF_Order.FirstOrDefault(x => x.ID == objOrder.OrderId && (x.IsPickup ?? false) == true);
                if (chkOrder != null)
                {
                    return new Result
                    {
                        Id = objOrder.OrderId ?? 0,
                        Status = 1,
                        Error = "Order is already picked up."
                    };
                }
                else
                {
                    DelieverNPayment delieverNPayment = new DelieverNPayment();
                    string msg = "";
                    string transId = "";
                    string BTResponse = "";

                    List<string> lstMsg = new List<string>();
                    lstMsg.Add("Payment start");
                    bool pSuccess = delieverNPayment.ChargePayments(objOrder.OrderId ?? 0, ref msg, ref transId, ref BTResponse, ref lstMsg); //Payment Succes/Failure
                    lstMsg.Add("Payment result: " + Convert.ToString(pSuccess));
                    LogClass.WriteLog(lstMsg, "");
                    long id = 0;
                    int status = 0;
                    string errMsg = "";
                    lstMsg.Add("transaction id before save: " + Convert.ToString(transId));
                    lstMsg.Add("msg before save: " + Convert.ToString(msg));
                    LogClass.WriteLog(lstMsg, "");
                    if (pSuccess)
                    {
                        GF_Order obj = new GF_Order();
                        obj = db.GF_Order.FirstOrDefault(x => x.ID == objOrder.OrderId);
                        obj.IsPickup = true;
                        obj.OrderStatus = OrderStatus.Picked;
                        obj.ModifyDate = DateTime.UtcNow;
                        obj.BT_TransId = transId;
                        obj.BT_ResponseText = BTResponse;
                        db.SaveChanges();

                        id = obj.ID;
                        lstMsg.Add("transaction id after save: " + Convert.ToString(obj.BT_TransId));
                        lstMsg.Add("msg after save: " + Convert.ToString(obj.BT_ResponseText));
                        LogClass.WriteLog(lstMsg, "");

                        status = ((obj.IsPickup ?? false) ? 1 : 0);
                        errMsg = ((obj.IsPickup ?? false) ? "Order is picked up successfully." : "Order is not picked up.");
                    }
                    else
                    {
                        GF_Order obj = new GF_Order();
                        obj = db.GF_Order.FirstOrDefault(x => x.ID == objOrder.OrderId);
                        obj.IsRejected = true;
                        obj.OrderStatus = OrderStatus.PaymentFailed;
                        obj.ModifyDate = DateTime.UtcNow;
                        db.SaveChanges();

                        id = objOrder.OrderId ?? 0;
                        status = 0;
                        errMsg = msg;
                    }

                    LogClass.WriteLog(lstMsg, "");

                    #region Push Notification

                    string orderInclude = string.Join(",", lstOrder.GF_OrderDetails.ToList()
                                                               .Select(y => y.GF_MenuItems.GF_SubCategory.GF_Category.Type)
                                                               .Distinct().ToList());

                    bool IsMessageToGolfer = true;

                    PushNotications pushNotications = new PushNotications();

                    //var order = lstOrder.FirstOrDefault();

                    //if (objOrder.OrderType == OrderType.TurnOrder)
                    //{
                    GF_AdminUsers adminUsers = new GF_AdminUsers();

                    if (orderInclude == FoodCategoryType.Proshop)
                        adminUsers = db.GF_AdminUsers.FirstOrDefault(x => x.ID == (lstOrder.ProShopID ?? 0));
                    else
                        adminUsers = db.GF_AdminUsers.FirstOrDefault(x => x.ID == (lstOrder.KitchenId ?? 0));

                    if (adminUsers.Type == UserType.Kitchen)
                    {
                        //Case : Turn Order
                        //Service :- Course/OrderPickupStatus
                        //Sender : Kitchen
                        //Receiver :Golfer
                        //Message : Order has been picked up.

                        pushNotications.SenderId = lstOrder.KitchenId ?? 0;
                    }

                    if (adminUsers.Type == UserType.Proshop)
                    {
                        //Case : Turn Order
                        //Service :- Course/OrderPickupStatus
                        //Sender : Pro Shop
                        //Receiver :Golfer
                        //Message : Order has been picked up.

                        pushNotications.SenderId = lstOrder.ProShopID ?? 0;
                    }

                    pushNotications.SenderName = adminUsers.FirstName + " " + adminUsers.LastName;

                    pushNotications.ReceiverId = lstOrder.GolferID ?? 0;
                    pushNotications.ReceiverName = lstOrder.GF_Golfer.FirstName + " " + lstOrder.GF_Golfer.LastName;
                    pushNotications.pushMsgFrom = PushnoficationMsgFrom.Course;
                    //pushNotications.Message = errMsg;

                    pushNotications.DeviceType = lstOrder.GF_Golfer.DeviceType;

                    if ((lstOrder.GF_Golfer.AppVersion ?? 0) > 0)
                    {
                        var jString = new
                        {
                            ScreenName = AppScreenName.EnRoute,
                            Message = errMsg,
                            Data = new { OrderID = lstOrder.ID }
                        };
                        string jsonString = JsonConvert.SerializeObject(jString);

                        if (pushNotications.DeviceType.ToLower() == "ios")
                        {
                            pushNotications.Message = errMsg;
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
                            pushNotications.Message = errMsg;
                        }
                        else
                        {
                            pushNotications.Message = "\"" + errMsg + "\"";
                        }
                    }

                    SendRecieveNotification.callingPushNotification(pushNotications, IsMessageToGolfer);
                    //}

                    #endregion

                    return new Result
                    {
                        Id = id,
                        Status = status,
                        Error = errMsg
                    };
                }
            }
            catch (Exception ex)
            {
                return new Result
                {
                    Id = objOrder.OrderId ?? 0,
                    Status = 0,
                    Error = ex.Message,
                    record = null
                };
            }
        }

        #endregion

        #region Manage Cartie

        /// <summary>
        /// Created By: Amit Kumar
        /// Created Date: 17 April 2015
        /// Purpose: Save user current position (lat/long)
        /// </summary>
        /// <param name="objOrder"></param>
        /// <returns></returns>
        public Result SaveUserCurrentPosition(GF_UserCurrentPosition user)
        {
            try
            {
                if (user.ReferenceType != UserType.Cartie &&
                    user.ReferenceType != UserType.Kitchen &&
                    user.ReferenceType != UserType.Proshop &&
                    user.ReferenceType != UserType.CourseAdmin &&
                    user.ReferenceType != UserType.Ranger)
                {
                    return new Result
                    {
                        Id = user.ReferenceID ?? 0,
                        Status = 1,
                        Error = "Invalid User",
                        record = null
                    };
                }



                try
                {
                    var _db = new GolflerEntities();
                    if (!string.IsNullOrEmpty(Convert.ToString(user.DeviceSerialNo)))
                    {
                        var gDetails = _db.GF_AdminUsers.Where(x => x.ID == user.ReferenceID).FirstOrDefault();

                        if (gDetails != null)
                        {
                            if (gDetails.DeviceSerialNo != null)
                            {
                                if ((!string.IsNullOrEmpty(Convert.ToString(gDetails.DeviceSerialNo))) &&
                                    (!string.IsNullOrEmpty(Convert.ToString(user.DeviceSerialNo))))
                                {
                                    #region Check Golfer Device
                                    if (Convert.ToString(gDetails.DeviceSerialNo) != Convert.ToString(user.DeviceSerialNo))
                                    {
                                        return new Result
                                        {
                                            Id = user.ReferenceID ?? 0,
                                            Status = 0,
                                            Error = "logout",
                                            record = null
                                        };
                                    }
                                    #endregion
                                }
                            }
                        }
                    }
                }
                catch
                {
                    //  blGolferLoginStatus = false;
                }

                #region user location

                #region Get Course Club House ID

                var ClubHouse = db.GF_CourseInfo.FirstOrDefault(x => x.ID == user.CourseID && !(x.IsClubHouse ?? true));
                long newCourseID = user.CourseID ?? 0;
                if (ClubHouse != null)
                {
                    newCourseID = ClubHouse.ClubHouseID ?? 0;
                }

                #endregion

                GF_UserCurrentPosition userPosition = db.GF_UserCurrentPosition.FirstOrDefault(x => x.ReferenceID == user.ReferenceID &&
                    x.ReferenceType == user.ReferenceType &&
                    x.CourseID == newCourseID);//user.CourseID);

                if (userPosition != null)
                {
                    //Update
                    userPosition.Latitude = user.Latitude;
                    userPosition.Longitude = user.Longitude;
                    userPosition.ModifyBy = user.ReferenceID;
                    userPosition.ModifyDate = DateTime.UtcNow;
                    db.SaveChanges();

                    return new Result
                    {
                        Id = user.ID,
                        Status = 1,
                        Error = "Position saved successfully.",
                        record = new
                        {
                            userPosition.CourseID,
                            userPosition.ReferenceID,
                            userPosition.ReferenceType,
                            userPosition.Latitude,
                            userPosition.Longitude
                        }
                    };
                }
                else
                {
                    //Save
                    userPosition = new GF_UserCurrentPosition();
                    userPosition.CourseID = user.CourseID;
                    userPosition.ReferenceID = user.ReferenceID;
                    userPosition.ReferenceType = user.ReferenceType;
                    userPosition.Latitude = user.Latitude;
                    userPosition.Longitude = user.Longitude;
                    userPosition.CreatedBy = user.ReferenceID;
                    userPosition.CreatedDate = DateTime.UtcNow;
                    userPosition.ModifyBy = user.ReferenceID;
                    userPosition.ModifyDate = DateTime.UtcNow;

                    db.GF_UserCurrentPosition.Add(userPosition);
                    db.SaveChanges();

                    return new Result
                    {
                        Id = user.ID,
                        Status = 1,
                        Error = "Position updated successfully.",
                        record = new
                        {
                            userPosition.CourseID,
                            userPosition.ReferenceID,
                            userPosition.ReferenceType,
                            userPosition.Latitude,
                            userPosition.Longitude
                        }
                    };
                }
                #endregion


            }
            catch (Exception ex)
            {
                return new Result
                {
                    Id = user.ID,
                    Status = 0,
                    Error = ex.Message,
                    record = null
                };
            }
        }

        #endregion

        #region Manage Statistics Report

        /// <summary>
        /// Created By: Amit Kumar
        /// Created Date: 01 May 2015
        /// Purpose: Get Order Statistics Report
        /// </summary>
        /// <param name="objOrder"></param>
        /// <returns></returns>
        public Result GetOrderStatisticsReport(OrderReport orderReport)
        {
            try
            {
                #region Get Course IDs

                string cIds = GetCourseIDs(orderReport.CourseID);
                long[] courseIDs = CommonFunctions.ConvertStringArrayToLongArray(cIds);

                #endregion

                if (orderReport.ReportType.ToLower() == OrderReportType.Daily.ToLower())
                {
                    #region Order Daily Report

                    DateTime OrderDate = Convert.ToDateTime(orderReport.ReportValue);

                    var lstOrder = db.GF_Order.ToList().Where(x => courseIDs.Contains(x.CourseID ?? 0) && // == orderReport.CourseID &&
                        !(x.IsRejected ?? false) &&
                        x.OrderDate.Value.ToShortDateString() == OrderDate.ToShortDateString());

                    double totalOrder = lstOrder.Count();

                    OrderReportData orderReportData = new OrderReportData();

                    orderReportData = GetOrderReportData(lstOrder.ToList());

                    if (totalOrder > 0)
                    {
                        return new Result
                        {
                            Id = orderReport.CourseID,
                            Status = 1,
                            Error = "Success",
                            record = orderReportData
                        };
                    }
                    else
                    {
                        return new Result
                        {
                            Id = orderReport.CourseID,
                            Status = 0,
                            Error = "No order has been placed for date " + orderReport.ReportValue,
                            record = null
                        };
                    }

                    #endregion
                }
                else if (orderReport.ReportType.ToLower() == OrderReportType.Monthly.ToLower())
                {
                    #region Order Monthly Report

                    DateTime OrderDate = Convert.ToDateTime(orderReport.ReportValue);

                    var lstOrder = db.GF_Order.ToList().Where(x => courseIDs.Contains(x.CourseID ?? 0) && // == orderReport.CourseID &&
                        !(x.IsRejected ?? false) &&
                        x.OrderDate.Value.Month == OrderDate.Month &&
                        x.OrderDate.Value.Year == OrderDate.Year);

                    double totalOrder = lstOrder.Count();

                    OrderReportData orderReportData = new OrderReportData();

                    orderReportData = GetOrderReportData(lstOrder.ToList());

                    if (totalOrder > 0)
                    {
                        return new Result
                        {
                            Id = orderReport.CourseID,
                            Status = 1,
                            Error = "Success",
                            record = orderReportData
                        };
                    }
                    else
                    {
                        return new Result
                        {
                            Id = orderReport.CourseID,
                            Status = 0,
                            Error = "No order has been placed in month " + CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(OrderDate.Month),
                            record = null
                        };
                    }

                    #endregion
                }
                else if (orderReport.ReportType.ToLower() == OrderReportType.Annually.ToLower())
                {
                    #region Order Annually Report

                    int OrderYear = Convert.ToInt32(orderReport.ReportValue);

                    var lstOrder = db.GF_Order.ToList().Where(x => courseIDs.Contains(x.CourseID ?? 0) && // == orderReport.CourseID &&
                        !(x.IsRejected ?? false) &&
                        x.OrderDate.Value.Year == OrderYear);

                    double totalOrder = lstOrder.Count();

                    OrderReportData orderReportData = new OrderReportData();

                    orderReportData = GetOrderReportData(lstOrder.ToList());

                    if (totalOrder > 0)
                    {
                        return new Result
                        {
                            Id = orderReport.CourseID,
                            Status = 1,
                            Error = "Success",
                            record = orderReportData
                        };
                    }
                    else
                    {
                        return new Result
                        {
                            Id = orderReport.CourseID,
                            Status = 0,
                            Error = "No order has been placed in year " + OrderYear,
                            record = null
                        };
                    }

                    #endregion
                }
                else if (orderReport.ReportType.ToLower() == OrderReportType.SaleBreakdown.ToLower())
                {
                    #region Sales Breakdown Report

                    DateTime OrderDate = Convert.ToDateTime(orderReport.ReportValue);

                    var lstOrder = db.GF_Order.ToList().Where(x => courseIDs.Contains(x.CourseID ?? 0) && // == orderReport.CourseID &&
                        !(x.IsRejected ?? false) &&
                        x.OrderDate.Value.ToShortDateString() == OrderDate.ToShortDateString());

                    string strOrderIDs = string.Join(",", lstOrder.Select(x => x.ID));

                    long[] OrderIDs = ConvertStringArrayToLongArray(strOrderIDs == "" ? "0" : strOrderIDs);

                    var lstOrderDetails = db.GF_OrderDetails.Where(x => OrderIDs.Contains(x.OrderID ?? 0));

                    long totalOrder = lstOrder.Count();
                    int totalUnitSold = lstOrderDetails.ToList().Select(x => (x.Quantity ?? 0)).Sum();

                    var breakdown = lstOrderDetails.ToList()
                        .GroupBy(x => new
                        {
                            x.MenuItemID,
                            x.GF_MenuItems.Name
                        })
                        .Select(x => new
                        {
                            //MenuItemID = x.Key.MenuItemID,
                            MenuName = x.Key.Name,
                            Breakdown = Math.Round(((Convert.ToDecimal(x.Sum(y => y.Quantity)) / Convert.ToDecimal(totalUnitSold)) * 100), 2)
                        });

                    if (totalOrder > 0)
                    {
                        return new Result
                        {
                            Id = orderReport.CourseID,
                            Status = 1,
                            Error = "Success",
                            record = breakdown
                        };
                    }
                    else
                    {
                        return new Result
                        {
                            Id = orderReport.CourseID,
                            Status = 0,
                            Error = "No order has been placed for date " + orderReport.ReportValue,
                            record = null
                        };
                    }

                    #endregion
                }
                else if (orderReport.ReportType.ToLower() == OrderReportType.GophieRating.ToLower())
                {
                    #region Gophie Rating Report

                    DateTime OrderDate = Convert.ToDateTime(orderReport.ReportValue);

                    var lstOrder = db.GF_Order.ToList().Where(x => courseIDs.Contains(x.CourseID ?? 0) && // == orderReport.CourseID &&
                        !(x.IsRejected ?? false) &&
                        x.OrderDate.Value.Month == OrderDate.Month &&
                        x.OrderDate.Value.Year == OrderDate.Year);
                    //x.OrderDate.Value.ToShortDateString() == OrderDate.ToShortDateString());

                    string strOrderIDs = string.Join(",", lstOrder.Select(x => x.ID));

                    long[] OrderIDs = ConvertStringArrayToLongArray(strOrderIDs == "" ? "0" : strOrderIDs);

                    var golphieRating = db.GF_GolferRating.ToList().Where(x => OrderIDs.Contains(x.OrderNo ?? 0)).ToList()
                        .GroupBy(x => new
                        {
                            x.ReferenceID,
                            x.ReferenceType
                        })
                        .Select(x => new
                        {
                            GolphieName = db.GF_AdminUsers.FirstOrDefault(y => y.ID == (x.Key.ReferenceID ?? 0)).FirstName + " " +
                                          db.GF_AdminUsers.FirstOrDefault(y => y.ID == (x.Key.ReferenceID ?? 0)).LastName,
                            AvgRating = x.Average(z => z.Rating)
                        });

                    long totalOrder = lstOrder.Count();

                    if (totalOrder > 0)
                    {
                        return new Result
                        {
                            Id = orderReport.CourseID,
                            Status = 1,
                            Error = "Success",
                            record = golphieRating
                        };
                    }
                    else
                    {
                        return new Result
                        {
                            Id = orderReport.CourseID,
                            Status = 0,
                            Error = "No record found.",
                            record = null
                        };
                    }

                    #endregion
                }
                else if (orderReport.ReportType.ToLower() == OrderReportType.CourseRevenueDaily.ToLower())
                {
                    #region Course Revenue Report Daily

                    DateTime OrderDate = Convert.ToDateTime(orderReport.ReportValue);

                    //Get all placed order(s) in particular date
                    var lstOrder = db.GF_Order.ToList().Where(x => courseIDs.Contains(x.CourseID ?? 0) && // == orderReport.CourseID &&
                        !(x.IsRejected ?? false) &&
                        x.OrderDate.Value.ToShortDateString() == OrderDate.ToShortDateString());

                    //Get all placed order(s) ids
                    var orderIDs = lstOrder.Select(x => x.ID).ToList();

                    //Get all placed order detail
                    var orderDetail = db.GF_OrderDetails.Where(x => orderIDs.Contains(x.OrderID ?? 0)).ToList();

                    #region Revenue

                    var dailyRevenue = lstOrder.Select(x => (x.GrandTotal ?? 0)).Sum();

                    #endregion

                    #region Calculate Margin

                    var totalMargin = orderDetail.Select(x => (((x.UnitPrice ?? 0M) * (x.Quantity ?? 0)) - ((x.CostPrice ?? 0M) * (x.Quantity ?? 0)))).Sum();
                    decimal margin = 0;
                    try
                    {
                        margin = (totalMargin / dailyRevenue) * 100;
                    }
                    catch
                    {
                        margin = 0;
                    }

                    #endregion

                    #region Calculate COGS (Cost of Goods Sold)

                    var totalCostGoodsSold = orderDetail.Select(x => ((x.CostPrice ?? 0M) * (x.Quantity ?? 0))).Sum();

                    #endregion

                    #region Calculate Net Sales

                    var discount = lstOrder.Select(x => (x.DiscountAmt ?? 0)).Sum();
                    var netSales = dailyRevenue - discount;

                    #endregion

                    #region Calculate Gross Profit

                    var grossProfit = netSales - totalCostGoodsSold;
                    decimal grossProfitPercent = 0;
                    try
                    {
                        grossProfitPercent = (grossProfit / dailyRevenue) * 100;
                    }
                    catch
                    {
                        grossProfitPercent = 0M;
                    }

                    #endregion

                    #region Calculate Operating Expense

                    var operatingExpense = 0;

                    #endregion

                    #region Calculate Commision

                    var orderCommision = lstOrder.Select(x => (x.Commission ?? 0)).Sum();

                    decimal commision = 0;
                    try
                    {
                        commision = (orderCommision / dailyRevenue) * 100;
                    }
                    catch
                    {
                        commision = 0M;
                    }

                    #endregion

                    #region Calculate Food Cost

                    var lstFoodOrder = orderDetail.Where(x => x.GF_MenuItems.GF_SubCategory.GF_Category.Type != FoodCategoryType.Proshop.ToLower());

                    var totalFoodCost = lstFoodOrder.Select(x => ((x.CostPrice ?? 0) * (x.Quantity ?? 0))).Sum();
                    decimal foodCost = 0;
                    try
                    {
                        foodCost = (totalFoodCost / dailyRevenue) * 100;
                    }
                    catch
                    {
                        foodCost = 0M;
                    }

                    #endregion

                    #region Calculate Net Profit

                    var totalNetProfit = dailyRevenue - totalCostGoodsSold - operatingExpense;
                    decimal netProfit = 0;
                    try
                    {
                        netProfit = (totalNetProfit / dailyRevenue) * 100;
                    }
                    catch
                    {
                        netProfit = 0M;
                    }

                    #endregion

                    if (lstOrder.Count() > 0)
                    {
                        return new Result
                        {
                            Id = orderReport.CourseID,
                            Status = 1,
                            Error = "Success",
                            record = new
                            {
                                Margin = margin,
                                GrossProfit = grossProfitPercent,
                                Commission = commision,
                                FoodCost = foodCost,
                                NetProfit = netProfit
                            }
                        };
                    }
                    else
                    {
                        return new Result
                        {
                            Id = orderReport.CourseID,
                            Status = 0,
                            Error = "No order has been placed for date " + orderReport.ReportValue,
                            record = null
                        };
                    }

                    #endregion
                }
                else if (orderReport.ReportType.ToLower() == OrderReportType.CourseRevenueMonthly.ToLower())
                {
                    #region Course Revenue Report Monthly

                    DateTime OrderDate = Convert.ToDateTime(orderReport.ReportValue);

                    //Get all placed order(s) in particular date
                    var lstOrder = db.GF_Order.ToList().Where(x => courseIDs.Contains(x.CourseID ?? 0) && // == orderReport.CourseID &&
                        !(x.IsRejected ?? false) &&
                        x.OrderDate.Value.Month == OrderDate.Month &&
                        x.OrderDate.Value.Year == OrderDate.Year);

                    //Get all placed order(s) ids
                    var orderIDs = lstOrder.Select(x => x.ID).ToList();

                    //Get all placed order detail
                    var orderDetail = db.GF_OrderDetails.Where(x => orderIDs.Contains(x.OrderID ?? 0)).ToList();

                    #region Revenue

                    var dailyRevenue = lstOrder.Select(x => (x.GrandTotal ?? 0)).Sum();

                    #endregion

                    #region Calculate Margin

                    var totalMargin = orderDetail.Select(x => (((x.UnitPrice ?? 0M) * (x.Quantity ?? 0)) - ((x.CostPrice ?? 0M) * (x.Quantity ?? 0)))).Sum();
                    decimal margin = 0;
                    try
                    {
                        margin = (totalMargin / dailyRevenue) * 100;
                    }
                    catch
                    {
                        margin = 0;
                    }

                    #endregion

                    #region Calculate COGS (Cost of Goods Sold)

                    var totalCostGoodsSold = orderDetail.Select(x => ((x.CostPrice ?? 0M) * (x.Quantity ?? 0))).Sum();

                    #endregion

                    #region Calculate Net Sales

                    var discount = lstOrder.Select(x => (x.DiscountAmt ?? 0)).Sum();
                    var netSales = dailyRevenue - discount;

                    #endregion

                    #region Calculate Gross Profit

                    var grossProfit = netSales - totalCostGoodsSold;
                    decimal grossProfitPercent = 0;
                    try
                    {
                        grossProfitPercent = (grossProfit / dailyRevenue) * 100;
                    }
                    catch
                    {
                        grossProfitPercent = 0M;
                    }

                    #endregion

                    #region Calculate Operating Expense

                    var operatingExpense = 0;

                    #endregion

                    #region Calculate Commision

                    var orderCommision = lstOrder.Select(x => (x.Commission ?? 0)).Sum();

                    decimal commision = 0;
                    try
                    {
                        commision = (orderCommision / dailyRevenue) * 100;
                    }
                    catch
                    {
                        commision = 0M;
                    }

                    #endregion

                    #region Calculate Food Cost

                    var lstFoodOrder = orderDetail.Where(x => x.GF_MenuItems.GF_SubCategory.GF_Category.Type != FoodCategoryType.Proshop.ToLower());

                    var totalFoodCost = lstFoodOrder.Select(x => ((x.CostPrice ?? 0) * (x.Quantity ?? 0))).Sum();
                    decimal foodCost = 0;
                    try
                    {
                        foodCost = (totalFoodCost / dailyRevenue) * 100;
                    }
                    catch
                    {
                        foodCost = 0M;
                    }

                    #endregion

                    #region Calculate Net Profit

                    var totalNetProfit = dailyRevenue - totalCostGoodsSold - operatingExpense;
                    decimal netProfit = 0;
                    try
                    {
                        netProfit = (totalNetProfit / dailyRevenue) * 100;
                    }
                    catch
                    {
                        netProfit = 0M;
                    }

                    #endregion

                    if (lstOrder.Count() > 0)
                    {
                        return new Result
                        {
                            Id = orderReport.CourseID,
                            Status = 1,
                            Error = "Success",
                            record = new
                            {
                                Margin = margin,
                                GrossProfit = grossProfitPercent,
                                Commission = commision,
                                FoodCost = foodCost,
                                NetProfit = netProfit
                            }
                        };
                    }
                    else
                    {
                        return new Result
                        {
                            Id = orderReport.CourseID,
                            Status = 0,
                            Error = "No order has been placed in month " + CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(OrderDate.Month),
                            record = null
                        };
                    }

                    #endregion
                }
                else if (orderReport.ReportType.ToLower() == OrderReportType.GolflerCommission.ToLower())
                {
                    #region Golfler Commission Report

                    DateTime OrderDate = Convert.ToDateTime(orderReport.ReportValue);

                    var lstOrder = db.GF_Order.ToList().Where(x => courseIDs.Contains(x.CourseID ?? 0) && // == orderReport.CourseID &&
                        !(x.IsRejected ?? false) &&
                        x.OrderDate.Value.ToShortDateString() == OrderDate.ToShortDateString());

                    decimal commission = lstOrder.Sum(x => x.Commission ?? 0);
                    decimal coursePlatformFee = lstOrder.Sum(x => x.CoursePlatformFee ?? 0);
                    decimal golferPlatformFee = lstOrder.Sum(x => x.GolferPlatformFee ?? 0);
                    decimal grandTotal = lstOrder.Sum(x => x.GrandTotal ?? 0);

                    decimal golflerShare = commission + coursePlatformFee + golferPlatformFee;

                    decimal golflerCommission = 0;

                    golflerCommission = golflerShare; //(golflerShare / grandTotal) * 100;

                    long totalOrder = lstOrder.Count();

                    if (totalOrder > 0)
                    {
                        return new Result
                        {
                            Id = orderReport.CourseID,
                            Status = 1,
                            Error = "Success",
                            record = new
                            {
                                GolflerCommission = golflerCommission
                            }
                        };
                    }
                    else
                    {
                        return new Result
                        {
                            Id = orderReport.CourseID,
                            Status = 0,
                            Error = "No record found.",
                            record = null
                        };
                    }

                    #endregion
                }
                else
                {
                    //When request parameter is wrong
                    return new Result
                    {
                        Id = orderReport.CourseID,
                        Status = 1,
                        Error = "Invalid Request: Please check request parameters",
                        record = new
                        {
                            orderReport.CourseID,
                            orderReport.ReportType,
                            orderReport.ReportValue
                        }
                    };
                }

                return new Result
                {
                    Id = 0,
                    Status = 1,
                    Error = "",
                    record = null
                };
            }
            catch (Exception ex)
            {
                return new Result
                {
                    Id = orderReport.CourseID,
                    Status = 0,
                    Error = ex.Message,
                    record = null
                };
            }
        }

        /// <summary>
        /// Created By: Amit Kumar
        /// Created Date: 01 May 2015
        /// Purpose: Get order report data accordingly (i.e. Daily/Monthly/Annually)
        /// </summary>
        /// <param name="lstOrder"></param>
        /// <returns></returns>
        public OrderReportData GetOrderReportData(List<GF_Order> lstOrder)
        {
            OrderReportData obj = new OrderReportData();
            double totalOrder = lstOrder.Count();

            if (totalOrder > 0)
            {
                //Calculation of Golphie/Cartie Sales
                double tolatDeliverCartieOrder = lstOrder.Where(x => x.CartieId > 0 &&
                    (x.IsDelivered ?? false)).Count();
                double GolphieSale = (tolatDeliverCartieOrder / totalOrder) * 100;

                //Calculation of turn sales
                double totalTurnOrder = lstOrder.Where(x => x.OrderType == OrderType.TurnOrder &&
                    (x.IsPickup ?? false)).Count();
                double TurnSales = (totalTurnOrder / totalOrder) * 100;

                //Calculation of proshop orders only
                double totalProshopOrder = lstOrder.Where(x => (x.IsDelivered ?? false) ||
                    (x.IsPickup ?? false)).ToList().Select(x => new
                    {
                        orderInclude = string.Join(",", x.GF_OrderDetails.ToList()
                                                                    .Select(y => y.GF_MenuItems.GF_SubCategory.GF_Category.Type)
                                                                    .Distinct().ToList()),
                    }).Where(x => x.orderInclude.ToLower() == FoodCategoryType.Proshop.ToLower()).Count();
                double ProshopOnly = (totalProshopOrder / totalOrder) * 100;

                //Calculation of kitchen orders only
                double totalKitchenOrder = lstOrder.Where(x => (x.IsDelivered ?? false) ||
                    (x.IsPickup ?? false)).ToList().Select(x => new
                    {
                        orderInclude = string.Join(",", x.GF_OrderDetails.ToList()
                                                                    .Select(y => y.GF_MenuItems.GF_SubCategory.GF_Category.Type)
                                                                    .Distinct().ToList()),
                    }).Where(x => x.orderInclude.ToLower().Contains(FoodCategoryType.Kitchen.ToLower()) ||
                        x.orderInclude.ToLower().Contains(FoodCategoryType.Cart.ToLower())).Count();
                double KitchenOnly = (totalKitchenOrder / totalOrder) * 100;

                //Calculation of cart sales
                double totalCartOrder = lstOrder.Where(x => x.OrderType == OrderType.CartOrder &&
                    (x.IsDelivered ?? false)).Count();
                double CartSales = (totalCartOrder / totalOrder) * 100;

                obj.GolphieSale = Math.Round(GolphieSale, 2);
                obj.TurnSales = Math.Round(TurnSales, 2);
                obj.ProshopOnly = Math.Round(ProshopOnly, 2);
                obj.KitchenOnly = Math.Round(KitchenOnly, 2);
                obj.CartSales = Math.Round(CartSales, 2);
            }
            else
            {
                obj.GolphieSale = 0;
                obj.TurnSales = 0;
                obj.ProshopOnly = 0;
                obj.KitchenOnly = 0;
                obj.CartSales = 0;
            }

            return obj;
        }

        #endregion

        public bool getOrderSplitAmount(long orderID, ref decimal golflerShare, ref decimal coureShare)
        {
            try
            {
                var order = db.GF_Order.FirstOrDefault(x => x.ID == orderID);

                if (order.GF_CourseInfo.IsPlateformFeeActive ?? false)
                {
                    golflerShare = (order.GolferPlatformFee ?? 0) + (order.CoursePlatformFee ?? 0);
                    coureShare = (order.GrandTotal ?? 0) - golflerShare;
                }
                else
                {
                    golflerShare = (order.Commission ?? 0) + (order.GolferPlatformFee ?? 0);
                    coureShare = (order.GrandTotal ?? 0) - golflerShare;
                }

                return true;
            }
            catch
            {
                golflerShare = 0;
                coureShare = 0;
                return false;
            }
        }

        private static long[] ConvertStringArrayToLongArray(string str)
        {
            try
            {
                return str.Split(",".ToCharArray()).Select(x => long.Parse(x.ToString())).ToArray();
            }
            catch
            {
                return new long[0];
            }
        }

        #region Order Cancellation Process

        /// <summary>
        /// Created By: Amit Kumar
        /// Created Date: 16 May 2015
        /// Descrition: Check weather the order has been cross default wait time or user set wait time limit or not
        ///             IF order has cross wait time limit then order will be cancelled automatically
        ///             Else not action performed
        /// </summary>
        /// <param name="orderID"></param>
        public void OrderCancellationCheck(long courseID)
        {
            #region Get Course IDs

            string cIds = GetCourseIDs(courseID);
            long[] courseIDs = CommonFunctions.ConvertStringArrayToLongArray(cIds);

            #endregion

            var webSetting = db.GF_Settings.FirstOrDefault(x => x.Name.ToLower() == WebSetting.OrderAutoCancelTime.ToLower() &&
                courseIDs.Contains(x.CourseID ?? 0));// == courseID);
            double orderAutoCancelTime = 10;
            try
            {
                //Set default or value set by user
                orderAutoCancelTime = Convert.ToDouble(webSetting != null ? webSetting.Value : ConfigClass.OrderAutoCancelTime);
            }
            catch
            {
                //Due to any crash set default value 10
                orderAutoCancelTime = 10;
            }

            DateTime curDate = DateTime.UtcNow;

            //Get those orders who cross the wait time limit
            var lstOrder = db.GF_Order.Where(x => courseIDs.Contains(x.CourseID ?? 0) &&// == courseID &&
                    EntityFunctions.TruncateTime(x.OrderDate) == TodaysDate &&
                    !(x.IsDelivered ?? false) &&
                    !(x.IsPickup ?? false) &&
                    !(x.IsRejected ?? false) &&
                    ((x.CartieId ?? 0) == 0) &&
                    ((x.KitchenId ?? 0) == 0) &&
                    ((x.ProShopID ?? 0) == 0))
                .ToList()
                .Select(x => new
                {
                    OrderID = x.ID,
                    TimeElapsed = curDate.Subtract(x.OrderDate.Value).TotalMinutes
                })
                .Where(x => x.TimeElapsed >= orderAutoCancelTime);

            var strOrderIDs = string.Join(",", lstOrder.Select(x => x.OrderID));
            long[] OrderIDs = ConvertStringArrayToLongArray(strOrderIDs);

            PushNotications pushNotications = new PushNotications();
            bool IsMessageToGolfer = true;
            var sender = db.GF_AdminUsers.FirstOrDefault(x => courseIDs.Contains(x.CourseId ?? 0) &&// == courseID &&
                    (x.Type == UserType.CourseAdmin || x.Type == UserType.Proshop));

            foreach (var order in db.GF_Order.Where(x => OrderIDs.Contains(x.ID)).ToList())
            {
                order.IsRejected = true;
                order.OrderStatus = OrderStatus.Cancel;

                #region Push Notification

                //Case : Cancel Order
                //Sender : Course Admin/Proshop
                //Receiver : Golfer

                pushNotications = new PushNotications();
                pushNotications.SenderId = sender == null ? 0 : sender.ID;
                pushNotications.SenderName = sender == null ? "Support Team" : sender.FirstName + " " + sender.LastName;

                pushNotications.ReceiverId = order.GolferID ?? 0;
                pushNotications.ReceiverName = order.GF_Golfer.FirstName + " " + order.GF_Golfer.LastName;
                pushNotications.pushMsgFrom = PushnoficationMsgFrom.Course;
                pushNotications.DeviceType = order.GF_Golfer.DeviceType;

                if ((order.GF_Golfer.AppVersion ?? 0) > 0)
                {
                    var jString = new
                    {
                        ScreenName = AppScreenName.EnRoute,
                        Message = "Your order is not able to processed as no one is available to accept this order.",
                        Data = new { OrderID = order.ID }
                    };
                    string jsonString = JsonConvert.SerializeObject(jString);

                    if (pushNotications.DeviceType.ToLower() == "ios")
                    {
                        pushNotications.Message = "Your order is not able to processed as no one is available to accept this order.";
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
                        pushNotications.Message = "Your order is not able to processed as no one is available to accept this order.";
                    }
                    else
                    {
                        pushNotications.Message = "\"Your order is not able to processed as no one is available to accept this order.\"";
                    }
                }

                SendRecieveNotification.callingPushNotification(pushNotications, IsMessageToGolfer);

                #endregion
            }
            db.SaveChanges();

            //Call Bulk Reset quantity of those orders which is rejected
            bulkResetMenuItemQuantity(OrderIDs);
        }

        #endregion

        #region Order Rejected Process

        public bool rejectOrder(long courseID, long orderID, string refType, long refID)
        {
            #region Get Course Club House ID

            var ClubHouse = db.GF_CourseInfo.FirstOrDefault(x => x.ID == courseID && !(x.IsClubHouse ?? true));
            long newCourseID = courseID;
            if (ClubHouse != null)
            {
                newCourseID = ClubHouse.ClubHouseID ?? 0;
            }

            #endregion

            //Get all user according to refernce type
            var courseUser = db.GF_AdminUsers.Where(x => x.CourseId == newCourseID &&//courseID &&
                x.Type == refType &&
                x.Status == StatusType.Active);

            //Get all user corresponding to rejected order
            var rejectOrder = db.GF_OrderAcceptReject.Where(x => x.OrderID == orderID && x.ReferenceType == refType);

            //If bothe above count is match then it indicates that order is rejected by all user according to reference type
            if (courseUser.Count() == rejectOrder.Count())
                return true;
            else
                return false;
        }

        #endregion

        #region All Active Course User

        public List<ActiveCourseUser> getActiveCourseUser(long courseID, long userID)
        {
            var lstActiveCourseUser = (from x in db.GF_AdminUsers
                                       join y in db.GF_UserCurrentPosition on x.ID equals y.ReferenceID
                                       where x.Type == y.ReferenceType &&
                                       (x.CourseId ?? 0) == courseID &&
                                       (x.IsOnline ?? false) &&
                                       x.ID != userID
                                       select new { x, y })
                                       .ToList()
                                       .Select(z => new ActiveCourseUser
                                       {
                                           Name = z.x.FirstName + " " + z.x.LastName,
                                           UserType = z.x.Type, //UserType.GetFullUserType(z.x.Type),
                                           Latitude = z.y.Latitude,
                                           Longitude = z.y.Longitude
                                       }).ToList();

            return lstActiveCourseUser;
        }

        #endregion

        #region Order Ready Status By ID

        public int GetOrderReadyStatus(long orderID, string type)
        {
            var order = db.GF_Order.FirstOrDefault(x => x.ID == orderID);

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
            }
            else
            {
                return 0;
            }
        }

        #endregion

        #region Push Notification Test

        public Result PushNotificationTest(PushNoticationsTest pNotications)
        {
            PushNotications pushNotications = new PushNotications();
            pushNotications.SenderId = pNotications.SenderId;
            pushNotications.SenderName = pNotications.SenderName;
            pushNotications.ReceiverId = pNotications.ReceiverId;
            pushNotications.ReceiverName = pNotications.ReceiverName;
            pushNotications.pushMsgFrom = pNotications.pushMsgFrom;
            pushNotications.DeviceType = pNotications.DeviceType;
            pushNotications.Message = pNotications.Message;
            pushNotications.iosMessageJson = pNotications.iosMessageJson;

            SendRecieveNotification.callingPushNotification(pushNotications, pNotications.IsMessageToGolfer);

            return new Result
            {
                Id = 0,
                Error = "Succes",
                Status = 1,
                record = new
                {
                    pNotications.SenderId,
                    pNotications.SenderName,
                    pNotications.ReceiverId,
                    pNotications.ReceiverName,
                    pNotications.pushMsgFrom,
                    pNotications.DeviceType,
                    pNotications.Message,
                    pNotications.IsMessageToGolfer,
                    pNotications.iosMessageJson
                }
            };
        }

        #endregion

        #region Reset Quantity

        public void resetMenuItemQuantity(long orderID)
        {
            var order = db.GF_Order.FirstOrDefault(x => x.ID == orderID && (x.IsRejected ?? false));

            if (order != null)
            {
                //Get all item with sum of quantity with same itemid
                var lstSoldOut = order.GF_OrderDetails.GroupBy(x => x.MenuItemID)
                    .Select(y => new
                    {
                        MenuItemID = y.First().MenuItemID,
                        Quantity = y.Sum(x => x.Quantity)
                    });

                foreach (var item in lstSoldOut)
                {
                    //Get item detail
                    var menuQuantity = db.GF_CourseFoodItemDetail.FirstOrDefault(x => (x.MenuItemID ?? 0) == item.MenuItemID &&
                        x.GF_CourseFoodItem.CourseID == order.CourseID);
                    if (menuQuantity != null)
                    {
                        menuQuantity.Quantity = (menuQuantity.Quantity ?? 0) + item.Quantity;
                        db.SaveChanges();
                    }
                }
            }
        }

        public void bulkResetMenuItemQuantity(long[] orderIDs)
        {
            foreach (var orders in orderIDs)
            {
                var order = db.GF_Order.FirstOrDefault(x => x.ID == orders && (x.IsRejected ?? false));

                if (order != null)
                {
                    //Get all item with sum of quantity with same itemid
                    var lstSoldOut = order.GF_OrderDetails.GroupBy(x => x.MenuItemID)
                        .Select(y => new
                        {
                            MenuItemID = y.First().MenuItemID,
                            Quantity = y.Sum(x => x.Quantity)
                        });

                    foreach (var item in lstSoldOut)
                    {
                        //Get item detail
                        var menuQuantity = db.GF_CourseFoodItemDetail.FirstOrDefault(x => (x.MenuItemID ?? 0) == item.MenuItemID &&
                        x.GF_CourseFoodItem.CourseID == order.CourseID);
                        if (menuQuantity != null)
                        {
                            menuQuantity.Quantity = (menuQuantity.Quantity ?? 0) + item.Quantity;
                            db.SaveChanges();
                        }
                    }
                }
            }
        }

        #endregion

        public string GetCourseIDs(long courseID)
        {
            return string.Join(",", db.GF_CourseInfo.Where(x => x.Status != StatusType.Delete && (x.ClubHouseID == courseID ||
                x.ID == courseID)).Select(v => v.ID).ToArray());
        }
    }

    public class OrderHistory
    {
        public long OrderId { get; set; }
        public long referenceID { get; set; }
        public string referenceType { get; set; }
        public string name { get; set; }
        public string orderDate { get; set; }

        //Paging
        public int? pageIndex { get; set; }
        public int? pageSize { get; set; }
    }

    public partial class GF_Order
    {
        public long? OrderId { get; set; }
        public long? referenceID { get; set; }
        public string referenceType { get; set; }

        //Paging
        public int? pageIndex { get; set; }
        public int? pageSize { get; set; }
    }

    public class OrderReport
    {
        public long CourseID { get; set; }
        public string ReportType { get; set; }
        public string ReportValue { get; set; }
    }

    public class OrderReportData
    {
        public double GolphieSale { get; set; }
        public double TurnSales { get; set; }
        public double ProshopOnly { get; set; }
        public double KitchenOnly { get; set; }
        public double CartSales { get; set; }
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
        public List<OrderDetails> itemsOrdered { get; set; }
        public decimal? billAmount { get; set; }
        public decimal? TaxPercentage { get; set; }
        public decimal? TaxAmount { get; set; }
        public decimal GolferPlatformFee { get; set; }
        public decimal? Total { get; set; }
        public string OrderType { get; set; }
        public DateTime? OrderDate { get; set; }
        public int ReadyStatus { get; set; }
        public string HEXColor { get; set; }
        public string RGBColor { get; set; }
        public string HUEColor { get; set; }
        public string orderInclude { get; set; }
        public decimal? DiscountAmt { get; set; }
        public string TimeElapsed { get; set; }
        public bool IsNew { get; set; }
        public long HoleNo { get; set; }
        public bool IsGolphieOrder { get; set; }
    }

    public class OrderDetails
    {
        public string Name { get; set; }
        public decimal Amount { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public List<MenuOptionName> MenuOptionName { get; set; }
    }

    public class ActiveCourseUser
    {
        public string Name { get; set; }
        public string UserType { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
    }

    public class MenuOptionName
    {
        public long ID { get; set; }
        public string Name { get; set; }
    }

    public class PushNoticationsTest
    {
        public static string PushMessage { get; set; }
        public static string PushDevice { get; set; }
        public long SenderId { get; set; }
        public long ReceiverId { get; set; }
        public string DeviceType { get; set; }
        public string Message { get; set; }
        public string SenderName { get; set; }
        public string ReceiverName { get; set; }
        public string iosMessageJson { get; set; }
        public string pushMsgFrom { get; set; }
        public bool IsMessageToGolfer { get; set; }
    }
}