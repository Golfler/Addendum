using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Linq;

using System.Web;
using Stripe;
using Braintree;
using CourseWebAPI.Models;
namespace CourseWebApi.Models
{
    public class DelieverNPayment
    {

        public string Token { get; set; }
        public int GolflerAmount { get; set; }
        public int CourseAmount { get; set; }
        public string Amount { get; set; }

        public string CustDescription { get; set; }
        public string Email { get; set; }
        public string OrderId { get; set; }
        public string GolferId { get; set; }
        public string Type { get; set; }
        public long GolferWalletId { get; set; }
        public string MemberShipId { get; set; }
        public string CourseToken { get; set; }
        public long CourseId { get; set; }
        public string CardNumber { get; set; }
        public string CardExpMonth { get; set; }
        public string CardExpYear { get; set; }
        public string CardCCV { get; set; }
        private GolflerEntities _db = null;
        public Decimal BTGolflerAmount { get; set; }
        public Decimal BTCourseAmount { get; set; }
        public DelieverNPayment()
        {
            _db = new GolflerEntities();
        }

        #region Stripe

        public OrderResult ChargeStripe(long orderid, long GolferId)
        {

            DelieverNPayment model = new DelieverNPayment();
            GolflerEntities _db = new GolflerEntities();
            var objOrder = _db.GF_Order.FirstOrDefault(x => x.ID == orderid);
            Order obj = new Order();

            decimal GolflerAmount = 0;
            decimal CourseAmount = 0;
            if (!(obj.getOrderSplitAmount(orderid, ref GolflerAmount, ref CourseAmount)))
            {
                return new OrderResult { Status = 0, Orders = null, Error = "Some Error Occur." };
            }
            model.GolflerAmount = Convert.ToInt32(Convert.ToString(GolflerAmount).Replace(".", ""));//convert to cents
            model.CourseAmount = Convert.ToInt32(Convert.ToString(CourseAmount).Replace(".", ""));//convert to cents
            model.Token = objOrder.StripeToken;//"tok_15jRNTG5NdHlgU66typyYraz";
            model.CourseToken = objOrder.StripeCourseToken;
            model.CourseId = Convert.ToInt64(objOrder.CourseID);
            string Email = "";
            try
            {
                Email = _db.GF_Golfer.FirstOrDefault(p => p.GF_ID == GolferId).Email;
                if (Email == "")
                {

                    return new OrderResult { Status = 0, Orders = null, Error = "Golfer does not exists." };
                }
            }
            catch
            {
                return new OrderResult { Status = 0, Orders = null, Error = "Golfer does not exists." };
            }

            model.Email = Email;
            model.CustDescription = "Golfer Customer";
            #region  Payment
            string chargeId = ProcessPayment(model);

            // You should do something with the chargeId --> Persist it maybe?
            if ((chargeId) != "")
            {
                try
                {
                    string[] resultvar = chargeId.Split('~');
                    if (resultvar.Length > 1)
                    {
                        #region  Order Table
                        ////update Order Table
                        var objorder = _db.GF_Order.FirstOrDefault(p => p.ID == orderid);
                        if (objorder != null)
                        {
                            objorder.IsDelivered = true;
                            objorder.PaymentStatus = "1";
                            objorder.Stripe_ChargeId = resultvar[0];
                            objorder.Stripe_CustomerId = resultvar[1];
                            objorder.Stripe_Course_ChargeId = resultvar[2];
                            _db.SaveChanges();
                            var lstOrders = _db.GF_Order.Where(x => x.ID == orderid).ToList()
                                   .Select(x =>
                                       new
                                       {
                                           orderID = x.ID,
                                           courseID = x.CourseID,
                                           golferName = x.GF_Golfer.FirstName + " " + x.GF_Golfer.LastName,
                                           x.Longitude,
                                           x.Latitude,
                                           time = x.OrderDate.Value.ToShortTimeString(),
                                           itemsOrdered = x.GF_OrderDetails.ToList().Select(y =>
                                               new
                                               {
                                                   y.GF_MenuItems.Name,
                                                   UnitPrice = y.GF_MenuItems.Amount,
                                                   y.Quantity,
                                                   Amount = (y.GF_MenuItems.Amount * y.Quantity)
                                               }),
                                           billAmount = x.GF_OrderDetails.Sum(y => (y.GF_MenuItems.Amount * y.Quantity)),
                                           x.TaxAmount,
                                           x.GolferPlatformFee,
                                           Total = ((x.GF_OrderDetails.Sum(y => (y.GF_MenuItems.Amount * y.Quantity))) + x.TaxAmount + x.GolferPlatformFee),
                                           x.OrderType,
                                           x.OrderDate,
                                           ReadyStatus = (x.KitchenId > 0) ? 1 : 0,
                                           x.HEXColor,
                                           x.RGBColor,
                                           x.HUEColor
                                       }).OrderByDescending(x => x.orderID);
                            //resulttype = 4;
                            return new OrderResult { Status = 1, Orders = lstOrders, Error = "Payment Successful." };
                        }
                        else
                        {
                            //  resulttype = 5;
                            return new OrderResult { Status = 0, Orders = null, Error = "Payment successful, but error occur in order updation." };
                        }
                        #endregion
                    }
                    else
                    {
                        //  resulttype = 1;
                        return new OrderResult { Status = 0, Orders = null, Error = "Payment successful, but error occur in order updation." };
                    }

                }
                catch
                {
                    //resulttype = 2;
                    return new OrderResult { Status = 0, Orders = null, Error = "Payment successful, but error occur in Order updation." };

                }

            }
            else
            {
                //resulttype = 3;
                return new OrderResult { Status = 0, Orders = null, Error = "payment unsuccessful from stripe payment gateway." };
            }


            #endregion





        }

