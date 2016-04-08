using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ErrorLibrary;
using System.IO;
using System.Configuration;
using Newtonsoft.Json;
//using System.Drawing;

namespace GolferWebAPI.Models
{
    public partial class Messages
    {

        //public string Type { get; set; }
        //public string Image { get; set; }
        //public string ImagePath { get; set; }
        public GolflerEntities _db = null;


        #region Save Message to DB
        public Base SaveMessage(GF_Messages message)
        {
            bool IsMessageToGolfer = false;
            long SenderId = 0;
            long ReceiverId = 0;
            string SenderName = "";
            string ReceiverName = "";
            string DeviceType = "";
            try
            {
                _db = new GolflerEntities();

                message.CreatedDate = DateTime.UtcNow;
                message.Status = false;
                message.Type = "0";
                long? courseId = 0;
                try
                {
                    if (Convert.ToString(message.IsMessagesFromGolfer) == "1")
                    {
                        courseId = _db.GF_GolferUser.FirstOrDefault(x => x.GolferID == message.MsgFrom).CourseID;
                        if (courseId > 0)
                        {
                            var parentCourse = _db.GF_CourseInfo.FirstOrDefault(x => x.ID == courseId && !(x.IsClubHouse ?? true));
                            if (parentCourse != null)
                            {
                                courseId = parentCourse.ClubHouseID;
                            }
                        }
                    }
                    else
                    {
                        courseId = _db.GF_AdminUsers.FirstOrDefault(x => x.ID == message.MsgFrom).CourseId;
                    }
                }
                catch
                {

                }
                if (Convert.ToString(message.IsMessagesFromGolfer) == "1")
                {
                    var objChatUserFrom = _db.GF_Golfer.FirstOrDefault(x => x.GF_ID == message.MsgFrom);
                    SenderId = objChatUserFrom.GF_ID;
                    SenderName = objChatUserFrom.FirstName + " " + objChatUserFrom.LastName;


                }
                else
                {
                    var objChatAdminUserFrom = _db.GF_AdminUsers.FirstOrDefault(x => x.ID == message.MsgFrom);
                    SenderId = objChatAdminUserFrom.ID;
                    SenderName = objChatAdminUserFrom.FirstName + " " + objChatAdminUserFrom.LastName;

                }

                if (message.MsgToList.Contains(","))
                {

                    string[] MsgToListArr = message.MsgToList.Split(',');
                    for (int i = 0; i < MsgToListArr.Length; i++)
                    {
                        message.MsgTo = Convert.ToInt64(MsgToListArr[i]);
                        if (IsUserBlocked(message.MsgFrom, message.MsgTo, message.IsMessagesToGolfer, message.IsMessagesFromGolfer, courseId))
                        {
                            string screenName = "";
                            int AppVersion = 0;
                            if (Convert.ToString(message.IsMessagesToGolfer) == "1")
                            {
                                var objChatUserTo = _db.GF_Golfer.FirstOrDefault(x => x.GF_ID == message.MsgTo);
                                ReceiverId = objChatUserTo.GF_ID;
                                ReceiverName = objChatUserTo.FirstName + " " + objChatUserTo.LastName;
                                IsMessageToGolfer = true;
                                DeviceType = objChatUserTo.DeviceType;
                                screenName = AppScreenName.Messaging;
                                AppVersion = objChatUserTo.AppVersion ?? 0;
                            }
                            else
                            {
                                var objChatAdminUserTo = _db.GF_AdminUsers.FirstOrDefault(x => x.ID == message.MsgTo);
                                ReceiverId = objChatAdminUserTo.ID;
                                ReceiverName = objChatAdminUserTo.FirstName + " " + objChatAdminUserTo.LastName;
                                IsMessageToGolfer = false;
                                DeviceType = objChatAdminUserTo.DeviceType;
                                screenName = AppScreenName.CourseMessaging;
                                AppVersion = objChatAdminUserTo.AppVersion ?? 0;
                            }

                            #region Push notification calling
                            // Start
                            // Push Notification Code
                            PushNotications objPush = new PushNotications();
                            objPush.SenderId = SenderId;
                            objPush.SenderName = SenderName;
                            objPush.ReceiverId = ReceiverId;
                            objPush.ReceiverName = ReceiverName;
                            objPush.pushMsgFrom = message.IsMessagesFromGolfer == "1" ? PushnoficationMsgFrom.Golfer : PushnoficationMsgFrom.Course;
                            objPush.DeviceType = DeviceType;

                            if (AppVersion > 0)
                            {
                                var jString = new
                                {
                                    ScreenName = screenName,
                                    Message = SenderName + " sent you a message : " + message.Message,
                                    Data = new
                                    {
                                        ReceiverId = SenderId,
                                        ReceiverName = SenderName,
                                        IsMessagesToGolfer = message.IsMessagesFromGolfer,
                                        IsMessagesFromGolfer = message.IsMessagesToGolfer
                                    }
                                };
                                string jsonString = JsonConvert.SerializeObject(jString);

                                if (objPush.DeviceType.ToLower() == "ios")
                                {
                                    objPush.Message = SenderName + " sent you a message : " + message.Message;
                                    objPush.iosMessageJson = jsonString;
                                }
                                else
                                {
                                    objPush.Message = jsonString;
                                }
                            }
                            else
                            {
                                if (objPush.DeviceType.ToLower() == "ios")
                                {
                                    objPush.Message = SenderName + " sent you a message : " + message.Message;
                                }
                                else
                                {
                                    objPush.Message = "\"" + SenderName + " sent you a message : " + message.Message + "\"";
                                }
                            }

                            invProfileApplicationCompliance = new delProfileApplicationCompliance(CallPushNotify);
                            invProfileApplicationCompliance.BeginInvoke(objPush, IsMessageToGolfer, new AsyncCallback(CallPushNotify), null);

                            #endregion
                        }
                    }
                }
                else
                {
                    message.MsgTo = Convert.ToInt64(message.MsgToList);
                    if (IsUserBlocked(message.MsgFrom, message.MsgTo, message.IsMessagesToGolfer, message.IsMessagesFromGolfer, courseId))
                    {
                        string screenName = "";
                        int AppVersion = 0;
                        if (Convert.ToString(message.IsMessagesToGolfer) == "1")
                        {
                            var objChatUserTo = _db.GF_Golfer.FirstOrDefault(x => x.GF_ID == message.MsgTo);
                            ReceiverId = objChatUserTo.GF_ID;
                            ReceiverName = objChatUserTo.FirstName + " " + objChatUserTo.LastName;
                            IsMessageToGolfer = true;
                            DeviceType = objChatUserTo.DeviceType;
                            screenName = AppScreenName.Messaging;
                            AppVersion = objChatUserTo.AppVersion ?? 0;
                        }
                        else
                        {
                            var objChatAdminUserTo = _db.GF_AdminUsers.FirstOrDefault(x => x.ID == message.MsgTo);
                            ReceiverId = objChatAdminUserTo.ID;
                            ReceiverName = objChatAdminUserTo.FirstName + " " + objChatAdminUserTo.LastName;
                            IsMessageToGolfer = false;
                            DeviceType = objChatAdminUserTo.DeviceType;
                            screenName = AppScreenName.CourseMessaging;
                            AppVersion = objChatAdminUserTo.AppVersion ?? 0;
                        }
                        #region Push notification calling

                        // Start
                        // Push Notification Code
                        PushNotications objPush = new PushNotications();
                        objPush.SenderId = SenderId;
                        objPush.SenderName = SenderName;
                        objPush.ReceiverId = ReceiverId;
                        objPush.ReceiverName = ReceiverName;
                        objPush.pushMsgFrom = message.IsMessagesFromGolfer == "1" ? PushnoficationMsgFrom.Golfer : PushnoficationMsgFrom.Course;
                        objPush.DeviceType = string.IsNullOrEmpty(DeviceType) ? string.Empty : DeviceType;

                        if (AppVersion > 0)
                        {
                            var jString = new
                            {
                                ScreenName = screenName,
                                Message = SenderName + " sent you a message : " + message.Message,
                                Data = new
                                {
                                    ReceiverId = SenderId,
                                    ReceiverName = SenderName,
                                    IsMessagesToGolfer = message.IsMessagesFromGolfer,
                                    IsMessagesFromGolfer = message.IsMessagesToGolfer
                                }
                            };
                            string jsonString = JsonConvert.SerializeObject(jString);

                            if (objPush.DeviceType.ToLower() == "ios")
                            {
                                objPush.Message = SenderName + " sent you a message : " + message.Message;
                                objPush.iosMessageJson = jsonString;
                            }
                            else
                            {
                                objPush.Message = jsonString;
                            }
                        }
                        else
                        {
                            if (objPush.DeviceType.ToLower() == "ios")
                            {
                                objPush.Message = SenderName + " sent you a message : " + message.Message;
                            }
                            else
                            {
                                objPush.Message = "\"" + SenderName + " sent you a message : " + message.Message + "\"";
                            }
                        }
                        
                        invProfileApplicationCompliance = new delProfileApplicationCompliance(CallPushNotify);
                        invProfileApplicationCompliance.BeginInvoke(objPush, IsMessageToGolfer, new AsyncCallback(CallPushNotify), null);

                        #endregion
                    }

                }


                message.CourseId = courseId;
                if (message.MsgToList.Contains(","))
                {
                    string[] MsgToListArr = message.MsgToList.Split(',');
                    for (int i = 0; i < MsgToListArr.Length; i++)
                    {

                        message.MsgTo = Convert.ToInt64(MsgToListArr[i]);
                        if (IsUserBlocked(message.MsgFrom, message.MsgTo, message.IsMessagesToGolfer, message.IsMessagesFromGolfer,courseId))
                        {
                            _db.GF_Messages.Add(message);
                            _db.SaveChanges();
                        }
                        else
                        { }
                    }
                }
                else
                {
                    message.MsgTo = Convert.ToInt64(message.MsgToList);
                    if (IsUserBlocked(message.MsgFrom, message.MsgTo, message.IsMessagesToGolfer, message.IsMessagesFromGolfer, courseId))
                    {
                        _db.GF_Messages.Add(message);
                        _db.SaveChanges();
                    }
                }
                return new Base { Status = 1, Error = "Message Saved Successfully." };
            }
            catch (Exception ex)
            {
                // ErrorClass.WriteLog(ex, HttpContext.Current.Request);
                return new Base { Status = 0, Error = ex.Message };
            }
        }
        #endregion

