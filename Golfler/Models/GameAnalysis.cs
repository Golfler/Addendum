using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.EntityClient;
using System.Data.Objects;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Web;

namespace Golfler.Models
{
    public class GameAnalysis
    {
        GolflerEntities Db = null;

        public GameAnalysis()
        {
            Db = new GolflerEntities();
        }

        public List<object> GetWeatherReport(long courseID, long GolferID, DateTime DateFrom, DateTime DateTo,
            string campareParameter, double range, string rptType)
        {
            List<object> weatherList = new List<object>();
            weatherList.Add(new object[] { " ", "Temp (F)", "Wind (KM)", "Rain (Inches)" });

            try
            {
                string reportType = ReportResultType.DateWise;

                #region Get Report Type

                var allDates = DateFrom.Range(DateTo);

                if (allDates.Count() <= 7)
                {
                    reportType = ReportResultType.DateWise;
                }
                else if (allDates.Count() <= 31)
                {
                    reportType = ReportResultType.WeekWise;
                }
                else if (allDates.Count() > 31 && allDates.Count() <= 366)
                {
                    reportType = ReportResultType.MonthWise;
                }
                else if (allDates.Count() > 366)
                {
                    reportType = ReportResultType.YearWise;
                }

                #endregion

                var cID = new SqlParameter
                {
                    ParameterName = "CourseID",
                    Value = courseID
                };

                var gID = new SqlParameter
                {
                    ParameterName = "GolferID",
                    Value = GolferID
                };

                var rType = new SqlParameter
                {
                    ParameterName = "ResultType",
                    Value = reportType
                };

                var dtFrom = new SqlParameter
                {
                    ParameterName = "DateFrom",
                    Value = DateFrom.ToString("yyyy-MM-dd")
                };

                var dtTo = new SqlParameter
                {
                    ParameterName = "DateTo",
                    Value = DateTo.ToString("yyyy-MM-dd")
                };

                var cParameter = new SqlParameter
                {
                    ParameterName = "CampareParameter",
                    Value = campareParameter
                };

                var rnge = new SqlParameter
                {
                    ParameterName = "Range",
                    Value = CommonFunctions.ConvertMilesToKilometers(range)
                };

                var rpType = new SqlParameter
                {
                    ParameterName = "RptType",
                    Value = rptType
                };

                var lstWeatherReport = Db.Database.SqlQuery<WeatherReport>("exec GF_SP_GetWeatherAverageReport @CourseID, @GolferID, @ResultType, @DateFrom, @DateTo, @CampareParameter, @Range, @RptType",
                    cID, gID, rType, dtFrom, dtTo, cParameter, rnge, rpType).ToList<WeatherReport>();

                if (lstWeatherReport.Count() > 0)
                {
                    string dText = "";
                    Calendar calendar = CultureInfo.CurrentCulture.Calendar;
                    var lstWeekYear = allDates.ToList().Select(x => new
                        {
                            WeekNumber = calendar.GetWeekOfYear(x.Date, CalendarWeekRule.FirstDay, DayOfWeek.Sunday).ToString(),
                            Year = x.Year
                        }).Distinct();

                    foreach (var row in lstWeatherReport)
                    {
                        if (reportType == ReportResultType.WeekWise)
                        {
                            if (lstWeekYear != null)
                            {
                                dText = lstWeekYear.FirstOrDefault(x => x.WeekNumber == row.displayText) != null ?
                                    CommonFunctions.FirstLastDayOfWeek(Convert.ToInt32(row.displayText), lstWeekYear.FirstOrDefault(x => x.WeekNumber == row.displayText).Year) : row.displayText;
                            }
                            else
                            {
                                dText = row.displayText;
                            }
                        }
                        else
                        {
                            dText = row.displayText;
                        }

                        weatherList.Add(new object[] { dText, row.temperature, row.windSpeed, row.rain });
                    }
                }
                else
                {
                    weatherList.Add(new object[] { " ", 0, 0, 0 });
                }

                return weatherList;
            }
            catch
            {
                weatherList.Add(new object[] { " ", 0, 0, 0 });
                return weatherList;
            }
        }

