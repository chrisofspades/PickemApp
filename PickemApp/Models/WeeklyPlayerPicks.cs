using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Dapper;

namespace PickemApp.Models
{
    //Being lazy and using this class for the weekly leaders on the home page, plus the player picks for each week.
    public class WeeklyPlayerPicks
    {
        public int Id { get; set; }
        public int WeekNumber { get; set; }
        public int Year { get; set; }
        public int PlayerId { get; set; }
        public string PlayerName { get; set; }
        public int CorrectPicks { get; set; }
        public double TieBreaker { get; set; }

        public virtual Player Player { get; set; }
        public virtual List<Pick> Picks { get; set; }

        public static List<WeeklyPlayerPicks> GetWeeklyLeaders(int week, int year, bool completed = false)
        {
            var lookup = new Dictionary<int, WeeklyPlayerPicks>();

            using (PickemDBContext db = new PickemDBContext())
            using (var conn = db.Database.Connection)
            {
                string sql = @"select wpp.Id, wpp.WeekNumber, wpp.Year, wpp.PlayerId, wpp.PlayerName, wpp.CorrectPicks, wpp.TieBreaker
                                , pl.*, p.*, g.*
                                from fnWeeklyPlayerPicks(@year, @week, @completed) wpp
                                inner join Players pl on wpp.PlayerId = pl.Id
                                inner join Games g on g.Week = wpp.WeekNumber and g.Year = wpp.Year 
                                inner join Picks p on p.PlayerId = wpp.PlayerId and p.GameId = g.Id 
                                order by wpp.Rank";
                conn.Query<WeeklyPlayerPicks, Player, Pick, Game, WeeklyPlayerPicks>(sql, (wpp, pl, p, g) =>
                {
                    WeeklyPlayerPicks weeklyPlayerPick;
                    if (!lookup.TryGetValue(wpp.Id, out weeklyPlayerPick))
                    {
                        wpp.Player = pl;
                        lookup.Add(wpp.Id, weeklyPlayerPick = wpp);
                    }
                    if (weeklyPlayerPick.Picks == null)
                    {
                        weeklyPlayerPick.Picks = new List<Pick>();
                    }
                    p.Game = g;
                    weeklyPlayerPick.Picks.Add(p);

                    return weeklyPlayerPick;
                },
                param: new { year = year, week = week, completed = completed },
                splitOn: "Id").AsQueryable();
            }

            return lookup.Values.ToList();
        }
    }
}