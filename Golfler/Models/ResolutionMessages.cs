using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Objects;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Golfler.Models
{
    public partial class GF_ResolutionCenter
    {

        private GolflerEntities _db = null;

        public string sentBy { get; set; }
        public string sentByEncryptedId { get; set; }
        public string courseName { get; set; }
        public string Name { get; set; }
        public string comment { get; set; }
        public DateTime LatestCreatedDate { get; set; }
        public string LatestComments { get; set; }
        public string LatestReplyBy { get; set; }
        public string strResolutionType { get; set; }

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
        public IQueryable<GF_ResolutionCenter> GetResolutionMessages(long filterExpression, string status, DateTime fromdate, DateTime todate, string username, string sortExpression, string sortDirection, int pageIndex, int pageSize, ref int totalRecords)
        {
            _db = new GolflerEntities();
            IQueryable<GF_ResolutionCenter> list = null;

            //CK  CC  CR  CA  CP
            if (LoginInfo.LoginUserType == UserType.Kitchen || LoginInfo.LoginUserType == UserType.Cartie || LoginInfo.LoginUserType == UserType.Ranger || LoginInfo.LoginUserType == UserType.CourseAdmin || LoginInfo.LoginUserType == UserType.Proshop)
            {//course....................Golfler/CourseAdmin/
                if (filterExpression != 0)
                {
                    list = _db.GF_ResolutionCenter.Where(x => x.CourseID == filterExpression && x.Status == status && (x.CreatedDate >= fromdate && x.CreatedDate <= todate) && x.CourseID == LoginInfo.CourseUserId && (x.GF_Golfer.FirstName.Contains(username) || x.GF_Golfer.LastName.Contains(username)));
                }

                else
                    list = _db.GF_ResolutionCenter.Where(x => x.Status == status && (x.CreatedDate >= fromdate && x.CreatedDate <= todate) && x.CourseID == LoginInfo.CourseUserId && (x.GF_Golfer.FirstName.Contains(username) || x.GF_Golfer.LastName.Contains(username)));

                totalRecords = list.Count();
            }
            else if (LoginInfo.GolferType == UserType.Golfer)
            {//Golfler/golfer/
                if (filterExpression != 0)
                {
                    list = _db.GF_ResolutionCenter.Where(x => x.CourseID == filterExpression && x.Status == status && (x.CreatedDate >= fromdate && x.CreatedDate <= todate) && x.GolferID == LoginInfo.GolferUserId && (x.GF_Golfer.FirstName.Contains(username) || x.GF_Golfer.LastName.Contains(username)));
                }

                else
                    // list = _db.GF_ResolutionCenter.Where(x => x.Status == status && (x.CreatedDate.Value.Date >= fromdate.Date && x.CreatedDate.Value.Date <= todate.Date) && x.GolferID == LoginInfo.GolferUserId);
                    list = _db.GF_ResolutionCenter.Where(x => x.Status == status && (x.CreatedDate >= fromdate && x.CreatedDate <= todate) && x.GolferID == LoginInfo.GolferUserId && (x.GF_Golfer.FirstName.Contains(username) || x.GF_Golfer.LastName.Contains(username)));

                totalRecords = list.Count();
            }

            return list.OrderBy(sortExpression + " " + sortDirection).Skip((pageIndex - 1) * pageSize).Take(pageSize);
        }

        public IQueryable<GF_ResolutionCenter> GetResolutionMessagesNew(long filterExpression, string status, string fromdate, string todate, string username, string MessageSentTo, string searchResolutionType, string sortExpression, string sortDirection, int pageIndex, int pageSize, ref int totalRecords)
        {
            _db = new GolflerEntities();
            IQueryable<GF_ResolutionCenter> list = null;


            List<GF_ResolutionCenter> listTemp = new List<GF_ResolutionCenter>();

            //   var lstTable = _db.GF_ResolutionCenter.ToList().AsEnumerable().Select((x =>
            List<string> lstSendTo = new List<string>();

            if (!string.IsNullOrEmpty(Convert.ToString(MessageSentTo)))
            {
                if (MessageSentTo.ToLower() != "undefined")
                {
                    if (MessageSentTo == "SA")
                    {
                        lstSendTo.Add(UserType.SuperAdmin);
                    }
                    else
                    {
                        lstSendTo.Add(UserType.Proshop);
                        lstSendTo.Add(UserType.CourseAdmin);
                    }
                }
                else
                {
                    lstSendTo.Add(UserType.SuperAdmin);
                    lstSendTo.Add(UserType.Proshop);
                    lstSendTo.Add(UserType.CourseAdmin);
                }
            }
            else
            {
                lstSendTo.Add(UserType.SuperAdmin);
                lstSendTo.Add(UserType.Proshop);
                lstSendTo.Add(UserType.CourseAdmin);
            }

            DateTime dtDate = DateTime.Parse(fromdate);
            DateTime dtToDate = DateTime.Parse(todate);

            //  var lstTable = _db.GF_ResolutionCenter.Where(x => (LoginInfo.GolferType == UserType.Golfer ? x.SendTo == UserType.Golfer : (x.SendTo == UserType.Proshop || x.SendTo == UserType.CourseAdmin)))
            var lstTableEntity = _db.GF_ResolutionCenter.Where(x =>
                (LoginInfo.IsGolferLoginUser ? (x.GolferID == LoginInfo.GolferUserId
                      && lstSendTo.Contains(x.SendTo) && EntityFunctions.TruncateTime(x.CreatedDate) >= EntityFunctions.TruncateTime(dtDate)
                      && EntityFunctions.TruncateTime(x.CreatedDate) <= EntityFunctions.TruncateTime(dtToDate)) :
                      ((x.SendTo == UserType.Proshop || x.SendTo == UserType.CourseAdmin) ||
                      ((LoginInfo.Type == UserType.CourseAdmin ? x.SenderType.Contains(UserTypeChar.CourseAdminChar) : x.SenderType == LoginInfo.Type) &&
                       (LoginInfo.Type == UserType.CourseAdmin ? (x.SenderID ?? 0) > 0 : (x.SenderID ?? 0) == LoginInfo.UserId))
                          && EntityFunctions.TruncateTime(x.CreatedDate) >= EntityFunctions.TruncateTime(dtDate)
                          && EntityFunctions.TruncateTime(x.CreatedDate) <= EntityFunctions.TruncateTime(dtToDate))))
                 .ToList().AsEnumerable()
                 .Select((x => new GF_ResolutionCenter
                            {
                                ID = x.ID,
                                CourseID = x.CourseID,
                                GolferID = x.GolferID ?? 0,
                                FeedbackTest = x.FeedbackTest,
                                SendTo = x.SendTo,
                                IsRead = x.IsRead,
                                IsReadByAdmin = x.IsReadByAdmin,
                                Status = CommonFunctions.GetLatestStatus(x.ID, x.Status),
                                IsActive = x.IsActive,
                                //CreatedDate = CommonFunctions.DateByCourseTimeZone(Convert.ToInt64(x.CourseID), Convert.ToDateTime(x.CreatedDate)),
                                CreatedDate = x.CreatedDate,
                                //LatestCreatedDate = CommonFunctions.DateByCourseTimeZone(Convert.ToInt64(x.CourseID), Convert.ToDateTime(CommonFunctions.GetLatestDate(x.ID, Convert.ToDateTime(x.CreatedDate)))),
                                LatestCreatedDate = Convert.ToDateTime(CommonFunctions.GetLatestDate(x.ID, Convert.ToDateTime(x.CreatedDate))),
                                CreatedBy = x.CreatedBy,
                                ModifyBy = x.ModifyBy,
                                ModifyDate = x.ModifyDate,
                                //CreatedDate = x.GF_ResolutionMessageHistory.OrderByDescending(y => y.CreatedDate).FirstOrDefault().CreatedDate,
                                //RecentName = CommonFunctions.GetLatestUserName(x.ID, (x.GF_Golfer.FirstName + " " + x.GF_Golfer.LastName)),
                                //Email = x.GF_Golfer.Email
                                sentBy = (x.GolferID ?? 0) != 0 ? (x.GF_Golfer.FirstName + " " + x.GF_Golfer.LastName) : x.SenderID == LoginInfo.UserId ? "me" : GetAdminName(x.SenderID ?? 0, x.SenderType, _db),
                                sentByEncryptedId = CommonFunctions.EncryptUrlParam((x.GolferID ?? 0) != 0 ? x.GolferID ?? 0 : x.SenderID ?? 0),
                                courseName = x.GF_CourseInfo.COURSE_NAME,
                                Name = CommonFunctions.GetLatestUserName(x.ID, (x.GolferID ?? 0) != 0 ? (x.GF_Golfer.FirstName + " " + x.GF_Golfer.LastName) : x.SenderID == LoginInfo.UserId ? "me" : GetAdminName(x.SenderID ?? 0, x.SenderType, _db)),
                                LatestComments = CommonFunctions.GetLatestCommentsFromMessageHistory(x.ID, x.FeedbackTest),
                                strResolutionType = CommonFunctions.GetResolutionType(x.ResolutionType)
                            }
                        ));


            //if (!LoginInfo.IsGolferLoginUser)
            //{
            //var lstCourseTableEntity = _db.GF_ResolutionCenter.Where(x => (LoginInfo.Type == UserType.CourseAdmin ? x.SenderType.Contains(UserTypeChar.CourseAdminChar) : x.SenderType == LoginInfo.Type) &&
            //             (LoginInfo.Type == UserType.CourseAdmin ? (x.SenderID ?? 0) > 0 : (x.SenderID ?? 0) == LoginInfo.UserId) &&
            //              EntityFunctions.TruncateTime(x.CreatedDate) >= EntityFunctions.TruncateTime(dtDate) &&
            //              EntityFunctions.TruncateTime(x.CreatedDate) <= EntityFunctions.TruncateTime(dtToDate))
            //                 .ToList().AsEnumerable()
            //                 .Select((x => new GF_ResolutionCenter
            //                 {
            //                     ID = x.ID,
            //                     CourseID = x.CourseID,
            //                     GolferID = x.GolferID ?? 0,
            //                     FeedbackTest = x.FeedbackTest,
            //                     SendTo = x.SendTo,
            //                     IsRead = x.IsRead,
            //                     Status = CommonFunctions.GetLatestStatus(x.ID, x.Status),
            //                     IsActive = x.IsActive,
            //                     CreatedDate = CommonFunctions.DateByCourseTimeZone(Convert.ToInt64(x.CourseID), Convert.ToDateTime(x.CreatedDate)),
            //                     LatestCreatedDate = CommonFunctions.DateByCourseTimeZone(Convert.ToInt64(x.CourseID), Convert.ToDateTime(CommonFunctions.GetLatestDate(x.ID, Convert.ToDateTime(x.CreatedDate)))),
            //                     CreatedBy = x.CreatedBy,
            //                     ModifyBy = x.ModifyBy,
            //                     ModifyDate = x.ModifyDate,
            //                     //sentBy = (x.GolferID ?? 0) != 0 ? (x.GF_Golfer.FirstName + " " + x.GF_Golfer.LastName) : "",
            //                     sentBy = x.SenderID == LoginInfo.UserId ? "me" : GetAdminName(x.SenderID ?? 0, x.SenderType, _db),
            //                     sentByEncryptedId = CommonFunctions.EncryptUrlParam(x.SenderID ?? 0),
            //                     courseName = x.GF_CourseInfo.COURSE_NAME,
            //                     Name = CommonFunctions.GetLatestUserName(x.ID, x.SenderID == LoginInfo.UserId ? "me" : GetAdminName(x.SenderID ?? 0, x.SenderType, _db)),
            //                     LatestComments = CommonFunctions.GetLatestCommentsFromMessageHistory(x.ID, x.FeedbackTest),
            //                     strResolutionType = CommonFunctions.GetResolutionType(x.ResolutionType)
            //                 }
            //            ));

            //lstTableEntity = lstTableEntity.Union(lstCourseTableEntity);
            //}

            var lstTable = lstTableEntity.ToList();
            listTemp = lstTable;//_db.GF_ResolutionCenter.ToList();
            
            if (!string.IsNullOrEmpty(Convert.ToString(filterExpression)))
            {
                Int64 intCourseId = Convert.ToInt64(filterExpression);

                #region Get Course IDs

                string cIds = string.Join(",", _db.GF_CourseInfo.Where(x => x.Status != StatusType.Delete && (x.ClubHouseID == intCourseId ||
                    x.ID == intCourseId)).Select(v => v.ID).ToArray());
                long[] courseIDs = CommonFunctions.ConvertStringArrayToLongArray(cIds);

                #endregion

                if (intCourseId != 0)
                    listTemp = listTemp.Where(x => courseIDs.Contains(x.CourseID ?? 0)).ToList();// == intCourseId).ToList();
            }
            if (!string.IsNullOrEmpty(status))
            {
                listTemp = listTemp.Where(x => x.Status == status).ToList();
            }

            if (!string.IsNullOrEmpty(username))
            {
                //listTemp = listTemp.Where(x => x.GF_Golfer.FirstName.Contains(username) || x.GF_Golfer.LastName.Contains(username)).ToList();
                listTemp = listTemp.Where(x => x.sentBy.ToUpper().Contains(username.ToUpper()) || x.sentBy.ToUpper().Contains(username.ToUpper())).ToList();
            }

            if (!string.IsNullOrEmpty(searchResolutionType))
            {
                if (searchResolutionType == Golfler.Models.ResolutionType.Praise)
                {
                    listTemp = listTemp.Where(x => x.strResolutionType == Golfler.Models.ResolutionType.GetFullResolutionType(Golfler.Models.ResolutionType.Praise)).ToList();
                }
                if (searchResolutionType == Golfler.Models.ResolutionType.Others)
                {
                    listTemp = listTemp.Where(x => x.strResolutionType == Golfler.Models.ResolutionType.GetFullResolutionType(Golfler.Models.ResolutionType.Others)).ToList();
                }
                if (searchResolutionType == Golfler.Models.ResolutionType.Complaint)
                {
                    listTemp = listTemp.Where(x => x.strResolutionType == Golfler.Models.ResolutionType.GetFullResolutionType(Golfler.Models.ResolutionType.Complaint)).ToList();
                }
            }


            //CK  CC  CR  CA  CP
            if (LoginInfo.LoginUserType == UserType.Kitchen || LoginInfo.LoginUserType == UserType.Cartie || LoginInfo.LoginUserType == UserType.Ranger || LoginInfo.LoginUserType == UserType.CourseAdmin || LoginInfo.LoginUserType == UserType.Proshop)
            {//course....................Golfler/CourseAdmin/
                //if (filterExpression != 0)
                //{
                //    list = _db.GF_ResolutionCenter.Where(x => x.CourseID == filterExpression && x.Status == status && (x.CreatedDate >= fromdate && x.CreatedDate <= todate) && x.CourseID == LoginInfo.CourseUserId && (x.GF_Golfer.FirstName.Contains(username) || x.GF_Golfer.LastName.Contains(username)));
                //}
                //else
                //{
                //    list = _db.GF_ResolutionCenter.Where(x => x.Status == status && (x.CreatedDate >= fromdate && x.CreatedDate <= todate) && x.CourseID == LoginInfo.CourseUserId && (x.GF_Golfer.FirstName.Contains(username) || x.GF_Golfer.LastName.Contains(username)));
                //}
                // totalRecords = list.Count();

                listTemp = listTemp.Where(x => x.CourseID == LoginInfo.CourseUserId).ToList();
            }
            else if (LoginInfo.GolferType == UserType.Golfer)
            {//Golfler/golfer/
                //if (filterExpression != 0)
                //{
                //    list = _db.GF_ResolutionCenter.Where(x => x.CourseID == filterExpression && x.Status == status && (x.CreatedDate >= fromdate && x.CreatedDate <= todate) && x.GolferID == LoginInfo.GolferUserId && (x.GF_Golfer.FirstName.Contains(username) || x.GF_Golfer.LastName.Contains(username)));
                //}
                //else
                //{
                //    // list = _db.GF_ResolutionCenter.Where(x => x.Status == status && (x.CreatedDate.Value.Date >= fromdate.Date && x.CreatedDate.Value.Date <= todate.Date) && x.GolferID == LoginInfo.GolferUserId);
                //    list = _db.GF_ResolutionCenter.Where(x => x.Status == status && (x.CreatedDate >= fromdate && x.CreatedDate <= todate) && x.GolferID == LoginInfo.GolferUserId && (x.GF_Golfer.FirstName.Contains(username) || x.GF_Golfer.LastName.Contains(username)));
                //}
                // totalRecords = list.Count();

                listTemp = listTemp.Where(x => x.GolferID == LoginInfo.GolferUserId).ToList();
            }

            list = listTemp.AsQueryable();

            totalRecords = list.Count();
            return list.OrderBy(sortExpression + " " + sortDirection).Skip((pageIndex - 1) * pageSize).Take(pageSize);
        }


        /// <summary>
        /// Created By:Arun
        /// Get course User
        /// </summary>
        /// <returns></returns>
        public List<GF_CourseInfo> GetCourseInfo()
        {
            _db = new GolflerEntities();

            try
            {
                var lstGolferUser = _db.GF_GolferUser.Where(x => (x.GolferID ?? 0) == LoginInfo.GolferUserId).ToList();

                if (lstGolferUser.Count() > 0)
                {
                    return lstGolferUser.Select(y => new
                            {
                                name = y.GF_CourseInfo == null ? "" : y.GF_CourseInfo.COURSE_NAME,
                                ID = y.GF_CourseInfo == null ? 0 : GF_CourseInfo.ID
                            }).ToList().Select(z => new GF_CourseInfo
                                {
                                    ID = z.ID,
                                    COURSE_NAME = z.name
                                }).ToList();
                }
                else
                {
                    return new List<GF_CourseInfo>();
                }
            }
            catch
            {
                return new List<GF_CourseInfo>();
            }
        }



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
        public IQueryable<AllMessageHistory> GetResolutionMessagesHistory(long filterExpression, string sortExpression, string sortDirection, int pageIndex, int pageSize, ref int totalRecords)
        {
            _db = new GolflerEntities();
            IQueryable<AllMessageHistory> listAll;
            List<AllMessageHistory> objListMsg = new List<AllMessageHistory>();
            string mainGolferName = "";
            var listMsgHistory = _db.GF_ResolutionMessageHistory.Where(x => x.MessageID == filterExpression).ToList();
            var MainMsg = _db.GF_ResolutionCenter.Where(x => x.ID == filterExpression).FirstOrDefault();
            if (MainMsg != null)
            {
                AllMessageHistory objMsg = new AllMessageHistory();
                objMsg.ID = MainMsg.ID;
                objMsg.MsgHistoryId = 0;
                objMsg.MsgComments = MainMsg.FeedbackTest;
                objMsg.MsgStatus = MainMsg.Status;
                objMsg.MsgDate = Convert.ToDateTime(MainMsg.CreatedDate);
                //objMsg.ReplyBy = _db.GF_Golfer.Where(x => x.GF_ID == MainMsg.GolferID).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault();
                objMsg.ReplyBy = (MainMsg.GolferID ?? 0) != 0 ? (MainMsg.GF_Golfer.FirstName + " " + MainMsg.GF_Golfer.LastName) : GetAdminName(MainMsg.SenderID ?? 0, MainMsg.SenderType, _db);
                mainGolferName = objMsg.ReplyBy;
                objListMsg.Add(objMsg);
            }
            if (listMsgHistory.Count > 0)
            {
                foreach (var obj in listMsgHistory)
                {
                    AllMessageHistory objMsg = new AllMessageHistory();
                    objMsg.ID = Convert.ToInt64(obj.MessageID);
                    objMsg.MsgHistoryId = Convert.ToInt64(obj.ID);
                    objMsg.MsgComments = obj.Message;
                    objMsg.MsgStatus = obj.Status;
                    objMsg.MsgDate = Convert.ToDateTime(obj.CreatedDate);
                    objMsg.ReplyBy = CommonFunctions.GetUserNameMessageHistory(Convert.ToInt64(obj.ID));
                    objListMsg.Add(objMsg);
                }
            }


            listAll = objListMsg.OrderByDescending(x => x.MsgDate).AsQueryable();
            totalRecords = listAll.Count();
            return listAll.OrderBy(sortExpression + " " + sortDirection).Skip((pageIndex - 1) * pageSize).Take(pageSize);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public bool AddResolutionMessage(GF_ResolutionMessageHistory obj)
        {
            _db = new GolflerEntities();
            string utype = "";

            if (LoginInfo.IsGolferLoginUser)
            {
                utype = LoginInfo.GolferType;
            }
            else
            {
                utype = LoginInfo.LoginUserType;
            }

            bool isSendPushNotification = false;
            if (utype == UserType.Kitchen || utype == UserType.Cartie || utype == UserType.Ranger || utype == UserType.CourseAdmin ||
                utype == UserType.Proshop || utype == UserType.SuperAdmin || utype == UserType.Admin)
            // if (LoginInfo.LoginUserType == "CK" || LoginInfo.LoginUserType == "CC" || LoginInfo.LoginUserType == "CR" || LoginInfo.LoginUserType == "CA" || LoginInfo.LoginUserType == "CP")
            {//course....................Golfler/CourseAdmin/
                obj.LogUserID = LoginInfo.UserId;
                obj.UserType = LoginInfo.Type;
                isSendPushNotification = true;
            }
            else //if (LoginInfo.LoginUserType == "")
            {//Golfler/golfer/
                obj.LogUserID = LoginInfo.GolferUserId;
                obj.UserType = LoginInfo.GolferType;
                isSendPushNotification = false;
            }

            obj.CreatedDate = DateTime.Now;
            obj.IsActive = true;
            _db.GF_ResolutionMessageHistory.Add(obj);
            _db.SaveChanges();

            UnReadStatusResolutionMessage(obj.MessageID ?? 0);

            #region Push Notification

            //Case : Resolution Center Response
            //Sender : Super Admin/Course Admin/Proshop
            //Receiver : Golfer

            if (isSendPushNotification)
            {
                bool IsMessageToGolfer = true;
                var adminUsers = _db.GF_AdminUsers.FirstOrDefault(x => x.ID == obj.LogUserID);
                if (adminUsers != null)
                {
                    PushNotications pushNotications = new PushNotications();
                    pushNotications = new PushNotications();

                    pushNotications.SenderId = obj.LogUserID ?? 0;
                    pushNotications.SenderName = adminUsers.FirstName == null ? "Support Team" : adminUsers.FirstName + " " + adminUsers.LastName;

                    var resolutionCenter = _db.GF_ResolutionCenter.FirstOrDefault(x => x.ID == obj.MessageID);
                    var lstResoltionMessagesResult = GetResolutionMessagesByID(obj.MessageID ?? 0);

                    if (resolutionCenter != null)
                    {
                        if ((resolutionCenter.GolferID ?? 0) != 0)
                        {
                            pushNotications.ReceiverId = resolutionCenter.GolferID ?? 0;
                            pushNotications.ReceiverName = resolutionCenter.GF_Golfer.FirstName + " " + resolutionCenter.GF_Golfer.LastName;
                            pushNotications.pushMsgFrom = PushnoficationMsgFrom.Course;

                            #region Commented Code
                            //pushNotications.Message = "You got the response of resolution message.";
                            //var jsonString = new
                            //{
                            //    ScreenName = AppScreenName.ResolutionCenter,
                            //    Message = "You got the response of resolution message.",
                            //    Data = new
                            //        {
                            //            ID = lstResoltionMessagesResult.ID,
                            //            //Name = lstResoltionMessagesResult.Name,
                            //            //SentBy = lstResoltionMessagesResult.SentBy,
                            //            CourseID = lstResoltionMessagesResult.CourseID,
                            //            //Comment = lstResoltionMessagesResult.Comment,
                            //            CourseName = lstResoltionMessagesResult.CourseName,
                            //            Status = lstResoltionMessagesResult.Status,
                            //            //UserID = lstResoltionMessagesResult.UserID,
                            //            //Date = lstResoltionMessagesResult.Date,
                            //            //TotalMsgs = lstResoltionMessagesResult.TotalMsgs,
                            //            //ResolutionType = lstResoltionMessagesResult.ResolutionType,
                            //            //SentTo = lstResoltionMessagesResult.SentTo,
                            //            //Message = lstResoltionMessagesResult.Message,
                            //            //LogUserID = lstResoltionMessagesResult.LogUserID,
                            //            //MessageID = lstResoltionMessagesResult.MessageID,
                            //            //ReplyUserType = lstResoltionMessagesResult.ReplyUserType,
                            //            //ReplyBy = lstResoltionMessagesResult.ReplyBy
                            //        }
                            //};
                            //pushNotications.Message = JsonConvert.SerializeObject(jsonString);
                            #endregion

                            pushNotications.DeviceType = resolutionCenter.GF_Golfer.DeviceType;

                            if (resolutionCenter.GF_Golfer.AppVersion > 0)
                            {
                                var jString = new
                                {
                                    ScreenName = AppScreenName.ResolutionCenter,
                                    Message = "You got the response of resolution message.",
                                    Data = new
                                    {
                                        ID = lstResoltionMessagesResult.ID,
                                        //Name = lstResoltionMessagesResult.Name,
                                        //SentBy = lstResoltionMessagesResult.SentBy,
                                        CourseID = lstResoltionMessagesResult.CourseID,
                                        //Comment = lstResoltionMessagesResult.Comment,
                                        CourseName = lstResoltionMessagesResult.CourseName,
                                        Status = lstResoltionMessagesResult.Status,
                                        //UserID = lstResoltionMessagesResult.UserID,
                                        //Date = lstResoltionMessagesResult.Date,
                                        //TotalMsgs = lstResoltionMessagesResult.TotalMsgs,
                                        //ResolutionType = lstResoltionMessagesResult.ResolutionType,
                                        //SentTo = lstResoltionMessagesResult.SentTo,
                                        //Message = lstResoltionMessagesResult.Message,
                                        //LogUserID = lstResoltionMessagesResult.LogUserID,
                                        //MessageID = lstResoltionMessagesResult.MessageID,
                                        //ReplyUserType = lstResoltionMessagesResult.ReplyUserType,
                                        //ReplyBy = lstResoltionMessagesResult.ReplyBy
                                    }
                                };
                                string jsonString = JsonConvert.SerializeObject(jString);

                                if (pushNotications.DeviceType.ToLower() == "ios")
                                {
                                    pushNotications.Message = "You got the response of resolution message.";
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
                                    pushNotications.Message = "You got the response of resolution message.";
                                }
                                else
                                {
                                    pushNotications.Message = "\"You got the response of resolution message.\"";
                                }
                            }

                            SendRecieveNotification.callingPushNotification(pushNotications, IsMessageToGolfer);
                        }
                    }
                }
            }

            #endregion

            return true;
        }

        public ResoltionMessagesResult GetResolutionMessagesByID(long msgID)
        {
            try
            {
                IQueryable<GF_ResolutionCenter> list = null;

                List<GF_ResolutionCenter> listTemp = new List<GF_ResolutionCenter>();

                var lstTable = _db.GF_ResolutionCenter.Where(x => x.ID == msgID)
                     .ToList().AsEnumerable().Select((x => new GF_ResolutionCenter
                                                        {
                                                            ID = x.ID,
                                                            CourseID = x.CourseID,
                                                            GolferID = x.GolferID,
                                                            FeedbackTest = x.FeedbackTest,
                                                            SendTo = x.SendTo,
                                                            IsRead = x.IsRead,
                                                            Status = CommonFunctions.GetLatestStatus(x.ID, x.Status),
                                                            IsActive = x.IsActive,
                                                            CreatedDate = CommonFunctions.DateByCourseTimeZone(Convert.ToInt64(x.CourseID), Convert.ToDateTime(CommonFunctions.GetLatestDate(x.ID, Convert.ToDateTime(x.CreatedDate)))),
                                                            CreatedBy = x.CreatedBy,
                                                            ModifyBy = x.ModifyBy,
                                                            ModifyDate = x.ModifyDate,
                                                            SentBy = (x.GF_Golfer.FirstName + " " + x.GF_Golfer.LastName),
                                                            CourseName = x.GF_CourseInfo.COURSE_NAME,
                                                            Name = CommonFunctions.GetLatestUserName(x.ID, (x.GF_Golfer.FirstName + " " + x.GF_Golfer.LastName)),
                                                            Comment = x.FeedbackTest,
                                                            ResolutionType = x.ResolutionType
                                                        }
                                                    )).ToList();

                list = lstTable.AsQueryable();

                int TotalMsgs = list.Count();
                var listresultTemp = list;
                List<ResoltionMessagesResult> Resoresult = new List<ResoltionMessagesResult>();
                if (listresultTemp.Count() > 0)
                {
                    foreach (var obj in listresultTemp)
                    {
                        ResoltionMessagesResult objResoresult = new ResoltionMessagesResult();
                        objResoresult.ID = obj.ID;
                        objResoresult.Name = obj.Name;
                        objResoresult.SentBy = obj.SentBy;
                        objResoresult.CourseID = obj.CourseID;
                        objResoresult.Comment = obj.Comment;
                        objResoresult.CourseName = obj.CourseName;
                        objResoresult.Status = obj.Status;
                        objResoresult.UserID = obj.GolferID;
                        objResoresult.Date = obj.CreatedDate;
                        objResoresult.TotalMsgs = TotalMsgs;
                        objResoresult.ResolutionType = obj.ResolutionType;
                        objResoresult.SentTo = obj.SendTo;
                        Resoresult.Add(objResoresult);
                    }

                    return Resoresult.FirstOrDefault();
                }
                else
                {
                    return new ResoltionMessagesResult();
                }
            }
            catch (Exception ex)
            {
                return new ResoltionMessagesResult();
            }
        }

        /// <summary>
        /// Created By:Kiran Bala
        /// Created Date: 22 Arpil 2015
        /// Purpose: Get Resolution messages listing
        /// </summary>
        /// <param name="filterExpression"></param>
        /// <param name="sortExpression"></param>
        /// <param name="sortDirection"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalRecords"></param>
        /// <returns></returns>
        //public IQueryable<GF_ResolutionCenter> GetResolutionMessagesForAdmin(long filterExpression, string status, DateTime fromdate, DateTime todate, string username, string sortExpression, string sortDirection, int pageIndex, int pageSize, ref int totalRecords)
        //{
        //    _db = new GolflerEntities();
        //    IQueryable<GF_ResolutionCenter> list = null;


        //    if (filterExpression != 0)
        //    {
        //        list = _db.GF_ResolutionCenter.Where(x => x.CourseID == filterExpression && x.Status == status && (x.CreatedDate >= fromdate && x.CreatedDate <= todate) && (x.GF_Golfer.FirstName.Contains(username) || x.GF_Golfer.LastName.Contains(username)));
        //    }

        //    else
        //        list = _db.GF_ResolutionCenter.Where(x => x.Status == status && (x.CreatedDate >= fromdate && x.CreatedDate <= todate) && (x.GF_Golfer.FirstName.Contains(username) || x.GF_Golfer.LastName.Contains(username)));

        //    totalRecords = list.Count();




        //    return list.OrderBy(sortExpression + " " + sortDirection).Skip((pageIndex - 1) * pageSize).Take(pageSize);
        //}

        public IQueryable<GF_ResolutionCenter> GetResolutionMessagesForAdmin(string courseID, string golferID, string status, string fromdate, string todate, string username, string searchResolutionType, string sortExpression, string sortDirection, int pageIndex, int pageSize, ref int totalRecords)
        {
            _db = new GolflerEntities();
            IQueryable<GF_ResolutionCenter> list = null;
            List<GF_ResolutionCenter> listTemp = new List<GF_ResolutionCenter>();
            try
            {
                DateTime dtDate = DateTime.Parse(fromdate);
                DateTime dtToDate = DateTime.Parse(todate);


                //var lstTable=(from main in _db.GF_ResolutionCenter
                //              join reply in _db.GF_ResolutionMessageHistory on main.ID equals reply.MessageID
                //              where reply.

                // var lstTable = _db.GF_ResolutionCenter.Where(x => (LoginInfo.IsSuper ? x.SendTo == UserType.SuperAdmin : x.SendTo == UserType.Proshop))
                var lstTable = _db.GF_ResolutionCenter.Where(x => (x.SendTo == UserType.SuperAdmin || x.SendTo == UserType.Admin) ||
                                            ((LoginInfo.Type == UserType.SuperAdmin || LoginInfo.Type == UserType.Admin) ? x.SenderType.Contains(UserTypeChar.SuperAdminChar) : x.SenderType == LoginInfo.Type) &&
                                            EntityFunctions.TruncateTime(x.CreatedDate) >= EntityFunctions.TruncateTime(dtDate) &&
                                            EntityFunctions.TruncateTime(x.CreatedDate) <= EntityFunctions.TruncateTime(dtToDate))
                    .ToList().AsEnumerable()
                    .Select((x => new GF_ResolutionCenter
                        {
                            ID = x.ID,
                            CourseID = x.CourseID,
                            GolferID = x.GolferID ?? 0,
                            FeedbackTest = x.FeedbackTest,
                            SendTo = x.SendTo,
                            IsRead = x.IsRead,
                            IsReadByAdmin = x.IsReadByAdmin,
                            Status = CommonFunctions.GetLatestStatus(x.ID, x.Status),
                            IsActive = x.IsActive,
                            // CreatedDate = CommonFunctions.GetLatestDate(x.ID, Convert.ToDateTime(x.CreatedDate)),
                            CreatedDate = x.CreatedDate,
                            LatestCreatedDate = CommonFunctions.GetLatestDate(x.ID, Convert.ToDateTime(x.CreatedDate)),
                            CreatedBy = x.CreatedBy,
                            ModifyBy = x.ModifyBy,
                            ModifyDate = x.ModifyDate,
                            //CreatedDate = x.GF_ResolutionMessageHistory.OrderByDescending(y => y.CreatedDate).FirstOrDefault().CreatedDate,
                            //RecentName = CommonFunctions.GetLatestUserName(x.ID, (x.GF_Golfer.FirstName + " " + x.GF_Golfer.LastName)),
                            //Email = x.GF_Golfer.Email
                            sentBy = (x.GolferID ?? 0) != 0 ? (x.GF_Golfer.FirstName + " " + x.GF_Golfer.LastName) : GetAdminName(x.SenderID ?? 0, x.SenderType, _db),
                            LatestReplyBy = (x.GolferID ?? 0) != 0 ? (x.GF_Golfer.FirstName + " " + x.GF_Golfer.LastName) : GetAdminName(x.SenderID ?? 0, x.SenderType, _db),
                            courseName = x.GF_CourseInfo.COURSE_NAME,
                            Name = CommonFunctions.GetLatestUserName(x.ID, (x.GolferID ?? 0) != 0 ? (x.GF_Golfer.FirstName + " " + x.GF_Golfer.LastName) : GetAdminName(x.SenderID ?? 0, x.SenderType, _db)),
                            comment = x.FeedbackTest,
                            LatestComments = CommonFunctions.GetLatestCommentsFromMessageHistory(x.ID, x.FeedbackTest),
                            strResolutionType = CommonFunctions.GetResolutionType(x.ResolutionType),
                            SenderType = string.IsNullOrEmpty(x.SenderType) ? UserType.Golfer : x.SenderType
                        }
                    )).ToList();

                listTemp = lstTable;//_db.GF_ResolutionCenter.ToList();

                if (!string.IsNullOrEmpty(Convert.ToString(courseID)))
                {
                    Int64 intCourseId = Convert.ToInt64(courseID);
                    if (intCourseId != 0)
                    {
                        listTemp = listTemp.Where(x => x.CourseID == intCourseId).ToList();
                    }
                }

                if (!string.IsNullOrEmpty(status))
                {
                    listTemp = listTemp.Where(x => x.Status == status).ToList();
                }

                if (!string.IsNullOrEmpty(username))
                {
                    //listTemp = listTemp.Where(x => x.GF_Golfer.FirstName.Contains(username) || x.GF_Golfer.LastName.Contains(username)).ToList();
                    listTemp = listTemp.Where(x => x.sentBy.ToUpper().Contains(username.ToUpper()) || x.sentBy.ToUpper().Contains(username.ToUpper())).ToList();
                }

                if (!string.IsNullOrEmpty(searchResolutionType))
                {
                    if (searchResolutionType == Golfler.Models.ResolutionType.Praise)
                    {
                        listTemp = listTemp.Where(x => x.strResolutionType == Golfler.Models.ResolutionType.GetFullResolutionType(Golfler.Models.ResolutionType.Praise)).ToList();
                    }
                    if (searchResolutionType == Golfler.Models.ResolutionType.Others)
                    {
                        listTemp = listTemp.Where(x => x.strResolutionType == Golfler.Models.ResolutionType.GetFullResolutionType(Golfler.Models.ResolutionType.Others)).ToList();
                    }
                    if (searchResolutionType == Golfler.Models.ResolutionType.Complaint)
                    {
                        listTemp = listTemp.Where(x => x.strResolutionType == Golfler.Models.ResolutionType.GetFullResolutionType(Golfler.Models.ResolutionType.Complaint)).ToList();
                    }
                }

                list = listTemp.AsQueryable();

                totalRecords = list.Count();


            }
            catch (Exception ex)
            {
                ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
            }

            return list.OrderBy(sortExpression + " " + sortDirection).Skip((pageIndex - 1) * pageSize).Take(pageSize);
        }

        public string GetAdminName(long id, string type, GolflerEntities db)
        {
            var admin = db.GF_AdminUsers.FirstOrDefault(x => x.ID == id && x.Type == type);

            if (admin != null)
                return admin.FirstName + " " + admin.LastName;

            return "";
        }

        /// <summary>
        /// Created By:Arun
        /// Created Date:26 March 2015
        /// Purpose: Get Resolution messsage reply listing
        /// </summary>
        /// <param name="filterExpression"></param>
        /// <param name="sortExpression"></param>
        /// <param name="sortDirection"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalRecords"></param>
        /// <returns></returns>
        public IQueryable<GF_ResolutionMessageHistory> GetResolutionMessagesHistoryAdmin(long filterExpression, string sortExpression, string sortDirection, int pageIndex, int pageSize, ref int totalRecords)
        {
            _db = new GolflerEntities();
            IQueryable<GF_ResolutionMessageHistory> list;

            list = _db.GF_ResolutionMessageHistory.Where(x => x.MessageID == filterExpression);

            totalRecords = list.Count();
            return list.OrderBy(sortExpression + " " + sortDirection).Skip((pageIndex - 1) * pageSize).Take(pageSize);
        }

        /// <summary>
        /// Created By: Amit Kumar
        /// Created Date: 11 August 2015
        /// Purpose: Add message center by golfer.
        /// </summary>
        /// <param name="feedback"></param>
        /// <returns></returns>
        public bool AddGolferResolutionCenter(ResolutionMessages resolutionCenter)
        {
            try
            {
                _db = new GolflerEntities();

                GF_ResolutionCenter objResolutionCenter = new GF_ResolutionCenter();

                if (resolutionCenter.SendTo == "AL" && LoginInfo.IsGolferLoginUser)
                {
                    objResolutionCenter.SenderID = LoginInfo.GolferUserId;
                    objResolutionCenter.SenderType = LoginInfo.GolferType;
                    objResolutionCenter.GolferID = LoginInfo.GolferUserId;
                    objResolutionCenter.FeedbackTest = resolutionCenter.FeedbackTest;
                    objResolutionCenter.ResolutionType = resolutionCenter.ResolutionType;
                    objResolutionCenter.CourseID = resolutionCenter.CourseID;
                    objResolutionCenter.SendTo = UserType.SuperAdmin;
                    objResolutionCenter.IsRead = false;
                    objResolutionCenter.Status = StatusType.Active;
                    objResolutionCenter.IsActive = true;
                    objResolutionCenter.CreatedDate = DateTime.UtcNow;
                    objResolutionCenter.CreatedBy = LoginInfo.GolferUserId;
                    _db.GF_ResolutionCenter.Add(objResolutionCenter);
                    _db.SaveChanges();

                    objResolutionCenter.SenderID = LoginInfo.GolferUserId;
                    objResolutionCenter.SenderType = LoginInfo.GolferType;
                    objResolutionCenter.GolferID = LoginInfo.GolferUserId;
                    objResolutionCenter.FeedbackTest = resolutionCenter.FeedbackTest;
                    objResolutionCenter.ResolutionType = resolutionCenter.ResolutionType;
                    objResolutionCenter.CourseID = resolutionCenter.CourseID;
                    objResolutionCenter.SendTo = UserType.Proshop;
                    objResolutionCenter.IsRead = false;
                    objResolutionCenter.Status = StatusType.Active;
                    objResolutionCenter.IsActive = true;
                    objResolutionCenter.CreatedDate = DateTime.UtcNow;
                    objResolutionCenter.CreatedBy = LoginInfo.GolferUserId;
                    _db.GF_ResolutionCenter.Add(objResolutionCenter);
                    _db.SaveChanges();
                }
                else
                {
                    if (LoginInfo.IsGolferLoginUser)
                    {
                        objResolutionCenter.SenderID = LoginInfo.GolferUserId;
                        objResolutionCenter.SenderType = LoginInfo.GolferType;
                        objResolutionCenter.GolferID = LoginInfo.GolferUserId;
                        objResolutionCenter.CourseID = resolutionCenter.CourseID;
                        objResolutionCenter.CreatedBy = LoginInfo.GolferUserId;
                        objResolutionCenter.SendTo = resolutionCenter.SendTo;
                    }
                    else
                    {
                        objResolutionCenter.SenderID = LoginInfo.UserId;
                        objResolutionCenter.SenderType = LoginInfo.Type;
                        objResolutionCenter.CourseID = LoginInfo.CourseId;
                        objResolutionCenter.CreatedBy = LoginInfo.UserId;
                        objResolutionCenter.SendTo = UserType.SuperAdmin;
                    }

                    objResolutionCenter.FeedbackTest = resolutionCenter.FeedbackTest;
                    objResolutionCenter.ResolutionType = resolutionCenter.ResolutionType;
                    objResolutionCenter.IsRead = false;
                    objResolutionCenter.Status = StatusType.Active;
                    objResolutionCenter.IsActive = true;
                    objResolutionCenter.CreatedDate = DateTime.UtcNow;

                    _db.GF_ResolutionCenter.Add(objResolutionCenter);
                    _db.SaveChanges();
                }

                #region Asyn Call

                GF_ResolutionCenter syncObj = objResolutionCenter;
                invFeedback = new delFeedback(CallDelegatedFunctionsFeedback);
                invFeedback.BeginInvoke(syncObj, new AsyncCallback(CallbackDelegatedFunctionsFeedback), null);

                #endregion

                return true;
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Created By: Amit Kumar
        /// Created Date: 11 August 2015
        /// Purpose: Add message center by golfer.
        /// </summary>
        /// <param name="feedback"></param>
        /// <returns></returns>
        public bool AddCourseResolutionCenter(ResolutionMessages resolutionCenter)
        {
            try
            {
                _db = new GolflerEntities();

                GF_ResolutionCenter objResolutionCenter = new GF_ResolutionCenter();

                objResolutionCenter.SenderID = LoginInfo.UserId;
                objResolutionCenter.SenderType = LoginInfo.Type;
                objResolutionCenter.CourseID = resolutionCenter.CourseID;
                objResolutionCenter.CreatedBy = LoginInfo.UserId;
                objResolutionCenter.SendTo = UserType.CourseAdmin;
                objResolutionCenter.FeedbackTest = resolutionCenter.FeedbackTest;
                objResolutionCenter.ResolutionType = resolutionCenter.ResolutionType;
                objResolutionCenter.IsRead = false;
                objResolutionCenter.Status = StatusType.Active;
                objResolutionCenter.IsActive = true;
                objResolutionCenter.CreatedDate = DateTime.UtcNow;

                _db.GF_ResolutionCenter.Add(objResolutionCenter);
                _db.SaveChanges();

                #region Asyn Call

                GF_ResolutionCenter syncObj = objResolutionCenter;
                invFeedback = new delFeedback(CallDelegatedFunctionsFeedback);
                invFeedback.BeginInvoke(syncObj, new AsyncCallback(CallbackDelegatedFunctionsFeedback), null);

                #endregion

                return true;
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                return false;
            }
        }

        public void ReadStatusResolutionMessage(long id)
        {
            _db = new GolflerEntities();
            var resolutionMessage = _db.GF_ResolutionCenter.FirstOrDefault(x => x.ID == id);

            if (resolutionMessage != null)
            {
                if (LoginInfo.IsGolferLoginUser)
                    resolutionMessage.IsRead = true;
                else
                    resolutionMessage.IsReadByAdmin = true;

                _db.SaveChanges();
            }
        }

        public void UnReadStatusResolutionMessage(long id)
        {
            _db = new GolflerEntities();
            var resolutionMessage = _db.GF_ResolutionCenter.FirstOrDefault(x => x.ID == id);

            if (resolutionMessage != null)
            {
                if (LoginInfo.IsGolferLoginUser)
                    resolutionMessage.IsReadByAdmin = false;
                else
                    resolutionMessage.IsRead = false;

                _db.SaveChanges();
            }
        }

        public long GetUnReadResolutionMessages()
        {
            try
            {
                _db = new GolflerEntities();
                var countUnread = _db.GF_ResolutionCenter.Where(x => (((LoginInfo.LoginUserType == UserType.SuperAdmin || LoginInfo.LoginUserType == UserType.Admin) && (LoginInfo.CourseId <= 0)) ?
                    x.SendTo == UserType.SuperAdmin :
                        ((LoginInfo.LoginUserType == UserType.CourseAdmin || LoginInfo.LoginUserType == UserType.Proshop)) ?
                            ((x.SendTo == UserType.CourseAdmin || x.SendTo == UserType.Proshop) && x.CourseID == LoginInfo.CourseId) :
                                x.SendTo == "NA") && !(x.IsReadByAdmin ?? false));

                return countUnread.Count();
            }
            catch
            {
                return 0;
            }
        }

        #region Feedback Send Email

        public delegate void delFeedback(GF_ResolutionCenter syncObj);
        public static delFeedback invFeedback;

        public static void CallbackDelegatedFunctionsFeedback(IAsyncResult t)
        {
            try
            {
                invFeedback.EndInvoke(t);
            }
            catch (Exception ex)
            {
                List<string> msg = new List<string>();
                msg.Add("Exception in call back Feeedback: " + Convert.ToString(ex.Message));
                LogClass.WriteLog(msg);
            }
        }
        public static void CallDelegatedFunctionsFeedback(GF_ResolutionCenter syncObj)
        {
            var _db = new GolflerEntities();
            GF_ResolutionCenter objGolferController = new GF_ResolutionCenter();
            objGolferController.SendMsgToReceiver(Convert.ToString(syncObj.FeedbackTest), Convert.ToInt64(syncObj.GolferID), Convert.ToString(syncObj.SendTo), Convert.ToInt64(syncObj.CourseID));

            Int64 gid = 0;
            gid = Convert.ToInt64(syncObj.GolferID);

            var golf = _db.GF_Golfer.Where(x => x.GF_ID == gid).FirstOrDefault();
            if (golf != null)
            {
                objGolferController.SendAutoResponseToGolfer(syncObj.FeedbackTest, Convert.ToInt64(syncObj.GolferID), golf.Email, (golf.FirstName + " " + golf.LastName));
            }
        }

        public void SendMsgToReceiver(string comment, Int64 golferID, string sentTo, Int64 courseID)
        {
            try
            {
                _db = new GolflerEntities();

                string email = "";
                string name = "";
                IQueryable<GF_EmailTemplatesFields> templateFields = null;
                var param = EmailParams.GetEmailParams(ref _db, "Resolution Message", ref templateFields);



                if (sentTo.ToUpper() == UserType.SuperAdmin || sentTo.ToUpper() == UserType.Admin)
                {//if sent to admin/superadmin
                    #region send EMail

                    var admin = _db.GF_AdminUsers.Where(x => x.Type == "SA" || x.Type == "A").ToList();
                    foreach (var usr in admin)
                    {
                        email = usr.Email;
                        name = usr.FirstName + " " + usr.LastName;

                        if (!ApplicationEmails.SendResolutionMsgEMail(golferID, comment, email, name, param, ref templateFields))
                        {
                            // Message = String.Format(Resources.Resources.mailerror);
                            // return new Result { Status = 0, Error = "An error occured while sending mail.", record = null };
                        }
                        else
                        {
                            //   return new Result { Status = 1, Error = "Your new password is sent on your email address. Please login with new password.", record = null };
                        }
                    }

                    #endregion
                }
                else if (sentTo.ToUpper() == UserType.Proshop)
                {//if sent to course
                    #region Send Email


                    var admin = _db.GF_AdminUsers.Where(x => x.CourseId == courseID).ToList();

                    foreach (var usr in admin)
                    {
                        email = usr.Email;
                        name = usr.FirstName + " " + usr.LastName;

                        if (!ApplicationEmails.SendResolutionMsgEMail(golferID, comment, email, name, param, ref templateFields))
                        {
                            // Message = String.Format(Resources.Resources.mailerror);
                            // return new Result { Status = 0, Error = "An error occured while sending mail.", record = null };
                        }
                        else
                        {
                            //   return new Result { Status = 1, Error = "Your new password is sent on your email address. Please login with new password.", record = null };
                        }
                    }

                    #endregion
                }
                else
                {
                    #region send EMail

                    var admin = _db.GF_AdminUsers.Where(x => x.Type == "SA" || x.Type == "A").ToList();
                    foreach (var usr in admin)
                    {
                        email = usr.Email;
                        name = usr.FirstName + " " + usr.LastName;

                        if (!ApplicationEmails.SendResolutionMsgEMail(golferID, comment, email, name, param, ref templateFields))
                        {
                            // Message = String.Format(Resources.Resources.mailerror);
                            // return new Result { Status = 0, Error = "An error occured while sending mail.", record = null };
                        }
                        else
                        {
                            //   return new Result { Status = 1, Error = "Your new password is sent on your email address. Please login with new password.", record = null };
                        }
                    }

                    #endregion

                    #region Send Email

                    admin = _db.GF_AdminUsers.Where(x => x.CourseId == courseID).ToList();

                    foreach (var usr in admin)
                    {
                        email = usr.Email;
                        name = usr.FirstName + " " + usr.LastName;

                        if (!ApplicationEmails.SendResolutionMsgEMail(golferID, comment, email, name, param, ref templateFields))
                        {
                            // Message = String.Format(Resources.Resources.mailerror);
                            // return new Result { Status = 0, Error = "An error occured while sending mail.", record = null };
                        }
                        else
                        {
                            //   return new Result { Status = 1, Error = "Your new password is sent on your email address. Please login with new password.", record = null };
                        }
                    }

                    #endregion
                }

            }
            catch (Exception ex)
            {
                //   return new Result { Id = 0, Status = 0, Error = ex.Message, record = null };
            }
        }

        public void SendAutoResponseToGolfer(string comment, Int64 golferID, string email, string name)
        {
            try
            {
                _db = new GolflerEntities();

                IQueryable<GF_EmailTemplatesFields> templateFields = null;
                var param = EmailParams.GetEmailParams(ref _db, "Resolution Golfer Auto Response", ref templateFields);


                if (!ApplicationEmails.SendAutoResponseToGolfer(golferID, comment, email, name, param, ref templateFields))
                {
                    // Message = String.Format(Resources.Resources.mailerror);
                    // return new Result { Status = 0, Error = "An error occured while sending mail.", record = null };
                }
                else
                {
                    //   return new Result { Status = 1, Error = "Your new password is sent on your email address. Please login with new password.", record = null };
                }
            }
            catch (Exception ex)
            {
                //   return new Result { Id = 0, Status = 0, Error = ex.Message, record = null };
            }
        }


        #endregion
    }

    public class AllMessageHistory
    {
        public Int64 ID { get; set; }
        public Int64 MsgHistoryId { get; set; }
        public string ReplyBy { get; set; }
        public string courseName { get; set; }
        public DateTime MsgDate { get; set; }
        public string MsgComments { get; set; }
        public string MsgStatus { get; set; }

    }

    public partial class GF_ResolutionCenter
    {
        public string SentBy { get; set; }
        public string CourseName { get; set; }
        public string Comment { get; set; }
    }

    public class ResoltionMessagesResult
    {
        public string SentBy { get; set; }
        public string CourseName { get; set; }
        public string Name { get; set; }
        public string Comment { get; set; }
        public string Status { get; set; }
        public long ID { get; set; }
        public long? CourseID { get; set; }
        public long? UserID { get; set; }
        public DateTime? Date { get; set; }
        public string Message { get; set; }
        public long? LogUserID { get; set; }
        public long? MessageID { get; set; }
        public string ReplyUserType { get; set; }
        public int TotalMsgs { get; set; }
        public string SentTo { get; set; }
        public string ReplyBy { get; set; }
        public string ResolutionType { get; set; }
    }

    public class ResolutionMessages
    {
        public long CourseID { get; set; }

        [Required(ErrorMessage = "Required")]
        public string SendTo { get; set; }

        [Required(ErrorMessage = "Required")]
        public string FeedbackTest { get; set; }

        [Required(ErrorMessage = "Required")]
        public string ResolutionType { get; set; }
    }
}