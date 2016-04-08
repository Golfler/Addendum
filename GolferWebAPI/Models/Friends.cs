using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using ErrorLibrary;
using GolferWebAPI.Models;
using System.Data.Objects;
namespace GolferWebAPI.Models
{
    public class Friends
    {
        GolflerEntities _db = null;

        public DateTime TodaysDate = Convert.ToDateTime(DateTime.UtcNow).Date;

        public Friends()
        {
            _db = new GolflerEntities();
        }
        public string SearchStr { get; set; }
        public string SearchCourseId { get; set; }
        public string SearchType { get; set; }
        public string UserId { get; set; }
        public string UserType { get; set; }

        /// <summary>
        /// Created By:Veera
        /// Created Date: 24 March 2015
        /// Purpose: Add Friend Info.
        /// </summary>
        /// <param name="objFriend"></param>
        /// <returns></returns>
        /// 
        public Result AddFriends(GF_ContactList objFriend)
        {
            try
            {

                var friend = _db.GF_ContactList.FirstOrDefault(p => ((p.FriendId == objFriend.FriendId && p.UserId == objFriend.UserId) || (p.UserId == objFriend.FriendId && p.FriendId == objFriend.UserId)) && p.FriendType == p.FriendType && p.IsConnected == true);
                if (friend != null)
                {
                    return new Result { Id = 0, Status = 1, Error = "Friend already exists." };
                }
                objFriend.CreatedDate = DateTime.Now;
                objFriend.IsConnected = true;
                _db.GF_ContactList.Add(objFriend);
                _db.SaveChanges();
                return new Result
                {
                    Id = objFriend.ID,
                    Status = 1,
                    Error = "Friend has been saved successfully.",
                    record = null
                };



            }
            catch (Exception ex)
            {
                ErrorClass.WriteLog(ex, HttpContext.Current.Request);
                return new Result { Id = 0, Status = 0, Error = ex.Message };
            }

        }