        public List<ScoreAverageReport> GetScoreAverageInfo(long courseID, long GolferID, DateTime DateFrom, DateTime DateTo,
            string campareParameter, double range, string rptType)
        {
            try
            {
                var cID = new SqlParameter
                {
                    ParameterName = "CourseID",
                    Value = courseID
                };

                var gID = new SqlParameter
                {
                    ParameterName = "GolferID",
                    Value = GolferID
                };

                var dtFrom = new SqlParameter
                {
                    ParameterName = "DateFrom",
                    Value = DateFrom.ToString("yyyy-MM-dd")
                };

                var dtTo = new SqlParameter
                {
                    ParameterName = "DateTo",
                    Value = DateTo.ToString("yyyy-MM-dd")
                };

                var cParameter = new SqlParameter
                {
                    ParameterName = "CampareParameter",
                    Value = campareParameter
                };

                var rnge = new SqlParameter
                {
                    ParameterName = "Range",
                    Value = CommonFunctions.ConvertMilesToKilometers(range)
                };

                var rpType = new SqlParameter
                {
                    ParameterName = "RptType",
                    Value = rptType
                };

                var lstScoreAverageReport = Db.Database.SqlQuery<ScoreAverageReport>("exec GF_SP_ScoreAverageReport @CourseID, @GolferID, @DateFrom, @DateTo, @CampareParameter, @Range, @RptType",
                    cID, gID, dtFrom, dtTo, cParameter, rnge, rpType).ToList<ScoreAverageReport>();

                return lstScoreAverageReport;
            }
            catch
            {
                return new List<ScoreAverageReport>();
            }
        }

        public List<GamePlayScoreCard> GetScoreCardInfo(long courseID, long GolferID, DateTime DateFrom, DateTime DateTo, long pageNo,
            string campareParameter, double range, string rptType, ref List<GamePlayPlayer> gamePlayPlayer)
        {
            try
            {
                //var lstGamePlayScoreCard = (from x in Db.GF_GamePlayRound
                //                            join y in Db.GF_GamePlayScoreCard on x.ID equals y.RoundID
                //                            where x.CourseID == courseID &&
                //                            x.GolferID == GolferID &&
                //                            (EntityFunctions.TruncateTime(x.CreatedDate) >= DateFrom.Date &&
                //                            EntityFunctions.TruncateTime(x.CreatedDate) <= DateTo.Date)
                //                            select y).ToList()
                //                            .Select(x => new GamePlayScoreCard
                //                            {
                //                                RoundID = x.RoundID ?? 0,
                //                                HoleNo = x.HoleNo ?? 0,
                //                                Distance = x.Distance ?? 0,
                //                                Par = x.Par ?? 0,
                //                                PlayerOneScore = x.PlayerOneScore ?? 0,
                //                                PlayerTwoScore = x.PlayerTwoScore ?? 0,
                //                                PlayerThreeScore = x.PlayerThreeScore ?? 0,
                //                                PlayerFourScore = x.PlayerFourScore ?? 0,
                //                                RoundStartFrom = (x.HoleNo ?? 0) <= 9 ? "Front" : "Back"
                //                            });

                var cID = new SqlParameter
                {
                    ParameterName = "CourseID",
                    Value = courseID
                };

                var gID = new SqlParameter
                {
                    ParameterName = "GolferID",
                    Value = GolferID
                };

                var dtFrom = new SqlParameter
                {
                    ParameterName = "DateFrom",
                    Value = DateFrom.ToString("yyyy-MM-dd")
                };

                var dtTo = new SqlParameter
                {
                    ParameterName = "DateTo",
                    Value = DateTo.ToString("yyyy-MM-dd")
                };

                var pNo = new SqlParameter
                {
                    ParameterName = "PageNo",
                    Value = pageNo
                };

                var cParameter = new SqlParameter
                {
                    ParameterName = "CampareParameter",
                    Value = campareParameter
                };

                var rnge = new SqlParameter
                {
                    ParameterName = "Range",
                    Value = CommonFunctions.ConvertMilesToKilometers(range)
                };

                var rpType = new SqlParameter
                {
                    ParameterName = "RptType",
                    Value = rptType
                };

                var lstGamePlayScoreCard = Db.Database.SqlQuery<GamePlayScoreCard>("exec GF_SP_ScoreCardReport @CourseID, @GolferID, @DateFrom, @DateTo, @PageNo, @CampareParameter, @Range, @RptType",
                    cID, gID, dtFrom, dtTo, pNo, cParameter, rnge, rpType).ToList<GamePlayScoreCard>();

                long[] roundID = CommonFunctions.ConvertStringArrayToLongArray(string.Join(",", lstGamePlayScoreCard.Select(x => x.RoundID)));

                gamePlayPlayer = Db.GF_GamePlayPlayerInfo.Where(x => roundID.Contains(x.RoundID ?? 0))
                    .Select(x => new GamePlayPlayer
                    {
                        RoundID = x.RoundID ?? 0,
                        PlayerNo = x.PlayerNo ?? 0,
                        PlayerName = x.PlayerName
                    }).ToList();

                return lstGamePlayScoreCard.ToList();
            }
            catch
            {
                return new List<GamePlayScoreCard>();
            }
        }

