using System;
using System.Collections.Generic;
using System.Data.Objects;
using System.Data.Objects.SqlClient;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Helpers;

namespace Golfler.Models
{
    public class CommissionReportFromat
    {
        public DateTime date { get; set; }
        public decimal commissionFee { get; set; }
        public decimal plateformFee { get; set; }
        public string CourseName { get; set; }
        public decimal CoursePlatformFee { get; set; }
        public long OrderID { get; set; }

        public decimal commissionFeeTotal { get; set; }
        public decimal plateformFeeTotal { get; set; }
        public string PaymentType { get; set; }
    }

    public class AnalyticalReportFromat
    {
        public DateTime date { get; set; }
        public decimal commissionFee { get; set; }
        public long OrderID { get; set; }
        public decimal Amount { get; set; }
        public string TransactionID { get; set; }
        public decimal plateformFee { get; set; }
        public string CourseName { get; set; }
        public long CourseID { get; set; }

        public decimal CoursePlatformFee { get; set; }
        public decimal commissionFeeTotal { get; set; }
        public decimal plateformFeeTotal { get; set; }
    }


    public class GraphReportFromat
    {
        public DateTime date { get; set; }
        public int GamePlay { get; set; }
        public int Foodordered { get; set; }
        public int Scale { get; set; }
        public int key{get;set;}
       public string hType{get;set;}

    }


    public class RatingAnalyticalFromat
    {
        public DateTime date { get; set; }
        public int Rating { get; set; }
        public int Complaint { get; set; }
        public int Scale { get; set; }
        public int key { get; set; }
        public string hType { get; set; }

    }


    public partial class CommissionReport
    {
        public decimal commissionFeeTotal { get; set; }
        public decimal plateformFeeTotal { get; set; }
        public decimal coursePlatformFee { get; set; }

        public decimal analyticAverageAmt { get; set; }
        public decimal analyticTotalAmt { get; set; }
        public decimal analyticPageTotalAmt { get; set; }

        private GolflerEntities _db = null;

     
        /// <summary>
        /// Created By: Kiran Bala
        /// Creation On: 14 May 2015
        /// Description: Commisison Reprot
        /// </summary>
        /// <param name="Courseid"></param>
        /// <param name="fromdate"></param>
        /// <param name="todate"></param>
        /// <param name="sortExpression"></param>
        /// <param name="sortDirection"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalRecords"></param>
        /// <returns></returns>
        [SessionExpireFilterAttribute]
        public IQueryable<CommissionReportFromat> GetCommissionReport(long Courseid, string fromdate, string todate, string paymentType,
            string sortExpression, string sortDirection, int pageIndex, int pageSize, ref int totalRecords)
        {
            _db = new GolflerEntities();
            var list = new List<CommissionReportFromat>();

            if (fromdate != "")
            {
                DateTime fdt = DateTime.Parse(fromdate);

                fromdate = fdt.ToShortDateString();
            }
            if (todate != "")
            {
                DateTime tdt = DateTime.Parse(todate);

                todate = tdt.ToShortDateString();
            }


            try
            {
                var courseID = new SqlParameter
                {
                    ParameterName = "COURSE_ID",
                    Value = Courseid
                };
                var fromDt = new SqlParameter
                {
                    ParameterName = "FROM_DATE",
                    Value = fromdate
                };
                var toDt = new SqlParameter
                {
                    ParameterName = "TO_DATE",
                    Value = todate
                };
                var paymenttype = new SqlParameter
                {
                    ParameterName = "@ORDER_TYPE",
                    Value = paymentType
                };

                var lstCommisionRpt = _db.Database.SqlQuery<CommissionReportFromat>("exec GF_SP_GETCOMMISSIONREPORT @COURSE_ID,@FROM_DATE,@TO_DATE,@ORDER_TYPE", courseID, fromDt, toDt, paymenttype).ToList<CommissionReportFromat>();

                list = lstCommisionRpt.ToList();

                commissionFeeTotal = list.Sum(x => x.commissionFee);
                plateformFeeTotal = list.Sum(x => x.plateformFee);
                coursePlatformFee = list.Sum(x => x.CoursePlatformFee);



                totalRecords = list.Count();
            }
            catch (Exception ex)
            {
                ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
            }

            return list.AsQueryable().OrderBy(sortExpression + " " + sortDirection).Skip((pageIndex - 1) * pageSize).Take(pageSize);
        }


