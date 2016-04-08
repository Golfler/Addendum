using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Caching;
using Golfler.Models;


namespace Golfler.Repositories
{   

    public class SuggestedCourse
    {
        public GF_CourseSuggest objSuggestedCourse { get; private set; }
        public string Message { get; private set; }
        protected GolflerEntities Db;

             #region Constructors

        public SuggestedCourse()
        {
            Db = new GolflerEntities();
        }

        public SuggestedCourse(long? id)
        {
            Db = new GolflerEntities();
            objSuggestedCourse = Db.GF_CourseSuggest.FirstOrDefault(u => u.ID == id);
        }

        #endregion


        internal bool ChangeStatus(bool status)
        {
            GF_RoleModules objModuleRole = new GF_RoleModules();
            

            //objModuleRole = CommonFunctions.GetAccessModule(ModuleValues.AllRights);


            //if (!objModuleRole.UpdateFlag)
            //{
            //    Message = "unaccess";
            //    return false;
            //}

            if (objSuggestedCourse != null)
            {

                objSuggestedCourse.Active = !status;
                objSuggestedCourse.ModifiedBy = LoginInfo.UserId;
                objSuggestedCourse.ModifiedOn = DateTime.Now;
                
                Db.SaveChanges();
                return true;
            }
            return false;
        }

      
    }
}