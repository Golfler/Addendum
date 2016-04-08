using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CourseWebApi.Models
{
    public class SendRecieveNotification
    {

        #region Push notification calling

        public static void callingPushNotification(PushNotications objPush, bool IsMessageToGolfer)
        {
            // Start
            // Push Notification Code
            invProfileApplicationCompliance = new delProfileApplicationCompliance(CallPushNotify);
            invProfileApplicationCompliance.BeginInvoke(objPush, IsMessageToGolfer, new AsyncCallback(CallPushNotify), null);
        }

        #endregion

        #region PushNotification

        public delegate void delProfileApplicationCompliance(PushNotications objMessage, bool IsGolfer);
        public static delProfileApplicationCompliance invProfileApplicationCompliance;
        public static void CallPushNotify(PushNotications objMessage, bool IsGolfer)
        {
            List<string> lstMsgs = new List<string>();
            lstMsgs.Add("Push Notification Process start.");

            PushNotications objPush = new PushNotications();
            string newmessage = objMessage.Message;
            if (objMessage.DeviceType.ToLower() == "ios")
            {
                lstMsgs.Add("For Apple.");
                objPush.ApplePush(newmessage, objMessage.iosMessageJson, objMessage.ReceiverId, IsGolfer, objMessage.pushMsgFrom, ref lstMsgs);
            }
            else
            {
                lstMsgs.Add("For Android.");
                objPush.AndroidPush(newmessage, objMessage.ReceiverId, IsGolfer, objMessage.pushMsgFrom);
            }
            lstMsgs.Add("Push notification end.");
            LogClass.WriteLog(lstMsgs, "PushNotificationlog.txt");
        }
        public static void CallPushNotify(IAsyncResult t)
        {
            List<string> objList = new List<string>();
            try
            {
                invProfileApplicationCompliance.EndInvoke(t);
            }
            catch (Exception ex)
            {
                //throw ex;
            }
        }

        #endregion
    }
}