using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using PickemApp.Models;
using HtmlAgilityPack;
using System.Text.RegularExpressions;

namespace PickemApp.SyncUtils
{
    public class NflSync
    {
        public static void UpdateGames(string xmlLocation)
        {
            //The XML document is expected to be in the same format as http://www.nfl.com/liveupdate/scorestrip/ss.xml
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

                    if (game.HomeTeamScore == game.VisitorTeamScore)
                        game.WinningTeam = null;
                    else if (game.HomeTeamScore > game.VisitorTeamScore)
                        game.WinningTeam = game.HomeTeam;
                    else if (game.VisitorTeamScore > game.HomeTeamScore)
                        game.WinningTeam = game.VisitorTeam;

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
                    p.PickResult = (p.TeamPicked == p.Game.WinningTeam) ? "W" : null;
                    db.Entry(p).State = System.Data.EntityState.Modified;
                }

                db.SaveChanges();
            }
        }
    }

    public class PickSync
    {
        public static void UpdatePicks(string htmldoc)
        {
            HtmlDocument doc = new HtmlDocument();
            htmldoc = HttpContext.Current.Server.MapPath(VirtualPathUtility.ToAbsolute("~/Content/datafiles/" + htmldoc));
            doc.Load(htmldoc);

            // The node containing the week will look something like: <p class="c2 title"><a name="h.gc180lrgofzr"></a><span>Week 1</span></p>
            int week = 1;
            var weekNode = doc.DocumentNode.Descendants().Where(n => n.Name == "p" && n.Attributes["class"].Value.Contains("title")).FirstOrDefault();
            if (weekNode != null)
            {
                week = Convert.ToInt32(weekNode.InnerText.Replace("Week", "").Trim());
            }

            // The node containing the year will look something like: <p class="c2 subtitle">	<a name="h.154hsp1t7nbe"></a><span>2012</span></p>
            int year = 2012;
            var yearNode = doc.DocumentNode.Descendants().Where(n => n.Name == "p" && n.Attributes["class"].Value.Contains("subtitle")).FirstOrDefault();
            if (yearNode != null)
            {
                year = Convert.ToInt32(yearNode.InnerText.Trim());
            }

            using (PickemDBContext db = new PickemDBContext())
            {
                List<Pick> newPicks = new List<Pick>();

                // Select pick container
                var playerpicks = doc.DocumentNode.Descendants().Where(n => n.Name == "div" && n.Attributes["class"].Value == "playerpick");
                foreach (var pp in playerpicks)
                {
                    // Player name node looks like: <h2 class="c2">	<a name="h.tid79cr1nhcg"></a><span>ANDY</span></h2>
                    var playerName = pp.SelectSingleNode("h2").InnerText.Trim();
                    Player player = db.Players.Where(q => q.Name.ToLower() == playerName.ToLower()).FirstOrDefault();

                    // Loop through picks. Each pick node will look like: 	<p class="c2"><span class="c0">SD@ </span><span class="c0 c1">OAK (41)</span></p>
                    var picks = pp.Descendants().Where(n => n.Name == "p");
                    foreach (var p in picks)
                    {
                        // Parse node inner text to retrive home team, visitor team, and total points
                        string innerText = p.InnerText;
                        int totalPoints = 0;
                        string homeTeam;
                        string visitorTeam;
                        string teamPicked = "";

                        // Start with total points, and then remove it from inner text
                        Match m = Regex.Match(innerText, @"(\(([\d]*)\))");
                        if (m.Success)
                        {
                            totalPoints = Convert.ToInt32(m.Groups[2].Value);
                            innerText = innerText.Replace(m.Value, "");
                        }

                        // We should be left with "SD@ OAK", so extract teams from there
                        homeTeam = innerText.Split('@')[1].Trim();
                        visitorTeam = innerText.Split('@')[0].Trim();

                        // Get team picked from span node with class=c1 (<span class="c0 c1">OAK (41)</span>)
                        var teamPickedNode = p.Descendants().Where(n => n.Name == "span" && n.Attributes["class"].Value.Contains("c1")).FirstOrDefault();
                        if (teamPickedNode != null)
                        {
                            teamPicked = Regex.Replace(teamPickedNode.InnerText, @"[^a-zA-Z]*", "");
                        }

                        // Find the game record
                        Game game = (from g in db.Games
                                     where g.Week == week && g.Year == year && g.HomeTeam == homeTeam && g.VisitorTeam == visitorTeam
                                     select g).FirstOrDefault();

                        if (player != null && game != null && !string.IsNullOrEmpty(teamPicked))
                        {
                            Pick newPick = new Pick
                            {
                                PlayerId = player.Id,
                                Player = player,
                                GameId = game.Id,
                                Game = game,
                                TeamPicked = teamPicked,
                                TotalPoints = totalPoints
                            };

                            newPick.Id = (from o in db.Picks
                                          where o.PlayerId == player.Id && o.GameId == game.Id
                                          select o.Id).FirstOrDefault();

                            newPick.PickResult = (newPick.TeamPicked == game.WinningTeam) ? "W" : null;

                            newPicks.Add(newPick);
                        }
                    }
                }

                foreach (var item in newPicks)
                {
                    if (item.Id != 0)
                    {
                        db.Entry(item).State = System.Data.EntityState.Modified;
                    }
                    else
                    {
                        db.Picks.Add(item);
                    }
                }
                db.SaveChanges();
            }
        }
    }
}