        public List<RoundComparison> GetRoundComparison_Old(long courseID, long GolferID, DateTime DateFrom, DateTime DateTo)
        {
            try
            {
                var cID = new SqlParameter
                {
                    ParameterName = "CourseID",
                    Value = courseID
                };

                var gID = new SqlParameter
                {
                    ParameterName = "GolferID",
                    Value = GolferID
                };

                var dtFrom = new SqlParameter
                {
                    ParameterName = "DateFrom",
                    Value = DateFrom.ToString("yyyy-MM-dd")
                };

                var dtTo = new SqlParameter
                {
                    ParameterName = "DateTo",
                    Value = DateTo.ToString("yyyy-MM-dd")
                };

                var lstRoundComparison = Db.Database.SqlQuery<RoundComparison>("exec GF_SP_GetRoundComparisonReport @CourseID, @GolferID, @DateFrom, @DateTo",
                    cID, gID, dtFrom, dtTo).ToList<RoundComparison>();

                return lstRoundComparison;
            }
            catch
            {
                return new List<RoundComparison>();
            }
        }

        public List<object> GetRoundComparison(long courseID, long GolferID, DateTime DateFrom, DateTime DateTo,
            string campareParameter, double range, string rptType, ref string[] columnNames)
        {
            var results = new List<object>();

            var _db = new GolflerEntities();

            try
            {
                var entityConnection = new EntityConnection("name=GolflerEntities");
                var dbConnection = entityConnection.StoreConnection as SqlConnection;

                var command = new SqlCommand("dbo.GF_SP_GetRoundComparisonReport", dbConnection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@CourseID", courseID);
                command.Parameters.AddWithValue("@GolferID", GolferID);
                command.Parameters.AddWithValue("@DateFrom", DateFrom.ToString("yyyy-MM-dd"));
                command.Parameters.AddWithValue("@DateTo", DateTo.ToString("yyyy-MM-dd"));
                command.Parameters.AddWithValue("@CampareParameter", campareParameter);
                command.Parameters.AddWithValue("@Range", CommonFunctions.ConvertMilesToKilometers(range));
                command.Parameters.AddWithValue("@RptType", rptType);

                dbConnection.Open();

                using (var reader = command.ExecuteReader())
                {
                    // Get the column names
                    columnNames = new string[reader.FieldCount];
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        columnNames[i] = reader.GetName(i);
                    }
                    // Get the actual results

                    while (reader.Read())
                    {
                        var result = new object[reader.FieldCount];
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            if (i == 0)
                            {
                                result[i] = reader[i].ToString();
                            }
                            else if (!string.IsNullOrEmpty(reader[i].ToString()))
                            {
                                result[i] = Convert.ToInt32(reader[i]);
                            }
                            else
                            {
                                result[i] = 0;
                            }
                        }

                        results.Add(result);
                    }
                }

                return results;
            }
            catch (Exception ex)
            {
                return results;
            }
        }


        public List<object> GetPlayingHistoryForGraph(string golferid)
        {
            try
            {
                //var gID = new SqlParameter
                //{
                //    ParameterName = "GolferID",
                //    Value = GolferID
                //};

                //var rType = new SqlParameter
                //{
                //    ParameterName = "ResultType",
                //    Value = ""
                //};

                //var dtFrom = new SqlParameter
                //{
                //    ParameterName = "DateFrom",
                //    Value = DateFrom
                //};

                //var dtTo = new SqlParameter
                //{
                //    ParameterName = "DateTo",
                //    Value = DateTo
                //};

                //var lstWeatherReport = Db.Database.SqlQuery<WeatherReport>("exec GF_SP_GetWeatherAverageReport @GolferID, @ResultType, @DateFrom, @DateTo",
                //    gID, rType, dtFrom, dtTo).ToList<WeatherReport>();

                List<object> weatherList = new List<object>();
                weatherList.Add(new[] { "Course", "Number of times played" });

                weatherList.Add(new[] { "Course 1", "5" });
                weatherList.Add(new[] { "Course 2", "6" });
                weatherList.Add(new[] { "Course 3", "7" });

                //foreach (var row in lstWeatherReport)
                //{
                //  //   weatherList.Add(new[] { row.month, row.temperature, row.windSpeed, row.rain });
                //}

                return weatherList;
            }
            catch
            {
                return null;
            }
        }

