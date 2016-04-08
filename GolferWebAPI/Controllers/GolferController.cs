using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using GolferWebAPI.Models;
using System.Device.Location;
using System.Text;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;
using System.Data.EntityClient;

namespace GolferWebAPI.Controllers
{
    public class GolferController : ApiController
    {
        GF_Golfer objGolfer = null;
        GolflerEntities _db = new GolflerEntities();
        public GolferController()
        {
            objGolfer = new GF_Golfer();
        }

        #region Registration
        /// <summary>
        /// Created By: Arun
        /// created On: 23 March 2015
        /// Purpose: Registration web service
        /// </summary>
        /// <param name="registration">List of Paramters sent from iphone/android  </param>
        /// <returns>Result class containing three parameters Id of user,Status= 0 or 1,Error in case of fail  </returns>
        [HttpPost]
        public Result RegistrationService([FromBody]GF_Golfer objReg)
        {
            if (objReg.GF_ID > 0)
            {
                if (string.IsNullOrEmpty(objReg.FirstName)
                          || string.IsNullOrEmpty(objReg.LastName)
                    //   || string.IsNullOrEmpty(objReg.Email)
                    //|| string.IsNullOrEmpty(objReg.Password)
                    // || string.IsNullOrEmpty(Convert.ToString(objReg.Tee))
                          || string.IsNullOrEmpty(objReg.Latitude)
                          || string.IsNullOrEmpty(objReg.Longitude)
                          || string.IsNullOrEmpty(Convert.ToString(objReg.IsReceivePushNotification))
                          || string.IsNullOrEmpty(Convert.ToString(objReg))
                    //|| string.IsNullOrEmpty(Convert.ToString(objReg.DeviceSerialNo))
                    )
                {
                    return new Result { Id = 0, Status = 0, record = null, Error = "One of the required parameter is missing." };
                }
            }
            else
            {
                if (string.IsNullOrEmpty(objReg.FirstName)
                          || string.IsNullOrEmpty(objReg.LastName)
                          || string.IsNullOrEmpty(objReg.Email)
                          || string.IsNullOrEmpty(objReg.Password)
                    // || string.IsNullOrEmpty(objReg.MobileNo)
                          || string.IsNullOrEmpty(Convert.ToString(objReg.Tee))
                          || string.IsNullOrEmpty(objReg.Latitude)
                          || string.IsNullOrEmpty(objReg.Longitude)
                    //        || string.IsNullOrEmpty(Convert.ToString(objReg.DeviceSerialNo))
                    )
                {
                    return new Result { Id = 0, Status = 0, record = null, Error = "One of the required parameter is missing." };
                }
            }
            if (objReg.DeviceType.ToLower() == "ios")
            {
                if (string.IsNullOrEmpty(objReg.APNID))
                    return new Result { Id = 0, Status = 0, record = null, Error = "APNID field is empty." };
            }
            else
            {
                if (string.IsNullOrEmpty(objReg.GCMID))
                    return new Result { Id = 0, Status = 0, record = null, Error = "GCMID field is empty." };
            }

            string devSerialNo = "";
            try
            {
                if (objReg.DeviceSerialNo == null)
                {
                    devSerialNo = "";
                }
                else
                {
                    devSerialNo = Convert.ToString(objReg.DeviceSerialNo);
                }
            }
            catch
            {
                devSerialNo = "";
            }
            objReg.DeviceSerialNo = devSerialNo;
            return objGolfer.AddOrUpdateGolferInfo(objReg);
        }
        #endregion

        #region Login
        /// <summary>
        /// Created By: Arun
        /// created On: 23 March 2015
        /// Purpose: Login web service
        /// <param name="log">contains parameters Email and Password for Login</param>
        /// <returns>returns Status 0 or 1 ,Id of user,Error in case of Fail to Login</returns>
        [HttpPost]
        public Result LoginService([FromBody]Login log)
        {
            // || string.IsNullOrEmpty(log.DeviceSerialNo)
            if (string.IsNullOrEmpty(log.Email)
                || string.IsNullOrEmpty(log.Password)
                || string.IsNullOrEmpty(log.DeviceId)
                || string.IsNullOrEmpty(log.DeviceType)
                || string.IsNullOrEmpty(log.Latitude)
                || string.IsNullOrEmpty(log.Longitude)
                )
            {
                return new Result { Id = 0, Status = 0, record = null, Error = "One of the required parameter is missing." };
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
            return log.LoginUser(log);
        }
        #endregion

        #region ForgotPassword
        /// <summary>
        /// Created By: Ankit
        /// created On: 04-nov-2013
        /// </summary>
        /// <param name="Email">User Email where password to send  </param>
        /// <returns>returns Status 0 or 1 ,Id of user,Error in case of Fail to Mailing Service</returns>
        [HttpPost]
        public Result ForgotPasswordService([FromBody]Login log)
        {
            if (string.IsNullOrEmpty(log.Email))
            {
                return new Result { Id = 0, Status = 0, Error = "Email is required.", record = null };
            }
            return objGolfer.ForgotPassword(log.Email);

        }
        #endregion

        #region Golfer Wallet

        public Result GolferWalletService([FromBody] GF_GolferWallet objWallet)
        {

            if (string.IsNullOrEmpty(objWallet.CCName)
                  || objWallet.ExpMonth == 0
                  || objWallet.ExpYear == 0
                  || string.IsNullOrEmpty(objWallet.CCNumber)
                  || string.IsNullOrEmpty(objWallet.Address1)
                  || string.IsNullOrEmpty(objWallet.City)
                  || string.IsNullOrEmpty(objWallet.State)
                  || string.IsNullOrEmpty(objWallet.Country)
                  || string.IsNullOrEmpty(objWallet.Zipcode)
                  || string.IsNullOrEmpty(objWallet.FirstName)
                  || string.IsNullOrEmpty(objWallet.LastName)
                  || objWallet.Golfer_ID == 0
                  || string.IsNullOrEmpty(Convert.ToString(objWallet.IsDefault))
                )
            {
                return new Result { Id = 0, Status = 0, record = null, Error = "One of the required parameter is missing." };
            }

            else if (objWallet.IsDefault == false || objWallet.IsDefault == true)
            {
                return objWallet.AddUpdateGolferWallet(objWallet);
            }
            else
            {
                return new Result { Id = 0, Status = 0, record = null, Error = "IsDefault parameter value is not vaild." };
            }
        }


        /// <summary>
        /// Created By: Amit Kumar
        /// created On: 02 April 2015
        /// Purpose: Get golfer wallet listing by user
        /// </summary>
        /// <param name="objWallet">List of Paramters sent from iphone/android  </param>
        /// <returns>Result class containing three parameters Id of user,Status= 0 or 1,Error in case of fail  </returns>
        public Result GolferWalletList([FromBody] GF_GolferWallet objWallet)
        {
            if (objWallet.Golfer_ID <= 0)
            {
                return new Result { Id = 0, Status = 0, record = null, Error = "One of the required parameter is missing." };
            }
            return objWallet.GetGolferWalletListing(objWallet);
        }

        /// <summary>
        /// Created By: Amit Kumar
        /// created On: 13 April 2015
        /// Purpose: Delete golfer wallet
        /// </summary>
        /// <param name="objWallet">List of Paramters sent from iphone/android  </param>
        /// <returns>Result class containing three parameters Id of user,Status= 0 or 1,Error in case of fail  </returns>
        [HttpPost]
        public Result DeleteGolferWallet([FromBody] GF_GolferWallet objWallet)
        {
            if (objWallet.Golfer_ID <= 0 || string.IsNullOrEmpty(objWallet.CCNumber))
            {
                return new Result { Id = 0, Status = 0, record = null, Error = "One of the required parameter is missing." };
            }
            return objWallet.DeleteWallet(objWallet);
        }

        #endregion

        #region Logout
        /// <summary>
        /// Created By:Arun
        /// Created Date:
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [HttpPost]
        public Result Logout([FromBody]GF_Golfer obj)
        {
            if (obj.GF_ID == 0)
            {
                return new Result { Id = 0, Status = 0, record = null, Error = "One of the required parameter is missing." };
            }
            return obj.Logout(obj.GF_ID);
        }
        #endregion

        #region Manage Orders

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
            if (string.IsNullOrEmpty(Convert.ToString(orderHistory.golferID)))
            {
                return new Result { Id = 0, Status = 0, record = null, Error = "One of the required parameter is missing." };
            }

            GF_Order obj = new GF_Order();
            return (obj.GetOrderHistoryList(orderHistory));
        }

