using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Xml;
using CourseWebApi.Models;
namespace CourseWebApi.Controllers
{
    public class CourseController : ApiController
    {
        #region Login
        /// <summary>
        /// Created By: Veera
        /// created On: 24 March 2015
        /// Purpose: Login web service
        /// <param name="log">contains parameters Email and Password for Login</param>
        /// <returns>returns Status 0 or 1 ,Id of user,Error in case of Fail to Login</returns>
        [HttpPost]
        public CourseLoginResult LoginService([FromBody]Login log)
        {
            if (string.IsNullOrEmpty(log.UserName) || string.IsNullOrEmpty(log.Password) || string.IsNullOrEmpty(log.DeviceId) || string.IsNullOrEmpty(log.DeviceType) || string.IsNullOrEmpty(log.Latitude) || (string.IsNullOrEmpty(log.Longitude)))
            {
                return new CourseLoginResult { Id = 0, Status = 0, record = null, Error = "One of the required parameter is missing." };
            }

            string devSerialNo = "";
            try
            {
                if (log.DeviceSerialNo == null)
                {
                    devSerialNo = "";
                }
                else
                {
                    devSerialNo = Convert.ToString(log.DeviceSerialNo);
                }
            }
            catch
            {
                devSerialNo = "";
            }
            log.DeviceSerialNo = devSerialNo;
            return log.LoginCourseUser(log.UserName, log.Password, log.DeviceId, log.DeviceType, log.Latitude, log.Longitude, log.AppVersion,log.DeviceSerialNo);
        }
        #endregion

        #region Orders

        /// <summary>
        /// Created By: Amit Kumar
        /// created On: 25 March 2015
        /// Purpose: List of those Orders which is accepted by cartie
        /// </summary>
        /// <param name="objOrder">List of Paramters sent from iphone/android  </param>
        /// <returns>Result class containing three parameters Id of user,Status= 0 or 1,Error in case of fail  </returns>
        [HttpPost]
        public Result ListOfOrders([FromBody]GF_Order objOrder)
        {
            if (string.IsNullOrEmpty(Convert.ToString(objOrder.CartieId)))
            {
                return new Result { Id = 0, Status = 0, record = null, Error = "One of the required parameter is missing." };
            }

            Order obj = new Order();
            return (obj.GetOrderList(objOrder.CartieId));
        }

        /// <summary>
        /// Created By: Amit Kumar
        /// created On: 25 March 2015
        /// Purpose: Accepted/Rejected Orders by cartie
        /// </summary>
        /// <param name="objOrder">List of Paramters sent from iphone/android  </param>
        /// <returns>Result class containing three parameters Id of user,Status= 0 or 1,Error in case of fail  </returns>
        [HttpPost]
        public Result OrderAcceptReject([FromBody]GF_OrderAcceptReject objOrder)
        {
            if (string.IsNullOrEmpty(Convert.ToString(objOrder.OrderID)) ||
                string.IsNullOrEmpty(Convert.ToString(objOrder.ReferenceID)) ||
                string.IsNullOrEmpty(Convert.ToString(objOrder.ReferenceType)) ||
                string.IsNullOrEmpty(Convert.ToString(objOrder.Status)))
            {
                return new Result { Id = 0, Status = 0, record = null, Error = "One of the required parameter is missing." };
            }

            Order obj = new Order();
            return (obj.OrderAcceptReject(objOrder));
        }

        /// <summary>
        /// Created By: Amit Kumar
        /// created On: 25 March 2015
        /// Purpose: List of those Orders which is accepted by kitchen
        /// </summary>
        /// <param name="objOrder">List of Paramters sent from iphone/android  </param>
        /// <returns>Result class containing three parameters Id of user,Status= 0 or 1,Error in case of fail  </returns>
        [HttpPost]
        public Result ListOfKitchenOrders([FromBody]GF_Order objOrder)
        {
            if (string.IsNullOrEmpty(Convert.ToString(objOrder.KitchenId)))
            {
                return new Result { Id = 0, Status = 0, record = null, Error = "One of the required parameter is missing." };
            }

            Order obj = new Order();
            return (obj.GetKitchenOrderList(objOrder.KitchenId));
        }