        //public void updateCourseCityStateCounty()
        //{
        //    try
        //    {
        //        var lstCourse = Db.GF_CourseInfo.Where(x => string.IsNullOrEmpty(x.CITY) &&
        //            string.IsNullOrEmpty(x.STATE));

        //        string url = "";

        //        foreach (var item in lstCourse)
        //        {
        //            url = "http://maps.googleapis.com/maps/api/geocode/json?address=" + item.ZIPCODE + "&sensor=true";

        //            MyWebRequest myRequest = new MyWebRequest(url);

        //            var jsonString = myRequest.GetResponse();
        //            RootObject result = JsonHelper.JsonDeserialize<RootObject>(jsonString);

        //            if (result != null)
        //            {
        //                if (result.results.Count() > 0)
        //                {
        //                    var data = result.results.FirstOrDefault().address_components.ToArray();

        //                    var courseInfo = new GF_CourseInfo();
        //                    courseInfo = Db.GF_CourseInfo.FirstOrDefault(x => x.ZIPCODE == item.ZIPCODE);

        //                    if (courseInfo != null)
        //                    {
        //                        //courseInfo.CITY = data[1].long_name;
        //                        //courseInfo.COUNTY = data[2].long_name;
        //                        //courseInfo.STATE = data[3].long_name;

        //                        var city = new SqlParameter
        //                        {
        //                            ParameterName = "City",
        //                            Value = data.Count() > 1 ? data[1].long_name : ""
        //                        };

        //                        var county = new SqlParameter
        //                        {
        //                            ParameterName = "County",
        //                            Value = data.Count() > 2 ? data[2].long_name : ""
        //                        };

        //                        var state = new SqlParameter
        //                        {
        //                            ParameterName = "State",
        //                            Value = data.Count() > 3 ? data[3].long_name : ""
        //                        };

        //                        var zip = new SqlParameter
        //                        {
        //                            ParameterName = "ZipCode",
        //                            Value = item.ZIPCODE
        //                        };

        //                        var lstRoundComparison = Db.Database.SqlQuery<zipcode>("exec UpdateCourseCityCountyState @City, @County, @State, @ZipCode",
        //                            city, county, state, zip).ToList<zipcode>();

        //                        //Db.SaveChanges();
        //                    }
        //                }
        //            }
        //        }
        //        Db.SaveChanges();
        //    }
        //    catch (System.Data.Entity.Validation.DbEntityValidationException ex)
        //    {
        //        string errMsg = ex.Message;
        //    }
        //}
    }

    public class WeatherReport
    {
        public string displayText { get; set; }
        public decimal temperature { get; set; }
        public decimal humidity { get; set; }
        public decimal pressure { get; set; }
        public decimal tempMin { get; set; }
        public decimal tempMax { get; set; }
        public decimal windSpeed { get; set; }
        public decimal rain { get; set; }
    }

    public class ScoreAverageReport
    {
        public int HoleNo { get; set; }
        public long Distance { get; set; }
        public int Par { get; set; }
        public int PlayerOneScore { get; set; }
        public int PlayerTwoScore { get; set; }
        public int PlayerThreeScore { get; set; }
        public int PlayerFourScore { get; set; }
        public double Time { get; set; }
    }

    public class GamePlayScoreCard
    {
        public int HoleNo { get; set; }
        public long RoundID { get; set; }
        public long Distance { get; set; }
        public int Par { get; set; }
        public int PlayerOneScore { get; set; }
        public int PlayerTwoScore { get; set; }
        public int PlayerThreeScore { get; set; }
        public int PlayerFourScore { get; set; }
        public long PageNumber { get; set; }
        public string RoundDate { get; set; }
    }

    public class GamePlayPlayer
    {
        public long RoundID { get; set; }
        public int PlayerNo { get; set; }
        public string PlayerName { get; set; }
    }

    public class RoundComparison
    {
        public string CourseName { get; set; }
        public long RoundNo { get; set; }
        public int PlayerScore { get; set; }
    }
}