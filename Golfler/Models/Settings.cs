using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Golfler.Models
{
    public partial class GF_Settings
    {

        protected GolflerEntities Db;

        public string Message { get; private set; }

        public List<GF_Settings> lstSettings
        {
            get
            {
                Db = new GolflerEntities();

                if (LoginInfo.Type == UserType.SuperAdmin || LoginInfo.Type == UserType.Admin)
                {
                    var list = Db.GF_Settings.Where(x => x.CourseID == 0).ToList();
                    list = list.Where(x => x.Name != "StripeAccount").ToList();
                    list = list.Where(x => x.Name != "StripeApiKey").ToList();
                    list = list.Where(x => x.Name != "StripeSecretApiKey").ToList();
                    return list;
                }
                else if (LoginInfo.Type == UserType.CourseAdmin ||
                         LoginInfo.Type == UserType.PowerAdmin ||
                         LoginInfo.Type == UserType.Proshop ||
                         LoginInfo.Type == UserType.Kitchen ||
                         LoginInfo.Type == UserType.Cartie ||
                         LoginInfo.Type == UserType.Ranger)
                {
                    var list = Db.GF_Settings.Where(x => x.CourseID == LoginInfo.CourseId).ToList();
                    list = list.Where(x => x.Name != "StripeAccount" ).ToList();
                    list = list.Where(x => x.Name != "StripeApiKey").ToList();
                    list = list.Where(x => x.Name != "StripeSecretApiKey").ToList();
                     
                    return list;
                }
                else
                {
                    var list = Db.GF_Settings.Where(x => x.CourseID == 0).ToList();
                    return list;
                }

            }

        }
        /// <summary>
        /// Created By:Arun
        /// </summary>
        /// <param name="lstName"></param>
        /// <param name="lstValue"></param>
        /// <returns></returns>
        public bool UpdateSettings(List<string> lstName, List<string> lstValue, long courseId)
        {
            Db = new GolflerEntities();
            for (int i = 0; i < lstName.Count; i++)
            {
                var name = Convert.ToString(lstName[i]);
                var obj = Db.GF_Settings.FirstOrDefault(x => x.Name == name && x.CourseID == courseId);
                if (obj != null)
                {
                    obj.Value = Convert.ToString(lstValue[i]);
                    obj.ModifyDate = DateTime.Now;
                    obj.ModifyBy = Convert.ToString(LoginInfo.UserId);

                }
                Db.SaveChanges();
            }
            return true;

        }



    }
}