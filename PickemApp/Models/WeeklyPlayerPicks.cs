using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PickemApp.Models
{
    //Being lazy and using this class for the weekly leaders on the home page, plus the player picks for each week.
    public class WeeklyPlayerPicks
    {
        public int WeekNumber { get; set; }
        public int Year { get; set; }
        public Player Player { get; set; } 
        public int CorrectPicks { get; set; }
        public int TieBreaker { get; set; }
        public List<Pick> Picks { get; set; }

        public static List<WeeklyPlayerPicks> GetWeeklyLeaders(int week, int year)
        {
            using (PickemDBContext db = new PickemDBContext())
            {
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
                        WeeklyPlayerPicks wpp = new WeeklyPlayerPicks()
                        {
                            WeekNumber = week,
                            Year = year,
                            Player = db.Players.Find(pp.PlayerId),
                            CorrectPicks = pp.CorrectPicks,
                            Picks = db.Picks.Where(q => q.PlayerId == pp.PlayerId && q.Game.Week == week && q.Game.Year == year).ToList()
                        };

                        wpp.TieBreaker = (from p in db.Picks
                                            where p.PlayerId == pp.PlayerId && p.Game.Week == week && p.Game.Year == year && p.TotalPoints > 0
                                            select Math.Abs(p.TotalPoints - (p.Game.HomeTeamScore + p.Game.VisitorTeamScore))).FirstOrDefault();

                        //Do something with the picks here so the view doesn't throw System.ObjectDisposedException
                        foreach (var pick in wpp.Picks)
                        {
                            pick.Game.HomeTeam = pick.Game.HomeTeam;
                        }

                        listLeaders.Add(wpp);

                    }
                }
                return listLeaders.OrderByDescending(o => o.CorrectPicks).ThenBy(o => o.TieBreaker).ToList();
            }
        }

    }
}