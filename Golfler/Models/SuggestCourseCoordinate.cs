using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Golfler.Models
{
    public partial class GF_SuggestCourseCoordinate
    {
        public GF_SuggestCourseCoordinate suggestCoordinate { get; private set; }
        public GF_SuggestCoordinateDetail holeDetail { get; private set; }
        public List<suggestCoordinateDetail> suggestCoordinateDetail { get; private set; }

        protected GolflerEntities Db = null;

        public bool Active { get { return Status == StatusType.Active; } set { Status = (value) ? Status = StatusType.Active : Status = StatusType.InActive; } }
        public string Message { get; private set; }

        public bool SaveSuggestCoordinate(string courseID, List<suggestCoordinateDetail> objHole, ref string Message)
        {
            try
            {
                Db = new GolflerEntities();

                suggestCoordinate = new GF_SuggestCourseCoordinate();
                suggestCoordinate.CourseID = Convert.ToInt32(courseID);
                suggestCoordinate.Status = StatusType.Active;
                suggestCoordinate.IsActive = true;
                suggestCoordinate.CreatedDate = DateTime.Now;
                suggestCoordinate.CreatedBy = LoginInfo.UserId;

                Db.GF_SuggestCourseCoordinate.Add(suggestCoordinate);
                Db.SaveChanges();

                #region Insert Suggested Hole Detail

                if (suggestCoordinate.ID > 0)
                {
                    foreach (var item in objHole)
                    {
                        holeDetail = new GF_SuggestCoordinateDetail();
                        holeDetail.SuggestCoordinateID = suggestCoordinate.ID;
                        holeDetail.HoleNumber = Convert.ToInt32(item.holeNumber);
                        holeDetail.Latitude = item.latitude;
                        holeDetail.Longitude = item.longitude;
                        holeDetail.DragItemType = item.dragItem.Contains("Tee") ? DragItemType.Tee : DragItemType.WhiteFlag;
                        holeDetail.Discription = item.dragItem;
                        Db.GF_SuggestCoordinateDetail.Add(holeDetail);
                    }
                    Db.SaveChanges();
                }

                #endregion

                Message = "add";

                return true;
            }
            catch (Exception ex)
            {
                ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
                Message = "Error occured. Please try again.";
                return false;
            }
        }

        public IEnumerable<suggestCoordinate> getSuggestCoordinate(long cid)
        {
            Db = new GolflerEntities();

            List<GF_SuggestCourseCoordinate> lstCourseBuilder = Db.GF_SuggestCourseCoordinate.Where(x => x.CourseID == cid)
                                                                                             .OrderByDescending(x => x.ID).Take(1).ToList();

            if (lstCourseBuilder.Count > 0 && lstCourseBuilder != null)
            {
                var lstHoleDetail = lstCourseBuilder.Select(x => new suggestCoordinate
                {
                    courseName = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(x.GF_CourseInfo.COURSE_NAME.ToLower()),
                    address = x.GF_CourseInfo.ADDRESS + ", " + x.GF_CourseInfo.CITY + ", " + x.GF_CourseInfo.STATE + ", " + x.GF_CourseInfo.COUNTY,
                    HoleDetail = x.GF_SuggestCoordinateDetail.ToList().Select((y, index) => new suggestCoordinateDetail
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

            return null;
        }

        public IQueryable<GF_SuggestCourseCoordinate> GetSuggestCoordinateList(string filterExpression, string sortExpression, string sortDirection, int pageIndex, int pageSize, ref int totalRecords)
        {
            Db = new GolflerEntities();

            IQueryable<GF_SuggestCourseCoordinate> list;

            list = Db.GF_SuggestCourseCoordinate.Where(x => (x.CourseID ?? 0) == LoginInfo.CourseId &&
                (x.IsActive ?? false)).OrderByDescending(x => x.ID);

            totalRecords = list.Count();

            return list.OrderBy(sortExpression + " " + sortDirection).Skip((pageIndex - 1) * pageSize).Take(pageSize);
        }

        public bool DeleteCourseBuilder(List<long> ids)
        {
            Db = new GolflerEntities();

            var lst = Db.GF_SuggestCourseCoordinate.Where(x => ids.AsQueryable().Contains(x.ID));
            foreach (var u in lst)
            {
                u.Status = StatusType.Delete;
                u.IsActive = false;
                u.ModifyBy = LoginInfo.UserId;
                u.ModifyDate = DateTime.Now;
            }

            Db.SaveChanges();

            return true;
        }
    }

    //public class courseBuilder
    public class suggestCoordinate
    {
        public string courseName { get; set; }
        public string address { get; set; }
        public List<suggestCoordinateDetail> HoleDetail { get; set; }
    }

    //public class courseHoleDetail
    public class suggestCoordinateDetail
    {
        public string markerID { get; set; }
        public string holeNumber { get; set; }
        public string latitude { get; set; }
        public string longitude { get; set; }
        public string dragItem { get; set; }
    }
}