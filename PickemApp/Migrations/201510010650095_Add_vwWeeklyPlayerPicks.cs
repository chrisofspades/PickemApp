namespace PickemApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Add_vwWeeklyPlayerPicks : DbMigration
    {
        public override void Up()
        {
            // Had to create this migration and then comment out the create table below
            // because I have already created vwWeeklyPlayerPicks on the database
            /*
            CreateTable(
                "dbo.vwWeeklyPlayerPicks",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PlayerId = c.Int(nullable: false),
                        PlayerName = c.String(),
                        Week = c.Int(nullable: false),
                        Year = c.Int(nullable: false),
                        CorrectPicks = c.Int(nullable: false),
                        TieBreaker = c.Double(nullable: false),
                        Rank = c.Int(nullable: false),
                        RankTies = c.Int(nullable: false),
                        RankDense = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Players", t => t.PlayerId, cascadeDelete: true)
                .Index(t => t.PlayerId);
             */
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.vwWeeklyPlayerPicks", "PlayerId", "dbo.Players");
            DropIndex("dbo.vwWeeklyPlayerPicks", new[] { "PlayerId" });
            DropTable("dbo.vwWeeklyPlayerPicks");
        }
    }
}