        /// <summary>
        /// Created By: Kiran Bala
        /// Creation On: 14 May 2015
        /// Description: Commisison Reprot Export
        /// </summary>
        /// <param name="Courseid"></param>
        /// <param name="fromdate"></param>
        /// <param name="todate"></param>
        /// <param name="sortExpression"></param>
        /// <param name="sortDirection"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalRecords"></param>
        /// <returns></returns>
        [SessionExpireFilterAttribute]
        public IQueryable<CommissionReportFromat> GetCommissionReportExport(long Courseid, string fromdate, string todate)
        {
            _db = new GolflerEntities();
            var list = new List<CommissionReportFromat>();

            if (fromdate != "")
            {
                DateTime fdt = DateTime.Parse(fromdate);
                fromdate = fdt.ToShortDateString();
            }
            if (todate != "")
            {
                DateTime tdt = DateTime.Parse(todate);
                todate = tdt.ToShortDateString();
            }
            try
            {
                var courseID = new SqlParameter
                {
                    ParameterName = "COURSE_ID",
                    Value = Courseid
                };
                var fromDt = new SqlParameter
                {
                    ParameterName = "FROM_DATE",
                    Value = fromdate
                };
                var toDt = new SqlParameter
                {
                    ParameterName = "TO_DATE",
                    Value = todate
                };

                var lstCommisionRpt = _db.Database.SqlQuery<CommissionReportFromat>("exec GF_SP_GETCOMMISSIONREPORT @COURSE_ID,@FROM_DATE,@TO_DATE", courseID, fromDt, toDt).ToList<CommissionReportFromat>();

                list = lstCommisionRpt.ToList();

                commissionFeeTotal = list.Sum(x => x.commissionFee);
                plateformFeeTotal = list.Sum(x => x.plateformFee);
                coursePlatformFee = list.Sum(x => x.CoursePlatformFee);
            }
            catch (Exception ex)
            {
                ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
            }

            return list.AsQueryable().OrderBy(" date desc ");
        }


        /// <summary>
        /// Created By:Arun
        /// Created Date:25 May 2015
        /// Purpose: Analytical report
        /// </summary>
        /// <param name="email"></param>
        /// <param name="fromdate"></param>
        /// <param name="todate"></param>
        /// <param name="category"></param>
        /// <param name="subcategory"></param>
        /// <param name="courseid"></param>
        /// <param name="sortExpression"></param>
        /// <param name="sortDirection"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalRecords"></param>
        /// <returns></returns>
        [SessionExpireFilterAttribute]
        public IQueryable<AnalyticalReportFromat> GetAnalyticsReport(string email, string fromdate, string todate, long category, long subcategory, long courseid, string sortExpression, string sortDirection, int pageIndex, int pageSize, ref int totalRecords)
        {
            _db = new GolflerEntities();
            var list = new List<AnalyticalReportFromat>();

            if (fromdate != "")
            {
               // DateTime fdt =Convert.ToDateTime(fromdate);

                fromdate =Convert.ToDateTime(fromdate).ToString("MM/dd/yyyy");
            }
            if (todate != "")
            {
                //DateTime tdt = DateTime.Parse(todate);

                todate = DateTime.Parse(todate).ToString("MM/dd/yyyy");
            }


            try
            {
                var courseID = new SqlParameter
                {
                    ParameterName = "COURSE_ID",
                    Value = courseid
                };
                var fromDt = new SqlParameter
                {
                    ParameterName = "FROM_DATE",
                    Value = fromdate
                };
                var toDt = new SqlParameter
                {
                    ParameterName = "TO_DATE",
                    Value = todate
                };
                var emailid = new SqlParameter
                {
                    ParameterName = "EMAIL",
                    Value = email
                };
                var cat = new SqlParameter
                {
                    ParameterName = "CATEGORY",
                    Value = category
                };
                var subcat = new SqlParameter
                {
                    ParameterName = "SUBCATEGORY",
                    Value = subcategory
                };

                var lstCommisionRpt = _db.Database.SqlQuery<AnalyticalReportFromat>("exec GF_SP_GETAnalyticalAdminREPORT @COURSE_ID,@FROM_DATE,@TO_DATE,@EMAIL,@CATEGORY,@SUBCATEGORY",
                    courseID, fromDt, toDt, emailid, cat, subcat).ToList<AnalyticalReportFromat>();

                list = lstCommisionRpt.ToList();

                if (list.Count() > 0)
                {
                    analyticAverageAmt = list.Average(x => x.Amount);
                    analyticTotalAmt = list.Sum(x => x.Amount);
                    analyticPageTotalAmt = list.AsQueryable().OrderBy(sortExpression + " " + sortDirection).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList().Sum(x => x.Amount);
                }
                else
                {
                    analyticAverageAmt = 0;
                    analyticTotalAmt = 0;
                    analyticPageTotalAmt = 0;
                }

                totalRecords = list.Count();
            }
            catch (Exception ex)
            {
                ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
            }

            return list.AsQueryable().OrderBy(sortExpression + " " + sortDirection).Skip((pageIndex - 1) * pageSize).Take(pageSize);
        }


