using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using Braintree;


namespace GolferWebAPI.Models
{
    public partial class GF_GolferWallet
    {
        GolflerEntities _db = null;
        public GF_GolferWallet()
        {
            _db = new GolflerEntities();
        }

        /// <summary>
        /// Created By:Arun
        /// Created Date: 23 March 2015
        /// Purpose: Add or Update Golfer's Wallet Info.
        /// </summary>
        /// <param name="objWallet"></param>
        /// <returns></returns>
        public Result AddUpdateGolferWallet(GF_GolferWallet objWallet)
        {
            try
            {
              //  #region Create Wallet on Braintree

              //  string strPublicKey = "";
              //  string strPrivateKey = "";
              //  string strMerchantId = "";
              //  string strMasterMerchantAccountId = "";
              //  Braintree.Environment strEnviroment = Braintree.Environment.SANDBOX;
              //  if (Convert.ToString(System.Web.Configuration.WebConfigurationManager.AppSettings["PAYMENT_MODE"]) == "L")
              //  {
              //      strEnviroment = Braintree.Environment.PRODUCTION;
              //      strPublicKey = Convert.ToString(ConfigurationManager.AppSettings["BTPublicKey_Live"]);
              //      strPrivateKey = Convert.ToString(ConfigurationManager.AppSettings["BTPrivateKey_Live"]);
              //      strMerchantId = Convert.ToString(ConfigurationManager.AppSettings["BTMerchantId_Live"]);
              //      strMasterMerchantAccountId = Convert.ToString(ConfigurationManager.AppSettings["BTMasterMerchantAccountId_Live"]);
              //  }
              //  else
              //  {
              //      strEnviroment = Braintree.Environment.SANDBOX;
              //      strPublicKey = Convert.ToString(ConfigurationManager.AppSettings["BTPublicKey"]);
              //      strPrivateKey = Convert.ToString(ConfigurationManager.AppSettings["BTPrivateKey"]);
              //      strMerchantId = Convert.ToString(ConfigurationManager.AppSettings["BTMerchantId"]);
              //      strMasterMerchantAccountId = Convert.ToString(ConfigurationManager.AppSettings["BTMasterMerchantAccountId"]);
              //  }


              //  BraintreeGateway Gateway = new BraintreeGateway
              //  {
              //      Environment = strEnviroment,
              //      PublicKey = strPublicKey,
              //      PrivateKey = strPrivateKey,
              //      MerchantId = strMerchantId
              //  };

              ////  var clientToken = Gateway.ClientToken.generate();

              //  var request = new CustomerRequest
              //  {
              //      FirstName = "Ramesh",
              //      LastName = "Kalra",
              //      Company = "Kalra company",
              //      Email = "rkalra@cogniter.com",
              //      Fax = "123-123-1234",
              //      Phone = "123-123-1234" 
              //  };
              //  Result<Customer> result = Gateway.Customer.Create(request);

              //  bool success = result.IsSuccess();
              //  // true

              //  string customerId = result.Target.Id;
              //  // e.g. 594019

              //  #endregion


                if (objWallet.Wlt_ID == 0)//Addition
                {
                    objWallet.CreatedOn = DateTime.Now;
                    objWallet.Status = true;
                    objWallet.StatusText = StatusType.Active;
                    _db.GF_GolferWallet.Add(objWallet);
                    _db.SaveChanges();

                    #region make other card's isdefault false
                    if (objWallet.IsDefault == true)
                    {
                        var objOtherCards = _db.GF_GolferWallet.Where(x => x.Golfer_ID == objWallet.Golfer_ID && x.Wlt_ID != objWallet.Wlt_ID);
                        if (objOtherCards != null)
                        {
                            foreach (var item in objOtherCards)
                            {
                                item.IsDefault = false;
                            }
                            _db.SaveChanges();
                        }
                    }
                    #endregion


                    return new Result
                    {
                        Id = objWallet.Wlt_ID,
                        Status = 1,
                        Error = "Wallet added successfully.",
                        record = new
                        {
                            objWallet.CCName,
                            objWallet.CCNumber,
                            ExpMonth = Convert.ToString(objWallet.ExpMonth).Length < 2 ? ("0" + Convert.ToString(objWallet.ExpMonth)) : (Convert.ToString(objWallet.ExpMonth)),
                            objWallet.ExpYear,
                            objWallet.FirstName,
                            objWallet.LastName,
                            objWallet.MembershipID,
                            objWallet.IsDefault,

                        }
                    };
                }
                else
                { //Update

                    var objOld = _db.GF_GolferWallet.FirstOrDefault(x => x.Wlt_ID == objWallet.Wlt_ID);
                    if (objOld != null)
                    {
                        objOld.CCName = objWallet.CCName;
                        objOld.CCNumber = objWallet.CCNumber;
                        objOld.ExpMonth = objWallet.ExpMonth;
                        objOld.ExpYear = objWallet.ExpYear;
                        objOld.Address1 = objWallet.Address1;
                        objOld.Address2 = objWallet.Address2;
                        objOld.City = objWallet.City;
                        objOld.State = objWallet.State;
                        objOld.Country = objWallet.Country;
                        objOld.Zipcode = objWallet.Zipcode;
                        objOld.MembershipID = objWallet.MembershipID;
                        objOld.FirstName = objWallet.FirstName;
                        objOld.LastName = objWallet.LastName;
                        objOld.MembershipID = objWallet.MembershipID;
                        objOld.IsDefault = objWallet.IsDefault;
                        _db.SaveChanges();
                        #region make other card's isdefault false
                        if (objWallet.IsDefault == true)
                        {
                            var objOtherCards = _db.GF_GolferWallet.Where(x => x.Golfer_ID == objWallet.Golfer_ID && x.Wlt_ID != objWallet.Wlt_ID);
                            if (objOtherCards != null)
                            {
                                foreach (var item in objOtherCards)
                                {
                                    item.IsDefault = false;
                                }
                                _db.SaveChanges();
                            }
                        }
                        #endregion
                    }
                    else
                    {
                        return new Result { Id = objWallet.Wlt_ID, Status = 0, Error = "Wallet for this ID does not exists." };
                    }
                    return new Result
                    {
                        Id = objWallet.Wlt_ID,
                        Status = 1,
                        Error = "Wallet updated successfully.",
                        record = new
                        {
                            objOld.CCName,
                            objOld.CCNumber,
                            ExpMonth = Convert.ToString(objOld.ExpMonth).Length < 2 ? ("0" + Convert.ToString(objOld.ExpMonth)) : (Convert.ToString(objOld.ExpMonth)),
                            objOld.ExpYear,
                            objOld.IsDefault

                        }
                    };
                }
            }
            catch (Exception ex)
            {
                return new Result { Id = 0, Status = 0, Error = ex.Message };
            }


        }


