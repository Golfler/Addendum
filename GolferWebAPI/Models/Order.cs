using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Objects;
using System.Linq;
using System.Web;

namespace GolferWebAPI.Models
{
    public partial class GF_Order
    {
        private GolflerEntities _db = null;
        public List<OrderMenuitems> MenuItems { get; set; }

        /// <summary>
        /// Created By:Arun
        /// Created Date:25 March 2015
        /// Purpose: Add order items.
        /// </summary>
        /// <param name="objOrder"></param>
        /// <returns></returns>
        public Result AddGolferOrder(GF_Order objOrder)
        {
            try
            {
                _db = new GolflerEntities();

                #region Get Course Club House ID

                var ClubHouse = _db.GF_CourseInfo.FirstOrDefault(x => x.ID == objOrder.CourseID && !(x.IsClubHouse ?? true));
                long newCourseID = objOrder.CourseID ?? 0;
                if (ClubHouse != null)
                {
                    newCourseID = ClubHouse.ClubHouseID ?? 0;
                }

                #endregion

                #region Check is course enabled delivery/pickup orders or not

                var enableOrderType = objOrder.OrderType == GolferWebAPI.Models.OrderType.TurnOrder ? WebSetting.EnablePickupOrder.ToLower() :
                    WebSetting.EnableDeliveryOrder.ToLower();

                var webSettingOrder = _db.GF_Settings.FirstOrDefault(x => x.Name.ToLower() == enableOrderType &&
                    x.CourseID == newCourseID);//objOrder.CourseID);

                bool isEnabled = true; //By default order placing is enabled
                if (webSettingOrder != null)
                {
                    isEnabled = webSettingOrder.Value != "False";
                }

                if (!isEnabled)
                {
                    var orderAccept = objOrder.OrderType == GolferWebAPI.Models.OrderType.TurnOrder ? "pickup" : "delivery" ;
                    return new Result
                    {
                        Id = 0,
                        Status = 0,
                        record = null,
                        Error = "This course is not accepting " + orderAccept + " order right now."
                        //This course is not accepting <any/pickup/delivery> order right now.
                    };
                }

                #endregion

                #region Order Limit Check

                var webSetting = _db.GF_Settings.FirstOrDefault(x => x.Name.ToLower() == WebSetting.OrderPlaceLimit.ToLower() &&
                    x.CourseID == newCourseID);//objOrder.CourseID);
                decimal orderPlaceLimit = 5;
                try
                {
                    //Set default or value set by user
                    orderPlaceLimit = Convert.ToDecimal(webSetting != null ? webSetting.Value : ConfigClass.OrderPlaceLimit);
                }
                catch
                {
                    //Due to any crash set default value 5
                    orderPlaceLimit = Convert.ToDecimal(4.99);
                }

                decimal orderAmount = (objOrder.GrandTotal ?? 0) - (objOrder.GolferPlatformFee ?? 0) - (objOrder.TaxAmount ?? 0) + (objOrder.DiscountAmt ?? 0);

                if (orderAmount <= orderPlaceLimit)
                {
                    return new Result
                    {
                        Id = 0,
                        Status = 0,
                        record = null,
                        //Error = "Order can't be placed. Your order amount is less then expected order value limit."
                        Error = "Order can't be placed. Your order amount should be more than $" + orderPlaceLimit.ToString() + "."
                    };
                }

                #endregion

                #region Membership Check

                if (objOrder.PaymentType == "0")
                {
                    var lstGolfer = _db.GF_Golfer.FirstOrDefault(x => x.GF_ID == objOrder.GolferID);

                    if (lstGolfer != null)
                    {
                        var lstMembership = _db.GF_CourseMemberShip.FirstOrDefault(x => x.CourseId == newCourseID &&// objOrder.CourseID &&
                            x.Email.ToLower().Contains(lstGolfer.Email.ToLower()) &&
                            x.Status == StatusType.Active);

                        if (lstMembership != null)
                        {
                            if (lstMembership.MemberShipId != objOrder.MemberShipID)
                            {
                                return new Result
                                {
                                    Id = 0,
                                    Status = 0,
                                    record = null,
                                    Error = "You have entered wrong membership ID. Please verify and try again"
                                };
                            }
                        }
                        else
                        {
                            return new Result
                            {
                                Id = 0,
                                Status = 0,
                                record = null,
                                Error = "You have entered wrong membership ID. Please verify and try again"
                            };
                        }
                    }
                    else
                    {
                        return new Result
                        {
                            Id = 0,
                            Status = 0,
                            record = null,
                            Error = "Invalid User"
                        };
                    }
                }
                else
                {
                    #region Check Sub-Merchant ID is availbale or not

                    string BTSubMerchantId = "";
                    //if (Convert.ToString(System.Web.Configuration.WebConfigurationManager.AppSettings["PAYMENT_MODE"]) == "L")
                    //{
                    BTSubMerchantId = Convert.ToString(_db.GF_Settings.Where(x => x.CourseID == newCourseID && //objOrder.CourseID &&
                        x.Name == "BTSubMerchantId").FirstOrDefault().Value);

                    //}
                    //else
                    //{
                    //BTSubMerchantId = "rkalrasgolfcourse_instant_32x3vtr2";
                    //}
                    if (string.IsNullOrEmpty(Convert.ToString(BTSubMerchantId)))
                    {
                        return new Result
                        {
                            Id = 0,
                            Status = 0,
                            record = null,
                            Error = "This course is not setup to recieve payment. Please contact course administrator."
                        };
                       
                    }

                    #endregion
                    objOrder.CardNumber = CommonFunctions.EncryptUrlParam(Convert.ToInt64(objOrder.CardNumber));
                    objOrder.CardExpMonth = CommonFunctions.EncryptUrlParam(Convert.ToInt64(objOrder.CardExpMonth));
                    objOrder.CardExpYear = CommonFunctions.EncryptUrlParam(Convert.ToInt64(objOrder.CardExpYear));
                    objOrder.CardCCV = CommonFunctions.EncryptUrlParam(Convert.ToInt64(objOrder.CardCCV));

                    //objOrder.CardNumber = Convert.ToString(objOrder.CardNumber);
                    //objOrder.CardExpMonth = Convert.ToString(objOrder.CardExpMonth);
                    //objOrder.CardExpYear = Convert.ToString(objOrder.CardExpYear);
                    //objOrder.CardCCV = Convert.ToString(objOrder.CardCCV);s
                }

                #endregion

                #region Check Sold Out Items

                string soldOutItems = "Sold out items: ";

                //Get all item with sum of quantity with same itemid
                var lstSoldOut = objOrder.MenuItems.GroupBy(x => x.ItemId)
                    .Select(y => new
                    {
                        ItemID = y.First().ItemId,
                        Quantity = y.Sum(x => x.Quantity)
                    });

                bool isItemSold = false;

                foreach (var item in lstSoldOut)
                {
                    //Get item detail
                    var menuQuantity = _db.GF_CourseFoodItemDetail.FirstOrDefault(x => (x.MenuItemID ?? 0) == item.ItemID &&
                        x.GF_CourseFoodItem.CourseID == newCourseID);//objOrder.CourseID);
                    if (menuQuantity != null)
                    {
                        //If item quantity is less then actual available quantity then item is sold out
                        if ((menuQuantity.Quantity ?? 0) < item.Quantity)
                        {
                            soldOutItems = soldOutItems + menuQuantity.GF_MenuItems.Name + " (Qty. Availlable " + (menuQuantity.Quantity ?? 0).ToString() + "),";
                            isItemSold = true;
                        }
                    }
                }

                soldOutItems = soldOutItems.TrimEnd(',') + ".";

                if (isItemSold)
                {
                    return new Result
                    {
                        Id = 0,
                        Status = 0,
                        record = null,
                        Error = soldOutItems
                    };
                }
                else //Deduct Quantity in Main table
                {
                    foreach (var item in lstSoldOut)
                    {
                        //Get item detail
                        var menuQuantity = _db.GF_CourseFoodItemDetail.FirstOrDefault(x => (x.MenuItemID ?? 0) == item.ItemID &&
                        x.GF_CourseFoodItem.CourseID == newCourseID);//objOrder.CourseID);
                        if (menuQuantity != null)
                        {
                            menuQuantity.Quantity = (menuQuantity.Quantity ?? 0) - item.Quantity;
                            _db.SaveChanges();
                        }
                    }
                }

                #endregion

                #region Check Promo Code is valid or not

                if ((objOrder.PromoCodeID ?? 0) > 0)
                {
                    string msg = "";
                    //bool status = PromoCodeValid(objOrder.PromoCodeID ?? 0, objOrder.CourseID ?? 0, ref msg);
                    bool status = PromoCodeValid(objOrder.PromoCodeID ?? 0, newCourseID, ref msg);
                    
                    if (!status)
                    {
                        return new Result { Id = 0, Status = status ? 1 : 0, record = null, Error = msg };
                    }
                }

                #endregion

                var objPlateformFee = _db.GF_Settings.FirstOrDefault(x => x.Name.ToLower() == Settings.GolferPlateFormfee);
                objOrder.GPFRate = objPlateformFee == null ? 0 : Convert.ToDecimal(objPlateformFee.Value);

                DateTime utcDateTime = DateTime.UtcNow;

                objOrder.IsDelivered = false;
                objOrder.Status = StatusType.Active;
                objOrder.IsActive = true;
                objOrder.CreatedDate = utcDateTime;
                objOrder.CreatedBy = Convert.ToString(objOrder.GolferID);
                objOrder.OrderDate = utcDateTime;
                objOrder.OrderStatus = GolferWebAPI.Models.OrderStatus.Placed;

                decimal comAmount = 0;
                decimal cpf = 0;
                decimal cpfRate = 0;
                SplitOrderAmount(objOrder, ref comAmount, ref cpf, ref cpfRate);
                objOrder.Commission = comAmount;
                objOrder.CoursePlatformFee = cpfRate;
                objOrder.CPFRate = cpfRate;

                #region Color Code

                ///Code Added By: Amit Kumar
                ///Code Added Date: 03 April 2015
                
                ColorFound:
                
                Random random = new Random();
                var HEXColor = String.Format("#{0:X6}", random.Next(0x1000000)); //Generate random HEX color code
                var RGBColor = System.Drawing.ColorTranslator.FromHtml(HEXColor); //Convert HEX color code into RGB color code
                var HUEColor = System.Drawing.Color.FromArgb(RGBColor.R, RGBColor.G, RGBColor.B).GetHue(); //Convert RGB color code into HUE color code

                #region If random color exists in Red/Green/Blue/Orange then regenerate the color code

                string[] skipColor = new string[] {"#FD7567", "#00E64D", "#6991FD", "#FF9424"};

                if (skipColor.Contains(HEXColor))
                    goto ColorFound;

                #endregion

                #region Check new assign color

                ///Check new assign random color is already assigned to other active order or not
                ///If assigned then re-genrate the random color code

                var lstColor = _db.GF_Order.FirstOrDefault(x => x.HEXColor == HEXColor &&
                    (!(x.IsDelivered ?? false) || !(x.IsPickup ?? false)) &&
                    !(x.IsRejected ?? false) &&
                    x.OrderDate == DateTime.UtcNow &&
                    x.CourseID == objOrder.CourseID);

                if (lstColor != null)
                {
                    //Re-Generate the random color code
                    goto ColorFound;

                    //HEXColor = String.Format("#{0:X6}", random.Next(0x1000000));
                    //RGBColor = System.Drawing.ColorTranslator.FromHtml(HEXColor);
                    //HUEColor = System.Drawing.Color.FromArgb(RGBColor.R, RGBColor.G, RGBColor.B).GetHue();
                }

                #endregion

                objOrder.HEXColor = HEXColor;
                objOrder.RGBColor = RGBColor.R.ToString() + "," + RGBColor.G.ToString() + "," + RGBColor.B.ToString();
                objOrder.HUEColor = HUEColor.ToString();

                #endregion

                _db.GF_Order.Add(objOrder);
                _db.SaveChanges();

                long OrderID = objOrder.ID;
                foreach (var item in objOrder.MenuItems)
                {
                    var menuCostPrice = _db.GF_CourseFoodItemDetail.FirstOrDefault(x => (x.MenuItemID ?? 0) == item.ItemId &&
                        x.GF_CourseFoodItem.CourseID == newCourseID);//objOrder.CourseID);
                    var objItems = new GF_OrderDetails
                    {
                        OrderID = OrderID,
                        MenuItemID = item.ItemId,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice, //_db.GF_MenuItems.FirstOrDefault(x => x.ID == item.ItemId).Amount ?? 0
                        CostPrice = menuCostPrice == null ? 0M : menuCostPrice.CostPrice
                    };
                    _db.GF_OrderDetails.Add(objItems);
                    _db.SaveChanges();

                    if (item.MenuItemOption != null)
                    {
                        //Add Menu Item Option
                        foreach (var option in item.MenuItemOption)
                        {
                            if (option.MenuOptionID != null)
                            {
                                var objItemOption = new GF_OrderMenuOptionDetail
                                {
                                    OrderDetailID = objItems.ID,
                                    MenuOptionID = option.MenuOptionID,
                                    MenuOptionName = option.MenuOptionName
                                };
                                _db.GF_OrderMenuOptionDetail.Add(objItemOption);
                                _db.SaveChanges();
                            }
                        }
                    }
                }

                #region Weather Thread Information

                GF_WeatherDetails weatherDetails = new GF_WeatherDetails();
                weatherDetails.OrderID = objOrder.ID;
                weatherDetails.GolferID = objOrder.GolferID;
                weatherDetails.Longitude = objOrder.Latitude;
                weatherDetails.Latitude = objOrder.Longitude;
                weather.callingWeatherApi(weatherDetails);

                #endregion

                #region Send order Mail To golfer

                var objReg = _db.GF_Golfer.FirstOrDefault(x => x.GF_ID == objOrder.GolferID);
                if (objReg.IsReceiveEmail ?? true)
                {
                    AscynMails.callingOrderMails(objReg, Convert.ToString(objOrder.ID), objOrder);
                }

                #endregion

                #region Order Placed Push Notification

                PushNotications pushNotications = new PushNotications();
                bool IsMessageToGolfer = false;
                var receiver = _db.GF_AdminUsers.Where(x => x.CourseId == newCourseID && //objOrder.CourseID &&
                    (x.IsOnline ?? false) && !string.IsNullOrEmpty(x.DeviceType) &&
                        objOrder.OrderType == GolferWebAPI.Models.OrderType.CartOrder ? x.Type == UserType.Cartie : x.Type == UserType.Kitchen);

                foreach (var rUser in receiver.ToList())
                {
                    //Case : Order Placed
                    //Sender : Golfer
                    //Receiver : Gophie/Kitchen

                    if (!string.IsNullOrEmpty(rUser.DeviceType))
                    {
                        pushNotications = new PushNotications();
                        pushNotications.SenderId = objOrder.GolferID ?? 0;
                        pushNotications.SenderName = objOrder.GF_Golfer.FirstName + " " + objOrder.GF_Golfer.LastName;

                        pushNotications.ReceiverId = rUser.ID;
                        pushNotications.ReceiverName = rUser.FirstName + " " + rUser.LastName;
                        pushNotications.pushMsgFrom = PushnoficationMsgFrom.Golfer;
                        pushNotications.DeviceType = rUser.DeviceType;

                        if ((objOrder.GF_Golfer.AppVersion ?? 0) > 0)
                        {
                            var jString = new
                            {
                                ScreenName = AppScreenName.ActiveOrder,
                                Message = "New order has been placed Order id " + objOrder.ID.ToString() + ".",
                                Data = new { OrderID = objOrder.ID }
                            };
                            string jsonString = JsonConvert.SerializeObject(jString);

                            if (pushNotications.DeviceType.ToLower() == "ios")
                            {
                                pushNotications.Message = "New order has been placed Order id " + objOrder.ID.ToString() + ".";
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
                                pushNotications.Message = "New order has been placed Order id " + objOrder.ID.ToString() + ".";
                            }
                            else
                            {
                                pushNotications.Message = "\"New order has been placed Order id " + objOrder.ID.ToString() + ".\"";
                            }
                        }

                        SendRecieveNotification.callingPushNotification(pushNotications, IsMessageToGolfer);
                    }
                }

                #endregion

                #region Time Zone

                string golferTimeZone = CommonFunctions.GolferTimeZone(objOrder.GolferID ?? 0); //Get Time Zone of Course
                DateTime tzOrderDate = CommonFunctions.GolferTimeZoneDateTime(golferTimeZone, utcDateTime); //Calculate time according to time zone

                #endregion

                return new Result
                {
                    Id = objOrder.ID,
                    Status = 1,
                    Error = "Success",
                    record = new
                    {
                        objOrder.ID,
                        OrderDate = tzOrderDate.ToString("MM/dd/yyyy"),//objOrder.OrderDate.Value.ToString("MM/dd/yyyy"),
                        name = objOrder.GF_Golfer.FirstName + " " + objOrder.GF_Golfer.LastName,
                        GolferID = objOrder.GolferID,
                        objOrder.Latitude,
                        objOrder.Longitude,
                        time = objOrder.OrderDate.Value.ToShortTimeString(),
                        itemsOrdered = objOrder.GF_OrderDetails.ToList().Select(y =>
                            new
                            {
                                Name = _db.GF_MenuItems.FirstOrDefault(k => k.ID == y.MenuItemID).Name,
                                y.UnitPrice,
                                y.Quantity,
                                Amount = (y.UnitPrice * y.Quantity),
                                MenuOptionName = y.GF_OrderMenuOptionDetail.Select(q => new MenuOptionName { ID = (q.MenuOptionID ?? 0), Name = q.MenuOptionName }).ToList()
                            }),
                        billAmount = objOrder.GF_OrderDetails.Sum(y => (y.UnitPrice * y.Quantity)),
                        TaxPercentage = objOrder.TaxAmount,
                        TaxAmount = objOrder.TaxAmount,//((objOrder.GF_OrderDetails.Sum(y => (y.UnitPrice * y.Quantity))) * objOrder.TaxAmount / 100),
                        GolferPlatformFee = objOrder.GolferPlatformFee,
                        total = objOrder.GrandTotal,
                        //total = ((objOrder.GF_OrderDetails.Sum(y => (y.UnitPrice * y.Quantity))) +
                        //                                     ((objOrder.GF_OrderDetails.Sum(y => (y.UnitPrice * y.Quantity))) * objOrder.TaxAmount / 100) +
                        //                                     objOrder.GolferPlatformFee),
                        OrderType = objOrder.OrderType,
                        CourseID = objOrder.CourseID,

                        DiscountAmt = (objOrder.DiscountAmt ?? 0),
                        PromoCode = (objOrder.PromoCodeID == null ? "" : (_db.GF_PromoCode.FirstOrDefault(k => k.ID == objOrder.PromoCodeID).PromoCode)),
                        DiscountPercentage = (objOrder.PromoCodeID == null ? 0 : (_db.GF_PromoCode.FirstOrDefault(k => k.ID == objOrder.PromoCodeID).Discount)),
                        OrderStatus = objOrder.OrderStatus,
                        GrandTotal = objOrder.GrandTotal,
                        CreatedBy = objOrder.CreatedBy
                    }

                };
            }
            catch (Exception ex)
            {
                return new Result { Id = 0, Status = 0, record = null, Error = ex.Message };
            }

        }

