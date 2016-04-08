using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using CourseWebApi.Models;

using System.Web;
using Stripe;
using Braintree;
namespace CourseWebApi.Controllers
{
    public class DelieverNPaymentController : ApiController
    {
        #region Deliever N Stripe Payment
        /// <summary>
        /// Created By: Veera
        /// created On: 26 March 2015
        /// Purpose:  Deliever N Stripe Payment

        [HttpPost]
        public OrderResult Payment([FromBody]DelieverNPayment obj)
        {
              // For example, let’s say you’ve created a $100 transaction for one of your sub-merchants. If you specify a $10 service fee, we’ll disburse $90 to the sub-merchant and $10 (less any Braintree transaction fees) to you.
             //If the Braintree transaction fee is greater than the service fee, we’ll first pull the funds from the service fee and then debit your bank account for the remaining balance.

            //if (string.IsNullOrEmpty(obj.Type))
            //{
                if (obj.Type == "1")
                {
                    if ( string.IsNullOrEmpty(obj.OrderId) || string.IsNullOrEmpty(obj.GolferId))
                    {
                        return new OrderResult { Status = 0, Orders = null, Error = "One of the required parameter is missing." };
                    }
                    else
                    {
                         return obj.Charge(Convert.ToInt64(obj.OrderId.Trim()), Convert.ToInt64(obj.GolferId.Trim()));
                    }
                }
                else if (obj.Type == "0")
                {
                    if (string.IsNullOrEmpty(obj.MemberShipId) || string.IsNullOrEmpty(obj.OrderId) || string.IsNullOrEmpty(obj.GolferId) || string.IsNullOrEmpty(obj.Amount))
                    {
                        return new OrderResult { Status = 0, Orders = null, Error = "One of the required parameter is missing." };
                    }
                    else
                    {

                          return obj.ChargeByMemberShipId(obj.MemberShipId, Convert.ToInt64(obj.OrderId.Trim()), Convert.ToInt64(obj.GolferId.Trim()), obj.Amount);
                    }
                } 
                else
                {
                    return new OrderResult { Status = 0, Orders = null, Error = "Type is not valid" };

                }

            //}
            //else
            //{
            //    return new OrderResult { Status = 0, Orders = null, Error = "One of the required parameter is missing." };
            //}
        }

        #endregion

    }
}
