using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using GolferWebAPI.Models;

namespace GolferWebAPI.Controllers
{
    public class MessagesController : ApiController
    {
        Messages obj = null;

        public MessagesController()
        {
            obj = new Messages();
        }


        #region Send Message
        #region Async Call
        public delegate void delUpdatePlay(GF_Messages msg);
        public static delUpdatePlay invUpdatePlay;

        public static void CallbackDelegatedFunctions(IAsyncResult t)
        {
            try
            {
                invUpdatePlay.EndInvoke(t);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void CallDelegatedFunctions(GF_Messages message)
        {
            Messages objmsg = new Messages();
            objmsg.SaveMessage(message);
        }
        #endregion
        /// <summary>
        /// Created By: Veera
        /// Created Date: 25 March 2015
        /// </summary>
        /// <param name="message">Parameters MsgFrom,MsgTo,Message</param>
        /// <returns>Status,Error</returns>

        public Base SendMessage(GF_Messages message)
        {
            if (message.MsgFrom == null || message.MsgToList == null || string.IsNullOrEmpty(message.IsMessagesFromGolfer) || string.IsNullOrEmpty(message.IsMessagesToGolfer))
            {
                return new Base { Error = "One of the required field is empty.", Status = 0 };
            }
            if (message.MsgTo == 0 || message.MsgFrom == 0)
            {
                return new Base { Error = "MsgTo and MsgFrom id's cannot be 0.", Status = 0 };
            }

            #region Gophie/Golfer Check

            //Below functionality is used to check weather the gophie and golfer can send message to each other or not

            GolflerEntities _db = new GolflerEntities();
            DateTime TodaysDate = Convert.ToDateTime(DateTime.UtcNow).Date;
            long msgTo = 0;
            try
            {
                msgTo = Convert.ToInt32(message.MsgToList);
            }
            catch
            {
                msgTo = 0;
            }

            //If Message sent by Course User then check wheather the sender is cartie(gophie) or not
            //And second thing reciever should be a Golfer user
            if (message.IsMessagesFromGolfer == "0" && message.IsMessagesToGolfer == "1")
            {
                var gophieCheck = _db.GF_AdminUsers.FirstOrDefault(x => x.ID == message.MsgFrom &&
                    (x.Type == UserType.Cartie || x.Type == UserType.PowerAdmin));

                //Check wheather the gophie have any active order with the golfer or not
                //If he/she have any active order then they can communicate with each other
                //Else they are not authorised to communicate with each other
                if (gophieCheck != null)
                {
                    var activeOrder = _db.GF_Order.Where(x => (x.CartieId ?? 0) == (message.MsgFrom ?? 0) &&
                        (x.GolferID ?? 0) == msgTo &&
                        System.Data.Objects.EntityFunctions.TruncateTime(x.OrderDate) == TodaysDate &&
                        !(x.IsDelivered ?? false) && !(x.IsPickup ?? false) &&
                        !(x.IsRejected ?? false));

                    //Check if their is an active order or not
                    if (activeOrder.Count() <= 0)
                    {
                        return new Base { Status = 0, Error = "You are not allowed to initiate chat with this person." };
                    }

                    //Else do nothing
                }
            }

            //If Message sent by Golfer user then check wheather the reciever is Course User or not
            //And second thing reciever should be a cartie(gophie) user
            if (message.IsMessagesFromGolfer == "1" && message.IsMessagesToGolfer == "0")
            {
                var gophieCheck = _db.GF_AdminUsers.FirstOrDefault(x => x.ID == msgTo && 
                    (x.Type == UserType.Cartie || x.Type == UserType.PowerAdmin));

                if (gophieCheck != null)
                {
                    var activeOrder = _db.GF_Order.Where(x => (x.CartieId ?? 0) == msgTo &&
                        (x.GolferID ?? 0) == (message.MsgFrom ?? 0) &&
                        System.Data.Objects.EntityFunctions.TruncateTime(x.OrderDate) == TodaysDate &&
                        !(x.IsDelivered ?? false) && !(x.IsPickup ?? false) &&
                        !(x.IsRejected ?? false));

                    //Check if their is an active order or not
                    if (activeOrder.Count() <= 0)
                    {
                        return new Base { Status = 0, Error = "You are not allowed to initiate chat with this person." };
                    }

                    //Else do nothing
                }
            }

            #endregion

            #region SaveMessage Async

            invUpdatePlay = new delUpdatePlay(CallDelegatedFunctions);
            invUpdatePlay.BeginInvoke(message, new AsyncCallback(CallbackDelegatedFunctions), null);

            #endregion

            return new Base { Status = 1, Error = "Message send successfully." };
        }

        #endregion

        #region Get Message Listing
        /// <summary>
        /// Created Date:26 March 2015
        /// Created by :veera
        /// </summary>
        /// <param name="msg"></param>
        /// <returns>Message Listing,status,error </returns>
        [HttpPost]
        public MessageResult GetMessageListing([FromBody] MsgObject msg)
        {
            if (msg.MsgFrom == null || msg.IsMessagesFromGolfer == null || msg.PgNo == null || msg.Offset == null || msg.Timezone == null)
            {
                return new MessageResult { Error = "One of the required field is empty.", Status = 0 };
            }
            if (msg.MsgFrom == 0)
            {
                return new MessageResult { Error = "Values should be greater than 0.", Status = 0 };
            }
            return obj.GetMessageListing(msg);
        }

        #endregion

        #region Get Person Wise Message Listing
        /// <summary>
        /// Created Date:19 May 2015
        /// Created by :veera
        /// </summary>
        /// <param name="msg"></param>
        /// <returns>Message Listing,status,error </returns>
        [HttpPost]
        public MessageResult GetPersonMessageListing([FromBody] MsgObject msg)
        {
            if (msg.MsgFrom == null || msg.IsMessagesFromGolfer == null)
            {
                return new MessageResult { Error = "One of the required field is empty.", Status = 0 };
            }
            if (msg.MsgFrom == 0)
            {
                return new MessageResult { Error = "Values should be greater than 0.", Status = 0 };
            }
            return obj.GetPesronMessageListing(msg);
        }

        #endregion

        #region Update Message Read/UnRead Status
        /// <summary>
        /// Created By:Veera
        /// Created Date:27 March. 2015
        /// Purpose: Update Message status
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        [HttpPost]
        public Base UpdateMessageStatus(GF_Messages message)
        {
            if (message == null)
            {
                return new Base { Error = "One of the required field is empty.", Status = 0 };
            }
            return obj.UpdatemessageStatus(message);
        }
        #endregion

        #region Get Contact user's Count having unread messages
        /// <summary>
        /// Created By:Veera
        /// Created Date:27 March. 2015
        /// Purpose: Get unread messages count group by userid
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        [HttpPost]
        public MessageCountResult GetUnreadMessageCount(GF_Messages message)
        {
            if (message.MsgTo == null || message.IsMessagesToGolfer == null)
            {
                return new MessageCountResult { Error = "One of the required field is empty.", Status = 0 };
            }
            return obj.UserUnreadMessages(message);
        }


        #endregion
    }
}