        #region Order History

        /// <summary>
        /// Created By: Amit Kumar
        /// Created Date: 02 April 2015
        /// Purpose: List of Orders History
        /// </summary>
        /// <param name="orderHistory"></param>
        /// <returns></returns>
        public Result GetOrderHistoryList(OrderHistory orderHistory)
        {
            try
            {
                _db = new GolflerEntities();

                string golferTimeZone = CommonFunctions.GolferTimeZone(orderHistory.golferID); //Get Time Zone of Course

                var lstOrders = _db.GF_Order.Where(x => x.GolferID == orderHistory.golferID).OrderByDescending(x => x.ID).ToList()
                    //&& (x.IsDelivered ?? false)
                    .Select(x =>
                        new
                        {
                            orderID = x.ID,
                            orderDate = CommonFunctions.GolferTimeZoneDateTime(golferTimeZone, x.OrderDate.Value).ToString("MM/dd/yyyy"),//x.OrderDate.Value.ToString("MM/dd/yyyy"),
                            name = x.GF_Golfer.FirstName + " " + x.GF_Golfer.LastName,
                            x.Longitude,
                            x.Latitude,
                            time = CommonFunctions.GolferTimeZoneDateTime(golferTimeZone, x.OrderDate.Value).ToShortTimeString(),//x.OrderDate.Value.ToShortTimeString(),
                            itemsOrdered = x.GF_OrderDetails.ToList().Select(y =>
                                new
                                {
                                    y.GF_MenuItems.Name,
                                    y.UnitPrice,
                                    y.Quantity,
                                    Amount = (y.UnitPrice * y.Quantity),
                                    MenuOptionName = y.GF_OrderMenuOptionDetail.Select(q => new MenuOptionName { ID = (q.MenuOptionID ?? 0), Name = q.MenuOptionName }).ToList()
                                }),
                            billAmount = x.GF_OrderDetails.Sum(y => (y.UnitPrice * y.Quantity)),
                            TaxPercentage = x.GF_CourseInfo.Tax,
                            taxAmount = x.TaxAmount,//((x.GF_OrderDetails.Sum(y => (y.UnitPrice * y.Quantity))) * x.GF_CourseInfo.Tax / 100),
                            golferPlatformFee = x.GolferPlatformFee ?? 0,
                            total = x.GrandTotal,
                            //total = ((x.GF_OrderDetails.Sum(y => (y.UnitPrice * y.Quantity))) +
                            //                                     ((x.GF_OrderDetails.Sum(y => (y.UnitPrice * y.Quantity))) * x.GF_CourseInfo.Tax / 100) +
                            //                                     x.GolferPlatformFee),
                            cartieName = (x.CartieId != null ? (_db.GF_AdminUsers.FirstOrDefault(y => y.ID == x.CartieId) == null ? "" : (_db.GF_AdminUsers.FirstOrDefault(y => y.ID == x.CartieId).FirstName + " " +
                                _db.GF_AdminUsers.FirstOrDefault(y => y.ID == x.CartieId).LastName)) : ""),
                            orderType = x.OrderType,
                            courseID = x.CourseID,
                            courseName = x.GF_CourseInfo.COURSE_NAME,
                            DiscountAmt = (x.DiscountAmt ?? 0),
                            PromoCode = (x.PromoCodeID == null ? "" : x.GF_PromoCode.PromoCode),
                            DiscountPercentage = (x.PromoCodeID == null ? 0 : x.GF_PromoCode.Discount),
                            orderStatus = x.OrderStatus,
                            PaymentMode = x.PaymentType == "0" ? "Member Ship" : "Card"
                        });

                if (!string.IsNullOrEmpty(orderHistory.orderDate))
                {
                    string orderDate = Convert.ToDateTime(orderHistory.orderDate).ToString("MM/dd/yyyy");
                    lstOrders = lstOrders.Where(x => x.orderDate.Contains(orderDate));
                }

                if (orderHistory.orderID > 0)
                {
                    lstOrders = lstOrders.Where(x => x.orderID == orderHistory.orderID);
                }

                if (orderHistory.courseID > 0)
                {
                    lstOrders = lstOrders.Where(x => x.courseID == orderHistory.courseID);
                }

                if (lstOrders.Count() > 0)
                {
                    var orders = lstOrders.Skip(((orderHistory.pageIndex ?? 1) - 1) * (orderHistory.pageSize ?? 10)).Take(orderHistory.pageSize ?? 10);

                    if (orders.Count() > 0)
                    {
                        return new Result
                        {
                            Id = orderHistory.golferID,
                            Status = 1,
                            Error = "Success",
                            record = orders
                        };
                    }
                    else
                    {
                        return new Result
                        {
                            Id = orderHistory.golferID,
                            Status = 1,
                            Error = "No order(s) has been placed.",
                            record = null
                        };
                    }
                }
                else
                {
                    return new Result
                    {
                        Id = orderHistory.golferID,
                        Status = 0,
                        Error = "No order(s) has been placed.",
                        record = null
                    };
                }
            }
            catch (Exception ex)
            {
                return new Result
                {
                    Id = orderHistory.golferID,
                    Status = 0,
                    Error = ex.Message,
                    record = null
                };
            }
        }

