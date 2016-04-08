using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using GolferWebAPI.Models;

namespace GolferWebAPI.Controllers
{
    public class OrderController : ApiController
    {
        GF_Order objOrder = null;
        public OrderController()
        {
            objOrder = new GF_Order();
        }

        /// <summary>
        /// Created By:Arun
        /// Created Date:25 March 2015
        /// Purpose: Place the golfer order.
        /// </summary>
        /// <param name="objorder"></param>
        /// <returns></returns>
        public Result AddGolferOrder([FromBody] GF_Order objorder)
        {
            if (string.IsNullOrEmpty(Convert.ToString(objorder.GolferID))
                  || string.IsNullOrEmpty(objorder.Latitude)
                  || string.IsNullOrEmpty(objorder.Longitude)
                  || string.IsNullOrEmpty(Convert.ToString(objorder.CourseID))
                  || string.IsNullOrEmpty(Convert.ToString(objorder.OrderType)))
            {
                return new Result { Id = 0, Status = 0, record = null, Error = "One of the required parameter is missing." };
            }
            if (objorder.OrderType!=OrderType.TurnOrder && objorder.OrderType!=OrderType.CartOrder )
            {
                return new Result { Id = 0, Status = 0, record = null, Error = "Order Type should be TO and CO." };
            }
            if (objorder.MenuItems == null || objorder.MenuItems.Count == 0)
            {
                return new Result { Id = 0, Status = 0, record = null, Error = "Menu items cann't be empty." };
            }
            if (objorder.PaymentType == "1")
            {
                if (objorder.CardNumber == "" || objorder.CardExpYear == "" || objorder.CardExpMonth == "" || objorder.CardCCV == "")
                {
                    return new Result { Id = 0, Status = 0, record = null, Error = "Card Details cann't be empty." };
                }
            }
            else if (objorder.PaymentType == "0")
            {
                if (objOrder.MemberShipID == "")
                {
                    return new Result { Id = 0, Status = 0, record = null, Error = "MemberShipID cann't be empty." };
                }
            }
            
            return objOrder.AddGolferOrder(objorder);

        }

    }
}
