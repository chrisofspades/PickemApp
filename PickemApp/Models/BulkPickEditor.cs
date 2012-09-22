using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PickemApp.Models
{
    public class BulkPickEditor
    {
        public int PlayerId { get; set; }
        public Player Player { get; set; }
        public int Year { get; set; }
        public int Week { get; set; }
        public IEnumerable<Pick> Picks { get; set; }
    }
}