using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using Golfler.Models;

namespace Golfler
{
    /// <summary>
    /// Summary description for wsReminderMails
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class wsReminderMails : System.Web.Services.WebService
    {

        
        [WebMethod]
        public void ReminderSchedular()
        {
            List<string> msgs = new List<string>();
            msgs.Add("Main Start.");
            msgs.Add("Main Start Time: " + DateTime.Now.ToString());
            try
            {
                Golfler.Models.CommonFunctions.SendCoordinateReminderMails();
                msgs.Add("Main End.");
                msgs.Add("Main End Time: " + DateTime.Now.ToString());
            }
            catch (Exception ex)
            {
                msgs.Add("Exception in Main Start: " + Convert.ToString(ex.Message));
                msgs.Add("Exception Main Time: " + DateTime.Now.ToString());               
            }
            LogClass.MassMessageLog(msgs, "ReminderMails.txt");
        }
    }
}