        #endregion

        #region Update Golfer Location
        public delegate void delUpdatePlay(GF_GolferChangingLocation objLoc, string sb);
        public static delUpdatePlay invUpdatePlay;

        public static void CallbackDelegatedFunctions(IAsyncResult t)
        {
            try
            {
                invUpdatePlay.EndInvoke(t);
            }
            catch (Exception ex)
            {
                //throw ex;
                List<string> msg = new List<string>();
                msg.Add("Exception in callback update locaton: " + Convert.ToString(ex.Message));
                LogClass.WriteLog(msg);
            }
        }

        public static void CallDelegatedFunctions(GF_GolferChangingLocation objLoc, string sb)
        {
            //if (objLoc.LocType.ToLower() == "online")
            //{
            //    GolflerEntities DbGolfer = new GolflerEntities();

            //    if (Convert.ToString(objLoc).EndsWith("|"))
            //    {
            //        objLoc.LocDetails = objLoc.LocDetails.Substring(0, objLoc.LocDetails.Length - 1);
            //    }

            //    string[] objDetails = Convert.ToString(objLoc.LocDetails).Split('|');

            //    foreach (var objDet in objDetails)
            //    {
            //        if (objDet.Length > 0)
            //        {
            //            string[] strArrDet = objDet.Split(',');

            //            objLoc.GolferId = Convert.ToInt64(strArrDet[0]);
            //            objLoc.Latitude = Convert.ToString(strArrDet[1]);
            //            objLoc.Longitude = Convert.ToString(strArrDet[2]);
            //            objLoc.TimeOfChange = Convert.ToDateTime(strArrDet[3]);

            //            //Int64 intCourseId = Convert.ToInt64(DbGolfer.GF_GolferUser.Where(x => x.GolferID == objLoc.GolferId).Select(x => x.CourseID).FirstOrDefault());
            //            string GameStartsFrom = "F";
            //            Int64 intCourseId = 0;
            //            var courseDet = DbGolfer.GF_GamePlayRound.Where(x => x.GolferID == objLoc.GolferId && x.IsQuit == false).OrderByDescending(x => x.CreatedDate).FirstOrDefault();
            //            if (courseDet != null)
            //            {
            //                intCourseId = Convert.ToInt64(courseDet.CourseID);
            //                if (!(string.IsNullOrEmpty(Convert.ToString(courseDet.RoundStartFrom))))
            //                {
            //                    GameStartsFrom = Convert.ToString(courseDet.RoundStartFrom);
            //                }
            //            }

            //            if (intCourseId > 0)
            //            {
            //                Int64 NextHole = 0;
            //                string NextPosition = "";

            //                #region Check User Previous Location
            //                var prevPosition = DbGolfer.GF_GolferPaceofPlay.Where(x => x.GolferId == objLoc.GolferId && x.Status == StatusType.Active && x.CourseId == intCourseId).OrderByDescending(x => x.Time).FirstOrDefault();
            //                if (prevPosition != null)  // Play has already been started.
            //                {
            //                    //Hole = prevPosition.HoleNumber ?? 0;

            //                    if (prevPosition.Position == "Tee")
            //                    {
            //                        NextHole = prevPosition.HoleNumber ?? 1;
            //                        NextPosition = "Flag";
            //                    }
            //                    if (prevPosition.Position == "Flag")
            //                    {
            //                        NextHole = Convert.ToInt64(prevPosition.HoleNumber) + 1;
            //                        NextPosition = "Tee";
            //                    }
            //                }
            //                else  // Play has not started yet.
            //                {
            //                    // check for Front play or Back Play
            //                    if (GameStartsFrom == "F")
            //                    {
            //                        NextHole = 1;
            //                        NextPosition = "Tee";
            //                    }
            //                    else
            //                    {
            //                        NextHole = 9;
            //                        NextPosition = "Tee";
            //                    }
            //                }
            //                #endregion

            //                #region Check Golfer Position near hole's position
            //                string Courselatitude = "";
            //                string Courselongitude = "";



            //                string[] dragitem = new string[3];
            //                if (NextPosition == "Tee")
            //                {
            //                    //// Tee =1 -- Professional  -- Blue TEE
            //                    //// Tee=2 -- Gental man -- White Tee
            //                    //// Tee=3 -- lady -- Red Tee
            //                    //string strTeePosition = Convert.ToString(DbGolfer.GF_Golfer.Where(x => x.GF_ID == objLoc.GolferId).Select(x => x.Tee).FirstOrDefault());
            //                    //if (!string.IsNullOrEmpty(Convert.ToString(strTeePosition)))
            //                    //{
            //                    //    Int64 TeePosition = 0;
            //                    //    try
            //                    //    {
            //                    //        TeePosition = Convert.ToInt64(strTeePosition);
            //                    //    }
            //                    //    catch
            //                    //    {
            //                    //        TeePosition = 0;
            //                    //    }

            //                    //    if (TeePosition > 0)
            //                    //    {
            //                    //        if (TeePosition == 1)
            //                    //        {
            //                    dragitem[0] = DragItemType.BlueTee;
            //                    //}
            //                    //if (TeePosition == 2)
            //                    //{
            //                    dragitem[1] = DragItemType.WhiteTee;

            //                    //}
            //                    //if (TeePosition == 3)
            //                    //{
            //                    dragitem[2] = DragItemType.RedTee;
            //                    //        }
            //                    //    }

            //                    //} 
            //                }
            //                else
            //                {
            //                    dragitem[0] = DragItemType.WhiteFlag;
            //                }

            //                var entrystat = true;
            //                foreach (var dragItm in dragitem)
            //                {
            //                    if (entrystat && (!string.IsNullOrEmpty(Convert.ToString(dragItm))))
            //                    {

            //                        CommonFunctions.GetLatLong(intCourseId, NextHole, dragItm, ref Courselatitude, ref Courselongitude);

            //                        if (Courselatitude != "" && Courselongitude != "")
            //                        {

            //                            //double miles = 1;
            //                            double meters = Convert.ToDouble(System.Web.Configuration.WebConfigurationManager.AppSettings["MetersForPaceOfPlay"]); //CommonFunctions.ConvertMilesToMeters(miles);

            //                            var holeCordinate = new GeoCoordinate(Convert.ToDouble(Courselatitude), Convert.ToDouble(Courselongitude));
            //                            var golfercordinate = new GeoCoordinate(Convert.ToDouble(objLoc.Latitude), Convert.ToDouble(objLoc.Longitude));

            //                            double distance = holeCordinate.GetDistanceTo(golfercordinate);

            //                            if (distance <= meters)
            //                            {
            //                                // Insert entry
            //                                GF_GolferPaceofPlay objPlay = new GF_GolferPaceofPlay();
            //                                objPlay.GolferId = objLoc.GolferId;
            //                                objPlay.Latitude = objLoc.Latitude;
            //                                objPlay.Longitude = objLoc.Longitude;
            //                                objPlay.Time = objLoc.TimeOfChange;
            //                                objPlay.HoleNumber = Convert.ToInt32(NextHole);
            //                                objPlay.Position = NextPosition;
            //                                objPlay.CourseId = intCourseId;
            //                                objPlay.Status = StatusType.Active;
            //                                objPlay.GameRoundID = courseDet.ID;
            //                                DbGolfer.GF_GolferPaceofPlay.Add(objPlay);
            //                                DbGolfer.SaveChanges();

            //                                entrystat = false;
            //                            }
            //                        }
            //                    }
            //                }
            //                #endregion
            //            }
            //        }
            //    }
            //}
            //else
            //{
            try
            {
                GolflerEntities DbGolfer = new GolflerEntities();

                if (Convert.ToString(objLoc).EndsWith("|"))
                {
                    objLoc.LocDetails = objLoc.LocDetails.Substring(0, objLoc.LocDetails.Length - 1);
                }

                string[] objDetails = Convert.ToString(objLoc.LocDetails).Split('|');
                if (objDetails.Count() > 0)
                {
                    var firstObjDet = objDetails.FirstOrDefault();
                    string[] strArrDet = firstObjDet.Split(',');

                    objLoc.GolferId = Convert.ToInt64(strArrDet[0]);
                    objLoc.Latitude = Convert.ToString(strArrDet[1]);
                    objLoc.Longitude = Convert.ToString(strArrDet[2]);
                    objLoc.TimeOfChange = Convert.ToDateTime(strArrDet[3]);

                    string GameStartsFrom = "F";
                    Int64 intCourseId = 0;
                    var courseDet = DbGolfer.GF_GamePlayRound.Where(x => x.GolferID == objLoc.GolferId && x.IsQuit == false).OrderByDescending(x => x.CreatedDate).FirstOrDefault();
                    if (courseDet != null)
                    {
                        intCourseId = Convert.ToInt64(courseDet.CourseID);
                        if (!(string.IsNullOrEmpty(Convert.ToString(courseDet.RoundStartFrom))))
                        {
                            GameStartsFrom = Convert.ToString(courseDet.RoundStartFrom);
                        }
                    }
                    else
                    {
                        // Create new round     
                        Int64 newCourseId = 0;
                        var objCourseFind = DbGolfer.GF_GolferUser.Where(x => x.GolferID == objLoc.GolferId).FirstOrDefault();
                        if (objCourseFind != null)
                        {
                            newCourseId = Convert.ToInt64(objCourseFind.CourseID);
                            if (newCourseId > 0)
                            {
                                GamePlay objGamePlay = new GamePlay();
                                GF_GamePlayRound objPlayRound = new GF_GamePlayRound();
                                objPlayRound.GolferID = objLoc.GolferId;
                                objPlayRound.CourseID = newCourseId;
                                objPlayRound.RoundStartFrom = "F";

                                Result objResult = objGamePlay.SaveGamePlayRound(objPlayRound);
                                if (objResult.Status == 1)
                                {
                                    courseDet = DbGolfer.GF_GamePlayRound.Where(x => x.GolferID == objLoc.GolferId && x.IsQuit == false).OrderByDescending(x => x.CreatedDate).FirstOrDefault();
                                    if (courseDet != null)
                                    {
                                        intCourseId = Convert.ToInt64(courseDet.CourseID);
                                        GameStartsFrom = Convert.ToString(courseDet.RoundStartFrom);
                                    }
                                }
                            }
                        }
                    }

                    if (intCourseId > 0)
                    {
                        // var spResult = DbGolfer.GF_SP_BulkInsertPaceOfPlay(sb.ToString(), intCourseId, GameStartsFrom, courseDet.ID, Convert.ToDecimal(System.Web.Configuration.WebConfigurationManager.AppSettings["MetersForPaceOfPlay"]));

                        var entityConnection = new EntityConnection("name=GolflerEntities");
                        var con = entityConnection.StoreConnection as SqlConnection;

                        //using (SqlConnection con = new SqlConnection(dbConnection))
                        //{

                        decimal meterSearch = Convert.ToDecimal(System.Web.Configuration.WebConfigurationManager.AppSettings["MetersForPaceOfPlay"]);
                        var courseSettings = DbGolfer.GF_Settings.Where(x => x.CourseID == intCourseId && x.Name == "Hole Radius" && x.Status == StatusType.Active && x.IsActive == true).FirstOrDefault();
                        if (courseSettings != null)
                        {
                            if (!(string.IsNullOrEmpty(Convert.ToString(courseSettings.Value))))
                            {
                                try
                                {
                                    meterSearch = Convert.ToDecimal(courseSettings.Value);
                                }
                                catch
                                {
                                    meterSearch = Convert.ToDecimal(System.Web.Configuration.WebConfigurationManager.AppSettings["MetersForPaceOfPlay"]);
                                }
                            }
                        }


                        SqlCommand cmd = new SqlCommand("GF_SP_BulkInsertPaceOfPlay", con);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@XMLData", sb.ToString());
                        cmd.Parameters.AddWithValue("@intCourseId", intCourseId);
                        cmd.Parameters.AddWithValue("@GameStartsFrom", GameStartsFrom);
                        cmd.Parameters.AddWithValue("@GameRoundID", courseDet.ID);
                        cmd.Parameters.AddWithValue("@meterSearch", meterSearch);

                        if (con.State != ConnectionState.Open)
                        {
                            con.Open();
                        }
                        cmd.CommandTimeout = 900;
                        int affRow = cmd.ExecuteNonQuery();
                        //if (affRow > 0)
                        //{
                        //    lblMsg.Text = "Successfully " + affRow + " record inserted.";
                        //    PopulateData();
                        //    AddRowsToGrid();
                        //}
                        //}
                    }
                }
                List<string> msg = new List<string>();
                msg.Add("Pace of play test msg");
                LogClass.WriteLog(msg);
                //}
            }
            catch (Exception ex)
            {
                List<string> msg = new List<string>();
                msg.Add("Exception during Pace of play: " + Convert.ToString(ex.Message));
                LogClass.WriteLog(msg);
                //string m = ex.Message;
            }
        }

