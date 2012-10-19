using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PickemApp.Models
{
    public class Game
    {
        public int Id { get; set; }
        public string Eid { get; set; }
        public string Gsis { get; set; }
        public int Week { get; set; }
        public int Year { get; set; }
        public string Day { get; set; }
        public string Time { get; set; }
        public string Quarter { get; set; }
        public string TimeRemaining { get; set; }
        public string HomeTeam { get; set; }
        public int HomeTeamScore { get; set; }
        public string VisitorTeam { get; set; }
        public int VisitorTeamScore { get; set; }
        public string GameType { get; set; }
        public string WinningTeam { get; set; }

        public string Status
        {
            get
            {
                string status = this.Quarter;
                switch (this.Quarter)
                {
                    case "F":
                        status = "Final";
                        break;
                    case "FO":
                        status = "Final/OT";
                        break;
                    case "1":
                        status = "1st";
                        break;
                    case "2":
                        status = "2nd";
                        break;
                    case "3":
                        status = "3rd";
                        break;
                    case "4":
                        status = "4th";
                        break;
                    case "P":
                        status = string.Format("{0} {1}", this.Day, this.Time);
                        break;
                }
                return status;
            }
        }
    }
}