        #endregion Order History

        #region Manage Cartie

        /// <summary>
        /// Created By: Amit Kumar
        /// Created Date: 17 April 2015
        /// Purpose: Cartie's online/offline status
        /// </summary>
        /// <param name="objOrder"></param>
        /// <returns></returns>
        public ResultOrder getCourseCartieStatus(GF_AdminUsers adminUsers)
        {
            try
            {
                _db = new GolflerEntities();

                bool blGolferLoginStatus = false;                
                try
                {

                    if (!string.IsNullOrEmpty(Convert.ToString(adminUsers.GolferID)))
                    {
                        var gDetails = _db.GF_Golfer.Where(x => x.GF_ID == adminUsers.GolferID).FirstOrDefault();

                        if (gDetails != null)
                        {
                            if ((!string.IsNullOrEmpty(Convert.ToString(gDetails.DeviceSerialNo))) && (!string.IsNullOrEmpty(Convert.ToString(adminUsers.DeviceSerialNo))))
                            {
                                #region Check Golfer Device
                                if (Convert.ToString( gDetails.DeviceSerialNo ) != Convert.ToString( adminUsers.DeviceSerialNo))
                                {
                                    return new ResultOrder
                                    {
                                        Id = 0,
                                        Status = 1,
                                        Error = "logout",
                                        GolferLoginStatus = false
                                    };
                                }
                                #endregion
                            }

                            if (gDetails.IsOnline != null)
                            {
                                blGolferLoginStatus = Convert.ToBoolean(gDetails.IsOnline);
                            }
                        }
                    }
                }
                catch
                {
                    blGolferLoginStatus = false;
                }

                #region Get Course Club House ID

                var ClubHouse = _db.GF_CourseInfo.FirstOrDefault(x => x.ID == adminUsers.CourseId && !(x.IsClubHouse ?? true));
                long newCourseID = adminUsers.CourseId ?? 0;
                if (ClubHouse != null)
                {
                    newCourseID = ClubHouse.ClubHouseID ?? 0;
                }

                #endregion

                var aUsers = _db.GF_AdminUsers.Where(x => (x.Type == UserType.Cartie || x.Type == UserType.PowerAdmin) &&
                    x.CourseId == newCourseID && //adminUsers.CourseId &&
                    (x.IsOnline ?? false) &&
                    x.Status == StatusType.Active);

                if (aUsers.Count() > 0)
                {
                    return new ResultOrder
                    {
                        Id = adminUsers.ID,
                        Status = 1,
                        Error = "Online",
                        GolferLoginStatus = blGolferLoginStatus
                    };
                }
                else
                {
                    return new ResultOrder
                    {
                        Id = adminUsers.ID,
                        Status = 1,
                        Error = "Offline",
                        GolferLoginStatus = blGolferLoginStatus
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResultOrder
                {
                    Id = adminUsers.ID,
                    Status = 0,
                    Error = ex.Message,
                    GolferLoginStatus = true
                };
            }
        }

        /// <summary>
        /// Created By: Amit Kumar
        /// Created Date: 17 April 2015
        /// Purpose: Get course user current position
        /// </summary>
        /// <param name="objOrder"></param>
        /// <returns></returns>
        public Result getCourseUserPosition(GF_UserCurrentPosition userPosition)
        {
            try
            {
                _db = new GolflerEntities();

                var lstOrder = _db.GF_Order.FirstOrDefault(x => x.ID == userPosition.OrderID);

                if (lstOrder == null)
                {
                    return new Result
                    {
                        Id = userPosition.ID,
                        Status = 0,
                        Error = "Invalid Orders",
                        record = null
                    };
                }

                #region Power Admin

                if (lstOrder.CartieId > 0)
                {
                    var powerAdmin = _db.GF_AdminUsers.FirstOrDefault(x => x.ID == lstOrder.CartieId &&
                        x.Type == UserType.PowerAdmin && x.Status == StatusType.Active);

                    if (powerAdmin != null)
                    {
                        var courseLocation = _db.GF_CourseInfo.FirstOrDefault(x => x.ID == powerAdmin.CourseId);

                        var paOrderCount = _db.GF_Order.Where(x => x.CartieId == (lstOrder.CartieId ?? 0) &&
                        !(x.IsDelivered ?? false) &&
                        !(x.IsRejected ?? false)).Count();

                        return new Result
                        {
                            Id = userPosition.ID,
                            Status = 1,
                            Error = "Success",
                            record = new
                            {
                                Latitude = courseLocation != null ? courseLocation.LATITUDE : "0",
                                Longitude = courseLocation != null ? courseLocation.LONGITUDE : "0",
                                lstOrder.OrderStatus,
                                lstOrder.RGBColor,
                                lstOrder.HEXColor,
                                lstOrder.HUEColor,
                                lstOrder.CartieId,
                                CartieName = powerAdmin.FirstName + " " + powerAdmin.LastName,
                                OrderCount = paOrderCount
                            }
                        };
                    }
                }

                #endregion

                var uPosition = _db.GF_UserCurrentPosition.FirstOrDefault(x => x.CourseID == (userPosition.CourseID ?? 0) &&
                    x.ReferenceID == lstOrder.CartieId && x.ReferenceType == userPosition.ReferenceType);

                if (uPosition != null)
                {
                    string CartieName = "";
                    int OrderCount = 0;

                    OrderCount = _db.GF_Order.Where(x => x.CartieId == (lstOrder.CartieId ?? 0) &&
                        !(x.IsDelivered ?? false) &&
                        !(x.IsRejected ?? false)).Count();

                    try
                    {
                        CartieName = _db.GF_AdminUsers.FirstOrDefault(x => x.ID == lstOrder.CartieId).FirstName + " "
                            + _db.GF_AdminUsers.FirstOrDefault(x => x.ID == lstOrder.CartieId).LastName;
                    }
                    catch
                    {

                    }
                    return new Result
                    {
                        Id = userPosition.ID,
                        Status = 1,
                        Error = "Success",
                        record = new
                        {
                            uPosition.Latitude,
                            uPosition.Longitude,
                            lstOrder.OrderStatus,
                            lstOrder.RGBColor,
                            lstOrder.HEXColor,
                            lstOrder.HUEColor,
                            lstOrder.CartieId,
                            CartieName,
                            OrderCount
                        }
                    };
                }
                else
                {
                    return new Result
                    {
                        Id = userPosition.ID,
                        Status = 1,
                        Error = "No cartie found.",
                        record = new
                        {
                            Latitude = "",
                            Longitude = "",
                            lstOrder.OrderStatus
                        }
                    };
                }
            }
            catch (Exception ex)
            {
                return new Result
                {
                    Id = userPosition.ID,
                    Status = 0,
                    Error = ex.Message,
                    record = null
                };
            }
        }

        #endregion

        #region Split Order Payment

        /// <summary>
        /// Created By: Amit Kumar
        /// Created Date: 23 April 2015
        /// Purpose: Get commission in order placed
        /// </summary>
        /// <param name="objOrder"></param>
        /// <returns></returns>
        public void SplitOrderAmount(GF_Order objOrder, ref decimal comAmount, ref decimal cpf, ref decimal cpfRate)
        {
            try
            {
                _db = new GolflerEntities();

                var course = _db.GF_CourseInfo.FirstOrDefault(x => x.ID == objOrder.CourseID);

                comAmount = 0;
                cpf = 0;
                cpfRate = 0;

                if (!(course.IsPlateformFeeActive ?? false))
                {
                    var foodCommission = _db.GF_FoodCommission.Where(x => x.CourseID == objOrder.CourseID);
                    var courseUnitPrice = _db.GF_CourseFoodItemDetail.Where(x => x.GF_CourseFoodItem.CourseID == objOrder.CourseID);

                    foreach (var item in objOrder.MenuItems)
                    {
                        var subCatID = _db.GF_MenuItems.FirstOrDefault(x => x.ID == item.ItemId);

                        var commission = foodCommission.FirstOrDefault(x => x.CategoryID == subCatID.GF_SubCategory.GF_Category.ID);

                        var unitPrice = courseUnitPrice.FirstOrDefault(x => x.MenuItemID == item.ItemId);

                        comAmount = comAmount + (((unitPrice.Price ?? 0) * ((commission.Commission ?? 0) / 100)) * item.Quantity);
                    }
                }
                //else
                //{
                    cpf = (((objOrder.GrandTotal ?? 0) - (objOrder.GolferPlatformFee ?? 0)) + (course.PlateformFee ?? 0));
                    cpfRate = course.PlateformFee ?? 0;
                //}
            }
            catch
            {
                comAmount = 0;
                cpf = 0;
                cpfRate = 0;
            }
        }

        #endregion

        #region Promo Code Validation Check

        /// <summary>
        /// Create By: Amit Kumar
        /// Created By: 11 June 2015
        /// Purpose: Check either enter promo code is valid or not
        /// </summary>
        /// <param name="promoCode"></param>
        /// <returns></returns>
        public bool PromoCodeValid(long promoCodeID, long courseID, ref string msg)
        {
            try
            {
                _db = new GolflerEntities();

                var lstPromoCode = _db.GF_PromoCode.FirstOrDefault(x => x.ID == promoCodeID && x.Status == StatusType.Active && x.CourseID == courseID);

                if (lstPromoCode != null)
                {
                    if ((lstPromoCode.IsOneTimeUse ?? false) && (lstPromoCode.IsUsed ?? false))
                    {
                        msg = "This promo code is already been used.";
                        return false;
                    }
                    else if ((lstPromoCode.IsOneTimeUse ?? false) && !(lstPromoCode.IsUsed ?? false))
                    {
                        lstPromoCode.IsUsed = true;
                        _db.SaveChanges();

                        msg = "Success";
                        return true;
                    }
                    else if (!(lstPromoCode.IsOneTimeUse ?? false))
                    {
                        msg = "Success";
                        return true;
                    }
                    else
                    {
                        msg = "Invalid Promo Code";
                        return false;
                    }
                }
                else
                {
                    msg = "Invalid Promo Code";
                    return false;
                }
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                return false;
            }
        }

        #endregion
    }

    public class OrderMenuitems
    {
        public long ItemId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public List<GF_OrderMenuOptionDetail> MenuItemOption { get; set; }
    }

    public class OrderHistory
    {
        public long courseID { get; set; }
        public long orderID { get; set; }
        public long golferID { get; set; }
        public string orderDate { get; set; }

        //Paging
        public int? pageIndex { get; set; }
        public int? pageSize { get; set; }
    }

    public partial class GF_UserCurrentPosition
    {
        public long OrderID { get; set; }
    }

    public class MenuOptionName
    {
        public long ID { get; set; }
        public string Name { get; set; }
    }
}