        /// <summary>
        /// Created By:Arun
        /// Created Date:25 May 2015
        /// Purpose: Analytical report
        /// </summary>
        /// <param name="email"></param>
        /// <param name="fromdate"></param>
        /// <param name="todate"></param>
        /// <param name="category"></param>
        /// <param name="subcategory"></param>
        /// <param name="courseid"></param>
        /// <param name="sortExpression"></param>
        /// <param name="sortDirection"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalRecords"></param>
        /// <returns></returns>
        [SessionExpireFilterAttribute]
        public IQueryable<AnalyticalReportFromat> GetAnalyticsReportExport(string email, string fromdate, string todate, long category, long subcategory, long courseid)
        {
            _db = new GolflerEntities();
            var list = new List<AnalyticalReportFromat>();

            if (fromdate != "")
            {
                DateTime fdt = DateTime.Parse(fromdate);

                fromdate = fdt.ToString("MM/dd/yyyy");
            }
            if (todate != "")
            {
                DateTime tdt = DateTime.Parse(todate);

                todate = tdt.ToString("MM/dd/yyyy");
            }


            try
            {
                var courseID = new SqlParameter
                {
                    ParameterName = "COURSE_ID",
                    Value = courseid
                };
                var fromDt = new SqlParameter
                {
                    ParameterName = "FROM_DATE",
                    Value = fromdate
                };
                var toDt = new SqlParameter
                {
                    ParameterName = "TO_DATE",
                    Value = todate
                };
                var emailid = new SqlParameter
                {
                    ParameterName = "EMAIL",
                    Value = email
                };
                var cat = new SqlParameter
                {
                    ParameterName = "CATEGORY",
                    Value = category
                };
                var subcat = new SqlParameter
                {
                    ParameterName = "SUBCATEGORY",
                    Value = subcategory
                };

                var lstCommisionRpt = _db.Database.SqlQuery<AnalyticalReportFromat>("exec GF_SP_GETAnalyticalAdminREPORT @COURSE_ID,@FROM_DATE,@TO_DATE,@EMAIL,@CATEGORY,@SUBCATEGORY",
                    courseID, fromDt, toDt, emailid, cat, subcat).ToList<AnalyticalReportFromat>();

                list = lstCommisionRpt.ToList();

                // commissionFeeTotal = list.Sum(x => x.commissionFee);
                // plateformFeeTotal = list.Sum(x => x.plateformFee);



               
            }
            catch (Exception ex)
            {
                ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
            }

            return list.AsQueryable().OrderBy(" date desc ");
        }
        
