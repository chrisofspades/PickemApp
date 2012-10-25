using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PickemApp.SyncUtils;
using PickemApp.Models;
using System.Net;

namespace PickemApp.Controllers
{
    public class HomeController : Controller
    {
        private PickemDBContext db = new PickemDBContext();

        delegate void HitWebPageDelegate(string URL);
        private void requestWithRecache(string URL)
        {
            HttpWebRequest wreq = null;
            HttpWebResponse wresp = null;
            wreq = (HttpWebRequest)WebRequest.Create(URL);
            wreq.Method = "GET";
            wreq.Timeout = 60000;
            wreq.KeepAlive = false;

            wresp = (HttpWebResponse)wreq.GetResponse();
            wresp.Close();
        }

        public ActionResult Index()
        {
            //Doing the sync here, which is ugly, but will have to suffice until I found a way to do it in the background.
            //Better now doing it asynchronously, but still not ideal.
            HitWebPageDelegate d = new HitWebPageDelegate(requestWithRecache);
            UriBuilder ub = new UriBuilder(Request.Url);
            ub.Path = "/Sync/Nfl";
            AsyncHelper.FireAndForget(d, new string[1] { ub.Uri.ToString() });

            //get all of the weeks
            var weeks = (from g in db.Games
                         where g.Year == 2012
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
