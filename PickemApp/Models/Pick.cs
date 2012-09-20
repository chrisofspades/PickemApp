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
    public class Pick
    {
        public int Id { get; set; }

        // Foreign Key to Player
        [ForeignKey("Player")]
        public int PlayerId { get; set; }
        public virtual Player Player { get; set; }

        // Foreign Key to Game
        [ForeignKey("Game")]
        public int GameId { get; set; }
        public virtual Game Game { get; set; }
        
        public string TeamPicked { get; set; }
        public int TotalPoints { get; set; }
        public string PickResult { get; set; }
    }
}