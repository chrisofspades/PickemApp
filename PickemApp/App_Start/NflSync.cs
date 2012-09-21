using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using PickemApp.Models;

namespace PickemApp
{
    public class NflSync
    {
        public static void UpdateGames(string xmlLocation)
        {
            XDocument xml = XDocument.Load(xmlLocation);
            var games = xml.Descendants("g");

            using (PickemDBContext db = new PickemDBContext())
            {
                foreach (var g in games)
                {
                    Game game = new Game();
                    game.Eid = g.Attribute("eid").Value.ToString();
                    game.Gsis = g.Attribute("gsis").Value.ToString();
                    game.Week = Convert.ToInt32(g.Parent.Attribute("w").Value.ToString());
                    game.Year = Convert.ToInt32(g.Parent.Attribute("y").Value.ToString());
                    game.Day = g.Attribute("d").Value.ToString();
                    game.Time = g.Attribute("t").Value.ToString();
                    game.Quarter = g.Attribute("q").Value.ToString();
                    game.TimeRemaining = (g.Attribute("k") != null) ? g.Attribute("k").Value.ToString() : null;
                    game.HomeTeam = g.Attribute("h").Value.ToString();
                    game.HomeTeamScore = Convert.ToInt32(g.Attribute("hs").Value.ToString());
                    game.VisitorTeam = g.Attribute("v").Value.ToString();
                    game.VisitorTeamScore = Convert.ToInt32(g.Attribute("vs").Value.ToString());
                    game.GameType = g.Attribute("gt").Value.ToString();

                    game.WinningTeam = (game.HomeTeamScore > game.VisitorTeamScore) ? game.HomeTeam : game.VisitorTeam;

                    int gameId = (from t in db.Games
                                   where t.Eid == game.Eid
                                   select t.Id).SingleOrDefault();

                    if (gameId != 0)
                    {
                        game.Id = Convert.ToInt32(gameId);

                        db.Entry(game).State = System.Data.EntityState.Modified;
                    }
                    else
                    {
                        db.Games.Add(game);
                    }
                }

                db.SaveChanges();

                var week = xml.Descendants("gms").FirstOrDefault();
                if (week != null)
                {
                    UpdatePicks(Convert.ToInt32(week.Attribute("w").Value), Convert.ToInt32(week.Attribute("y").Value));
                }
            }

        }

        public static void UpdatePicks(int week, int year)
        {
            using (PickemDBContext db = new PickemDBContext())
            {
                var picks = (from p in db.Picks
                            join g in db.Games.Where(q => q.Week == week && q.Year == year) on p.GameId equals g.Id
                            select p).ToList<Pick>();

                foreach (Pick p in picks)
                {
                    //var game = db.Games.Find(p.gamei
                    p.PickResult = (p.TeamPicked == p.Game.WinningTeam) ? "W" : null;
                    db.Entry(p).State = System.Data.EntityState.Modified;
                }

                db.SaveChanges();
            }
        }
    }
}