using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

using PickemApp.Models;

namespace PickemApp.ViewModels
{
    public class PicksIndex
    {
        public Player CurrentPlayer { get; set; }
        public Week CurrentWeek { get; set; }
        public List<PickRadio> Picks { get; set; }
        public List<Week> Weeks { get; set; }
    }

    public class PickRadio
    {
        public int Id { get; set; }

        [Required]
        public string TeamPicked { get; set; }

        public double TotalPoints { get; set; }
        public string PickResult { get; set; }
        public int GameId { get; set; }
        public string HomeTeam { get; set; }
        public string VisitorTeam { get; set; }
        public string WinningTeam { get; set; }
    }
}