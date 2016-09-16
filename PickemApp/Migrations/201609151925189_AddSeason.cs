namespace PickemApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddSeason : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Seasons",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Year = c.Int(nullable: false),
                        StartDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Seasons");
        }
    }
}