        #region Get Chat Listing
        public MessageResult GetMessageListing(MsgObject msg)
        {
            try
            {
                _db = new GolflerEntities();
                decimal PgSize = ConfigClass.MessageListingPageSize;
                string DefaultImagePath = ConfigurationManager.AppSettings["DefaultImagePath"];
                string GolferImagePath = ConfigurationManager.AppSettings["GolferImagePath"];
                string CourseImagePath = ConfigurationManager.AppSettings["CourseImagePath"];

                var result = _db.GF_SP_GetMessageListing(msg.PgNo, PgSize, msg.MsgFrom, msg.MsgTo, msg.IsMessagesFromGolfer, DefaultImagePath, GolferImagePath, CourseImagePath).ToList();
                if (result.Count > 0)
                {

                    List<GF_SP_GetMessageListing_Result> msgs1 = new List<GF_SP_GetMessageListing_Result>();

                    foreach (var obj in result)
                    {
                        var hourMin = msg.Offset.Split(':');
                        if (hourMin != null)
                        {
                            TimeSpan tDate = new TimeSpan(Convert.ToInt16(hourMin[0]), Convert.ToInt16(hourMin[1]), 0);
                            obj.CreatedDate = msg.Timezone == 1 ? obj.CreatedDate.Value.Add(tDate).Add(new TimeSpan(1, 0, 0)) : obj.CreatedDate.Value.Add(tDate);
                        }
                        msgs1.Add(obj);
                    }


                    IEnumerable<object> msgs = msgs1.Select(x => new
                    {
                        x.id,
                        x.Message,
                        x.CreatedDate,
                        x.PageNo,
                        x.IsSender,
                        x.Status,
                        x.TotalPages,
                        x.SenderImg,
                        x.ReceiverImg,
                        x.MsgFrom,
                        x.MsgTo,
                        x.Name,
                        x.Type,
                        x.OnlineStatus




                    }).ToList();
                    return new MessageResult { Error = "Chat retrieved successfully.", Status = 1, Messages = msgs };

                }
                return new MessageResult { Error = "No record found.", Status = 0 };
            }

            catch (Exception ex)
            {
                ErrorClass.WriteLog(ex, HttpContext.Current.Request);
                return new MessageResult { Error = ex.Message, Status = 0 };
            }


        }

