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
    }
}