        /// <summary>
        /// Created By: Amit Kumar
        /// created On: 26 March 2015
        /// Purpose: List of those Orders which is placed in course
        /// </summary>
        /// <param name="objOrder">List of Paramters sent from iphone/android  </param>
        /// <returns>Result class containing three parameters Id of user,Status= 0 or 1,Error in case of fail  </returns>
        [HttpPost]
        public Result ListOfCourseOrders([FromBody]GF_Order objOrder)
        {
            if (string.IsNullOrEmpty(Convert.ToString(objOrder.CourseID)) ||
                string.IsNullOrEmpty(Convert.ToString(objOrder.referenceID)) ||
                string.IsNullOrEmpty(objOrder.referenceType) ||
                string.IsNullOrEmpty(Convert.ToString(objOrder.pageIndex)))
            {
                return new Result { Id = 0, Status = 0, record = null, Error = "One of the required parameter is missing." };
            }

            Order obj = new Order();
            return (obj.GetCourseOrderList(objOrder));
        }

        /// <summary>
        /// Created By: Amit Kumar
        /// created On: 26 March 2015
        /// Purpose: Update the order delivery status
        /// </summary>
        /// <param name="objOrder">List of Paramters sent from iphone/android  </param>
        /// <returns>Result class containing three parameters Id of user,Status= 0 or 1,Error in case of fail  </returns>
        [HttpPost]
        public Result OrderDeliveryStatus([FromBody]GF_OrderAcceptReject objOrder)
        {
            if (string.IsNullOrEmpty(Convert.ToString(objOrder.OrderID)) ||
                string.IsNullOrEmpty(Convert.ToString(objOrder.ReferenceID)) ||
                string.IsNullOrEmpty(Convert.ToString(objOrder.ReferenceType)))
            {
                return new Result { Id = 0, Status = 0, record = null, Error = "One of the required parameter is missing." };
            }

            Order obj = new Order();
            return (obj.SaveOrderDeliveryStatus(objOrder));
        }

        /// <summary>
        /// Created By: Amit Kumar
        /// created On: 30 March 2015
        /// Purpose: List of Orders History
        /// </summary>
        /// <param name="objOrder">List of Paramters sent from iphone/android  </param>
        /// <returns>Result class containing three parameters Id of user,Status= 0 or 1,Error in case of fail  </returns>
        [HttpPost]
        public Result ListOfOrdersHistory([FromBody]OrderHistory orderHistory)
        {
            if (string.IsNullOrEmpty(Convert.ToString(orderHistory.referenceID)) ||
                string.IsNullOrEmpty(orderHistory.referenceType))
            {
                return new Result { Id = 0, Status = 0, record = null, Error = "One of the required parameter is missing." };
            }

            Order obj = new Order();
            return (obj.GetOrderHistoryList(orderHistory));
        }

        /// <summary>
        /// Created By: Amit Kumar
        /// Created Date: 02 April 2015
        /// Purpose: Update the order delivery status
        /// </summary>
        /// <param name="objOrder">List of Paramters sent from iphone/android  </param>
        /// <returns>Result class containing three parameters Id of user,Status= 0 or 1,Error in case of fail  </returns>
        [HttpPost]
        public Result OrderDeliveryStatusByCartie([FromBody]GF_Order objOrder)
        {
            if (string.IsNullOrEmpty(Convert.ToString(objOrder.OrderId)) ||
                string.IsNullOrEmpty(Convert.ToString(objOrder.CartieId)))
            {
                return new Result { Id = 0, Status = 0, record = null, Error = "One of the required parameter is missing." };
            }

            Order obj = new Order();
            return (obj.UpdateOrderDeliveryStatus(objOrder));
        }

        /// <summary>
        /// Created By: Amit Kumar
        /// Created Date: 02 April 2015
        /// Purpose: Update the order pickup status
        /// </summary>
        /// <param name="objOrder">List of Paramters sent from iphone/android  </param>
        /// <returns>Result class containing three parameters Id of user,Status= 0 or 1,Error in case of fail  </returns>
        [HttpPost]
        public Result OrderPickupStatus([FromBody]GF_Order objOrder)
        {
            if (string.IsNullOrEmpty(Convert.ToString(objOrder.OrderId)))
            {
                return new Result { Id = 0, Status = 0, record = null, Error = "One of the required parameter is missing." };
            }

            Order obj = new Order();
            return (obj.UpdateOrderPickupStatus(objOrder));
        }

