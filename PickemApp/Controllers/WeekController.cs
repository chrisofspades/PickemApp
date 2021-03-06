﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PickemApp.Models;
using PickemApp.SyncUtils;
using PickemApp.ViewModels;

namespace PickemApp.Controllers
{
    public class WeekController : Controller
    {
        private PickemDBContext db = new PickemDBContext();

        public ActionResult Index(int week, int year, bool completed = false)
        {
            //doing the sync here, which is ugly, but will have to suffice until I found a way to do it in the background.
            //2014-09-06: using CloudScheduler now so I can stop doing this!
            //NflSync.UpdateGames("http://www.nfl.com/liveupdate/scorestrip/ss.xml");

            var vm = new WeekIndex();

            vm.CurrentWeek = new Week(week, year);

            //get all of the weeks
            vm.Weeks = (from g in db.Games
                             where g.Year == year && g.GameType == "REG"
                             select new Week
                             {
                                 WeekNumber = g.Week,
                                 Year = g.Year
                             }).Distinct().ToList();

            if (DateTime.Now > PickemApp.Models.Season.GetDeadline(week, year))
            {
                var listLeaders = WeeklyPlayerPicks.GetWeeklyLeaders(week, year, completed);
                vm.Leaders = listLeaders.ToList();
            }

            var games = db.Games.Where(q => q.Week == week && q.Year == year && q.GameType == "REG").ToList();
            vm.Games = games.OrderBy(o => o.Eid.Substring(0, 8)).ThenBy(o => o.Time.PadLeft(5, '0')).ThenBy(o => o.Gsis).ToList();

            return View(vm);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}
