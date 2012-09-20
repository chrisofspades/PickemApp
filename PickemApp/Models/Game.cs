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
    }
}