        [HttpPost]
        public Result UpdatePaceOfPlay([FromBody]GF_GolferPaceofPlay objLoc)
        {
            List<string> msg = new List<string>();
            msg.Add("UpdatePaceOfPlay start.");

            try
            {
                if (string.IsNullOrEmpty(Convert.ToString(objLoc.PaceOfPlayDetails)) || string.IsNullOrEmpty(objLoc.PaceOfPlayType))
                {
                    msg.Add("Required parameter missing.");
                    LogClass.WriteLog(msg);
                    return new Result { Status = 0, Error = "One of the required parameter is missing." };
                }

                var ObjResult = new Result();
                var dbGolferEntity = new GolflerEntities();

                if (Convert.ToString(objLoc.PaceOfPlayDetails).EndsWith("|"))
                {
                    objLoc.PaceOfPlayDetails = objLoc.PaceOfPlayDetails.Substring(0, objLoc.PaceOfPlayDetails.Length - 1);
                }

                string[] objDetails = Convert.ToString(objLoc.PaceOfPlayDetails).Split('|');

                //string dtTim = "";
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("<?xml version=\"1.0\" ?>");
                sb.AppendLine("     <GF_GolferPaceofPlay>");

                //DateTime newDateTime = DateTime.Now;
                //DateTime calculatedDateTime = DateTime.Now;
                foreach (var objDet in objDetails)
                {
                    try
                    {
                        if (objDet.Length > 0)
                        {
                            string[] strArrDet = objDet.Split(',');

                            //if (Convert.ToString(objLoc.PaceOfPlayType).ToLower() == "offline")
                            //{
                            //    if (dtTim == "calculate")
                            //    {
                            //        newDateTime = newDateTime.AddSeconds(15);
                            //    }
                            //    else
                            //    {
                            //        Int64 gid = Convert.ToInt64(strArrDet[0]);
                            //        var lastTime = dbGolferEntity.GF_GolferChangingLocation.Where(x => x.GolferId == gid).OrderByDescending(x => x.TimeOfChange).FirstOrDefault();
                            //        if (lastTime != null)
                            //        {
                            //            calculatedDateTime = Convert.ToDateTime(lastTime.TimeOfChange);
                            //            newDateTime = calculatedDateTime.AddSeconds(15);
                            //        }
                            //        else
                            //        {
                            //            calculatedDateTime = Convert.ToDateTime(strArrDet[3]);
                            //            newDateTime = calculatedDateTime;
                            //        }

                            //        dtTim = "calculate";
                            //    }
                            //}
                            //else
                            //{
                            //    newDateTime = Convert.ToDateTime(strArrDet[3]);
                            //}

                            sb.AppendLine("     <GolferPaceofPlay>");
                            sb.AppendLine("         <GolferId>" + Convert.ToInt64(strArrDet[0]) + "</GolferId>");
                            sb.AppendLine("         <Latitude>" + Convert.ToString(strArrDet[1]) + "</Latitude>");
                            sb.AppendLine("         <Longitude>" + Convert.ToString(strArrDet[2]) + "</Longitude>");

                            string dt = Convert.ToString(strArrDet[3]);
                            if (dt.Contains("+"))
                            {
                                string[] arrDt = dt.Split('+');
                                dt = arrDt[0].Trim();
                            }

                            sb.AppendLine("         <Time>" + Convert.ToDateTime(dt) + "</Time>");

                            sb.AppendLine("         <HoleNumber>" + Convert.ToInt64(strArrDet[4]) + "</HoleNumber>");
                            sb.AppendLine("         <Position>" + Convert.ToString(strArrDet[5]) + "</Position>");
                            sb.AppendLine("         <Status>" + Convert.ToString("A") + "</Status>");
                            sb.AppendLine("         <GameRoundID>" + Convert.ToInt64(strArrDet[6]) + "</GameRoundID>");
                            sb.AppendLine("         <CourseId>" + Convert.ToInt64(strArrDet[7]) + "</CourseId>");

                            sb.AppendLine("         <PaceOfPlayDetails>" + Convert.ToString(objLoc.PaceOfPlayDetails) + "</PaceOfPlayDetails>"); //  
                            sb.AppendLine("         <PaceOfPlayType>" + Convert.ToString(objLoc.PaceOfPlayType) + "</PaceOfPlayType>");
                            sb.AppendLine("     </GolferPaceofPlay>");

                            // lstLocDetails.Add(newObj);
                            //  dbGolferEntity.GF_GolferChangingLocation.Add(newObj);
                        }
                    }
                    catch (Exception ex)
                    {
                        msg.Add("pace of play Inner Exception: " + Convert.ToString(ex.Message));
                    }
                }
                sb.AppendLine("     </GF_GolferPaceofPlay>");

                var spResult = dbGolferEntity.GF_SP_BulkInsertPaceOfPlay(sb.ToString());

                msg.Add("pace of play SP execute: " + Convert.ToString(spResult));
                LogClass.WriteLog(msg);

                return new Result { Status = 1, Error = Convert.ToString("") };
            }
            catch (Exception ex)
            {
                msg.Add("pace of play Exception: " + Convert.ToString(ex.Message));
                LogClass.WriteLog(msg);
                return new Result { Status = 0, Error = Convert.ToString(ex.Message) };
            }
            msg.Add("pace of play End.");
            LogClass.WriteLog(msg);
        }

