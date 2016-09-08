using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Dapper;

using PickemApp.Models;
using PickemApp.ViewModels;

namespace PickemApp.Controllers
{
    public class HistoryController : Controller
    {
        private PickemDBContext db = new PickemDBContext();

        public ActionResult Index()
        {
            HistoryIndex vm = new HistoryIndex();

            using (var conn = db.Database.Connection)
            {
                var currentYear = ConfigurationManager.AppSettings["currentYear"] != null ? Convert.ToInt32(ConfigurationManager.AppSettings["currentYear"]) : DateTime.Today.Year;

                string query = @"
                                SELECT [Season] as [Year], [1], [2], [3], p1.*, p2.*, p3.*
                                FROM
                                (
	                                select Season, PlayerId, Place 
	                                from (
	                                select playerid, playername, year as Season, SUM(correctpicks) as totalpicks
	                                , rank() over (partition by year order by sum(correctpicks) desc, sum(tiebreaker) asc) as Place
	                                from vwWeeklyPlayerPicks
	                                group by playerid, playername, year
	                                ) as o
	                                where o.Place <= 3
                                    and o.Season < @currentYear
                                ) AS SourceTable
                                PIVOT
                                (
	                                MAX(PlayerId)
	                                FOR Place IN ([1], [2], [3])
                                ) AS pt
                                INNER JOIN Players p1 on pt.[1] = p1.Id
                                INNER JOIN Players p2 on pt.[2] = p2.Id
                                INNER JOIN Players p3 on pt.[3] = p3.Id
                                ORDER BY pt.Season DESC
                                ";

                var seasons = conn.Query<Season, Player, Player, Player, Season>(query, (s, p1, p2, p3) =>
                {
                    s.First = p1;
                    s.Second = p2;
                    s.Third = p3;

                    return s;
                }, param: new { currentYear = currentYear }).ToList();

                vm.Seasons = seasons;
            }

            return View(vm);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}
