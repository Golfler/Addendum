using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Golfler.Models;

namespace Golfler.Controllers
{
    public class HomeController : Controller
    {
        GolflerEntities db = new GolflerEntities();

        public ActionResult AboutUs()
        {
            var pageContent = db.GF_StaticPages.FirstOrDefault(x => x.PageCode.Contains("AboutUs"));

            ViewBag.AboutUsContent = pageContent.PageHTML;

            return View();
        }

        public ActionResult DeveloperGuidlines()
        {
            var pageContent = db.GF_StaticPages.FirstOrDefault(x => x.PageCode.Contains("DeveloperGuidlines"));

            ViewBag.Content = pageContent.PageHTML;

            return View();
        }

        public ActionResult Team()
        {
            var pageContent = db.GF_StaticPages.FirstOrDefault(x => x.PageCode.Contains("Team"));

            ViewBag.Content = pageContent.PageHTML;

            return View();
        }

        public ActionResult TermAndCondition()
        {
            var pageContent = db.GF_StaticPages.FirstOrDefault(x => x.PageCode.Contains("TermAndCondition"));

            ViewBag.Content = pageContent.PageHTML;

            return View();
        }
    }
}
