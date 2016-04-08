using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.Collections;
using System.ComponentModel;
using System.Net;
using System.Web.Http.Services;
using PushSharp.Apple;
using PushSharp.Core;
using PushSharp;
using PushSharp.Android;
using ErrorLibrary;

using GolferWebAPI.Models;
namespace GolferWebAPI.Models
{
    public class PushNotications
    {
        public static string PushMessage { get; set; }
        public static string PushDevice { get; set; }

        private GolflerEntities _db = null;
        public PushNotications()
        {
            _db = new GolflerEntities();
        }
        public long SenderId { get; set; }
        public long ReceiverId { get; set; }
        public string DeviceType { get; set; }
        public string Message { get; set; }
        public string SenderName { get; set; }
        public string ReceiverName { get; set; }
        public string iosMessageJson { get; set; }
        public string pushMsgFrom { get; set; }
        /// <summary>


        /// Message = String value which we want to push on Device as Message
        /// UserID = Primary key of Users Table. By using this UserID, we will get Device ID of the user on which we want to send push notification. 

        public void ApplePush(string Message, string iosMessageJson, long UserID, bool IsGolfer, string pushMsgFrom)
        {
            try
            {
                bool isPushSend = true;
                string regapnId = getApnToken(UserID, IsGolfer, pushMsgFrom, ref isPushSend);  // Get DeviceUUID 
                if (isPushSend)
                {
                    if (regapnId != "" && regapnId != null)
                    {
                        //IOS Push
                        var push = new PushBroker();
                        push.OnNotificationSent += NotificationSent;
                        push.OnChannelException += ChannelException;
                        push.OnServiceException += ServiceException;
                        push.OnNotificationFailed += NotificationFailed;
                        push.OnDeviceSubscriptionExpired += DeviceSubscriptionExpired;
                        push.OnDeviceSubscriptionChanged += DeviceSubscriptionChanged;
                        push.OnChannelCreated += ChannelCreated;
                        push.OnChannelDestroyed += ChannelDestroyed;
                        var appleCert = File.ReadAllBytes(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigClass.IPHONE_CERT));
                        PushMessage = Message;
                        PushDevice = "ios";
                        // NOTES:
                        // Put Certificate Name and Password in Web Config.
                        //"Certificates.p12" generated from Apple provided by iphone team
                        //"123456" password for certificate provided by iphone team
                        //  int badge = RM_Registration.GetBadgeNumber(UserID);
                        push.RegisterAppleService(new ApplePushChannelSettings(true, appleCert, ConfigClass.CERT_PASSWORD, true), new PushServiceSettings() { AutoScaleChannels = false, Channels = 1, MaxAutoScaleChannels = 1, MaxNotificationRequeues = 100, NotificationSendTimeout = 5000 });
                        push.QueueNotification(new AppleNotification()
                                                   .ForDeviceToken(regapnId)
                                                   .WithAlert(Message)
                                                   .WithSound("sound.caf")
                                                   .WithCategory(iosMessageJson)
                            // .WithBadge(badge)
                                                  );
                        //Stop and wait for the queues to drains
                        push.StopAllServices();

                    }
                    else
                    {
                    }
                }
            }

            catch (Exception ex)
            {

                string fileName = string.Empty;
                fileName = "Error.txt";
                string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigClass.ErrorFilePath);
                StreamWriter w;

                w = File.Exists(filePath) ? File.AppendText(filePath) : File.CreateText(filePath);

                w.WriteLine("---------------------------------------------------------------------");
                w.WriteLine(DateTime.Now);
                w.WriteLine("---------------------------------------------------------------------");

                w.WriteLine("Golfer PushMessage: " + ex.Message);
                w.WriteLine("Device: " + "Apple Push error");