        /// <summary>
        /// Created By: Veera
        /// created On: 3 April 2015
        /// Purpose: Update Golfer Location web service
        /// </summary>
        /// <param name="registration">List of Paramters sent from iphone/android  </param>
        /// <returns>Result class containing three parameters Id of user,Status= 0 or 1,Error in case of fail  </returns>
        [HttpPost]
        public Result UpdateGolferLocationService([FromBody]GF_GolferChangingLocation objLoc)
        {
            try
            {
                if (string.IsNullOrEmpty(Convert.ToString(objLoc.LocDetails)) || string.IsNullOrEmpty(objLoc.LocType))
                {
                    return new Result { Id = 0, Status = 0, record = null, Error = "One of the required parameter is missing." };
                }

                //  List<GF_GolferChangingLocation> lstLocDetails = new List<GF_GolferChangingLocation>();
                var ObjResult = new Result();
                var dbGolferEntity = new GolflerEntities();

                if (Convert.ToString(objLoc.LocDetails).EndsWith("|"))
                {
                    objLoc.LocDetails = objLoc.LocDetails.Substring(0, objLoc.LocDetails.Length - 1);
                }

                string[] objDetails = Convert.ToString(objLoc.LocDetails).Split('|');

                string dtTim = "";
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("<?xml version=\"1.0\" ?>");
                sb.AppendLine("     <GF_GolferChangingLocation>");

                DateTime newDateTime = DateTime.Now;
                DateTime calculatedDateTime = DateTime.Now;
                foreach (var objDet in objDetails)
                {
                    try
                    {
                        if (objDet.Length > 0)
                        {
                            string[] strArrDet = objDet.Split(',');

                            string dt = Convert.ToString(strArrDet[3]);
                            if (dt.Contains("+"))
                            {
                                string[] arrDt = dt.Split('+');
                                dt = arrDt[0].Trim();
                            }


                            if (Convert.ToString(objLoc.LocType).ToLower() == "offline")
                            {
                                if (dtTim == "calculate")
                                {
                                    newDateTime = newDateTime.AddSeconds(15);
                                }
                                else
                                {
                                    Int64 gid = Convert.ToInt64(strArrDet[0]);
                                    var lastTime = dbGolferEntity.GF_GolferChangingLocation.Where(x => x.GolferId == gid).OrderByDescending(x => x.TimeOfChange).FirstOrDefault();
                                    if (lastTime != null)
                                    {
                                        calculatedDateTime = Convert.ToDateTime(lastTime.TimeOfChange);
                                        newDateTime = calculatedDateTime.AddSeconds(15);
                                    }
                                    else
                                    {
                                        calculatedDateTime = Convert.ToDateTime(dt);
                                        newDateTime = calculatedDateTime;
                                    }

                                    dtTim = "calculate";
                                }
                            }
                            else
                            {
                                newDateTime = Convert.ToDateTime(dt);
                            }

                            sb.AppendLine("     <GolferChangingLocation>");
                            sb.AppendLine("         <GolferId>" + Convert.ToInt64(strArrDet[0]) + "</GolferId>");
                            sb.AppendLine("         <Latitude>" + Convert.ToString(strArrDet[1]) + "</Latitude>");
                            sb.AppendLine("         <Longitude>" + Convert.ToString(strArrDet[2]) + "</Longitude>");


                            sb.AppendLine("         <TimeOfChange>" + Convert.ToDateTime(newDateTime) + "</TimeOfChange>");
                            try
                            {
                                sb.AppendLine("         <HoleNumber>" + Convert.ToInt64(strArrDet[4]) + "</HoleNumber>");
                            }
                            catch
                            {
                                sb.AppendLine("         <HoleNumber>" + Convert.ToInt64("0") + "</HoleNumber>");
                            }
                            sb.AppendLine("         <LocDetails>" + Convert.ToString(objLoc.LocDetails) + "</LocDetails>"); //  
                            sb.AppendLine("         <LocType>" + Convert.ToString(objLoc.LocType) + "</LocType>");
                            sb.AppendLine("     </GolferChangingLocation>");

                            // lstLocDetails.Add(newObj);
                            //  dbGolferEntity.GF_GolferChangingLocation.Add(newObj);
                        }
                    }
                    catch (Exception ex)
                    { }
                }
                sb.AppendLine("     </GF_GolferChangingLocation>");

                var spResult = dbGolferEntity.GF_SP_BulkInsertGolflerLocation(sb.ToString());

                //if (objLoc.LocType.ToLower() == "online")
                //{
                //    dbGolferEntity.SaveChanges();
                //}
                //else
                //{
                //    SaveBulkLocation(lstLocDetails);
                //}

                // ObjResult = objGolfer.UpdateGolferLocation(lstLocDetails);

                #region Update Pace of Play by Async call
                //invUpdatePlay = new delUpdatePlay(CallDelegatedFunctions);
                //invUpdatePlay.BeginInvoke(objLoc, Convert.ToString(sb), new AsyncCallback(CallbackDelegatedFunctions), null);
                #endregion


                // return ObjResult;
                return new Result { Id = 0, Status = 1, record = null, Error = "Success." };
            }
            catch (Exception ex)
            {
                return new Result { Id = 0, Status = 0, record = null, Error = Convert.ToString(ex.Message) };
            }
        }

