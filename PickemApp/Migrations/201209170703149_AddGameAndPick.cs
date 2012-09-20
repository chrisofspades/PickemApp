namespace PickemApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddGameAndPick : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Games",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Eid = c.String(),
                        Gsis = c.String(),
                        Week = c.Int(nullable: false),
                        Year = c.Int(nullable: false),
                        Day = c.String(),
                        Time = c.String(),
                        Quarter = c.String(),
                        TimeRemaining = c.String(),
                        HomeTeam = c.String(),
                        HomeTeamScore = c.Int(nullable: false),
                        VisitorTeam = c.String(),
                        VisitorTeamScore = c.Int(nullable: false),
                        GameType = c.String(),
                        WinningTeam = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.Picks",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        TeamPicked = c.String(),
                        TotalPoints = c.Int(nullable: false),
                        PickResult = c.String(),
                        Player_ID = c.Int(),
                        Game_ID = c.Int(),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Players", t => t.Player_ID)
                .ForeignKey("dbo.Games", t => t.Game_ID)
                .Index(t => t.Player_ID)
                .Index(t => t.Game_ID);
            
        }
        
        public override void Down()
        {
            DropIndex("dbo.Picks", new[] { "Game_ID" });
            DropIndex("dbo.Picks", new[] { "Player_ID" });
            DropForeignKey("dbo.Picks", "Game_ID", "dbo.Games");
            DropForeignKey("dbo.Picks", "Player_ID", "dbo.Players");
            DropTable("dbo.Picks");
            DropTable("dbo.Games");
        }
    }
}