        private string ProcessPayment(DelieverNPayment model)
        {
            string returnstring = "";
            try
            {
                #region Stripe accounts info

                string StripeApiKeyGolfler = _db.GF_Settings.Where(x => x.CourseID == 0 && x.Name == "StripeSecretApiKey").FirstOrDefault().Value;


                #endregion
                var myCharge = new StripeChargeCreateOptions
                {

                    //  Amount = model.GolflerAmount,
                    Currency = "usd",
                    Description = "GolflerAmount",
                    CardId = model.Token

                };

                var chargeService = new StripeChargeService(StripeApiKeyGolfler);
                var stripeCharge = chargeService.Create(myCharge);
                //Create Customer
                var customer = new StripeCustomerCreateOptions
                {
                    Email = model.Email,
                    Description = model.CustDescription

                };
                var customerService = new StripeCustomerService(StripeApiKeyGolfler);
                var createCustomer = customerService.Create(customer);
                var createdCustomerId = createCustomer.Id;
                // Course Charge
                var myChargeCourse = new StripeChargeCreateOptions
                {

                    //Amount = model.CourseAmount,
                    Currency = "usd",
                    Description = "CourseAmount",
                    CardId = model.CourseToken

                };
                string StripeApiKeyCourse = "";
                try
                {
                    StripeApiKeyCourse = _db.GF_Settings.Where(x => x.CourseID == model.CourseId && x.Name == "StripeSecretApiKey").FirstOrDefault().Value;

                    var CoursechargeService = new StripeChargeService(StripeApiKeyCourse);

                    var stripeCourseCharge = CoursechargeService.Create(myChargeCourse);
                    returnstring = stripeCharge.Id + "~" + createdCustomerId + "~" + stripeCourseCharge.Id;
                }

                catch
                {
                    returnstring = stripeCharge.Id + "~" + createdCustomerId + "~" + "0";
                }

            }
            catch (Exception ex)
            {
                returnstring = "";
            }
            return returnstring;

        }
        #endregion