                w.WriteLine("---------------------------------------------------------------------");
                w.Flush();
                w.Close();
            }

        }
        public void AndroidPush(string Message, long UserID, bool IsGolfer, string pushMsgFrom)
        {
            try
            {
                bool isPushSend = true;
                string regId = getGCMID(UserID, IsGolfer, pushMsgFrom, ref isPushSend);
                //  regId = "7a237f07dc3aa59364638bfbc784b22e6bbe7805c06aba84fc922f60936e0624";
                // Example regId format "APA91bG3oH9Dyaq0wfjr1o7p9MhKRBPNHBOOEcg9j63JbtpMs4_ir7SCKlIzzgs6QtWgD6IHzYq-0m7D9o3yxhZwIfmF2y9ku1yDGzde8Wp9UXfwOZ3rKLw5pPRJDcdRJGnCt8f5eLVhFhwtmZKXisPbDfL7PJa87w";
                if (isPushSend)
                {
                    if (regId != "" && regId != null)
                    {

                        var applicationID = ConfigClass.GOOGLE_API_KEY;                             //SERVER ip Apkey generated from google             
                        var SENDER_ID = ConfigClass.GOOGLE_PROJECT_ID; //Project Id from Google                

                        PushMessage = Message;
                        PushDevice = "android";
                        //Create our push services broker
                        var push = new PushBroker();
                        //Wire up the events for all the services that the broker registers
                        push.OnNotificationSent += NotificationSent;
                        push.OnChannelException += ChannelException;
                        push.OnServiceException += ServiceException;
                        push.OnNotificationFailed += NotificationFailed;
                        push.OnDeviceSubscriptionExpired += DeviceSubscriptionExpired;
                        push.OnDeviceSubscriptionChanged += DeviceSubscriptionChanged;
                        push.OnChannelCreated += ChannelCreated;
                        push.OnChannelDestroyed += ChannelDestroyed;
                        push.RegisterGcmService(new GcmPushChannelSettings(applicationID));
                        //push.QueueNotification(new GcmNotification().ForDeviceRegistrationId(regId).WithJson("{\"message\":\"" + Message + "\",\"badge\":\"1\"}"));
                        push.QueueNotification(new GcmNotification().ForDeviceRegistrationId(regId).WithJson("{\"message\":" + Message + ",\"badge\":\"1\"}"));
                        //Stop and wait for the queues to drains
                        push.StopAllServices();

                    }
                }
            }
            catch (Exception ex)
            {
                //   ErrorLibrary.ErrorClass.WriteLog(ex, HttpContext.Current.Request);
                string fileName = string.Empty;
                fileName = "Error.txt";
                string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigClass.ErrorFilePath);
                StreamWriter w;

                w = File.Exists(filePath) ? File.AppendText(filePath) : File.CreateText(filePath);

                w.WriteLine("---------------------------------------------------------------------");
                w.WriteLine(DateTime.Now);
                w.WriteLine("---------------------------------------------------------------------");

                w.WriteLine("Golfer PushMessage: " + ex.Message);
                w.WriteLine("Device: " + "AndroidPush error");
                //w.WriteLine("Data: " + );
                w.WriteLine("---------------------------------------------------------------------");
                w.Flush();
                w.Close();
            }
        }

        //Currently it will raise only for android devices
        public static void DeviceSubscriptionChanged(object sender, string oldSubscriptionId, string newSubscriptionId, INotification notification)
        {
            //Do something here
        }

        //this even raised when a notification is successfully sent
        public static void NotificationSent(object sender, INotification notification)
        {
            //Do something here
            WritePushNotificationErrorLog("Sent", null, PushMessage, PushDevice);

        

        }

        //this is raised when a notification is failed due to some reason
        public static void NotificationFailed(object sender, INotification notification, Exception notificationFailureException)
        {
            //Do something here
            WritePushNotificationErrorLog(notificationFailureException.Message.ToString(), notificationFailureException, PushMessage, PushDevice);


        }

        //this is fired when there is exception is raised by the channel
        public static void ChannelException(object sender, IPushChannel channel, Exception exception)
        {
            //Do something here
        }

        //this is fired when there is exception is raised by the service
        public static void ServiceException(object sender, Exception exception)
        {
            //Do something here
        }

        //this is raised when the particular device subscription is expired
        public static void DeviceSubscriptionExpired(object sender, string expiredDeviceSubscriptionId, DateTime timestamp, INotification notification)
        {
            //Do something here
        }

        //this is raised when the channel is destroyed
        public static void ChannelDestroyed(object sender)
        {
            //Do something here
        }

        //this is raised when the channel is created
        public static void ChannelCreated(object sender, IPushChannel pushChannel)
        {
            //Do something here
        }


        // Used for Android
        public string getGCMID(long userid, bool isGolfer, string pushMsgFrom, ref bool isPushSend)
        {

            string returnValue = "";
            if (isGolfer)
            {
                var enduser = _db.GF_Golfer.FirstOrDefault(x => x.GF_ID == userid);// && x.IsPushNotify==true);
                if (enduser != null)
                {
                    #region Check if golfer user is Opt out to recieve Push Notification

                    //Check if push notification is sent by course
                    if (pushMsgFrom == PushnoficationMsgFrom.Course)
                    {
                        //Check if golfer user opt out to recieve push notification
                        isPushSend = enduser.IsReceivePushNotification ?? true;
                    }
                    else //Else if push notification is sent by golfer
                    {
                        isPushSend = enduser.IsReceivePushNotificationGolfer ?? true;
                    }

                    #endregion

                    returnValue = enduser.GCMID;
                }
            }
            else
            {

                var enduser = _db.GF_AdminUsers.FirstOrDefault(x => x.ID == userid);
                if (enduser != null)
                {
                    isPushSend = true;//Send defailt true, in case push notification sent to course.
                    returnValue = enduser.GCMID;
                }
            }
            return returnValue;
        }


        /// Get DeviceUUID of user from database  [For Apple ]
        public string getApnToken(long userid, bool isGolfer, string pushMsgFrom, ref bool isPushSend)
        {

            string returnValue = "";
            if (isGolfer)
            {
                var enduser = _db.GF_Golfer.FirstOrDefault(x => x.GF_ID == userid);//  && x.IsPushNotify == true);
                if (enduser != null)
                {
                    #region Check if golfer user is Opt out to recieve Push Notification

                    //Check if push notification is sent by course
                    if (pushMsgFrom == PushnoficationMsgFrom.Course)
                    {
                        //Check if golfer user opt out to recieve push notification
                        isPushSend = enduser.IsReceivePushNotification ?? true;
                    }
                    else //Else if push notification is sent by golfer
                    {
                        isPushSend = enduser.IsReceivePushNotificationGolfer ?? true;
                    }

                    #endregion

                    returnValue = enduser.APNID;
                }
            }
            else
            {
                var enduser = _db.GF_AdminUsers.FirstOrDefault(x => x.ID == userid);
                if (enduser != null)
                {
                    isPushSend = true;//Send defailt true, in case push notification sent to course.
                    returnValue = enduser.APNID;
                }
            }
            // else
            // {
            //     // We are using this string only for testing.
            //     returnValue = "7a237f07dc3aa59364638bfbc784b22e6bbe7805c06aba84fc922f60936e0624"
            //// "6d37a99827da47ef2a675f426d560733658b09cc91d96c48917849e68ec19d32";//ipad air

            // }
            return returnValue;
        }

        #region PushNotification Error and Success Log Methods

        public static string WritePushNotificationErrorLog(string result, Exception ex, string msg, string type)
        {
            string strResult = "";
            string RequestFilePath = "";
            bool RequestLogMode;
            try
            {
                if (Convert.ToString(System.Web.Configuration.WebConfigurationManager.AppSettings["ERROR_LOG_MODE"]) == "1")
                {
                    RequestLogMode = true;
                }
                else
                {
                    RequestLogMode = false;
                }
                RequestFilePath = Convert.ToString(System.Web.Configuration.WebConfigurationManager.AppSettings["Error_FilePath"]);

                if (RequestLogMode)
                {
                    string fileName = string.Empty;
                    fileName = "Error.txt";
                    string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigClass.ErrorFilePath);
                    StreamWriter w;

                    w = File.Exists(filePath) ? File.AppendText(filePath) : File.CreateText(filePath);

                    w.WriteLine("---------------------------------------------------------------------");
                    w.WriteLine(DateTime.Now);
                    w.WriteLine("---------------------------------------------------------------------");
                    if (ex != null)
                    {
                        w.WriteLine("Message: " + ex.Message);
                        w.WriteLine("InnerException: " + ex.InnerException);
                        w.WriteLine("StackTrace: " + ex.StackTrace);
                        w.WriteLine("TargetSite: " + ex.TargetSite);
                    }
                    w.WriteLine("PushMessage: " + msg);
                    w.WriteLine("Device: " + type);
                    //w.WriteLine("Data: " + );
                    w.WriteLine("---------------------------------------------------------------------");
                    w.Flush();
                    w.Close();
                    strResult = "Log file created successfully";

                }
            }
            catch (Exception exp)
            {
                strResult = "Exception: " + exp.InnerException;
                strResult += "Message: " + exp.Message;
            }
            return strResult;
        }

        #endregion

    }
}