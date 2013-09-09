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
            NflSync.UpdateGames("http://www.nfl.com/liveupdate/scorestrip/ss.xml");

            int year = ConfigurationManager.AppSettings["currentYear"] != null ? Convert.ToInt32(ConfigurationManager.AppSettings["currentYear"]) : DateTime.Today.Year;

            //get all of the weeks
            var weeks = (from g in db.Games.Where(x => x.Year == year)
                         select new WeeklyPlayerPicks
                         {
                             WeekNumber = g.Week,
                             Year = g.Year
                         }).Distinct().ToList();


            //find the leader for each week
            for (var i = 0; i < weeks.Count; i++)
            {
                weeks[i] = WeeklyPlayerPicks.GetWeeklyLeaders(weeks[i].WeekNumber, weeks[i].Year).FirstOrDefault() ?? weeks[i];
                if (weeks[i].Player == null)
                {
                    weeks[i].Player = new Player();
                }
            }

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