        #region Card Braintree
        public OrderResult Charge(long orderid, long GolferId)
        {

            DelieverNPayment model = new DelieverNPayment();
            GolflerEntities _db = new GolflerEntities();

            var objOrder = _db.GF_Order.FirstOrDefault(x => x.ID == orderid);
            Order obj = new Order();

            decimal GolflerAmount = 0;
            decimal CourseAmount = 0;

            if (!(obj.getOrderSplitAmount(orderid, ref GolflerAmount, ref CourseAmount)))
            {
                return new OrderResult { Status = 0, Orders = null, Error = "Some Error Occur." };
            }

            model.BTGolflerAmount = GolflerAmount;
            model.BTCourseAmount = CourseAmount;
            model.OrderId = Convert.ToString(orderid);

            model.CardNumber = Convert.ToString(CommonFunctions.DecryptUrlParam(objOrder.CardNumber));
            model.CardExpMonth = Convert.ToString(CommonFunctions.DecryptUrlParam(objOrder.CardExpMonth));
            model.CardExpYear = Convert.ToString(CommonFunctions.DecryptUrlParam(objOrder.CardExpYear));
            model.CardCCV = Convert.ToString(CommonFunctions.DecryptUrlParam(objOrder.CardCCV));

            model.CourseId = Convert.ToInt64(objOrder.CourseID);
            string Email = "";
            try
            {
                var objGolfer = _db.GF_Golfer.FirstOrDefault(p => p.GF_ID == GolferId);
                Email = objGolfer.Email;

                if (Email == "")
                {

                    return new OrderResult { Status = 0, Orders = null, Error = "Golfer does not exists." };
                }
            }
            catch
            {
                return new OrderResult { Status = 0, Orders = null, Error = "Golfer does not exists." };
            }

            model.Email = Email;

            if (!string.IsNullOrEmpty(Convert.ToString(objOrder.BT_TransId)))
            {
                return new OrderResult { Status = 0, Orders = null, Error = "Payment has already been successful." };
            }
            string BTResponse = "";
            #region  Payment
            List<string> lstMsg=new List<string>();
            string chargeId = ProcessBTPayment(model, ref BTResponse,ref lstMsg);

            // You should do something with the chargeId --> Persist it maybe?
            if ((chargeId) != "")
            {
                try
                {
                    string[] resultvar = chargeId.Split('~');
                    if (resultvar.Length == 1)
                    {
                        #region  Order Table
                        ////update Order Table
                        var objorder = _db.GF_Order.FirstOrDefault(p => p.ID == orderid);
                        if (objorder != null)
                        {
                            objorder.IsDelivered = true;
                            objorder.PaymentStatus = "1";
                            objorder.BT_TransId = chargeId;
                            _db.SaveChanges();

                            var lstOrders = _db.GF_Order.Where(x => x.ID == orderid).ToList()
                                   .Select(x =>
                                       new
                                       {
                                           orderID = x.ID,
                                           courseID = x.CourseID,
                                           golferName = x.GF_Golfer.FirstName + " " + x.GF_Golfer.LastName,
                                           x.Longitude,
                                           x.Latitude,
                                           time = x.OrderDate.Value.ToShortTimeString(),
                                           itemsOrdered = x.GF_OrderDetails.ToList().Select(y =>
                                               new
                                               {
                                                   y.GF_MenuItems.Name,
                                                   UnitPrice = y.GF_MenuItems.Amount,
                                                   y.Quantity,
                                                   Amount = (y.GF_MenuItems.Amount * y.Quantity)
                                               }),
                                           billAmount = x.GF_OrderDetails.Sum(y => (y.GF_MenuItems.Amount * y.Quantity)),
                                           x.TaxAmount,
                                           x.GolferPlatformFee,
                                           Total = ((x.GF_OrderDetails.Sum(y => (y.GF_MenuItems.Amount * y.Quantity))) + x.TaxAmount + x.GolferPlatformFee),
                                           x.OrderType,
                                           x.OrderDate,
                                           ReadyStatus = (x.KitchenId > 0) ? 1 : 0,
                                           x.HEXColor,
                                           x.RGBColor,
                                           x.HUEColor
                                       }).OrderByDescending(x => x.orderID);
                            //resulttype = 4;
                            return new OrderResult { Status = 1, Orders = lstOrders, Error = "Payment Successful." };
                        }
                        else
                        {
                            //  resulttype = 5;
                            return new OrderResult { Status = 0, Orders = null, Error = "Payment successful, but error occur in order updation." };
                        }
                        #endregion
                    }
                    else
                    {
                        //  resulttype = 1;
                        return new OrderResult { Status = 0, Orders = null, Error = "Payment successful, but error occur in order updation." };
                    }

                }
                catch
                {
                    //resulttype = 2;
                    return new OrderResult { Status = 0, Orders = null, Error = "Payment successful, but error occur in Order updation." };

                }

            }
            else
            {
                //resulttype = 3;
                return new OrderResult { Status = 0, Orders = null, Error = "payment unsuccessful from bratinTree payment gateway." };
            }


            #endregion





        }
        private string ProcessBTPayment(DelieverNPayment model, ref string btResponse, ref List<string> lstMsg)
        {
            string returnstring = "";
            _db = new GolflerEntities();
            try
            {

                string number = model.CardNumber;
                string cvv = model.CardCCV;
                string month = model.CardExpMonth;
                string year = model.CardExpYear;

                #region Get Course Club House ID

                var ClubHouse = _db.GF_CourseInfo.FirstOrDefault(x => x.ID == model.CourseId && !(x.IsClubHouse ?? true));
                long newCourseID = model.CourseId;
                if (ClubHouse != null)
                {
                    newCourseID = ClubHouse.ClubHouseID ?? 0;
                }

                #endregion

                string BTSubMerchantId = "";
                //if (Convert.ToString(System.Web.Configuration.WebConfigurationManager.AppSettings["PAYMENT_MODE"]) == "L")
                //{
                BTSubMerchantId = Convert.ToString(_db.GF_Settings.Where(x => x.CourseID == newCourseID &&
                        x.Name == "BTSubMerchantId").FirstOrDefault().Value);

                //}
                //else
                //{
                //    BTSubMerchantId = "rkalrasgolfcourse_instant_32x3vtr2";
                //}

                if (string.IsNullOrEmpty(Convert.ToString(BTSubMerchantId)))
                {
                    returnstring = "";
                }
                else
                {

                    string strPublicKey = "";
                    string strPrivateKey = "";
                    string strMerchantId = "";

                    Braintree.Environment strEnviroment = Braintree.Environment.SANDBOX;
                    if (Convert.ToString(System.Web.Configuration.WebConfigurationManager.AppSettings["PAYMENT_MODE"]) == "L")
                    {
                        lstMsg.Add("live");
                        strEnviroment = Braintree.Environment.PRODUCTION;
                        strPublicKey = Convert.ToString(ConfigurationManager.AppSettings["BTPublicKey_Live"]);
                        strPrivateKey = Convert.ToString(ConfigurationManager.AppSettings["BTPrivateKey_Live"]);
                        strMerchantId = Convert.ToString(ConfigurationManager.AppSettings["BTMerchantId_Live"]);
                    }
                    else
                    {
                        lstMsg.Add("test");
                        strEnviroment = Braintree.Environment.SANDBOX;
                        strPublicKey = Convert.ToString(ConfigurationManager.AppSettings["BTPublicKey"]);
                        strPrivateKey = Convert.ToString(ConfigurationManager.AppSettings["BTPrivateKey"]);
                        strMerchantId = Convert.ToString(ConfigurationManager.AppSettings["BTMerchantId"]);
                    }
                    BraintreeGateway Gateway = new BraintreeGateway
                      {
                          Environment = strEnviroment,
                          PublicKey = strPublicKey,
                          PrivateKey = strPrivateKey,
                          MerchantId = strMerchantId
                      };

                    try
                    {
                        var request = new TransactionRequest
                        {
                            MerchantAccountId = BTSubMerchantId,///sub merchant id
                            OrderId = model.OrderId,

                            Amount = model.BTGolflerAmount + model.BTCourseAmount,//15.00M, (this amount -ServiceFeeAmount) goes to sub merchant. in our case it will be course admin n enter a course admin share in this
                            CreditCard = new TransactionCreditCardRequest
                            {
                                Number = number,
                                CVV = cvv,
                                ExpirationMonth = month,
                                ExpirationYear = year,
                            },
                            Options = new TransactionOptionsRequest
                            {
                                SubmitForSettlement = true
                            },
                            ServiceFeeAmount = model.BTGolflerAmount///this amount goes to master merchant. in our case it will be super admin n enter a super admin share in this
                        };

                        Result<Transaction> result = Gateway.Transaction.Sale(request);
                        lstMsg.Add("ProcessorResponseCode: " + Convert.ToString(result.Target.ProcessorResponseCode));
                        if (result.Target.ProcessorResponseCode == "1000" && result.Target.ProcessorResponseText == "Approved")
                        {
                            lstMsg.Add("Result String: "+ Convert.ToString(result.Target.Id));
                            returnstring = result.Target.Id;
                            btResponse = result.Target.ProcessorResponseText;
                        }
                        else
                        {

                            returnstring = "";
                            btResponse = result.Target.ProcessorResponseText;
                        }
                        lstMsg.Add("ProcessorResponseCode: " + Convert.ToString(result.Target.ProcessorResponseText));
                    }

                    catch(Exception ex)
                    {
                        btResponse = ex.Message;
                    }
                }
            }
            catch (Exception ex)
            {
                lstMsg.Add("Exception: " + Convert.ToString(ex.Message));
                btResponse = ex.Message;
            }
            return returnstring;
        }