        public void SaveBulkLocation(List<GF_GolferChangingLocation> lstLocDetails)
        {
            GolflerEntities gfSaveBulk = new GolflerEntities();
            // Save Here

            foreach (var objLocation in lstLocDetails)
            {


            }

            //using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["con"].ConnectionString))
            //{
            //    SqlCommand cmd = new SqlCommand("ContactBulkInsert", con);
            //    cmd.CommandType = CommandType.StoredProcedure;
            //    cmd.Parameters.AddWithValue("@XMLData", sb.ToString());
            //    if (con.State != ConnectionState.Open)
            //    {
            //        con.Open();
            //    }

            //    int affRow = cmd.ExecuteNonQuery();
            //    if (affRow > 0)
            //    {
            //        lblMsg.Text = "Successfully " + affRow + " record inserted.";
            //        PopulateData();
            //        AddRowsToGrid();
            //    }
            //}
        }
        #endregion

        #region List of  Golfers for Course--Map Service

        /// <summary>
        /// Created By: Veera
        /// created On: 3 April 2015
        /// Purpose: List of Golfers
        /// </summary>
        /// <param name="objOrder">List of Paramters sent from iphone/android  </param>
        /// <returns>Result class containing three parameters Id of user,Status= 0 or 1,Error in case of fail  </returns>
        [HttpPost]
        public GolferMapResult ListOfGolfersByCourseId([FromBody]GF_Courses objCourse)
        {
            if (string.IsNullOrEmpty(Convert.ToString(objCourse.ID)))
            {
                return new GolferMapResult { Status = 0, Golfers = null, Error = "One of the required parameter is missing." };
            }

            GF_Golfer obj = new GF_Golfer();
            return (obj.GetGolferByCourseIdList(Convert.ToString(objCourse.ID)));
        }

        /// <summary>
        /// Created By: Ramesh Kalra
        /// created On: 12 May 2015
        /// Purpose: List of Golfers with Hole Positions
        /// </summary>
        /// <param name="objOrder">List of Paramters sent from iphone/android  </param>
        /// <returns>Result class containing List of Golfers with Hole Positions </returns>
        [HttpPost]
        public GolferMapResult GetGolfersPositionByCourseId([FromBody]GF_Courses objCourse)
        {
            if (string.IsNullOrEmpty(Convert.ToString(objCourse.ID)))
            {
                return new GolferMapResult { Status = 0, Golfers = null, Error = "One of the required parameter is missing." };
            }

            GF_Golfer obj = new GF_Golfer();
            return (obj.GetGolfersPositionByCourseId(Convert.ToString(objCourse.ID)));
        }
        #endregion

