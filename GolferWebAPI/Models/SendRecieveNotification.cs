using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GolferWebAPI.Models
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
            PushNotications objPush = new PushNotications();
            string newmessage = objMessage.Message;
            if (objMessage.DeviceType.ToLower() == "ios")
            {
                objPush.ApplePush(newmessage, objMessage.iosMessageJson, objMessage.ReceiverId, IsGolfer, objMessage.pushMsgFrom);
            }
            else
            {
                objPush.AndroidPush(newmessage, objMessage.ReceiverId, IsGolfer, objMessage.pushMsgFrom);
            }
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