        public OrderResult ChargeByMemberShipId(string membershipId, long orderid, long GolferId, string amount)
        {

            DelieverNPayment model = new DelieverNPayment();
            GolflerEntities _db = new GolflerEntities();
            //  model.GolflerAmount = amount;
            long golferID = 0; int golferWalletIDCount = 0;
            try
            {
                golferID = _db.GF_Golfer.FirstOrDefault(p => p.GF_ID == GolferId).GF_ID;
                if (golferID == 0)
                {
                    return new OrderResult { Status = 0, Orders = null, Error = "Golfer does not exists." };
                }
                else
                {
                    golferWalletIDCount = _db.GF_GolferWallet.Where(p => p.MembershipID == membershipId && p.Golfer_ID == GolferId).Count();
                    if (golferWalletIDCount > 0)
                    {
                        var IsDefaultCardExists = _db.GF_GolferWallet.Where(p => p.MembershipID == membershipId && p.Golfer_ID == GolferId && p.IsDefault == true).Count();
                        if (IsDefaultCardExists > 0)
                        {
                            model.GolferWalletId = _db.GF_GolferWallet.FirstOrDefault(p => p.MembershipID == membershipId && p.Golfer_ID == GolferId && p.IsDefault == true).Wlt_ID;
                        }
                        else
                        {
                            return new OrderResult { Status = 0, Orders = null, Error = "No Card is selected as default for this golfer." };
                        }
                    }
                    else
                    {
                        return new OrderResult { Status = 0, Orders = null, Error = "Golfer's membershipId does not exists." };
                    }
                }
            }
            catch
            {
                return new OrderResult { Status = 0, Orders = null, Error = "Golfer does not exists." };
            }


            try
            {
                #region  Order Table
                ////update Order Table
                var objorder = _db.GF_Order.FirstOrDefault(p => p.ID == orderid);
                if (objorder != null)
                {
                    objorder.IsDelivered = true;
                    objorder.PaymentStatus = "1";
                    objorder.GolferWalletId = model.GolferWalletId;
                    _db.SaveChanges();
                    // get that order
                    var lstOrders = _db.GF_Order.Where(x => x.ID == orderid).ToList()
                                       .Select(x =>
                                           new
                                           {
                                               orderID = x.ID,
                                               courseID = x.CourseID,
                                               golferName = x.GF_Golfer.FirstName + " " + x.GF_Golfer.LastName,
                                               x.Longitude,
                                               x.Latitude,
                                               time = x.OrderDate.Value.ToShortTimeString(),
                                               itemsOrdered = x.GF_OrderDetails.ToList().Select(y =>
                                                   new
                                                   {
                                                       y.GF_MenuItems.Name,
                                                       UnitPrice = y.GF_MenuItems.Amount,
                                                       y.Quantity,
                                                       Amount = (y.GF_MenuItems.Amount * y.Quantity)
                                                   }),
                                               billAmount = x.GF_OrderDetails.Sum(y => (y.GF_MenuItems.Amount * y.Quantity)),
                                               x.TaxAmount,
                                               x.GolferPlatformFee,
                                               Total = ((x.GF_OrderDetails.Sum(y => (y.GF_MenuItems.Amount * y.Quantity))) + x.TaxAmount + x.GolferPlatformFee),
                                               x.OrderType,
                                               x.OrderDate,
                                               ReadyStatus = (x.KitchenId > 0) ? 1 : 0,
                                               x.HEXColor,
                                               x.RGBColor,
                                               x.HUEColor
                                           }).OrderByDescending(x => x.orderID);

                    return new OrderResult { Status = 1, Orders = lstOrders, Error = "Payment Successful." };
                }
                else
                {

                    return new OrderResult { Status = 0, Orders = null, Error = "Payment successful, but error occur in order updation." };
                }
                #endregion
            }
            catch (Exception ex)
            {
                return new OrderResult { Status = 0, Orders = null, Error = ex.Message };

            }

        }

