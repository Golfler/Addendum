using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Golfler.Models;
using System.Data.Objects;

namespace Golfler.Repositories
{
    public class MassMessageClass
    {
        protected GolflerEntities Db;

        /// <summary>
        /// Created By: Ramesh Kalra
        /// Created on: 21st April, 2015
        /// </summary>
        /// <remarks>Get Users Listing for Mass Messages</remarks>
        public IQueryable<MassMessages> GetUsersForMassMessages(string filterExpression, string CourseId, string strUserType,string strRequestFrom,string RangeParameter,  string sortExpression, string sortDirection, int pageIndex, int pageSize, ref int totalRecords)
        {
            List<MassMessages> list = new List<MassMessages>();
            try
            {
                if (strRequestFrom == "CourseAdmin")
                {
                    CourseId = Convert.ToString(LoginInfo.CourseId);
                }
                if (!string.IsNullOrEmpty(Convert.ToString(CourseId)))
                {
                    Int64 intCourseId = Convert.ToInt64(CourseId);
                    Db = new GolflerEntities();
                    List<GF_AdminUsers> listUsers = new List<GF_AdminUsers>();
                    List<GF_Golfer> listGolferUsers = new List<GF_Golfer>();

                    #region Step 1: Get Course Users
                    // Get Course Users only If User Type is not Golfler OR User Type is empty
                    if (Convert.ToString(strUserType) != UserType.Golfer || (string.IsNullOrEmpty(Convert.ToString(strUserType))))
                    {
                        // Course Search Filter
                        listUsers = Db.GF_AdminUsers.Where(x => x.CourseId == intCourseId && x.Status == StatusType.Active).ToList();


                        // Name/Email Search Filter
                        if (!String.IsNullOrWhiteSpace(filterExpression))
                        {
                            listUsers = listUsers.Where(x => x.Email.ToLower().Contains(filterExpression.ToLower()) || ((x.FirstName + " " + x.LastName).ToLower().Contains(filterExpression.ToLower()))).ToList();
                        }

                        try
                        {
                            if ((strRequestFrom == "SuperAdmin") && Convert.ToInt64(RangeParameter) > 0 && Convert.ToInt64(intCourseId)>0)
                            {
                                var lstAdminUsers = Db.GF_SP_GetCourseAdminUsersByRadius(Convert.ToInt64(CourseId), RangeParameter).ToList();
                                var lstAdminUserIds = lstAdminUsers.Select(x => x.ID).ToList();
                                listUsers = listUsers.Where(x => lstAdminUserIds.Contains(x.ID)).ToList();
                            }
                        }
                        catch (Exception ex)
                        {
                            ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
                        }
                    }
                    #endregion

                    #region Step 2: Get Golfer Users
                    // Get Golfer Users only If User Type is Golfer OR User Type is empty
                    if (Convert.ToString(strUserType) == UserType.Golfer || (string.IsNullOrEmpty(Convert.ToString(strUserType))))
                    {
                        // Course Search Filter
                        listGolferUsers = (from user in Db.GF_Golfer
                                           join course in Db.GF_GolferUser on user.GF_ID equals course.GolferID
                                           where course.CourseID == intCourseId && user.Status == StatusType.Active
                                           select user).ToList();

                        // Name/Email Search Filter
                        if (!String.IsNullOrWhiteSpace(filterExpression))
                        {
                            listGolferUsers = listGolferUsers.Where(x => x.Email.ToLower().Contains(filterExpression.ToLower()) || ((x.FirstName + " " + x.LastName).ToLower().Contains(filterExpression.ToLower()))).ToList();
                        }

                        try
                        {
                            if ((strRequestFrom == "SuperAdmin") && Convert.ToInt64(RangeParameter) > 0 && Convert.ToInt64(intCourseId) > 0)
                            {
                                var lstGolferUsers = Db.GF_SP_GetCourseGolferUsersByRadius(Convert.ToInt64(CourseId), RangeParameter).ToList();
                                var lstGolferUserIds = lstGolferUsers.Select(x => x.GF_ID).ToList();

                                var lstGolferUsersLocations = Db.GF_SP_GetCourseGolferUsersLocationByRadius(Convert.ToInt64(CourseId), RangeParameter).ToList();
                                var lstGolferUserIdsLocations = lstGolferUsersLocations.Select(x => x.GF_ID).ToList();

                                lstGolferUserIdsLocations.AddRange(lstGolferUserIds);

                                listGolferUsers = listGolferUsers.Where(x => lstGolferUserIdsLocations.Contains(x.GF_ID)).ToList();
                            }
                        }
                        catch (Exception ex)
                        {
                            ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
                        }
                    }
                    #endregion

                    string strCourseName = Convert.ToString(Db.GF_CourseInfo.Where(x => x.ID == intCourseId).FirstOrDefault().COURSE_NAME);

                    #region Step 3: Merge Both Users
                    int cnt = 1;
                    // If User Type is empty than merge both users because in this case we need to show both type of users 
                    if (string.IsNullOrEmpty(Convert.ToString(strUserType)))
                    {
                        // add course users
                        foreach (var objUser in listUsers)
                        {
                            MassMessages objMsg = new MassMessages();
                            objMsg.UserName = objUser.FirstName + " " + objUser.LastName;
                            objMsg.Email = objUser.Email;
                            objMsg.userid = objUser.ID;
                            objMsg.Id = cnt;
                            objMsg.CourseName = strCourseName;
                            objMsg.Type = UserType.GetFullUserType(objUser.Type);
                            list.Add(objMsg);
                            cnt = cnt + 1;
                        }
                        // add golfer users
                        foreach (var objUser in listGolferUsers)
                        {
                            MassMessages objMsg = new MassMessages();
                            objMsg.UserName = objUser.FirstName + " " + objUser.LastName;
                            objMsg.Email = objUser.Email;
                            objMsg.Id = cnt;
                            objMsg.userid = objUser.GF_ID;
                            objMsg.CourseName = strCourseName;
                            objMsg.Type = UserType.GetFullUserType(UserType.Golfer); 
                            list.Add(objMsg);
                            cnt = cnt + 1;
                        }
                    }
                    else // User Type is not empty than Add only Selected Users.
                    {
                        if (Convert.ToString(strUserType) == UserType.Golfer)
                        {
                            // Add only Golfler Users 
                            foreach (var objUser in listGolferUsers)
                            {
                                MassMessages objMsg = new MassMessages();
                                objMsg.UserName = objUser.FirstName + " " + objUser.LastName;
                                objMsg.Email = objUser.Email;
                                objMsg.Id = cnt;
                                objMsg.userid = objUser.GF_ID;
                                objMsg.CourseName = strCourseName;
                                objMsg.Type = UserType.GetFullUserType(UserType.Golfer); 
                                list.Add(objMsg);
                                cnt = cnt + 1;
                            }
                        }
                        else
                        {
                            // Add Course Users by type
                            foreach (var objUser in listUsers)
                            {
                                if (objUser.Type == strUserType)
                                {
                                    MassMessages objMsg = new MassMessages();
                                    objMsg.UserName = objUser.FirstName + " " + objUser.LastName;
                                    objMsg.Email = objUser.Email;
                                    objMsg.Id = cnt;
                                    objMsg.userid = objUser.ID;
                                    objMsg.CourseName = strCourseName;
                                    objMsg.Type = UserType.GetFullUserType(objUser.Type);
                                    list.Add(objMsg);
                                    cnt = cnt + 1;
                                }
                            }
                        }
                    }
                    #endregion

                }
            }
            catch (Exception ex)
            {
                ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
            }

            IQueryable<MassMessages> qryList = list.AsQueryable();
            totalRecords = qryList.Count();
            HttpContext.Current.Session["UsersForMassMessages"] = qryList;

            return qryList.OrderBy(sortExpression + " " + sortDirection).Skip((pageIndex - 1) * pageSize).Take(pageSize);
        }

