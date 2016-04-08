using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Linq;
using System.Web;
using CourseWebAPI.Models;

namespace CourseWebApi.Models
{
    public class Menu
    {
        GolflerEntities db = null;

        public Menu()
        {
            db = new GolflerEntities();
        }

        /// <summary>
        /// Created By: Amit Kumar
        /// Created Date: 27 April 2015
        /// Purpose: Compare menu with other courses
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public Result GetCamparedMenuList(CampareMenu obj)
        {
            try
            {
                var course = db.GF_CourseInfo.FirstOrDefault(x => x.ID == obj.courseID);

                double radius = CommonFunctions.ConvertMilesToMeters(10);
                var center = new GeoCoordinate(Convert.ToDouble(course.LATITUDE), Convert.ToDouble(course.LONGITUDE));

                #region Demo Distance Calculation

                //var firstCordinate = new GeoCoordinate(Convert.ToDouble(course1.LATITUDE), Convert.ToDouble(course1.LONGITUDE));
                //var secondCordinate = new GeoCoordinate(Convert.ToDouble(course2.LATITUDE), Convert.ToDouble(course2.LONGITUDE));

                //double distance = firstCordinate.GetDistanceTo(secondCordinate);

                #endregion

                var result = string.Join(",", db.GF_CourseInfo.Where(x => x.PartnershipStatus == PartershipStatus.Partner).ToList()
                    .Select(x => new
                    {
                        courseID = x.ID,
                        distance = center.GetDistanceTo(new GeoCoordinate(Convert.ToDouble(x.LATITUDE), Convert.ToDouble(x.LONGITUDE))),
                        status = x.Status
                    }).Where(x => x.distance <= radius &&
                                  x.status == StatusType.Active).Select(y => y.courseID));

                long[] courseIDs = CommonFunctions.ConvertStringArrayToLongArray(result);

                var lstMenu = (from x in db.GF_MenuItems
                               join y in db.GF_CourseFoodItemDetail on x.ID equals y.MenuItemID
                               join z in db.GF_CourseFoodItem on y.CourseFoodItemID equals z.ID
                               where x.Name.ToLower() == obj.searchText.ToLower()
                               select new
                               {
                                   z.CourseID,
                                   CourseName = z.GF_CourseInfo.COURSE_NAME,
                                   x.Name,
                                   y.Price
                               }).Where(x => courseIDs.Contains(x.CourseID ?? 0));

                //System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(z.GF_CourseInfo.COURSE_NAME.ToLower())

                if (lstMenu.Count() > 0)
                {
                    var ownMenu = lstMenu.FirstOrDefault(x => x.CourseID == obj.courseID);
                    if (ownMenu != null)
                    {
                        var lstResult = new
                        {
                            QwnCourseID = ownMenu.CourseID ?? 0,
                            QwnCourseName = ownMenu.CourseName,
                            QwnName = ownMenu.Name,
                            QwnPrice = ownMenu.Price ?? 0,
                            CampreMenuList = lstMenu.Where(x => x.CourseID != obj.courseID)
                        };

                        return new Result
                        {
                            Id = obj.courseID,
                            Status = 1,
                            Error = "Success",
                            record = lstResult
                        };
                    }
                    else
                    {
                        return new Result
                        {
                            Id = obj.courseID,
                            Status = 0,
                            Error = "This item is not in your menu list.",
                            record = null
                        };
                    }
                }

                return new Result
                {
                    Id = obj.courseID,
                    Status = 0,
                    Error = "No record found.",
                    record = null
                };
            }
            catch (Exception ex)
            {
                return new Result
                {
                    Id = obj.courseID,
                    Status = 0,
                    Error = ex.Message,
                    record = null
                };
            }
        }

