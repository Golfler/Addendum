using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Golfler.Models
{
    [MetadataType(typeof(CourseBuilderMetaData))]
    public partial class GF_CourseBuilder
    {
        public GF_CourseBuilder courseBuilder { get; private set; }
        public GF_CourseBuilderHolesDetail holeDetail { get; private set; }
        public List<courseHoleDetail> courseHoleDetail { get; private set; }

        protected GolflerEntities Db = null;

        public bool Active { get { return Status == StatusType.Active; } set { Status = (value) ? Status = StatusType.Active : Status = StatusType.InActive; } }
        public string Message { get; private set; }


        public bool UpdateRecDates(string coursebuildertitle, ref Int64 CourseBuilderId)
        {
            try
            {
                List<DateTime?> lstRecDates = new List<DateTime?>();

                lstRecDates = (List<DateTime?>)HttpContext.Current.Session["RecurringTempDatesForSubmit"];


                Db = new GolflerEntities();


                string RecComments = "";

                if (!(string.IsNullOrEmpty(Convert.ToString(HttpContext.Current.Session["RecurringDescriptionForSubmit"]))))
                {
                    RecComments = Convert.ToString(HttpContext.Current.Session["RecurringDescriptionForSubmit"]);
                }


                #region Co-Ordinates setting for Course Admin
                Int64 cid = Convert.ToInt64(CourseBuilderId);
                courseBuilder = new GF_CourseBuilder();
                if (CourseBuilderId > 0)
                {
                    courseBuilder = Db.GF_CourseBuilder.FirstOrDefault(x => x.ID == cid);
                }



                if (courseBuilder.ID != 0) //Update Data
                {
                    courseBuilder.Title = Convert.ToString(coursebuildertitle);
                    courseBuilder.CoordinateType = Golfler.Models.CoordinateType.SetTemporary;


                    try
                    {
                        courseBuilder.DateFrom = lstRecDates.OrderBy(x => x.Value).FirstOrDefault(); //Convert.ToDateTime(dateFrom);
                    }
                    catch
                    {
                    }
                    try
                    {
                        courseBuilder.DateTo = lstRecDates.OrderBy(x => x.Value).LastOrDefault(); // Convert.ToDateTime(dateTo);
                    }
                    catch
                    {
                    }

                    courseBuilder.ModifyDate = DateTime.Now;
                    courseBuilder.ModifyBy = LoginInfo.UserId;
                    courseBuilder.BuildBy = "CA";
                    if (!string.IsNullOrEmpty(Convert.ToString(RecComments)))
                    {
                        courseBuilder.Comments = RecComments;
                    }
                    Db.SaveChanges();

                    if (courseBuilder.ID > 0)
                    {
                        #region Insert Recurring Dates
                        if (lstRecDates != null)
                        {
                            if (lstRecDates.Count > 0)
                            {
                                // delete old
                                var lstOlddates = Db.GF_CourseBuilderRecDates.Where(x => x.CourseBuilderId == courseBuilder.ID).ToList();
                                foreach (var olddate in lstOlddates)
                                {
                                    olddate.Status = StatusType.Delete;
                                    Db.SaveChanges();
                                }

                                // insert new
                                foreach (var dat in lstRecDates)
                                {
                                    var dateDetail = new GF_CourseBuilderRecDates();
                                    dateDetail.CourseId = LoginInfo.CourseId;
                                    dateDetail.RecDate = dat;
                                    dateDetail.CourseBuilderId = courseBuilder.ID;
                                    dateDetail.Status = StatusType.Active;
                                    Db.GF_CourseBuilderRecDates.Add(dateDetail);
                                    Db.SaveChanges();
                                }
                            }
                        }
                        #endregion

                    }
                    CourseBuilderId = courseBuilder.ID;

                }
                else
                {
                    courseBuilder = new GF_CourseBuilder();
                    courseBuilder.CourseID = LoginInfo.CourseId;
                    courseBuilder.Title = Convert.ToString(coursebuildertitle);
                    courseBuilder.CoordinateType = Golfler.Models.CoordinateType.SetTemporary;
                    courseBuilder.Comments = RecComments;

                    try
                    {
                        courseBuilder.DateFrom = lstRecDates.OrderBy(x => x.Value).FirstOrDefault(); // Convert.ToDateTime(dateFrom);
                    }
                    catch
                    {
                    }
                    try
                    {
                        courseBuilder.DateTo = lstRecDates.OrderBy(x => x.Value).LastOrDefault(); // Convert.ToDateTime(dateTo);
                    }
                    catch
                    {
                    }

                    courseBuilder.Status = StatusType.Active;
                    courseBuilder.IsActive = true;
                    courseBuilder.CreatedDate = DateTime.Now;
                    courseBuilder.CreatedBy = LoginInfo.UserId;
                    courseBuilder.BuildBy = "CA";

                    Db.GF_CourseBuilder.Add(courseBuilder);
                    Db.SaveChanges();


                    #region Insert Recurring Dates
                    if (lstRecDates != null)
                    {
                        if (lstRecDates.Count > 0)
                        {
                            foreach (var dat in lstRecDates)
                            {
                                var dateDetail = new GF_CourseBuilderRecDates();
                                dateDetail.CourseId = LoginInfo.CourseId;
                                dateDetail.RecDate = dat;
                                dateDetail.CourseBuilderId = courseBuilder.ID;
                                dateDetail.Status = StatusType.Active;
                                Db.GF_CourseBuilderRecDates.Add(dateDetail);
                                Db.SaveChanges();
                            }
                        }
                    }
                    #endregion
                    CourseBuilderId = courseBuilder.ID;
                }
                #endregion
                return true;
            }
            catch (Exception ex)
            {
                ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
                Message = "Error occured. Please try again.";
                return false;
            }
        }


        public bool SaveCourseBuilder(string buildBy, Int64 courseid, string golferComments, string title, string coordinateType,
            string dateFrom, string dateTo, string CourseBuilderId, string status, List<courseHoleDetail> objHole,
            string callfrom, ref string Message, ref Int64 intNewCourseBuilderId)
        {
            try
            {
                List<DateTime?> lstRecDates = new List<DateTime?>();
                if (coordinateType.Contains(Golfler.Models.CoordinateType.SetTemporary))
                {
                    lstRecDates = (List<DateTime?>)HttpContext.Current.Session["RecurringTempDatesForSubmit"];
                }

                Db = new GolflerEntities();
                string strBuildby = "CA";

                if (buildBy == "CourseAdmin")
                {
                    string RecComments = "";
                    if (coordinateType.Contains(Golfler.Models.CoordinateType.SetTemporary))
                    {
                        if (!(string.IsNullOrEmpty(Convert.ToString(HttpContext.Current.Session["RecurringDescriptionForSubmit"]))))
                        {
                            RecComments = Convert.ToString(HttpContext.Current.Session["RecurringDescriptionForSubmit"]);
                        }
                    }

                    strBuildby = UserType.BuildByCourseAdmin;

                    #region Co-Ordinates setting for Course Admin
                    if (Golfler.Models.CoordinateType.LoadOriginal == coordinateType)
                    {
                        courseBuilder = Db.GF_CourseBuilder.FirstOrDefault(x => x.CourseID == courseid &&
                            x.CoordinateType == Golfler.Models.CoordinateType.LoadOriginal && x.BuildBy == strBuildby && x.Status != StatusType.Delete &&
                            x.IsActive == true);
                    }
                    else
                    {
                        Int64 cbid = 0;
                        try
                        {
                            cbid = Convert.ToInt64(CourseBuilderId);
                        }
                        catch
                        {
                            cbid = 0;
                        }
                        if (cbid == 0)
                        {
                            courseBuilder = Db.GF_CourseBuilder.FirstOrDefault(x => x.CourseID == courseid && x.Title == title &&
                                x.Status != StatusType.Delete && x.IsActive == true);
                        }
                        else
                        {
                            courseBuilder = Db.GF_CourseBuilder.FirstOrDefault(x => x.ID == cbid);
                        }
                    }

                    if (courseBuilder != null) //Update Data
                    {
                        courseBuilder.Title = title;
                        courseBuilder.CoordinateType = coordinateType;

                        if (coordinateType.Contains(Golfler.Models.CoordinateType.SetTemporary))
                        {
                            try
                            {
                                courseBuilder.DateFrom = lstRecDates.OrderBy(x => x.Value).FirstOrDefault(); //Convert.ToDateTime(dateFrom);
                            }
                            catch
                            {
                            }
                            try
                            {
                                courseBuilder.DateTo = lstRecDates.OrderBy(x => x.Value).LastOrDefault(); // Convert.ToDateTime(dateTo);
                            }
                            catch
                            {
                            }
                        }
                        courseBuilder.ModifyDate = DateTime.Now;
                        courseBuilder.ModifyBy = LoginInfo.UserId;
                        courseBuilder.BuildBy = strBuildby;
                        if (!string.IsNullOrEmpty(Convert.ToString(RecComments)))
                        {
                            courseBuilder.Comments = RecComments;
                        }
                        Db.SaveChanges();

                        #region Delete all holes if Save all
                        if (callfrom == "saveallatonce")
                        {
                            List<GF_CourseBuilderHolesDetail> allholedetail = Db.GF_CourseBuilderHolesDetail.Where(x => x.CourseBuilderID == courseBuilder.ID).ToList();

                            foreach (var singleHole in allholedetail)
                            {
                                Db.GF_CourseBuilderHolesDetail.Remove(singleHole);
                                Db.SaveChanges();
                            }
                        }

                        #endregion

                        if (courseBuilder.ID > 0)
                        {

                            #region Insert New Hole Detail

                            //foreach (var item in objHole)
                            //{
                            for (int cnt = 1; cnt <= 18; cnt++)
                            {
                                List<courseHoleDetail> lstSingleHole = new List<courseHoleDetail>();
                                string cntString = Convert.ToString(cnt);
                                lstSingleHole = objHole.Where(x => x.holeNumber == cntString).ToList();

                                if (lstSingleHole.Count > 0)
                                {

                                    List<GF_CourseBuilderHolesDetail> holedetail = Db.GF_CourseBuilderHolesDetail
                                        .Where(x => x.CourseBuilderID == courseBuilder.ID && x.HoleNumber == cnt).ToList();

                                    #region Delete Old Records

                                    foreach (var holede in holedetail)
                                    {
                                        Db.GF_CourseBuilderHolesDetail.Remove(holede);
                                        Db.SaveChanges();
                                    }

                                    #endregion



                                    foreach (var item in lstSingleHole)
                                    {

                                        string strDragitemtype = "";

                                        if (item.dragItem.Contains("White Tee"))
                                        {
                                            strDragitemtype = DragItemType.WhiteTee;
                                        }
                                        else
                                        {
                                            if (item.dragItem.Contains("Red Tee"))
                                            {
                                                strDragitemtype = DragItemType.RedTee;
                                            }
                                            else
                                            {
                                                if (item.dragItem.Contains("Blue Tee"))
                                                {
                                                    strDragitemtype = DragItemType.BlueTee;
                                                }
                                                else
                                                {
                                                    strDragitemtype = DragItemType.WhiteFlag;
                                                }
                                            }
                                        }


                                        holeDetail = new GF_CourseBuilderHolesDetail();
                                        holeDetail.CourseBuilderID = courseBuilder.ID;
                                        holeDetail.HoleNumber = Convert.ToInt32(item.holeNumber);
                                        holeDetail.Latitude = item.latitude;
                                        holeDetail.Longitude = item.longitude;
                                        holeDetail.DragItemType = strDragitemtype;
                                        holeDetail.Discription = item.dragItem;

                                        Db.GF_CourseBuilderHolesDetail.Add(holeDetail);
                                        Db.SaveChanges();
                                    }
                                }
                            }
                            #endregion

                            #region Insert Recurring Dates
                            if (lstRecDates != null)
                            {
                                if (lstRecDates.Count > 0)
                                {
                                    // delete old
                                    var lstOlddates = Db.GF_CourseBuilderRecDates.Where(x => x.CourseBuilderId == courseBuilder.ID).ToList();
                                    foreach (var olddate in lstOlddates)
                                    {
                                        olddate.Status = StatusType.Delete;
                                        Db.SaveChanges();
                                    }

                                    // insert new
                                    if (coordinateType.Contains(Golfler.Models.CoordinateType.SetTemporary))
                                    {
                                        foreach (var dat in lstRecDates)
                                        {
                                            var dateDetail = new GF_CourseBuilderRecDates();
                                            dateDetail.CourseId = courseid;// LoginInfo.CourseId;
                                            dateDetail.RecDate = dat;
                                            dateDetail.CourseBuilderId = courseBuilder.ID;
                                            dateDetail.Status = StatusType.Active;
                                            Db.GF_CourseBuilderRecDates.Add(dateDetail);
                                            Db.SaveChanges();
                                        }
                                    }

                                }
                            }
                            #endregion

                        }
                        intNewCourseBuilderId = courseBuilder.ID;
                        Message = "update";
                    }
                    else
                    {
                        courseBuilder = new GF_CourseBuilder();
                        courseBuilder.CourseID = courseid;// LoginInfo.CourseId;
                        courseBuilder.Title = title;
                        courseBuilder.CoordinateType = coordinateType;
                        courseBuilder.Comments = RecComments;

                        if (coordinateType.Contains(Golfler.Models.CoordinateType.SetTemporary))
                        {
                            try
                            {
                                courseBuilder.DateFrom = lstRecDates.OrderBy(x => x.Value).FirstOrDefault(); // Convert.ToDateTime(dateFrom);
                            }
                            catch
                            {
                            }
                            try
                            {
                                courseBuilder.DateTo = lstRecDates.OrderBy(x => x.Value).LastOrDefault(); // Convert.ToDateTime(dateTo);
                            }
                            catch
                            {
                            }
                        }
                        courseBuilder.Status = StatusType.Active;
                        courseBuilder.IsActive = true;
                        courseBuilder.CreatedDate = DateTime.Now;
                        courseBuilder.CreatedBy = LoginInfo.UserId;
                        courseBuilder.BuildBy = strBuildby;

                        Db.GF_CourseBuilder.Add(courseBuilder);
                        Db.SaveChanges();

                        #region Insert Hole Detail

                        if (courseBuilder.ID > 0)
                        {
                            foreach (var item in objHole)
                            {
                                holeDetail = new GF_CourseBuilderHolesDetail();
                                holeDetail.CourseBuilderID = courseBuilder.ID;
                                holeDetail.HoleNumber = Convert.ToInt32(item.holeNumber);
                                holeDetail.Latitude = item.latitude;
                                holeDetail.Longitude = item.longitude;

                                string strDragitemtype = "";

                                if (item.dragItem.Contains("White Tee"))
                                {
                                    strDragitemtype = DragItemType.WhiteTee;
                                }
                                else
                                {
                                    if (item.dragItem.Contains("Red Tee"))
                                    {
                                        strDragitemtype = DragItemType.RedTee;
                                    }
                                    else
                                    {
                                        if (item.dragItem.Contains("Blue Tee"))
                                        {
                                            strDragitemtype = DragItemType.BlueTee;
                                        }
                                        else
                                        {
                                            strDragitemtype = DragItemType.WhiteFlag;
                                        }
                                    }
                                }

                                holeDetail.DragItemType = strDragitemtype;
                                holeDetail.Discription = item.dragItem;

                                Db.GF_CourseBuilderHolesDetail.Add(holeDetail);
                                Db.SaveChanges();
                            }
                        }

                        #endregion

                        #region Insert Recurring Dates
                        if (coordinateType.Contains(Golfler.Models.CoordinateType.SetTemporary))
                        {
                            foreach (var dat in lstRecDates)
                            {
                                var dateDetail = new GF_CourseBuilderRecDates();
                                dateDetail.CourseId = courseid;// LoginInfo.CourseId;
                                dateDetail.RecDate = dat;
                                dateDetail.CourseBuilderId = courseBuilder.ID;
                                dateDetail.Status = StatusType.Active;
                                Db.GF_CourseBuilderRecDates.Add(dateDetail);
                                Db.SaveChanges();
                            }
                        }
                        #endregion
                        intNewCourseBuilderId = courseBuilder.ID;
                        Message = "add";
                    }
                    #endregion
                    HttpContext.Current.Session["RecurringTempDatesForSubmit"] = null;
                    HttpContext.Current.Session["RecurringDescriptionForSubmit"] = null;
                }
                else
                {
                    if (buildBy == "Golfer")
                    {
                        strBuildby = UserType.Golfer;

                        #region Co-ordinate settings for Golfer
                        courseBuilder = Db.GF_CourseBuilder.Where(x => x.CourseID == courseid && x.BuildBy == strBuildby && x.CreatedBy == LoginInfo.GolferUserId).FirstOrDefault();
                        if (courseBuilder != null)
                        {
                            // update
                            courseBuilder.Status = StatusType.InActive;
                            courseBuilder.IsActive = false;

                            courseBuilder.ModifyDate = DateTime.Now;
                            courseBuilder.ModifyBy = LoginInfo.GolferUserId;

                            courseBuilder.Comments = golferComments;
                            Db.SaveChanges();

                            #region Insert Hole Detail

                            if (courseBuilder.ID > 0)
                            {
                                #region delete old hole details
                                var lstHoles = Db.GF_CourseBuilderHolesDetail.Where(x => x.CourseBuilderID == courseBuilder.ID).ToList();
                                foreach (var objHoleDelete in lstHoles)
                                {
                                    Db.GF_CourseBuilderHolesDetail.Remove(objHoleDelete);
                                }
                                Db.SaveChanges();
                                #endregion

                                foreach (var item in objHole)
                                {
                                    holeDetail = new GF_CourseBuilderHolesDetail();
                                    holeDetail.CourseBuilderID = courseBuilder.ID;
                                    holeDetail.HoleNumber = Convert.ToInt32(item.holeNumber);
                                    holeDetail.Latitude = item.latitude;
                                    holeDetail.Longitude = item.longitude;

                                    string strDragitemtype = "";

                                    if (item.dragItem.Contains("White Tee"))
                                    {
                                        strDragitemtype = DragItemType.WhiteTee;
                                    }
                                    else
                                    {
                                        if (item.dragItem.Contains("Red Tee"))
                                        {
                                            strDragitemtype = DragItemType.RedTee;
                                        }
                                        else
                                        {
                                            if (item.dragItem.Contains("Blue Tee"))
                                            {
                                                strDragitemtype = DragItemType.BlueTee;
                                            }
                                            else
                                            {
                                                strDragitemtype = DragItemType.WhiteFlag;
                                            }
                                        }
                                    }

                                    holeDetail.DragItemType = strDragitemtype;
                                    holeDetail.Discription = item.dragItem;

                                    Db.GF_CourseBuilderHolesDetail.Add(holeDetail);
                                    Db.SaveChanges();
                                }
                            }

                            #endregion
                        }
                        else
                        {
                            courseBuilder = new GF_CourseBuilder();
                            courseBuilder.CourseID = courseid;
                            courseBuilder.Title = "By Golfer";
                            courseBuilder.CoordinateType = coordinateType;

                            courseBuilder.Status = StatusType.InActive;
                            courseBuilder.IsActive = false;
                            courseBuilder.CreatedDate = DateTime.Now;
                            courseBuilder.CreatedBy = LoginInfo.GolferUserId;
                            courseBuilder.ModifyDate = DateTime.Now;
                            courseBuilder.ModifyBy = LoginInfo.GolferUserId;
                            courseBuilder.BuildBy = strBuildby;
                            courseBuilder.Comments = golferComments;

                            Db.GF_CourseBuilder.Add(courseBuilder);
                            Db.SaveChanges();

                            #region Insert Hole Detail

                            if (courseBuilder.ID > 0)
                            {
                                foreach (var item in objHole)
                                {
                                    holeDetail = new GF_CourseBuilderHolesDetail();
                                    holeDetail.CourseBuilderID = courseBuilder.ID;
                                    holeDetail.HoleNumber = Convert.ToInt32(item.holeNumber);
                                    holeDetail.Latitude = item.latitude;
                                    holeDetail.Longitude = item.longitude;

                                    string strDragitemtype = "";

                                    if (item.dragItem.Contains("White Tee"))
                                    {
                                        strDragitemtype = DragItemType.WhiteTee;
                                    }
                                    else
                                    {
                                        if (item.dragItem.Contains("Red Tee"))
                                        {
                                            strDragitemtype = DragItemType.RedTee;
                                        }
                                        else
                                        {
                                            if (item.dragItem.Contains("Blue Tee"))
                                            {
                                                strDragitemtype = DragItemType.BlueTee;
                                            }
                                            else
                                            {
                                                strDragitemtype = DragItemType.WhiteFlag;
                                            }
                                        }
                                    }

                                    holeDetail.DragItemType = strDragitemtype;
                                    holeDetail.Discription = item.dragItem;

                                    Db.GF_CourseBuilderHolesDetail.Add(holeDetail);
                                    Db.SaveChanges();
                                }
                            }

                            #endregion
                        }
                        intNewCourseBuilderId = courseBuilder.ID;
                        Message = "add";

                        #region send mail to Super Admin
                        try
                        {
                            string mailresult = "";

                            //Int64 courseidForParams = 0;
                            //if (!string.IsNullOrEmpty(Convert.ToString(LoginInfo.CourseId)))
                            //{
                            //    courseidForParams = Convert.ToInt64(LoginInfo.CourseId);
                            //}


                            IQueryable<GF_EmailTemplatesFields> templateFields = null;
                            var param = EmailParams.GetEmailParamsNew(ref Db, EmailTemplateName.golferCoordinateSuggestion, ref templateFields, courseid, LoginInfo.LoginType, ref mailresult);

                            if (mailresult == "") // means Parameters are OK
                            {
                                if (ApplicationEmails.SuggestedCoordinateSend(ref Db, courseBuilder, param, ref templateFields, ref mailresult))
                                {
                                    // Do steps for Mail Send successful
                                }
                                else
                                {
                                    // Do steps for Mail Failure. Mail failure reason can be find on "mailresult"
                                }
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

                        #endregion


                    }
                    else
                    {
                        if (buildBy == "SuperAdmin")
                        {
                            string StatusForMail = "";
                            if (status == ApproveStatusType.Approve)
                            {
                                StatusForMail = "Approved";
                                var orgCourseBuildId = Db.GF_CourseBuilder.Where(x => x.CourseID == courseid && x.CoordinateType == "O" && x.BuildBy == "CA" && x.Status == StatusType.Active).Select(x => x.ID).FirstOrDefault();
                                #region Delete Old Records

                                var results = from c in Db.GF_CourseBuilderHolesDetail
                                              where c.CourseBuilderID == orgCourseBuildId
                                              select c;

                                foreach (var item in results)
                                {
                                    Db.GF_CourseBuilderHolesDetail.Remove(item);
                                }

                                Db.SaveChanges();

                                #endregion

                                Int64 tempCoursebuildid = Convert.ToInt64(CourseBuilderId);
                                var courseholes = Db.GF_CourseBuilderHolesDetail.Where(x => x.CourseBuilderID == tempCoursebuildid).ToList();

                                foreach (var x in courseholes)
                                {
                                    holeDetail = new GF_CourseBuilderHolesDetail();
                                    holeDetail.CourseBuilderID = orgCourseBuildId;
                                    holeDetail.HoleNumber = Convert.ToInt32(x.HoleNumber);
                                    holeDetail.Latitude = x.Latitude;
                                    holeDetail.Longitude = x.Longitude;


                                    holeDetail.DragItemType = x.DragItemType;
                                    holeDetail.Discription = x.Discription;

                                    Db.GF_CourseBuilderHolesDetail.Add(holeDetail);
                                    Db.SaveChanges();
                                }


                                courseBuilder = new GF_CourseBuilder();
                                Int64 cid = tempCoursebuildid;
                                courseBuilder = Db.GF_CourseBuilder.Where(x => x.ID == cid).FirstOrDefault();
                                courseBuilder.IsActive = true;
                                courseBuilder.Status = StatusType.Active;
                                Db.SaveChanges();
                            }
                            if (status == ApproveStatusType.Reject)
                            {
                                StatusForMail = "Rejected";
                                courseBuilder = new GF_CourseBuilder();
                                Int64 cid = Convert.ToInt64(CourseBuilderId);
                                courseBuilder = Db.GF_CourseBuilder.Where(x => x.ID == cid).FirstOrDefault();
                                courseBuilder.IsActive = false;
                                courseBuilder.Status = StatusType.Delete;
                                Db.SaveChanges();
                            }

                            intNewCourseBuilderId = Convert.ToInt64(CourseBuilderId);
                            Message = "update";

                            if (status == ApproveStatusType.Reject || status == ApproveStatusType.Approve)
                            {
                                #region send mail to Golfer
                                try
                                {
                                    string mailresult = "";

                                    Int64 courseidForParams = 0;
                                    if (!string.IsNullOrEmpty(Convert.ToString(LoginInfo.CourseId)))
                                    {
                                        courseidForParams = Convert.ToInt64(LoginInfo.CourseId);
                                    }

                                    IQueryable<GF_EmailTemplatesFields> templateFields = null;
                                    var param = EmailParams.GetEmailParamsNew(ref Db, EmailTemplateName.SuggestedCoordinateStatus, ref templateFields, courseidForParams, LoginInfo.LoginType, ref mailresult);

                                    if (mailresult == "") // means Parameters are OK
                                    {
                                        if (ApplicationEmails.SuggestedCoordinateStatus(ref Db, courseBuilder, StatusForMail, param, ref templateFields, ref mailresult))
                                        {
                                            // Do steps for Mail Send successful
                                        }
                                        else
                                        {
                                            // Do steps for Mail Failure. Mail failure reason can be find on "mailresult"
                                        }
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


                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
                Message = "Error occured. Please try again.";
                return false;
            }
        }

        /// <summary>
        /// To show Course Listing for Golfer suggestion on Co-Ordinates
        /// </summary>
        /// <param name="filterExpression"></param>
        /// <param name="partnerType"></param>
        /// <param name="sortExpression"></param>
        /// <param name="sortDirection"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalRecords"></param>
        /// <returns></returns>
        public IQueryable<GF_CourseInfo> GetCoursesInfo(string filterExpression, string CourseType, string partnerType, string sortExpression, string sortDirection, int pageIndex, int pageSize, ref int totalRecords)
        {
            IQueryable<GF_CourseInfo> list;

            Db = new GolflerEntities();
            if (!String.IsNullOrWhiteSpace(filterExpression))
                list = Db.GF_CourseInfo.Where(x => (x.COURSE_NAME.ToLower().Contains(filterExpression.ToLower()) ||
                    x.CITY.ToLower().Contains(filterExpression.ToLower()) ||
                    x.STATE.ToLower().Contains(filterExpression.ToLower()) ||
                    x.Country.ToLower().Contains(filterExpression.ToLower())));
            else
                list = Db.GF_CourseInfo.OrderByDescending(x => x.ID);

            if (!string.IsNullOrWhiteSpace(partnerType))
                list = list.Where(x => x.PartnershipStatus == partnerType);

            if (CourseType == SuggestionType.Suggested)
            {
                var lstSugCourses = Db.GF_CourseBuilder.Where(x => x.BuildBy == UserType.BuildByGolfer && x.CreatedBy == LoginInfo.GolferUserId).Select(x => x.CourseID).ToList();

                list = list.Where(x => lstSugCourses.Contains(x.ID));
            }
            else
            {
                if (CourseType == SuggestionType.NonSuggested)
                {
                    var lstSugCourses = Db.GF_CourseBuilder.Where(x => x.Status != StatusType.Delete && x.BuildBy == UserType.BuildByGolfer && x.CreatedBy == LoginInfo.GolferUserId).Select(x => x.CourseID).ToList();

                    var list2 = list.Where(x => lstSugCourses.Contains(x.ID));
                    list = list.Except(list2);
                }
            }

            totalRecords = list.Count();

            return list.OrderBy(sortExpression + " " + sortDirection).Skip((pageIndex - 1) * pageSize).Take(pageSize);
        }

        public IEnumerable<courseBuilder> getCourseBuilder(long id, Int64 courseid, Int16 intholenumber, string coordinatetype)
        {
            Db = new GolflerEntities();
            string CoOrdinateStatusInDB = "";
            List<GF_CourseBuilder> lstCourseBuilder = new List<GF_CourseBuilder>();



            if (coordinatetype == "new")
            {
                string coordType = Golfler.Models.CoordinateType.LoadOriginal;
                string recDates = "";
                List<DateTime?> lstRecDates = new List<DateTime?>();
                lstRecDates = (List<DateTime?>)HttpContext.Current.Session["RecurringTempDatesForSubmit"];

                if (lstRecDates != null)
                {
                    if (lstRecDates.Count > 0)
                    {
                        coordType = Golfler.Models.CoordinateType.SetTemporary;

                        if (!string.IsNullOrEmpty(Convert.ToString(HttpContext.Current.Session["RecurringDescriptionForSubmit"])))
                        {
                            recDates = Convert.ToString(HttpContext.Current.Session["RecurringDescriptionForSubmit"]);
                        }
                    }
                }

                var lstHoleDetail = Db.GF_CourseInfo.Where(x => x.ID == courseid).ToList().Select(x => new courseBuilder
                {
                    coursebuilderid = 0,
                    title = "",
                    courseName = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(x.COURSE_NAME.ToLower()),
                    CoordinateType = coordType,
                    DateFrom = "",
                    DateTo = "",
                    Comments = recDates,
                    HoleDetail = null
                });
                return lstHoleDetail;
            }
            else
            {
                if (coordinatetype == "newRecord")
                {
                    string coordType = Golfler.Models.CoordinateType.LoadOriginal;
                    var lstHoleDetail = Db.GF_CourseInfo.Where(x => x.ID == courseid).ToList().Select(x => new courseBuilder
                    {
                        coursebuilderid = 0,
                        title = "",
                        courseName = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(x.COURSE_NAME.ToLower()),
                        CoordinateType = coordType,
                        DateFrom = "",
                        DateTo = "",
                        Comments = "",
                        HoleDetail = null
                    });
                    return lstHoleDetail;
                }
                else
                {
                    if (id <= 0)
                    {
                        CoOrdinateStatusInDB = "notfound";
                    }
                    else
                    {
                        if (coordinatetype == "GolferCoordinate")
                        {
                            lstCourseBuilder = Db.GF_CourseBuilder.Where(x => x.CourseID == courseid && x.BuildBy == UserType.Golfer).ToList();
                        }
                        else
                        {
                            lstCourseBuilder = Db.GF_CourseBuilder.Where(x => x.CourseID == courseid && x.BuildBy == UserType.CourseAdmin && x.Status == StatusType.Active).ToList();
                        }

                        if (lstCourseBuilder != null)
                        {
                            if (lstCourseBuilder.Count > 0)
                            {
                                #region get co-ordinates
                                if (intholenumber == 0) // get data for all holes
                                {
                                    var lstHoleDetail = lstCourseBuilder.Where(x => x.ID == id).Select(x => new courseBuilder
                                    {
                                        coursebuilderid = x.ID,
                                        title = x.Title,
                                        courseName = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(x.GF_CourseInfo.COURSE_NAME.ToLower()),
                                        CoordinateType = x.CoordinateType,
                                        DateFrom = x.DateFrom == null ? null : (x.DateFrom ?? DateTime.Now).ToString("m/d/yyyy"),
                                        DateTo = x.DateTo == null ? null : (x.DateTo ?? DateTime.Now).ToString("m/d/yyyy"),
                                        Comments = Convert.ToString(x.Comments),
                                        HoleDetail = x.GF_CourseBuilderHolesDetail.ToList().Select((y, index) => new courseHoleDetail
                                        {
                                            markerID = (index + 1).ToString(),
                                            holeNumber = y.HoleNumber.ToString(),
                                            latitude = y.Latitude,
                                            longitude = y.Longitude.ToString(),
                                            dragItem = y.Discription
                                        }).ToList()
                                    });

                                    return lstHoleDetail;
                                }
                                else  // get data for particular hole
                                {
                                    var lstHoleDetail = lstCourseBuilder.Where(x => x.ID == id).Select(x => new courseBuilder
                                    {
                                        coursebuilderid = x.ID,
                                        title = x.Title,
                                        courseName = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(x.GF_CourseInfo.COURSE_NAME.ToLower()),
                                        CoordinateType = x.CoordinateType,
                                        DateFrom = x.DateFrom == null ? null : (x.DateFrom ?? DateTime.Now).ToString("m/d/yyyy"),
                                        DateTo = x.DateTo == null ? null : (x.DateTo ?? DateTime.Now).ToString("m/d/yyyy"),
                                        Comments = Convert.ToString(x.Comments),
                                        HoleDetail = x.GF_CourseBuilderHolesDetail.Where(z => z.HoleNumber == intholenumber).ToList().Select((y, index) => new courseHoleDetail
                                        {
                                            markerID = (index + 1).ToString(),
                                            holeNumber = y.HoleNumber.ToString(),
                                            latitude = y.Latitude,
                                            longitude = y.Longitude.ToString(),
                                            dragItem = y.Discription
                                        }).ToList()
                                    });

                                    return lstHoleDetail;
                                }
                                #endregion
                            }
                            else
                            {
                                CoOrdinateStatusInDB = "notfound";
                            }
                        }
                        else
                        {
                            CoOrdinateStatusInDB = "notfound";
                        }
                    }
                }
            }
            if (CoOrdinateStatusInDB == "notfound")
            {
                if (coordinatetype == "temp")
                {
                    var lstHoleDetail = Db.GF_CourseInfo.Where(x => x.ID == courseid).ToList().Select(x => new courseBuilder
                    {
                        coursebuilderid = 0,
                        title = "",
                        courseName = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(x.COURSE_NAME.ToLower()),
                        CoordinateType = Golfler.Models.CoordinateType.SetTemporary,
                        DateFrom = "",
                        DateTo = "",
                        Comments = "",
                        HoleDetail = null
                    });
                    return lstHoleDetail;
                }
                else
                {
                    var lstHoleDetail = Db.GF_CourseInfo.Where(x => x.ID == courseid).ToList().Select(x => new courseBuilder
                    {
                        coursebuilderid = 0,
                        title = "",
                        courseName = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(x.COURSE_NAME.ToLower()),
                        CoordinateType = Golfler.Models.CoordinateType.LoadOriginal,
                        DateFrom = "",
                        DateTo = "",
                        Comments = "",
                        HoleDetail = null
                    });
                    return lstHoleDetail;
                }
            }
            else
            {
                string coordType = Golfler.Models.CoordinateType.LoadOriginal;
                var lstHoleDetail = Db.GF_CourseInfo.Where(x => x.ID == courseid).ToList().Select(x => new courseBuilder
                {
                    coursebuilderid = 0,
                    title = "",
                    courseName = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(x.COURSE_NAME.ToLower()),
                    CoordinateType = coordType,
                    DateFrom = "",
                    DateTo = "",
                    Comments = "",
                    HoleDetail = null
                });
                return lstHoleDetail;
            }
        }

        /// <summary>
        /// Created By: Amit Kumar
        /// Created on: 22 April 2015
        /// </summary>
        /// <remarks>Get Course Builder Listing</remarks>
        public IQueryable<GF_CourseBuilder> GetCourseBuilderList(string filterExpression, Int64 courseid, string buildby, string sortExpression, string sortDirection, int pageIndex, int pageSize, ref int totalRecords)
        {
            Db = new GolflerEntities();

            IQueryable<GF_CourseBuilder> list;

            if (buildby == UserType.BuildByGolfer) // Listing for Golfer
            {
                list = Db.GF_CourseBuilder.Where(x => (x.CourseID ?? 0) == courseid &&
                     x.BuildBy == buildby && x.CreatedBy == LoginInfo.GolferUserId).OrderByDescending(x => x.ID);
            }
            else
            {
                if (buildby == "forSuperAdmin") // Listing for Super Admin
                {
                    list = Db.GF_CourseBuilder.Where(x => x.BuildBy == UserType.BuildByGolfer).OrderByDescending(x => x.ID);

                    // Currently, Search Filter is only applicable for Super Admin
                    if (!string.IsNullOrEmpty(Convert.ToString(filterExpression)))
                    {
                        list = list.Where(x => x.GF_CourseInfo.COURSE_NAME.Contains(filterExpression) ||
                            x.GF_CourseInfo.Country.Contains(filterExpression) ||
                            x.GF_CourseInfo.STATE.Contains(filterExpression) ||
                            x.GF_CourseInfo.CITY.Contains(filterExpression));
                    }
                }
                else // Listing for Course admin
                {
                    list = Db.GF_CourseBuilder.Where(x => (x.CourseID ?? 0) == courseid &&
                          (x.IsActive ?? false) && x.BuildBy == buildby && x.Status != StatusType.Delete).OrderByDescending(x => x.ID);
                }
            }

            totalRecords = list.Count();

            return list.OrderBy(sortExpression + " " + sortDirection).Skip((pageIndex - 1) * pageSize).Take(pageSize);
        }

        public bool DeleteCourseBuilder(List<long> ids)
        {
            Db = new GolflerEntities();

            var lst = Db.GF_CourseBuilder.Where(x => ids.AsQueryable().Contains(x.ID));
            foreach (var u in lst)
            {
                #region delete rec dates in case of temporary co-ordinates
                if (u.CoordinateType == Golfler.Models.CoordinateType.SetTemporary)
                {
                    var DbNew = new GolflerEntities();
                    var lstDates = DbNew.GF_CourseBuilderRecDates.Where(x => x.CourseBuilderId == u.ID && x.Status == StatusType.Active).ToList();
                    foreach (var dat in lstDates)
                    {
                        dat.Status = StatusType.Delete;

                    }
                    DbNew.SaveChanges();
                }
                #endregion

                var DbTest = new GolflerEntities();
                var lstDeleteRecord = DbTest.GF_CourseBuilder.FirstOrDefault(x => x.ID == u.ID);
                if (lstDeleteRecord != null)
                {
                    lstDeleteRecord.Status = StatusType.Delete;
                    lstDeleteRecord.IsActive = false;
                    lstDeleteRecord.ModifyBy = LoginInfo.UserId;
                    lstDeleteRecord.ModifyDate = DateTime.Now;
                    DbTest.SaveChanges();

                }
            }
            return true;
        }
    }

    class CourseBuilderMetaData
    {
        [DisplayName("Coordinate Type")]
        public string CoordinateType { get; set; }

        [DisplayName("From")]
        public DateTime? DateFrom { get; set; }

        [DisplayName("To")]
        public DateTime? DateTo { get; set; }

        [DisplayName("Is Active")]
        public bool Active { get; set; }
    }

    public class courseBuilder
    {
        public Int64 coursebuilderid { get; set; }
        public string title { get; set; }
        public string Comments { get; set; }
        public string courseName { get; set; }
        public string CoordinateType { get; set; }
        public string DateFrom { get; set; }
        public string DateTo { get; set; }
        public List<courseHoleDetail> HoleDetail { get; set; }
    }

    public class courseHoleDetail
    {
        public string markerID { get; set; }
        public string holeNumber { get; set; }
        public string latitude { get; set; }
        public string longitude { get; set; }
        public string dragItem { get; set; }
    }
}