        /// <summary>
        /// Created By: Amit Kumar
        /// Created Date: 02 April 2015
        /// Purpose: Update the order ready status
        /// </summary>
        /// <param name="objOrder">List of Paramters sent from iphone/android  </param>
        /// <returns>Result class containing three parameters Id of user,Status= 0 or 1,Error in case of fail  </returns>
        [HttpPost]
        public Result OrderReadyStatus([FromBody]GF_Order objOrder)
        {
            if (objOrder.OrderId <= 0 ||
                objOrder.referenceID <= 0 ||
                string.IsNullOrEmpty(objOrder.referenceType))
            {
                return new Result { Id = 0, Status = 0, record = null, Error = "One of the required parameter is missing." };
            }

            Order obj = new Order();
            return (obj.UpdateOrderReadyStatus(objOrder));
        }

        /// <summary>
        /// Created By: Amit Kumar
        /// created On: 08 April 2015
        /// Purpose: List of those Orders which is accepted by Prohsop
        /// </summary>
        /// <param name="objOrder">List of Paramters sent from iphone/android  </param>
        /// <returns>Result class containing three parameters Id of user, Status = 0 or 1,Error in case of fail  </returns>
        [HttpPost]
        public Result ListOfProshopOrders([FromBody]GF_Order objOrder)
        {
            if (string.IsNullOrEmpty(Convert.ToString(objOrder.ProShopID)))
            {
                return new Result { Id = 0, Status = 0, record = null, Error = "One of the required parameter is missing." };
            }

            Order obj = new Order();
            return (obj.GetProshopOrderList(objOrder.ProShopID));
        }

        /// <summary>
        /// Created By: Amit Kumar
        /// created On: 11 March 2016
        /// Purpose: List of those Orders which is accepted by Power Admin
        /// </summary>
        /// <param name="objOrder">List of Paramters sent from iphone/android  </param>
        /// <returns>Result class containing three parameters Id of user, Status = 0 or 1,Error in case of fail  </returns>
        [HttpPost]
        public Result ListOfPowerAdminOrders([FromBody]GF_Order objOrder)
        {
            if (string.IsNullOrEmpty(Convert.ToString(objOrder.ProShopID)))
            {
                return new Result { Id = 0, Status = 0, record = null, Error = "One of the required parameter is missing." };
            }

            Order obj = new Order();
            return (obj.GetPowerAdminOrderList(objOrder.ProShopID));
        }

        /// <summary>
        /// Created By: Amit Kumar
        /// created On: 01 May 2015
        /// Purpose: Get Order Statistics Report
        /// </summary>
        /// <param name="objOrder">List of Paramters sent from iphone/android  </param>
        /// <returns>Result class containing three parameters Id of user, Status = 0 or 1,Error in case of fail  </returns>
        [HttpPost]
        public Result GetOrderStatisticsReport([FromBody]OrderReport orderReport)
        {
            if (orderReport.CourseID <= 0 ||
                string.IsNullOrEmpty(Convert.ToString(orderReport.ReportType)) ||
                string.IsNullOrEmpty(Convert.ToString(orderReport.ReportValue)))
            {
                return new Result { Id = 0, Status = 0, record = null, Error = "One of the required parameter is missing." };
            }

            Order obj = new Order();
            return (obj.GetOrderStatisticsReport(orderReport));
        }

        #endregion

        #region Weather

