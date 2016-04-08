using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using CourseWebApi.Models;
using CourseWebAPI.Models;
using ErrorLibrary;

namespace CourseWebApi.Models
{
    public class Users
    {
        private GolflerEntities _db = null;
       
        public Users()
        {
            _db = new GolflerEntities();
        }
        public Result AddUpdateCourseUser(GF_AdminUsers obj)
        {

            try
            {
               
                if (obj.ID > 0)
                {
                    #region Update User
                    bool isMail = false;
                    var UserObj = _db.GF_AdminUsers.FirstOrDefault(p => p.ID == obj.ID);

                    if (UserObj != null)
                    {
                        if ( CommonFunctions.DecryptString(UserObj.Password,UserObj.SALT) != obj.Password)
                        {
                            UserObj.SALT = CommonLibClass.FetchRandStr(3);
                            UserObj.Password = CommonFunctions.EncryptString(obj.Password, UserObj.SALT);
                            isMail = true;
                        }

                        UserObj.FirstName = obj.FirstName;
                        UserObj.LastName = obj.LastName;
                        UserObj.Email = obj.Email;
                        UserObj.ModifiedBy = obj.CreatedBy;
                        UserObj.ModifiedOn = DateTime.Now;
                        UserObj.Phone = obj.Phone ?? UserObj.Phone;
                        UserObj.Type = obj.Type;
                        _db.SaveChanges();
                        if (isMail)
                        {
                            #region Send Mail To course user

                            AsyncMails.callingAscynMails(UserObj, "Your Profile has been updated " + ": ");


                            #endregion
                        }
                        return new Result { Id = UserObj.ID, Status = 1, Error = "User updated successfully.", record = null };
                    }
                    else
                    {
                        return new Result { Id = 0, Status = 0, Error = "User does not exists.", record = null };
                    }
                    #endregion
                }
                else
                {
                    var objCourse = _db.GF_CourseInfo.FirstOrDefault(p => p.ID == obj.CourseId);
                    if (objCourse == null)
                    {
                        return new Result { Id = 0, Status = 0, Error = "Course does not exists.", record = null };
                    }

                    #region add User
                    var user = _db.GF_AdminUsers.FirstOrDefault(p => p.UserName.ToLower() == obj.UserName.ToLower());
                    if (user != null)
                    {
                        return new Result { Id = 0, Status = 0, Error = "User already exists." };
                    }
                  
                    obj.CreatedOn = DateTime.Now;
                    obj.Status = StatusType.Active;
                    obj.SALT = CommonLibClass.FetchRandStr(3);
                    obj.Password = CommonFunctions.EncryptString(obj.Password, obj.SALT);
                    _db.GF_AdminUsers.Add(obj);
                    _db.SaveChanges();
                   
                    #region Send Mail To course user

                  
                    AsyncMails.callingAscynMails(obj, "Your Profile has been created " + ": ");

                  
                    #endregion

                    return new Result { Id = obj.ID, Status = 1, Error = "User added successfully.", record = null };
                    #endregion
                }



            }
            catch (Exception ex)
            {
                return new Result { Id = 0, Status = 0, Error = ex.Message, record = null };
            }
        }
        public Result DeleteCourseUser(long UserID)
        {
            try
            {
                if (_db.GF_AdminUsers.Count(x => x.ID == UserID) > 0)
                {
                   
                    var obj = _db.GF_AdminUsers.FirstOrDefault(x => x.ID == UserID);
                    obj.Status = StatusType.Delete;
                    obj.IsOnline = false;
                    _db.SaveChanges();
                    return new Result { Id = 0, Status = 1, Error = "User deleted successfully.", record = null };
                }
                else
                {
                    return new Result { Id = 0, Status = 0, Error = "User does not exists.", record = null };
                }
            }
            catch (Exception ex)
            {
                return new Result { Id = 0, Status = 0, Error = ex.Message, record = null };
            }
        }
    }
}