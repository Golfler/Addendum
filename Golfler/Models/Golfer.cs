using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data.Objects;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Golfler.Models
{
    [MetadataType(typeof(GolferMetaData))]
    public partial class GF_Golfer
    {
        GolflerEntities Db = null;
        public string Message { get; private set; }

        public bool Active { get; set; }

        public bool ReceiveEmail { get; set; }
        public bool ReceivePushNotification { get; set; }
        public bool ReceiveChat { get; set; }
        public string Birthdate { get; set; }

        public bool ReceiveEmailGolfer { get; set; }
        public bool ReceivePushNotificationGolfer { get; set; }
        /// <summary>
        /// Created By:Arun
        /// Created Date:26 March 2015
        /// Purpose: Get Golfer listing
        /// </summary>
        /// <param name="filterExpression"></param>
        /// <param name="sortExpression"></param>
        /// <param name="sortDirection"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalRecords"></param>
        /// <returns></returns>
        public IQueryable<GF_Golfer> GetAdminUsers(string filterExpression, string sortExpression, string sortDirection, int pageIndex, int pageSize, ref int totalRecords)
        {
            Db = new GolflerEntities();
            IQueryable<GF_Golfer> list;

            if (!String.IsNullOrWhiteSpace(filterExpression))
                list = Db.GF_Golfer.Where(x => (x.Email.ToLower().Contains(filterExpression.ToLower()) ||
                     (x.FirstName + " " + x.LastName).ToLower().Contains(filterExpression.ToLower()) && x.Status != StatusType.Delete));
            else
                list = Db.GF_Golfer.Where(x => x.Status != StatusType.Delete);

            totalRecords = list.Count();

            return list.OrderBy(sortExpression + " " + sortDirection).Skip((pageIndex - 1) * pageSize).Take(pageSize);
        }

        public IQueryable<GolferUsersInformation> GetGolferUsers(string filterExpression, string sortExpression, string sortDirection, int pageIndex, int pageSize, ref int totalRecords)
        {
            Db = new GolflerEntities();
            //IQueryable<GF_Golfer> list;

            //if (!String.IsNullOrWhiteSpace(filterExpression))
            //    list = Db.GF_Golfer.Where(x => (x.Email.ToLower().Contains(filterExpression.ToLower()) ||
            //         (x.FirstName + " " + x.LastName).ToLower().Contains(filterExpression.ToLower()) && x.Status != StatusType.Delete));
            //else
            //    list = Db.GF_Golfer.Where(x => x.Status != StatusType.Delete);

            //totalRecords = list.Count();

            IQueryable<GolferUsersInformation> list;

            var fExpression = new SqlParameter
            {
                ParameterName = "FilterExpression",
                Value = filterExpression
            };

            list = Db.Database.SqlQuery<GolferUsersInformation>("exec GF_SP_GetGolferUser @FilterExpression",
                fExpression).ToList<GolferUsersInformation>().AsQueryable();

            totalRecords = list.Count();

            return list.OrderBy(sortExpression + " " + sortDirection).Skip((pageIndex - 1) * pageSize).Take(pageSize);
        }

        /// <summary>
        /// Created By:Arun
        /// Created Date:26 March 2015
        /// Purpose: Chnage Status
        /// </summary>
        /// <param name="id"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        internal bool ChangeStatus(long id, string status)
        {
            GF_RoleModules objModuleRole = new GF_RoleModules();
            Db = new GolflerEntities();

            var objGolfer = Db.GF_Golfer.FirstOrDefault(x => x.GF_ID == id);
            ///// if (LoginInfo.IsSuper)
            /////    objModuleRole = CommonFunctions.GetAccessModule(ModuleValues.AllRights);
            ////////else
            ////////    objModuleRole = CommonFunctions.GetAccessModule(ModuleValues.Golfer);

            //if (!objModuleRole.UpdateFlag)
            //{
            //    Message = "Not Access";//Resources.Resources.unaccess;
            //    return false;
            //}
            if (objGolfer != null)
            {
                objGolfer.Status = status == StatusType.Active ? StatusType.InActive : StatusType.Active;

                Db.SaveChanges();

                Active = status == StatusType.Active ? true : false;

                return true;
            }
            return false;
        }

        /// <summary>
        /// Created By:Arun
        /// Created Date:26 March 2015
        /// Purpose: Delete selected user(s).
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        internal bool DeleteGolfer(long[] ids)
        {
            GF_RoleModules objModuleRole = new GF_RoleModules();

            Db = new GolflerEntities();
            if (LoginInfo.IsSuper)
                objModuleRole = CommonFunctions.GetAccessModule(ModuleValues.AllRights);
            //////////else
            //////////    objModuleRole = CommonFunctions.GetAccessModule(ModuleValues.Golfer);

            if (!objModuleRole.DeleteFlag)
            {
                Message = "Not Access";// Resources.Resources.unaccess;
                return false;
            }

            var golfer = Db.GF_Golfer.Where(x => ids.AsQueryable().Contains(x.GF_ID));
            foreach (var r in golfer)
            {
                r.Status = StatusType.Delete;
            }
            Db.SaveChanges();
            return true;
        }


        /// <summary>
        /// Created By:Arun
        /// Created Date: 27 march 2015
        /// Purpose: Get Golfer By ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public GF_Golfer GetGolferById(long id)
        {

            Db = new GolflerEntities();
            var obj = Db.GF_Golfer.FirstOrDefault(x => x.GF_ID == id);
            if (obj == null)
            {
                obj = new GF_Golfer();
            }
            return obj;
        }

        public string GetGolferNameById(long id)
        {
            Db = new GolflerEntities();
            var obj = Db.GF_Golfer.Where(x => x.GF_ID == id).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault();
            if (obj == null)
            {
                return "-";
            }
            else
            {
                return obj;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="objGolfer"></param>
        /// <returns></returns>
        public bool UpdateGolferInfo(GF_Golfer objGolfer, ref string image)
        {

            GF_RoleModules objModuleRole = new GF_RoleModules();
            bool IsUpdateMail = false;

            //if (LoginInfo.IsSuper)
            //    objModuleRole = CommonFunctions.GetAccessModule(ModuleValues.AllRights);
            //else
            //    objModuleRole = CommonFunctions.GetAccessModule(ModuleValues.Golfer);

            //if (!objModuleRole.DeleteFlag)
            //{
            //    Message = "Not Access";// Resources.Resources.unaccess;
            //    return false;
            //}

            Db = new GolflerEntities();
            var obj = Db.GF_Golfer.FirstOrDefault(x => x.GF_ID == objGolfer.GF_ID);
            if (obj != null)
            {
                obj.FirstName = objGolfer.FirstName;
                obj.LastName = objGolfer.LastName;

                if (objGolfer.TimeZoneId != null)
                {
                    obj.TimeZoneId = objGolfer.TimeZoneId;
                }
                else
                {
                    obj.TimeZoneId = 0;
                }

                obj.MobileNo = objGolfer.MobileNo;
                obj.Status = objGolfer.Active == false ? StatusType.Active : StatusType.InActive;
                var ps = CommonFunctions.DecryptString(obj.Password, obj.Salt);
                if (CommonFunctions.DecryptString(obj.Password, obj.Salt) != objGolfer.Password)
                    obj.Password = CommonFunctions.EncryptString(objGolfer.Password, obj.Salt);

                obj.Address = objGolfer.Address;
                obj.City = objGolfer.City;

                obj.State = objGolfer.State;
                obj.Country = objGolfer.Country;
                obj.ZipCode = objGolfer.ZipCode;

                var objInner = Db.GF_Golfer.FirstOrDefault(x => x.GF_ID != objGolfer.GF_ID && x.Email == objGolfer.Email);
                if (objInner != null)
                {

                    Message = "Email already exists.";
                    return false;
                }


                if (obj.Email != objGolfer.Email)
                {
                    IsUpdateMail = true;
                }
                obj.Email = objGolfer.Email;
                obj.Phone = objGolfer.Phone;
                obj.Gender = objGolfer.Gender;
                obj.IncomeID = objGolfer.IncomeID;


                obj.IsReceiveEmail = objGolfer.ReceiveEmail;
                obj.IsReceivePushNotification = objGolfer.ReceivePushNotification;
                obj.IsReceiveChatMessages = objGolfer.ReceiveChat;

                obj.IsReceiveEmailGolfer = objGolfer.ReceiveEmailGolfer;
                obj.IsReceivePushNotificationGolfer = objGolfer.ReceivePushNotificationGolfer;

                obj.temperature = objGolfer.temperature;
                obj.measurement = objGolfer.measurement;
                obj.speed = objGolfer.speed;

                if (objGolfer.Image == null || objGolfer.Image == "")
                {
                    if (obj.Image != null)
                    {
                        //stay with existing image

                    }
                    else
                    {
                        obj.Image = objGolfer.Image;
                    }
                }
                else
                {
                    obj.Image = objGolfer.Image;
                }
                //obj.DateOfBirth = objGolfer.DateOfBirth;
                if (objGolfer.Birthdate == null)
                {
                    obj.DateOfBirth = objGolfer.DateOfBirth;
                }
                else
                {
                    obj.DateOfBirth = Convert.ToDateTime(objGolfer.Birthdate);
                }
                obj.Race = objGolfer.Race;
                obj.Tee = objGolfer.Tee;

                Db.SaveChanges();
                image = obj.Image;
                if (IsUpdateMail)
                {
                    #region send mail to golfer
                    try
                    {
                        string mailresult = "";
                        string CCMail = "";
                        Int64 courseidForParams = 0;

                        var course = Db.GF_GolferUser.Where(x => x.GolferID == objGolfer.GF_ID);


                        IQueryable<GF_EmailTemplatesFields> templateFields = null;
                        var param = EmailParams.GetEmailParamsNew(ref Db, EmailTemplateName.Registration, ref templateFields, courseidForParams, LoginInfo.LoginType, ref mailresult);

                        if (mailresult == "") // means Parameters are OK
                        {
                            if (ApplicationEmails.EndUserRegistrationMail(obj, param, ref templateFields))
                            {

                                // Do steps for Mail Send successful
                            }
                            //else
                            //{
                            //    // Do steps for Mail Failure. Mail failure reason can be find on "mailresult"
                            //}
                        }
                        else
                        {
                            // Do steps for Parameters not available.Reason can be find on "mailresult"
                        }
                    }
                    catch (Exception ex)
                    {
                        ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
                    }
                    #endregion
                }
                Message = "Golfer info Updated successfully.";
            }
            return true;
        }

        /// <summary>
        /// Created By: Amit Kumar
        /// Created on: 01 April 2015
        /// </summary>
        /// <remarks>Get Menu Items Listing</remarks>
        public IEnumerable<GolferIncome> GetGolferIncome()
        {
            List<GolferIncome> lstIncome = new List<GolferIncome>();
            Db = new GolflerEntities();
            var templstIncome = Db.GF_IncomeSlab.Select(x => new { ID = x.ID, Name = x.Income }).OrderBy(x => x.ID).AsQueryable();
            foreach (var items in templstIncome)
            {
                GolferIncome objGolferIncome = new GolferIncome();
                objGolferIncome.ID = items.ID;
                objGolferIncome.Name = items.Name;
                lstIncome.Add(objGolferIncome);
            }
            return lstIncome;
        }

        /// <summary>
        /// Created By: Amit Kumar
        /// Created on: 23 Sep 2015
        /// </summary>
        /// <param name="OrderID"></param>
        /// <returns></returns>
        public GF_Order GetOrderByID(long OrderID)
        {
            try
            {
                Db = new GolflerEntities();
                var order = Db.GF_Order.FirstOrDefault(x => x.ID == OrderID);
                return order;
            }
            catch
            {
                return new GF_Order();
            }
        }

        #region GolferByCourseId List
        ///// <summary>
        ///// Created By: Veera
        ///// Created Date: 03 April 2015
        ///// Purpose: List of Orders History
        ///// </summary>
        ///// <param name="orderHistory"></param>
        ///// <returns></returns>
        public List<GolfersWithOrders> GetGolferByCourseIdList(string CourseId, string filterExpression, string sortExpression, string sortDirection, int pageIndex, int pageSize, ref int totalRecords)
        {
            try
            {
                List<GolfersWithOrders> lstGolfers = new List<GolfersWithOrders>();
                // List<GF_SP_GetGolfersListingByCourseId_Result>
                GolflerEntities _db = new GolflerEntities();
                Int64 courseid = Convert.ToInt64(CourseId);
                var lstOrders = _db.GF_SP_GetGolfersListingByCourseId(courseid).ToList();
                var templst = lstOrders;

                foreach (var objOrder in lstOrders)
                {
                    GolfersWithOrders obj = new GolfersWithOrders();
                    obj.Id = objOrder.Id;
                    obj.Name = objOrder.Name;
                    obj.Latitude = objOrder.Latitude;
                    obj.Longitude = objOrder.Longitude;
                    obj.type = objOrder.type;
                    obj.Hexa = objOrder.Hexa;
                    obj.RGB = objOrder.RGB;
                    obj.HUE = objOrder.HUE;
                    obj.Admin_Latitude = objOrder.Admin_Latitude;
                    obj.Admin_Name = objOrder.Admin_Name;
                    obj.Admin_Longitude = objOrder.Admin_Longitude;
                    obj.Admin_type = objOrder.Admin_type;

                    var lstOrder = _db.GF_Order.Where(x => x.GolferID == obj.Id && x.CourseID == courseid && (x.IsDelivered ?? false) == false && (x.IsPickup ?? false) == false && (x.IsRejected ?? false) == false && EntityFunctions.TruncateTime(x.OrderDate) == EntityFunctions.TruncateTime(DateTime.UtcNow)).ToList();
                    if (lstOrder.Count > 0)
                    {
                        string strorder = "<div width='100%'>";
                        foreach (var objOrd in lstOrder)
                        {
                            string ordertype = "";
                            if (objOrd.OrderType == OrderType.TurnOrder)
                            {
                                ordertype = OrderType.GetFullOrderType(OrderType.TurnOrder);
                            }
                            else
                            {
                                ordertype = OrderType.GetFullOrderType(OrderType.CartOrder);
                            }


                            long minutes = ((long)DateTime.UtcNow.Subtract(Convert.ToDateTime(objOrd.OrderDate)).TotalMinutes);
                            int totalMinutes = Convert.ToInt32(minutes);
                            TimeSpan spnTotal = TimeSpan.FromMinutes(totalMinutes);

                            int days = spnTotal.Days;
                            int hours = spnTotal.Hours;
                            int totalMins = spnTotal.Minutes;
                            int secs = spnTotal.Seconds;

                            string strTimeElap = "";
                            #region Day
                            if (days > 0)
                            {
                                if (strTimeElap.Length > 0)
                                {
                                    strTimeElap = strTimeElap + "," + days + " Day(s)";
                                }
                                else
                                {
                                    strTimeElap = days + " Day(s)";
                                }
                            }
                            #endregion

                            #region Hours
                            if (hours > 0)
                            {
                                if (strTimeElap.Length > 0)
                                {
                                    strTimeElap = strTimeElap + "," + hours + " Hour(s)";
                                }
                                else
                                {
                                    strTimeElap = hours + " Hour(s)";
                                }
                            }
                            #endregion

                            #region Mins
                            if (strTimeElap.Length > 0)
                            {
                                strTimeElap = strTimeElap + "," + totalMins + " Minute(s)";
                            }
                            else
                            {
                                strTimeElap = totalMins + " Minute(s)";
                            }
                            #endregion

                            #region seconds
                            if (secs > 0)
                            {
                                if (strTimeElap.Length > 0)
                                {
                                    strTimeElap = strTimeElap + "," + secs + " Second(s)";
                                }
                                else
                                {
                                    strTimeElap = secs + " Second(s)";
                                }
                            }
                            #endregion

                            if (minutes > 15)
                            {
                                strorder = strorder + "<div width='33%'>Order Id: " + objOrd.ID + "</div><div width='33%'>Order Type: " + ordertype + "</div><div width='33%'>Time Elapsed: " + strTimeElap + " <img src='/images/blinker.gif' width='6%' /></div><br>";
                            }
                            else
                            {
                                strorder = strorder + "<div width='33%'>Order Id: " + objOrd.ID + "</div><div width='33%'>Order Type: " + ordertype + "</div><div width='33%'>Time Elapsed: " + strTimeElap + "</div><br>";
                            }
                        }
                        strorder = strorder + "</div>";
                        obj.Order_Details = strorder;
                    }
                    else
                    {
                        obj.Order_Details = "<div width='100%'>No Orders</div>";
                    }

                    lstGolfers.Add(obj);
                }

                totalRecords = lstGolfers.Count();
                if (totalRecords > 0)
                {

                    if (pageSize > 0)
                    {
                        return lstGolfers.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
                        //  return lstOrders;
                    }
                    else
                    {
                        return lstGolfers;
                    }
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                return null;

            }
        }


        public IQueryable<GolferPlayingHistory> GetGolphersbyCourseForPlayingHistory(string filterExpression, Int64 CourseId, string HistoryFrom, string HistoryTo, string CompareParameter, string RangeParameter, string strUserType, string strRequestFrom, string sortExpression, string sortDirection, int pageIndex, int pageSize, ref int totalRecords)
        {
            List<GF_GamePlayRound> list = new List<GF_GamePlayRound>();
            List<GolferPlayingHistory> lstGolfers = new List<GolferPlayingHistory>();

            try
            {
                Db = new GolflerEntities();

                #region Compare Parameter
                // compare parameter
                List<Int64> searchInCourseIds = new List<Int64>();
                if (!string.IsNullOrEmpty(Convert.ToString(CompareParameter)))
                {
                    if (CompareParameter == "1") // county
                    {
                        string stCounty = Convert.ToString(Db.GF_CourseInfo.Where(x => x.ID == CourseId).Select(x => x.COUNTY).FirstOrDefault());

                        searchInCourseIds = Db.GF_CourseInfo.Where(x => x.COUNTY == stCounty && x.Status == StatusType.Active).Select(x => x.ID).ToList();
                    }
                    if (CompareParameter == "2") // state
                    {
                        string stState = Convert.ToString(Db.GF_CourseInfo.Where(x => x.ID == CourseId).Select(x => x.STATE).FirstOrDefault());

                        searchInCourseIds = Db.GF_CourseInfo.Where(x => x.STATE == stState && x.Status == StatusType.Active).Select(x => x.ID).ToList();
                    }
                    if (CompareParameter == "3") // ctiy
                    {
                        string stCity = Convert.ToString(Db.GF_CourseInfo.Where(x => x.ID == CourseId).Select(x => x.CITY).FirstOrDefault());

                        searchInCourseIds = Db.GF_CourseInfo.Where(x => x.CITY == stCity && x.Status == StatusType.Active).Select(x => x.ID).ToList();
                    }
                    if (CompareParameter == "4") // zip
                    {
                        string miles = "0";
                        if (!string.IsNullOrEmpty(Convert.ToString(RangeParameter)))
                        {
                            try
                            {
                                miles = Convert.ToString(RangeParameter);
                            }
                            catch
                            {
                                miles = "0";
                            }
                        }
                        var lstCourses = Db.GF_SP_GetCoursesByRadius(Convert.ToInt64(CourseId), miles).ToList();
                        searchInCourseIds = lstCourses.Select(x => x.ID).ToList();
                    }
                }
                else
                {
                    searchInCourseIds.Add(CourseId);
                }
                #endregion

                #region  Get Golfer Users
                //// Course Search Filter
                var listEntity = (from user in Db.GF_Golfer
                                  join gameplay in Db.GF_GamePlayRound on user.GF_ID equals gameplay.GolferID
                                  where searchInCourseIds.Contains(gameplay.CourseID ?? 0) && user.Status == StatusType.Active && gameplay.IsQuit == true
                                  select gameplay);


                #region History dates
                List<Int64> idsRemove = new List<Int64>();
                // var listHistory = list.AsQueryable();
                if (!string.IsNullOrEmpty(HistoryFrom) && !string.IsNullOrEmpty(HistoryTo))
                {
                    string courseTimeZone = CommonFunctions.GetCourseTimeZone(Convert.ToInt64(CourseId));

                    DateTime dtDate = DateTime.Parse(HistoryFrom);
                    DateTime dtToDate = DateTime.Parse(HistoryTo);

                    foreach (var obj in listEntity)
                    {
                        DateTime objDate = CommonFunctions.DateByTimeZone(courseTimeZone, Convert.ToDateTime(obj.CreatedDate));
                        if (!((objDate.Date >= dtDate.Date) && (objDate.Date <= dtToDate.Date)))
                        {
                            idsRemove.Add(obj.ID);
                        }
                    }

                    //listEntity = listEntity.Where(x => EntityFunctions.TruncateTime(CommonFunctions.DateByTimeZone(courseTimeZone,Convert.ToDateTime(x.CreatedDate))) >= EntityFunctions.TruncateTime(dtDate)
                    //      && EntityFunctions.TruncateTime(CommonFunctions.DateByTimeZone(courseTimeZone, Convert.ToDateTime(x.CreatedDate))) <= EntityFunctions.TruncateTime(dtToDate));

                    //listHistory = listHistory.Where(x => x.CreatedDate.Value.Year >= dtDate.Year
                    //    && x.CreatedDate.Value.Month >= dtDate.Month
                    //    && x.CreatedDate.Value.Day >= dtDate.Day

                    //&& x.CreatedDate.Value.Year <= dtToDate.Year
                    //    && x.CreatedDate.Value.Month <= dtToDate.Month
                    //    && x.CreatedDate.Value.Day <= dtToDate.Day);
                }
                else if (!string.IsNullOrEmpty(HistoryFrom))
                {
                    string courseTimeZone = CommonFunctions.GetCourseTimeZone(Convert.ToInt64(CourseId));

                    DateTime dtDate = DateTime.Parse(HistoryFrom);

                    foreach (var obj in listEntity)
                    {
                        DateTime objDate = CommonFunctions.DateByTimeZone(courseTimeZone, Convert.ToDateTime(obj.CreatedDate));
                        if (!(objDate.Date >= dtDate.Date))
                        {
                            idsRemove.Add(obj.ID);
                        }
                    }

                    // listEntity = listEntity.Where(x => EntityFunctions.TruncateTime(CommonFunctions.DateByTimeZone(courseTimeZone, Convert.ToDateTime(x.CreatedDate))) >= EntityFunctions.TruncateTime(dtDate));

                    //listHistory = listHistory.Where(x => x.CreatedDate.Value.Year >= dtDate.Year
                    //    && x.CreatedDate.Value.Month >= dtDate.Month
                    //    && x.CreatedDate.Value.Day >= dtDate.Day
                    //    );
                }
                else if (!string.IsNullOrEmpty(HistoryTo))
                {
                    string courseTimeZone = CommonFunctions.GetCourseTimeZone(Convert.ToInt64(CourseId));

                    DateTime dtToDate = DateTime.Parse(HistoryTo);


                    foreach (var obj in listEntity)
                    {
                        DateTime objDate = CommonFunctions.DateByTimeZone(courseTimeZone, Convert.ToDateTime(obj.CreatedDate));
                        if (!((objDate.Date <= dtToDate.Date)))
                        {
                            idsRemove.Add(obj.ID);
                        }
                    }

                    // listEntity = listEntity.Where(x => EntityFunctions.TruncateTime(CommonFunctions.DateByTimeZone(courseTimeZone, Convert.ToDateTime(x.CreatedDate))) <= EntityFunctions.TruncateTime(dtToDate));


                    //listHistory = listHistory.Where(x => x.CreatedDate.Value.Year <= dtToDate.Year
                    //    && x.CreatedDate.Value.Month <= dtToDate.Month
                    //    && x.CreatedDate.Value.Day <= dtToDate.Day
                    //    );
                }
                #endregion


                if (idsRemove.Count > 0)
                {
                    foreach (var obj in listEntity)
                    {
                        if (!(idsRemove.Contains(obj.ID)))
                        {
                            list.Add(obj); // = listEntity.Where(x => idsRemove.Contains(x.ID)).ToList();
                        }
                    }
                }
                else
                {
                    list = listEntity.ToList();
                }

                //  list = list.GroupBy(x => x.CourseID).Select(x => x.First()).ToList();

                #endregion

                list = list.GroupBy(x => x.GolferID).Select(x => x.First()).ToList();


                // string courseName = CommonFunctions.GetCourseNameById(CourseId);
                foreach (var objGolfer in list)
                {
                    var cntExistingRecords = lstGolfers.Where(x => x.GolpherId == objGolfer.GolferID).ToList();  //  && x.CourseId == objGolfer.CourseID
                    if (cntExistingRecords.Count <= 0)
                    {
                        var objGolf = new GolferPlayingHistory();
                        objGolf.GolpherId = Convert.ToInt64(objGolfer.GolferID);
                        objGolf.CourseId = Convert.ToInt64(objGolfer.CourseID);
                        objGolf.GolpherName = Db.GF_Golfer.FirstOrDefault(x => x.GF_ID == objGolfer.GolferID).FirstName + " " + Db.GF_Golfer.FirstOrDefault(x => x.GF_ID == objGolfer.GolferID).LastName;
                        objGolf.Email = Db.GF_Golfer.FirstOrDefault(x => x.GF_ID == objGolfer.GolferID).Email;
                        objGolf.CourseName = CommonFunctions.GetCourseNameById(Convert.ToInt64(objGolfer.CourseID));
                        objGolf.DeviceType = objGolfer.GF_Golfer == null ? "" : objGolfer.GF_Golfer.DeviceType ?? "";
                        lstGolfers.Add(objGolf);
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
            }

            #region Name Filter
            // Name/Email Search Filter
            if (!String.IsNullOrWhiteSpace(filterExpression))
            {
                lstGolfers = lstGolfers.Where(x => x.Email.ToLower().Contains(filterExpression.ToLower()) || ((x.GolpherName).ToLower().Contains(filterExpression.ToLower()))).ToList();
            }
            #endregion

            IQueryable<GolferPlayingHistory> qryList = lstGolfers.AsQueryable();


            totalRecords = qryList.Count();

            return qryList.OrderBy(sortExpression + " " + sortDirection).Skip((pageIndex - 1) * pageSize).Take(pageSize);
        }

        public IQueryable<GolferPlayingHistory> GetGolpherPlayingHistory(string filterExpression, string HistoryFrom, string HistoryTo, Int64 CourseId, Int64 golferid, string strRequestFrom, string sortExpression, string sortDirection, int pageIndex, int pageSize, ref int totalRecords, bool forExcel = false)
        {
            List<GF_Golfer> list = new List<GF_Golfer>();
            List<GolferPlayingHistory> lstGolfers = new List<GolferPlayingHistory>();

            try
            {
                Db = new GolflerEntities();
                #region  Get Game Rounds
                var lstRoundsEntity = Db.GF_GamePlayRound.Where(x => x.IsQuit == true && x.GolferID == golferid);
                string courseTimeZone = CommonFunctions.GetCourseTimeZone(Convert.ToInt64(CourseId));

                #region History dates
                List<Int64> idsRemove = new List<Int64>();
                // var listHistory = list.AsQueryable();
                if (!string.IsNullOrEmpty(HistoryFrom) && !string.IsNullOrEmpty(HistoryTo))
                {

                    DateTime dtDate = DateTime.Parse(HistoryFrom);
                    DateTime dtToDate = DateTime.Parse(HistoryTo);

                    foreach (var obj in lstRoundsEntity)
                    {
                        DateTime objDate = CommonFunctions.DateByTimeZone(courseTimeZone, Convert.ToDateTime(obj.CreatedDate));
                        if (!((objDate.Date >= dtDate.Date) && (objDate.Date <= dtToDate.Date)))
                        {
                            idsRemove.Add(obj.ID);
                        }
                    }

                    //lstRoundsEntity = lstRoundsEntity.Where(x => EntityFunctions.TruncateTime(CommonFunctions.DateByTimeZone(courseTimeZone, Convert.ToDateTime(x.CreatedDate))) >= EntityFunctions.TruncateTime(dtDate)
                    //      && EntityFunctions.TruncateTime(CommonFunctions.DateByTimeZone(courseTimeZone, Convert.ToDateTime(x.CreatedDate))) <= EntityFunctions.TruncateTime(dtToDate));

                    //listHistory = listHistory.Where(x => x.CreatedDate.Value.Year >= dtDate.Year
                    //    && x.CreatedDate.Value.Month >= dtDate.Month
                    //    && x.CreatedDate.Value.Day >= dtDate.Day

                    //&& x.CreatedDate.Value.Year <= dtToDate.Year
                    //    && x.CreatedDate.Value.Month <= dtToDate.Month
                    //    && x.CreatedDate.Value.Day <= dtToDate.Day);
                }
                else if (!string.IsNullOrEmpty(HistoryFrom))
                {
                    DateTime dtDate = DateTime.Parse(HistoryFrom);

                    foreach (var obj in lstRoundsEntity)
                    {
                        DateTime objDate = CommonFunctions.DateByTimeZone(courseTimeZone, Convert.ToDateTime(obj.CreatedDate));
                        if (!(objDate.Date >= dtDate.Date))
                        {
                            idsRemove.Add(obj.ID);
                        }
                    }

                    // lstRoundsEntity = lstRoundsEntity.Where(x => EntityFunctions.TruncateTime(CommonFunctions.DateByTimeZone(courseTimeZone, Convert.ToDateTime(x.CreatedDate))) >= EntityFunctions.TruncateTime(dtDate));

                    //listHistory = listHistory.Where(x => x.CreatedDate.Value.Year >= dtDate.Year
                    //    && x.CreatedDate.Value.Month >= dtDate.Month
                    //    && x.CreatedDate.Value.Day >= dtDate.Day
                    //    );
                }
                else if (!string.IsNullOrEmpty(HistoryTo))
                {

                    DateTime dtToDate = DateTime.Parse(HistoryTo);

                    foreach (var obj in lstRoundsEntity)
                    {
                        DateTime objDate = CommonFunctions.DateByTimeZone(courseTimeZone, Convert.ToDateTime(obj.CreatedDate));
                        if (!(objDate.Date <= dtToDate.Date))
                        {
                            idsRemove.Add(obj.ID);
                        }
                    }

                    //  lstRoundsEntity = lstRoundsEntity.Where(x => EntityFunctions.TruncateTime(CommonFunctions.DateByTimeZone(courseTimeZone, Convert.ToDateTime(x.CreatedDate))) <= EntityFunctions.TruncateTime(dtToDate));


                    //listHistory = listHistory.Where(x => x.CreatedDate.Value.Year <= dtToDate.Year
                    //    && x.CreatedDate.Value.Month <= dtToDate.Month
                    //    && x.CreatedDate.Value.Day <= dtToDate.Day
                    //    );
                }
                #endregion

                var lstRounds = new List<GF_GamePlayRound>(); // lstRoundsEntity.ToList();
                if (idsRemove.Count > 0)
                {
                    foreach (var obj in lstRoundsEntity)
                    {
                        if (!(idsRemove.Contains(obj.ID)))
                        {
                            lstRounds.Add(obj); // = listEntity.Where(x => idsRemove.Contains(x.ID)).ToList();
                        }
                    }
                }
                else
                {
                    lstRounds = lstRoundsEntity.ToList();
                }



                var RoundByCourse = lstRounds.GroupBy(x => x.CourseID).Select(x => x.First()).ToList();

                List<Int64> validRoundForCalculations = new List<Int64>();
                validRoundForCalculations = lstRounds.Select(x => x.ID).ToList();

                foreach (var objCourse in RoundByCourse)
                {
                    var objGolf = new GolferPlayingHistory();
                    objGolf.GolpherId = Convert.ToInt64(objCourse.GolferID);
                    objGolf.CourseId = Convert.ToInt64(objCourse.CourseID);
                    objGolf.CourseName = Db.GF_CourseInfo.FirstOrDefault(x => x.ID == objCourse.CourseID).COURSE_NAME;

                    objGolf.TotalPlay = lstRounds.Where(x => x.CourseID == objCourse.CourseID).ToList().Count;
                    objGolf.OneHoleAvgTime = CommonFunctions.GetAvgTimePlay(Convert.ToInt64(objCourse.ID), Convert.ToInt64(objCourse.CourseID), Convert.ToInt64(objCourse.GolferID), 1, validRoundForCalculations);
                    objGolf.NineHoleAvgTime = CommonFunctions.GetAvgTimePlay(Convert.ToInt64(objCourse.ID), Convert.ToInt64(objCourse.CourseID), Convert.ToInt64(objCourse.GolferID), 9, validRoundForCalculations);
                    objGolf.EighteenHoleAvgTime = CommonFunctions.GetAvgTimePlay(Convert.ToInt64(objCourse.ID), Convert.ToInt64(objCourse.CourseID), Convert.ToInt64(objCourse.GolferID), 18, validRoundForCalculations);
                    objGolf.LastGameDate = CommonFunctions.DateByTimeZone(courseTimeZone, Convert.ToDateTime(lstRounds.Where(x => x.CourseID == objCourse.CourseID).OrderByDescending(x => x.CreatedDate).FirstOrDefault().CreatedDate));
                    objGolf.strLastGameDate = Convert.ToString(Convert.ToDateTime(objGolf.LastGameDate).ToShortDateString());
                    //objGolf.GolpherName = Convert.ToString(Db.GF_Golfer.Where(x => x.GF_ID == objCourse.GolferID).Select(x => x.FirstName + " " + LastName).FirstOrDefault());
                    objGolf.GolpherName = Db.GF_Golfer.FirstOrDefault(x => x.GF_ID == objCourse.GolferID).FirstName + " " + Db.GF_Golfer.FirstOrDefault(x => x.GF_ID == objCourse.GolferID).LastName;
                    lstGolfers.Add(objGolf);
                }

                #endregion


            }
            catch (Exception ex)
            {
                ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
            }
            IQueryable<GolferPlayingHistory> qryList = lstGolfers.AsQueryable();
            qryList = qryList.OrderByDescending(x => x.LastGameDate);
            totalRecords = qryList.Count();

            if (forExcel)
            {
                return qryList;
            }
            else
            {
                return qryList.OrderBy(sortExpression + " " + sortDirection).Skip((pageIndex - 1) * pageSize).Take(pageSize);
            }
        }

        public IQueryable<GolferPlayingHistory> GetGolpherPlayingHistoryForCourse(string filterExpression, string HistoryFrom, string HistoryTo, Int64 CourseId, Int64 golferid, string strRequestFrom, string sortExpression, string sortDirection, int pageIndex, int pageSize, ref int totalRecords, bool forExcel = false)
        {
            List<GF_Golfer> list = new List<GF_Golfer>();
            List<GolferPlayingHistory> lstGolfers = new List<GolferPlayingHistory>();

            try
            {
                Db = new GolflerEntities();
                #region  Get Game Rounds
                string courseTimeZone = CommonFunctions.GetCourseTimeZone(Convert.ToInt64(CourseId));
                var lstRoundsEntity = Db.GF_GamePlayRound.Where(x => x.IsQuit == true && x.GolferID == golferid && x.CourseID == CourseId);

                #region History dates
                List<Int64> idsRemove = new List<Int64>();
                // var listHistory = list.AsQueryable();
                if (!string.IsNullOrEmpty(HistoryFrom) && !string.IsNullOrEmpty(HistoryTo))
                {
                    DateTime dtDate = DateTime.Parse(HistoryFrom);
                    DateTime dtToDate = DateTime.Parse(HistoryTo);


                    foreach (var obj in lstRoundsEntity)
                    {
                        DateTime objDate = CommonFunctions.DateByTimeZone(courseTimeZone, Convert.ToDateTime(obj.CreatedDate));
                        if (!((objDate.Date >= dtDate.Date) && (objDate.Date <= dtToDate.Date)))
                        {
                            idsRemove.Add(obj.ID);
                        }
                    }

                    //lstRoundsEntity = lstRoundsEntity.Where(x => EntityFunctions.TruncateTime(CommonFunctions.DateByTimeZone(courseTimeZone, Convert.ToDateTime(x.CreatedDate))) >= EntityFunctions.TruncateTime(dtDate)
                    //      && EntityFunctions.TruncateTime(CommonFunctions.DateByTimeZone(courseTimeZone, Convert.ToDateTime(x.CreatedDate))) <= EntityFunctions.TruncateTime(dtToDate));

                    //listHistory = listHistory.Where(x => x.CreatedDate.Value.Year >= dtDate.Year
                    //    && x.CreatedDate.Value.Month >= dtDate.Month
                    //    && x.CreatedDate.Value.Day >= dtDate.Day

                    //&& x.CreatedDate.Value.Year <= dtToDate.Year
                    //    && x.CreatedDate.Value.Month <= dtToDate.Month
                    //    && x.CreatedDate.Value.Day <= dtToDate.Day);
                }
                else if (!string.IsNullOrEmpty(HistoryFrom))
                {
                    DateTime dtDate = DateTime.Parse(HistoryFrom);

                    foreach (var obj in lstRoundsEntity)
                    {
                        DateTime objDate = CommonFunctions.DateByTimeZone(courseTimeZone, Convert.ToDateTime(obj.CreatedDate));
                        if (!(objDate.Date >= dtDate.Date))
                        {
                            idsRemove.Add(obj.ID);
                        }
                    }

                    //lstRoundsEntity = lstRoundsEntity.Where(x => EntityFunctions.TruncateTime(CommonFunctions.DateByTimeZone(courseTimeZone, Convert.ToDateTime(x.CreatedDate))) >= EntityFunctions.TruncateTime(dtDate));

                    //listHistory = listHistory.Where(x => x.CreatedDate.Value.Year >= dtDate.Year
                    //    && x.CreatedDate.Value.Month >= dtDate.Month
                    //    && x.CreatedDate.Value.Day >= dtDate.Day
                    //    );
                }
                else if (!string.IsNullOrEmpty(HistoryTo))
                {
                    DateTime dtToDate = DateTime.Parse(HistoryTo);

                    foreach (var obj in lstRoundsEntity)
                    {
                        DateTime objDate = CommonFunctions.DateByTimeZone(courseTimeZone, Convert.ToDateTime(obj.CreatedDate));
                        if (!(objDate.Date <= dtToDate.Date))
                        {
                            idsRemove.Add(obj.ID);
                        }
                    }

                    //lstRoundsEntity = lstRoundsEntity.Where(x => EntityFunctions.TruncateTime(CommonFunctions.DateByTimeZone(courseTimeZone, Convert.ToDateTime(x.CreatedDate))) <= EntityFunctions.TruncateTime(dtToDate));


                    //listHistory = listHistory.Where(x => x.CreatedDate.Value.Year <= dtToDate.Year
                    //    && x.CreatedDate.Value.Month <= dtToDate.Month
                    //    && x.CreatedDate.Value.Day <= dtToDate.Day
                    //    );
                }
                #endregion

                var lstRounds = new List<GF_GamePlayRound>(); // lstRoundsEntity.ToList();
                if (idsRemove.Count > 0)
                {
                    foreach (var obj in lstRoundsEntity)
                    {
                        if (!(idsRemove.Contains(obj.ID)))
                        {
                            lstRounds.Add(obj); // = listEntity.Where(x => idsRemove.Contains(x.ID)).ToList();
                        }
                    }
                }
                else
                {
                    lstRounds = lstRoundsEntity.ToList();
                }


                // var lstRounds = lstRoundsEntity.ToList();

                // var RoundByCourse = lstRounds.GroupBy(x => x.CourseID).Select(x => x.First()).ToList();

                Int32 GameCount = 1;
                List<Int64> validRoundForCalculations = new List<Int64>();
                foreach (var objRound in lstRounds)
                {
                    var objGolf = new GolferPlayingHistory();
                    objGolf.GameName = "Game " + GameCount;
                    objGolf.GameRoundId = Convert.ToString(objRound.ID);
                    objGolf.TotalTimePlay = CommonFunctions.GetAvgTimePlay(Convert.ToInt64(objRound.ID), Convert.ToInt64(objRound.CourseID), Convert.ToInt64(objRound.GolferID), 0, validRoundForCalculations);
                    objGolf.TotalHoles = CommonFunctions.GetAvgTimePlay(Convert.ToInt64(objRound.ID), Convert.ToInt64(objRound.CourseID), Convert.ToInt64(objRound.GolferID), 2, validRoundForCalculations);
                    objGolf.OneHoleAvgTime = CommonFunctions.GetAvgTimePlay(Convert.ToInt64(objRound.ID), Convert.ToInt64(objRound.CourseID), Convert.ToInt64(objRound.GolferID), 11, validRoundForCalculations);
                    objGolf.NineHoleAvgTime = CommonFunctions.GetAvgTimePlay(Convert.ToInt64(objRound.ID), Convert.ToInt64(objRound.CourseID), Convert.ToInt64(objRound.GolferID), 99, validRoundForCalculations);
                    objGolf.EighteenHoleAvgTime = CommonFunctions.GetAvgTimePlay(Convert.ToInt64(objRound.ID), Convert.ToInt64(objRound.CourseID), Convert.ToInt64(objRound.GolferID), 188, validRoundForCalculations);
                    objGolf.LastGameDate = CommonFunctions.DateByTimeZone(courseTimeZone, Convert.ToDateTime(objRound.CreatedDate));
                    objGolf.strLastGameDate = Convert.ToString(Convert.ToDateTime(objGolf.LastGameDate).ToShortDateString()); // CommonFunctions.DateByTimeZone(courseTimeZone, Convert.ToDateTime(objRound.CreatedDate));
                    objGolf.CourseId = Convert.ToInt64(objRound.CourseID);

                    //objGolf.GolpherId = Convert.ToInt64(objCourse.GolferID); 
                    //objGolf.CourseName = Db.GF_CourseInfo.FirstOrDefault(x => x.ID == objCourse.CourseID).COURSE_NAME;

                    //objGolf.TotalPlay = lstRounds.Where(x => x.CourseID == objCourse.CourseID).ToList().Count;
                    //objGolf.GolpherName = Convert.ToString(Db.GF_Golfer.Where(x => x.GF_ID == objCourse.GolferID).Select(x => x.FirstName + " " + LastName).FirstOrDefault());
                    //objGolf.GolpherName = Db.GF_Golfer.FirstOrDefault(x => x.GF_ID == objCourse.GolferID).FirstName + " " + Db.GF_Golfer.FirstOrDefault(x => x.GF_ID == objCourse.GolferID).LastName;
                    lstGolfers.Add(objGolf);

                    GameCount = GameCount + 1;
                }

                #endregion


            }
            catch (Exception ex)
            {
                ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
            }
            IQueryable<GolferPlayingHistory> qryList = lstGolfers.AsQueryable();
            qryList = qryList.OrderByDescending(x => x.LastGameDate);
            totalRecords = qryList.Count();

            if (forExcel)
            {
                return qryList;
            }
            else
            {
                return qryList.OrderBy(sortExpression + " " + sortDirection).Skip((pageIndex - 1) * pageSize).Take(pageSize);
            }
        }

        #endregion

        #region GetOnlineGolferListByCourseId  List
        ///// <summary>
        ///// Created By: Veera
        ///// Created Date: 22 April 2015
        ///// Purpose: GetOnlineGolferListByCourseId
        ///// </summary>

        ///// <returns></returns>
        public List<OnLineUsers> GetOnlineGolferListByCourseIdList(string courseId, long UserId, string UserType, string type, string status, string filterExpression, string sortExpression, string sortDirection, int pageIndex, int pageSize, ref int totalRecords)
        {
            OnLineUsers objOnLineUsers = new OnLineUsers();

            try
            {
                GolflerEntities _db = new GolflerEntities();
                //long _courseId = Convert.ToInt64(courseId);
                long _courseId;
                bool isCourseID = long.TryParse(courseId, out _courseId);
                string courseName = "";
                if (_courseId > 0)
                {
                    courseName = _db.GF_CourseInfo.FirstOrDefault(x => x.ID == _courseId).COURSE_NAME;
                }

                type = _courseId < 0 ? "SC" : type;

                DateTime TodaysDate = Convert.ToDateTime(DateTime.UtcNow).Date;
                if (type == "NU")
                {
                    #region Golfer User

                    try
                    {
                        if (_courseId == 0)//means Golfer is looking for other golfers
                        {
                            _courseId = Convert.ToInt64(_db.GF_GolferUser.FirstOrDefault(x => x.GolferID == UserId).CourseID);

                            var values = _db.GF_GolferUser.Where(x => x.CourseID == _courseId && x.GolferID != UserId).Select(x => x.GolferID).ToList();
                            string GolferDetails = string.Join(",", values.Select(v => v.ToString()).ToArray());
                            long[] GolferIDs = CommonFunctions.ConvertStringArrayToLongArray(GolferDetails);
                            courseName = _db.GF_CourseInfo.FirstOrDefault(x => x.ID == _courseId).COURSE_NAME;

                            //ids who blocked current logged in user
                            var valuesBlocked = _db.GF_BlockUserList.Where(x => x.BlockedUserId == UserId && x.IsBlockedGolferUser == true && x.IsGolferUser == true).Select(x => x.UserId).ToList();
                            string GolferBlockedDetails = string.Join(",", valuesBlocked.Select(v => v.ToString()).ToArray());

                            if (GolferBlockedDetails != "")
                            {
                                long[] GolferBlockedIDs = CommonFunctions.ConvertStringArrayToLongArray(GolferBlockedDetails);
                                #region query
                                var query = from golfer in _db.GF_Golfer
                                            where GolferIDs.Contains(golfer.GF_ID)
                                            && golfer.Status == "A"
                                            && (!(GolferBlockedIDs.Contains(golfer.GF_ID)))
                                            select new OnLineUsers()
                                            {
                                                Id = golfer.GF_ID,
                                                Name = golfer.FirstName + " " + golfer.LastName,
                                                EmailAddress = golfer.Email,
                                                IsOnline = golfer.IsOnline,
                                                Type = "NU",
                                                DateOfBirth = golfer.DateOfBirth,
                                                Gender = golfer.Gender,
                                                Phone = golfer.MobileNo,
                                                GolferCourse = courseName,
                                                IsReceiveChat = golfer.IsReceiveChatMessages,
                                                IsBlocked = (from golferBlockedList in _db.GF_BlockUserList
                                                             where golferBlockedList.UserId == UserId
                                                                      && golferBlockedList.BlockedUserId == golfer.GF_ID
                                                                      && golferBlockedList.IsGolferUser == true && golferBlockedList.IsBlockedGolferUser == true
                                                             select (golferBlockedList.ID)).Count(),

                                                TodaysMsgsCount = (from golferMsgs in _db.GF_Messages
                                                                   where golferMsgs.MsgFrom == golfer.GF_ID
                                                                    && golferMsgs.MsgTo == UserId
                                                                       //&& golferMsgs.CreatedDate.Value.Date==DateTime.UtcNow.Date
                                                                    && EntityFunctions.TruncateTime(golferMsgs.CreatedDate) == TodaysDate
                                                                    && golferMsgs.IsMessagesFromGolfer == "1"
                                                                    && golferMsgs.IsMessagesToGolfer == UserType
                                                                   select (golferMsgs.ID)).Count()
                                            };
                                if (filterExpression != "")
                                {

                                    query = from golfer in _db.GF_Golfer
                                            where ((golfer.FirstName.Contains(filterExpression) || golfer.Email.Contains(filterExpression))
                                            && GolferIDs.Contains(golfer.GF_ID)
                                            && golfer.Status == "A"
                                            && (!(GolferBlockedIDs.Contains(golfer.GF_ID)))
                                            )
                                            select new OnLineUsers()
                                            {
                                                Id = golfer.GF_ID,
                                                Name = golfer.FirstName + " " + golfer.LastName,
                                                EmailAddress = golfer.Email,
                                                IsOnline = golfer.IsOnline,
                                                Type = "NU",
                                                DateOfBirth = golfer.DateOfBirth,
                                                Gender = golfer.Gender,
                                                Phone = golfer.MobileNo,
                                                GolferCourse = courseName,
                                                IsReceiveChat = golfer.IsReceiveChatMessages,
                                                IsBlocked = (from golferBlockedList in _db.GF_BlockUserList
                                                             where golferBlockedList.UserId == UserId
                                                                      && golferBlockedList.BlockedUserId == golfer.GF_ID
                                                                      && golferBlockedList.IsGolferUser == true && golferBlockedList.IsBlockedGolferUser == true
                                                             select (golferBlockedList.ID)).Count(),
                                                TodaysMsgsCount = (from golferMsgs in _db.GF_Messages
                                                                   where golferMsgs.MsgFrom == golfer.GF_ID
                                                                    && golferMsgs.MsgTo == UserId
                                                                     && EntityFunctions.TruncateTime(golferMsgs.CreatedDate) == TodaysDate
                                                                    && golferMsgs.IsMessagesFromGolfer == "1"
                                                                    && golferMsgs.IsMessagesToGolfer == UserType
                                                                   select (golferMsgs.ID)).Count()
                                            };
                                }
                                if (status == "1")
                                {
                                    query = query.Where(x => x.IsOnline == true);
                                }
                                else if (status == "0")
                                {
                                    query = query.Where(x => x.IsOnline == false);
                                }
                                else
                                { }

                                totalRecords = query.Count();
                                IQueryable<OnLineUsers> lst = query.AsQueryable();

                                return lst.OrderBy(sortExpression + " " + sortDirection).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
                                #endregion
                            }
                            else
                            {
                                #region query
                                var query = from golfer in _db.GF_Golfer
                                            where GolferIDs.Contains(golfer.GF_ID)
                                            && golfer.Status == "A"
                                            select new OnLineUsers()
                                            {
                                                Id = golfer.GF_ID,
                                                Name = golfer.FirstName + " " + golfer.LastName,
                                                EmailAddress = golfer.Email,
                                                IsOnline = golfer.IsOnline,
                                                Type = "NU",
                                                DateOfBirth = golfer.DateOfBirth,
                                                Gender = golfer.Gender,
                                                Phone = golfer.MobileNo,
                                                GolferCourse = courseName,
                                                IsReceiveChat = golfer.IsReceiveChatMessages,
                                                IsBlocked = (from golferBlockedList in _db.GF_BlockUserList
                                                             where golferBlockedList.UserId == UserId
                                                                      && golferBlockedList.BlockedUserId == golfer.GF_ID
                                                                      && golferBlockedList.IsGolferUser == true && golferBlockedList.IsBlockedGolferUser == true
                                                             select (golferBlockedList.ID)).Count(),

                                                TodaysMsgsCount = (from golferMsgs in _db.GF_Messages
                                                                   where golferMsgs.MsgFrom == golfer.GF_ID
                                                                    && golferMsgs.MsgTo == UserId
                                                                       //&& golferMsgs.CreatedDate.Value.Date==DateTime.UtcNow.Date
                                                                    && EntityFunctions.TruncateTime(golferMsgs.CreatedDate) == TodaysDate
                                                                    && golferMsgs.IsMessagesFromGolfer == "1"
                                                                    && golferMsgs.IsMessagesToGolfer == UserType
                                                                   select (golferMsgs.ID)).Count()
                                            };
                                if (filterExpression != "")
                                {

                                    query = from golfer in _db.GF_Golfer
                                            where ((golfer.FirstName.Contains(filterExpression) || golfer.Email.Contains(filterExpression))
                                            && GolferIDs.Contains(golfer.GF_ID)
                                            && golfer.Status == "A"
                                            )
                                            select new OnLineUsers()
                                            {
                                                Id = golfer.GF_ID,
                                                Name = golfer.FirstName + " " + golfer.LastName,
                                                EmailAddress = golfer.Email,
                                                IsOnline = golfer.IsOnline,
                                                Type = "NU",
                                                DateOfBirth = golfer.DateOfBirth,
                                                Gender = golfer.Gender,
                                                Phone = golfer.MobileNo,
                                                GolferCourse = courseName,
                                                IsReceiveChat = golfer.IsReceiveChatMessages,
                                                IsBlocked = (from golferBlockedList in _db.GF_BlockUserList
                                                             where golferBlockedList.UserId == UserId
                                                                      && golferBlockedList.BlockedUserId == golfer.GF_ID
                                                                      && golferBlockedList.IsGolferUser == true && golferBlockedList.IsBlockedGolferUser == true
                                                             select (golferBlockedList.ID)).Count(),
                                                TodaysMsgsCount = (from golferMsgs in _db.GF_Messages
                                                                   where golferMsgs.MsgFrom == golfer.GF_ID
                                                                    && golferMsgs.MsgTo == UserId
                                                                     && EntityFunctions.TruncateTime(golferMsgs.CreatedDate) == TodaysDate
                                                                    && golferMsgs.IsMessagesFromGolfer == "1"
                                                                    && golferMsgs.IsMessagesToGolfer == UserType
                                                                   select (golferMsgs.ID)).Count()
                                            };
                                }
                                if (status == "1")
                                {
                                    query = query.Where(x => x.IsOnline == true);
                                }
                                else if (status == "0")
                                {
                                    query = query.Where(x => x.IsOnline == false);
                                }
                                else
                                { }

                                totalRecords = query.Count();
                                IQueryable<OnLineUsers> lst = query.AsQueryable();

                                return lst.OrderBy(sortExpression + " " + sortDirection).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
                                #endregion
                            }
                            //

                        }
                        else
                        {
                            var values = _db.GF_GolferUser.Where(x => x.CourseID == _courseId).Select(x => x.GolferID).ToList();
                            string GolferDetails = string.Join(",", values.Select(v => v.ToString()).ToArray());
                            long[] GolferIDs = CommonFunctions.ConvertStringArrayToLongArray(GolferDetails);
                            courseName = _db.GF_CourseInfo.FirstOrDefault(x => x.ID == _courseId).COURSE_NAME;

                            var query = from golfer in _db.GF_Golfer
                                        where GolferIDs.Contains(golfer.GF_ID)
                                        && golfer.Status == "A"
                                        select new OnLineUsers()
                                        {
                                            Id = golfer.GF_ID,
                                            Name = golfer.FirstName + " " + golfer.LastName,
                                            EmailAddress = golfer.Email,
                                            IsOnline = golfer.IsOnline,
                                            Type = "NU",
                                            DateOfBirth = golfer.DateOfBirth,
                                            Gender = golfer.Gender,
                                            Phone = golfer.MobileNo,
                                            GolferCourse = courseName,
                                            IsReceiveChat = golfer.IsReceiveChatMessages,

                                            IsBlocked = (from courseBlockedList in _db.GF_CourseBlockUserList
                                                         where courseBlockedList.CourseAdminId == UserId
                                                                  && courseBlockedList.BlockedUserId == golfer.GF_ID
                                                                  && courseBlockedList.IsBlockedGolfer == true
                                                         select (courseBlockedList.ID)).Count(),

                                            TodaysMsgsCount = (from golferMsgs in _db.GF_Messages
                                                               where golferMsgs.MsgFrom == golfer.GF_ID
                                                                && golferMsgs.MsgTo == UserId
                                                                   //&& golferMsgs.CreatedDate.Value.Date==DateTime.UtcNow.Date
                                                                && EntityFunctions.TruncateTime(golferMsgs.CreatedDate) == TodaysDate
                                                                && golferMsgs.IsMessagesFromGolfer == "1"
                                                                && golferMsgs.IsMessagesToGolfer == UserType
                                                               select (golferMsgs.ID)).Count()
                                        };
                            if (filterExpression != "")
                            {

                                query = from golfer in _db.GF_Golfer
                                        where ((golfer.FirstName.Contains(filterExpression) || golfer.Email.Contains(filterExpression))
                                        && GolferIDs.Contains(golfer.GF_ID)
                                        && golfer.Status == "A"
                                        )
                                        select new OnLineUsers()
                                        {
                                            Id = golfer.GF_ID,
                                            Name = golfer.FirstName + " " + golfer.LastName,
                                            EmailAddress = golfer.Email,
                                            IsOnline = golfer.IsOnline,
                                            Type = "NU",
                                            DateOfBirth = golfer.DateOfBirth,
                                            Gender = golfer.Gender,
                                            Phone = golfer.MobileNo,
                                            GolferCourse = courseName,
                                            IsReceiveChat = golfer.IsReceiveChatMessages,

                                            IsBlocked = (from courseBlockedList in _db.GF_CourseBlockUserList
                                                         where courseBlockedList.CourseAdminId == UserId
                                                                  && courseBlockedList.BlockedUserId == golfer.GF_ID
                                                                  && courseBlockedList.IsBlockedGolfer == true
                                                         select (courseBlockedList.ID)).Count(),

                                            TodaysMsgsCount = (from golferMsgs in _db.GF_Messages
                                                               where golferMsgs.MsgFrom == golfer.GF_ID
                                                                && golferMsgs.MsgTo == UserId
                                                                 && EntityFunctions.TruncateTime(golferMsgs.CreatedDate) == TodaysDate
                                                                && golferMsgs.IsMessagesFromGolfer == "1"
                                                                && golferMsgs.IsMessagesToGolfer == UserType
                                                               select (golferMsgs.ID)).Count()
                                        };
                            }
                            if (status == "1")
                            {
                                query = query.Where(x => x.IsOnline == true);
                            }
                            else if (status == "0")
                            {
                                query = query.Where(x => x.IsOnline == false);
                            }
                            else
                            { }
                            totalRecords = query.Count();
                            IQueryable<OnLineUsers> lst = query.AsQueryable();
                            return lst.OrderBy(sortExpression + " " + sortDirection).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
                        }
                    }
                    catch
                    {
                        totalRecords = 0;

                        return new List<OnLineUsers>();

                    }

                    #endregion
                }
                else if (type == "CC")
                {
                    #region Cartie/Gophie User

                    var query = from golfer in _db.GF_AdminUsers
                                where (golfer.Type == "CC" && golfer.CourseId == _courseId)
                                && golfer.Status == "A"
                                select new OnLineUsers()
                                {
                                    Id = golfer.ID,
                                    Name = golfer.FirstName + " " + golfer.LastName,
                                    EmailAddress = golfer.Email,
                                    IsOnline = golfer.IsOnline,
                                    Type = "CC",
                                    DateOfBirth = DateTime.Now,
                                    Gender = "",
                                    Phone = golfer.Phone,
                                    GolferCourse = courseName,
                                    IsReceiveChat = true,

                                    IsBlocked = (from courseBlockedList in _db.GF_CourseBlockUserList
                                                 where courseBlockedList.CourseAdminId == UserId
                                                          && courseBlockedList.BlockedUserId == golfer.ID
                                                          && courseBlockedList.IsBlockedGolfer == false
                                                 select (courseBlockedList.ID)).Count(),

                                    TodaysMsgsCount = (from golferMsgs in _db.GF_Messages
                                                       where golferMsgs.MsgFrom == golfer.ID
                                                        && golferMsgs.MsgTo == UserId
                                                            && EntityFunctions.TruncateTime(golferMsgs.CreatedDate) == TodaysDate
                                                        && golferMsgs.IsMessagesFromGolfer == UserType
                                                        && golferMsgs.IsMessagesToGolfer == "0"
                                                       select (golferMsgs.ID)).Count()
                                };
                    if (filterExpression != "")
                    {

                        query = from golfer in _db.GF_AdminUsers
                                where ((golfer.FirstName.Contains(filterExpression) || golfer.Email.Contains(filterExpression)) && golfer.Type == "CC" && golfer.CourseId == _courseId)
                                && golfer.Status == "A"
                                select new OnLineUsers()
                                {
                                    Id = golfer.ID,
                                    Name = golfer.FirstName + " " + golfer.LastName,
                                    EmailAddress = golfer.Email,
                                    IsOnline = golfer.IsOnline,
                                    Type = "CC",
                                    DateOfBirth = DateTime.Now,
                                    Gender = "",
                                    Phone = golfer.Phone,
                                    GolferCourse = courseName,
                                    IsReceiveChat = true,

                                    IsBlocked = (from courseBlockedList in _db.GF_CourseBlockUserList
                                                 where courseBlockedList.CourseAdminId == UserId
                                                          && courseBlockedList.BlockedUserId == golfer.ID
                                                          && courseBlockedList.IsBlockedGolfer == false
                                                 select (courseBlockedList.ID)).Count(),

                                    TodaysMsgsCount = (from golferMsgs in _db.GF_Messages
                                                       where golferMsgs.MsgFrom == golfer.ID
                                                        && golferMsgs.MsgTo == UserId
                                                             && EntityFunctions.TruncateTime(golferMsgs.CreatedDate) == TodaysDate
                                                        && golferMsgs.IsMessagesFromGolfer == UserType
                                                        && golferMsgs.IsMessagesToGolfer == "0"
                                                       select (golferMsgs.ID)).Count()
                                };
                    }
                    if (status == "1")
                    {
                        query = query.Where(x => x.IsOnline == true);
                    }
                    else if (status == "0")
                    {
                        query = query.Where(x => x.IsOnline == false);
                    }
                    else
                    { }
                    totalRecords = query.Count();
                    IQueryable<OnLineUsers> lst = query.AsQueryable();
                    return lst.OrderBy(sortExpression + " " + sortDirection).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();

                    #endregion
                }
                else if (type == "CK")
                {
                    #region Kitchen User

                    var query = from golfer in _db.GF_AdminUsers
                                where (golfer.Type == "CK" && golfer.CourseId == _courseId)
                                && golfer.Status == "A"
                                select new OnLineUsers()
                                {
                                    Id = golfer.ID,
                                    Name = golfer.FirstName + " " + golfer.LastName,
                                    EmailAddress = golfer.Email,
                                    IsOnline = golfer.IsOnline,
                                    Type = "CK",
                                    DateOfBirth = DateTime.Now,
                                    Gender = "",
                                    Phone = golfer.Phone,
                                    GolferCourse = courseName,
                                    IsReceiveChat = true,

                                    IsBlocked = (from courseBlockedList in _db.GF_CourseBlockUserList
                                                 where courseBlockedList.CourseAdminId == UserId
                                                          && courseBlockedList.BlockedUserId == golfer.ID
                                                          && courseBlockedList.IsBlockedGolfer == false
                                                 select (courseBlockedList.ID)).Count(),

                                    TodaysMsgsCount = (from golferMsgs in _db.GF_Messages
                                                       where golferMsgs.MsgFrom == golfer.ID
                                                        && golferMsgs.MsgTo == UserId
                                                            && EntityFunctions.TruncateTime(golferMsgs.CreatedDate) == TodaysDate
                                                        && golferMsgs.IsMessagesFromGolfer == UserType
                                                        && golferMsgs.IsMessagesToGolfer == "0"
                                                       select (golferMsgs.ID)).Count()
                                };
                    if (filterExpression != "")
                    {

                        query = from golfer in _db.GF_AdminUsers

                                where ((golfer.FirstName.Contains(filterExpression) || golfer.Email.Contains(filterExpression)) && golfer.Type == "CK" && golfer.CourseId == _courseId)
                                   && golfer.Status == "A"
                                select new OnLineUsers()
                                {
                                    Id = golfer.ID,
                                    Name = golfer.FirstName + " " + golfer.LastName,
                                    EmailAddress = golfer.Email,
                                    IsOnline = golfer.IsOnline,
                                    Type = "CK",
                                    DateOfBirth = DateTime.Now,
                                    Gender = "",
                                    Phone = golfer.Phone,
                                    GolferCourse = courseName,
                                    IsReceiveChat = true,

                                    IsBlocked = (from courseBlockedList in _db.GF_CourseBlockUserList
                                                 where courseBlockedList.CourseAdminId == UserId
                                                          && courseBlockedList.BlockedUserId == golfer.ID
                                                          && courseBlockedList.IsBlockedGolfer == false
                                                 select (courseBlockedList.ID)).Count(),

                                    TodaysMsgsCount = (from golferMsgs in _db.GF_Messages
                                                       where golferMsgs.MsgFrom == golfer.ID
                                                        && golferMsgs.MsgTo == UserId
                                                              && EntityFunctions.TruncateTime(golferMsgs.CreatedDate) == TodaysDate
                                                        && golferMsgs.IsMessagesFromGolfer == UserType
                                                        && golferMsgs.IsMessagesToGolfer == "0"
                                                       select (golferMsgs.ID)).Count()
                                };
                    }
                    if (status == "1")
                    {
                        query = query.Where(x => x.IsOnline == true);
                    }
                    else if (status == "0")
                    {
                        query = query.Where(x => x.IsOnline == false);
                    }
                    else
                    { }
                    totalRecords = query.Count();
                    IQueryable<OnLineUsers> lst = query.AsQueryable();
                    return lst.OrderBy(sortExpression + " " + sortDirection).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();

                    #endregion
                }
                else if (type == "CP")
                {
                    #region Proshop User

                    var query = from golfer in _db.GF_AdminUsers
                                where (golfer.Type == "CP" && golfer.CourseId == _courseId)
                                && golfer.Status == "A"
                                select new OnLineUsers()
                                {
                                    Id = golfer.ID,
                                    Name = golfer.FirstName + " " + golfer.LastName,
                                    EmailAddress = golfer.Email,
                                    IsOnline = golfer.IsOnline,
                                    Type = "CP",
                                    DateOfBirth = DateTime.Now,
                                    Gender = "",
                                    Phone = golfer.Phone,
                                    GolferCourse = courseName,
                                    IsReceiveChat = true,

                                    IsBlocked = (from courseBlockedList in _db.GF_CourseBlockUserList
                                                 where courseBlockedList.CourseAdminId == UserId
                                                          && courseBlockedList.BlockedUserId == golfer.ID
                                                          && courseBlockedList.IsBlockedGolfer == false
                                                 select (courseBlockedList.ID)).Count(),

                                    TodaysMsgsCount = (from golferMsgs in _db.GF_Messages
                                                       where golferMsgs.MsgFrom == golfer.ID
                                                        && golferMsgs.MsgTo == UserId
                                                           && EntityFunctions.TruncateTime(golferMsgs.CreatedDate) == TodaysDate
                                                        && golferMsgs.IsMessagesFromGolfer == UserType
                                                        && golferMsgs.IsMessagesToGolfer == "0"
                                                       select (golferMsgs.ID)).Count()
                                };
                    if (filterExpression != "")
                    {

                        query = from golfer in _db.GF_AdminUsers

                                where ((golfer.FirstName.Contains(filterExpression) || golfer.Email.Contains(filterExpression)) && golfer.Type == "CP" && golfer.CourseId == _courseId)
                                && golfer.Status == "A"
                                select new OnLineUsers()
                                {
                                    Id = golfer.ID,
                                    Name = golfer.FirstName + " " + golfer.LastName,
                                    EmailAddress = golfer.Email,
                                    IsOnline = golfer.IsOnline,
                                    Type = "CP",
                                    DateOfBirth = DateTime.Now,
                                    Gender = "",
                                    Phone = golfer.Phone,
                                    GolferCourse = courseName,
                                    IsReceiveChat = true,

                                    IsBlocked = (from courseBlockedList in _db.GF_CourseBlockUserList
                                                 where courseBlockedList.CourseAdminId == UserId
                                                          && courseBlockedList.BlockedUserId == golfer.ID
                                                          && courseBlockedList.IsBlockedGolfer == false
                                                 select (courseBlockedList.ID)).Count(),

                                    TodaysMsgsCount = (from golferMsgs in _db.GF_Messages
                                                       where golferMsgs.MsgFrom == golfer.ID
                                                        && golferMsgs.MsgTo == UserId
                                                      && EntityFunctions.TruncateTime(golferMsgs.CreatedDate) == TodaysDate
                                                        && golferMsgs.IsMessagesFromGolfer == UserType
                                                        && golferMsgs.IsMessagesToGolfer == "0"
                                                       select (golferMsgs.ID)).Count()
                                };
                    }
                    if (status == "1")
                    {
                        query = query.Where(x => x.IsOnline == true);
                    }
                    else if (status == "0")
                    {
                        query = query.Where(x => x.IsOnline == false);
                    }
                    else
                    { }
                    totalRecords = query.Count();
                    IQueryable<OnLineUsers> lst = query.AsQueryable();
                    return lst.OrderBy(sortExpression + " " + sortDirection).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();

                    #endregion
                }
                else if (type == "SC")
                {
                    #region Support Center User

                    var query = from golfer in _db.GF_AdminUsers
                                where (golfer.Type == Golfler.Models.UserType.SuperAdmin || golfer.Type == Golfler.Models.UserType.Admin)
                                && golfer.Status == StatusType.Active
                                select new OnLineUsers()
                                {
                                    Id = golfer.ID,
                                    Name = golfer.FirstName + " " + golfer.LastName,
                                    EmailAddress = golfer.Email,
                                    IsOnline = golfer.IsOnline,
                                    Type = "SC",
                                    DateOfBirth = DateTime.Now,
                                    Gender = "",
                                    Phone = golfer.Phone,
                                    GolferCourse = "Support Center",
                                    IsReceiveChat = true,

                                    IsBlocked = 0,

                                    TodaysMsgsCount = (from golferMsgs in _db.GF_Messages
                                                       where golferMsgs.MsgFrom == golfer.ID
                                                        && golferMsgs.MsgTo == UserId
                                                           && EntityFunctions.TruncateTime(golferMsgs.CreatedDate) == TodaysDate
                                                        && golferMsgs.IsMessagesFromGolfer == UserType
                                                        && golferMsgs.IsMessagesToGolfer == "0"
                                                       select (golferMsgs.ID)).Count()
                                };
                    if (filterExpression != "")
                    {

                        query = from golfer in _db.GF_AdminUsers

                                where ((golfer.FirstName.Contains(filterExpression) || golfer.Email.Contains(filterExpression)) &&
                                (golfer.Type == Golfler.Models.UserType.SuperAdmin || golfer.Type == Golfler.Models.UserType.Admin))
                                && golfer.Status == StatusType.Active
                                select new OnLineUsers()
                                {
                                    Id = golfer.ID,
                                    Name = golfer.FirstName + " " + golfer.LastName,
                                    EmailAddress = golfer.Email,
                                    IsOnline = golfer.IsOnline,
                                    Type = "SC",
                                    DateOfBirth = DateTime.Now,
                                    Gender = "",
                                    Phone = golfer.Phone,
                                    GolferCourse = "Support Center",
                                    IsReceiveChat = true,

                                    IsBlocked = 0,

                                    TodaysMsgsCount = (from golferMsgs in _db.GF_Messages
                                                       where golferMsgs.MsgFrom == golfer.ID
                                                        && golferMsgs.MsgTo == UserId
                                                      && EntityFunctions.TruncateTime(golferMsgs.CreatedDate) == TodaysDate
                                                        && golferMsgs.IsMessagesFromGolfer == UserType
                                                        && golferMsgs.IsMessagesToGolfer == "0"
                                                       select (golferMsgs.ID)).Count()
                                };
                    }
                    if (status == "1")
                    {
                        query = query.Where(x => x.IsOnline == true);
                    }
                    else if (status == "0")
                    {
                        query = query.Where(x => x.IsOnline == false);
                    }
                    else
                    { }
                    totalRecords = query.Count();
                    IQueryable<OnLineUsers> lst = query.AsQueryable();
                    return lst.OrderBy(sortExpression + " " + sortDirection).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();

                    #endregion
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                return null;

            }
        }

        /// <summary>
        /// Created By: Amit Kumar
        /// purpose: get all visited courses by golfer
        /// </summary>
        /// <returns></returns>
        public IEnumerable<object> GetAllVisitedCourses()
        {
            Db = new GolflerEntities();

            var visistedCourse = Db.GF_CourseVisitLog.Where(x => (x.GolferID ?? 0) == LoginInfo.GolferUserId &&
                (x.CourseID ?? 0) != 0).Select(x =>
                    new
                    {
                        CourseID = x.CourseID,
                        CourseName = x.GF_CourseInfo.COURSE_NAME
                    }).Distinct().ToList<object>().AsEnumerable();

            List<object> defaultRecord = new List<object>();
            defaultRecord.Add(new
            {
                CourseID = -1,
                CourseName = "Support Center"
            });

            return defaultRecord.Union(visistedCourse);

            //return Db.GF_CourseInfo.Where(x => visistedCourseIDs.Contains(x.ID)).OrderBy(x => x.COURSE_NAME).ToList();
        }

        #endregion

        #region Get DashBoard Info List
        ///// <summary>
        ///// Created By: Veera
        ///// Created Date: 27 April 2015
        ///// Purpose: Get Golfer DashBoard Info
        ///// </summary>
        ///// <param name="orderHistory"></param>
        ///// <returns></returns>
        public static void GetGolferDashBoardInfo(long golferId, ref int userCount, ref int orderCount, ref int msgCount, ref int resoCount)
        {
            try
            {
                GolflerEntities _db = new GolflerEntities();

                #region UserCount
                var objCourse = _db.GF_GolferUser.FirstOrDefault(x => x.GolferID == golferId);
                if (objCourse != null)
                {
                    userCount = _db.GF_GolferUser.Where(x => x.CourseID == objCourse.CourseID).Count();
                }
                else
                {
                    userCount = 0;
                }
                #endregion

                #region orderCount
                orderCount = _db.GF_Order.Where(x => x.GolferID == golferId).Count();
                #endregion

                #region msgCount
                msgCount = _db.GF_Messages.Where(x => x.MsgTo == golferId && x.IsMessagesToGolfer == "1").Count();
                #endregion

                #region resoCount
                resoCount = _db.GF_ResolutionMessageHistory.Where(x => x.LogUserID == golferId && x.UserType == "G").Count();
                #endregion
            }
            catch (Exception ex)
            {


            }
        }


        ///// <summary>
        ///// Created By: Veera
        ///// Created Date: 27 April 2015
        ///// Purpose: Get Superadmin DashBoard Info
        ///// </summary>
        ///// <param name="orderHistory"></param>
        ///// <returns></returns>
        public static void GetSuperAdminDashBoardInfo(long Id, ref int userCount, ref int roleCount, ref int courseCount,
            ref int staticpageCount, ref int golferUserCount, ref int suggCourseCount)
        {

            try
            {
                GolflerEntities _db = new GolflerEntities();

                #region UserCount

                var objCourse = _db.GF_AdminUsers.FirstOrDefault(x => x.ID == Id);
                if (objCourse != null)
                {
                    userCount = _db.GF_AdminUsers.Count(x => x.Status != StatusType.Delete &&
                        x.ID != LoginInfo.UserId &&
                        x.Type.Contains(UserType.Admin) &&
                        x.Type != UserType.SuperAdmin);
                }
                else
                {
                    userCount = 0;
                }

                #endregion

                #region roleCount

                roleCount = _db.GF_Roles.Where(x => x.Status != StatusType.Delete && x.CourseUserId == 0).Count();

                #endregion

                #region courseCount

                courseCount = _db.GF_CourseInfo.Where(x => x.Status != StatusType.Delete).Count();

                #endregion

                #region staticpageCount

                staticpageCount = _db.GF_StaticPages.Where(x => x.Status != StatusType.Delete).Count();

                #endregion

                #region UserCount

                golferUserCount = _db.GF_Golfer.Where(x => x.Status != StatusType.Delete).Count();

                #endregion

                #region Suggested Course Count

                DateTime current = DateTime.UtcNow.AddDays(-6).Date;
                suggCourseCount = _db.GF_CourseSuggest.Where(x => x.Status == StatusType.Active &&
                    EntityFunctions.TruncateTime(x.CreatedOn) >= current).Count();

                #endregion
            }
            catch (Exception ex)
            {


            }
        }



        ///// <summary>
        ///// Created By: Veera
        ///// Created Date: 27 April 2015
        ///// Purpose: Get CourseAdmin DashBoard Info
        ///// </summary>
        ///// <param name="orderHistory"></param>
        ///// <returns></returns>
        public static void GetCourseAdminDashBoardInfo(long Id, ref int userCount, ref int foodCount, ref int courseCount, ref int orderCount)
        {

            try
            {
                GolflerEntities _db = new GolflerEntities();

                #region UserCount

                userCount = _db.GF_AdminUsers.Where(x => x.CourseId == LoginInfo.CourseId &&
                    x.Status != StatusType.Delete &&
                    x.ID != LoginInfo.UserId).Count();

                #endregion

                #region orderCount

                orderCount = _db.GF_Order.Where(x => x.CourseID == LoginInfo.CourseId && !(x.IsRejected ?? false)).Count();

                #endregion

                #region courseCount

                courseCount = _db.GF_CourseInfo.Count();

                #endregion

                #region foodCount

                foodCount = _db.GF_CourseFoodItem.Where(x => x.CourseID == LoginInfo.CourseId).Count();

                #endregion
            }
            catch (Exception ex)
            {


            }
        }

        #endregion

        #region Get All Course Admin's of Messaging Center

        /// <summary>
        /// Created By: Amit Kumar
        /// Created Date: 14 Oct 2015
        /// Purpose: Get All Course Admin's of Messaging Center
        /// </summary>
        /// <returns></returns>
        public List<OnLineUsers> GetOnlineCourseUserList(string searchText, string courseId, long UserId, string status, string userType,
            string sortExpression, string sortDirection, int pageIndex, int pageSize, ref int totalRecords)
        {
            OnLineUsers objOnLineUsers = new OnLineUsers();

            try
            {
                Db = new GolflerEntities();
                long _courseId;
                bool isCourseID = long.TryParse(courseId, out _courseId);
                DateTime TodaysDate = Convert.ToDateTime(DateTime.UtcNow).Date;

                IQueryable<OnLineUsers> query;

                if (userType == "SC")
                {
                    #region Course User

                    query = from golfer in Db.GF_AdminUsers
                            let courseName = Db.GF_CourseInfo.FirstOrDefault(x => x.ID == golfer.CourseId)
                            where (!string.IsNullOrEmpty(searchText) ? (golfer.FirstName.StartsWith(searchText) || golfer.Email.StartsWith(searchText)) : golfer.CourseId >= 0) &&
                                    (golfer.Type == UserType.CourseAdmin || golfer.Type == UserType.Proshop) &&
                                    (_courseId <= 0 ? golfer.CourseId >= 0 : golfer.CourseId == _courseId) &&
                                    golfer.Status == StatusType.Active
                            select new OnLineUsers()
                            {
                                Id = golfer.ID,
                                Name = golfer.FirstName + " " + golfer.LastName,
                                EmailAddress = golfer.Email,
                                IsOnline = golfer.IsOnline,
                                Type = "CP",
                                DateOfBirth = DateTime.Now,
                                Gender = "",
                                Phone = golfer.Phone,
                                GolferCourse = courseName.COURSE_NAME, //GetCourseName(golfer.CourseId ?? 0, Db),
                                IsReceiveChat = true,
                                IsBlocked = 0,

                                TodaysMsgsCount = (from golferMsgs in Db.GF_Messages
                                                   where golferMsgs.MsgFrom == golfer.ID &&
                                                           golferMsgs.MsgTo == UserId &&
                                                           EntityFunctions.TruncateTime(golferMsgs.CreatedDate) == TodaysDate &&
                                                           (golferMsgs.IsMessagesFromGolfer == UserType.CourseAdmin || golferMsgs.IsMessagesFromGolfer == UserType.Proshop) &&
                                                           golferMsgs.IsMessagesToGolfer == "0"
                                                   select (golferMsgs.ID)).Count()
                            };

                    #endregion
                }
                else if (userType == "NU")
                {
                    #region Golfer User

                    query = from golfer in Db.GF_Golfer
                            let courseName = Db.GF_CourseUsers.FirstOrDefault(x => x.UserID == golfer.GF_ID && (_courseId <= 0 ? x.CourseID > 0 : x.CourseID == _courseId))
                            where (!string.IsNullOrEmpty(searchText) ? (golfer.FirstName.StartsWith(searchText) || golfer.Email.StartsWith(searchText)) : golfer.GF_ID > 0) &&
                                    golfer.Status == StatusType.Active
                            select new OnLineUsers()
                            {
                                Id = golfer.GF_ID,
                                Name = golfer.FirstName + " " + golfer.LastName,
                                EmailAddress = golfer.Email,
                                IsOnline = golfer.IsOnline,
                                Type = "NU",
                                DateOfBirth = golfer.DateOfBirth,
                                Gender = golfer.Gender,
                                Phone = golfer.MobileNo,
                                GolferCourse = courseName != null ? courseName.GF_CourseInfo.COURSE_NAME : "",
                                IsReceiveChat = golfer.IsReceiveChatMessages,
                                IsBlocked = 0,

                                TodaysMsgsCount = (from golferMsgs in Db.GF_Messages
                                                   where golferMsgs.MsgFrom == golfer.GF_ID
                                                    && golferMsgs.MsgTo == UserId
                                                    && EntityFunctions.TruncateTime(golferMsgs.CreatedDate) == TodaysDate
                                                    && golferMsgs.IsMessagesFromGolfer == "1"
                                                    && golferMsgs.IsMessagesToGolfer == "0"
                                                   select (golferMsgs.ID)).Count()
                            };

                    #endregion
                }
                else
                {
                    query = null;
                }

                if (query != null)
                {
                    if (status == "1")
                        query = query.Where(x => (x.IsOnline ?? false));
                    else if (status == "0")
                        query = query.Where(x => !(x.IsOnline ?? false));

                    totalRecords = query.Count();

                    IQueryable<OnLineUsers> lst = query.AsQueryable();

                    return lst.OrderBy(sortExpression + " " + sortDirection).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
                }
                else
                {
                    totalRecords = 0;
                    return new List<OnLineUsers>();
                }
            }
            catch (Exception ex)
            {
                string errMsg = ex.Message;
                return new List<OnLineUsers>();
            }
        }

        internal string GetCourseName(long courseID, GolflerEntities Db)
        {
            try
            {
                var courseName = Db.GF_CourseInfo.FirstOrDefault(x => x.ID == courseID);
                if (courseName != null)
                    return courseName.COURSE_NAME;

                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        #endregion

        /// <summary>
        /// Created By:Veera
        /// Created Date: 29 May 2015
        /// Purpose: Get Golfer spend and Play chart By ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public GolferSpendNPlayResult GetGolferSpendNplayById(long id)
        {
            GolferSpendNPlayResult objResult = new GolferSpendNPlayResult();
            try
            {

                Db = new GolflerEntities();

                var lst = new List<GF_SP_GetGolferSpend_Result>();
                lst = Db.GF_SP_GetGolferSpend(id).ToList();

                if (lst.Count > 0)
                {
                    List<SpendResult> lstSpendResult = new List<SpendResult>();
                    foreach (var o in lst)
                    {
                        SpendResult resultSpend = new SpendResult();

                        resultSpend.Amount = o.Amount;
                        resultSpend.Date = o.Date;

                        lstSpendResult.Add(resultSpend);

                    }
                    objResult.SpendResult = lstSpendResult;
                }

                var lstPlay = new List<GF_SP_GetGolferPlay_Result>();
                lstPlay = Db.GF_SP_GetGolferPlay(id).ToList();

                if (lstPlay.Count > 0)
                {
                    List<PlayResult> lstPlayResult = new List<PlayResult>();
                    foreach (var o in lstPlay)
                    {
                        PlayResult resultPlay = new PlayResult();

                        resultPlay.NumberofRounds = o.NumberofRounds;
                        resultPlay.Date = o.Date;

                        lstPlayResult.Add(resultPlay);

                    }
                    objResult.PlayResult = lstPlayResult;
                }

            }
            catch
            {

            }
            return objResult;
        }

        /// <summary>
        /// Created By:Veera
        /// Created Date: 1 Jume 2015
        /// Purpose: Get new Incoming Msgs after golfer/course login
        /// </summary>
        /// <param name="id"></param>
        /// <param name="isGolfer"></param>
        /// <returns></returns>
        public int GetNewMsgCount(long id, int isGolfer)
        {
            int result = 0;
            try
            {

                Db = new GolflerEntities();
                var list = Db.GF_SP_GetNewIncomingMsgs(id, isGolfer).ToList();
                if (list != null && list.Count() > 0)
                {
                    result = Convert.ToInt32(list[0].NewMsgCount);
                }

            }
            catch
            {

            }
            return result;
        }

        /// <summary>
        /// Created By:Veera
        /// Created Date: 2 June 2015
        /// Purpose: Block/Unblock User
        /// </summary>
        /// <param name="id"></param>
        /// <param name="isGolfer"></param>
        /// <returns></returns>
        public string BlockNUnblockUser(long userId, long blockedUserId, bool isGolferUser, bool isBlockedGolferUser, string block)
        {
            string result = "-1";
            try
            {

                Db = new GolflerEntities();
                if (block == "1")//block User
                {
                    GF_BlockUserList obj = new GF_BlockUserList();
                    obj.UserId = userId;
                    obj.BlockedUserId = blockedUserId;
                    obj.IsGolferUser = isGolferUser;
                    obj.IsBlockedGolferUser = isBlockedGolferUser;
                    obj.CreatedDate = DateTime.Now;
                    Db.GF_BlockUserList.Add(obj);
                    Db.SaveChanges();
                    result = "1";

                }
                else //unblock User
                {
                    var blockUser = Db.GF_BlockUserList.Where(x => x.UserId == userId && x.BlockedUserId == blockedUserId && x.IsGolferUser == isGolferUser && x.IsBlockedGolferUser == isBlockedGolferUser);
                    foreach (var unblock in blockUser)
                    {
                        Db.GF_BlockUserList.Remove(unblock);
                    }
                    Db.SaveChanges();
                    result = "2";
                }


            }
            catch
            {
                result = "-1";
            }
            return result;
        }
        /// <summary>
        /// Created By:Veera
        /// Created Date: 2 June 2015
        /// Purpose: Block/Unblock User
        /// </summary>
        /// <param name="id"></param>
        /// <param name="isGolfer"></param>
        /// <returns></returns>
        /// 

        //public bool IsBlockedByCourse(long userId, bool IsGolfer)
        //{
        //    bool result = true;
        //    try
        //    {

        //        Db = new GolflerEntities();
        //        long CourseId=Convert.ToInt64(Db.GF_GolferUser.FirstOrDefault(x=>x.GolferID==userId).CourseID);
        //        long CourseAdminId=Convert.ToInt64(Db.GF_AdminUsers.FirstOrDefault(x=>x.CourseId==CourseId && x.Type=="CA").ID);
        //        if(CourseId != null)
        //        {
        //            var obj= Db.GF_CourseBlockUserList.Where(x => x.BlockedUserId==userId && x.IsBlockedGolfer==IsGolfer && x.CourseId==CourseId
        //        }
        //        else
        //        {
        //            result = false;

        //         }



        //    }
        //    catch
        //    {
        //        result = false;
        //    }
        //    return result;
        //}
        //public partial class GF_IncomeSlab
        //{
        //    public string Name { get; set; }
        //}
    }
    class GolferMetaData
    {

        //[Required(ErrorMessage = "Required")]
        //[RegularExpression(RegularExp.Numeric, ErrorMessage = "Only numbers allowed")]
        //[RegularExpression(RegularExp.PhoneNotReq, ErrorMessage = "Please enter valid phone number.")]
        //[StringLength(50, ErrorMessage = " Only numbers allowed.")]
        //[DisplayName("Phone")]

        [Display(Name = "Phone")]
        [RegularExpression(RegularExp.PhoneNotReq, ErrorMessage = "Please enter valid phone number.")]
        [StringLength(20, ErrorMessage = "Phone number cannot be greater than 20 character long.")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        [DefaultValue("")]
        public string Phone { get; set; }

        // [Required(ErrorMessage = "Required")]
        [RegularExpression(RegularExp.AlphaNumeric, ErrorMessage = " Name only contains alphabets and numbers")]
        [StringLength(50, ErrorMessage = " Name cannot be longer than 50 characters.")]
        [DisplayName("Mobile Number")]
        public string MobileNo { get; set; }

        [Required(ErrorMessage = "Required")]
        [RegularExpression(RegularExp.AlphaNumeric, ErrorMessage = "Name only contains alphabets and numbers")]
        [StringLength(50, ErrorMessage = " Name cannot be longer than 50 characters.")]
        [DisplayName("First Name")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Required")]
        [RegularExpression(RegularExp.AlphaNumeric, ErrorMessage = " Name only contains alphabets and numbers")]
        [StringLength(50, ErrorMessage = " Name cannot be longer than 50 characters.")]
        [DisplayName("Last Name")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Required")]
        [DisplayName("Password")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Required")]
        [RegularExpression(@"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$", ErrorMessage = "Please enter a valid e-mail adress")]

        [DisplayName("Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Required")]
        [DisplayName("Tee")]
        public string Tee { get; set; }

        //[Required(ErrorMessage = "Required")]
        ////[RegularExpression(RegularExp.Numeric, ErrorMessage = " Only numbers are allowed.")]
        //[StringLength(50, ErrorMessage = " Only numbers are allowed.")]
        //[DisplayName("Phone")]
        //public string Phone { get; set; }

        ////  [Required(ErrorMessage = "Required")]
        //  [RegularExpression(RegularExp.Numeric, ErrorMessage = " Only numbers are allowed.")]
        //  [StringLength(50, ErrorMessage = " Only numbers are allowed.")]
        //  [DisplayName("ZipCode")]
        //  public string ZipCode { get; set; }


    }
    public class GolferMapResult
    {
        public int Status { get; set; }
        public string Error { get; set; }
        public IEnumerable<object> Golfers { get; set; }
    }
    public partial class GF_EmailTemplates
    {
        public string MessageBodyOriginal { get; set; }
    }
    public class OnLineUsers
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string EmailAddress { get; set; }
        public bool? IsOnline { get; set; }
        public string Type { get; set; }
        public int TodaysMsgsCount { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string Phone { get; set; }
        public string GolferCourse { get; set; }
        public bool? IsReceiveChat { get; set; }
        public int IsBlocked { get; set; }
    }

    public class GolferPlayingHistory
    {
        public Int64 GolpherId { get; set; }
        public Int64 CourseId { get; set; }
        public string CourseName { get; set; }
        public string GolpherName { get; set; }
        public string Email { get; set; }

        public Int64 TotalPlay { get; set; }
        public string OneHoleAvgTime { get; set; }
        public string NineHoleAvgTime { get; set; }
        public string EighteenHoleAvgTime { get; set; }
        public DateTime LastGameDate { get; set; }
        public string strLastGameDate { get; set; }

        public string GameName { get; set; }
        public string TotalTimePlay { get; set; }
        public string TotalHoles { get; set; }
        public string GameRoundId { get; set; }

        public string DeviceType { get; set; }
    }

    public class GolferSpendNPlayResult
    {
        public IEnumerable<object> SpendResult { get; set; }
        public IEnumerable<object> PlayResult { get; set; }
    }
    public class SpendResult
    {
        public decimal? Amount { get; set; }
        public string Date { get; set; }
    }
    public class PlayResult
    {
        public int? NumberofRounds { get; set; }
        public string Date { get; set; }
    }
    public class NewMsgCountResult
    {
        public int NewMsgCount { get; set; }

    }
    public class GolferIncome
    {
        public long ID { get; set; }
        public string Name { get; set; }
    }

    public class GolferUsersInformation
    {
        public long SrNo { get; set; }
        public long GF_ID { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public string MobileNo { get; set; }
        public DateTime? CreatedOn { get; set; }
        public DateTime? LastLogin { get; set; }
        public string COURSE_NAME { get; set; }
    }
}