        /// <summary>
        /// Created By: Veera
        /// created On: 25 March 2015
        /// Purpose: Weather web service
        /// <param name="log">contains parameters Email and Password for Login</param>
        /// <returns>returns Status 0 or 1 ,Id of user,Error in case of Fail to Login</returns>
        [HttpPost]
        public Result Getweather([FromBody]Login log)
        {
            //yahoo
            /*  string query = String.Format("http://weather.yahooapis.com/forecastrss?w=44418");
              XmlDocument wData = new XmlDocument();
              wData.Load(query);

              XmlNamespaceManager manager = new XmlNamespaceManager(wData.NameTable);
              manager.AddNamespace("yweather", "http://xml.weather.yahoo.com/ns/rss/1.0");
              XmlNode channel = wData.SelectSingleNode("rss").SelectSingleNode("channel");
              XmlNodeList nodes = wData.SelectNodes("/rss/channel/item/yweather:forecast", manager);

              string Temperature = channel.SelectSingleNode("item").SelectSingleNode("yweather:condition", manager).Attributes["temp"].Value;
              string Condition = channel.SelectSingleNode("item").SelectSingleNode("yweather:condition", manager).Attributes["text"].Value;
              string Humidity = channel.SelectSingleNode("yweather:atmosphere", manager).Attributes["humidity"].Value;
              string WindSpeed = channel.SelectSingleNode("yweather:wind", manager).Attributes["speed"].Value;
              string TFCond = channel.SelectSingleNode("item").SelectSingleNode("yweather:forecast", manager).Attributes["text"].Value;
              string TFHigh = channel.SelectSingleNode("item").SelectSingleNode("yweather:forecast", manager).Attributes["high"].Value;
              string TFLow = channel.SelectSingleNode("item").SelectSingleNode("yweather:forecast", manager).Attributes["low"].Value;*/

            /*http://api.openweathermap.org/data/2.5/weather?lat=35&lon=139*/
            return new Result { Id = 0, Status = 1, record = null, Error = "One of the required parameter is missing." };
            /*http://api.wunderground.com/api/Your_Key/geolookup/q/37.776289,-122.395234.json*/
        }

        #endregion

        #region ForgotPassword
        /// <summary>
        /// Created By: Veera
        /// created On: 24-march-2015
        /// </summary>
        /// <param name="Email">User Email where password to send  </param>
        /// <returns>returns Status 0 or 1 ,Id of user,Error in case of Fail to Mailing Service</returns>
        [HttpPost]
        public Result ForgotPasswordService([FromBody]Login log)
        {
            if (string.IsNullOrEmpty(log.UserName))
            {
                return new Result { Id = 0, Status = 0, Error = "UserName is required.", record = null };
            }
            return log.ForgotPassword(log.UserName);

        }
        #endregion

        #region Logout
        /// <summary>
        /// Created By:Veera
        /// Created Date:
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>        
        [HttpPost]
        public Result Logout([FromBody]Login obj)
        {
            if (obj.ID == 0)
            {
                return new Result { Id = 0, Status = 0, record = null, Error = "One of the required parameter is missing." };
            }
            return obj.Logout(obj.ID);
        }
        #endregion

        #region WeatherApi
        /// <summary>
        /// Created By:Veera
        /// Created Date:14 April 2015
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>        
        [HttpPost]
        public Result WeatherApi([FromBody]GF_WeatherDetails obj)
        {
            if (string.IsNullOrEmpty(Convert.ToString(obj.OrderID)) || string.IsNullOrEmpty(Convert.ToString(obj.GolferID)) || string.IsNullOrEmpty(Convert.ToString(obj.Longitude)) || string.IsNullOrEmpty(Convert.ToString(obj.Latitude)))
            {
                return new Result { Id = 0, Status = 0, record = null, Error = "One of the required parameter is missing." };
            }
            weather objWeather = new weather();
            return objWeather.AddWeather(obj);
        }
        #endregion

        #region Manage Cartie