        #region payments

        public bool ChargePayments(long orderid, ref string message, ref string transId, ref string BTResponse, ref List<string> lstMsg)
        {

            try
            {
                DelieverNPayment model = new DelieverNPayment();
                GolflerEntities _db = new GolflerEntities();

                var objOrder = _db.GF_Order.FirstOrDefault(x => x.ID == orderid);
                Order obj = new Order();
                long? GolferId = objOrder.GolferID;
                string paymentType = objOrder.PaymentType;
                if (paymentType == "0")//membership payments
                {
                    message = "membership payment";
                    transId = "";
                    return true;
                }
                else// card payments
                {
                    decimal GolflerAmount = 0;
                    decimal CourseAmount = 0;

                    if (!(obj.getOrderSplitAmount(orderid, ref GolflerAmount, ref CourseAmount)))
                    {
                        message = "Some Error Occur.";
                        transId = "";
                        return false;
                    }

                    model.BTGolflerAmount = GolflerAmount;
                    model.BTCourseAmount = CourseAmount;
                    model.OrderId = Convert.ToString(orderid);

                    model.CardNumber = Convert.ToString(CommonFunctions.DecryptUrlParam(objOrder.CardNumber));
                    model.CardExpMonth = Convert.ToString(CommonFunctions.DecryptUrlParam(objOrder.CardExpMonth));
                    model.CardExpYear = Convert.ToString(CommonFunctions.DecryptUrlParam(objOrder.CardExpYear));
                    model.CardCCV = Convert.ToString(CommonFunctions.DecryptUrlParam(objOrder.CardCCV));

                    model.CourseId = Convert.ToInt64(objOrder.CourseID);
                    string Email = "";
                    try
                    {
                        var objGolfer = _db.GF_Golfer.FirstOrDefault(p => p.GF_ID == GolferId);
                        Email = objGolfer.Email;

                        if (Email == "")
                        {
                            message = "Golfer does not exists.";
                            transId = "";
                            return false;

                        }
                    }
                    catch
                    {
                        message = "Golfer does not exists.";
                        transId = "";
                        return false;
                    }

                    model.Email = Email;

                    if (!string.IsNullOrEmpty(Convert.ToString(objOrder.BT_TransId)))
                    {
                        message = "Payment has already been successful.";
                        transId = "";
                        return false;

                    }

                    string btresponse = "";
                    lstMsg.Add("before charge");
                    string chargeId = ProcessBTPayment(model, ref btresponse,ref lstMsg);
                    lstMsg.Add("after charge: " + Convert.ToString(chargeId));
                    if (chargeId != "")
                    {
                        message = "Payment successful.";
                        transId = chargeId;
                        BTResponse = btresponse;
                        return true;
                    }
                    else
                    {
                        message = "Payment fail.";
                        transId = "";
                        BTResponse = btresponse;
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
                transId = "";
                return false;
            }
        }

        #endregion
        #endregion
    }
}