        /// <summary>
        /// Created By: Amit Kumar
        /// Created Date: 27 April 2015
        /// Purpose: Get the course menu list
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public Result GetCourseUserAndMenuList(GF_AdminUsers obj)
        {
            try
            {
                #region Checks wheather the user is not course admin or proshop

                var lstUser = db.GF_AdminUsers.FirstOrDefault(x => x.ID == obj.ID &&
                     x.CourseId == obj.CourseId &&
                    (x.Type == UserType.CourseAdmin || x.Type == UserType.Proshop));

                if (lstUser == null)
                {
                    return new Result
                    {
                        Id = obj.ID,
                        Status = 0,
                        Error = "Invalid User",
                        record = null
                    };
                }

                #endregion

                var lstMenu = db.GF_CourseMenu.Where(y => y.CourseID == obj.CourseId).ToList()
                                .Select(y => new
                                {
                                    CourseMenuID = y.ID,
                                    CategoryID = y.CategoryID,
                                    CategoryName = y.GF_Category.Name,
                                    //IsActive = y.GF_CourseFoodItem.Where(q => q.AllowStatus == StatusType.Active).Count() > 0,
                                    SubCategory =
                                    y.GF_Category.GF_SubCategory.Where(p => p.Status == StatusType.Active).Select(x => new
                                        {
                                            ID = x.ID,
                                            Name = x.Name,
                                            CategoryID = x.CategoryID,
                                            IsActive = y.GF_CourseFoodItem.Where(q => q.SubCategoryID == x.ID && (q.IsAllow ?? false)).Count() > 0,
                                            MenuItems = ((y.GF_CourseFoodItem.Where(q => q.SubCategoryID == x.ID && (q.IsAllow ?? false)).Count() > 0) ?
                                            (x.GF_MenuItems.Where(p => p.Status == StatusType.Active).ToList()
                                            .Select(p => new
                                            {
                                                ID = p.GF_CourseFoodItemDetail.FirstOrDefault(q => q.GF_CourseFoodItem.CourseID == obj.CourseId) == null ? 0 : p.GF_CourseFoodItemDetail.FirstOrDefault(q => q.GF_CourseFoodItem.CourseID == obj.CourseId).ID,
                                                CourseFoodItemID = p.GF_CourseFoodItemDetail.FirstOrDefault(q => q.GF_CourseFoodItem.CourseID == obj.CourseId) == null ? 0 : p.GF_CourseFoodItemDetail.FirstOrDefault(q => q.GF_CourseFoodItem.CourseID == obj.CourseId).CourseFoodItemID,
                                                MenuItemID = p.ID,
                                                ItemName = p.Name,
                                                Price = p.GF_CourseFoodItemDetail.FirstOrDefault(q => q.GF_CourseFoodItem.CourseID == obj.CourseId) == null ? p.Amount : p.GF_CourseFoodItemDetail.FirstOrDefault(q => q.GF_CourseFoodItem.CourseID == obj.CourseId).Price,
                                                IsAllow = p.GF_CourseFoodItemDetail.FirstOrDefault(q => q.GF_CourseFoodItem.CourseID == obj.CourseId) == null ? false : p.GF_CourseFoodItemDetail.FirstOrDefault(q => q.GF_CourseFoodItem.CourseID == obj.CourseId).IsActive ?? false,
                                                MenuOption = p.GF_MenuItemOption.Where(k => k.Status == StatusType.Active && p.ID < 0).Select(k => new { k.ID, k.Name })
                                            }).ToList()) :
                                            (x.GF_MenuItems.Where(p => p.Status == StatusType.Active && p.ID < 0).ToList()
                                            .Select(p => new
                                            {
                                                ID = p.GF_CourseFoodItemDetail.FirstOrDefault(q => q.GF_CourseFoodItem.CourseID == obj.CourseId) == null ? 0 : p.GF_CourseFoodItemDetail.FirstOrDefault(q => q.GF_CourseFoodItem.CourseID == obj.CourseId).ID,
                                                CourseFoodItemID = p.GF_CourseFoodItemDetail.FirstOrDefault(q => q.GF_CourseFoodItem.CourseID == obj.CourseId) == null ? 0 : p.GF_CourseFoodItemDetail.FirstOrDefault(q => q.GF_CourseFoodItem.CourseID == obj.CourseId).CourseFoodItemID,
                                                MenuItemID = p.ID,
                                                ItemName = p.Name,
                                                Price = p.GF_CourseFoodItemDetail.FirstOrDefault(q => q.GF_CourseFoodItem.CourseID == obj.CourseId) == null ? p.Amount : p.GF_CourseFoodItemDetail.FirstOrDefault(q => q.GF_CourseFoodItem.CourseID == obj.CourseId).Price,
                                                IsAllow = p.GF_CourseFoodItemDetail.FirstOrDefault(q => q.GF_CourseFoodItem.CourseID == obj.CourseId) == null ? false : p.GF_CourseFoodItemDetail.FirstOrDefault(q => q.GF_CourseFoodItem.CourseID == obj.CourseId).IsActive ?? false,
                                                MenuOption = p.GF_MenuItemOption.Where(k => k.Status == StatusType.Active && p.ID < 0).Select(k => new { k.ID, k.Name })
                                            }).ToList()))
                                        })
                                    //y.GF_CourseFoodItem.Where(q => q.AllowStatus == StatusType.Active).Select(x => new
                                    //{
                                    //    ID = x.SubCategoryID,
                                    //    x.GF_SubCategory.Name,
                                    //    x.GF_SubCategory.CategoryID,
                                    //    //MenuItems = x.GF_CourseFoodItemDetail.Where(p => p.Status == StatusType.Active).ToList()
                                    //    //.Select(m => new
                                    //    //{
                                    //    //    CourseFoodItemID = m.CourseFoodItemID,
                                    //    //    MenuItemID = m.MenuItemID,
                                    //    //    Name = m.GF_MenuItems.Name,
                                    //    //    Amount = (y.GF_CourseInfo.IsPlateformFeeActive ?? false) ? m.Price :
                                    //    //              m.Price + (y.GF_CourseInfo.GF_FoodCommission.FirstOrDefault(z => z.CategoryID == y.CategoryID &&
                                    //    //                         z.CourseID == obj.CourseId).Commission ?? 0)
                                    //    //}).AsQueryable(),
                                    //    MenuItems = db.GF_MenuItems.Where(p => p.Status == StatusType.Active && p.SubCategoryID == x.SubCategoryID).ToList()
                                    //    .Select(p => new
                                    //    {
                                    //        ID = p.GF_CourseFoodItemDetail.FirstOrDefault(q => q.GF_CourseFoodItem.CourseID == obj.CourseId) == null ? 0 : p.GF_CourseFoodItemDetail.FirstOrDefault(q => q.GF_CourseFoodItem.CourseID == obj.CourseId).ID,
                                    //        CourseFoodItemID = p.GF_CourseFoodItemDetail.FirstOrDefault(q => q.GF_CourseFoodItem.CourseID == obj.CourseId) == null ? 0 : p.GF_CourseFoodItemDetail.FirstOrDefault(q => q.GF_CourseFoodItem.CourseID == obj.CourseId).CourseFoodItemID,
                                    //        MenuItemID = p.ID,
                                    //        ItemName = p.Name,
                                    //        Price = p.GF_CourseFoodItemDetail.FirstOrDefault(q => q.GF_CourseFoodItem.CourseID == obj.CourseId) == null ? p.Amount : p.GF_CourseFoodItemDetail.FirstOrDefault(q => q.GF_CourseFoodItem.CourseID == obj.CourseId).Price,
                                    //        IsAllow = p.GF_CourseFoodItemDetail.FirstOrDefault(q => q.GF_CourseFoodItem.CourseID == obj.CourseId) == null ? false : p.GF_CourseFoodItemDetail.FirstOrDefault(q => q.GF_CourseFoodItem.CourseID == obj.CourseId).IsActive ?? false
                                    //    }).ToList(),
                                    //    IsAllow = (x.IsAllow ?? false)
                                    //})
                                });

                var CourseAdminID = db.GF_AdminUsers.FirstOrDefault(x => x.CourseId == obj.CourseId &&
                    x.Status == StatusType.Active &&
                    x.Type == UserType.CourseAdmin);

                var lstAdminUser = db.GF_AdminUsers.Where(y => y.CourseId == obj.CourseId && y.Status != StatusType.Delete &&
                    y.ID != obj.ID).ToList()
                    .Select(y => new
                    {
                        y.ID,
                        y.UserName,
                        Password = CommonFunctions.DecryptString(y.Password, y.SALT),
                        //y.Password,
                        y.Email,
                        y.FirstName,
                        y.LastName,
                        y.Phone,
                        UserType = y.Type
                    });

                lstAdminUser = lstAdminUser.Where(y => y.ID != (CourseAdminID == null ? 0 : CourseAdminID.ID));

                return new Result
                {
                    Id = obj.CourseId ?? 0,
                    Status = 1,
                    Error = "",
                    record = new
                    {
                        CourseMenuItems = lstMenu.ToList(),
                        AdminUser = lstAdminUser.ToList()
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
        /// Created Date: 28 April 2015
        /// Purpose: Activate sub category of course
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public Result ActivateSubCategory(GF_CourseFoodItem courseFoodItem)
        {
            try
            {
                var objFoodItem = db.GF_CourseFoodItem.FirstOrDefault(x => x.SubCategoryID == (courseFoodItem.SubCategoryID ?? 0) &&
                    x.CourseID == (courseFoodItem.CourseID ?? 0));

                string msg = "";

                if (objFoodItem != null)
                {
                    objFoodItem.AllowStatus = (courseFoodItem.IsAllow ?? false) ? StatusType.Active : StatusType.InActive;
                    objFoodItem.IsAllow = courseFoodItem.IsAllow ?? false;
                    objFoodItem.ModifyBy = courseFoodItem.UserID;
                    objFoodItem.ModifyDate = DateTime.Now;
                    db.SaveChanges();

                    courseFoodItem.ID = objFoodItem.ID;
                    msg = (objFoodItem.IsAllow ?? false) ? "Food item activated successfully" : "Food item deactivated successfully";
                }
                else
                {
                    #region Add Course Menu

                    var lstCourseMenu = db.GF_CourseMenu.FirstOrDefault(x => x.CourseID == (courseFoodItem.CourseID ?? 0));

                    if (lstCourseMenu == null)
                    {
                        var lstCategory = db.GF_Category.Where(x => x.Status == StatusType.Active);

                        foreach (var item in lstCategory)
                        {
                            GF_CourseMenu courseMenu = new GF_CourseMenu();
                            courseMenu.CategoryID = item.ID;
                            courseMenu.CourseID = courseFoodItem.CourseID ?? 0;
                            courseMenu.CreatedBy = courseFoodItem.UserID.ToString();
                            courseMenu.CreatedDate = DateTime.Now;
                            courseMenu.Status = StatusType.Active;
                            courseMenu.IsActive = true;

                            db.GF_CourseMenu.Add(courseMenu);
                        }
                        db.SaveChanges();
                    }

                    #endregion

                    var catID = db.GF_SubCategory.FirstOrDefault(x => x.ID == (courseFoodItem.SubCategoryID ?? 0)).CategoryID;

                    GF_CourseFoodItem obj = new GF_CourseFoodItem();
                    obj.CourseID = courseFoodItem.CourseID ?? 0;
                    obj.CourseMenuID = db.GF_CourseMenu.FirstOrDefault(x => x.CategoryID == catID &&
                                                x.CourseID == (courseFoodItem.CourseID ?? 0)).ID;
                    obj.SubCategoryID = courseFoodItem.SubCategoryID ?? 0;
                    obj.AllowStatus = StatusType.Active;
                    obj.IsAllow = true;
                    obj.CreatedBy = courseFoodItem.UserID;
                    obj.CreatedDate = DateTime.Now;
                    db.GF_CourseFoodItem.Add(obj);
                    db.SaveChanges();

                    courseFoodItem.ID = obj.ID;
                    msg = (obj.IsAllow ?? false) ? "Food item activated successfully" : "Food item deactivated successfully";
                }

                return new Result
                {
                    Id = courseFoodItem.ID,
                    Status = 1,
                    Error = msg
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
        /// Created Date: 28 April 2015
        /// Purpose: Save food items
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public Result SaveCourseFoodItem(GF_CourseFoodItemDetail obj)
        {
            try
            {
                var lstFoodItem = db.GF_CourseFoodItemDetail.FirstOrDefault(x => x.MenuItemID == obj.MenuItemID &&
                    x.GF_CourseFoodItem.CourseID == obj.CourseID);

                if (lstFoodItem != null)
                {
                    lstFoodItem.Price = obj.Price;
                    lstFoodItem.IsActive = obj.IsAllow ?? false;
                    lstFoodItem.Status = (obj.IsAllow ?? false) ? StatusType.Active : StatusType.InActive;
                    lstFoodItem.ModifyBy = obj.UserID;
                    lstFoodItem.ModifyDate = DateTime.Now;

                    db.SaveChanges();

                    return new Result
                    {
                        Id = lstFoodItem.ID,
                        Status = 1,
                        Error = "Food item updated successfully.",
                        record = null
                    };
                }
                else
                {
                    GF_CourseFoodItemDetail courseFoodItem = new GF_CourseFoodItemDetail();

                    if ((obj.CourseFoodItemID ?? 0) <= 0)
                    {
                        var subCategory = db.GF_MenuItems.FirstOrDefault(x => x.ID == obj.MenuItemID);

                        if (subCategory != null)
                        {
                            var courseFoodItemID = db.GF_CourseFoodItem.FirstOrDefault(x => x.SubCategoryID == subCategory.SubCategoryID &&
                                x.CourseID == obj.CourseID);

                            obj.CourseFoodItemID = courseFoodItemID.ID;
                        }
                    }

                    courseFoodItem.CourseFoodItemID = obj.CourseFoodItemID;
                    courseFoodItem.MenuItemID = obj.MenuItemID;
                    courseFoodItem.Price = obj.Price;
                    courseFoodItem.IsActive = obj.IsAllow ?? false;
                    courseFoodItem.Status = (obj.IsAllow ?? false) ? StatusType.Active : StatusType.InActive;
                    courseFoodItem.CreatedBy = obj.UserID;
                    courseFoodItem.CreatedDate = DateTime.Now;

                    db.GF_CourseFoodItemDetail.Add(courseFoodItem);
                    db.SaveChanges();

                    return new Result
                    {
                        Id = courseFoodItem.ID,
                        Status = 1,
                        Error = "Food item saved successfully.",
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

    public class CampareMenu
    {
        public string searchText { get; set; }
        public long courseID { get; set; }
    }

    public partial class GF_MenuItems
    {
        public decimal Price { get; set; }
    }

    public partial class GF_CourseFoodItem
    {
        public long UserID { get; set; }
    }

    public partial class GF_CourseFoodItemDetail
    {
        public bool? IsAllow { get; set; }
        public long UserID { get; set; }
        public long CourseID { get; set; }
    }
}