        /// <summary>
        /// Created By: Amit Kumar
        /// created On: 17 April 2015
        /// Purpose: Save user current position
        /// </summary>
        /// <param name="objOrder">List of Paramters sent from iphone/android  </param>
        /// <returns>Result class containing three parameters Id of user,Status= 0 or 1,Error in case of fail  </returns>
        [HttpPost]
        public Result UserCurrentPosition([FromBody]GF_UserCurrentPosition uPosition)
        {
            if (uPosition.CourseID <= 0 ||
                uPosition.ReferenceID <= 0 ||
                string.IsNullOrEmpty(uPosition.ReferenceType) ||
                string.IsNullOrEmpty(uPosition.Latitude) ||
                string.IsNullOrEmpty(uPosition.Longitude))
            {
                return new Result { Id = 0, Status = 0, record = null, Error = "One of the required parameter is missing." };
            }

            string devSerialNo = "";
            try
            {
                if (uPosition.DeviceSerialNo == null)
                {
                    devSerialNo = "";
                }
                else
                {
                    devSerialNo = Convert.ToString(uPosition.DeviceSerialNo);
                }
            }
            catch
            {
                devSerialNo = "";
            }
            uPosition.DeviceSerialNo = devSerialNo;

            Order obj = new Order();


            return (obj.SaveUserCurrentPosition(uPosition));
        }

        #endregion

        #region Manage Menu

        /// <summary>
        /// Created By: Amit Kumar
        /// created On: 28 April 2015
        /// Purpose: Get campared menu(s) list
        /// </summary>
        /// <param name="">List of Paramters sent from iphone/android  </param>
        /// <returns>Result class containing three parameters Id of user,Status= 0 or 1,Error in case of fail  </returns>
        [HttpPost]
        public Result CampareMenu([FromBody]CampareMenu objCampareMenu)
        {
            if (string.IsNullOrEmpty(objCampareMenu.searchText) || objCampareMenu.courseID <= 0)
            {
                return new Result { Id = 0, Status = 0, record = null, Error = "One of the required parameter is missing." };
            }

            Menu obj = new Menu();
            return (obj.GetCamparedMenuList(objCampareMenu));
        }

        /// <summary>
        /// Created By: Amit Kumar
        /// created On: 27 April 2015
        /// Purpose: Get menu(s) and user(s) list
        /// </summary>
        /// <param name="">List of Paramters sent from iphone/android  </param>
        /// <returns>Result class containing three parameters Id of user,Status= 0 or 1,Error in case of fail  </returns>
        [HttpPost]
        public Result CourseUserAndMenuList([FromBody]GF_AdminUsers objAdmin)
        {
            if (objAdmin.ID <= 0 || objAdmin.CourseId <= 0)
            {
                return new Result { Id = 0, Status = 0, record = null, Error = "One of the required parameter is missing." };
            }

            Menu obj = new Menu();
            return (obj.GetCourseUserAndMenuList(objAdmin));
        }

        /// <summary>
        /// Created By: Amit Kumar
        /// created On: 28 April 2015
        /// Purpose: Activate sub category
        /// </summary>
        /// <param name="">List of Paramters sent from iphone/android  </param>
        /// <returns>Result class containing three parameters Id of user,Status= 0 or 1,Error in case of fail  </returns>
        [HttpPost]
        public Result ActivateSubCategory([FromBody]GF_CourseFoodItem courseFoodItem)
        {
            if (courseFoodItem.SubCategoryID <= 0 ||
                courseFoodItem.CourseID <= 0 ||
                courseFoodItem.UserID <= 0)
            {
                return new Result { Id = 0, Status = 0, record = null, Error = "One of the required parameter is missing." };
            }

            Menu obj = new Menu();
            return (obj.ActivateSubCategory(courseFoodItem));
        }

        /// <summary>
        /// Created By: Amit Kumar
        /// created On: 28 April 2015
        /// Purpose: Save Course Food Item
        /// </summary>
        /// <param name="">List of Paramters sent from iphone/android  </param>
        /// <returns>Result class containing three parameters Id of user,Status= 0 or 1,Error in case of fail  </returns>
        [HttpPost]
        public Result SaveCourseFoodItem([FromBody]GF_CourseFoodItemDetail courseFoodItem)
        {
            //courseFoodItem.CourseFoodItemID <= 0 ||
            if (courseFoodItem.Price <= 0 ||
                courseFoodItem.CourseID <= 0 ||
                courseFoodItem.MenuItemID <= 0)
            {
                return new Result { Id = 0, Status = 0, record = null, Error = "One of the required parameter is missing." };
            }

            Menu obj = new Menu();
            return (obj.SaveCourseFoodItem(courseFoodItem));
        }

        #endregion

        #region Manage Course Admin User

