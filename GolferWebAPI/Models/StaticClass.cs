using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace GolferWebAPI.Models
{
    public class StaticClass
    {
        private GolflerEntities _db = null;

        /// <summary>
        /// Created By: Kiran Bala
        /// Created Date: 21 April 2015
        /// Purpose: Static Page Detail
        /// </summary>
        /// <param name="orderHistory"></param>
        /// <returns></returns>
        public Result GetPageDetail(GF_StaticPages objPage)
        {
            try
            {
                _db = new GolflerEntities();

                var lstStatic = _db.GF_StaticPages.Where(x => x.PageCode==objPage.PageCode && x.IsActive==true).ToList()
                    .Select(x =>
                        new
                        {
                           x.ID,
                           x.PageName,
                           x.MetaTitle,
                           x.MetaDescription,
                           x.MetaKeywords,
                          Content= x.PageHTML,
                           x.Status,
                          x.PageCode,
                          PageUrl = ConfigurationManager.AppSettings["AdminPanelUrl"]+ x.PageCode
                        });


                if (lstStatic.Count() > 0)
                {
                    return new Result
                    {
                        Id = 1,
                        Status = 1,
                        Error = "Success",
                        record = lstStatic
                    };

                }
                else
                {
                    return new Result
                    {
                        Id =0,
                        Status = 0,
                        Error = "Detail not available.",
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



    }
}