        /// <summary>
        /// Created By:Veera
        /// Created Date: 24 March 2015
        /// Purpose: Get Friend Info.
        /// </summary>
        /// <param name="objFriend"></param>
        /// <returns></returns>
        public FriendsResult GetFriendList(long? PgNo, GF_ContactList objFriend)
        {
            try
            {
                _db = new GolflerEntities();
                decimal PgSize = ConfigClass.MessageListingPageSize;
                string DefaultImagePath = ConfigurationManager.AppSettings["DefaultImagePath"];
                string GolferImagePath = ConfigurationManager.AppSettings["GolferImagePath"];
                string CourseImagePath = ConfigurationManager.AppSettings["CourseImagePath"];

                #region

                //Messaging: Gophie should not be able to message a Golfer that does not have an active order placed.

                List<Int64> refIDs = new List<Int64>();

                if (objFriend.UserType == GolferWebAPI.Models.UserType.Cartie || objFriend.UserType == "NU")
                {
                    //If I'm Cartie/Gophie then show me only those golfer user's which i have to deliver the order
                    if (objFriend.UserType == GolferWebAPI.Models.UserType.Cartie && objFriend.FriendType == "NU")
                    {
                        var order = _db.GF_Order.Where(x => x.OrderType == OrderType.CartOrder &&
                            (x.CartieId ?? 0) == objFriend.UserId && EntityFunctions.TruncateTime(x.OrderDate) == TodaysDate &&
                            !(x.IsDelivered ?? false) && !(x.IsPickup ?? false) && !(x.IsRejected ?? false));

                        if (order.Count() > 0)
                        {
                            //string IDs = string.Join(",", order.Select(x => (objFriend.UserType == "NU" ? x.CartieId : x.GolferID)));
                            refIDs = order.Select(x => (x.GolferID ?? 0)).ToList();
                        }
                    }
                    else
                    {
                        //If I'm golfer user then show me only those Cartie/Gophie user's who has accepted my order
                        if (objFriend.UserType == "NU" && objFriend.FriendType == GolferWebAPI.Models.UserType.Cartie)
                        {
                            var order = _db.GF_Order.Where(x => x.OrderType == OrderType.CartOrder &&
                                (x.GolferID ?? 0) == objFriend.UserId && EntityFunctions.TruncateTime(x.OrderDate) == TodaysDate &&
                                !(x.IsDelivered ?? false) && !(x.IsPickup ?? false) && !(x.IsRejected ?? false));

                            if (order.Count() > 0)
                            {
                                //string IDs = string.Join(",", order.Select(x => (objFriend.UserType == "NU" ? x.CartieId : x.GolferID)));
                                refIDs = order.Select(x => (x.CartieId ?? 0)).ToList();
                            }
                        }
                    }
                }

                #endregion

                string msg = "No record found.";
                var result = _db.GF_SP_GetFriendListing(PgNo, PgSize, objFriend.UserId, objFriend.FriendType, true, objFriend.CourseId, objFriend.UserType, DefaultImagePath, GolferImagePath, CourseImagePath).ToList();
                if (result.Count > 0)
                {
                    IEnumerable<object> frnds = result.Where(x => (x.Status ?? StatusType.Delete).ToString().Trim() == StatusType.Active).Select(x => new
                    {
                        x.ID,
                        x.FriendName,
                        CreatedDate = x.CreatedDate.HasValue ? x.CreatedDate.Value.ToString("dd/MM/yyyy") : "",
                        x.PageNo,
                        x.TotalPages,
                        FriendType = x.FriendType,
                        UserType = objFriend.UserType.ToUpper(),
                        x.IsOnline,
                        x.FriendImage,
                        LastMessageSent = x.LastMessageSent.HasValue ? x.LastMessageSent.Value.ToString("dd/MM/yyyy") : "",
                        TimeElapsed = ((long)DateTime.UtcNow.Subtract(Convert.ToDateTime(x.LastMessageSent ?? DateTime.UtcNow)).TotalMinutes).ToString()
                    }).ToList()
                    .Where(x => (((objFriend.UserType == "NU" && objFriend.FriendType == GolferWebAPI.Models.UserType.Cartie) ||
                                  (objFriend.UserType == GolferWebAPI.Models.UserType.Cartie && objFriend.FriendType == "NU")) ? refIDs.Contains(x.ID ?? 0) : x.ID > 0));

                    if (frnds.Count() <= 0)
                    {
                        if (objFriend.FriendType == GolferWebAPI.Models.UserType.CourseAdmin || objFriend.FriendType == GolferWebAPI.Models.UserType.Proshop)
                        {
                            msg = "Sorry, neither a pro-shop nor course admin is currently not available.";
                        }
                        else if (objFriend.FriendType == GolferWebAPI.Models.UserType.Cartie)
                        {
                            msg = "Sorry, gophie is currently not available.";
                        }
                        else if (objFriend.FriendType == GolferWebAPI.Models.UserType.Kitchen)
                        {
                            msg = "Sorry, kitchen is currently not available.";
                        }
                        else if (objFriend.FriendType == GolferWebAPI.Models.UserType.Ranger)
                        {
                            msg = "Sorry, ranger is currently not available.";
                        }
                        else
                        {
                            //msg = "Sorry, golfer is currently not available.";
                            msg = "There are currently no players signed into chat at this course.";
                        }
                    }
                    else
                    {
                        msg = "Users retrieved successfully.";
                    }

                    return new FriendsResult { Error =msg , Status = 1, Friends = frnds, FriendType = objFriend.FriendType, UserType = objFriend.UserType }; // "FriendList retrieved successfully."
                }

                if (objFriend.FriendType == GolferWebAPI.Models.UserType.CourseAdmin || objFriend.FriendType == GolferWebAPI.Models.UserType.Proshop)
                {
                    msg = "Sorry, neither a pro-shop nor course admin is currently signed in.";
                }
                else if (objFriend.FriendType == GolferWebAPI.Models.UserType.Cartie)
                {
                    msg = "Sorry, gophie is not currently signed in.";
                }
                else if (objFriend.FriendType == GolferWebAPI.Models.UserType.Kitchen)
                {
                    msg = "Sorry, kitchen is not currently signed in.";
                }
                else if (objFriend.FriendType == GolferWebAPI.Models.UserType.Ranger)
                {
                    msg = "Sorry, ranger is not currently signed in.";
                }
                else
                {
                    //msg = "Sorry, golfer is not currently signed in.";
                    msg = "There are currently no players signed into chat at this course.";
                }

                return new FriendsResult { Error = msg, Status = 0 };
            }

            catch (Exception ex)
            {

                ErrorClass.WriteLog(ex, HttpContext.Current.Request);
                return new FriendsResult { Error = ex.Message, Status = 0 };
            }


        }