        /// <summary>
        /// Created By: Veera
        /// created On: 27-April-2015
        /// </summary>
        /// <param name="objUser"> List of Paramters sent from iphone/android  </param>
        /// <returns>Result class containing three parameters Id of user,Status= 0 or 1,Error in case of fail </returns>
        [HttpPost]
        public Result AddAdminUser([FromBody]GF_AdminUsers objUser)
        {
            if (!(string.IsNullOrEmpty(Convert.ToString(objUser.ID))))
            {
                if (objUser.ID > 0)
                {
                    if (string.IsNullOrEmpty(objUser.FirstName)
                           || string.IsNullOrEmpty(objUser.LastName)
                        // || string.IsNullOrEmpty(objUser.UserName)
                           || string.IsNullOrEmpty(objUser.Password)
                            || string.IsNullOrEmpty(objUser.Email)
                           || string.IsNullOrEmpty(objUser.Type))
                    {
                        return new Result { Id = 0, Status = 0, record = null, Error = "One of the required parameter is missing." };
                    }
                    else
                    {
                        Users obj = new Users();
                        return obj.AddUpdateCourseUser(objUser);

                    }

                }
                else
                {
                    if (string.IsNullOrEmpty(objUser.Email)
                       || string.IsNullOrEmpty(Convert.ToString(objUser.CourseId))
                       || string.IsNullOrEmpty(objUser.FirstName)
                       || string.IsNullOrEmpty(objUser.LastName)
                        //  || string.IsNullOrEmpty(objUser.UserName)
                       || string.IsNullOrEmpty(objUser.Password)
                       || string.IsNullOrEmpty(objUser.Type)
                       || string.IsNullOrEmpty(Convert.ToString(objUser.CreatedBy))
                       || string.IsNullOrEmpty(objUser.Phone))
                    {
                        return new Result { Id = 0, Status = 0, record = null, Error = "One of the required parameter is missing." };
                    }
                    else if (objUser.UserName.Length > 5)
                    {
                        Users obj = new Users();
                        return obj.AddUpdateCourseUser(objUser);

                    }
                    else
                    {
                        return new Result { Id = 0, Status = 0, record = null, Error = "UserName must be atleast 6 Characters long" };
                    }

                }
            }
            else
            {
                return new Result { Id = 0, Status = 0, record = null, Error = "One of the required parameter is missing." };
            }


        }

        /// <summary>
        /// Created By: Veera
        /// created On: 27-April-2015
        /// </summary>
        /// <param name="objUser"> List of Paramters sent from iphone/android  </param>
        /// <returns>Result class containing three parameters Id of user,Status= 0 or 1,Error in case of fail </returns>
        [HttpPost]
        public Result DeleteAdminUser([FromBody]GF_AdminUsers objUser)
        {
            if (objUser.ID == 0)
            {
                return new Result { Id = 0, Status = 0, record = null, Error = "One of the required parameter is missing." };
            }
            Users obj = new Users();
            return obj.DeleteCourseUser(objUser.ID);

        }

        #endregion

        #region Push Notification Test

        /// <summary>
        /// Created By: Amit Kumar
        /// created On: 07 August 2015
        /// Purpose: Push Notification Test
        /// </summary>
        /// <param name="">List of Paramters sent from iphone/android  </param>
        /// <returns>Result class containing three parameters Id of user,Status= 0 or 1,Error in case of fail  </returns>
        [HttpPost]
        public Result PushNotificationTest([FromBody]PushNoticationsTest pNotications)
        {
            if (pNotications.SenderId <= 0 ||
                string.IsNullOrEmpty(pNotications.SenderName) ||
                pNotications.ReceiverId <= 0 ||
                string.IsNullOrEmpty(pNotications.ReceiverName) ||
                string.IsNullOrEmpty(pNotications.pushMsgFrom) ||
                string.IsNullOrEmpty(pNotications.DeviceType) ||
                string.IsNullOrEmpty(pNotications.Message))
            {
                return new Result { Id = 0, Status = 0, record = null, Error = "One of the required parameter is missing." };
            }

            Order obj = new Order();
            return (obj.PushNotificationTest(pNotications));
        }

        #endregion
    }
}
