using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Golfler
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            //routes.MapRoute(
            //    name: "Default",
            //    url: "{controller}/{action}/{id}",
            //    defaults: new { controller = "Admin", action = "LogIn", id = UrlParameter.Optional }
            //);

            routes.MapRoute(
                "Default",
                "Golfler/{controller}/{action}/{eid}",
                new { Controller = "Admin", action = "Login", eid = UrlParameter.Optional }
            );


           // routes.MapRoute(
           //     "CourseAdmin",
           //     "CourseAdmin/{action}/{eid}",
           //     new { Controller = "CourseAdmin", action = "Login", eid = UrlParameter.Optional }
           // );

           // routes.MapRoute(
           //    "Golfer",
           //    "Golfer/{action}/{eid}",
           //    new { Controller = "Golfer", action = "Login", eid = UrlParameter.Optional }
           //);

           // routes.MapRoute(
           //    "Default",
           //    "{controller}/{action}/{eid}",
           //    new { Controller = "Admin", action = "Login", eid = UrlParameter.Optional }
           //);
        }
    }
}