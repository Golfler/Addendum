using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GolferWebAPI.Models
{
    public partial class GF_GolferRating
    {
        GolflerEntities _db = null;

        public GF_GolferRating()
        {
            _db = new GolflerEntities();
        }

        /// <summary>
        /// Created By: Amit Kumar
        /// Created Date: 13 April 2015
        /// Purpose: Save golfer rating given to cartie
        /// </summary>
        /// <param name="rating"></param>
        /// <returns></returns>
        public Result SaveGolferRating(GF_GolferRating rating)
        {
            try
            {
                #region Check wheather the cartie user is exist or not

                var lstCartie = _db.GF_AdminUsers.FirstOrDefault(x => x.ID == rating.ReferenceID &&
                    (x.Type == UserType.Cartie || x.Type == UserType.PowerAdmin));

                if (lstCartie == null)
                {
                    return new Result
                    {
                        Id = rating.GolferID ?? 0,
                        Status = 0,
                        Error = "Invalid Cartie.",
                        record = null
                    };
                }

                #endregion

                #region Check wheather the Same Rating user is exist or not

                var lstRate = _db.GF_GolferRating.FirstOrDefault(x => x.OrderNo == rating.OrderNo &&
                    x.GolferID ==    rating.GolferID  &&
                    x.ReferenceID == rating.ReferenceID &&
                    x.CourseId  ==    rating.CourseId
                    );

                if (lstRate != null)
                {
                    return new Result
                    {
                        Id = rating.GolferID ?? 0,
                        Status = 0,
                        Error = "Rating is already given for this Order.",
                        record = null
                    };
                }

                #endregion

                rating.CreatedDate = DateTime.Now;
                _db.GF_GolferRating.Add(rating);
                _db.SaveChanges();

                return new Result
                {
                    Id = rating.ID,
                    Status = 1,
                    Error = "Rating saved successfully.",
                    record = new
                    {
                        rating.Rating,
                        rating.ReferenceID,
                        rating.ReferenceType,
                        rating.GolferID,
                        rating.CreatedDate
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