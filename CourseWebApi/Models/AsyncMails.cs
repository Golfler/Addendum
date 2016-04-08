using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Xml;
using Newtonsoft.Json;

namespace CourseWebApi.Models
{
    public class AsyncMails
    {
        private static GolflerEntities _db = null;

        #region Async Thread register mails

        public static void callingAscynMails(GF_AdminUsers objReg,string subject)
        {
            invProfileApplicationCompliance = new delProfileApplicationCompliance(SendAsynMails);
            invProfileApplicationCompliance.BeginInvoke(objReg, subject, new AsyncCallback(SendAsynMails), null);
        }

        public delegate void delProfileApplicationCompliance(GF_AdminUsers objReg,string subject);
        public static delProfileApplicationCompliance invProfileApplicationCompliance;

        public static void SendAsynMails(IAsyncResult t)
        {
            List<string> objList = new List<string>();
            try
            {
                invProfileApplicationCompliance.EndInvoke(t);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public static void SendAsynMails(GF_AdminUsers objReg, string subject)
        {
            try
            {
                _db = new GolflerEntities();
                IQueryable<GF_EmailTemplatesFields> templateFields = null;
                long courseid = Convert.ToInt64(objReg.CourseId);
                string mailresult = "";

                var param = EmailParams.GetEmailParamsNew(ref _db, "Registration", ref templateFields, courseid, "CA", ref mailresult, true);
                   
               // var param = EmailParams.GetEmailParams(ref _db, "Registration", ref templateFields);

                if (param != null)
                {
                  
                    if (!ApplicationEmails.CourseUserRegistrationMail(objReg, param, ref templateFields,subject))
                    {
                        //

                    }
                    else
                    {
                        //
                    }
                }
                else
                {
                    //
                }
            }
            catch
            {
                return;
            }
        }

        #endregion
    }
}