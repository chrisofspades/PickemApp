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

            var listLeaders = WeeklyPlayerPicks.GetWeeklyLeaders(week, year);

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