        /// <summary>
        /// Created By:Veera
        /// Created Date: 24 March 2015
        /// Purpose: search friends.
        /// </summary>
        /// <param name="searchstr"></param>
        /// <returns></returns>
        public Result searchGolferNAdminUsers(string searchstr, long courseId, string searchType, long userId, string userType)
        {
            try
            {
                _db = new GolflerEntities();

                string DefaultImagePath = ConfigurationManager.AppSettings["DefaultImagePath"];
                string GolferImagePath = ConfigurationManager.AppSettings["GolferImagePath"];
                string CourseImagePath = ConfigurationManager.AppSettings["CourseImagePath"];

                #region

                //Messaging: Gophie should not be able to message a Golfer that does not have an active order placed.

                List<Int64> refIDs = new List<Int64>();

                if (userType == GolferWebAPI.Models.UserType.Cartie || userType == "NU")
                {
                    //If I'm Cartie/Gophie then show me only those golfer user's which i have to deliver the order
                    if (userType == GolferWebAPI.Models.UserType.Cartie && searchType == "NU")
                    {
                        var order = _db.GF_Order.Where(x => x.OrderType == OrderType.CartOrder &&
                            (x.CartieId ?? 0) == userId && EntityFunctions.TruncateTime(x.OrderDate) == TodaysDate &&
                            !(x.IsDelivered ?? false) && !(x.IsPickup ?? false) && !(x.IsRejected ?? false));

                        if (order.Count() > 0)
                        {
                            //string IDs = string.Join(",", order.Select(x => (objFriend.UserType == "NU" ? x.CartieId : x.GolferID)));
                            refIDs = order.Select(x => (x.GolferID ?? 0)).ToList();
                        }
                    }
                    else
                    {
                        //If I'm golfer user then show me only those Cartie/Gophie user's who has accepted my order
                        if (userType == "NU" && searchType == GolferWebAPI.Models.UserType.Cartie)
                        {
                            var order = _db.GF_Order.Where(x => x.OrderType == OrderType.CartOrder &&
                                (x.GolferID ?? 0) == userId && EntityFunctions.TruncateTime(x.OrderDate) == TodaysDate &&
                                !(x.IsDelivered ?? false) && !(x.IsPickup ?? false) && !(x.IsRejected ?? false));

                            if (order.Count() > 0)
                            {
                                //string IDs = string.Join(",", order.Select(x => (objFriend.UserType == "NU" ? x.CartieId : x.GolferID)));
                                refIDs = order.Select(x => (x.CartieId ?? 0)).ToList();
                            }
                        }
                    }
                }

                #endregion

                string msg = "No record(s) found.";
                var result = _db.GF_SP_SearchFriends(searchstr, courseId, searchType, userId, userType, DefaultImagePath, GolferImagePath, CourseImagePath).ToList();
                if (result.Count > 0)
                {
                    IEnumerable<object> frnds = result.Where(x => (userType == "NU" ? x.FriendType != GolferWebAPI.Models.UserType.Cartie : x.id > 0))
                        .Select(x => new
                        {
                            x.id,
                            Name = x.Name,
                            x.image,
                            FriendType = x.FriendType.ToUpper(),
                            x.isOnline,
                            x.LastMessageSent,
                            //  NU means friend is a golfer
                            TimeElapsed = ((long)DateTime.UtcNow.Subtract(Convert.ToDateTime(x.LastMessageSent)).TotalMinutes).ToString()
                        }).ToList()
                        .Where(x => (((userType == GolferWebAPI.Models.UserType.Cartie && searchType == "NU") ||
                                      (userType == "NU" && searchType == GolferWebAPI.Models.UserType.Cartie)) ? refIDs.Contains(x.id) : x.id > 0));


                    if (frnds.Count() <= 0)
                    {
                        if (searchType == GolferWebAPI.Models.UserType.CourseAdmin || searchType == GolferWebAPI.Models.UserType.Proshop)
                        {
                            msg = "Sorry, neither a pro-shop nor course admin is currently not available.";
                        }
                        else if (searchType == GolferWebAPI.Models.UserType.Cartie)
                        {
                            msg = "Sorry, gophie is currently not available.";
                        }
                        else if (searchType == GolferWebAPI.Models.UserType.Kitchen)
                        {
                            msg = "Sorry, kitchen is currently not available.";
                        }
                        else if (searchType == GolferWebAPI.Models.UserType.Ranger)
                        {
                            msg = "Sorry, ranger is currently not available.";
                        }
                        else
                        {
                            //msg = "Sorry, golfer is currently not available.";
                            msg = "There are currently no players signed into chat at this course.";
                        }
                    }
                    else
                    {
                        msg = "Users retrieved successfully.";
                    }

                    return new Result { Error = msg, Status = 1, record = frnds };
                }

                if (searchType == GolferWebAPI.Models.UserType.CourseAdmin || searchType == GolferWebAPI.Models.UserType.Proshop)
                {
                    msg = "Sorry, neither a pro-shop nor course admin is currently not available.";
                }
                else if (searchType == GolferWebAPI.Models.UserType.Cartie)
                {
                    msg = "Sorry, gophie is currently not available.";
                }
                else if (searchType == GolferWebAPI.Models.UserType.Kitchen)
                {
                    msg = "Sorry, kitchen is currently not available.";
                }
                else if (searchType == GolferWebAPI.Models.UserType.Ranger)
                {
                    msg = "Sorry, ranger is currently not available.";
                }
                else
                {
                    //msg = "Sorry, golfer is currently not available.";
                    msg = "There are currently no players signed into chat at this course.";
                }

                return new Result { Error = msg, Status = 0 };
            }

            catch (Exception ex)
            {

                ErrorClass.WriteLog(ex, HttpContext.Current.Request);
                return new Result { Error = ex.Message, Status = 0 };
            }


        }

    }
    public partial class GF_ContactList
    {
        public long CourseId { get; set; }
    }
}