        #region Rating

        /// <summary>
        /// Created By: Amit Kumar
        /// created On: 13 April 2015
        /// Purpose: Cartie rating given by golfer
        /// </summary>
        /// <param name="objWallet">List of Paramters sent from iphone/android  </param>
        /// <returns>Result class containing three parameters Id of user,Status= 0 or 1,Error in case of fail  </returns>
        [HttpPost]
        public Result SaveGolferRating([FromBody]GF_GolferRating rating)
        {
            if (rating.Rating < 0 ||
                rating.ReferenceID <= 0 ||
                string.IsNullOrEmpty(rating.ReferenceType) ||
                rating.GolferID <= 0 ||
                rating.OrderNo <= 0 ||
                rating.CourseId <= 0)
            {
                return new Result { Id = 0, Status = 0, record = null, Error = "One of the required parameter is missing." };
            }

            return rating.SaveGolferRating(rating);
        }

        #endregion

        #region Manage Cartie

        /// <summary>
        /// Created By: Amit Kumar
        /// created On: 17 April 2015
        /// Purpose: Cartie's online/offline status
        /// </summary>
        /// <param name="objWallet">List of Paramters sent from iphone/android  </param>
        /// <returns>Result class containing three parameters Id of user,Status= 0 or 1,Error in case of fail  </returns>
        [HttpPost]
        public ResultOrder CourseCartieStatus([FromBody]GF_AdminUsers adminUsers)
        {
            if (adminUsers.CourseId <= 0)
            {
                return new ResultOrder { Id = 0, Status = 0, record = null, Error = "One of the required parameter is missing." };
            }
            //if (string.IsNullOrEmpty(Convert.ToString(adminUsers.DeviceSerialNo)))
            //{
            //    return new ResultOrder { Id = 0, Status = 0, record = null, Error = "One of the required parameter is missing." };
            //}
            GF_Order obj = new GF_Order();
            return obj.getCourseCartieStatus(adminUsers);
        }

        /// <summary>
        /// Created By: Amit Kumar
        /// created On: 17 April 2015
        /// Purpose: Get course user current position
        /// </summary>
        /// <param name="objWallet">List of Paramters sent from iphone/android  </param>
        /// <returns>Result class containing three parameters Id of user,Status= 0 or 1,Error in case of fail  </returns>
        [HttpPost]
        public Result CourseUserPosition([FromBody]GF_UserCurrentPosition userPosition)
        {
            if (userPosition.OrderID <= 0 ||
                //userPosition.ReferenceID <= 0 ||
                string.IsNullOrEmpty(userPosition.ReferenceType) ||
                userPosition.CourseID <= 0)
            {
                return new Result { Id = 0, Status = 0, record = null, Error = "One of the required parameter is missing." };
            }

            GF_Order obj = new GF_Order();
            return obj.getCourseUserPosition(userPosition);
        }

        #endregion

        #region Feedback

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
            GolferController objGolferController = new GolferController();
            objGolferController.SendMsgToReceiver(Convert.ToString(syncObj.FeedbackTest), Convert.ToString(syncObj.ResolutionType), Convert.ToInt64(syncObj.GolferID), Convert.ToString(syncObj.SendTo), Convert.ToInt64(syncObj.CourseID));

            Int64 gid = 0;
            gid = Convert.ToInt64(syncObj.GolferID);

            var golf = _db.GF_Golfer.Where(x => x.GF_ID == gid).FirstOrDefault();
            if (golf != null)
            {
                objGolferController.SendAutoResponseToGolfer(syncObj.FeedbackTest, Convert.ToInt64(syncObj.GolferID), golf.Email, (golf.FirstName + " " + golf.LastName));
            }
        }


        /// <summary>
        /// Created By: Amit Kumar
        /// Created Date: 17 April 2015
        /// Purpose: Save message center by golfer.
        /// </summary>
        /// <param name="feedback">Save MESSAGE CENTER by golfer</param>
        /// <returns>Result class containing four parameters Id of user,Status= 0 or 1, Error in case of fail and record which is used return the result</returns>
        public Result AddGolferFeedback([FromBody]GF_ResolutionCenter obj)
        {
            if (string.IsNullOrEmpty(Convert.ToString(obj.CourseID)) ||
                string.IsNullOrEmpty(Convert.ToString(obj.GolferID)) ||
                string.IsNullOrEmpty(obj.FeedbackTest) ||
                string.IsNullOrEmpty(obj.SendTo) ||
                string.IsNullOrEmpty(obj.ResolutionType))
            {
                return new Result { Id = 0, Status = 0, record = null, Error = "One of the required parameter is missing." };
            }
            if (obj.SendTo != UserType.SuperAdmin && obj.SendTo != UserType.Proshop && obj.SendTo != "AL")
            {
                return new Result { Id = 0, Status = 0, record = null, Error = "Send Type should be SA (Super Admin) or CP (Course Proshop) or AL (All)." };
            }

            #region Asyn Call
            GF_ResolutionCenter syncObj = obj;
            invFeedback = new delFeedback(CallDelegatedFunctionsFeedback);
            invFeedback.BeginInvoke(syncObj, new AsyncCallback(CallbackDelegatedFunctionsFeedback), null);
            #endregion

            return obj.AddGolferResolutionCenter(obj);
        }

        #endregion

