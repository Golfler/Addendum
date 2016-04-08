using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CourseWebApi.Models;
using Golfler;

namespace CourseWebApi.Models
{
    public partial class CourseSuggest
    {
        GolflerEntities _db = null;
        public CourseSuggest()
        {
            _db = new GolflerEntities();
        }
        /// <summary>
        /// Created By:Veera
        /// Created Date: 24 March 2015
        /// Purpose: Add CourseSuggest Info.
        /// </summary>
        /// <param name="objCourse"></param>
        /// <returns></returns>
        //
        public Result AddCourseSuggestInfo(GF_CourseSuggest objCourse)
        {
            try
            {

                var user = _db.GF_CourseSuggest.FirstOrDefault(p => p.Name.ToLower() == objCourse.Name.ToLower());
                if (user != null)
                {
                    return new Result { Id = 0, Status = 0, Error = "Course already exists." };
                }
                objCourse.CreatedOn = DateTime.Now;
                objCourse.Date = DateTime.Now;
                objCourse.Status = StatusType.InActive;
                _db.GF_CourseSuggest.Add(objCourse);
                _db.SaveChanges();
                return new Result
                {
                    Id = objCourse.ID,
                    Status = 1,
                    Error = "Course has been saved successfully.",
                    record = new
                    {
                        objCourse.ID,
                        objCourse.Name,
                        objCourse.Latitude,
                        objCourse.Longitude,
                        objCourse.UserId,
                        objCourse.Date,
                        objCourse.CreatedOn

                    }
                };



            }
            catch (Exception ex)
            {
                return new Result { Id = 0, Status = 0, Error = ex.Message };
            }

        }

    }
}