        public List<GraphReportFromat> GetGraphData(long courseid, string fromdate, string todate,string email,ref string daytype)
        {
            var lstClicks = new List<GraphReportFromat>();
            var lstClicksFilter=new List<GraphReportFromat>();
            string datetype = "";
            int Startindex = 0, Lastindex = 0;
            List<object[]> test = new List<object[]>();
              _db = new GolflerEntities();
            var list = new List<AnalyticalReportFromat>();

            if (fromdate != "")
            {
               // DateTime fdt =Convert.ToDateTime(fromdate);

                fromdate =Convert.ToDateTime(fromdate).ToString("MM/dd/yyyy");
            }
            if (todate != "")
            {
                //DateTime tdt = DateTime.Parse(todate);

                todate = DateTime.Parse(todate).ToString("MM/dd/yyyy");
            }


            try
            {
                var courseID = new SqlParameter
                {
                    ParameterName = "COURSE_ID",
                    Value = courseid
                };
                var fromDt = new SqlParameter
                {
                    ParameterName = "FROM_DATE",
                    Value = fromdate
                };
                var toDt = new SqlParameter
                {
                    ParameterName = "TO_DATE",
                    Value = todate
                };
                var emailid = new SqlParameter
                {
                    ParameterName = "EMAIL",
                    Value = email
                };


                 lstClicks = _db.Database.SqlQuery<GraphReportFromat>("exec GF_SP_GraphAnalyticalAdminREPORT @COURSE_ID,@FROM_DATE,@TO_DATE,@EMAIL",
                    courseID, fromDt, toDt, emailid).ToList<GraphReportFromat>();

               // lstClicks = lstCommisionRpt.ToList();
                #region Toatal Summary
                //total clicks till date
                double weeknumber = Math.Round(Convert.ToDouble(DateTime.Now.DayOfYear) / 7) + 1;  //get week number
                DateTime weekstartdate = FirstDateOfWeek(DateTime.Now.Year, weeknumber);   //Get week start date
                DateTime weekenddate = weekstartdate.AddDays(6);  //Get week enddate


                #endregion

                DateTime stDatelst = DateTime.Parse(fromdate);
                DateTime edDate = DateTime.Parse(todate);
                var days = (edDate - stDatelst).TotalDays;

                // if date range is Less than Thirty days.
                if (days < 7)
                {
                    daytype = "Day(s)";
                    datetype = "d";
                    var lstClicksFilter1 = (from t in lstClicks
                                            group t by t.date.Day into ut
                                            select new
                                            {
                                                htype = ut.Key,
                                                key = ut.Key,
                                                GamePlay = ut.Sum(z => z.GamePlay),
                                                Foodordered = ut.Sum(z => z.Foodordered),
                                                date = ut.FirstOrDefault().date
                                            }).ToList().Select(z =>
                                           new GraphReportFromat
                                           {
                                               hType = z.htype.ToString(),
                                               key = z.key,
                                               date = z.date,
                                               GamePlay = z.GamePlay,
                                               Foodordered = z.Foodordered

                                           }).ToList();

                    //Include all days which are not exists in database.
                    Startindex = stDatelst.Day;
                    Lastindex = edDate.Day;

                    lstClicksFilter = SetData(lstClicksFilter1, Startindex, Lastindex, datetype);
                }

                // if date range is Less than 3 Months.
                else if (days <= 30)
                {
                    daytype = "Week(s) of year";
                    datetype = "W";
                    var lstClicksFilter1 = lstClicks.GroupBy(i =>GetWeekNumber(i.date)).Select(z =>
                                     new
                                     {
                                         htype = z.Key,
                                         GamePlay = z.Sum(ut => ut.GamePlay),
                                         Foodordered = z.Sum(ut => ut.Foodordered),
                                         date = z.Max(t => t.date.Month)
                                     }).ToList().Select(y => new GraphReportFromat
                                     {
                                         hType = y.htype.ToString(),
                                         key = y.htype,
                                         GamePlay = y.GamePlay,
                                         Foodordered = y.Foodordered
                                     }).ToList();

                    //Add all week in the list which are not exists in database and lies between first and last week of search criteria.
                    Startindex = GetWeekNumber(Convert.ToDateTime(fromdate));
                    Lastindex = GetWeekNumber(Convert.ToDateTime(todate));
                    lstClicksFilter = SetData(lstClicksFilter1, Startindex, Lastindex, datetype);

                }

                    // if date range between 4 to 12 months.
                else if (days < 366)
                {
                    daytype = "Month(s)";
                    datetype = "m";
                    var lstClicksFilter1 = (from t in lstClicks
                                            group t by t.date.Month into ut
                                            select new
                                            {
                                                hType = ut.Key,
                                                GamePlay = ut.Sum(z=>z.GamePlay),
                                                Foodordered = ut.Sum(z => z.Foodordered),
                                                key = ut.Key
                                            }).ToList().Select(z => new GraphReportFromat
                                            {
                                                hType = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(z.hType),
                                                GamePlay = z.GamePlay,
                                                Foodordered = z.Foodordered,
                                                key = z.key
                                            }).ToList();

                    //Add all week in the list which are not exists in database and lies between first and last week of search criteria.
                    Startindex = stDatelst.Month;
                    Lastindex = edDate.Month;
                    lstClicksFilter = SetData(lstClicksFilter1, Startindex, Lastindex, datetype);

                }

      // if date range greater than 1 year.
                else
                {
                    daytype = "Year(s)";
                    datetype = "y";
                    var lstClicksFilter1 = (from t in lstClicks
                                            group t by t.date.Year into ut
                                            select new
                                            {
                                                hType = ut.Key,
                                                GamePlay = ut.Sum(z => z.GamePlay),
                                                Foodordered = ut.Sum(z => z.Foodordered),
                                            }).ToList().Select(z => new GraphReportFromat
                                            {
                                                hType = z.hType.ToString(),
                                                GamePlay = z.GamePlay,
                                                Foodordered = z.Foodordered,
                                                key = z.hType,
                                            }).ToList();

                    //Add all week in the list which are not exists in database and lies between first and last week of search criteria.
                    Startindex = stDatelst.Year;
                    Lastindex = edDate.Year;
                    lstClicksFilter = SetData(lstClicksFilter1, Startindex, Lastindex, datetype);
                }
            }
            catch (Exception ex)
            { 
            
            
            }

            return lstClicksFilter;

        }
        
