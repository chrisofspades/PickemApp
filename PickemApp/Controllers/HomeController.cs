using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PickemApp.Models;


namespace PickemApp.Controllers
{
    public class HomeController : Controller
    {
        private PickemDBContext db = new PickemDBContext();

        public ActionResult Index()
        {
            ViewBag.Message = "Modify this template to jump-start your ASP.NET MVC application.";
            
            //get all of the weeks
            var weeks = (from g in db.Games
                         select new WeeklyPlayerPicks
                         {
                             WeekNumber = g.Week,
                             Year = g.Year
                         }).Distinct().ToList();


            //find the leader for each week
            foreach (var week in weeks)
            {
                using (PickemDBContext db2 = new PickemDBContext())
                {
                    var playerId = (from g in db2.Games.Where(p => p.Week == week.WeekNumber && p.Year == week.Year)
                                    join p in db2.Picks on g.Id equals p.GameId into j1
                                    from j2 in j1.DefaultIfEmpty()
                                    group j2 by new { g.Week, g.Year, j2.PlayerId } into grp
                                    orderby grp.Count(t => t.PickResult == "W") descending
                                    select (int?)grp.Key.PlayerId
                                    ).FirstOrDefault();

                    if (playerId != null)
                        week.Player = db2.Players.Find(playerId);
                    else
                        week.Player = new Player();
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
