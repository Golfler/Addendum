using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Linq;
using System.Web;
using Stripe;
namespace Golfler.Models
{
    public class StripePayments
    {

        public string Token { get; set; }
        public string Amount { get; set; }
        public string CustDescription { get; set; }
        public string Email { get; set; }
        public string OrderId { get; set; }
        public string GolferId { get; set; }
        public string Type { get; set; }
        public long GolferWalletId { get; set; }
        public string MemberShipId { get; set; }

        private GolflerEntities _db = null;

        public StripePayments()
        {
            _db = new GolflerEntities();
        }

        public bool RefundCharge(string description, long orderid, string type, decimal amount, ref string errMsg)
        {


            GolflerEntities _db = new GolflerEntities();
            string StripeChargeId = "";
            try
            {
                StripeChargeId = _db.GF_Order.FirstOrDefault(p => p.ID == orderid).Stripe_ChargeId;
                if (StripeChargeId == "")
                {

                    return false;
                }
            }
            catch
            {
                return false;
            }



            string chargeId = ProcessPayment(StripeChargeId, description, type, amount, ref errMsg);

            if ((chargeId) != "")
            {
                try
                {
                    string resultvar = chargeId;
                    if (resultvar.Length > 1)
                    {

                        ////update Order refund  Table
                        GF_OrderRefund objRefund = new GF_OrderRefund();
                        objRefund.CreatedDate = DateTime.Now;
                        objRefund.CreatedBy = LoginInfo.UserId;
                        objRefund.OrderId = orderid;
                        objRefund.RefundAmount = amount;
                        objRefund.RefundType = type;
                        objRefund.Stripe_ChargeId = resultvar;
                        objRefund.Status = "A";
                        objRefund.Description = description;
                        objRefund.StripeMessage = "";
                        _db.GF_OrderRefund.Add(objRefund);
                        _db.SaveChanges();
                        //
                        #region send mail to golfer
                        try
                        {
                            string mailresult = "";
                            string CCMail = "";
                            Int64 courseidForParams = 0;
                            if (!string.IsNullOrEmpty(Convert.ToString(LoginInfo.CourseId)))
                            {
                                courseidForParams = Convert.ToInt64(LoginInfo.CourseId);
                                //CCMail = "amitkumar@cogniter.com";
                            }

                           IQueryable<GF_EmailTemplatesFields> templateFields = null;
                           var param = EmailParams.GetEmailParamsNew(ref _db, EmailTemplateName.OrderRefund, ref templateFields, courseidForParams, LoginInfo.LoginType, ref mailresult);

                            if (mailresult == "") // means Parameters are OK
                            {
                                if (ApplicationEmails.OrderRefund(ref _db, param, objRefund,CCMail,ref templateFields, ref mailresult))
                                {
                                    // Do steps for Mail Send successful
                                }
                                else
                                {
                                    // Do steps for Mail Failure. Mail failure reason can be find on "mailresult"
                                }
                            }
                            else
                            {
                                // Do steps for Parameters not available.Reason can be find on "mailresult"
                            }
                        }
                        catch (Exception ex)
                        {
                            ErrorLibrary.ErrorClass.WriteLog(ex.GetBaseException(), System.Web.HttpContext.Current.Request);
                        }
                        #endregion
                        //
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch
                {
                    return false;

                }

            }
            else
            {
                return false;

            }
        }

        private string ProcessPayment(string StripeChargeId, string description, string type, decimal amount, ref string errMsg)
        {
            string returnstring = "";
            bool? temp = false;
            int? amounttemp = Convert.ToInt32(amount) * 100;//convert to cent 
            try
            {
                var chargeService = new StripeChargeService(ConfigurationManager.AppSettings["StripeApiKey"]);
                var refund = new StripeRefund
               {
                   Amount = Convert.ToInt32(amount) * 100,//convert to cent
                   ChargeId = StripeChargeId,
                   Currency = "usd",
                   Reason = description,

               };
                var stripeRefund = chargeService.Refund(StripeChargeId, amounttemp, temp);
                var stripeRefundId = stripeRefund.Id;
                returnstring = stripeRefundId;

            }
            catch (Exception ex)
            {
                returnstring = "";
                errMsg = ex.Message;
            }
            return returnstring;

        }


    }
}