        public List<RatingAnalyticalFromat> GetRatingGraphData(long courseid, string fromdate, string todate, string email, ref string daytype)
        {
            var lstClicks = new List<RatingAnalyticalFromat>();
            var lstClicksFilter = new List<RatingAnalyticalFromat>();
            string datetype = "";
            int Startindex = 0, Lastindex = 0;
            List<object[]> test = new List<object[]>();
            _db = new GolflerEntities();
            var list = new List<RatingAnalyticalFromat>();

            if (fromdate != "")
            {
                // DateTime fdt =Convert.ToDateTime(fromdate);

                fromdate = Convert.ToDateTime(fromdate).ToString("MM/dd/yyyy");
            }
            if (todate != "")
            {
                //DateTime tdt = DateTime.Parse(todate);

                todate = DateTime.Parse(todate).ToString("MM/dd/yyyy");
            }


            try
            {
                var courseID = new SqlParameter
                {
                    ParameterName = "COURSE_ID",
                    Value = courseid
                };
                var fromDt = new SqlParameter
                {
                    ParameterName = "FROM_DATE",
                    Value = fromdate
                };
                var toDt = new SqlParameter
                {
                    ParameterName = "TO_DATE",
                    Value = todate
                };
                var emailid = new SqlParameter
                {
                    ParameterName = "EMAIL",
                    Value = email
                };


                lstClicks = _db.Database.SqlQuery<RatingAnalyticalFromat>("exec GF_SP_RatingAnalyticalAdminREPORT @COURSE_ID,@FROM_DATE,@TO_DATE,@EMAIL",
                   courseID, fromDt, toDt, emailid).ToList<RatingAnalyticalFromat>();

                // lstClicks = lstCommisionRpt.ToList();
                #region Toatal Summary
                //total clicks till date
                double weeknumber = Math.Round(Convert.ToDouble(DateTime.Now.DayOfYear) / 7) + 1;  //get week number
                DateTime weekstartdate = FirstDateOfWeek(DateTime.Now.Year, weeknumber);   //Get week start date
                DateTime weekenddate = weekstartdate.AddDays(6);  //Get week enddate


                #endregion

                DateTime stDatelst = DateTime.Parse(fromdate);
                DateTime edDate = DateTime.Parse(todate);
                var days = (edDate - stDatelst).TotalDays;

                // if date range is Less than Thirty days.
                if (days < 7)
                {
                    daytype = "Day(s)";
                    datetype = "d";
                    var lstClicksFilter1 = (from t in lstClicks
                                            group t by t.date.Day into ut
                                            select new
                                            {
                                                htype = ut.Key,
                                                key = ut.Key,
                                                Rating = ut.Sum(z => z.Rating),
                                                Complaint = ut.Sum(z => z.Complaint),
                                                date = ut.FirstOrDefault().date
                                            }).ToList().Select(z =>
                                           new RatingAnalyticalFromat
                                           {
                                               hType = z.htype.ToString(),
                                               key = z.key,
                                               date = z.date,
                                               Rating = z.Rating,
                                               Complaint = z.Complaint

                                           }).ToList();

                    //Include all days which are not exists in database.
                    Startindex = stDatelst.Day;
                    Lastindex = edDate.Day;

                    lstClicksFilter = SetData1(lstClicksFilter1, Startindex, Lastindex, datetype);
                }

                // if date range is Less than 3 Months.
                else if (days <= 30)
                {
                    daytype = "Week(s) of year";
                    datetype = "W";
                    var lstClicksFilter1 = lstClicks.GroupBy(i => GetWeekNumber(i.date)).Select(z =>
                                     new
                                     {
                                         htype = z.Key,
                                         Rating = z.Sum(ut => ut.Rating),
                                         Complaint = z.Sum(ut => ut.Complaint),
                                         date = z.Max(t => t.date.Month)
                                     }).ToList().Select(y => new RatingAnalyticalFromat
                                     {
                                         hType = y.htype.ToString(),
                                         key = y.htype,
                                         Rating = y.Rating,
                                         Complaint = y.Complaint
                                     }).ToList();

                    //Add all week in the list which are not exists in database and lies between first and last week of search criteria.
                    Startindex = GetWeekNumber(Convert.ToDateTime(fromdate));
                    Lastindex = GetWeekNumber(Convert.ToDateTime(todate));
                    lstClicksFilter = SetData1(lstClicksFilter1, Startindex, Lastindex, datetype);

                }

                    // if date range between 4 to 12 months.
                else if (days < 366)
                {
                    daytype = "Month(s)";
                    datetype = "m";
                    var lstClicksFilter1 = (from t in lstClicks
                                            group t by t.date.Month into ut
                                            select new
                                            {
                                                hType = ut.Key,
                                                Rating = ut.Sum(z => z.Rating),
                                                Complaint = ut.Sum(z => z.Complaint),
                                                key = ut.Key
                                            }).ToList().Select(z => new RatingAnalyticalFromat
                                            {
                                                hType = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(z.hType),
                                                Rating = z.Rating,
                                                Complaint = z.Complaint,
                                                key = z.key
                                            }).ToList();

                    //Add all week in the list which are not exists in database and lies between first and last week of search criteria.
                    Startindex = stDatelst.Month;
                    Lastindex = edDate.Month;
                    lstClicksFilter = SetData1(lstClicksFilter1, Startindex, Lastindex, datetype);

                }

      // if date range greater than 1 year.
                else
                {
                    daytype = "Year(s)";
                    datetype = "y";
                    var lstClicksFilter1 = (from t in lstClicks
                                            group t by t.date.Year into ut
                                            select new
                                            {
                                                hType = ut.Key,
                                                Rating = ut.Sum(z => z.Rating),
                                                Complaint = ut.Sum(z => z.Complaint),
                                            }).ToList().Select(z => new RatingAnalyticalFromat
                                            {
                                                hType = z.hType.ToString(),
                                                Rating = z.Rating,
                                                Complaint = z.Complaint,
                                                key = z.hType,
                                            }).ToList();

                    //Add all week in the list which are not exists in database and lies between first and last week of search criteria.
                    Startindex = stDatelst.Year;
                    Lastindex = edDate.Year;
                    lstClicksFilter = SetData1(lstClicksFilter1, Startindex, Lastindex, datetype);
                }
            }
            catch (Exception ex)
            {


            }

            return lstClicksFilter;

        }

