using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace PickemApp.Models
{
    public class PickemDBContext : DbContext
    {
        public PickemDBContext()
            : base("name=PickemDBContext")
        {
        }

        public DbSet<Player> Players { get; set; }

        public DbSet<Game> Games { get; set; }

        public DbSet<Pick> Picks { get; set; }
    }
}