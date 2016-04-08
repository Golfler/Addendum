using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Xml;
using Newtonsoft.Json;

namespace GolferWebAPI.Models
{
    public class AscynMails
    {
        private static GolflerEntities _db = null;

        #region Async Thread register mails

        public static void callingAscynMails(GF_Golfer objReg)
        {
            invProfileApplicationCompliance = new delProfileApplicationCompliance(SendAsynMails);
            invProfileApplicationCompliance.BeginInvoke(objReg, new AsyncCallback(SendAsynMails), null);
        }

        public delegate void delProfileApplicationCompliance(GF_Golfer objReg);
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
        public static void SendAsynMails(GF_Golfer objReg)
        {
            try
            {
                _db = new GolflerEntities();
                IQueryable<GF_EmailTemplatesFields> templateFields = null;
               // var param = EmailParams.GetEmailParams(ref _db, "Registration", ref templateFields);
                long courseid = 0;
                try
                {
                    courseid = Convert.ToInt64(_db.GF_GolferUser.FirstOrDefault(x => x.GolferID == objReg.GF_ID).CourseID);
                }
                catch
                {
                    courseid = 0;
                }
                string mailresult = "";

                var param = EmailParams.GetEmailParamsNew(ref _db, "Registration", ref templateFields, courseid, "G", ref mailresult, true);
                   
                if (param != null)
                {
                    if (!ApplicationEmails.EndUserRegistrationMail(objReg, param, ref templateFields))
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

        #region Async Thread order mails

        public static void callingOrderMails(GF_Golfer objReg, string orderNumber, GF_Order objOrder)
        {
            invProfileApplicationComplianceOrder = new delProfileApplicationComplianceOrder(SendAsynOrderMails);
            invProfileApplicationComplianceOrder.BeginInvoke(objReg,orderNumber, objOrder,new AsyncCallback(SendAsynOrderMails),null);
        }

        public delegate void delProfileApplicationComplianceOrder(GF_Golfer objReg, string orderNumber, GF_Order objOrder);
        public static delProfileApplicationComplianceOrder invProfileApplicationComplianceOrder;

        public static void SendAsynOrderMails(IAsyncResult t)
        {
            List<string> objList = new List<string>();
            try
            {
                invProfileApplicationComplianceOrder.EndInvoke(t);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public static void SendAsynOrderMails(GF_Golfer objReg, string orderNumber, GF_Order objOrder)
        {
            try
            {
                _db = new GolflerEntities();
                IQueryable<GF_EmailTemplatesFields> templateFields = null;
              //  var param = EmailParams.GetEmailParams(ref _db, "Place Order", ref templateFields);
                long courseid = 0;
                try
                {
                    courseid = Convert.ToInt64(_db.GF_GolferUser.FirstOrDefault(x => x.GolferID == objReg.GF_ID).CourseID);
                }
                catch
                {
                    courseid = 0;
                }
                string mailresult = "";

                var param = EmailParams.GetEmailParamsNew(ref _db, "Place Order", ref templateFields, courseid, "G", ref mailresult, true);
                   

                if (param != null)
                {
                    if (!ApplicationEmails.EndUserPlaceOrderMail(objReg, param, ref templateFields, orderNumber, objOrder))
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