        public DateTime FirstDateOfWeek(int year, double weekOfYear)
        {
            DateTime jan1 = new DateTime(year, 1, 1);
            int daysOffset = DayOfWeek.Thursday - jan1.DayOfWeek;

            DateTime firstThursday = jan1.AddDays(daysOffset);
            var cal = CultureInfo.CurrentCulture.Calendar;
            int firstWeek = cal.GetWeekOfYear(firstThursday, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);

            var weekNum = weekOfYear;
            if (firstWeek <= 1)
            {
                weekNum -= 1;
            }
            var result = firstThursday.AddDays(weekNum * 7);
            return result.AddDays(-3);
        }

        public List<GraphReportFromat> SetData(List<GraphReportFromat> lstClicksFilter1, int Startindex, int Lastindex, string datetype)
        {
            var lstClicksFilter = new List<GraphReportFromat>();

            if (Startindex <= Lastindex)
            {

                if (Startindex == Lastindex)
                    Startindex = Startindex - 1;
                for (int i = Startindex; i <= Lastindex; i++)
                {
                    var obj = lstClicksFilter1.FirstOrDefault(t => t.key == i);
                    if (obj != null)
                    {
                        lstClicksFilter.Add(new GraphReportFromat { GamePlay = obj.GamePlay,Foodordered=obj.Foodordered, key = obj.key, hType = obj.hType });
                    }
                    else
                    {
                        if (datetype == "m")
                            lstClicksFilter.Add(new GraphReportFromat { GamePlay = 0,Foodordered=0, key = i, hType = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(i) });
                        else
                            lstClicksFilter.Add(new GraphReportFromat {GamePlay = 0,Foodordered=0, key = i, hType = i.ToString() });
                    }
                }
            }
            else
            {

                int end = 30 + Lastindex;
                for (int i = Startindex; i <= end; i++)
                {
                    var obj = lstClicksFilter1.FirstOrDefault(t => t.key == (i > 30 ? i - 30 : i));

                    if (obj != null)
                    {
                        lstClicksFilter.Add(new GraphReportFromat { GamePlay = obj.GamePlay,Foodordered=obj.Foodordered, key = obj.key, hType = obj.hType });
                    }
                    else
                    {

                        if (datetype == "m")
                        {
                            lstClicksFilter.Add(new GraphReportFromat { GamePlay =0,Foodordered=0, key = i, hType = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(i) });
                        }
                        else
                        {

                            lstClicksFilter.Add(new GraphReportFromat { GamePlay = 0,Foodordered=0, key = i, hType = i > 30 ? (i - 30).ToString() : i.ToString() });
                        }
                    }
                }
            }
            return lstClicksFilter;

        }

