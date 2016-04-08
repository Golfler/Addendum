using System.Web;
using System.Web.Mvc;
using System;

namespace Golfler.Models
{
    /// <summary>
    /// Created By Renuka Hira
    /// Created on 17 April, 2013
    /// Purpose: Set filter
    /// </summary>
    public class SessionExpireFilterAttribute:ActionFilterAttribute 
    {

        public override void OnActionExecuting( ActionExecutingContext filterContext ) {
            HttpContext ctx = HttpContext.Current;

            // check if session is supported
            bool isLogin = LoginInfo.IsLoginUser;
            
            if (LoginInfo.IsGolferLoginUser)
            {
                isLogin = true;
            }

            if (isLogin)
            {
                if (ctx.Request.Url.ToString().ToLower().IndexOf(UserType.GetFolder(LoginInfo.Type)) == -1)
                    //isLogin = false;
                    isLogin = true;
            }

            if (!isLogin)
            {
                // If it says it is a new session, but an existing cookie exists, then it must have timed out
                LoginInfo.LoginOffSession();

                var segments = ctx.Request.Url.Segments;
                /// Edit By Pardeep Singh (04-sep-2013) 
               
                string requrl = ctx.Request.Url.ToString().ToLower();
                if (ctx.Request.Cookies["loginsuper"] != null)
                    filterContext.Result = new RedirectResult("~" + segments[0] + segments[1] + segments[2]);
                else if (ctx.Request.Cookies["logincauser"] != null && !string.IsNullOrEmpty(ctx.Request.Cookies["logincauser"].Values["url"]))
                    filterContext.Result = new RedirectResult(Convert.ToString(ctx.Request.Cookies["logincauser"].Values["url"]));
                else if (ctx.Request.Cookies["logingolferuser"] != null && !string.IsNullOrEmpty(ctx.Request.Cookies["logingolferuser"].Values["url"]))
                    filterContext.Result = new RedirectResult(Convert.ToString(ctx.Request.Cookies["logingolferuser"].Values["url"]));
                else
                    filterContext.Result = new RedirectResult("~" + segments[0] + segments[1] + segments[2]);

                //filterContext.Result = new RedirectResult("~/Golfler");
            }

            base.OnActionExecuting ( filterContext );


            //Adding below code for expires the page imediatly so that no browser back can work.
            ctx.Response.Expires = -1;
            ctx.Response.Cache.SetNoServerCaching();
            ctx.Response.Cache.SetAllowResponseInBrowserHistory(false);
            ctx.Response.CacheControl = "no-cache";
            ctx.Response.Cache.SetNoStore();

        }
    }

    public class SessionExpireOnAjaxRequestFilterAttribute : ActionFilterAttribute
    {

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            HttpContext ctx = HttpContext.Current;

            // check if session is supported
            bool isLogin = LoginInfo.IsLoginUser;
            if (!isLogin)
            {
                // If it says it is a new session, but an existing cookie exists, then it must
                // have timed out
                LoginInfo.LoginOffSession();

                var url = "";

                if (ctx.Request.Cookies["loginsuper"] != null)
                    Convert.ToString(ctx.Request.Cookies["loginsuper"].Values["url"]);

                if (string.IsNullOrEmpty(url) && ctx.Request.Cookies["logincauser"] != null)
                    url = Convert.ToString(ctx.Request.Cookies["logincauser"].Values["url"]);

                if (string.IsNullOrEmpty(url) && ctx.Request.Cookies["logincourseuser"] != null)
                    url = Convert.ToString(ctx.Request.Cookies["logincourseuser"].Values["url"]);

                filterContext.Result = new JsonResult
            {
                Data = new
                {
                    // put whatever data you want which will be sent
                    // to the client
                    message = "sessionexpire",
                    url = Params.SiteUrl.Substring(0,Params.SiteUrl.Length-1) + url
                },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
            }
            else
            {
                base.OnActionExecuting(filterContext);
            }

            //Adding below code for expires the page imediatly so that no browser back can work.
            ctx.Response.Expires = -1;
            ctx.Response.Cache.SetNoServerCaching();
            ctx.Response.Cache.SetAllowResponseInBrowserHistory(false);
            ctx.Response.CacheControl = "no-cache";
            ctx.Response.Cache.SetNoStore();

        }
    }
}