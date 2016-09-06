using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;

using PickemApp.Models;

namespace PickemApp.Controllers
{
    public class StandingsController : Controller
    {
        private PickemDBContext db = new PickemDBContext();

        //
        // GET: /Standings/

        public ActionResult Index(int? year)
        {
            if (!year.HasValue)
            {
                year = ConfigurationManager.AppSettings["currentYear"] != null ? Convert.ToInt32(ConfigurationManager.AppSettings["currentYear"]) : DateTime.Today.Year;
            }

            var wpp = from v in db.vwWeeklyPlayerPicks
                      where v.Year == year
                      group v by new { v.PlayerId, v.PlayerName } into g
                      orderby g.Sum(x => x.CorrectPicks) descending, g.Sum(x => x.TieBreaker) ascending
                      select new WeeklyPlayerPicks
                      {
                          PlayerId = g.Key.PlayerId, 
                          PlayerName = g.Key.PlayerName,
                          CorrectPicks = g.Sum(x => x.CorrectPicks)
                      };

            ViewBag.Year = year;

            return View(wpp.ToList());
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}