        public MessageResult GetPesronMessageListing(MsgObject msg)
        {
            try
            {
                _db = new GolflerEntities();
                decimal PgSize = ConfigClass.MessageListingPageSize;

                string DefaultImagePath = ConfigurationManager.AppSettings["DefaultImagePath"];
                string GolferImagePath = ConfigurationManager.AppSettings["GolferImagePath"];
                string CourseImagePath = ConfigurationManager.AppSettings["CourseImagePath"];

                var result = _db.GF_SP_GetPersonWiseMessageListing(1, PgSize, msg.MsgFrom, msg.IsMessagesFromGolfer, DefaultImagePath, GolferImagePath, CourseImagePath).ToList();
                if (result.Count > 0)
                {

                    // List<GF_SP_GetPersonWiseMessageListing_Result> msgs1 = new List<GF_SP_GetPersonWiseMessageListing_Result>();
                    IEnumerable<object> msgs = result.Select(x => new
                    {
                        id = x.UserId,
                        FriendType = x.UserType,
                        x.Name,
                        x.image,
                        x.LastMessageSent,
                        isOnline = x.IsOnline,
                        TimeElapsed = ((long)DateTime.UtcNow.Subtract(Convert.ToDateTime(x.LastMessageSent)).TotalMinutes).ToString()
                    }).ToList();
                    return new MessageResult { Error = "Chat retrieved successfully.", Status = 1, Messages = msgs };

                }
                return new MessageResult { Error = "No record found.", Status = 0 };
            }

            catch (Exception ex)
            {
                ErrorClass.WriteLog(ex, HttpContext.Current.Request);
                return new MessageResult { Error = ex.Message, Status = 0 };
            }


        }
        #endregion

