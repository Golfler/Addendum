using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using GolferWebAPI.Models;
namespace GolferWebAPI.Controllers
{
    public class CourseController : ApiController
    {
        GF_CourseMenu objCourse = null;
        public CourseController()
        {
            objCourse = new GF_CourseMenu();
        }

        #region Get menu Items

        /// <summary>
        /// Created By: Arun
        /// created On: 24 March 2015
        /// Purpose: Get Menu Item by Course ID
        /// <param name="log">contains parameters Email and Password for Login</param>
        /// <returns>returns Status 0 or 1 ,Id of user,Error in case of Fail to Login</returns>
        [HttpPost]
        public Result GetMenuItems([FromBody]GF_CourseVisitLog obj)
        {
            if ((obj.CourseID ?? 0) <= 0)
            {
                return new Result { Id = 0, Status = 0, record = null, Error = "One of the required parameter is missing." };
            }

            if ((obj.GolferID ?? 0) <= 0)
            {
                return new Result { Id = 0, Status = 0, record = null, Error = "One of the required parameter is missing." };
            }

            if (string.IsNullOrEmpty(Convert.ToString(obj.strTimeZone)))
            {
                return new Result { Id = 0, Status = 0, record = null, Error = "One of the required parameter is missing." };
            }

            GF_CourseMenu ob = new GF_CourseMenu();

            return ob.GetMenuByCourseNew(obj);
        }

        #endregion

        #region Course Suggest
        /// <summary>
        /// Created By: Veera
        /// created On: 24 March 2015
        /// Purpose: Course Suggest web service
        /// </summary>
        /// <param name="objCourse">List of Paramters sent from iphone/android  </param>
        /// <returns>Result class containing three parameters Id of user,Status= 0 or 1,Error in case of fail  </returns>
        [HttpPost]
        public Result CourseSuggestService([FromBody]GF_CourseSuggest objCourse)
        {
            if (string.IsNullOrEmpty(objCourse.Name)
                || string.IsNullOrEmpty(Convert.ToString(objCourse.UserId))
                || string.IsNullOrEmpty(objCourse.Latitude)
                || string.IsNullOrEmpty(objCourse.Longitude))
            {
                return new Result { Id = 0, Status = 0, record = null, Error = "One of the required parameter is missing." };
            }

            CourseSuggest objCourseSuggest = new CourseSuggest();
            return (objCourseSuggest.AddCourseSuggestInfo(objCourse));
        }
        #endregion

        #region Promo Code Check

        /// <summary>
        /// Created By: Amit Kumar
        /// created On: 11 June 2015
        /// Purpose: Check promo code is valid or not
        /// <param name="log">contains parameters Email and Password for Login</param>
        /// <returns>returns Status 0 or 1 ,Id of user,Error in case of Fail to Login</returns>
        [HttpPost]
        public Result PromoCodeCheck([FromBody]GF_PromoCode obj)
        {
            if ((obj.CourseID ?? 0) <= 0)
            {
                return new Result { Id = 0, Status = 0, record = null, Error = "One of the required parameter is missing." };
            }

            if (string.IsNullOrEmpty(obj.PromoCode))
            {
                return new Result { Id = 0, Status = 0, record = null, Error = "One of the required parameter is missing." };
            }

            GF_CourseMenu ob = new GF_CourseMenu();

            return ob.PromoCodeValid(obj);
        }

        #endregion
    }
}
