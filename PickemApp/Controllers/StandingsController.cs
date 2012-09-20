using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PickemApp.Models;

namespace PickemApp.Controllers
{
    public class StandingsController : Controller
    {
        private PickemDBContext db = new PickemDBContext();

        //
        // GET: /Standings/

        public ActionResult Index()
        {

            var playerPicks = (from g in db.Games
                               join p in db.Picks on g.Id equals p.GameId into j1
                               from j2 in j1.DefaultIfEmpty()
                               group j2 by new { g.Week, g.Year, j2.PlayerId } into grp
                               orderby grp.Count(t => t.PickResult == "W") descending
                               select new
                               {
                                   PlayerId = (int?)grp.Key.PlayerId,
                                   CorrectPicks = grp.Count(t => t.PickResult == "W")
                               }
                               );

            List<WeeklyPlayerPicks> wpp = new List<WeeklyPlayerPicks>();
            foreach (var pp in playerPicks)
            {
                if (pp.PlayerId != null)
                {
                    using (PickemDBContext db2 = new PickemDBContext())
                    {
                        wpp.Add(new WeeklyPlayerPicks()
                        {
                            Player = db2.Players.Find(pp.PlayerId),
                            CorrectPicks = pp.CorrectPicks
                        });
                    }
                }
            }

            ViewBag.PlayerPicks = wpp.ToList();

            return View();
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}