        #region Update Message Read/UnRead Status
        /// <summary>
        /// Created By:Veera
        /// Created Date:26 March. 2015
        /// Purpose: Update Message status
        /// </summary>
        /// <param name="msgid"></param>
        /// <returns></returns>
        public Base UpdatemessageStatus(GF_Messages msg)
        {
            try
            {
                _db = new GolflerEntities();
                _db.GF_Messages.Where(x => x.ID == msg.ID).ToList().ForEach(x => x.Status = true);
                _db.SaveChanges();
                return new Base { Status = 1, Error = "Success" };
            }
            catch (Exception ex)
            {
                ErrorClass.WriteLog(ex, HttpContext.Current.Request);
                return new Base { Status = 0, Error = ex.Message };
            }
        }
        #endregion

        #region Get unread messages's users count
        public MessageCountResult UserUnreadMessages(GF_Messages msg)
        {
            try
            {
                _db = new GolflerEntities();
                var count = _db.GF_Messages.Where(x => x.MsgTo == msg.MsgTo && x.IsMessagesToGolfer == msg.IsMessagesToGolfer && x.Status == false).Count();
                //.GroupBy(y => y.MsgFrom)
                return new MessageCountResult { Status = 1, Error = "Success", UserCount = count };
            }
            catch (Exception ex)
            {
                ErrorClass.WriteLog(ex, HttpContext.Current.Request);
                return new MessageCountResult { Status = 0, Error = ex.Message, UserCount = 0 };
            }
        }