        /// <summary>
        /// Created By: Ramesh Kalra
        /// Created on: 21st April, 2015
        /// </summary>
        /// <remarks>Get Messages history</remarks>
        public IQueryable<GF_MassMessages> GetMessageHistory(string filterExpression, string CourseId, string strUserType,string strRequestFrom, string fromDate, string toDate, string sortExpression, string sortDirection, int pageIndex, int pageSize, ref int totalRecords)
        {
            List<GF_MassMessages> list = new List<GF_MassMessages>();
            IQueryable<GF_MassMessages> qryList = null;

            Db = new GolflerEntities();
            try
            {
                Int64 intcourseid = 0;

                if (strRequestFrom == "CourseAdmin")
                {
                    intcourseid = LoginInfo.CourseId;
                }
                else
                {
                    intcourseid = 0;
                }

               var listEntity = Db.GF_MassMessages.Where(x => x.Status == StatusType.Active && x.CourseId == intcourseid);

               #region dates
               // var listHistory = list.AsQueryable();
               if (!string.IsNullOrEmpty(fromDate) && !string.IsNullOrEmpty(toDate))
               {
                   DateTime dtDate = DateTime.Parse(fromDate);
                   DateTime dtToDate = DateTime.Parse(toDate);

                   listEntity = listEntity.Where(x => EntityFunctions.TruncateTime(x.CreatedDate) >= EntityFunctions.TruncateTime(dtDate)
                         && EntityFunctions.TruncateTime(x.CreatedDate) <= EntityFunctions.TruncateTime(dtToDate));
                    
               }
               else if (!string.IsNullOrEmpty(fromDate))
               {
                   DateTime dtDate = DateTime.Parse(fromDate);

                   listEntity = listEntity.Where(x => EntityFunctions.TruncateTime(x.CreatedDate) >= EntityFunctions.TruncateTime(dtDate));
                    
               }
               else if (!string.IsNullOrEmpty(toDate))
               {
                   DateTime dtToDate = DateTime.Parse(toDate);

                   listEntity = listEntity.Where(x => EntityFunctions.TruncateTime(x.CreatedDate) <= EntityFunctions.TruncateTime(dtToDate));
                    
               }
               #endregion

               list = listEntity.ToList();
                if (strRequestFrom == "SuperAdmin")
                {
                    if (!string.IsNullOrEmpty(Convert.ToString(CourseId)))
                    {
                        Int64 intCourseId = Convert.ToInt64(CourseId);
                        list = list.Where(x => x.SendToCourseId == intCourseId).ToList();
                    }
                }

                if (!string.IsNullOrEmpty(Convert.ToString(strUserType)))
                {
                    string fullUserType = UserType.GetFullUserType(strUserType);
                    list = list.Where(x => x.SendToUserType == fullUserType).ToList();
                }

                if (!String.IsNullOrWhiteSpace(filterExpression))
                {
                    list = list.Where(x => x.SendToUserEmail.ToLower().Contains(filterExpression.ToLower()) || ((x.SendToUserName).ToLower().Contains(filterExpression.ToLower()))).ToList();
                }

                qryList = list.AsQueryable();

                //if (!string.IsNullOrEmpty(fromDate) && !string.IsNullOrEmpty(toDate))
                //{
                //    DateTime dtDate = DateTime.Parse(fromDate);
                //    DateTime dtToDate = DateTime.Parse(toDate);

                //    qryList = qryList.Where(x => x.CreatedDate.Value.Year >= dtDate.Year
                //        && x.CreatedDate.Value.Month >= dtDate.Month
                //        && x.CreatedDate.Value.Day >= dtDate.Day

                //    && x.CreatedDate.Value.Year <= dtToDate.Year
                //        && x.CreatedDate.Value.Month <= dtToDate.Month
                //        && x.CreatedDate.Value.Day <= dtToDate.Day);
                //}
                //else if (!string.IsNullOrEmpty(fromDate))
                //{
                //    DateTime dtDate = DateTime.Parse(fromDate);

                //    qryList = qryList.Where(x => x.CreatedDate.Value.Year >= dtDate.Year
                //        && x.CreatedDate.Value.Month >= dtDate.Month
                //        && x.CreatedDate.Value.Day >= dtDate.Day
                //        );
                //}
                //else if (!string.IsNullOrEmpty(toDate))
                //{
                //    DateTime dtToDate = DateTime.Parse(toDate);

                //    qryList = qryList.Where(x => x.CreatedDate.Value.Year <= dtToDate.Year
                //        && x.CreatedDate.Value.Month <= dtToDate.Month
                //        && x.CreatedDate.Value.Day <= dtToDate.Day
                //        );

                //}
            }
            catch (Exception ex)
            {
                ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
            }

           
            totalRecords = qryList.Count();
            
            return qryList.OrderBy(sortExpression + " " + sortDirection).Skip((pageIndex - 1) * pageSize).Take(pageSize);
        }

    }
}