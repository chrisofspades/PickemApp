using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PickemApp.Models;
using PickemApp.SyncUtils;

namespace PickemApp.Controllers
{
    public class WeekController : Controller
    {
        private PickemDBContext db = new PickemDBContext();

        public ActionResult Index(int week, int year, bool completed = false)
        {
            //doing the sync here, which is ugly, but will have to suffice until I found a way to do it in the background.
            NflSync.UpdateGames("http://www.nfl.com/liveupdate/scorestrip/ss.xml");

            ViewBag.Week = week;

            //get all of the weeks
            ViewBag.Weeks = (from g in db.Games
                             where g.Year == year
                             select new WeeklyPlayerPicks
                             {
                                 WeekNumber = g.Week,
                                 Year = g.Year
                             }).Distinct().ToList();

            var listLeaders = WeeklyPlayerPicks.GetWeeklyLeaders(week, year, completed);

            ViewBag.Leaders = listLeaders.ToList();
            ViewBag.Games = db.Games.Where(q => q.Week == week && q.Year == year).OrderBy(o => o.Gsis);

            return View(listLeaders.ToList());
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}
