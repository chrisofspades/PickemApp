using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Dapper;

using PickemApp.Models;
using PickemApp.ViewModels;

using WebMatrix.WebData;

namespace PickemApp.Controllers
{
    [Authorize]
    public class PicksController : Controller
    {
        private PickemDBContext db = new PickemDBContext();

        public ActionResult Index(int? week, int? year)
        {
            if (!week.HasValue || ! year.HasValue)
            {
                var game = db.Games.Where(g => g.GameType == "REG").OrderByDescending(g => g.Year).ThenByDescending(g => g.Week).FirstOrDefault();
                if (game == null)
                    return HttpNotFound();

                return RedirectToAction("Index", new { week = game.Week, year = game.Year });
            }

            ViewBag.Deadline = PickemApp.Models.Season.GetDeadline(week.Value, year.Value);
            ViewBag.PastDeadline = (DateTime.Now >= PickemApp.Models.Season.GetDeadline(week.Value, year.Value));

            var user = db.Players.Find(WebSecurity.CurrentUserId);

            var vm = new PicksIndex();

            vm.CurrentPlayer = user;
            vm.CurrentWeek = new Week(week.Value, year.Value);

            //get all of the weeks
            vm.Weeks = (from g in db.Games
                        where g.Year == year && g.GameType == "REG"
                        select new Week
                        {
                            WeekNumber = g.Week,
                            Year = g.Year
                        }).Distinct().ToList();

            using (var conn = db.Database.Connection)
            {
                string sql = @"select p.Id, p.TeamPicked, p.TotalPoints, p.PickResult, g.Id as GameId, g.HomeTeam, g.VisitorTeam, g.WinningTeam
                            from Games g 
                            left outer join Picks p on g.Id = p.GameId and p.PlayerId = @playerId
                            where g.Week = @week and g.Year = @year
                            order by left(g.Eid, 8), RIGHT('00000' + ISNULL(g.Time, ''), 5), g.Gsis";

                vm.Picks = conn.Query<PickRadio>(sql,
                param: new { week = week, year = year, playerId = user.Id }).ToList();
            }
            
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Index(int week, int year, PicksIndex form)
        {
            var user = db.Players.Find(WebSecurity.CurrentUserId);
            form.CurrentPlayer = user;
            form.CurrentWeek = new Week(week, year);

            //get all of the weeks
            form.Weeks = (from g in db.Games
                        where g.Year == year && g.GameType == "REG"
                        select new Week
                        {
                            WeekNumber = g.Week,
                            Year = g.Year
                        }).Distinct().ToList();

            if (ModelState.IsValid)
            {
                var picks = form.Picks.Select(p => new Pick 
                { 
                    Id = p.Id,
                    PlayerId = user.Id,
                    GameId = p.GameId,
                    TeamPicked = p.TeamPicked,
                    TotalPoints = p.TotalPoints,
                    PickResult = p.PickResult
                });

                foreach (var pick in picks)
                {
                    if (pick.Id != 0)
                    {
                        db.Entry(pick).State = System.Data.Entity.EntityState.Modified;
                    }
                    else
                    {
                        db.Picks.Add(pick);
                    }
                }

                db.SaveChanges();

                return RedirectToAction("Index", new { week = week, year = year});
            }

            Response.Write(form.Picks.Count);

            return View("Index", form);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}