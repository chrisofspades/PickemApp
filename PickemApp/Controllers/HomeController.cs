using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PickemApp.SyncUtils;
using PickemApp.Models;
using System.Configuration;

namespace PickemApp.Controllers
{
    public class HomeController : Controller
    {
        private PickemDBContext db = new PickemDBContext();

        public ActionResult Index()
        {
            //doing the sync here, which is ugly, but will have to suffice until I found a way to do it in the background.
            //2014-09-06: using CloudScheduler now so I can stop doing this!
            //NflSync.UpdateGames("http://www.nfl.com/liveupdate/scorestrip/ss.xml");

            int year = ConfigurationManager.AppSettings["currentYear"] != null ? Convert.ToInt32(ConfigurationManager.AppSettings["currentYear"]) : DateTime.Today.Year;

            //get all of the weeks
            var weeks = (from v in db.vwWeeklyPlayerPicks
                     where v.Year == year && v.Rank == 1
                     orderby v.Week
                     select new WeeklyPlayerPicks
                     {
                         WeekNumber = v.Week,
                         Year = v.Year,
                         Player = v.Player
                     }).ToList();

            ViewBag.Weeks = weeks;

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your app description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}