        /// <summary>
        /// Created By: Amit Kumar
        /// Created Date: 02 April 2015
        /// Purpose: Get golfer wallet listing
        /// </summary>
        /// <param name="golferWallet"></param>
        /// <returns></returns>
        public Result GetGolferWalletListing(GF_GolferWallet golferWallet)
        {
            try
            {
                _db = new GolflerEntities();

                var lstWallet = _db.GF_GolferWallet.Where(x => x.Golfer_ID == golferWallet.Golfer_ID && (x.Status ?? false))
                    .Select(x =>
                        new
                        {
                            x.CCName,
                            //ExpMonth = Convert.ToString(item.ExpMonth).Length < 2 ? ("0" + Convert.ToString(item.ExpMonth)) : (Convert.ToString(item.ExpMonth)),
                            x.ExpMonth,
                            x.ExpYear,
                            x.CCNumber,
                            x.IsDefault
                        });
                List<WalletResult> lstWalletResult = new List<WalletResult>();
                foreach (var item in lstWallet)
                {
                    WalletResult objwallet = new WalletResult();
                    objwallet.CCName = item.CCName;
                    objwallet.ExpMonth = Convert.ToString(item.ExpMonth).Length < 2 ? ("0" + Convert.ToString(item.ExpMonth)) : (Convert.ToString(item.ExpMonth));
                    objwallet.ExpYear = Convert.ToString(item.ExpYear);
                    objwallet.CCNumber = item.CCNumber;
                    objwallet.IsDefault = item.IsDefault;
                    lstWalletResult.Add(objwallet);
                }
                return new Result
                {
                    Id = golferWallet.Golfer_ID ?? 0,
                    Status = 1,
                    Error = "Success.",
                    record = lstWalletResult
                };
            }
            catch (Exception ex)
            {
                return new Result
                {
                    Id = golferWallet.Golfer_ID ?? 0,
                    Status = 0,
                    Error = ex.Message,
                    record = null
                };
            }
        }

        /// <summary>
        /// Created By: Amit Kumar
        /// Created Date: 13 April 2015
        /// Purpose: Delete golfer wallet
        /// </summary>
        /// <param name="golferWallet"></param>
        /// <returns></returns>
        public Result DeleteWallet(GF_GolferWallet golferWallet)
        {
            try
            {
                GolflerEntities _db = new GolflerEntities();

                GF_GolferWallet objWallet = _db.GF_GolferWallet.FirstOrDefault(x => x.Golfer_ID == golferWallet.Golfer_ID &&
                    x.CCNumber.Contains(golferWallet.CCNumber) &&
                    x.Status == true &&
                    x.StatusText != StatusType.Delete);

                if (objWallet != null)
                {
                    objWallet.StatusText = StatusType.Delete;
                    objWallet.Status = false;
                    _db.SaveChanges();

                    return new Result
                    {
                        Id = golferWallet.Golfer_ID ?? 0,
                        Status = 1,
                        Error = "Success.",
                        record = new
                        {
                            golferWallet.Golfer_ID,
                            golferWallet.CCNumber
                        }
                    };
                }

                return new Result
                {
                    Id = golferWallet.Golfer_ID ?? 0,
                    Status = 0,
                    Error = "No record found.",
                    record = new
                    {
                        golferWallet.Golfer_ID,
                        golferWallet.CCNumber
                    }
                };
            }
            catch (Exception ex)
            {
                return new Result
                {
                    Id = golferWallet.Golfer_ID ?? 0,
                    Status = 0,
                    Error = ex.Message,
                    record = null
                };
            }
        }
    }
    public class WalletResult
    {
        public string CCName { get; set; }
        public string ExpMonth { get; set; }
        public string ExpYear { get; set; }
        public string CCNumber { get; set; }
        public bool? IsDefault { get; set; }
    }
}