        #region Feedback Send Email
        public void SendMsgToReceiver(string comment, string resolutiontype, Int64 golferID, string sentTo, Int64 courseID)
        {
            try
            {
                string resolutionType = "others";
                if (resolutiontype.ToLower() == "c")
                {
                    resolutionType = "complaint";
                }
                else
                {
                    if (resolutiontype.ToLower() == "p")
                    {
                        resolutionType = "praise";
                    }
                }
                string email = "";
                string name = "";
                IQueryable<GF_EmailTemplatesFields> templateFields = null;
                var param = EmailParams.GetEmailParams(ref _db, "Resolution Message", ref templateFields);


                string sendername = "";
                string senderemail = "";
                string sendercourse = "";
                if (Convert.ToInt64(golferID) > 0)
                {
                    var senderDet = _db.GF_Golfer.Where(x => x.GF_ID == golferID).FirstOrDefault();
                    if (senderDet != null)
                    {
                        sendername = Convert.ToString(senderDet.FirstName) + " " + Convert.ToString(senderDet.LastName);
                        senderemail = Convert.ToString(senderDet.Email);
                    }
                }
                if (Convert.ToInt64(courseID) > 0)
                {
                    var senderCourseDet = _db.GF_CourseInfo.Where(x => x.ID == courseID).FirstOrDefault();
                    if (senderCourseDet != null)
                    {
                        sendercourse = Convert.ToString(senderCourseDet.COURSE_NAME);
                    }
                }

                if (sentTo.ToUpper() == UserType.SuperAdmin || sentTo.ToUpper() == UserType.Admin)
                {//if sent to admin/superadmin

                    #region send EMail

                    var admin = _db.GF_AdminUsers.Where(x => (x.Type == "SA" || x.Type == "A") && x.Status == StatusType.Active && (x.ReceiveResolutionMails ?? false == true)).ToList();

                    #region  Add Super Admin
                    var varSuperadmin = _db.GF_AdminUsers.Where(x => x.Status == StatusType.Active && x.Type == UserType.SuperAdmin).ToList();
                    foreach (var objSuperAdmin in varSuperadmin)
                    {
                        if (!(admin.Where(x => x.ID == objSuperAdmin.ID).Count() > 0))
                        {
                            admin.Add(objSuperAdmin);
                        }
                    }
                    #endregion

                    foreach (var usr in admin)
                    {
                        email = usr.Email;
                        name = usr.FirstName + " " + usr.LastName;

                        if (!ApplicationEmails.SendResolutionMsgEMail(sendername, senderemail, sendercourse, golferID, comment, resolutionType, email, name, param, ref templateFields))
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


                    var admin = _db.GF_AdminUsers.Where(x => x.CourseId == courseID && x.Status == StatusType.Active && (x.ReceiveResolutionMails ?? false == true)).ToList();

                    #region  Add Course Admin
                    var varCourseadmin = _db.GF_AdminUsers.Where(x => x.CourseId == courseID && x.Status == StatusType.Active && x.Type == UserType.CourseAdmin).ToList();
                    foreach (var objCourseAdmin in varCourseadmin)
                    {
                        if (!(admin.Where(x => x.ID == objCourseAdmin.ID).Count() > 0))
                        {
                            admin.Add(objCourseAdmin);
                        }
                    }
                    #endregion

                    foreach (var usr in admin)
                    {
                        email = usr.Email;
                        name = usr.FirstName + " " + usr.LastName;

                        if (!ApplicationEmails.SendResolutionMsgEMail(sendername, senderemail, sendercourse, golferID, comment, resolutionType, email, name, param, ref templateFields))
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

                    var admin = _db.GF_AdminUsers.Where(x => (x.Type == "SA" || x.Type == "A") && x.Status == StatusType.Active && (x.ReceiveResolutionMails ?? false == true)).ToList();

                    #region  Add Super Admin
                    var varSuperadmin = _db.GF_AdminUsers.Where(x => x.Status == StatusType.Active && x.Type == UserType.SuperAdmin).ToList();
                    foreach (var objSuperAdmin in varSuperadmin)
                    {
                        if (!(admin.Where(x => x.ID == objSuperAdmin.ID).Count() > 0))
                        {
                            admin.Add(objSuperAdmin);
                        }
                    }
                    #endregion


                    foreach (var usr in admin)
                    {
                        email = usr.Email;
                        name = usr.FirstName + " " + usr.LastName;

                        if (!ApplicationEmails.SendResolutionMsgEMail(sendername, senderemail, sendercourse, golferID, comment, resolutionType, email, name, param, ref templateFields))
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

                    admin = _db.GF_AdminUsers.Where(x => x.CourseId == courseID && x.Status == StatusType.Active && (x.ReceiveResolutionMails ?? false == true)).ToList();

                    #region  Add Course Admin
                    var varCourseadmin = _db.GF_AdminUsers.Where(x => x.CourseId == courseID && x.Status == StatusType.Active && x.Type == UserType.CourseAdmin).ToList();
                    foreach (var objCourseAdmin in varCourseadmin)
                    {
                        if (!(admin.Where(x => x.ID == objCourseAdmin.ID).Count() > 0))
                        {
                            admin.Add(objCourseAdmin);
                        }
                    }
                    #endregion

                    foreach (var usr in admin)
                    {
                        email = usr.Email;
                        name = usr.FirstName + " " + usr.LastName;

                        if (!ApplicationEmails.SendResolutionMsgEMail(sendername, senderemail, sendercourse, golferID, comment, resolutionType, email, name, param, ref templateFields))
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

        #region Static Page

        /// <summary>
        /// Created By: Kiran Bala
        /// created On: 21 April 2015
        /// Purpose: To fetch static page detail based on static code
        /// </summary>
        /// <param name="objStatic">List of Paramters sent from iphone/android  </param>
        /// <returns>Result class containing three parameters Id of user,Status= 0 or 1,Error in case of fail  </returns>
        [HttpPost]
        public Result StaticPageDetail([FromBody]GF_StaticPages objStatic)
        {
            if (objStatic.PageCode == "")
            {
                return new Result { Id = 0, Status = 0, record = null, Error = "Page Code is missing." };
            }

            StaticClass obj = new StaticClass();
            return obj.GetPageDetail(objStatic);
        }

        #endregion

        #region Manage Game Play

        /// <summary>
        /// Created By: Amit Kumar
        /// created On: 29 April 2015
        /// Purpose: Save new game play round
        /// </summary>
        /// <param name="objWallet">List of Paramters sent from iphone/android  </param>
        /// <returns>Result class containing three parameters Id of user,Status= 0 or 1,Error in case of fail  </returns>
        [HttpPost]
        public Result SaveGamePlayRound([FromBody]GF_GamePlayRound gamePlayRound)
        {
            if (gamePlayRound.CourseID <= 0 ||
                gamePlayRound.GolferID <= 0 ||
                string.IsNullOrEmpty(gamePlayRound.RoundStartFrom))
            {
                return new Result { Id = 0, Status = 0, record = null, Error = "One of the required parameter is missing." };
            }

            GamePlay obj = new GamePlay();
            return obj.SaveGamePlayRound(gamePlayRound);
        }

        /// <summary>
        /// Created By: Amit Kumar
        /// created On: 29 April 2015
        /// Purpose: Get game play round list
        /// </summary>
        /// <param name="objWallet">List of Paramters sent from iphone/android  </param>
        /// <returns>Result class containing three parameters Id of user,Status= 0 or 1,Error in case of fail  </returns>
        [HttpPost]
        public Result GetGamePlayRoundList([FromBody]GF_GamePlayRound gamePlayRound)
        {
            if (gamePlayRound.CourseID <= 0 ||
                gamePlayRound.GolferID <= 0)
            {
                return new Result { Id = 0, Status = 0, record = null, Error = "One of the required parameter is missing." };
            }

            GamePlay obj = new GamePlay();
            return obj.GetGamePlayRoundList(gamePlayRound);
        }

        ///// <summary>
        ///// Created By: Amit Kumar
        ///// created On: 29 April 2015
        ///// Purpose: Quit game play round
        ///// </summary>
        ///// <param name="objWallet">List of Paramters sent from iphone/android  </param>
        ///// <returns>Result class containing three parameters Id of user,Status= 0 or 1,Error in case of fail  </returns>
        //[HttpPost]
        //public Result QuitGamePlayRound([FromBody]GF_GamePlayRound gamePlayRound)
        //{
        //    if (gamePlayRound.CourseID <= 0 ||
        //        gamePlayRound.GolferID <= 0)
        //    {
        //        return new Result { Id = 0, Status = 0, record = null, Error = "One of the required parameter is missing." };
        //    }

        //    GamePlay obj = new GamePlay();
        //    return obj.QuitGamePlayRound(gamePlayRound);
        //}

        /// <summary>
        /// Created By: Amit Kumar
        /// created On: 29 April 2015
        /// Purpose: Save game play player information
        /// </summary>
        /// <param name="objWallet">List of Paramters sent from iphone/android  </param>
        /// <returns>Result class containing three parameters Id of user,Status= 0 or 1,Error in case of fail  </returns>
        [HttpPost]
        public Result SaveGamePlayerInfo([FromBody]GF_GamePlayPlayerInfo playerInfo)
        {
            if (playerInfo.RoundID <= 0 ||
                playerInfo.PlayerNo <= 0 ||
                string.IsNullOrEmpty(playerInfo.PlayerName))
            {
                return new Result { Id = 0, Status = 0, record = null, Error = "One of the required parameter is missing." };
            }

            GamePlay obj = new GamePlay();
            return obj.SaveGamePlayPlayer(playerInfo);
        }

        /// <summary>
        /// Created By: Amit Kumar
        /// created On: 30 April 2015
        /// Purpose: Save game play score card information
        /// </summary>
        /// <param name="objWallet">List of Paramters sent from iphone/android  </param>
        /// <returns>Result class containing three parameters Id of user,Status= 0 or 1,Error in case of fail  </returns>
        [HttpPost]
        public Result SaveGamePlayScoreCardInfo([FromBody]GF_GamePlayRound playRound)
        {
            if (playRound.RoundID <= 0 ||
                playRound.GamePlayScoreCard == null)
            {
                return new Result { Id = 0, Status = 0, record = null, Error = "One of the required parameter is missing." };
            }

            GamePlay obj = new GamePlay();
            return obj.SaveGamePlayScoreInfo(playRound);
        }

        /// <summary>
        /// Created By: Amit Kumar
        /// created On: 30 April 2015
        /// Purpose: Get the score information
        /// </summary>
        /// <param name="objWallet">List of Paramters sent from iphone/android  </param>
        /// <returns>Result class containing three parameters Id of user,Status= 0 or 1,Error in case of fail  </returns>
        [HttpPost]
        public Result GetGamePlayScoreCardInfo([FromBody]GF_GamePlayRound playRound)
        {
            if (playRound.RoundID <= 0 ||
                playRound.CourseID <= 0)
            {
                return new Result { Id = 0, Status = 0, record = null, Error = "One of the required parameter is missing." };
            }

            GamePlay obj = new GamePlay();
            return obj.GetGamePlayScoreInfo(playRound);
        }

        /// <summary>
        /// Created By: Amit Kumar
        /// created On: 30 July 2015
        /// Purpose: Delete round
        /// </summary>
        /// <param name="objWallet">List of Paramters sent from iphone/android  </param>
        /// <returns>Result class containing three parameters Id of user,Status= 0 or 1,Error in case of fail  </returns>
        [HttpPost]
        public Result DeleteScoreCard([FromBody]GF_GamePlayRound playRound)
        {
            if (playRound.RoundID <= 0)
            {
                return new Result { Id = 0, Status = 0, record = null, Error = "One of the required parameter is missing." };
            }

            GamePlay obj = new GamePlay();
            return obj.DeleteGamePlayRound(playRound);
        }

        #endregion

        #region ResolutionCenter

        public Result ResolutionCenterListing([FromBody]GF_ResolutionCenter obj)
        {
            if (string.IsNullOrEmpty(Convert.ToString(obj.CourseID)) ||
                string.IsNullOrEmpty(Convert.ToString(obj.UserID)) ||
                string.IsNullOrEmpty(obj.Type) ||
                string.IsNullOrEmpty(Convert.ToString(obj.PageNo)) ||
                string.IsNullOrEmpty(Convert.ToString(obj.Row)))
            {
                return new Result { Id = 0, Status = 0, record = null, Error = "One of the required parameter is missing." };
            }

            return obj.GetResolutionMessages(obj.Search, obj.UserID, obj.Type, obj.Status, obj.FromDate, obj.ToDate, obj.UserName, obj.PageNo, obj.Row);


        }

        public Result ResolutionCenterSendReply([FromBody]GF_ResolutionMessageHistory obj)
        {
            if (
                string.IsNullOrEmpty(Convert.ToString(obj.LogUserID)) ||
                string.IsNullOrEmpty(obj.UserType) ||
                string.IsNullOrEmpty(Convert.ToString(obj.MessageID)) ||
                string.IsNullOrEmpty(Convert.ToString(obj.Status)) ||
                 string.IsNullOrEmpty(Convert.ToString(obj.Message))
               )
            {
                return new Result { Id = 0, Status = 0, record = null, Error = "One of the required parameter is missing." };
            }
            GF_ResolutionCenter objReso = new GF_ResolutionCenter();
            return objReso.SendReply(obj);


        }
        [HttpPost]
        public Result GetMessageHistory([FromBody]GF_ResolutionMessageHistory obj)
        {
            if (string.IsNullOrEmpty(Convert.ToString(obj.MessageID)) ||
                string.IsNullOrEmpty(Convert.ToString(obj.PageNo)) ||
                string.IsNullOrEmpty(Convert.ToString(obj.Row)))
            {
                return new Result { Id = 0, Status = 0, record = null, Error = "One of the required parameter is missing." };
            }
            GF_ResolutionCenter objReso = new GF_ResolutionCenter();

            return objReso.GetMessageHistory(obj.MessageID, obj.PageNo, obj.Row);


        }

        #endregion

        #region Golfer Preference Setting

        /// <summary>
        /// Created By: Amit Kumar
        /// Created On: 10 August 2015
        /// Purpose: Save golfer prefrence setting
        /// </summary>
        /// <param name="objStatic">List of Paramters sent from iphone/android  </param>
        /// <returns>Result class containing three parameters Id of user,Status= 0 or 1,Error in case of fail  </returns>
        [HttpPost]
        public Result SaveGolferPreferenceSetting([FromBody]GF_Golfer golfer)
        {
            if (golfer.GF_ID <= 0 ||
                string.IsNullOrEmpty(golfer.measurement) ||
                string.IsNullOrEmpty(golfer.temperature) ||
                string.IsNullOrEmpty(golfer.speed))
            {
                return new Result { Id = 0, Status = 0, record = null, Error = "One of the required parameter is missing." };
            }

            GF_Golfer obj = new GF_Golfer();
            return obj.SaveGolferPreferenceSetting(golfer);
        }

        #endregion
    }
}
