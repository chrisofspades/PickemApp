using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Globalization;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace PickemApp.Models
{
    public class vwWeeklyPlayerPicks
    {
        public int Id { get; set; }

        // Foreign Key to Player
        [ForeignKey("Player")]
        public int PlayerId { get; set; }
        public virtual Player Player { get; set; }
        public string PlayerName { get; set; }

        public int Week { get; set; }
        public int Year { get; set; }
        public int CorrectPicks { get; set; }
        public double TieBreaker { get; set; }
        public int Rank { get; set; }
        public int RankTies { get; set; }
        public int RankDense { get; set; }
    }
}