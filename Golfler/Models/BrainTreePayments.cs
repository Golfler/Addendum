using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using Braintree;

namespace Golfler.Models
{

    /// <summary>
    /// Created By: Veera 
    /// Creation On: 6 June 2015
    /// Description: BrainTree Payment
    /// </summary>
    
 
    public class BrainTreePayments
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

        public BrainTreePayments()
        {
            _db = new GolflerEntities();
        }

        public bool RefundCharge(string description, long orderid, string type, decimal amount, ref string errMsg)
        {


            GolflerEntities _db = new GolflerEntities();
            string BtChargeId = "";
            try
            {
                BtChargeId = _db.GF_Order.FirstOrDefault(p => p.ID == orderid).BT_TransId;
                if (BtChargeId == "")
                {

                    return false;
                }
            }
            catch
            {
                return false;
            }



            string chargeId = ProcessPayment(BtChargeId, description, type, amount, ref errMsg);

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
                                if (ApplicationEmails.OrderRefund(ref _db, param, objRefund, CCMail, ref templateFields, ref mailresult))
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

        private string ProcessPayment(string ChargeId, string description, string type, decimal amount, ref string errMsg)
        {
            string returnstring = "";

            try
            {
                BraintreeGateway Gateway = new BraintreeGateway
                  {
                      Environment = Braintree.Environment.SANDBOX,
                      PublicKey = ConfigurationManager.AppSettings["BTPublicKey"].ToString(),
                      PrivateKey = ConfigurationManager.AppSettings["BTPrivateKey"].ToString(),
                      MerchantId = ConfigurationManager.AppSettings["BTMerchantId"].ToString()
                  };

                Result<Transaction> result = Gateway.Transaction.Refund(ChargeId, amount);
                if (result.Target != null)
                {
                    returnstring = result.Target.Id;
                    errMsg = result.Message;
                }
                else
                {
                    returnstring = "";
                    errMsg = result.Message;

                }
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