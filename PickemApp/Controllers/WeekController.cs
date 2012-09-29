using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PickemApp.Models;

namespace PickemApp.Controllers
{
    public class WeekController : Controller
    {
        private PickemDBContext db = new PickemDBContext();

        //
        // GET: /Week/2012/1

        public ActionResult Index(int week, int year)
        {
            ViewBag.Week = week;

            var leaders = (from g in db.Games.Where(p => p.Week == week && p.Year == year)
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

            List<WeeklyPlayerPicks> listLeaders = new List<WeeklyPlayerPicks>();
            foreach (var pp in leaders)
            {
                if (pp.PlayerId != null)
                {
                    using (PickemDBContext db2 = new PickemDBContext())
                    {
                        WeeklyPlayerPicks wpp = new WeeklyPlayerPicks()
                        {
                            Player = db2.Players.Find(pp.PlayerId),
                            CorrectPicks = pp.CorrectPicks,
                            Picks = db2.Picks.Where(q => q.PlayerId == pp.PlayerId && q.Game.Week == week && q.Game.Year == year).ToList()
                        };

                        //Do something with the picks here so the view doesn't throw System.ObjectDisposedException
                        foreach (var pick in wpp.Picks)
                        {
                            pick.Game = db2.Games.Find(pick.GameId);
                        }

                        listLeaders.Add(wpp);
                    }
                }
            }

            ViewBag.Leaders = listLeaders.ToList();

            return View(listLeaders.ToList());
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}
