using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GolferWebAPI.Models
{
    public partial class GF_ResolutionCenter
    {
        private GolflerEntities _db = null;

        public long UserID { get; set; }
        public string Type { get; set; }
        public string DDLStatus { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public string UserName { get; set; }
        public long Search { get; set; }
        public int PageNo { get; set; }
        public int Row { get; set; }
        public string SentBy { get; set; }
        public string CourseName { get; set; }
        public string Name { get; set; }
        public string Comment { get; set; }

        /// <summary>
        /// Created By: Amit Kumar
        /// Created Date: 17 April 2015
        /// Purpose: Add message center by golfer.
        /// </summary>
        /// <param name="feedback"></param>
        /// <returns></returns>
        public Result AddGolferResolutionCenter(GF_ResolutionCenter resolutionCenter)
        {
            try
            {
                _db = new GolflerEntities();

                #region Check Validations

                var lstCourse = _db.GF_CourseInfo.FirstOrDefault(x => x.ID == resolutionCenter.CourseID);
                if (lstCourse == null)
                {
                    return new Result { Id = 0, Status = 0, record = null, Error = "Invalid Course." };
                }

                var lstGolferUser = _db.GF_Golfer.FirstOrDefault(x => x.GF_ID == resolutionCenter.GolferID);
                if (lstGolferUser == null)
                {
                    return new Result { Id = 0, Status = 0, record = null, Error = "Invalid Golfer User." };
                }

                #endregion

                if (resolutionCenter.SendTo == "AL")
                {
                    resolutionCenter.SenderID = resolutionCenter.GolferID;
                    resolutionCenter.SenderType = UserType.Golfer;
                    resolutionCenter.SendTo = UserType.SuperAdmin;
                    resolutionCenter.IsRead = false;
                    resolutionCenter.Status = StatusType.Active;
                    resolutionCenter.IsActive = true;
                    resolutionCenter.CreatedDate = DateTime.UtcNow;
                    resolutionCenter.CreatedBy = resolutionCenter.GolferID;
                    _db.GF_ResolutionCenter.Add(resolutionCenter);
                    _db.SaveChanges();

                    resolutionCenter.SenderID = resolutionCenter.GolferID;
                    resolutionCenter.SenderType = UserType.Golfer;
                    resolutionCenter.SendTo = UserType.Proshop;
                    resolutionCenter.IsRead = false;
                    resolutionCenter.Status = StatusType.Active;
                    resolutionCenter.IsActive = true;
                    resolutionCenter.CreatedDate = DateTime.UtcNow;
                    resolutionCenter.CreatedBy = resolutionCenter.GolferID;
                    _db.GF_ResolutionCenter.Add(resolutionCenter);
                    _db.SaveChanges();
                }
                else
                {
                    resolutionCenter.SenderID = resolutionCenter.GolferID;
                    resolutionCenter.SenderType = UserType.Golfer;
                    resolutionCenter.IsRead = false;
                    resolutionCenter.Status = StatusType.Active;
                    resolutionCenter.IsActive = true;
                    resolutionCenter.CreatedDate = DateTime.UtcNow;
                    resolutionCenter.CreatedBy = resolutionCenter.GolferID;

                    _db.GF_ResolutionCenter.Add(resolutionCenter);
                    _db.SaveChanges();
                }

                return new Result
                {
                    Id = resolutionCenter.ID,
                    Status = 1,
                    Error = "Success",
                    record = new
                    {
                        resolutionCenter.ID,
                        resolutionCenter.GolferID,
                        resolutionCenter.FeedbackTest,
                        resolutionCenter.SendTo,
                        resolutionCenter.ResolutionType
                    }
                };

            }
            catch (Exception ex)
            {
                return new Result { Id = 0, Status = 0, record = null, Error = ex.Message };
            }
        }


        public Result GetResolutionMessages(long filterExpression, long userId, string userType, string status, string fromdate, string todate, string username, int pageIndex, int pageSize)
        {
            try
            {
                _db = new GolflerEntities();
                IQueryable<GF_ResolutionCenter> list = null;


                List<GF_ResolutionCenter> listTemp = new List<GF_ResolutionCenter>();

                var lstTable = _db.GF_ResolutionCenter.Where(x => (userType == "G" ? x.GolferID == userId : (x.SendTo == "CP" || x.SendTo == "CA")))
                     .ToList().AsEnumerable().Select((x =>
                                    new GF_ResolutionCenter
                                    {
                                        ID = x.ID,
                                        CourseID = x.CourseID,
                                        GolferID = x.GolferID,
                                        FeedbackTest = x.FeedbackTest,
                                        SendTo = x.SendTo,
                                        IsRead = x.IsRead,
                                        Status = CommonFunctions.GetLatestStatus(x.ID, x.Status),
                                        IsActive = x.IsActive,
                                        CreatedDate = CommonFunctions.TimeZoneDateTimeByCourseID(Convert.ToInt64( x.CourseID),Convert.ToDateTime( CommonFunctions.GetLatestDate(x.ID, Convert.ToDateTime(x.CreatedDate)))),
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

                listTemp = lstTable;//_db.GF_ResolutionCenter.ToList();

                if (!string.IsNullOrEmpty(Convert.ToString(filterExpression)))
                {
                    Int64 intCourseId = Convert.ToInt64(filterExpression);
                    if (intCourseId != 0)
                        listTemp = listTemp.Where(x => x.CourseID == intCourseId).ToList();
                }
                if (!string.IsNullOrEmpty(status))
                {
                    listTemp = listTemp.Where(x => x.Status == status).ToList();
                }

                if (!string.IsNullOrEmpty(username))
                {
                    listTemp = listTemp.Where(x => x.SentBy.ToUpper().Contains(username.ToUpper()) || x.SentBy.ToUpper().Contains(username.ToUpper())).ToList();
                }

                //CK  CC  CR  CA  CP
                if (userType == "CK" || userType == "CC" || userType == "CR" || userType == "CA" || userType == "CP")
                {


                    listTemp = listTemp.Where(x => x.CourseID == CourseID).ToList();
                }
                else if (userType == "G")
                {


                    listTemp = listTemp.Where(x => x.GolferID == UserID).ToList();
                }

                list = listTemp.AsQueryable();

                if (!string.IsNullOrEmpty(fromdate) && !string.IsNullOrEmpty(todate))
                {
                    DateTime dtDate = DateTime.Parse(fromdate);
                    DateTime dtToDate = DateTime.Parse(todate);

                    list = list.Where(x => x.CreatedDate.Value.Year >= dtDate.Year
                        && x.CreatedDate.Value.Month >= dtDate.Month
                        && x.CreatedDate.Value.Day >= dtDate.Day

                    && x.CreatedDate.Value.Year <= dtToDate.Year
                        && x.CreatedDate.Value.Month <= dtToDate.Month
                        && x.CreatedDate.Value.Day <= dtToDate.Day);
                }
                else if (!string.IsNullOrEmpty(fromdate))
                {
                    DateTime dtDate = DateTime.Parse(fromdate);

                    list = list.Where(x => x.CreatedDate.Value.Year >= dtDate.Year
                        && x.CreatedDate.Value.Month >= dtDate.Month
                        && x.CreatedDate.Value.Day >= dtDate.Day
                        );
                }
                else if (!string.IsNullOrEmpty(todate))
                {
                    DateTime dtToDate = DateTime.Parse(todate);

                    list = list.Where(x => x.CreatedDate.Value.Year <= dtToDate.Year
                        && x.CreatedDate.Value.Month <= dtToDate.Month
                        && x.CreatedDate.Value.Day <= dtToDate.Day
                        );

                }
                int TotalMsgs = list.Count();
                var listresultTemp = list.OrderByDescending(x => x.CreatedDate).Skip((pageIndex - 1) * pageSize).Take(pageSize);
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


                    return new Result
                    {
                        Id = 0,
                        Status = 1,
                        Error = "Success",
                        record = Resoresult

                    };
                }
                else
                {
                    return new Result { Id = 0, Status = 0, record = null, Error = "No Record found" };
                }
            }
            catch (Exception ex)
            {
                return new Result { Id = 0, Status = 0, record = null, Error = ex.Message };
            }
        }

        public delegate void delResolutionReply(GF_ResolutionMessageHistory objHistory);
        public static delResolutionReply invResolutionReply;

        public static void CallbackDelegatedFunctions(IAsyncResult t)
        {
            try
            {
                invResolutionReply.EndInvoke(t);
            }
            catch (Exception ex)
            {
                ////throw ex;
                //List<string> msg = new List<string>();
                //msg.Add("Exception in callback update locaton: " + Convert.ToString(ex.Message));
                //LogClass.WriteLog(msg);
            }
        }

        public static void CallDelegatedFunctions(GF_ResolutionMessageHistory objHistory)
        {
            try
            {
                GF_ResolutionCenter objRes = new GF_ResolutionCenter();
                objRes.ResolutionReply(objHistory.Message, objHistory.Status, objHistory.MessageID, objHistory.UserType, Convert.ToInt64(objHistory.LogUserID));
            }
            catch (Exception ex)
            {
                //List<string> msg = new List<string>();
                //msg.Add("Exception during Pace of play: " + Convert.ToString(ex.Message));
                //LogClass.WriteLog(msg);
                ////string m = ex.Message;
            }
        }


        public Result SendReply(GF_ResolutionMessageHistory objHistory)
        {
            try
            {
                _db = new GolflerEntities();
                objHistory.CreatedDate = DateTime.Now;
                objHistory.IsActive = true;
                _db.GF_ResolutionMessageHistory.Add(objHistory);
                _db.SaveChanges();
                //

                #region Update Pace of Play by Async call
                invResolutionReply = new delResolutionReply(CallDelegatedFunctions);
                invResolutionReply.BeginInvoke(objHistory, new AsyncCallback(CallbackDelegatedFunctions), null);
                #endregion
                //
                return new Result
                {
                    Id = objHistory.ID,
                    Status = 1,
                    Error = "Success",
                    record = objHistory

                };
            }
            catch (Exception ex)
            {
                return new Result { Id = 0, Status = 0, record = null, Error = ex.Message };
            }
        }

        public Result GetMessageHistory(long? msgId, int pageIndex, int pageSize)
        {
            _db = new GolflerEntities();
            IQueryable<GF_ResolutionMessageHistory> list;
            list = _db.GF_ResolutionMessageHistory.Where(x => x.MessageID == msgId);
            int TotalMsgs = list.Count();
            var listresultTemp = list.OrderBy(x => x.CreatedDate).Skip((pageIndex - 1) * pageSize).Take(pageSize);
            List<ResoltionMessagesResult> Resoresult = new List<ResoltionMessagesResult>();
            IQueryable<GF_ResolutionCenter> listinitial;
            listinitial = _db.GF_ResolutionCenter.Where(x => x.ID == msgId);
            if (pageIndex == 1)
            {
                if (listinitial.Count() > 0)
                {
                    foreach (var obj in listinitial)
                    {

                        ResoltionMessagesResult objResoresult = new ResoltionMessagesResult();
                        objResoresult.ID = obj.ID;
                        objResoresult.Status = obj.Status;
                        objResoresult.LogUserID = obj.GolferID;
                        objResoresult.Date = obj.CreatedDate;
                        objResoresult.Message = obj.FeedbackTest;
                        objResoresult.MessageID = obj.ID;
                        objResoresult.ReplyUserType = "G";
                        objResoresult.TotalMsgs = TotalMsgs;
                        objResoresult.ReplyBy = GetReplyByName(obj.GolferID, "G");
                        Resoresult.Add(objResoresult);
                    }
                }
            }
            if (listresultTemp.Count() > 0)
            {
                foreach (var obj in listresultTemp)
                {
                    ResoltionMessagesResult objResoresult = new ResoltionMessagesResult();
                    objResoresult.ID = obj.ID;
                    objResoresult.Status = obj.Status;
                    objResoresult.LogUserID = obj.LogUserID;
                    objResoresult.Date = obj.CreatedDate;
                    objResoresult.Message = obj.Message;
                    objResoresult.MessageID = obj.MessageID;
                    objResoresult.ReplyUserType = obj.UserType;
                    objResoresult.TotalMsgs = TotalMsgs;
                    objResoresult.ReplyBy = GetReplyByName(obj.LogUserID, obj.UserType);
                    Resoresult.Add(objResoresult);
                }


                return new Result
                {
                    Id = 0,
                    Status = 1,
                    Error = "Success",
                    record = Resoresult

                };
            }
            else
            {
                //  return new Result { Id = 0, Status = 0, record = null, Error = "No Record found" };
                return new Result
                {
                    Id = 0,
                    Status = 1,
                    Error = "Success",
                    record = Resoresult

                };
            }
        }

        public void ResolutionReply(string comment, string status, long? msgID, string type, long UserId)
        {

            try
            {

                #region send email
                IQueryable<GF_EmailTemplatesFields> templateFields = null;
                GolflerEntities Db = new GolflerEntities();

                var param = EmailParams.GetEmailParams(ref Db, "Resolution Reply", ref templateFields);

                string email = "";
                string name = "";

                if (param != null)
                {
                    long id = 0;
                    if (type == UserType.Kitchen || type == UserType.Cartie || type == UserType.Ranger || type == UserType.CourseAdmin || type == UserType.Proshop)
                    {//course....................Golfler/CourseAdmin/

                        #region CourseAdmin Login...send mail to golfer

                        id = UserId;

                        var mainRes = Db.GF_ResolutionCenter.Where(x => x.ID == msgID).FirstOrDefault();
                        if (mainRes != null)
                        {
                            email = mainRes.GF_Golfer.Email.ToString();
                            name = mainRes.GF_Golfer.FirstName.ToString();
                            if (!ApplicationEmails.GolferResolutionReply(id, comment, status, email, name, param, ref templateFields))
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
                    else if (type == UserType.Golfer)
                    {//Golfler/golfer/

                        #region Golfer Login

                        id = UserId;

                        var mainRes = Db.GF_ResolutionCenter.Where(x => x.ID == msgID).FirstOrDefault();

                        if (mainRes != null)
                        {
                            if (mainRes.SendTo.ToUpper() == "SA" || mainRes.SendTo.ToUpper() == "A")
                            {//sent to super admin/ Admin
                                #region If send to Superadmin

                                var admin = Db.GF_AdminUsers.Where(x => x.Type == "SA" || x.Type == "A").ToList();
                                foreach (var usr in admin)
                                {
                                    email = usr.Email;
                                    name = usr.FirstName + " " + usr.LastName;

                                    // if (!ApplicationEmails.GolferResolutionReply(id, comment, status, LoginInfo.GolferEmail, LoginInfo.UserName, param, ref templateFields))
                                    if (!ApplicationEmails.GolferResolutionReply(id, comment, status, email, name, param, ref templateFields))
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
                            else if (mainRes.SendTo.ToUpper() == "CP")
                            {
                                #region if sent to courseadmin
                                Int64 cId = Convert.ToInt64(mainRes.CourseID);

                                var admin = Db.GF_AdminUsers.Where(x => x.CourseId == cId).ToList();
                                foreach (var usr in admin)
                                {
                                    email = usr.Email;
                                    name = usr.FirstName + " " + usr.LastName;

                                    if (!ApplicationEmails.GolferResolutionReply(id, comment, status, email, name, param, ref templateFields))
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

                        #endregion
                    }
                    else if (type == UserType.Admin || type == UserType.SuperAdmin)
                    {

                        #region if golfer admin login

                        id = UserId;

                        var mainRes = Db.GF_ResolutionCenter.Where(x => x.ID == msgID).FirstOrDefault();
                        if (mainRes != null)
                        {
                            email = mainRes.GF_Golfer.Email.ToString();
                            name = mainRes.GF_Golfer.FirstName.ToString();
                            if (!ApplicationEmails.GolferResolutionReply(id, comment, status, email, name, param, ref templateFields))
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
                else
                {
                    //  return new Result { Status = 0, Error = "An error occured while sending mail.", record = null };
                }
                #endregion
            }
            catch (Exception ex)
            {
                //   return new Result { Id = 0, Status = 0, Error = ex.Message, record = null };
            }
        }
        public string GetReplyByName(long? id, string type)
        {
            string result = "";
            try
            {
                _db = new GolflerEntities();
                if (type == "G")
                {
                    var obj = _db.GF_Golfer.FirstOrDefault(x => x.GF_ID == id);
                    if (obj != null)
                    {
                        result = obj.FirstName + " " + obj.LastName;
                    }
                    else
                    {
                        result = "";
                    }
                }
                else
                {
                    var obj = _db.GF_AdminUsers.FirstOrDefault(x => x.ID == id);
                    if (obj != null)
                    {
                        result = obj.FirstName + " " + obj.LastName;
                    }
                    else
                    {
                        result = "";
                    }
                }
            }
            catch { }
            return result;
        }
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
    public partial class GF_ResolutionMessageHistory
    {
        public int PageNo { get; set; }
        public int Row { get; set; }
    }
}