using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace GolferWebAPI.Models
{
    public class GamePlay
    {
        private GolflerEntities db = null;

        public GamePlay()
        {
            db = new GolflerEntities();
        }

        /// <summary>
        /// Created By: Amit Kumar
        /// Created Date: 29 April 2015
        /// Purpose: Save new game play round 
        /// </summary>
        /// <param name="gamePlayRound"></param>
        /// <returns></returns>
        public Result SaveGamePlayRound(GF_GamePlayRound gamePlayRound)
        {
            try
            {
                #region Score Card Detail Check

                //This helps to get blank score card detail. When new round is created

                var lstHoleInformation = GetHoleInformatio(gamePlayRound.CourseID ?? 0, gamePlayRound.GolferID ?? 0);

                var lstScoreBoard = lstHoleInformation.Select(x => new
                {
                    x.HoleNo,
                    x.Distance,
                    Par = x.ParDetail,
                    PlayerOneScore = 0,
                    PlayerTwoScore = 0,
                    PlayerThreeScore = 0,
                    PlayerFourScore = 0
                });

                if (lstHoleInformation.Count() <= 0)
                {
                    return new Result
                    {
                        Id = 0,
                        Status = 0,
                        Error = "Round could not be created because course is not setup yet.",
                        record = null
                    };
                }

                #endregion

                GF_GamePlayRound obj = new GF_GamePlayRound();

                #region Quit all game older than 24 hours

                var chkGameRound = db.GF_GamePlayRound.Where(x => x.CourseID == gamePlayRound.CourseID && x.GolferID == gamePlayRound.GolferID && x.IsQuit == false).OrderByDescending(x => x.CreatedDate).ToList();

                foreach (var gr in chkGameRound)
                {
                    if (gr.CreatedDate <= DateTime.UtcNow.AddHours(-24))
                    {
                        gr.IsQuit = true;
                        db.SaveChanges();
                    }
                }

                #endregion

                #region Get New/Current Game

                var GolferZone = Convert.ToString(db.GF_Golfer.Where(x => x.GF_ID == gamePlayRound.GolferID).FirstOrDefault().strTimeZone);

                chkGameRound = db.GF_GamePlayRound.Where(x => x.CourseID == gamePlayRound.CourseID && x.GolferID == gamePlayRound.GolferID &&
                    x.IsQuit == false).OrderByDescending(x => x.CreatedDate).ToList();
                if (chkGameRound.Count <= 0) // means game round not exists -- so create new game round
                {
                    obj = new GF_GamePlayRound();
                    obj.CourseID = gamePlayRound.CourseID;
                    obj.GolferID = gamePlayRound.GolferID;
                    obj.RoundStartFrom = gamePlayRound.RoundStartFrom;
                    obj.CreatedDate = DateTime.UtcNow;
                    obj.IsQuit = false;
                    obj.strTimeZone = Convert.ToString(GolferZone);
                    obj.Status = StatusType.Active;
                    db.GF_GamePlayRound.Add(obj);
                    db.SaveChanges();
                }
                else // means game round exists -- so pick existing game round
                {
                    //existing game round id
                    Int64 existGameRoundId = Convert.ToInt64(chkGameRound.Select(x => x.ID).FirstOrDefault());

                    //delete all other game round except existing
                    foreach (var gameRound in chkGameRound)
                    {
                        if (gameRound.ID != existGameRoundId)
                        {
                            gameRound.IsQuit = true;
                            db.SaveChanges();
                        }
                    }

                    obj = new GF_GamePlayRound();
                    obj = chkGameRound.Where(x => x.ID == existGameRoundId).FirstOrDefault();
                    obj.strTimeZone = Convert.ToString(GolferZone);
                    obj.ModifyDate = DateTime.UtcNow;
                    db.SaveChanges();
                }
                #endregion

                #region Remove data from Other Game rounds

                #region Golfer Changing Location

                //var deleteGolferChangingLocation = 
                db.GF_GolferChangingLocation.Where(y => y.GolferId == obj.GolferID && y.TimeOfChange < obj.CreatedDate).ToList().ForEach(y => db.GF_GolferChangingLocation.Remove(y));
                //foreach (var golferChangingLocation in deleteGolferChangingLocation)
                //{
                //    if (golferChangingLocation.TimeOfChange < obj.CreatedDate)
                //    {
                // db.GF_GolferChangingLocation.Remove(deleteGolferChangingLocation);
                //    }
                //}
                db.SaveChanges();

                #endregion

                #region delete from paceofplay

                var lstpaceofplay = db.GF_GolferPaceofPlay.Where(x => x.GolferId == obj.GolferID && x.GameRoundID != obj.ID && x.Status != StatusType.Delete).ToList();

                foreach (var rel in lstpaceofplay)
                {
                    rel.Status = StatusType.Delete;
                    db.SaveChanges();
                }

                #endregion

                #region delete from paceofplay Temp

                //var lstpaceofplayTemp = db.GF_GolferPaceofPlay_Temp.Where(x => x.GolferId == obj.GolferID && x.GameRoundID != obj.ID).ToList();
                //foreach (var rel in lstpaceofplayTemp)
                //{
                //    db.GF_GolferPaceofPlay_Temp.Remove(rel);
                //}
                //db.SaveChanges();

                #endregion

                #endregion

                #region Weather Thread Information

                weather.callingRoundWeatherApi(obj);

                #endregion

                return new Result
                {
                    Id = gamePlayRound.ID,
                    Status = 1,
                    Error = "New round added successfully.",
                    record = new
                    {
                        RoundID = obj.ID,
                        obj.CourseID,
                        obj.GolferID,
                        obj.CreatedDate,
                        obj.IsQuit,
                        ScoreBoard = lstScoreBoard,

                    }
                };
            }
            catch (Exception ex)
            {
                return new Result
                {
                    Id = 0,
                    Status = 0,
                    Error = ex.Message,
                    record = null
                };
            }
        }

        /// <summary>
        /// Created By: Amit Kumar
        /// Created Date: 29 April 2015
        /// Purpose: Get game play round list
        /// </summary>
        /// <param name="gamePlayRound"></param>
        /// <returns></returns>
        public Result GetGamePlayRoundList(GF_GamePlayRound gamePlayRound)
        {
            try
            {
                var lstResult = db.GF_GamePlayRound.Where(x => x.CourseID == gamePlayRound.CourseID &&
                    x.GolferID == gamePlayRound.GolferID &&
                    (x.Status ?? StatusType.Active) == StatusType.Active).ToList()
                    .Select(x => new
                    {
                        RoundID = x.ID,
                        x.CourseID,
                        CourseName = x.GF_CourseInfo.COURSE_NAME,
                        DateTime = CommonFunctions.TimeZoneDateTimeByCourseID(Convert.ToInt64(gamePlayRound.CourseID), Convert.ToDateTime(x.CreatedDate.Value)).ToString("dd MMMM, yyyy") + " - " + CommonFunctions.TimeZoneDateTimeByCourseID(Convert.ToInt64(gamePlayRound.CourseID), Convert.ToDateTime(x.CreatedDate.Value)).ToShortTimeString(),
                        x.IsQuit
                    })
                    .OrderByDescending(x => x.RoundID);

                if (lstResult.Count() > 0)
                {
                    var lst = lstResult.Skip(((gamePlayRound.pageIndex ?? 1) - 1) * (gamePlayRound.pageSize ?? 10)).Take(gamePlayRound.pageSize ?? 10);

                    if (lst.Count() > 0)
                    {
                        return new Result
                        {
                            Id = gamePlayRound.GolferID ?? 0,
                            Status = 1,
                            Error = "Success",
                            record = lst
                        };
                    }
                    else
                    {
                        return new Result
                        {
                            Id = gamePlayRound.GolferID ?? 0,
                            Status = 1,
                            Error = "No record found.",
                            record = null
                        };
                    }
                }
                else
                {
                    return new Result
                    {
                        Id = gamePlayRound.GolferID ?? 0,
                        Status = 0,
                        Error = "No record found.",
                        record = null
                    };
                }
            }
            catch (Exception ex)
            {
                return new Result
                {
                    Id = 0,
                    Status = 0,
                    Error = ex.Message,
                    record = null
                };
            }
        }

        /// <summary>
        /// Created By: Amit Kumar
        /// Created Date: 29 April 2015
        /// Purpose: Quit game play round 
        /// </summary>
        /// <param name="gamePlayRound"></param>
        /// <returns></returns>
        public Result QuitGamePlayRound(GF_GamePlayRound gamePlayRound)
        {
            try
            {
                var lstGamePlayRound = db.GF_GamePlayRound.FirstOrDefault(x => x.ID == gamePlayRound.RoundID);

                if (lstGamePlayRound == null)
                {
                    return new Result
                    {
                        Id = gamePlayRound.RoundID ?? 0,
                        Status = 0,
                        Error = "No record found.",
                        record = null
                    };
                }
                else
                {
                    if (lstGamePlayRound.IsQuit ?? false)
                    {
                        return new Result
                        {
                            Id = gamePlayRound.RoundID ?? 0,
                            Status = 0,
                            Error = "Game play round already quitted successfully.",
                            record = null
                        };
                    }

                    lstGamePlayRound.ModifyDate = DateTime.Now;
                    lstGamePlayRound.IsQuit = true;
                    db.SaveChanges();
                }

                return new Result
                {
                    Id = gamePlayRound.ID,
                    Status = 1,
                    Error = "Game play round quitted successfully.",
                    record = new
                    {
                        lstGamePlayRound.ID,
                        lstGamePlayRound.CourseID,
                        lstGamePlayRound.GolferID,
                        lstGamePlayRound.CreatedDate,
                        lstGamePlayRound.IsQuit
                    }
                };
            }
            catch (Exception ex)
            {
                return new Result
                {
                    Id = 0,
                    Status = 0,
                    Error = ex.Message,
                    record = null
                };
            }
        }

        /// <summary>
        /// Created By: Amit Kumar
        /// Created Date: 29 April 2015
        /// Purpose: Save game play player information
        /// </summary>
        /// <param name="playerInfo"></param>
        /// <returns></returns>
        public Result SaveGamePlayPlayer(GF_GamePlayPlayerInfo playerInfo)
        {
            try
            {
                #region Check wheather the player number is greater than 4

                ///User can only enters Player number 1,2,3,4 else return error message

                if (playerInfo.PlayerNo > 4)
                {
                    return new Result
                    {
                        Id = 0,
                        Status = 0,
                        Error = "You cannot give player number 5 or greater information.",
                        record = null
                    };
                }

                #endregion

                #region Check wheather RoundID is exist or not

                var lstRoundCheck = db.GF_GamePlayRound.FirstOrDefault(x => x.ID == playerInfo.RoundID);

                if (lstRoundCheck == null)
                {
                    return new Result
                    {
                        Id = 0,
                        Status = 0,
                        Error = "No record found.",
                        record = null
                    };
                }

                #endregion

                #region Check wheather the player no is exist or not corresponding to Round

                var lstPlayer = db.GF_GamePlayPlayerInfo.FirstOrDefault(x => x.PlayerNo == playerInfo.PlayerNo &&
                    x.RoundID == playerInfo.RoundID);

                if (lstPlayer != null)
                {
                    return new Result
                    {
                        Id = lstPlayer.ID,
                        Status = 0,
                        Error = "Player number " + playerInfo.PlayerNo.ToString() + " is already exists with name " + lstPlayer.PlayerName + ".",
                        record = null
                    };
                }

                #endregion

                GF_GamePlayPlayerInfo obj = new GF_GamePlayPlayerInfo();
                obj.RoundID = playerInfo.RoundID;
                obj.PlayerNo = playerInfo.PlayerNo;
                obj.PlayerName = playerInfo.PlayerName;
                obj.CreatedDate = DateTime.Now;
                db.GF_GamePlayPlayerInfo.Add(obj);
                db.SaveChanges();

                return new Result
                {
                    Id = obj.ID,
                    Status = 1,
                    Error = "Player added successfully.",
                    record = new
                    {
                        obj.ID,
                        obj.RoundID,
                        obj.PlayerNo,
                        obj.PlayerName,
                        obj.CreatedDate
                    }
                };
            }
            catch (Exception ex)
            {
                return new Result
                {
                    Id = 0,
                    Status = 0,
                    Error = ex.Message,
                    record = null
                };
            }
        }

        /// <summary>
        /// Created By: Amit Kumar
        /// Created Date: 30 April 2015
        /// Purpose: Save game play score card information
        /// </summary>
        /// <param name="playerInfo"></param>
        /// <returns></returns>
        public Result SaveGamePlayScoreInfo(GF_GamePlayRound playRound)
        {
            try
            {
                #region Check Round

                var lstRoundCheck = db.GF_GamePlayRound.FirstOrDefault(x => x.ID == playRound.RoundID);

                //Check weather the round is exist or not
                if (lstRoundCheck == null)
                {
                    return new Result
                    {
                        Id = 0,
                        Status = 0,
                        Error = "No record found.",
                        record = null
                    };
                }
                else
                {
                    //Check wheather the round is quitted or not
                    if (lstRoundCheck.IsQuit ?? false)
                    {
                        return new Result
                        {
                            Id = 0,
                            Status = 0,
                            Error = "Requested round is quitted. You cannot update any information when round is quitted.",
                            record = null
                        };
                    }
                    else
                    {
                        #region Check wheather the round last update is greater than or equal 45 min or not

                        ///If the user is not updating the score card information for last 45 min (45 min is default admin can change anytime)
                        ///then round should be quitted automatically

                        DateTime LastUpdateTime = lstRoundCheck.ModifyDate == null ? lstRoundCheck.CreatedDate.Value :
                                                                                     lstRoundCheck.ModifyDate.Value;
                        DateTime CurrentTime = DateTime.UtcNow;
                        TimeSpan span = CurrentTime.Subtract(LastUpdateTime);

                        //"Time Difference (seconds): " + span.Seconds
                        //"Time Difference (minutes): " + span.Minutes
                        //"Time Difference (hours): " + span.Hours
                        //"Time Difference (days): " + span.Days

                        if (span.Minutes >= 45 || span.Hours > 0)
                        {
                            lstRoundCheck.ModifyDate = DateTime.UtcNow;
                            lstRoundCheck.IsQuit = true;
                            db.SaveChanges();

                            #region Delete from changing location

                            //Clear GF_GolferChangingLocation table for golfer
                            var deleteGolferChangingLocation = db.GF_GolferChangingLocation.Where(y => y.GolferId == lstRoundCheck.GolferID);

                            foreach (var golferChangingLocation in deleteGolferChangingLocation)
                            {
                                db.GF_GolferChangingLocation.Remove(golferChangingLocation);
                            }
                            db.SaveChanges();

                            #endregion

                            #region Delete from paceofplay

                            var lstpaceofplay = db.GF_GolferPaceofPlay.Where(x => x.GolferId == lstRoundCheck.GolferID &&
                                    x.Status != StatusType.Delete).ToList();
                            foreach (var rel in lstpaceofplay)
                            {
                                rel.Status = StatusType.Delete;
                            }
                            db.SaveChanges();

                            #endregion

                            return new Result
                            {
                                Id = playRound.RoundID ?? 0,
                                Status = 0,
                                Error = "You are no longer active on this round for last 45 minutes. So this round is automatically quitted.",
                                record = null
                            };
                        }

                        #endregion
                    }
                }

                #endregion

                var lstGamePlayRound = db.GF_GamePlayRound.FirstOrDefault(x => x.ID == playRound.RoundID);

                lstGamePlayRound.ModifyDate = DateTime.UtcNow;
                lstGamePlayRound.IsQuit = playRound.IsQuit;
                db.SaveChanges();

                #region Delete from paceofplay

                if (playRound.IsQuit ?? false)
                {
                    #region Delete from changing location

                    //Clear GF_GolferChangingLocation table for golfer
                    var deleteGolferChangingLocation = db.GF_GolferChangingLocation.Where(y => y.GolferId == lstGamePlayRound.GolferID);

                    foreach (var golferChangingLocation in deleteGolferChangingLocation)
                    {
                        db.GF_GolferChangingLocation.Remove(golferChangingLocation);
                    }
                    db.SaveChanges();

                    #endregion

                    var lstpaceofplay = db.GF_GolferPaceofPlay.Where(x => x.GolferId == lstGamePlayRound.GolferID &&
                        x.Status != StatusType.Delete).ToList();
                    foreach (var rel in lstpaceofplay)
                    {
                        rel.Status = StatusType.Delete;
                    }
                    db.SaveChanges();
                }

                #endregion

                #region Add/Update Score Card Detail

                foreach (var item in playRound.GamePlayScoreCard)
                {
                    var scoreCard = db.GF_GamePlayScoreCard.FirstOrDefault(x => x.HoleNo == item.HoleNo &&
                        x.RoundID == playRound.RoundID);

                    if (scoreCard != null) //Update
                    {
                        if (scoreCard.PlayerOneScore != item.PlayerOneScore ||
                            scoreCard.PlayerTwoScore != item.PlayerTwoScore ||
                            scoreCard.PlayerThreeScore != item.PlayerThreeScore ||
                            scoreCard.PlayerFourScore != item.PlayerFourScore)
                        {
                            scoreCard.Distance = item.Distance;
                            scoreCard.Par = item.Par;
                            scoreCard.PlayerOneScore = item.PlayerOneScore;
                            scoreCard.PlayerTwoScore = item.PlayerTwoScore;
                            scoreCard.PlayerThreeScore = item.PlayerThreeScore;
                            scoreCard.PlayerFourScore = item.PlayerFourScore;
                            scoreCard.ModifyDate = DateTime.UtcNow;
                        }
                    }
                    else //Add
                    {
                        if ((item.PlayerOneScore ?? 0) > 0 ||
                            (item.PlayerTwoScore ?? 0) > 0 ||
                            (item.PlayerThreeScore ?? 0) > 0 ||
                            (item.PlayerFourScore ?? 0) > 0)
                        {
                            scoreCard = new GF_GamePlayScoreCard();
                            scoreCard.RoundID = playRound.RoundID;
                            scoreCard.HoleNo = item.HoleNo;
                            scoreCard.Distance = item.Distance;
                            scoreCard.Par = item.Par;
                            scoreCard.PlayerOneScore = item.PlayerOneScore;
                            scoreCard.PlayerTwoScore = item.PlayerTwoScore;
                            scoreCard.PlayerThreeScore = item.PlayerThreeScore;
                            scoreCard.PlayerFourScore = item.PlayerFourScore;
                            scoreCard.CreatedDate = DateTime.UtcNow;
                            db.GF_GamePlayScoreCard.Add(scoreCard);
                        }
                    }
                }

                db.SaveChanges();

                #endregion

                return new Result
                {
                    Id = playRound.RoundID ?? 0,
                    Status = 1,
                    //Error = "Score Card information saved successfully" + ((playRound.IsQuit ?? false) ? " and round quitted successfully." : "."),
                    Error = "Round saved. Returning you to Scorecard.",
                    record = null
                };
            }
            catch (Exception ex)
            {
                return new Result
                {
                    Id = 0,
                    Status = 0,
                    Error = ex.Message,
                    record = null
                };
            }
        }

        /// <summary>
        /// Created By: Amit Kumar
        /// Created Date: 30 April 2015
        /// Purpose: Get the score information
        /// </summary>
        /// <param name="playerInfo"></param>
        /// <returns></returns>
        public Result GetGamePlayScoreInfo(GF_GamePlayRound playRound)
        {
            try
            {
                string message = "";

                #region Check Round

                var lstRoundCheck = db.GF_GamePlayRound.FirstOrDefault(x => x.ID == playRound.RoundID);

                //Check weather the round is exist or not
                if (lstRoundCheck == null)
                {
                    return new Result
                    {
                        Id = 0,
                        Status = 0,
                        Error = "No record found.",
                        record = null
                    };
                }
                else
                {
                    //Check wheather the round is quitted or not
                    if (lstRoundCheck.IsQuit ?? false)
                    {
                        message = "Requested round is quitted. You cannot update any information when round is quitted.";
                    }
                    else
                    {
                        #region Check wheather the round last update is greater than or equal 45 min or not

                        ///If the user is not updating the score card information for last 45 min (45 min is default admin can change anytime)
                        ///then round should be quitted automatically

                        DateTime LastUpdateTime = lstRoundCheck.ModifyDate == null ? lstRoundCheck.CreatedDate.Value :
                                                                                     lstRoundCheck.ModifyDate.Value;
                        DateTime CurrentTime = DateTime.Now;
                        TimeSpan span = CurrentTime.Subtract(LastUpdateTime);

                        if (span.Minutes >= 45 || span.Hours > 0)
                        {
                            lstRoundCheck.ModifyDate = DateTime.Now;
                            lstRoundCheck.IsQuit = true;
                            db.SaveChanges();

                            #region Delete from changing location

                            //Clear GF_GolferChangingLocation table for golfer
                            var deleteGolferChangingLocation = db.GF_GolferChangingLocation.Where(y => y.GolferId == lstRoundCheck.GolferID);

                            foreach (var golferChangingLocation in deleteGolferChangingLocation)
                            {
                                db.GF_GolferChangingLocation.Remove(golferChangingLocation);
                            }
                            db.SaveChanges();

                            #endregion

                            #region Delete from paceofplay

                            var lstpaceofplay = db.GF_GolferPaceofPlay.Where(x => x.GolferId == lstRoundCheck.GolferID &&
                                    x.Status != StatusType.Delete).ToList();
                            foreach (var rel in lstpaceofplay)
                            {
                                rel.Status = StatusType.Delete;
                            }

                            db.SaveChanges();

                            #endregion

                            message = "You are no longer active on this round for last 45 minutes. So this round is automatically quitted.";
                        }
                        else
                        {
                            message = "Success.";
                        }

                        #endregion
                    }
                }

                #endregion

                var lstHoleInformation = GetHoleInformatio(playRound.CourseID ?? 0, playRound.GolferID ?? 0);

                var lstGamePlayScoreCard = db.GF_GamePlayScoreCard.Where(x => x.RoundID == playRound.RoundID).ToList();

                if (lstGamePlayScoreCard.Count() > 0)
                {
                    var lstScoreBoard = (from x in lstHoleInformation
                                         join y in lstGamePlayScoreCard on x.HoleNo equals y.HoleNo into z
                                         from p in z.DefaultIfEmpty()
                                         select new
                                         {
                                             x.HoleNo,
                                             x.Distance,
                                             Par = x.ParDetail,
                                             PlayerOneScore = (p == null ? 0 : p.PlayerOneScore),
                                             PlayerTwoScore = (p == null ? 0 : p.PlayerTwoScore),
                                             PlayerThreeScore = (p == null ? 0 : p.PlayerThreeScore),
                                             PlayerFourScore = (p == null ? 0 : p.PlayerFourScore)
                                         }).ToList();

                    var lstPlayerInfo = db.GF_GamePlayPlayerInfo.Where(x => x.RoundID == playRound.RoundID).Select(x => new
                    {
                        x.RoundID,
                        PlayerID = x.ID,
                        x.PlayerNo,
                        x.PlayerName
                    }).ToList();

                    return new Result
                    {
                        Id = playRound.RoundID ?? 0,
                        Status = 1,
                        Error = message,
                        record = new
                        {
                            RoundID = playRound.RoundID ?? 0,
                            IsQuit = lstRoundCheck.IsQuit,
                            ScoreBoard = lstScoreBoard,
                            PlayerInfo = lstPlayerInfo
                        }
                    };
                }
                else
                {
                    var lstScoreBoard = lstHoleInformation.OrderBy(x => x.HoleNo).Select(x => new
                    {
                        x.HoleNo,
                        x.Distance,
                        Par = x.ParDetail,
                        PlayerOneScore = 0,
                        PlayerTwoScore = 0,
                        PlayerThreeScore = 0,
                        PlayerFourScore = 0
                    }).ToList();

                    var lstPlayerInfo = db.GF_GamePlayPlayerInfo.Where(x => x.RoundID == playRound.RoundID).Select(x => new
                        {
                            x.RoundID,
                            PlayerID = x.ID,
                            x.PlayerNo,
                            x.PlayerName
                        }).ToList();

                    return new Result
                    {
                        Id = playRound.RoundID ?? 0,
                        Status = 1,
                        Error = message,
                        record = new
                        {
                            RoundID = playRound.RoundID ?? 0,
                            IsQuit = lstRoundCheck.IsQuit,
                            ScoreBoard = lstScoreBoard,
                            PlayerInfo = lstPlayerInfo
                        }
                    };
                }

            }
            catch (Exception ex)
            {
                return new Result
                {
                    Id = 0,
                    Status = 0,
                    Error = ex.Message,
                    record = null
                };
            }
        }

        /// <summary>
        /// Created By: Amit Kumar
        /// Created Date: 30 July 2015
        /// Purpose: Delete game play round 
        /// </summary>
        /// <param name="gamePlayRound"></param>
        /// <returns></returns>
        public Result DeleteGamePlayRound(GF_GamePlayRound gamePlayRound)
        {
            try
            {
                var lstGamePlayRound = db.GF_GamePlayRound.FirstOrDefault(x => x.ID == gamePlayRound.RoundID);

                if (lstGamePlayRound != null)
                {
                    lstGamePlayRound.Status = StatusType.Delete;
                    lstGamePlayRound.ModifyDate = DateTime.UtcNow;
                    lstGamePlayRound.IsQuit = true;
                    db.SaveChanges();

                    var lstResult = db.GF_GamePlayRound.Where(x => x.CourseID == lstGamePlayRound.CourseID &&
                    x.GolferID == lstGamePlayRound.GolferID &&
                    (x.Status ?? StatusType.Active) == StatusType.Active).ToList()
                    .Select(x => new
                    {
                        RoundID = x.ID,
                        x.CourseID,
                        CourseName = x.GF_CourseInfo.COURSE_NAME,
                        DateTime = CommonFunctions.TimeZoneDateTimeByCourseID(Convert.ToInt64(lstGamePlayRound.CourseID), Convert.ToDateTime(x.CreatedDate.Value)).ToString("dd MMMM, yyyy") + " - " + CommonFunctions.TimeZoneDateTimeByCourseID(Convert.ToInt64(lstGamePlayRound.CourseID), Convert.ToDateTime(x.CreatedDate.Value)).ToShortTimeString(),
                        x.IsQuit
                    })
                    .OrderByDescending(x => x.RoundID)
                    .Skip(((gamePlayRound.pageIndex ?? 1) - 1) * (gamePlayRound.pageSize ?? 10)).Take(gamePlayRound.pageSize ?? 10);

                    return new Result
                    {
                        Id = gamePlayRound.ID,
                        Status = 1,
                        Error = "Score card deleted successfully.",
                        record = lstResult
                    };
                }
                else
                {
                    return new Result
                    {
                        Id = gamePlayRound.ID,
                        Status = 1,
                        Error = "Invalid score card.",
                        record = null
                    };
                }
            }
            catch (Exception ex)
            {
                return new Result
                {
                    Id = 0,
                    Status = 0,
                    Error = ex.Message,
                    record = null
                };
            }
        }

        public List<HoleDetail> GetHoleInformatio(long CourseID, long golflerID)
        {
            try
            {
                var lstGolfer = db.GF_Golfer.FirstOrDefault(x => x.GF_ID == golflerID);

                var idParam = new SqlParameter
                {
                    ParameterName = "CourseID",
                    Value = CourseID
                };

                var lstHoleDetail = db.Database.SqlQuery<HoleInformation>("exec GF_SP_GetCourseHoleDetail @CourseID", idParam).ToList<HoleInformation>();

                if (lstGolfer != null)
                {
                    if (lstGolfer.Tee == 1)//Professional
                    {
                        return lstHoleDetail.Select(x => new HoleDetail
                        {
                            HoleNo = x.HoleNo,
                            ParDetail = x.ParDetail,
                            Distance = x.BlueDistance
                        }).OrderBy(x => x.HoleNo).ToList();
                    }
                    else if (lstGolfer.Tee == 3)//Lady
                    {
                        return lstHoleDetail.Select(x => new HoleDetail
                        {
                            HoleNo = x.HoleNo,
                            ParDetail = x.ParDetail,
                            Distance = x.RedDistance
                        }).OrderBy(x => x.HoleNo).ToList();
                    }
                    else//Gentleman
                    {
                        return lstHoleDetail.Select(x => new HoleDetail
                        {
                            HoleNo = x.HoleNo,
                            ParDetail = x.ParDetail,
                            Distance = x.Distance
                        }).OrderBy(x => x.HoleNo).ToList();
                    }
                }
                else
                {
                    return lstHoleDetail.Select(x => new HoleDetail
                    {
                        HoleNo = x.HoleNo,
                        ParDetail = x.ParDetail,
                        Distance = x.Distance
                    }).OrderBy(x => x.HoleNo).ToList();
                }
            }
            catch
            {
                return new List<HoleDetail>();
            }
        }
    }

    public partial class GF_GamePlayRound
    {
        public long? RoundID { get; set; }
        public int? pageIndex { get; set; }
        public int? pageSize { get; set; }
        public List<GF_GamePlayScoreCard> GamePlayScoreCard { get; set; }
    }

    public class HoleInformation
    {
        public Int32 HoleNo { get; set; }
        public Int32 Distance { get; set; }
        public Int32 ParDetail { get; set; }
        public Int32 RedDistance { get; set; }
        public Int32 BlueDistance { get; set; }
    }

    public class HoleDetail
    {
        public Int32 HoleNo { get; set; }
        public Int32 Distance { get; set; }
        public Int32 ParDetail { get; set; }
    }
}