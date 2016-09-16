using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.Data.SqlClient;

namespace PickemApp.Models
{
    public class PickemDBContext : DbContext
    {
        public PickemDBContext()
            : base(PickemDBContext.GetConnectionString())
        {
        }

        public static string GetConnectionString()
        {
            SqlConnectionStringBuilder conn = new SqlConnectionStringBuilder(ConfigurationManager.ConnectionStrings["PickemDBContext"].ConnectionString);
            conn.MultipleActiveResultSets = true;

            return conn.ConnectionString;
        }

        public DbSet<Player> Players { get; set; }

        public DbSet<Game> Games { get; set; }

        public DbSet<Pick> Picks { get; set; }

        public DbSet<Season> Seasons { get; set; }

        public DbSet<vwWeeklyPlayerPicks> vwWeeklyPlayerPicks { get; set; }

    }
}