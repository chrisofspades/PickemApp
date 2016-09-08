using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using PickemApp.Models;

namespace PickemApp.ViewModels
{
    public class HistoryIndex
    {
        public List<Season> Seasons { get; set; }
    }

    public class Season
    {
        public int Year { get; set; }
        public Player First { get; set; }
        public Player Second { get; set; }
        public Player Third { get; set; }
    }
}