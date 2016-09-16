namespace PickemApp.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    using PickemApp.Models;

    internal sealed class Configuration : DbMigrationsConfiguration<PickemApp.Models.PickemDBContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
        }

        protected override void Seed(PickemApp.Models.PickemDBContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );
            //

            context.Seasons.AddOrUpdate(
                s => s.Year,
                new Season { Year = 2012, StartDate = DateTime.Parse("2012-09-05") },
                new Season { Year = 2013, StartDate = DateTime.Parse("2013-09-05") },
                new Season { Year = 2014, StartDate = DateTime.Parse("2014-09-04") },
                new Season { Year = 2015, StartDate = DateTime.Parse("2015-09-10") },
                new Season { Year = 2016, StartDate = DateTime.Parse("2016-09-08") }
            );

        }
    }
}