        #endregion

        #region PushNotification
        public delegate void delProfileApplicationCompliance(PushNotications objMessage, bool IsGolfer);
        public static delProfileApplicationCompliance invProfileApplicationCompliance;
        public static void CallPushNotify(PushNotications objMessage, bool IsGolfer)
        {

            PushNotications objPush = new PushNotications();
            string newmessage = objMessage.Message;
            if (objMessage.DeviceType.ToLower() == "ios")
            {
                objPush.ApplePush(newmessage, objMessage.iosMessageJson, objMessage.ReceiverId, IsGolfer, objMessage.pushMsgFrom);
            }
            else
            {
                objPush.AndroidPush(newmessage, objMessage.ReceiverId, IsGolfer, objMessage.pushMsgFrom);
            }
        }
        public static void CallPushNotify(IAsyncResult t)
        {
            List<string> objList = new List<string>();
            try
            {
                invProfileApplicationCompliance.EndInvoke(t);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region IsBlocked
        public bool IsUserBlocked(long? msgFrom, long? msgTo, string isMsgToGolfer, string isMsgFromGolfer, long? courseId)
        {
            bool result = false;
            try
            {
                _db = new GolflerEntities();
                if (isMsgToGolfer == "1" && isMsgFromGolfer == "1")
                {
                    var count = _db.GF_BlockUserList.Where(x => x.IsGolferUser == true && x.IsBlockedGolferUser == true && ((x.BlockedUserId == msgFrom && x.UserId == msgTo) || (x.UserId == msgFrom && x.BlockedUserId == msgTo))).Count();
                    if (count == 0)
                    {
                        result = true;
                    }
                    else
                    {
                        result = false;
                    }
                }
                else if (isMsgFromGolfer == "0" && isMsgToGolfer == "0")// check if a sender course user is blocked or check if a receiver course user is blocked
                {
                    var count = _db.GF_CourseBlockUserList.Where(x => x.BlockedUserId == msgFrom && x.IsBlockedGolfer == false && x.CourseId == courseId).Count();
                    if (count == 0)
                    {
                        var countInner = _db.GF_CourseBlockUserList.Where(x => x.BlockedUserId == msgTo && x.IsBlockedGolfer == false && x.CourseId == courseId).Count();
                        if (countInner == 0)
                        {
                            result = true;
                        }
                        else
                        {
                            result = false;
                        }
                    }
                    else
                    {
                        result = false;
                    }
                }
                else if (isMsgFromGolfer == "1" && isMsgToGolfer == "0")
                {
                    
                    var countInner = _db.GF_CourseBlockUserList.Where(x => x.BlockedUserId == msgTo && x.IsBlockedGolfer == false && x.CourseId == courseId).Count();
                    if (countInner == 0)
                    {
                        result = true;
                    }
                    else
                    {
                        result = false;
                    }
                }
                else if (isMsgFromGolfer == "0" && isMsgToGolfer == "1")
                {

                    var countInner = _db.GF_CourseBlockUserList.Where(x => x.BlockedUserId == msgFrom && x.IsBlockedGolfer == false && x.CourseId == courseId).Count();
                    if (countInner == 0)
                    {
                        result = true;
                    }
                    else
                    {
                        result = false;
                    }
                }
                else
                {
                    result = true;
                }
                return result;
            }
            catch (Exception ex)
            {

                return result;
            }
        }
        #endregion
    }
    public class MsgObject
    {
        public long? PgNo { get; set; }
        public long? PgSize { get; set; }
        public long? MsgFrom { get; set; }
        public long? MsgTo { get; set; }
        public string IsMessagesFromGolfer { get; set; }
        public int Timezone { get; set; }
        public string Offset { get; set; }

    }
    public partial class GF_Messages
    {
        public string MsgToList { get; set; }
    }

}