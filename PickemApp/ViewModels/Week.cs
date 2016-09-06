using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using PickemApp.Models;

namespace PickemApp.ViewModels
{
    public class WeekIndex
    {
        public Week CurrentWeek { get; set; }
        public List<Game> Games { get; set; }
        public List<Week> Weeks { get; set; }
        public List<WeeklyPlayerPicks> Leaders { get; set; }
    }

    public class Week
    {
        public int WeekNumber { get; set; }
        public int Year { get; set; }

        public Week()
        {

        }

        public Week(int week, int year)
        {
            WeekNumber = week;
            Year = year;
        }
    }
}