        public List<RatingAnalyticalFromat> SetData1(List<RatingAnalyticalFromat> lstClicksFilter1, int Startindex, int Lastindex, string datetype)
        {
            var lstClicksFilter = new List<RatingAnalyticalFromat>();

            if (Startindex <= Lastindex)
            {

                if (Startindex == Lastindex)
                    Startindex = Startindex - 1;
                for (int i = Startindex; i <= Lastindex; i++)
                {
                    var obj = lstClicksFilter1.FirstOrDefault(t => t.key == i);
                    if (obj != null)
                    {
                        lstClicksFilter.Add(new RatingAnalyticalFromat { Rating = obj.Rating, Complaint = obj.Complaint, key = obj.key, hType = obj.hType });
                    }
                    else
                    {
                        if (datetype == "m")
                            lstClicksFilter.Add(new RatingAnalyticalFromat { Rating = 0, Complaint = 0, key = i, hType = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(i) });
                        else
                            lstClicksFilter.Add(new RatingAnalyticalFromat { Rating = 0, Complaint = 0, key = i, hType = i.ToString() });
                    }
                }
            }
            else
            {

                int end = 30 + Lastindex;
                for (int i = Startindex; i <= end; i++)
                {
                    var obj = lstClicksFilter1.FirstOrDefault(t => t.key == (i > 30 ? i - 30 : i));

                    if (obj != null)
                    {
                        lstClicksFilter.Add(new RatingAnalyticalFromat { Rating = obj.Rating, Complaint = obj.Complaint, key = obj.key, hType = obj.hType });
                    }
                    else
                    {

                        if (datetype == "m")
                        {
                            lstClicksFilter.Add(new RatingAnalyticalFromat { Rating = 0, Complaint = 0, key = i, hType = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(i) });
                        }
                        else
                        {

                            lstClicksFilter.Add(new RatingAnalyticalFromat { Rating = 0, Complaint = 0, key = i, hType = i > 30 ? (i - 30).ToString() : i.ToString() });
                        }
                    }
                }
            }
            return lstClicksFilter;

        }


