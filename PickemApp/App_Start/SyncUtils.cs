using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using PickemApp.Models;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using System.Threading;
using System.Data;
using System.Data.Odbc;
using System.Data.OleDb;

using Microsoft.VisualBasic.FileIO;

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
                    game.HomeTeam = g.Attribute("h").Value.ToString().Replace("JAX", "JAC");
                    game.HomeTeamScore = Convert.ToInt32(g.Attribute("hs").Value.ToString());
                    game.VisitorTeam = g.Attribute("v").Value.ToString().Replace("JAX", "JAC");
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

                        db.Entry(game).State = System.Data.Entity.EntityState.Modified;
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
                    //Update pick results
                    UpdatePicks(Convert.ToInt32(week.Attribute("w").Value), Convert.ToInt32(week.Attribute("y").Value));

                    //Save XML to file if all games are completed
                    xml.Save(HttpContext.Current.Server.MapPath(VirtualPathUtility.ToAbsolute(string.Format("~/Content/datafiles/{0}Week{1}.xml", week.Attribute("y").Value, week.Attribute("w").Value))));
                }


            }

        }

        public static void UpdatePicks(int week, int year)
        {
            using (PickemDBContext db = new PickemDBContext())
            {
                var picks = (from p in db.Picks
                             join g in db.Games.Where(q => q.Week == week && q.Year == year && q.GameType == "REG") on p.GameId equals g.Id
                             select p).ToList<Pick>();

                foreach (Pick p in picks)
                {
                    p.PickResult = (p.TeamPicked == p.Game.WinningTeam) ? "W" : null;
                    db.Entry(p).State = System.Data.Entity.EntityState.Modified;
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
                                     where g.Week == week && g.Year == year && g.GameType == "REG" && g.HomeTeam == homeTeam && g.VisitorTeam == visitorTeam
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
                        db.Entry(item).State = System.Data.Entity.EntityState.Modified;
                    }
                    else
                    {
                        db.Picks.Add(item);
                    }
                }
                db.SaveChanges();
            }
        }

        public static void UpdatePicksXls(string xlsfile)
        {
            using (PickemDBContext db = new PickemDBContext())
            {
                //Get the year from filename (YYYY_NFL_Weeks.xls)
                int year;

                if (!int.TryParse(xlsfile.Substring(0, 4), out year))
                {
                    year = 0;
                }

                int week = 1;

                week = db.Games.Where(g => g.Year == year && g.GameType == "REG").Select(g => g.Week).Max();

                HttpContext.Current.Response.Write(string.Format("{0} {1}", year, week));

                xlsfile = HttpContext.Current.Server.MapPath(VirtualPathUtility.ToAbsolute("~/Content/datafiles/" + xlsfile));
                DataSet ds = ImportExcelXLS(xlsfile, false);

                DataTable dt;
                var tableNameLookup = string.Format("'WEEK {0}$'", week);
                if (ds.Tables.Contains(tableNameLookup))
                {
                    dt = ds.Tables[tableNameLookup];
                }
                else
                {
                    dt = ds.Tables[ds.Tables.Count - 1];
                }


                //Year and Week data will be in the first row, column F3, formatted "YYYY / WEEK #"
                string[] weekData = dt.Rows[0]["F3"].ToString().Split('/');

                if (weekData.Length == 2)
                {
                    if (year < 0)
                    {
                        year = Convert.ToInt32(weekData[0].Trim());
                    }
                    week = Convert.ToInt32(weekData[1].ToUpper().Replace("WEEK", "").Trim());
                }
                else if (weekData.Length == 1)
                {
                    week = Convert.ToInt32(weekData[0].ToUpper().Replace("WEEK", "").Trim());
                }

                HttpContext.Current.Response.Write(string.Format("{0} {1}", year, week));


                //Create a dictionary of games, where the key is the column name
                //Games will start in column 2, last column is for scores
                int gameColIndexLower = 2,
                    gameColIndexUpper = dt.Columns.Count - 2;  //-2 because index is zero-based

                Dictionary<string, Game> dictGames = new Dictionary<string, Game>();
                for (int i = gameColIndexLower; i <= gameColIndexUpper; i++)
                {
                    //Visitor team is row 4, Home team is row 5
                    string homeTeam = dt.Rows[4][i].ToString();
                    string visitorTeam = dt.Rows[3][i].ToString();

                    //Find the game record
                    Game game = (from g in db.Games
                                 where g.Week == week && g.Year == year && g.GameType == "REG" && g.HomeTeam == homeTeam && g.VisitorTeam == visitorTeam
                                 select g).FirstOrDefault();

                    dictGames.Add(dt.Columns[i].ColumnName, game);
                }

                List<Pick> newPicks = new List<Pick>();

                foreach (DataRow row in dt.Rows)
                {
                    if (!string.IsNullOrEmpty(row["F2"].ToString()))
                    {
                        var playerName = row["F2"].ToString();
                        Player player = db.Players.Where(q => q.Name.ToLower() == playerName.ToLower()).FirstOrDefault();

                        if (player != null)
                        {
                            for (int i = gameColIndexLower; i <= gameColIndexUpper; i++)
                            {
                                string teamPicked = row[dt.Columns[i].ColumnName].ToString();
                                Game game = dictGames[dt.Columns[i].ColumnName];

                                if (game != null && !string.IsNullOrEmpty(teamPicked))
                                {
                                    double totalPoints = 0;

                                    //Total points will be in the column next to the final game
                                    if (i == gameColIndexUpper)
                                    {
                                        totalPoints = Convert.ToDouble(row[i + 1].ToString());
                                    }

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
                    }
                }

                foreach (var item in newPicks)
                {
                    if (item.Id != 0)
                    {
                        db.Entry(item).State = System.Data.Entity.EntityState.Modified;
                    }
                    else
                    {
                        db.Picks.Add(item);
                    }
                }
                db.SaveChanges();
            }
        }

        public static void UpdatePicksCsv(string csvfile)
        {
            using (PickemDBContext db = new PickemDBContext())
            {
                csvfile = HttpContext.Current.Server.MapPath(VirtualPathUtility.ToAbsolute("~/Content/datafiles/" + csvfile));
                using (TextFieldParser parser = new TextFieldParser(csvfile))
                {
                    parser.TextFieldType = FieldType.Delimited;
                    parser.SetDelimiters(",");

                    // Read the first row. It contains week, year, and game data
                    var firstRow = parser.ReadFields();

                    // Setup year and week.
                    int year = 0;
                    int week = 0;

                    var firstCol = firstRow[0].Split('-');
                    if (firstCol.Length == 2)
                    {
                        int.TryParse(firstCol[0].Trim(), out year);
                        int.TryParse(firstCol[1].Trim(), out week);
                    }

                    if (year == 0)
                    {
                        year = db.Seasons.Select(y => y.Year).Max();
                    }
                    if (week == 0)
                    {
                        week = db.Games.Where(g => g.Year == year && g.GameType == "REG").Select(g => g.Week).Max();
                    }

                    // Create a dictionary of games, where the key is the index
                    Dictionary<int, Game> dictGames = new Dictionary<int, Game>();
                    for (int i = 1; i < firstRow.Length; i++)
                    {
                        if (firstRow[i].Trim().Length == 0)
                        {
                            continue;
                        }

                        var teams = firstRow[i].Trim().Split('-');
                        if (teams.Length != 2)
                        {
                            continue;
                        }

                        var homeTeam = teams[1];
                        var visitorTeam = teams[0];

                        //Find the game record
                        Game game = (from g in db.Games
                                     where g.Week == week && g.Year == year && g.GameType == "REG" && g.HomeTeam == homeTeam && g.VisitorTeam == visitorTeam
                                     select g).FirstOrDefault();

                        dictGames.Add(i, game);
                    }

                    // Get the picks
                    List<Pick> newPicks = new List<Pick>();

                    while (!parser.EndOfData)
                    {
                        var fields = parser.ReadFields();

                        // Get player (first field)
                        var playerName = fields[0];
                        Player player = db.Players.Where(q => q.Name.ToLower() == playerName.ToLower()).FirstOrDefault();

                        if (player == null)
                        {
                            continue;
                        }

                        for (int i = 1; i < fields.Length - 1; i++)
                        {
                            var teamPicked = fields[i];
                            Game game = dictGames[i];

                            if (game != null && !string.IsNullOrEmpty(teamPicked))
                            {
                                double totalPoints = 0;

                                //Total points will be in the column next to the final game
                                if (i == fields.Length - 2)
                                {
                                    totalPoints = Convert.ToDouble(fields[i + 1]);
                                }

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

                    // Save picks
                    foreach (var item in newPicks)
                    {
                        if (item.Id != 0)
                        {
                            db.Entry(item).State = System.Data.Entity.EntityState.Modified;
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

        public static DataSet ImportExcelXLS(string FileName, bool hasHeaders)
        {
            string HDR = hasHeaders ? "Yes" : "No";
            string strConn;
            if (FileName.Substring(FileName.LastIndexOf('.')).ToLower() == ".xlsx" || !HttpContext.Current.Request.IsLocal)
                strConn = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + FileName + ";Extended Properties=\"Excel 12.0;HDR=" + HDR + ";IMEX=0\"";
            else
                strConn = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + FileName + ";Extended Properties=\"Excel 8.0;HDR=" + HDR + ";IMEX=1\"";

            DataSet output = new DataSet();

            using (OleDbConnection conn = new OleDbConnection(strConn))
            {
                conn.Open();

                DataTable schemaTable = conn.GetOleDbSchemaTable(
                    OleDbSchemaGuid.Tables, new object[] { null, null, null, "TABLE" });

                if (schemaTable.Rows.Count == 0)
                {
                    DataRow row = schemaTable.NewRow();
                    row["TABLE_NAME"] = "WEEK 9";
                    schemaTable.Rows.Add(row);
                }

                foreach (DataRow schemaRow in schemaTable.Rows)
                {
                    string sheet = schemaRow["TABLE_NAME"].ToString();

                    if (!sheet.EndsWith("_"))
                    {
                        try
                        {
                            OleDbCommand cmd = new OleDbCommand("SELECT * FROM [" + sheet + "]", conn);
                            cmd.CommandType = CommandType.Text;

                            DataTable outputTable = new DataTable(sheet);
                            output.Tables.Add(outputTable);
                            new OleDbDataAdapter(cmd).Fill(outputTable);
                        }
                        catch (Exception ex)
                        {
                            throw new Exception(ex.Message + string.Format("Sheet:{0}.File:F{1}", sheet, FileName), ex);
                        }
                    }
                }
            }

            return output;
        }
    }

    public class AsyncHelper
    {
        class TargetInfo
        {
            internal TargetInfo(Delegate d, object[] args)
            {
                Target = d;
                Args = args;
            }

            internal readonly Delegate Target;
            internal readonly object[] Args;
        }

        private static WaitCallback dynamicInvokeShim = new WaitCallback(DynamicInvokeShim);

        public static void FireAndForget(Delegate d, params object[] args)
        {
            ThreadPool.QueueUserWorkItem(dynamicInvokeShim, new TargetInfo(d, args));
        }

        static void DynamicInvokeShim(object o)
        {
            try
            {
                TargetInfo ti = (TargetInfo)o;
                ti.Target.DynamicInvoke(ti.Args);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.ToString());
            }
        }
    }
}