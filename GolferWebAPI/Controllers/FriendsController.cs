using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using GolferWebAPI.Models;
namespace GolferWebAPI.Controllers
{
    public class FriendsController : ApiController
    {
        #region Friends
        /// <summary>
        /// Created By: Veera
        /// created On: 27 March 2015
        /// Purpose: Search Golfers web service
        /// </summary>
        /// <param name="objCourse">List of Paramters sent from iphone/android  </param>
        /// <returns>Result class containing three parameters Id of user,Status= 0 or 1,Error in case of fail  </returns>
        [HttpPost]
        public Result SearchGolferService([FromBody]Friends  objsearch)
        {
            if ( string.IsNullOrEmpty(objsearch.SearchCourseId) || string.IsNullOrEmpty(objsearch.SearchType))
            {
                return new Result { Id = 0, Status = 0,record = null, Error = "One of the required parameter is missing." };
            }

            Friends objFriend = new Friends();
            return (objFriend.searchGolferNAdminUsers(objsearch.SearchStr, Convert.ToInt64(objsearch.SearchCourseId), objsearch.SearchType,Convert.ToInt64(objsearch.UserId),objsearch.UserType));
        }
        /// <summary>
        /// Created By: Veera
        /// created On: 27 March 2015
        /// Purpose: Add Friends web service
        /// </summary>
        /// <param name="objCourse">List of Paramters sent from iphone/android  </param>
        /// <returns>Result class containing three parameters Id of user,Status= 0 or 1,Error in case of fail  </returns>
        [HttpPost]
        public Result AddFriendsService([FromBody]GF_ContactList friend)
        {
            if (string.IsNullOrEmpty(Convert.ToString(friend.UserId))
                || string.IsNullOrEmpty(Convert.ToString(friend.FriendId))
                || string.IsNullOrEmpty(friend.UserType)
                || string.IsNullOrEmpty(friend.FriendType))
            {
                return new Result { Id = 0, Status = 0, record = null, Error = "One of the required parameter is missing." };
            }

            Friends objFriend = new Friends();
            return (objFriend.AddFriends(friend));
        }
        /// <summary>
        /// Created By: Veera
        /// created On: 27 March 2015
        /// Purpose: Get Friends web service
        /// </summary>
        /// <param name="objCourse">List of Paramters sent from iphone/android  </param>
        /// <returns>Result class containing three parameters Id of user,Status= 0 or 1,Error in case of fail  </returns>
        [HttpPost]
        public FriendsResult GetFriendsService([FromBody]GF_ContactList friend)
        {
            if (string.IsNullOrEmpty(Convert.ToString(friend.UserId))  || string.IsNullOrEmpty(Convert.ToString(friend.FriendType)))
            {
                return new FriendsResult { Id = 0, Status = 0, Friends = null, Error = "One of the required parameter is missing." };
            }

            Friends objFriend = new Friends();
            return (objFriend.GetFriendList(1,friend));
        }
        #endregion
    }
}