        public static int GetWeekNumber(DateTime dtPassed)
        {
            CultureInfo ciCurr = CultureInfo.CurrentCulture;
            int weekNum = ciCurr.Calendar.GetWeekOfYear(dtPassed, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            return weekNum;
        }


        public IQueryable<GraphReportFromat> GetGamePlayAnalyticsReportExport(string email, string fromdate, string todate,  long courseid)
        {
            _db = new GolflerEntities();
            var list = new List<GraphReportFromat>();

            if (fromdate != "")
            {
                // DateTime fdt =Convert.ToDateTime(fromdate);

                fromdate = Convert.ToDateTime(fromdate).ToString("MM/dd/yyyy");
            }
            if (todate != "")
            {
                //DateTime tdt = DateTime.Parse(todate);

                todate = DateTime.Parse(todate).ToString("MM/dd/yyyy");
            }



            try
            {
                var courseID = new SqlParameter
                {
                    ParameterName = "COURSE_ID",
                    Value = courseid
                };
                var fromDt = new SqlParameter
                {
                    ParameterName = "FROM_DATE",
                    Value = fromdate
                };
                var toDt = new SqlParameter
                {
                    ParameterName = "TO_DATE",
                    Value = todate
                };
                var emailid = new SqlParameter
                {
                    ParameterName = "EMAIL",
                    Value = email
                };


                list = _db.Database.SqlQuery<GraphReportFromat>("exec GF_SP_GraphAnalyticalAdminREPORT @COURSE_ID,@FROM_DATE,@TO_DATE,@EMAIL",
                    courseID, fromDt, toDt, emailid).ToList<GraphReportFromat>();

                list = list.ToList();

                // commissionFeeTotal = list.Sum(x => x.commissionFee);
                // plateformFeeTotal = list.Sum(x => x.plateformFee);




            }
            catch (Exception ex)
            {
                ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
            }

            return list.AsQueryable().OrderBy(" date desc ");
        }

        public IQueryable<RatingAnalyticalFromat> GetRatingAnalyticsReportExport(string email, string fromdate, string todate, long courseid)
        {
            _db = new GolflerEntities();
            var list = new List<RatingAnalyticalFromat>();

            if (fromdate != "")
            {
                // DateTime fdt =Convert.ToDateTime(fromdate);

                fromdate = Convert.ToDateTime(fromdate).ToString("MM/dd/yyyy");
            }
            if (todate != "")
            {
                //DateTime tdt = DateTime.Parse(todate);

                todate = DateTime.Parse(todate).ToString("MM/dd/yyyy");
            }


            try
            {
                var courseID = new SqlParameter
                {
                    ParameterName = "COURSE_ID",
                    Value = courseid
                };
                var fromDt = new SqlParameter
                {
                    ParameterName = "FROM_DATE",
                    Value = fromdate
                };
                var toDt = new SqlParameter
                {
                    ParameterName = "TO_DATE",
                    Value = todate
                };
                var emailid = new SqlParameter
                {
                    ParameterName = "EMAIL",
                    Value = email
                };


                list = _db.Database.SqlQuery<RatingAnalyticalFromat>("exec GF_SP_RatingAnalyticalAdminREPORT @COURSE_ID,@FROM_DATE,@TO_DATE,@EMAIL",
                courseID, fromDt, toDt, emailid).ToList<RatingAnalyticalFromat>();

                list = list.ToList();

                // commissionFeeTotal = list.Sum(x => x.commissionFee);
                // plateformFeeTotal = list.Sum(x => x.plateformFee);




            }
            catch (Exception ex)
            {
                ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
            }

            return list.AsQueryable().OrderBy(" date desc ");
        }
    }
}