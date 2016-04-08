using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Braintree;
using Golfler.Models;

namespace Golfler.Controllers
{
    public class WebHooksController : Controller
    {
        //
        // GET: /WebHooks/
        BraintreeGateway Gateway = new BraintreeGateway
        {
            Environment = Braintree.Environment.SANDBOX,
            PublicKey = ConfigurationManager.AppSettings["BTPublicKey"].ToString(),
            PrivateKey = ConfigurationManager.AppSettings["BTPrivateKey"].ToString(),
            MerchantId = ConfigurationManager.AppSettings["BTMerchantId"].ToString()
        };
        public ActionResult Accept()
        {
            if (Request.HttpMethod == "POST")
            {
                try
                {
                    CommonFunctions.WebHooksLog("POST Request");
                    WebhookNotification webhookNotification = Gateway.WebhookNotification.Parse(
                        Request.Params["bt_signature"],
                        Request.Params["bt_payload"]
                        );
                    if (webhookNotification != null)
                    {
                        string message = string.Format(
                           "[Webhook Received {0}] | Kind: {1} | Subscription: {2}",
                            webhookNotification.Timestamp.Value,
                            webhookNotification.Kind,
                            webhookNotification.Subscription.Id
                            );

                        CommonFunctions.WebHooksLog(message);
                        return new HttpStatusCodeResult(200);
                    }
                    else
                    {
                        string message = "";

                        CommonFunctions.WebHooksLog(message);
                        return new HttpStatusCodeResult(200);
                    }
                }
                catch (Exception ex)
                {
                    CommonFunctions.WebHooksLog(ex.Message);
                    return new HttpStatusCodeResult(200);
                }
            }
            else
            {
                CommonFunctions.WebHooksLog("Get Request");
                return Content(Gateway.WebhookNotification.Verify(Request.QueryString["bt_challenge"]));
            }
        }

    }
}
