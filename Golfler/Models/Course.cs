using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.EntityClient;
using System.Data.Objects;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Golfler.Models
{
    public class Course
    {
        public GF_CourseSuggest OrderObj { get; private set; }

        protected GolflerEntities Db;

        public long Userid { get; set; }

        public bool Active { get; set; }

        public string Message { get; private set; }

        #region Constructors

        public Course()
        {
            Db = new GolflerEntities();
        }

        #endregion

        /// <summary>
        /// Created By: Amit Kumar
        /// Created on: 28 March 2015
        /// </summary>
        /// <remarks>Get Course Suggest Listing</remarks>
        public IQueryable<GF_CourseSuggest> GetCourseSuggest(string filterExpression, string sortExpression, string sortDirection, int pageIndex, int pageSize, ref int totalRecords)
        {
            IQueryable<GF_CourseSuggest> list;

            if (!String.IsNullOrWhiteSpace(filterExpression))
                list = Db.GF_CourseSuggest.Where(x => (x.Name.ToLower().Contains(filterExpression.ToLower())));
            else
                list = Db.GF_CourseSuggest.OrderByDescending(x => x.ID);

            totalRecords = list.Count();

            return list.OrderBy(sortExpression + " " + sortDirection).Skip((pageIndex - 1) * pageSize).Take(pageSize);
        }

        public IQueryable<GF_CourseSuggest> GetGolferListForSuggestCourse(string filterExpression, string sortExpression, string sortDirection, int pageIndex, int pageSize, ref int totalRecords)
        {
            IQueryable<GF_CourseSuggest> list;

            //if (!String.IsNullOrWhiteSpace(filterExpression))
                list = Db.GF_CourseSuggest.Where(x => (x.Name.ToLower().Equals(filterExpression.ToLower())));
         //   else
           //     list = Db.GF_CourseSuggest.OrderByDescending(x => x.ID);

            totalRecords = list.Count();

            return list.OrderBy(sortExpression + " " + sortDirection).Skip((pageIndex - 1) * pageSize).Take(pageSize);
        }

        public IQueryable<CourseSuggest> GetSuggestedCourseList(string filterExpression, string sortExpression, string sortDirection, int pageIndex, int pageSize, ref int totalRecords)
        {
            List<CourseSuggest> courselist = new List<CourseSuggest>(); ;
            IQueryable<CourseSuggest> finalList;
            List<GF_CourseSuggest> list;

            if (!String.IsNullOrWhiteSpace(filterExpression))
                list = Db.GF_CourseSuggest.Where(x => (x.Name.ToLower().Contains(filterExpression.ToLower()))).ToList();
            else
                list = Db.GF_CourseSuggest.OrderByDescending(x => x.ID).ToList();

            List<GF_CourseSuggest> listFiltered = list.GroupBy(x => x.Name).Select(x => x.First()).ToList();
            foreach (var objFilter in listFiltered)
            {
                CourseSuggest objCourse = new CourseSuggest();
                // objCourse.courseid = objFilter.Name;
                objCourse.coursename = objFilter.Name;
                objCourse.NoOfSuggestions = list.Where(x => x.Name == objFilter.Name).Count();

                courselist.Add(objCourse);
            }

            finalList = courselist.AsQueryable();
            totalRecords = finalList.Count();

            return finalList.OrderBy(sortExpression + " " + sortDirection).Skip((pageIndex - 1) * pageSize).Take(pageSize);
        }

        /// <summary>
        /// Created By:Arun
        /// Created date:31 March 2015
        /// purpose: Get Course listing
        /// </summary>
        /// <param name="filterExpression"></param>
        /// <param name="sortExpression"></param>
        /// <param name="sortDirection"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalRecords"></param>
        /// <returns></returns>
        public IQueryable<GF_CourseInfo> GetCoursesInfo_Old(string filterExpression, string cityName, string partnerType, string sortExpression, string sortDirection, int pageIndex, int pageSize, ref int totalRecords)
        {
            IQueryable<GF_CourseInfo> list;

            if (!String.IsNullOrWhiteSpace(filterExpression))
            {
                //list = Db.GF_CourseInfo.Where(x => (x.COURSE_NAME.ToLower().StartsWith(filterExpression.ToLower()) ||
                //                                    x.CITY.ToLower().Contains(filterExpression.ToLower())) &&
                //                                   (x.Status ?? StatusType.InActive) != StatusType.Delete);
                totalRecords = Db.GF_CourseInfo.Where(x => x.COURSE_NAME.ToLower().StartsWith(filterExpression.ToLower()) &&
                                                  (x.Status ?? StatusType.InActive) != StatusType.Delete).Count();

                list = Db.GF_CourseInfo.Where(x => x.COURSE_NAME.ToLower().StartsWith(filterExpression.ToLower()) &&
                                             (x.Status ?? StatusType.InActive) != StatusType.Delete)
                                       .OrderBy(sortExpression + " " + sortDirection).Skip((pageIndex - 1) * pageSize).Take(pageSize);
            }
            else
            {
                totalRecords = Db.GF_CourseInfo.Where(x => (x.Status ?? StatusType.InActive) != StatusType.Delete).OrderByDescending(x => x.ID).Count();

                list = Db.GF_CourseInfo.Where(x => (x.Status ?? StatusType.InActive) != StatusType.Delete).OrderByDescending(x => x.ID)
                    .OrderBy(sortExpression + " " + sortDirection).Skip((pageIndex - 1) * pageSize).Take(pageSize);
            }

            if (!string.IsNullOrWhiteSpace(partnerType))
            {
                list = list.Where(x => x.PartnershipStatus == partnerType);
                totalRecords = list.Count();
            }

            if (!string.IsNullOrWhiteSpace(cityName))
            {
                list = list.Where(x => x.CITY.ToLower().StartsWith(cityName.ToLower()));
                totalRecords = list.Count();
            }

            //totalRecords = list.Count();

            return list;
        }

        /// <summary>
        /// Created By:Amit Kumar
        /// Created date:31 March 2015
        /// purpose: Get Course listing
        /// </summary>
        /// <param name="filterExpression"></param>
        /// <param name="sortExpression"></param>
        /// <param name="sortDirection"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalRecords"></param>
        /// <returns></returns>
        public IQueryable<CourseInformation> GetCoursesInfo(string filterExpression, string cityName, string partnerType, string sortExpression, string sortDirection, int pageIndex, int pageSize, ref int totalRecords)
        {
            IQueryable<CourseInformation> list;

            var cName = new SqlParameter
            {
                ParameterName = "CourseName",
                Value = filterExpression
            };

            var pType = new SqlParameter
            {
                ParameterName = "PartnerType",
                Value = partnerType
            };

            var ctyName = new SqlParameter
            {
                ParameterName = "CityName",
                Value = cityName
            };

            list = Db.Database.SqlQuery<CourseInformation>("exec GF_SP_GetCourseInformation @CourseName, @PartnerType, @CityName",
                cName, pType, ctyName).ToList<CourseInformation>().AsQueryable();

            totalRecords = list.Count();

            return list.OrderBy(sortExpression + " " + sortDirection).Skip((pageIndex - 1) * pageSize).Take(pageSize);
        }

        /// <summary>
        /// Created By:Amit Kumar
        /// Created date:31 March 2015
        /// purpose: Get Course listing
        /// </summary>
        /// <param name="filterExpression"></param>
        /// <param name="sortExpression"></param>
        /// <param name="sortDirection"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalRecords"></param>
        /// <returns></returns>
        public IQueryable<CourseInfoDetail> GetCourseListInfo(string filterExpression, string cityName,
            string sortExpression, string sortDirection, int pageIndex, int pageSize, ref int totalRecords)
        {
            IQueryable<CourseInfoDetail> list;

            var cName = new SqlParameter
            {
                ParameterName = "CourseName",
                Value = filterExpression
            };

            var ctyName = new SqlParameter
            {
                ParameterName = "CityName",
                Value = cityName
            };

            var cID = new SqlParameter
            {
                ParameterName = "CourseID",
                Value = LoginInfo.CourseId
            };

            list = Db.Database.SqlQuery<CourseInfoDetail>("exec GF_SP_GetCourseList @CourseName, @CityName, @CourseID",
                cName, ctyName, cID).ToList<CourseInfoDetail>().AsQueryable();

            totalRecords = list.Count();

            return list.OrderBy(sortExpression + " " + sortDirection).Skip((pageIndex - 1) * pageSize).Take(pageSize);
        }

        public string GetCourseAdmin(long CourseID)
        {
            try
            {
                var lstCourseAdmin = Db.GF_AdminUsers.FirstOrDefault(q => (q.CourseId ?? 0) == CourseID && q.Type == UserType.CourseAdmin);

                if (lstCourseAdmin != null)
                {
                    return lstCourseAdmin.UserName;
                }

                return "";
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        /// Created By:Arun
        /// Created date:31 March 2015
        /// purpose: Get Course By ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public GF_CourseInfo GetCourseByID(long id)
        {
            var obj = Db.GF_CourseInfo.FirstOrDefault(x => x.ID == id);

            if (obj.FoodCommission == null)
            {
                var lstFoodCommission = Db.GF_FoodCommission.Where(x => x.CourseID == id).ToList();

                if (lstFoodCommission.Count() == 0)
                {
                    obj.FoodCommission = Db.GF_Category.Where(x => x.Status == StatusType.Active).ToList().Select(x => new GF_FoodCommission
                    {
                        CategoryID = x.ID,
                        CategoryName = x.Name,
                        Commission = 0,
                        CourseID = LoginInfo.CourseId
                    }).ToList();
                }
                else
                {
                    obj.FoodCommission = lstFoodCommission.Select(x => new GF_FoodCommission
                    {
                        CategoryID = x.CategoryID,
                        CategoryName = x.GF_Category.Name,
                        Commission = x.Commission,
                        CourseID = x.CourseID
                    }).ToList();
                }
            }

            if (obj == null)
            {
                obj = new GF_CourseInfo();

                obj.FoodCommission = Db.GF_Category.Where(x => x.Status == StatusType.Active).ToList().Select(x => new GF_FoodCommission
                {
                    CategoryID = x.ID,
                    CategoryName = x.Name,
                    Commission = 0,
                    CourseID = LoginInfo.CourseId
                }).ToList();
            }

            if (obj.CourseMenu == null)
            {
                var lstCourseMenu = Db.GF_CourseMenu.Where(x => x.CourseID == id).ToList();

                if (lstCourseMenu.Count() == 0)
                {
                    obj.CourseMenu = Db.GF_Category.Where(x => x.Status == StatusType.Active).ToList().Select(x => new GF_CourseMenu
                    {
                        CategoryID = x.ID,
                        CategoryName = x.Name,
                        TaxPercentage = 0,
                        CourseID = LoginInfo.CourseId
                    }).ToList();
                }
                else
                {
                    obj.CourseMenu = lstCourseMenu.Select(x => new GF_CourseMenu
                    {
                        CategoryID = x.CategoryID,
                        CategoryName = x.GF_Category.Name,
                        TaxPercentage = x.TaxPercentage,
                        CourseID = x.CourseID
                    }).ToList();
                }
            }

            return obj;

        }

        /// Created By:Arun
        /// Created date:31 March 2015
        /// purpose: Get user by Course ID
        /// 
        public long GetUserByCourseID(long id)
        {
            //var obj = Db.GF_CourseUsers.FirstOrDefault(x => x.CourseID == id);
            var obj = Db.GF_AdminUsers.FirstOrDefault(x => x.CourseId == id);
            if (obj == null)
                return 0;

            return obj.ID;

        }

        /// <summary>
        /// Created By:Arun
        /// Created date:31 March 2015
        /// purpose: Get Course users
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public List<GF_AdminUsers> GetCourseUser()
        {
            var lstUser = Db.GF_AdminUsers.Where(x => x.Type == UserType.CourseAdmin &&
                x.Status == StatusType.Active).ToList();
            return lstUser;
        }

        /// <summary>
        /// Created By: Amit Kumar
        /// Created date: 29 April 2015
        /// purpose: Get Course users
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public List<GF_AdminUsers> GetCourseUserByCourseID(long? courseID)
        {
            var lstAdminUser = Db.GF_AdminUsers.FirstOrDefault(x => x.CourseId == courseID &&
                x.Status == StatusType.Active &&
                x.Type == UserType.CourseAdmin);

            var lstUser = Db.GF_AdminUsers.Where(x => x.Type == UserType.CourseAdmin &&
                    x.Status == StatusType.Active &&
                    (x.CourseId ?? 0) <= 0).ToList();

            if (lstAdminUser != null)
            {
                lstUser = lstUser.Union(Db.GF_AdminUsers.Where(x => x.ID == lstAdminUser.ID)).OrderBy(x => x.FirstName).ToList();
            }

            return lstUser;
        }

        public List<GF_AdminUsers> GetCountry()
        {
            var lstUser = Db.GF_AdminUsers.Where(x => x.Type == UserType.CourseAdmin && x.Status == StatusType.Active).ToList();
            return lstUser;
        }
        public List<GF_AdminUsers> GetState()
        {
            var lstUser = Db.GF_AdminUsers.Where(x => x.Type == UserType.CourseAdmin && x.Status == StatusType.Active).ToList();
            return lstUser;
        }

        /// <summary>
        /// Created By:Arun
        /// Created date:31 March 2015
        /// purpose: Add/Update Course Info
        /// </summary>
        /// <param name="objCourse"></param>
        /// <returns></returns>
        public bool updateCourseInfo(GF_CourseInfo objCourse, long userid, ref long courseID)
        {

            if (objCourse.ID == 0) //ADD
            {
                objCourse.Status = objCourse.Active ? StatusType.Active : StatusType.InActive;
                objCourse.CreatedDate = DateTime.UtcNow;
                objCourse.ModifyBy = LoginInfo.UserId;
                objCourse.IsClubHouse = true;
                objCourse.ClubHouseID = 0;
                Db.GF_CourseInfo.Add(objCourse);
                Db.SaveChanges();

                courseID = objCourse.ID;

                #region Update User table by Course ID

                GF_AdminUsers objAdminUser = Db.GF_AdminUsers.FirstOrDefault(x => x.ID == objCourse.UserID);
                if (objAdminUser != null)
                {
                    objAdminUser.CourseId = objCourse.ID;
                    Db.SaveChanges();

                    #region send mail to Course Admin
                    try
                    {
                        string mailresult = "";

                        IQueryable<GF_EmailTemplatesFields> templateFields = null;
                        var param = EmailParams.GetEmailParamsNew(ref Db, EmailTemplateName.AssignCourseAdmin, ref templateFields, 0, LoginInfo.LoginType, ref mailresult);

                        if (mailresult == "") // means Parameters are OK
                        {
                            if (ApplicationEmails.AssignCourseAdmin(ref Db, objCourse, param, ref templateFields, ref mailresult))
                            {
                                // Do steps for Mail Send successful
                            }
                            else
                            {
                                // Do steps for Mail Failure. Mail failure reason can be find on "mailresult"
                            }
                        }
                        else
                        {
                            // Do steps for Parameters not available.Reason can be find on "mailresult"
                        }
                    }
                    catch (Exception ex)
                    {
                        ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
                    }
                    #endregion

                    //#region We will not use this table , so need to remove this.... GF_CourseUsers
                    #region CourseUsers

                    ///Due to some reason we also need to update this GF_CourseUsers table
                    ///In this table we duplicate the entry of user course id
                    ///Because somewhere this table is used for manupulation

                    var objUser = new GF_CourseUsers();
                    objUser.CourseID = objCourse.ID;
                    objUser.UserID = objCourse.UserID;
                    Db.GF_CourseUsers.Add(objUser);
                    Db.SaveChanges();

                    #endregion
                }

                #endregion

                if (objCourse.ID > 0 && objCourse.FoodCommission != null && !(objCourse.IsPlateformFeeActive ?? false))
                {
                    #region Delete Old Records

                    var results = from c in Db.GF_FoodCommission
                                  where c.CourseID == objCourse.ID
                                  select c;

                    foreach (var item in results)
                    {
                        Db.GF_FoodCommission.Remove(item);
                    }

                    Db.SaveChanges();

                    #endregion

                    #region Insert Commission Detail

                    foreach (var item in objCourse.FoodCommission)
                    {
                        GF_FoodCommission foodCommission = new GF_FoodCommission();
                        foodCommission.CourseID = objCourse.ID;
                        foodCommission.CategoryID = item.CategoryID;
                        foodCommission.Commission = item.Commission;

                        Db.GF_FoodCommission.Add(foodCommission);
                        Db.SaveChanges();
                    }

                    #endregion
                }
                #region Add Email Templates for this Course
                try
                {
                    Db.GF_SP_CopyModules(objCourse.ID);
                }
                catch
                {

                }
                #endregion
            }
            else   //UPDATE
            {
                var obj = Db.GF_CourseInfo.FirstOrDefault(x => x.ID == objCourse.ID);
                if (obj != null)
                {
                    obj.COURSE_NAME = objCourse.COURSE_NAME;
                    obj.ADDRESS = objCourse.ADDRESS;
                    obj.CITY = objCourse.CITY;
                    obj.STATE = objCourse.STATE;
                    obj.COUNTY = objCourse.COUNTY;
                    obj.PHONE = objCourse.PHONE;
                    obj.Status = objCourse.Active ? StatusType.Active : StatusType.InActive;
                    obj.LATITUDE = objCourse.LATITUDE;
                    obj.LONGITUDE = objCourse.LONGITUDE;
                    obj.Description = objCourse.Description;
                    obj.IsPlateformFeeActive = objCourse.IsPlateformFeeActive;
                    obj.PlateformFee = objCourse.PlateformFee;
                    obj.PartnershipStatus = objCourse.PartnershipStatus;
                    obj.Tax = objCourse.Tax;

                    obj.TYPE = objCourse.TYPE;
                    obj.GREENGRASSTYPE = objCourse.GREENGRASSTYPE;
                    obj.SANDBUNKERS = objCourse.SANDBUNKERS;
                    obj.YARDAGEMARKERS = objCourse.YARDAGEMARKERS;
                    obj.ACCESS = objCourse.ACCESS;

                    obj.DISCOUNTS = objCourse.DISCOUNTS;
                    obj.RENTALCLUBS = objCourse.RENTALCLUBS;
                    obj.PULLCARTS = objCourse.PULLCARTS;
                    obj.WALKING = objCourse.WALKING;
                    obj.RESTAURANT = objCourse.RESTAURANT;
                    obj.Food = objCourse.Food;
                    obj.AVAILABLEPRODUCTS = objCourse.AVAILABLEPRODUCTS;
                    obj.BAR = objCourse.BAR;
                    obj.HOURS = objCourse.HOURS;
                    obj.HOMESONCOURSE = objCourse.HOMESONCOURSE;
                    obj.ZIPCODE = objCourse.ZIPCODE;

                    obj.ModifyDate = DateTime.UtcNow;
                    obj.ModifyBy = LoginInfo.UserId;

                    //obj.Status = objCourse.Status;

                    #region If Course Admin is changed than update user table for the same

                    var PrevUsers = Db.GF_AdminUsers.Where(x => x.CourseId == objCourse.ID && x.Type == UserType.CourseAdmin && x.Status != StatusType.Delete).FirstOrDefault();

                    GF_AdminUsers objAdminUser = new GF_AdminUsers();

                    if (PrevUsers != null && PrevUsers.ID != objCourse.UserID) // means admin is changed
                    {
                        // update old user course id as 0
                        objAdminUser = Db.GF_AdminUsers.FirstOrDefault(x => x.ID == PrevUsers.ID);
                        if (objAdminUser != null)
                        {
                            objAdminUser.CourseId = 0;
                            Db.SaveChanges();
                        }
                    }

                    // update new user course id
                    objAdminUser = Db.GF_AdminUsers.FirstOrDefault(x => x.ID == objCourse.UserID);
                    if (objAdminUser != null)
                    {
                        objAdminUser.CourseId = objCourse.ID;
                        Db.SaveChanges();

                        #region send mail to Course Admin
                        try
                        {
                            string mailresult = "";

                            IQueryable<GF_EmailTemplatesFields> templateFields = null;
                            var param = EmailParams.GetEmailParamsNew(ref Db, EmailTemplateName.AssignCourseAdmin, ref templateFields, 0, LoginInfo.LoginType, ref mailresult);

                            if (mailresult == "") // means Parameters are OK
                            {
                                if (ApplicationEmails.AssignCourseAdmin(ref Db, objCourse, param, ref templateFields, ref mailresult))
                                {
                                    // Do steps for Mail Send successful
                                }
                                else
                                {
                                    // Do steps for Mail Failure. Mail failure reason can be find on "mailresult"
                                }
                            }
                            else
                            {
                                // Do steps for Parameters not available.Reason can be find on "mailresult"
                            }
                        }
                        catch (Exception ex)
                        {
                            ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
                        }
                        #endregion
                    }

                    #region CourseUsers

                    ///Due to some reason we also need to update this GF_CourseUsers table
                    ///In this table we duplicate the entry of user course id
                    ///Because somewhere this table is used for manupulation

                    var objUser = Db.GF_CourseUsers.FirstOrDefault(x => x.CourseID == objCourse.ID);
                    if (objUser == null && userid != null)
                    {
                        objUser = new GF_CourseUsers();
                        objUser.CourseID = objCourse.ID;
                        objUser.UserID = objCourse.UserID;
                        Db.GF_CourseUsers.Add(objUser);
                        Db.SaveChanges();
                    }
                    else
                    {
                        obj.UserID = objCourse.UserID;
                        Db.SaveChanges();
                    }

                    #endregion

                    #endregion

                    obj.UserID = objCourse.UserID;//course user id
                    obj.Country = objCourse.Country;
                    obj.IsClubHouse = true;
                    objCourse.ClubHouseID = 0;
                    Db.SaveChanges();

                    courseID = objCourse.ID;

                    if (objCourse.ID > 0 && objCourse.FoodCommission != null && !(objCourse.IsPlateformFeeActive ?? false))
                    {
                        #region Delete Old Records

                        var results = from c in Db.GF_FoodCommission
                                      where c.CourseID == obj.ID
                                      select c;

                        foreach (var item in results)
                        {
                            Db.GF_FoodCommission.Remove(item);
                        }

                        Db.SaveChanges();

                        #endregion

                        #region Insert Commission Detail

                        foreach (var item in objCourse.FoodCommission)
                        {
                            GF_FoodCommission foodCommission = new GF_FoodCommission();
                            foodCommission.CourseID = objCourse.ID;
                            foodCommission.CategoryID = item.CategoryID;
                            foodCommission.Commission = item.Commission;

                            Db.GF_FoodCommission.Add(foodCommission);
                            Db.SaveChanges();
                        }

                        #endregion
                    }
                }
            }
            return true;

        }

        public bool updateCourseInfoByCourseAdmin(GF_CourseInfo objCourse)
        {
            var obj = Db.GF_CourseInfo.FirstOrDefault(x => x.ID == objCourse.ID);
            if (obj != null)
            {
                obj.COURSE_NAME = objCourse.COURSE_NAME;
                obj.ADDRESS = objCourse.ADDRESS;
                obj.CITY = objCourse.CITY;
                obj.STATE = objCourse.STATE;
                obj.COUNTY = objCourse.COUNTY;

                obj.PHONE = objCourse.PHONE;
                obj.LATITUDE = objCourse.LATITUDE;
                obj.LONGITUDE = objCourse.LONGITUDE;
                obj.Description = objCourse.Description;
                obj.IsPlateformFeeActive = objCourse.IsPlateformFeeActive;
                obj.PlateformFee = objCourse.PlateformFee;
                obj.Tax = objCourse.Tax;


                obj.TYPE = objCourse.TYPE;
                obj.GREENGRASSTYPE = objCourse.GREENGRASSTYPE;
                obj.SANDBUNKERS = objCourse.SANDBUNKERS;
                obj.YARDAGEMARKERS = objCourse.YARDAGEMARKERS;
                obj.ACCESS = objCourse.ACCESS;

                obj.DISCOUNTS = objCourse.DISCOUNTS;
                obj.RENTALCLUBS = objCourse.RENTALCLUBS;
                obj.PULLCARTS = objCourse.PULLCARTS;
                obj.WALKING = objCourse.WALKING;
                obj.RESTAURANT = objCourse.RESTAURANT;
                obj.Food = objCourse.Food;
                obj.AVAILABLEPRODUCTS = objCourse.AVAILABLEPRODUCTS;
                obj.BAR = objCourse.BAR;
                obj.HOURS = objCourse.HOURS;
                obj.HOMESONCOURSE = objCourse.HOMESONCOURSE;
                obj.ZIPCODE = objCourse.ZIPCODE;
                obj.UserID = objCourse.UserID;//course user id


                obj.Country = objCourse.Country;

                //  obj.Status = objCourse.Status;


                //long userid = objCourse.UserID;

                //var objUser = Db.GF_CourseUsers.FirstOrDefault(x => x.CourseID == objCourse.ID);
                //if (objUser == null)
                //{
                //    objUser = new GF_CourseUsers();
                //    objUser.CourseID = objCourse.ID;
                //    objUser.UserID = userid;
                //    Db.GF_CourseUsers.Add(objUser);
                //}
                //else
                //{
                //    obj.UserID = userid;
                //}

                obj.IsClubHouse = true;
                obj.ClubHouseID = 0;
                obj.ModifyDate = DateTime.UtcNow;
                obj.ModifyBy = LoginInfo.UserId;

                Db.SaveChanges();

                #region Add Course Menu

                var lstCourseMenu = Db.GF_CourseMenu.FirstOrDefault(x => x.CourseID == LoginInfo.CourseId);

                if (lstCourseMenu == null)
                {
                    var lstCategory = Db.GF_Category.Where(x => x.Status == StatusType.Active);

                    foreach (var item in lstCategory)
                    {
                        GF_CourseMenu courseMenu = new GF_CourseMenu();
                        courseMenu.CategoryID = item.ID;
                        courseMenu.CourseID = LoginInfo.CourseId;
                        courseMenu.CreatedBy = LoginInfo.UserId.ToString();
                        courseMenu.CreatedDate = DateTime.Now;
                        courseMenu.Status = StatusType.Active;
                        courseMenu.IsActive = true;

                        Db.GF_CourseMenu.Add(courseMenu);
                    }
                    Db.SaveChanges();
                }

                #endregion

                #region Insert Tax Detail

                if (objCourse.ID > 0 && objCourse.CourseMenu != null)
                {
                    foreach (var item in objCourse.CourseMenu)
                    {
                        GF_CourseMenu courseMenu = new GF_CourseMenu();
                        courseMenu = Db.GF_CourseMenu.FirstOrDefault(x => x.CategoryID == item.CategoryID && x.CourseID == LoginInfo.CourseId);
                        courseMenu.CourseID = objCourse.ID;
                        courseMenu.CategoryID = item.CategoryID;
                        courseMenu.TaxPercentage = item.TaxPercentage;

                        Db.SaveChanges();
                    }
                }

                #endregion

                Message = "update";
            }
            else
            {
                return false;
            }

            return true;

        }

        public bool updateCourseSettingsbyCourseAdmin(GF_CourseInfo objCourse)
        {
            var obj = Db.GF_CourseInfo.FirstOrDefault(x => x.ID == objCourse.ID);
            if (obj != null)
            {
                //---hole white
                obj.Hole_1 = objCourse.Hole_1;
                obj.Hole_2 = objCourse.Hole_2;
                obj.Hole_3 = objCourse.Hole_3;
                obj.Hole_4 = objCourse.Hole_4;
                obj.Hole_5 = objCourse.Hole_5;
                obj.Hole_6 = objCourse.Hole_6;
                obj.Hole_7 = objCourse.Hole_7;
                obj.Hole_8 = objCourse.Hole_8;
                obj.Hole_9 = objCourse.Hole_9;
                obj.Hole_10 = objCourse.Hole_10;
                obj.Hole_11 = objCourse.Hole_11;
                obj.Hole_12 = objCourse.Hole_12;
                obj.Hole_13 = objCourse.Hole_13;
                obj.Hole_14 = objCourse.Hole_14;
                obj.Hole_15 = objCourse.Hole_15;
                obj.Hole_16 = objCourse.Hole_16;
                obj.Hole_17 = objCourse.Hole_17;
                obj.Hole_18 = objCourse.Hole_18;

                //----hole Red
                obj.Hole_1_Red = objCourse.Hole_1_Red;
                obj.Hole_2_Red = objCourse.Hole_2_Red;
                obj.Hole_3_Red = objCourse.Hole_3_Red;
                obj.Hole_4_Red = objCourse.Hole_4_Red;
                obj.Hole_5_Red = objCourse.Hole_5_Red;
                obj.Hole_6_Red = objCourse.Hole_6_Red;
                obj.Hole_7_Red = objCourse.Hole_7_Red;
                obj.Hole_8_Red = objCourse.Hole_8_Red;
                obj.Hole_9_Red = objCourse.Hole_9_Red;
                obj.Hole_10_Red = objCourse.Hole_10_Red;
                obj.Hole_11_Red = objCourse.Hole_11_Red;
                obj.Hole_12_Red = objCourse.Hole_12_Red;
                obj.Hole_13_Red = objCourse.Hole_13_Red;
                obj.Hole_14_Red = objCourse.Hole_14_Red;
                obj.Hole_15_Red = objCourse.Hole_15_Red;
                obj.Hole_16_Red = objCourse.Hole_16_Red;
                obj.Hole_17_Red = objCourse.Hole_17_Red;
                obj.Hole_18_Red = objCourse.Hole_18_Red;


                //-----Hole Blue
                obj.Hole_1_Blue = objCourse.Hole_1_Blue;
                obj.Hole_2_Blue = objCourse.Hole_2_Blue;
                obj.Hole_3_Blue = objCourse.Hole_3_Blue;
                obj.Hole_4_Blue = objCourse.Hole_4_Blue;
                obj.Hole_5_Blue = objCourse.Hole_5_Blue;
                obj.Hole_6_Blue = objCourse.Hole_6_Blue;
                obj.Hole_7_Blue = objCourse.Hole_7_Blue;
                obj.Hole_8_Blue = objCourse.Hole_8_Blue;
                obj.Hole_9_Blue = objCourse.Hole_9_Blue;
                obj.Hole_10_Blue = objCourse.Hole_10_Blue;
                obj.Hole_11_Blue = objCourse.Hole_11_Blue;
                obj.Hole_12_Blue = objCourse.Hole_12_Blue;
                obj.Hole_13_Blue = objCourse.Hole_13_Blue;
                obj.Hole_14_Blue = objCourse.Hole_14_Blue;
                obj.Hole_15_Blue = objCourse.Hole_15_Blue;
                obj.Hole_16_Blue = objCourse.Hole_16_Blue;
                obj.Hole_17_Blue = objCourse.Hole_17_Blue;
                obj.Hole_18_Blue = objCourse.Hole_18_Blue;


                obj.Hdcp_1 = objCourse.Hdcp_1;
                obj.Hdcp_2 = objCourse.Hdcp_2;
                obj.Hdcp_3 = objCourse.Hdcp_3;
                obj.Hdcp_4 = objCourse.Hdcp_4;
                obj.Hdcp_5 = objCourse.Hdcp_5;
                obj.Hdcp_6 = objCourse.Hdcp_6;
                obj.Hdcp_7 = objCourse.Hdcp_7;
                obj.Hdcp_8 = objCourse.Hdcp_8;
                obj.Hdcp_9 = objCourse.Hdcp_9;
                obj.Hdcp_10 = objCourse.Hdcp_10;
                obj.Hdcp_11 = objCourse.Hdcp_11;
                obj.Hdcp_12 = objCourse.Hdcp_12;
                obj.Hdcp_13 = objCourse.Hdcp_13;
                obj.Hdcp_14 = objCourse.Hdcp_14;
                obj.Hdcp_15 = objCourse.Hdcp_15;
                obj.Hdcp_16 = objCourse.Hdcp_16;
                obj.Hdcp_17 = objCourse.Hdcp_17;
                obj.Hdcp_18 = objCourse.Hdcp_18;

                obj.Par_1 = objCourse.Par_1;
                obj.Par_2 = objCourse.Par_2;
                obj.Par_3 = objCourse.Par_3;
                obj.Par_4 = objCourse.Par_4;
                obj.Par_5 = objCourse.Par_5;
                obj.Par_6 = objCourse.Par_6;
                obj.Par_7 = objCourse.Par_7;
                obj.Par_8 = objCourse.Par_8;
                obj.Par_9 = objCourse.Par_9;
                obj.Par_10 = objCourse.Par_10;
                obj.Par_11 = objCourse.Par_11;
                obj.Par_12 = objCourse.Par_12;
                obj.Par_13 = objCourse.Par_13;
                obj.Par_14 = objCourse.Par_14;
                obj.Par_15 = objCourse.Par_15;
                obj.Par_16 = objCourse.Par_16;
                obj.Par_17 = objCourse.Par_17;
                obj.Par_18 = objCourse.Par_18;

                obj.ModifyDate = DateTime.UtcNow;
                obj.ModifyBy = LoginInfo.UserId;

                Db.SaveChanges();

                Message = "update";
            }
            else
            {
                return false;
            }
            return true;
        }

        public bool AddEditCourse(long id, long clubHouseID, string courseName, string hole, bool status,
            ref string message)
        {
            try
            {
                #region Duplicate Check

                var duplicate = Db.GF_CourseInfo.FirstOrDefault(x => x.ClubHouseID == clubHouseID &&
                    x.COURSE_NAME.ToLower() == courseName.ToLower() &&
                    x.ID != id &&
                    x.Status == StatusType.Active);

                if (duplicate != null)
                {
                    message = "Course name already exists.";
                    return false;
                }

                #endregion

                GF_CourseInfo objCourseInfo = new GF_CourseInfo();
                var courseInfo = Db.GF_CourseInfo.FirstOrDefault(x => x.ID == clubHouseID);

                if (id > 0) //Update Case
                {
                    courseInfo = new GF_CourseInfo();
                    courseInfo = Db.GF_CourseInfo.FirstOrDefault(x => x.ID == id);
                }

                objCourseInfo = courseInfo;

                objCourseInfo.COURSE_NAME = courseName;
                objCourseInfo.NUMBEROFHOLES = hole;
                objCourseInfo.Status = status ? StatusType.Active : StatusType.InActive;
                objCourseInfo.IsClubHouse = false;
                objCourseInfo.ClubHouseID = clubHouseID;

                if (id <= 0)
                {
                    #region Score Card

                    //---hole white
                    objCourseInfo.Hole_1 = 0;
                    objCourseInfo.Hole_2 = 0;
                    objCourseInfo.Hole_3 = 0;
                    objCourseInfo.Hole_4 = 0;
                    objCourseInfo.Hole_5 = 0;
                    objCourseInfo.Hole_6 = 0;
                    objCourseInfo.Hole_7 = 0;
                    objCourseInfo.Hole_8 = 0;
                    objCourseInfo.Hole_9 = 0;
                    objCourseInfo.Hole_10 = 0;
                    objCourseInfo.Hole_11 = 0;
                    objCourseInfo.Hole_12 = 0;
                    objCourseInfo.Hole_13 = 0;
                    objCourseInfo.Hole_14 = 0;
                    objCourseInfo.Hole_15 = 0;
                    objCourseInfo.Hole_16 = 0;
                    objCourseInfo.Hole_17 = 0;
                    objCourseInfo.Hole_18 = 0;

                    //----hole Red
                    objCourseInfo.Hole_1_Red = 0;
                    objCourseInfo.Hole_2_Red = 0;
                    objCourseInfo.Hole_3_Red = 0;
                    objCourseInfo.Hole_4_Red = 0;
                    objCourseInfo.Hole_5_Red = 0;
                    objCourseInfo.Hole_6_Red = 0;
                    objCourseInfo.Hole_7_Red = 0;
                    objCourseInfo.Hole_8_Red = 0;
                    objCourseInfo.Hole_9_Red = 0;
                    objCourseInfo.Hole_10_Red = 0;
                    objCourseInfo.Hole_11_Red = 0;
                    objCourseInfo.Hole_12_Red = 0;
                    objCourseInfo.Hole_13_Red = 0;
                    objCourseInfo.Hole_14_Red = 0;
                    objCourseInfo.Hole_15_Red = 0;
                    objCourseInfo.Hole_16_Red = 0;
                    objCourseInfo.Hole_17_Red = 0;
                    objCourseInfo.Hole_18_Red = 0;


                    //-----Hole Blue
                    objCourseInfo.Hole_1_Blue = 0;
                    objCourseInfo.Hole_2_Blue = 0;
                    objCourseInfo.Hole_3_Blue = 0;
                    objCourseInfo.Hole_4_Blue = 0;
                    objCourseInfo.Hole_5_Blue = 0;
                    objCourseInfo.Hole_6_Blue = 0;
                    objCourseInfo.Hole_7_Blue = 0;
                    objCourseInfo.Hole_8_Blue = 0;
                    objCourseInfo.Hole_9_Blue = 0;
                    objCourseInfo.Hole_10_Blue = 0;
                    objCourseInfo.Hole_11_Blue = 0;
                    objCourseInfo.Hole_12_Blue = 0;
                    objCourseInfo.Hole_13_Blue = 0;
                    objCourseInfo.Hole_14_Blue = 0;
                    objCourseInfo.Hole_15_Blue = 0;
                    objCourseInfo.Hole_16_Blue = 0;
                    objCourseInfo.Hole_17_Blue = 0;
                    objCourseInfo.Hole_18_Blue = 0;


                    objCourseInfo.Hdcp_1 = 0;
                    objCourseInfo.Hdcp_2 = 0;
                    objCourseInfo.Hdcp_3 = 0;
                    objCourseInfo.Hdcp_4 = 0;
                    objCourseInfo.Hdcp_5 = 0;
                    objCourseInfo.Hdcp_6 = 0;
                    objCourseInfo.Hdcp_7 = 0;
                    objCourseInfo.Hdcp_8 = 0;
                    objCourseInfo.Hdcp_9 = 0;
                    objCourseInfo.Hdcp_10 = 0;
                    objCourseInfo.Hdcp_11 = 0;
                    objCourseInfo.Hdcp_12 = 0;
                    objCourseInfo.Hdcp_13 = 0;
                    objCourseInfo.Hdcp_14 = 0;
                    objCourseInfo.Hdcp_15 = 0;
                    objCourseInfo.Hdcp_16 = 0;
                    objCourseInfo.Hdcp_17 = 0;
                    objCourseInfo.Hdcp_18 = 0;

                    objCourseInfo.Par_1 = 0;
                    objCourseInfo.Par_2 = 0;
                    objCourseInfo.Par_3 = 0;
                    objCourseInfo.Par_4 = 0;
                    objCourseInfo.Par_5 = 0;
                    objCourseInfo.Par_6 = 0;
                    objCourseInfo.Par_7 = 0;
                    objCourseInfo.Par_8 = 0;
                    objCourseInfo.Par_9 = 0;
                    objCourseInfo.Par_10 = 0;
                    objCourseInfo.Par_11 = 0;
                    objCourseInfo.Par_12 = 0;
                    objCourseInfo.Par_13 = 0;
                    objCourseInfo.Par_14 = 0;
                    objCourseInfo.Par_15 = 0;
                    objCourseInfo.Par_16 = 0;
                    objCourseInfo.Par_17 = 0;
                    objCourseInfo.Par_18 = 0;

                    #endregion

                    Db.GF_CourseInfo.Add(objCourseInfo);
                    message = "Course details submitted successfully.";
                }
                else
                {
                    message = "Course details updated successfully.";
                }

                Db.SaveChanges();

                return true;
            }
            catch (Exception e)
            {
                message = "Technical Error: Please try again.";
                return false;
            }
        }

        //public bool updateCourseHOLEbyCourseAdmin(GF_CourseInfo objCourse)
        //{
        //    var obj = Db.GF_CourseInfo.FirstOrDefault(x => x.ID == objCourse.ID);
        //    if (obj != null)
        //    {
        //        obj.Hole_1 = objCourse.Hole_1;
        //        obj.Hole_2 = objCourse.Hole_2;
        //        obj.Hole_3 = objCourse.Hole_3;
        //        obj.Hole_4 = objCourse.Hole_4;
        //        obj.Hole_5 = objCourse.Hole_5;
        //        obj.Hole_6 = objCourse.Hole_6;
        //        obj.Hole_7 = objCourse.Hole_7;
        //        obj.Hole_8 = objCourse.Hole_8;
        //        obj.Hole_9 = objCourse.Hole_9;
        //        obj.Hole_10 = objCourse.Hole_10;
        //        obj.Hole_11 = objCourse.Hole_11;
        //        obj.Hole_12 = objCourse.Hole_12;
        //        obj.Hole_13 = objCourse.Hole_13;
        //        obj.Hole_14 = objCourse.Hole_14;
        //        obj.Hole_15 = objCourse.Hole_15;
        //        obj.Hole_16 = objCourse.Hole_16;
        //        obj.Hole_17 = objCourse.Hole_17;
        //        obj.Hole_18 = objCourse.Hole_18;

        //        Db.SaveChanges();

        //        Message = "update";
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //    return true;
        //}
        //public bool updateCourseHANDICAPEDbyCourseAdmin(GF_CourseInfo objCourse)
        //{
        //    var obj = Db.GF_CourseInfo.FirstOrDefault(x => x.ID == objCourse.ID);
        //    if (obj != null)
        //    {
        //        obj.Hdcp_1 = objCourse.Hdcp_1;
        //        obj.Hdcp_2 = objCourse.Hdcp_2;
        //        obj.Hdcp_3 = objCourse.Hdcp_3;
        //        obj.Hdcp_4 = objCourse.Hdcp_4;
        //        obj.Hdcp_5 = objCourse.Hdcp_5;
        //        obj.Hdcp_6 = objCourse.Hdcp_6;
        //        obj.Hdcp_7 = objCourse.Hdcp_7;
        //        obj.Hdcp_8 = objCourse.Hdcp_8;
        //        obj.Hdcp_9 = objCourse.Hdcp_9;
        //        obj.Hdcp_10 = objCourse.Hdcp_10;
        //        obj.Hdcp_11 = objCourse.Hdcp_11;
        //        obj.Hdcp_12 = objCourse.Hdcp_12;
        //        obj.Hdcp_13 = objCourse.Hdcp_13;
        //        obj.Hdcp_14 = objCourse.Hdcp_14;
        //        obj.Hdcp_15 = objCourse.Hdcp_15;
        //        obj.Hdcp_16 = objCourse.Hdcp_16;
        //        obj.Hdcp_17 = objCourse.Hdcp_17;

        //        Db.SaveChanges();

        //        Message = "update";
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //    return true;
        //}
        //public bool updateCoursePARbyCourseAdmin(GF_CourseInfo objCourse)
        //{
        //    var obj = Db.GF_CourseInfo.FirstOrDefault(x => x.ID == objCourse.ID);
        //    if (obj != null)
        //    {
        //        obj.Par_1 = objCourse.Par_1;
        //        obj.Par_2 = objCourse.Par_2;
        //        obj.Par_3 = objCourse.Par_3;
        //        obj.Par_4 = objCourse.Par_4;
        //        obj.Par_5 = objCourse.Par_5;
        //        obj.Par_6 = objCourse.Par_6;
        //        obj.Par_7 = objCourse.Par_7;
        //        obj.Par_8 = objCourse.Par_8;
        //        obj.Par_9 = objCourse.Par_9;
        //        obj.Par_10 = objCourse.Par_10;
        //        obj.Par_11 = objCourse.Par_11;
        //        obj.Par_12 = objCourse.Par_12;
        //        obj.Par_13 = objCourse.Par_13;
        //        obj.Par_14 = objCourse.Par_14;
        //        obj.Par_15 = objCourse.Par_15;
        //        obj.Par_16 = objCourse.Par_16;
        //        obj.Par_17 = objCourse.Par_17;
        //        obj.Par_18 = objCourse.Par_18;

        //        Db.SaveChanges();

        //        Message = "update";
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //    return true;
        //}

        /// <summary>
        /// Created By:Arun
        /// Created Date:26 March 2015
        /// Purpose: Chnage Status
        /// </summary>
        /// <param name="id"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        internal bool ChangeStatus(long id, string status)
        {
            GF_RoleModules objModuleRole = new GF_RoleModules();


            var objCourse = Db.GF_CourseInfo.FirstOrDefault(x => x.ID == id);
            if (LoginInfo.IsSuper || LoginInfo.LoginUserType == UserType.CourseAdmin)
                objModuleRole = CommonFunctions.GetAccessModule(ModuleValues.AllRights);
            ////////else
            ////////    objModuleRole = CommonFunctions.GetAccessModule(ModuleValues.Golfer);

            if (!objModuleRole.UpdateFlag)
            {
                Message = "Not Access";//Resources.Resources.unaccess;
                return false;
            }
            if (objCourse != null)
            {
                Message = status == StatusType.Active ? "Course info deactivated successfully" : "Course info activated successfully";
                objCourse.Status = status == StatusType.Active ? StatusType.InActive : StatusType.Active;
                objCourse.ModifyDate = DateTime.UtcNow;
                objCourse.ModifyBy = LoginInfo.UserId;
                Db.SaveChanges();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Created By:Arun
        /// Created Date:26 March 2015
        /// Purpose: Delete selected user(s).
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        internal bool DeletCourseInfo(long[] ids)
        {
            GF_RoleModules objModuleRole = new GF_RoleModules();

            if (LoginInfo.IsSuper || LoginInfo.LoginUserType == UserType.CourseAdmin)
                objModuleRole = CommonFunctions.GetAccessModule(ModuleValues.AllRights);
            ////////else
            ////////    objModuleRole = CommonFunctions.GetAccessModule(ModuleValues.Golfer);

            if (!objModuleRole.DeleteFlag)
            {
                Message = "Not Access";// Resources.Resources.unaccess;
                return false;
            }

            var objCourse = Db.GF_CourseInfo.Where(x => ids.AsQueryable().Contains(x.ID));
            foreach (var r in objCourse)
            {
                r.Status = StatusType.Delete;
                r.ModifyDate = DateTime.UtcNow;
                r.ModifyBy = LoginInfo.UserId;
            }
            Db.Configuration.ValidateOnSaveEnabled = false;
            Db.SaveChanges();
            return true;
        }

        /// <summary>
        /// Created By:veera
        /// Created date:7 april 2015
        /// purpose: Get course admin to a golfer chat
        /// </summary>
        /// <param name="golferId"></param>
        /// <returns></returns>

        public IQueryable<GF_Messages> GetMsgsfromGolfer(long golferId, long courseadminId, int pageIndex, int pageSize,
            ref int totalRecords, string MsgTo, bool Onlygolfer, string type, bool? isSupport)
        {

            try
            {
                //if (golferId == 0)
                //{
                //    var list = Db.GF_Messages.Where(x => x.MsgTo == courseadminId ||x. && x.IsMessagesFromGolfer == "1" && x.IsMessagesToGolfer == "0").ToList().Select(x =>
                //           new GF_Messages
                //           {
                //              ID= x.ID,
                //              MsgFrom= x.MsgFrom,
                //              MsgTo=x.MsgTo,
                //              CreatedDate= x.CreatedDate,
                //              Message= x.Message,
                //              Name = Db.GF_Golfer.Where(k => k.GF_ID == x.MsgFrom).FirstOrDefault().FirstName

                //           }).AsQueryable();
                //  list = list.ToList().Where(x => x.CreatedDate.Value.Date == DateTime.UtcNow.Date).AsQueryable();
                // return list;

                //  }
                // else
                //{
                if (!Onlygolfer)
                {
                    if (type == "0")
                    {
                        var list = Db.GF_Messages.Where(x => (x.MsgTo == courseadminId && x.MsgFrom == golferId && x.IsMessagesFromGolfer == "1" && x.IsMessagesToGolfer == "0")).ToList().Select(x =>
                                new GF_Messages
                                {
                                    ID = x.ID,
                                    MsgFrom = x.MsgFrom,
                                    MsgTo = x.MsgTo,
                                    CreatedDate = x.CreatedDate,
                                    MsgDate = x.CreatedDate.Value.ToLocalTime().ToString(),
                                    Message = x.Message,
                                    Name = Db.GF_Golfer.Where(k => k.GF_ID == x.MsgFrom).FirstOrDefault().FirstName + " " + Db.GF_Golfer.Where(k => k.GF_ID == x.MsgFrom).FirstOrDefault().LastName
                                }).AsQueryable();
                        if (list.Count() == 0)
                        {
                            list = Db.GF_Messages.Where(x => (x.MsgTo == courseadminId && x.MsgFrom == golferId && x.IsMessagesFromGolfer == "1" && x.IsMessagesToGolfer == "1")).ToList().Select(x =>
                                   new GF_Messages
                                   {
                                       ID = x.ID,
                                       MsgFrom = x.MsgFrom,
                                       MsgTo = x.MsgTo,
                                       CreatedDate = x.CreatedDate,
                                       MsgDate = x.CreatedDate.Value.ToLocalTime().ToString(),
                                       Message = x.Message,
                                       Name = Db.GF_Golfer.Where(k => k.GF_ID == x.MsgFrom).FirstOrDefault().FirstName + " " + Db.GF_Golfer.Where(k => k.GF_ID == x.MsgFrom).FirstOrDefault().LastName

                                   }).AsQueryable();
                        }
                        var listadmin = Db.GF_Messages.Where(x => (x.MsgFrom == courseadminId && x.MsgTo == golferId && x.IsMessagesFromGolfer == "0" && x.IsMessagesToGolfer == "1")).ToList().Select(x =>
                             new GF_Messages
                             {
                                 ID = x.ID,
                                 MsgFrom = x.MsgFrom,
                                 MsgTo = x.MsgTo,
                                 CreatedDate = x.CreatedDate,
                                 MsgDate = x.CreatedDate.Value.ToLocalTime().ToString(),
                                 Message = x.Message,
                                 Name = Db.GF_AdminUsers.Where(k => k.ID == x.MsgFrom).FirstOrDefault().FirstName + " " + Db.GF_AdminUsers.Where(k => k.ID == x.MsgFrom).FirstOrDefault().LastName

                             }).AsQueryable();

                        var listall = list.Union(listadmin);
                        totalRecords = listall.Count();
                        listall = listall.ToList().Skip((pageIndex - 1) * pageSize).Take(pageSize).AsQueryable();
                        listall = listall.OrderBy(x => x.CreatedDate);
                        return listall;
                    }
                    else
                    {
                        // for other Users
                        var list = Db.GF_Messages.Where(x => (x.MsgFrom == courseadminId && x.MsgTo == golferId && x.IsMessagesFromGolfer == "0" && x.IsMessagesToGolfer == "0")).ToList().Select(x =>
                                new GF_Messages
                                {
                                    ID = x.ID,
                                    MsgFrom = x.MsgFrom,
                                    MsgTo = x.MsgTo,

                                    CreatedDate = x.CreatedDate,
                                    MsgDate = x.CreatedDate.Value.ToLocalTime().ToString(),
                                    Message = x.Message,
                                    Name = Db.GF_AdminUsers.Where(k => k.ID == x.MsgFrom).FirstOrDefault().FirstName + " " + Db.GF_AdminUsers.Where(k => k.ID == x.MsgFrom).FirstOrDefault().LastName

                                }).AsQueryable();
                        var listadmin = Db.GF_Messages.Where(x => (x.MsgFrom == golferId && x.MsgTo == courseadminId && x.IsMessagesFromGolfer == "0" && x.IsMessagesToGolfer == "0")).ToList().Select(x =>
                            new GF_Messages
                            {
                                ID = x.ID,
                                MsgFrom = x.MsgFrom,
                                MsgTo = x.MsgTo,
                                CreatedDate = x.CreatedDate,
                                MsgDate = x.CreatedDate.Value.ToLocalTime().ToString(),
                                Message = x.Message,
                                Name = Db.GF_AdminUsers.Where(k => k.ID == x.MsgFrom).FirstOrDefault().FirstName + " " + Db.GF_AdminUsers.Where(k => k.ID == x.MsgFrom).FirstOrDefault().LastName

                            }).AsQueryable();

                        var listall = list.Union(listadmin);

                        totalRecords = listall.Count();
                        listall = listall.ToList().Skip((pageIndex - 1) * pageSize).Take(pageSize).AsQueryable();
                        listall = listall.OrderBy(x => x.CreatedDate);

                        return listall;
                    }
                }
                else
                {
                    if (isSupport ?? false)
                    {
                        var list = Db.GF_Messages.Where(x => (x.MsgTo == golferId && x.MsgFrom == courseadminId && x.IsMessagesFromGolfer == MsgTo && x.IsMessagesToGolfer == "0")).ToList().Select(x =>
                                    new GF_Messages
                                    {
                                        ID = x.ID,
                                        MsgFrom = x.MsgFrom,
                                        MsgTo = x.MsgTo,
                                        CreatedDate = x.CreatedDate,
                                        MsgDate = x.CreatedDate.Value.ToLocalTime().ToString(),
                                        Message = x.Message,
                                        Name = Db.GF_Golfer.Where(k => k.GF_ID == x.MsgFrom).FirstOrDefault().FirstName + " " + Db.GF_Golfer.Where(k => k.GF_ID == x.MsgFrom).FirstOrDefault().LastName

                                    }).AsQueryable();
                        var listIn = Db.GF_Messages.Where(x => (x.MsgFrom == golferId && x.MsgTo == courseadminId && x.IsMessagesFromGolfer == "0" && x.IsMessagesToGolfer == MsgTo)).ToList().Select(x =>
                                  new GF_Messages
                                  {
                                      ID = x.ID,
                                      MsgFrom = x.MsgFrom,
                                      MsgTo = x.MsgTo,
                                      CreatedDate = x.CreatedDate,
                                      MsgDate = x.CreatedDate.Value.ToLocalTime().ToString(),
                                      Message = x.Message,
                                      Name = Db.GF_AdminUsers.Where(k => k.ID == golferId).FirstOrDefault().FirstName + " " + Db.GF_AdminUsers.Where(k => k.ID == golferId).FirstOrDefault().LastName

                                  }).AsQueryable();
                        var listall = list.Union(listIn);
                        totalRecords = listall.Count();
                        listall = listall.ToList().Skip((pageIndex - 1) * pageSize).Take(pageSize).AsQueryable();
                        listall = listall.OrderBy(x => x.CreatedDate);
                        return listall;
                    }
                    else
                    {
                        var list = Db.GF_Messages.Where(x => (x.MsgTo == courseadminId && x.MsgFrom == golferId && x.IsMessagesFromGolfer == "1" && x.IsMessagesToGolfer == MsgTo)).ToList().Select(x =>
                                    new GF_Messages
                                    {
                                        ID = x.ID,
                                        MsgFrom = x.MsgFrom,
                                        MsgTo = x.MsgTo,
                                        CreatedDate = x.CreatedDate,
                                        MsgDate = x.CreatedDate.Value.ToLocalTime().ToString(),
                                        Message = x.Message,
                                        Name = Db.GF_Golfer.Where(k => k.GF_ID == x.MsgFrom).FirstOrDefault().FirstName + " " + Db.GF_Golfer.Where(k => k.GF_ID == x.MsgFrom).FirstOrDefault().LastName

                                    }).AsQueryable();
                        var listIn = Db.GF_Messages.Where(x => (x.MsgFrom == courseadminId && x.MsgTo == golferId && x.IsMessagesFromGolfer == "1" && x.IsMessagesToGolfer == MsgTo)).ToList().Select(x =>
                                  new GF_Messages
                                  {
                                      ID = x.ID,
                                      MsgFrom = x.MsgFrom,
                                      MsgTo = x.MsgTo,
                                      CreatedDate = x.CreatedDate,
                                      MsgDate = x.CreatedDate.Value.ToLocalTime().ToString(),
                                      Message = x.Message,
                                      Name = Db.GF_Golfer.Where(k => k.GF_ID == x.MsgFrom).FirstOrDefault().FirstName + " " + Db.GF_Golfer.Where(k => k.GF_ID == x.MsgFrom).FirstOrDefault().LastName

                                  }).AsQueryable();
                        var listall = list.Union(listIn);
                        totalRecords = listall.Count();
                        listall = listall.ToList().Skip((pageIndex - 1) * pageSize).Take(pageSize).AsQueryable();
                        listall = listall.OrderBy(x => x.CreatedDate);
                        return listall;
                    }
                }
                //   }



            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Created By:Arun
        /// purpose: get all Course Info Details
        /// </summary>
        /// <returns></returns>
        public List<GF_CourseInfo> GetAllCourses()
        {
            GolflerEntities _db = new GolflerEntities();
            return _db.GF_CourseInfo.ToList();
        }

        public List<GF_CourseInfo> GetAllActiveCourses()
        {
            GolflerEntities _db = new GolflerEntities();
            return _db.GF_CourseInfo.Where(x => x.Status == StatusType.Active).ToList();
        }

        /// <summary>
        /// Created By: Ramesh Kalra
        /// purpose: get all Active Partner Courses 
        /// </summary>
        /// <returns></returns>
        public List<GF_CourseInfo> GetAllActivePartnerCourses()
        {
            GolflerEntities _db = new GolflerEntities();
            return _db.GF_CourseInfo
                        .Where(x => x.Status == StatusType.Active && x.PartnershipStatus == PartershipStatus.Partner)
                            .OrderBy(x=>x.COURSE_NAME).ToList();
        }

        /// <summary>
        /// Created By: Amit Kumar
        /// purpose: get course according to condition
        /// </summary>
        /// <returns></returns>
        public List<GF_CourseInfo> GetCourses(string type, string partnerShip, long? golferID)
        {
            Db = new GolflerEntities();
            var lstCourse = new List<GF_CourseInfo>();

            if (type == "all" && string.IsNullOrEmpty(partnerShip) && (golferID ?? 0) <= 0)
            {
                lstCourse = Db.GF_CourseInfo.Where(x => x.Status == StatusType.Active).ToList();
            }
            else if ((type == "all" || type == "") && !string.IsNullOrEmpty(partnerShip) && (golferID ?? 0) <= 0)
            {
                lstCourse = Db.GF_CourseInfo.Where(x => x.PartnershipStatus == partnerShip &&
                    x.Status == StatusType.Active).ToList();
            }
            else if ((type == "all" || type == "") && !string.IsNullOrEmpty(partnerShip) && (golferID ?? 0) > 0)
            {
                lstCourse = (from x in Db.GF_CourseInfo
                             join y in Db.GF_CourseVisitLog on x.ID equals y.CourseID
                             where y.GolferID == golferID &&
                             x.PartnershipStatus == partnerShip
                             select x).ToList();
            }
            else
            {
                lstCourse = Db.GF_CourseInfo.ToList();
            }

            return lstCourse;
        }

        /// <summary>
        /// Created By:veera
        /// purpose: get countries 
        /// </summary>
        /// <returns></returns> 
        public List<OrderAnalyticsListResult> GetAllActiveCountries()
        {
            GolflerEntities _db = new GolflerEntities();
            var lstCourse = (from x in Db.GF_CourseInfo
                             where x.Status == StatusType.Active && x.COUNTY != null
                             select new OrderAnalyticsListResult
                             {
                                 ID = x.COUNTY,
                                 Name = x.COUNTY
                             }).Distinct().ToList();

            return lstCourse;
        }

        /// <summary>
        /// Get Countries list
        /// </summary>
        /// <returns></returns>
        public List<GF_COUNTRY> GetAllActiveCountiesCourse()
        {
            GolflerEntities _db = new GolflerEntities();
            var lstCounty = _db.GF_COUNTRY.Where(x => x.IS_ACTIVE == true).ToList();

            return lstCounty;
        }

        /// <summary>
        /// Created By:KIRAN
        /// purpose: get COUNTIES 
        /// </summary>
        /// <returns></returns> 
        public List<GF_COUNTY> GetAllActiveCounties()
        {
            GolflerEntities _db = new GolflerEntities();
            var lstCounty = _db.GF_COUNTY.Where(x => x.IS_ACTIVE == true).OrderBy(x => x.COUNTY_NAME).ToList();

            return lstCounty;
        }

        /// <summary>
        /// Created By:veera
        /// purpose: get FoodItems 
        /// </summary>
        /// <returns></returns>
        public List<GF_Category> GetAllActiveFoodItems()
        {
            GolflerEntities _db = new GolflerEntities();
            return _db.GF_Category.Where(x => x.IsActive == true).OrderBy(x => x.DisplayOrder).ToList();
        }

        /// <summary>
        /// Created By:veera
        /// purpose: get countries 
        /// </summary>
        /// <returns></returns>
        public List<OrderAnalyticsListResult> GetAllActiveOrderCourses(string golferid)
        {
            GolflerEntities _db = new GolflerEntities();
            long _golferId = Convert.ToInt64(golferid);
            var values = _db.GF_Order.Where(x => x.GolferID == _golferId && x.IsDelivered == true).Select(x => x.CourseID).Distinct().ToList();
            string orderCourses = string.Join(",", values.Select(v => v.ToString()).ToArray());
            long[] courseIDs = CommonFunctions.ConvertStringArrayToLongArray(orderCourses);
            //var lstCourse = (from x in Db.GF_CourseInfo

            //                 where x.Status == StatusType.Active && SqlFunctions.StringConvert((double)x.ID).Contains(orderCourses)
            //                 select new OrderAnalyticsListResult
            //                 {
            //                     CourseID = x.ID,
            //                     Name = x.COURSE_NAME
            //                 }).ToList();
            var lstCourse = _db.GF_CourseInfo.ToList().Where(x => courseIDs.Contains(x.ID)).Select(x => new OrderAnalyticsListResult
            {
                CourseID = x.ID,
                Name = x.COURSE_NAME
            }).ToList();
            return lstCourse;
        }

        //public OrderAnalyticsSearchResult OrderAnalyticsSearch(long golferid, string type, string course, string foodItem,string subfoodCategory)
        //{
        //    GolflerEntities _db = new GolflerEntities();
        //  //  long _golferId = Convert.ToInt64(golferid);
        //    long _courseId = Convert.ToInt64(course);
        //    long _menuItemId = Convert.ToInt64(foodItem);
        //    OrderAnalyticsSearchResult objResult = new OrderAnalyticsSearchResult();
        //    //if (subfoodCategory != "")
        //    //{
        //    //    long _subCategoryId = Convert.ToInt64(subfoodCategory);

        //    //    var values = _db.GF_OrderDetails.Where(x => x.GF_MenuItems.GF_SubCategory.GF_Category.ID == _menuItemId 
        //    //        && x.GF_MenuItems.GF_SubCategory.ID==_subCategoryId).Select(x => x.OrderID).Distinct().ToList();
        //    //    string orderDetails = string.Join(",", values.Select(v => v.ToString()).ToArray());
        //    //    long[] orderIDs = CommonFunctions.ConvertStringArrayToLongArray(orderDetails);
        //    //    var avgPricePerOrderList = _db.GF_Order.ToList().Where(x => orderIDs.Contains(x.ID) && x.GolferID == golferid && x.CourseID == _courseId && x.IsDelivered == true).Select(x => x.GrandTotal ?? 0).ToList();
        //    //    objResult.AvgPricePerOrder = avgPricePerOrderList.DefaultIfEmpty(0).Average();
        //    //}
        //    //else
        //    //{


        //        //var values = _db.GF_OrderDetails.Where(x => x.GF_MenuItems.GF_SubCategory.GF_Category.ID == _menuItemId).Select(x => x.OrderID).Distinct().ToList();
        //        //string orderDetails = string.Join(",", values.Select(v => v.ToString()).ToArray());
        //        //long[] orderIDs = CommonFunctions.ConvertStringArrayToLongArray(orderDetails);


        //        //var avgPricePerOrderList = _db.GF_Order.ToList().Where(x => orderIDs.Contains(x.ID) && x.GolferID == golferid && x.CourseID == _courseId && x.IsDelivered == true).Select(x => x.GrandTotal ?? 0).ToList();

        //        //objResult.AvgPricePerOrder = avgPricePerOrderList.DefaultIfEmpty(0).Average();
        //  //  }
        //    //if (type == "1")
        //    //{ 

        //    //}
        //    //var valuesFoodItems = _db.GF_Order.Where(x => x.CourseID == _courseId && x.GolferID == golferid && x.IsDelivered == true).Select(x => x.ID).Distinct().ToList();
        //    //string foodItemsDetails = string.Join(",", valuesFoodItems.Select(v => v.ToString()).ToArray());
        //    //long[] orderFoodItemIDs = CommonFunctions.ConvertStringArrayToLongArray(foodItemsDetails);
        //    //var avgPricePerFoodItemList = _db.GF_OrderDetails.Where(x => orderFoodItemIDs.Contains(x.OrderID ?? 0) && x.GF_MenuItems.GF_SubCategory.GF_Category.ID == _menuItemId).Select(x => new { total=(x.UnitPrice ?? 0 * x.Quantity ?? 0),x.OrderID}).GroupBy(x => x.OrderID).ToList();

        //    //objResult.AvgPricePerFoodItem = Math.Truncate(100 * (avgPricePerFoodItemList.Select(item => item.DefaultIfEmpty(0).Average())) / 100;


        //    return objResult;
        //}
        public OrderAnalyticsSearchResult OrderAnalyticsSearch(long golferid, string type, string course, string foodItem, string subfoodCategory, string miles)
        {
            GolflerEntities _db = new GolflerEntities();
            long _subfoodCategory = 0;
            if (subfoodCategory == "")
            { }
            else
            {
                _subfoodCategory = Convert.ToInt64(subfoodCategory);
            }
            long _courseId = Convert.ToInt64(course);
            long _menuItemId = Convert.ToInt64(foodItem);
            OrderAnalyticsSearchResult objResult = new OrderAnalyticsSearchResult();
            var lstOrders = new List<GF_SP_GetOrderAnalyticsSearch_Result>();
            lstOrders = _db.GF_SP_GetOrderAnalyticsSearch(golferid, type, _courseId, _menuItemId, _subfoodCategory, miles).ToList();
            if (lstOrders.Count > 0)
            {
                objResult.AvgPricePerOrder = Convert.ToDecimal(lstOrders[0].AvgPricePerOrder);
                objResult.AvgPricePerAllCourses = Convert.ToDecimal(lstOrders[0].AvgPricePerAllCourses);
                objResult.AvgRatingPerOrder = Convert.ToDecimal(lstOrders[0].AvgRatingPerOrder);
                objResult.AvgRatingPerAllCourses = Convert.ToDecimal(lstOrders[0].AvgRatingPerAllCourses);
            }
            var lstMenu = new List<GF_SP_GetOrderAnalyticsMenuSearch_Result>();
            lstMenu = _db.GF_SP_GetOrderAnalyticsMenuSearch(golferid, type, _courseId, _menuItemId, _subfoodCategory, miles).ToList();

            if (lstMenu.Count > 0)
            {
                List<OrderAnalyticsMenuSearchResult> lstMenuResult = new List<OrderAnalyticsMenuSearchResult>();
                foreach (var o in lstMenu)
                {
                    OrderAnalyticsMenuSearchResult resultMenu = new OrderAnalyticsMenuSearchResult();
                    resultMenu.CourseId = Convert.ToInt64(resultMenu.CourseId);
                    resultMenu.Name = o.Name;
                    resultMenu.AvgPrice = o.AvgPrice;
                    resultMenu.CategoryName = o.CategoryName;
                    resultMenu.CategoryId = o.CategoryID;
                    resultMenu.SubCategoryName = o.SubCategoryName;
                    resultMenu.SubCategoryId = o.SubCategoryID;

                    lstMenuResult.Add(resultMenu);

                }
                objResult.MenuSearch = lstMenuResult;


            }
            var lstGraph = new List<GF_SP_GetOrderAnalyticsGraphSearch_Result>();
            lstGraph = _db.GF_SP_GetOrderAnalyticsGraphSearch(golferid, type, _courseId, _menuItemId, _subfoodCategory, miles).ToList();

            if (lstGraph.Count > 0)
            {

                List<OrderAnalyticsMenuSearchResult> lstGraphResult = new List<OrderAnalyticsMenuSearchResult>();
                List<OrderAnalyticsGraphSearchResult> lstGraphCourseResult = new List<OrderAnalyticsGraphSearchResult>();
                foreach (var o in lstGraph)
                {
                    OrderAnalyticsMenuSearchResult resultGraph = new OrderAnalyticsMenuSearchResult();
                    OrderAnalyticsGraphSearchResult resultGraphCourse = new OrderAnalyticsGraphSearchResult();
                    resultGraph.CourseName = o.CourseName;
                    resultGraphCourse.CourseName = o.CourseName;
                    resultGraph.Name = o.Name;
                    resultGraph.AvgPrice = o.AvgPrice;
                    resultGraph.CategoryName = o.CategoryName;
                    resultGraph.CategoryId = o.CategoryID;
                    resultGraph.SubCategoryName = o.SubCategoryName;
                    resultGraph.SubCategoryId = o.SubCategoryID;
                    string courseName = "";
                    string CombineValuesName = "";
                    foreach (var item in lstGraph)
                    {
                        if (!(courseName.Contains(item.CourseName)))
                        {
                            string price = "";
                            try
                            {
                                price = Convert.ToString(lstGraph.FirstOrDefault(x => x.Name == o.Name && x.CourseName == item.CourseName).AvgPrice);
                            }
                            catch
                            {
                                price = "0.00";
                            }
                            if (CombineValuesName == "")
                            {
                                CombineValuesName = o.Name + "," + price;

                            }
                            else
                            {
                                CombineValuesName = CombineValuesName + "," + price;
                            }
                        }
                        if (courseName == "")
                        {
                            courseName = item.CourseName;
                        }
                        else
                        {
                            courseName = courseName + "," + item.CourseName;
                        }
                    }
                    resultGraph.CombineName = CombineValuesName;
                    if (_menuItemId > 0)
                    {
                        if (_subfoodCategory > 0)
                        {
                            if (_menuItemId == resultGraph.CategoryId && _subfoodCategory == resultGraph.SubCategoryId)
                            {
                                lstGraphResult.Add(resultGraph);
                            }
                        }
                        else
                        {
                            if (_menuItemId == resultGraph.CategoryId)
                            {
                                lstGraphResult.Add(resultGraph);
                            }
                        }
                    }
                    lstGraphCourseResult.Add(resultGraphCourse);
                }
                objResult.GraphSearch = lstGraphResult.OrderBy(x => x.AvgPrice).ToList();
                objResult.GraphCourseSearch = lstGraphCourseResult;
            }
            return objResult;
        }


        public EmployeeAnalyticsSearchResult EmployeeAnalyticsSearch(long courseid, string type, string name, string fromdate, string todate)
        {
            GolflerEntities _db = new GolflerEntities();

            EmployeeAnalyticsSearchResult objResult = new EmployeeAnalyticsSearchResult();
            long? userId = 0;
            string titleType = "";
            string innerTitle = "";
            int searchtype = 0;
            try
            {
                userId = _db.GF_AdminUsers.FirstOrDefault(x => x.UserName.ToLower() == name.ToLower() && x.Type == type && x.CourseId == courseid && x.Status == "A").ID;
                objResult.IsUserExists = true;
                objResult.eUID = CommonFunctions.EncryptUrlParam(userId ?? 0);
            }
            catch
            {
                userId = 0;
                objResult.IsUserExists = false;
                objResult.eUID = CommonFunctions.EncryptUrlParam(userId ?? 0);
            }
            if (userId > 0)
            {
                var lstEmployee = _db.GF_SP_GetEmployeeAnalyticsGraphSearch(userId, type, courseid).ToList();

                if (lstEmployee.Count > 0)
                {

                    List<EmployeeAnalyticsDataSearchResult> lstResult = new List<EmployeeAnalyticsDataSearchResult>();

                    foreach (var o in lstEmployee)
                    {
                        EmployeeAnalyticsDataSearchResult result = new EmployeeAnalyticsDataSearchResult();

                        result.PriceMe = o.PriceMe;
                        result.PriceCourse = o.PriceCourse;
                        result.NoOfOrdersMe = o.NoOfOrdersMe;
                        result.NoOfOrdersCourse = o.NoOfOrdersCourse;
                        result.RatingMe = o.RatingMe;
                        result.RatingCourse = o.RatingCourse;
                        lstResult.Add(result);
                    }
                    objResult.EmployeeSearch = lstResult;

                }
                DateTime stDate = DateTime.Parse(fromdate);
                DateTime edDate = DateTime.Parse(todate);
                var days = (edDate - stDate).TotalDays;
                if (days == 0.0)
                {
                    // today n yesterday
                    searchtype = 0;
                    if (stDate == DateTime.Now.Date)
                    {
                        titleType = "Today";
                    }
                    else
                    {
                        titleType = "Yesterday";
                    }
                }

                else if (days == 6.0 || days < 7)
                {
                    searchtype = 1;
                    titleType = "Weekly";

                }
                else if (stDate.Year != edDate.Year)
                {
                    if (days == 24253.0)
                    {
                        searchtype = 5;
                        titleType = "All Time";
                    }
                    else
                    {
                        //within multiple year
                        searchtype = 8;
                        titleType = "Within a Decade";
                    }
                }
                else if (days == 30.0 || days == 31.0 || (days == 28.0 && (stDate.Month == 2)) || (days == 29.0))
                {
                    if (stDate.Month != edDate.Month)
                    {
                        //within year
                        searchtype = 6;
                        titleType = "Within a Year";
                    }
                    else
                    {
                        searchtype = 3;
                        titleType = "Monthly";
                    }
                }
                else if (days > 7 && (days <= 30 || days <= 29 || days <= 31))
                {
                    if (stDate.Month != edDate.Month)
                    {
                        //within year
                        searchtype = 6;
                        titleType = "Within a Year";
                    }
                    else
                    {
                        searchtype = 4;
                        titleType = "Within a Month";
                    }
                }

                else if (days == 364.0 || days == 365.0)
                {
                    //within a year
                    if (stDate.Year == edDate.Year)
                    {
                        searchtype = 6;
                        titleType = "Yearly";
                    }
                }
                else if (days == 24253.0)
                {
                    searchtype = 5;
                    titleType = "All Time";
                }
                else if (stDate.Month != edDate.Month)
                {
                    //within year
                    searchtype = 6;
                    titleType = "Within a Year";
                }
                else
                {
                    searchtype = 6;
                }
                var lstEmployeePersonal = _db.GF_SP_GetEmployeePersonalAnalyticsGraphSearch(userId, type, courseid, stDate, edDate, searchtype).ToList();

                if (lstEmployeePersonal.Count > 0)
                {

                    List<EmployeeAnalyticsDataSearchResult> lstResultPersonal = new List<EmployeeAnalyticsDataSearchResult>();
                    int tempcounter = 0;

                    foreach (var o in lstEmployeePersonal)
                    {
                        EmployeeAnalyticsDataSearchResult result = new EmployeeAnalyticsDataSearchResult();

                        result.PriceMe = o.PriceMe;
                        result.NoOfOrdersMe = o.NoOfOrdersMe;
                        result.RatingMe = o.RatingMe;
                        if (searchtype == 4)
                        {
                            if (tempcounter == 0)
                            {
                                result.InnerTitle = Convert.ToString(stDate).Split(' ')[0] + "-" + o.InnerTitle.Split('-')[1];
                            }
                            else if (tempcounter == lstEmployeePersonal.Count - 1)
                            {
                                result.InnerTitle = o.InnerTitle.Split('-')[0] + "-" + Convert.ToString(edDate).Split(' ')[0];
                            }

                            else
                            {
                                result.InnerTitle = o.InnerTitle;
                            }
                            tempcounter = tempcounter + 1;
                        }
                        else
                        {
                            result.InnerTitle = o.InnerTitle;
                        }


                        result.GetTitle = titleType;
                        lstResultPersonal.Add(result);
                    }
                    objResult.EmployeePersonalSearch = lstResultPersonal;

                }
            }
            return objResult;


        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="trend"></param>
        /// <param name="sortExpression"></param>
        /// <param name="sortDirection"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalRecords"></param>
        /// <returns></returns>
        [SessionExpireFilterAttribute]
        public IQueryable<HotmenuItems> GetHotMenuItems(string trend, string trendin, string catID,
            string sortExpression, string sortDirection, int pageIndex, int pageSize, ref int totalRecords)
        {
            GolflerEntities _db = new GolflerEntities();
            var courseid = Convert.ToInt64(HttpContext.Current.Session["AdminCourseId"]);
            var lstClicks = new List<HotmenuItems>();
            try
            {
                var courseID = new SqlParameter
                {
                    ParameterName = "COURSEID",
                    Value = courseid
                };
                var Trend = new SqlParameter
                {
                    ParameterName = "TREND",
                    Value = trend
                };
                var CategoryID = new SqlParameter
                {
                    ParameterName = "CATEGORYID",
                    Value = string.IsNullOrEmpty(catID) ? "0" : catID
                };

                if (trendin == "Own" || trendin == "")
                {
                    lstClicks = _db.Database.SqlQuery<HotmenuItems>("exec GF_Sp_GetHotMenuItems @COURSEID,@TREND,@CATEGORYID",
                     courseID, Trend, CategoryID).ToList<HotmenuItems>();
                }
                else
                {
                    lstClicks = _db.Database.SqlQuery<HotmenuItems>("exec GF_SP_GETHOTMENUITEMSOTHERCOURSE @COURSEID,@TREND,@CATEGORYID",
                     courseID, Trend, CategoryID).ToList<HotmenuItems>();
                }

                totalRecords = lstClicks.Count();

            }
            catch (Exception ex)
            {
                ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
            }
            return lstClicks.AsQueryable().Skip((pageIndex - 1) * pageSize).Take(pageSize);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="range"></param>
        /// <param name="fromdate"></param>
        /// <param name="todate"></param>
        /// <param name="columnNames"></param>
        /// <param name="Courses"></param>
        /// <param name="category"></param>
        /// <param name="subcategory"></param>
        /// <returns></returns>
        public List<object> GetFoodItemsData(int range, string fromdate, string todate, ref string[] columnNames, ref string[] Courses, long category, long subcategory)
        {
            var results = new List<object>();

            var _db = new GolflerEntities();

            if (!string.IsNullOrEmpty(fromdate))
            {
                fromdate = Convert.ToDateTime(fromdate).ToString("MM/dd/yyyy");
            }
            if (!string.IsNullOrEmpty(todate))
            {
                todate = DateTime.Parse(todate).ToString("MM/dd/yyyy");
            }
            var courseid = Convert.ToInt64(HttpContext.Current.Session["AdminCourseId"]);

            try
            {

                var entityConnection = new EntityConnection("name=GolflerEntities");
                var dbConnection = entityConnection.StoreConnection as SqlConnection;

                var command = new SqlCommand("dbo.GF_SP_FOODITEMREPORT", dbConnection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@COURSID", courseid);
                command.Parameters.AddWithValue("@RADIUS", range);
                command.Parameters.AddWithValue("@FROMDATE", fromdate);
                command.Parameters.AddWithValue("@TODATE", todate);
                command.Parameters.AddWithValue("@CATEGORY", category);
                command.Parameters.AddWithValue("@SUBCATEGORY", subcategory);

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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="range"></param>
        /// <param name="fromdate"></param>
        /// <param name="todate"></param>
        /// <param name="columnNames"></param>
        /// <param name="Courses"></param>
        /// <param name="category"></param>
        /// <param name="subcategory"></param>
        /// <returns></returns>
        public List<object> GetFoodItemsDataLineChart(int range, string fromdate, string todate, ref string[] columnNames, ref string[] Courses, long category, long subcategory)
        {
            var results = new List<object>();

            var _db = new GolflerEntities();

            if (!string.IsNullOrEmpty(fromdate))
            {
                fromdate = Convert.ToDateTime(fromdate).ToString("MM/dd/yyyy");
            }
            if (!string.IsNullOrEmpty(todate))
            {
                todate = DateTime.Parse(todate).ToString("MM/dd/yyyy");
            }
            var courseid = Convert.ToInt64(HttpContext.Current.Session["AdminCourseId"]);

            try
            {

                var entityConnection = new EntityConnection("name=GolflerEntities");
                var dbConnection = entityConnection.StoreConnection as SqlConnection;

                var command = new SqlCommand("dbo.GF_SP_FOODITEMREPORTLinerChart", dbConnection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@COURSID", courseid);
                command.Parameters.AddWithValue("@RADIUS", range);
                command.Parameters.AddWithValue("@FROMDATE", fromdate);
                command.Parameters.AddWithValue("@TODATE", todate);
                command.Parameters.AddWithValue("@CATEGORY", category);
                command.Parameters.AddWithValue("@SUBCATEGORY", subcategory);


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



        public List<object> GetFoodItemsDataExcelExport(int range, string fromdate, string todate, ref string[] columnNames, ref string[] Courses, long category, long subcategory)
        {
            var results = new List<object>();

            var _db = new GolflerEntities();

            if (!string.IsNullOrEmpty(fromdate))
            {
                fromdate = Convert.ToDateTime(fromdate).ToString("MM/dd/yyyy");
            }
            if (!string.IsNullOrEmpty(todate))
            {
                todate = DateTime.Parse(todate).ToString("MM/dd/yyyy");
            }
            var courseid = Convert.ToInt64(HttpContext.Current.Session["AdminCourseId"]);

            try
            {

                var entityConnection = new EntityConnection("name=GolflerEntities");
                var dbConnection = entityConnection.StoreConnection as SqlConnection;

                var command = new SqlCommand("dbo.GF_SP_FOODITEMREPORT", dbConnection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@COURSID", courseid);
                command.Parameters.AddWithValue("@RADIUS", range);
                command.Parameters.AddWithValue("@FROMDATE", fromdate);
                command.Parameters.AddWithValue("@TODATE", todate);
                command.Parameters.AddWithValue("@CATEGORY", category);
                command.Parameters.AddWithValue("@SUBCATEGORY", subcategory);

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




        /// <summary>
        /// Created By:Veera
        /// Created Date: 2 June 2015
        /// Purpose: Block/Unblock User
        /// </summary>
        /// <param name="id"></param>
        /// <param name="isGolfer"></param>
        /// <returns></returns>
        public string BlockNUnblockUser(long userId, long blockedUserId, bool isBlockedGolfer, string block)
        {
            string result = "-1";
            try
            {

                Db = new GolflerEntities();
                if (block == "1")//block User
                {
                    GF_CourseBlockUserList obj = new GF_CourseBlockUserList();
                    obj.CourseAdminId = userId;
                    obj.BlockedUserId = blockedUserId;
                    obj.IsBlockedGolfer = isBlockedGolfer;
                    obj.CourseId = LoginInfo.CourseId;
                    obj.CreatedDate = DateTime.Now;
                    Db.GF_CourseBlockUserList.Add(obj);
                    Db.SaveChanges();
                    result = "1";

                }
                else //unblock User
                {
                    var blockUser = Db.GF_CourseBlockUserList.Where(x => x.CourseAdminId == userId && x.BlockedUserId == blockedUserId && x.IsBlockedGolfer == isBlockedGolfer);
                    foreach (var unblock in blockUser)
                    {
                        Db.GF_CourseBlockUserList.Remove(unblock);
                    }
                    Db.SaveChanges();
                    result = "2";
                }


            }
            catch
            {
                result = "-1";
            }
            return result;
        }

        #region CourseMembership

        public IQueryable<GF_CourseMemberShip> GetCourseMembershipList(string filterExpression, string sortExpression, string sortDirection, int pageIndex, int pageSize, ref int totalRecords)
        {
            IQueryable<GF_CourseMemberShip> list;

            if (!String.IsNullOrWhiteSpace(filterExpression))
            {
                list = Db.GF_CourseMemberShip.Where(x => x.CourseId == LoginInfo.CourseId &&
                                                   (x.Email.ToLower().Contains(filterExpression.ToLower()) ||
                                                   (x.FirstName + " " + x.LastName).ToLower().Contains(filterExpression.ToLower()) ||
                                                   (x.MemberShipId == filterExpression.Trim()))
                                                    && x.Status != StatusType.Delete)
                        .ToList()
                        .Select((x => new GF_CourseMemberShip
                        {
                            ID = x.ID,
                            FirstName = x.FirstName,
                            LastName = x.LastName,
                            Email = x.Email,
                            MemberShipId = x.MemberShipId,
                            CreatedDate = x.CreatedDate
                        }


                )).AsQueryable();
            }
            else
            {
                list = Db.GF_CourseMemberShip.
                    Where(x => x.CourseId == LoginInfo.CourseId && x.Status != StatusType.Delete)
                       .ToList()
                       .Select((x => new GF_CourseMemberShip
                       {
                           ID = x.ID,
                           FirstName = x.FirstName,
                           LastName = x.LastName,
                           Email = x.Email,
                           MemberShipId = x.MemberShipId,
                           CreatedDate = x.CreatedDate
                       }


                )).AsQueryable();

                totalRecords = list.Count();
            }
            return list.OrderBy(sortExpression + " " + sortDirection).Skip((pageIndex - 1) * pageSize).Take(pageSize);




        }

        public bool DeleteMembership(List<long> ids)
        {

            var course = Db.GF_CourseMemberShip.Where(x => ids.AsQueryable().Contains(x.ID));
            foreach (var u in course)
            {
                u.Status = StatusType.Delete;

            }



            Db.SaveChanges();

            return true;
        }

        public GF_CourseMemberShip GetMemberShip(long? id)
        {
            try
            {
                if (Convert.ToInt64(id) > 0)
                {
                    var obj = Db.GF_CourseMemberShip.FirstOrDefault(x => x.ID == id);
                    return obj ?? new GF_CourseMemberShip();
                }
                else
                {
                    return new GF_CourseMemberShip();
                }
            }
            catch
            {
                return new GF_CourseMemberShip();
            }
        }

        public bool SaveMembership(GF_CourseMemberShip obj)
        {

            try
            {

                if (obj.ID > 0)
                {
                    #region Update Membership

                    if (obj != null)
                    {
                        var objMembership = Db.GF_CourseMemberShip.FirstOrDefault(x => x.ID == obj.ID);
                        objMembership.MemberShipId = obj.MemberShipId;
                        objMembership.Email = obj.Email;
                        objMembership.FirstName = obj.FirstName;
                        objMembership.LastName = obj.LastName;

                    }

                    Message = "update";
                    Db.SaveChanges();

                    #endregion update
                }
                else
                {
                    #region new user

                    obj.CreatedDate = DateTime.Now;
                    obj.CourseId = LoginInfo.CourseId;
                    obj.CourseAdminId = LoginInfo.UserId;
                    obj.Status = "A";
                    Db.GF_CourseMemberShip.Add(obj);

                    Db.SaveChanges();

                    Message = "add";

                    #endregion
                }

                return true;


                return false;
            }
            catch (Exception ex)
            {
                ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
                Message = "Exception: " + ex.Message;
                return false;
            }
        }


        public bool InsertMulipleMembership(List<GF_CourseMemberShip> lstobj, int courseId, int CourseAdminId)
        {

            try
            {

                foreach (GF_CourseMemberShip obj in lstobj)
                {

                    obj.CourseId = courseId;
                    obj.CreatedDate = DateTime.Now;
                    obj.CourseAdminId = CourseAdminId;
                    obj.CourseAdminId = CourseAdminId;
                    obj.Status = "A";
                    Db.GF_CourseMemberShip.Add(obj);
                    Message = "add";
                    Db.SaveChanges();

                }

                return true;


            }
            catch (Exception ex)
            {

                Message = "Error occured. Please try again.";
                return false;
            }
        }

        #endregion

        /// <summary>
        /// Created By:Veera
        /// Created Date: 29 May 2015
        /// Purpose: Get Golfer spend and Play chart By ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public CourseVisitNRevenueResult GetCourseSiteVisitsNRevenueById(long id, int type, int exeType)
        {
            CourseVisitNRevenueResult objResult = new CourseVisitNRevenueResult();
            try
            {

                Db = new GolflerEntities();
                if (exeType == 1)
                {
                    var lst = new List<GF_SP_GetCourseSiteVisits_Result>();
                    lst = Db.GF_SP_GetCourseSiteVisits(id, type).ToList();

                    if (lst.Count > 0)
                    {
                        List<SiteVisitResult> lstSiteVisitResult = new List<SiteVisitResult>();
                        foreach (var o in lst)
                        {
                            SiteVisitResult resultSiteVisit = new SiteVisitResult();

                            resultSiteVisit.NoOfVisits = o.NumberofVisits;
                            resultSiteVisit.Date = o.Date;

                            lstSiteVisitResult.Add(resultSiteVisit);

                        }
                        objResult.SiteVisitResult = lstSiteVisitResult;
                    }
                    else
                    {
                        List<SiteVisitResult> lstSiteVisitResult = new List<SiteVisitResult>();
                        SiteVisitResult resultSiteVisit = new SiteVisitResult();
                        resultSiteVisit.NoOfVisits = 0;
                        resultSiteVisit.Date = DateTime.Now.ToShortDateString();
                        lstSiteVisitResult.Add(resultSiteVisit);
                        objResult.SiteVisitResult = lstSiteVisitResult;
                    }
                }
                //
                if (exeType == 2)
                {
                    var lstRevenue = new List<GF_SP_GetCourseRevenue_Result>();
                    lstRevenue = Db.GF_SP_GetCourseRevenue(id, type).ToList();

                    if (lstRevenue.Count > 0)
                    {
                        List<RevenueResult> lstRevenueResult = new List<RevenueResult>();
                        foreach (var o in lstRevenue)
                        {
                            RevenueResult resultRevenue = new RevenueResult();

                            resultRevenue.Revenue = o.Revenue;
                            if (type == 1)
                            {
                                resultRevenue.Month = CommonFunctions.GetMonthName(Convert.ToInt32(o.Month));
                            }
                            else
                            {
                                resultRevenue.Month = Convert.ToString(o.Month);
                            }
                            lstRevenueResult.Add(resultRevenue);

                        }
                        objResult.RevenueResult = lstRevenueResult;
                    }
                    else
                    {
                        List<RevenueResult> lstRevenueResult = new List<RevenueResult>();
                        RevenueResult resultRevenue = new RevenueResult();
                        resultRevenue.Revenue = 0;
                        if (type == 1)
                        {
                            resultRevenue.Month = CommonFunctions.GetMonthName(Convert.ToInt32((DateTime.Now.Month)));
                        }
                        else
                        {
                            resultRevenue.Month = Convert.ToString(DateTime.Now.Month);
                        }
                        lstRevenueResult.Add(resultRevenue);

                        objResult.RevenueResult = lstRevenueResult;
                    }
                }
                //

            }
            catch (Exception ex)
            {
                string msg = ex.Message;
            }
            return objResult;
        }


    }

    #region Course Info Class
    /// <summary>
    /// Created By: Arun
    /// Created Date: 31 March 2015
    /// Extend the Course Class
    /// </summary>
    /// 

    [MetadataType(typeof(MetadataCourse))]
    public partial class GF_CourseInfo
    {
        public List<GF_FoodCommission> FoodCommission { get; set; }
        public List<GF_CourseMenu> CourseMenu { get; set; }

        public long? UserID { get; set; }
        bool isActive = true;
        public bool Active { get { return isActive; } set { isActive = value; } }

        public IEnumerable<SelectListItem> CourseType
        {
            get
            {
                yield return new SelectListItem { Text = "Public", Value = "Public" };
                yield return new SelectListItem { Text = "Private", Value = "Private" };
                yield return new SelectListItem { Text = "Semi-Private", Value = "Semi-Private" };
                yield return new SelectListItem { Text = "N/A", Value = "N/A" };
                yield return new SelectListItem { Text = "Resort", Value = "Resort" };
                yield return new SelectListItem { Text = "Military", Value = "Military" };

            }
        }

        public IEnumerable<SelectListItem> CourseGREENGRASSTYPE
        {
            get
            {
                yield return new SelectListItem { Text = "Alaska Blue Nugget", Value = "Alaska Blue Nugget" };
                yield return new SelectListItem { Text = "Artificial Turf", Value = "Artificial Turf" };
                yield return new SelectListItem { Text = "Bent Grass", Value = "Bent Grass" };
                yield return new SelectListItem { Text = "Bermuda Grass", Value = "Bermuda Grass" };
                yield return new SelectListItem { Text = "Other", Value = "Other" };
                yield return new SelectListItem { Text = "Poa Annua Grass", Value = "Poa Annua Grass" };
                yield return new SelectListItem { Text = "Sand", Value = "Sand" };
                yield return new SelectListItem { Text = "Seashore Paspalum", Value = "Seashore Paspalum" };
                yield return new SelectListItem { Text = "Tifdwarf Grass", Value = "Tifdwarf Grass" };
                yield return new SelectListItem { Text = "Winter Rye", Value = "Winter Rye" };
                yield return new SelectListItem { Text = "Tifeagle", Value = "Tifeagle" };
                yield return new SelectListItem { Text = "Zoysia Grass", Value = "Zoysia Grass" };
                yield return new SelectListItem { Text = "N/A", Value = "N/A" };
            }
        }

        public IEnumerable<SelectListItem> CourseSANDBUNKERS
        {
            get
            {
                yield return new SelectListItem { Text = "0", Value = "0" };
                yield return new SelectListItem { Text = "1-10", Value = "1-10" };
                yield return new SelectListItem { Text = "11-20", Value = "11-20" };
                yield return new SelectListItem { Text = "21-30", Value = "21-30" };
                yield return new SelectListItem { Text = "31-40", Value = "31-40" };
                yield return new SelectListItem { Text = "41-50", Value = "41-50" };
                yield return new SelectListItem { Text = "51-60", Value = "51-60" };
                yield return new SelectListItem { Text = "61-70", Value = "61-70" };
                yield return new SelectListItem { Text = "71-80", Value = "71-80" };
                yield return new SelectListItem { Text = "81-90", Value = "81-90" };
                yield return new SelectListItem { Text = "91-100", Value = "91-100" };
                yield return new SelectListItem { Text = "100+", Value = "100+" };
                yield return new SelectListItem { Text = "N/A", Value = "N/A" };
            }
        }

        public IEnumerable<SelectListItem> CourseYARDAGEMARKERS
        {
            get
            {
                yield return new SelectListItem { Text = "No Yardage Markers", Value = "No Yardage Markers" };
                yield return new SelectListItem { Text = "200, 150, 100 Yard Markers", Value = "200, 150, 100 Yard Markers" };
                yield return new SelectListItem { Text = "150 Yard Markers Only", Value = "150 Yard Markers Only" };
                yield return new SelectListItem { Text = "Sprinkler Heads Marked", Value = "Sprinkler Heads Marked" };
                yield return new SelectListItem { Text = "Every 25 Yards", Value = "Every 25 Yards" };
                yield return new SelectListItem { Text = "150 Yard Bird House Markers", Value = "150 Yard Bird House Markers" };
                yield return new SelectListItem { Text = "200, 150, 100 Yrd Markers and Sprinkler Heads Marked", Value = "200, 150, 100 Yrd Markers and Sprinkler Heads Marked" };
                yield return new SelectListItem { Text = "Electronic Range System", Value = "Electronic Range System" };
                yield return new SelectListItem { Text = "Kirby Marking System", Value = "Kirby Marking System" };
                yield return new SelectListItem { Text = "91-100", Value = "91-100" };
                yield return new SelectListItem { Text = "100+", Value = "100+" };
                yield return new SelectListItem { Text = "N/A", Value = "N/A" };
            }
        }

        public IEnumerable<SelectListItem> CourseAccess
        {
            get
            {
                yield return new SelectListItem { Text = "Yes", Value = "Yes" };
                yield return new SelectListItem { Text = "No", Value = "No" };

            }
        }

        public IEnumerable<SelectListItem> CourseDisCont
        {
            get
            {
                yield return new SelectListItem { Text = "Junior", Value = "Junior" };
                yield return new SelectListItem { Text = "Twilight,None", Value = "Twilight,None" };
                yield return new SelectListItem { Text = "Twilight,Junior,Senior", Value = "Twilight,Junior,Senior" };
                yield return new SelectListItem { Text = "None", Value = "None" };
                yield return new SelectListItem { Text = "Junior,Senior", Value = "Junior,Senior" };
                yield return new SelectListItem { Text = "Twilight", Value = "Twilight" };
                yield return new SelectListItem { Text = "Twilight,Senior", Value = "Twilight,Senior" };
                yield return new SelectListItem { Text = "Twilight,Junior,None", Value = "Twilight,Junior,None" };
                yield return new SelectListItem { Text = "Twilight,None", Value = "Twilight,None" };
                yield return new SelectListItem { Text = "Senior,None", Value = "Senior,None" };
                yield return new SelectListItem { Text = "Senior", Value = "Senior" };
                yield return new SelectListItem { Text = "100+", Value = "100+" };
                yield return new SelectListItem { Text = "N/A", Value = "N/A" };
            }
        }

        public IEnumerable<SelectListItem> CourseRENTALCLUBS
        {
            get
            {
                yield return new SelectListItem { Text = "Yes", Value = "Yes" };
                yield return new SelectListItem { Text = "No", Value = "No" };

            }
        }

        public IEnumerable<SelectListItem> CoursePULLCARTS
        {
            get
            {

                yield return new SelectListItem { Text = "$0-5", Value = "$0-5" };
                yield return new SelectListItem { Text = "$5-10", Value = "$5-10" };
                yield return new SelectListItem { Text = "$10-15", Value = "$10-15" };
                yield return new SelectListItem { Text = "Yes", Value = "Yes" };
                yield return new SelectListItem { Text = "No", Value = "No" };
                yield return new SelectListItem { Text = "N/A", Value = "N/A" };

            }
        }

        public IEnumerable<SelectListItem> CourseWALKING
        {
            get
            {
                yield return new SelectListItem { Text = "Allowed", Value = "Allowed" };
                yield return new SelectListItem { Text = "Not Allowed", Value = "Not Allowed" };

            }
        }

        public IEnumerable<SelectListItem> CourseRESTAURANT
        {
            get
            {

                yield return new SelectListItem { Text = "Restaurant, Snack Bar, Beverage Cart", Value = "Restaurant, Snack Bar, Beverage Cart" };
                yield return new SelectListItem { Text = "Snack Bar", Value = "Snack Bar" };
                yield return new SelectListItem { Text = "None", Value = "None" };
                yield return new SelectListItem { Text = "Restaurant, Snack Bar", Value = "Restaurant, Snack Bar" };
                yield return new SelectListItem { Text = "Restaurant", Value = "Restaurant" };
                yield return new SelectListItem { Text = "Snack Bar, Beverage Cart", Value = "Snack Bar, Beverage Cart" };
                yield return new SelectListItem { Text = "Convenience Food", Value = "Convenience Food" };
                yield return new SelectListItem { Text = "Restaurant, Beverage Cart", Value = "Restaurant, Beverage Cart" };

            }
        }

        public IEnumerable<SelectListItem> CourseBAR
        {
            get
            {

                yield return new SelectListItem { Text = "None", Value = "None" };
                yield return new SelectListItem { Text = "Full Bar", Value = "Full Bar" };
                yield return new SelectListItem { Text = "Beer,Wine", Value = "Beer,Wine" };
                yield return new SelectListItem { Text = "Yes", Value = "Yes" };
                yield return new SelectListItem { Text = "Beer", Value = "Beer" };


            }
        }

        public IEnumerable<SelectListItem> CourseHOURS
        {
            get
            {

                yield return new SelectListItem { Text = "Open 8", Value = "Open 8" };
                yield return new SelectListItem { Text = "Open 7", Value = "Open 7" };
                yield return new SelectListItem { Text = "Open 6", Value = "Open 6" };
                yield return new SelectListItem { Text = "Open 5", Value = "Open 5" };
                yield return new SelectListItem { Text = "Open Dawn, Close Dusk", Value = "Open Dawn, Close Dusk" };


            }
        }

        public IEnumerable<SelectListItem> CourseFOOD
        {
            get
            {
                yield return new SelectListItem { Text = "Yes", Value = "Yes" };
                yield return new SelectListItem { Text = "No", Value = "No" };

            }
        }

        public IEnumerable<SelectListItem> CourseAVAILABLEPRODUCTS
        {
            get
            {

                yield return new SelectListItem { Text = "Accessories", Value = "Accessories" };
                yield return new SelectListItem { Text = "Clubs, Apparel, Accessories", Value = "Clubs, Apparel, Accessories" };
                yield return new SelectListItem { Text = "None", Value = "None" };


            }
        }

        public IEnumerable<SelectListItem> CourseHOMESONCOURSE
        {
            get
            {
                yield return new SelectListItem { Text = "Yes", Value = "Yes" };
                yield return new SelectListItem { Text = "No", Value = "No" };
                yield return new SelectListItem { Text = "N/A", Value = "N/A" };
            }
        }






    }


    /// <summary>
    /// Meta Data Class
    /// </summary>
    public class MetadataCourse
    {

        [Required(ErrorMessage = "Required")]
        public string COURSE_NAME { get; set; }

        //[Required(ErrorMessage = "Required")]
        public string ADDRESS { get; set; }

        //[Required(ErrorMessage = "Required")]
        public string CITY { get; set; }

        //[Required(ErrorMessage = "Required")]
        public string STATE { get; set; }

        //[Required(ErrorMessage = "Required")]
        public string ZIPCODE { get; set; }

        [Required(ErrorMessage = "Required")]
        [Range(0, 9999, ErrorMessage = "Value should be less than 9999")]
        [RegularExpression(RegularExp.Decimal, ErrorMessage = "Please insert a zero before the decimal for platform fees less than $1.00 as follows 0.99.")]
        public string PlateformFee { get; set; }

        [Required(ErrorMessage = "Required")]
        public string PartnershipStatus { get; set; }

        //[Required(ErrorMessage = "Required")]
        [Range(0, 9999, ErrorMessage = "Value should be less than 9999")]
        public string Tax { get; set; }

        //[Required(ErrorMessage = "Required")]
        public string LATITUDE { get; set; }

        //[Required(ErrorMessage = "Required")]
        public string LONGITUDE { get; set; }

        //[Required(ErrorMessage = "Required")]
        //public long UserID { get; set; }

        //[Required(ErrorMessage = "Required")]
        [Display(Name = "Phone")]
        [RegularExpression(RegularExp.PhoneNotReq, ErrorMessage = "Please enter valid phone number.")]
        [StringLength(20, ErrorMessage = "Phone number cannot be greater than 20 character long.")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string PHONE { get; set; }
    }

    #endregion

    #region Course Membership Class
    /// <summary>
    /// Created By: Veera
    /// Created Date: 5 June 2015
    /// Extend the CourseMemberShip Class
    /// </summary>
    /// 

    [MetadataType(typeof(MetadataCourseMemberShip))]
    public partial class GF_CourseMemberShip
    {



    }


    /// <summary>
    /// Meta Data Class
    /// </summary>
    public class MetadataCourseMemberShip
    {

        [Required(ErrorMessage = "Required")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Required")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Required")]
        [StringLength(200, ErrorMessage = "Email cannot be greater than 200 character long.")]
        [RegularExpression(RegularExp.Email, ErrorMessage = "Please enter valid email address.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Required")]
        public string MemberShipId { get; set; }


    }

    #endregion

    public class GolferresultSet
    {
        public long Id { get; set; }
        public int Status { get; set; }
        public List<GolferMsgResult> record { get; set; }
        public string Error { get; set; }
    }


    public class FoodItemreport
    {

        public int Total { get; set; }
        public string Name { get; set; }
        public string CourseName { get; set; }
    }

    public class HotmenuItems
    {
        public long ID { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }

    }

    public class GolferMsgResult
    {
        public long Id { get; set; }
        public string Message { get; set; }
        public string CreatedDate { get; set; }
        public string PageNo { get; set; }
        public string IsSender { get; set; }
        public string Status { get; set; }
        public string TotalPages { get; set; }
    }
    public partial class GF_Messages
    {
        public string Name { get; set; }
        public string MsgDate { get; set; }
        public int TodaysMsgCount { get; set; }
    }

    public partial class GF_FoodCommission
    {
        public string CategoryName { get; set; }
    }
    public class OrderAnalyticsListResult
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public long CourseID { get; set; }
        public long? CatID { get; set; }

    }
    public class OrderAnalyticsSearchResult
    {
        public decimal AvgPricePerOrder { get; set; }
        public decimal AvgPricePerAllCourses { get; set; }
        public decimal AvgRatingPerOrder { get; set; }
        public decimal AvgRatingPerAllCourses { get; set; }
        public IEnumerable<OrderAnalyticsMenuSearchResult> MenuSearch { get; set; }
        public IEnumerable<object> GraphSearch { get; set; }
        public string jsonGraphSearch { get; set; }
        public IEnumerable<OrderAnalyticsGraphSearchResult> GraphCourseSearch { get; set; }
    }
    public class OrderAnalyticsMenuSearchResult
    {
        public long CourseId { get; set; }
        public string Name { get; set; }
        public decimal? AvgPrice { get; set; }
        public string CategoryName { get; set; }
        public long? CategoryId { get; set; }
        public string SubCategoryName { get; set; }
        public long? SubCategoryId { get; set; }
        public string CourseName { get; set; }
        public string CombineName { get; set; }
    }
    public class OrderAnalyticsGraphSearchResult
    {
        public string CourseName { get; set; }
    }
    public class EmployeeAnalyticsSearchResult
    {
        public IEnumerable<EmployeeAnalyticsDataSearchResult> EmployeeSearch { get; set; }
        public IEnumerable<EmployeeAnalyticsDataSearchResult> EmployeePersonalSearch { get; set; }
        public bool IsUserExists { get; set; }
        public string eUID { get; set; }

    }
    public class EmployeeAnalyticsDataSearchResult
    {
        public decimal? PriceMe { get; set; }
        public decimal? PriceCourse { get; set; }
        public int? NoOfOrdersMe { get; set; }
        public int? NoOfOrdersCourse { get; set; }
        public decimal? RatingMe { get; set; }
        public decimal? RatingCourse { get; set; }
        public string GetTitle { get; set; }
        public string InnerTitle { get; set; }
    }
    public partial class GF_CourseMenu
    {
        public string CategoryName { get; set; }
    }


    public class CourseVisitNRevenueResult
    {
        public IEnumerable<object> SiteVisitResult { get; set; }
        public IEnumerable<object> RevenueResult { get; set; }
    }
    public class SiteVisitResult
    {
        public int? NoOfVisits { get; set; }
        public string Date { get; set; }
    }

    public class RevenueResult
    {
        public decimal? Revenue { get; set; }
        public string Month { get; set; }
    }

    public class CourseInformation
    {
        public long ID { get; set; }
        public string COURSE_NAME { get; set; }
        public string PartnershipStatus { get; set; }
        public string STATE { get; set; }
        public string CITY { get; set; }
        public string Status { get; set; }
        public long EID { get; set; }
        public string UserName { get; set; }
        public string Coordinate { get; set; }
        public string ScoreCard { get; set; }
    }

    public class CourseInfoDetail
    {
        public long ClubHouseID { get; set; }
        public string CLUB_HOUSE { get; set; }
        public long ID { get; set; }
        public string COURSE_NAME { get; set; }
        public string STATUS { get; set; }
        public long EID { get; set; }
        public string HOLE { get; set; }
        public string SCORECARD { get; set; }
    }
}
