namespace PickemApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddForeignKeys : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Picks", "Player_ID", "dbo.Players");
            DropForeignKey("dbo.Picks", "Game_ID", "dbo.Games");
            DropIndex("dbo.Picks", new[] { "Player_ID" });
            DropIndex("dbo.Picks", new[] { "Game_ID" });
            RenameColumn(table: "dbo.Picks", name: "Player_ID", newName: "PlayerId");
            RenameColumn(table: "dbo.Picks", name: "Game_ID", newName: "GameId");
            AlterColumn("dbo.Players", "Id", c => c.Int(nullable: false, identity: true));
            AlterColumn("dbo.Games", "Id", c => c.Int(nullable: false, identity: true));
            AlterColumn("dbo.Picks", "Id", c => c.Int(nullable: false, identity: true));
            DropPrimaryKey("dbo.Players", new[] { "ID" });
            AddPrimaryKey("dbo.Players", "Id");
            DropPrimaryKey("dbo.Games", new[] { "ID" });
            AddPrimaryKey("dbo.Games", "Id");
            DropPrimaryKey("dbo.Picks", new[] { "ID" });
            AddPrimaryKey("dbo.Picks", "Id");
            AddForeignKey("dbo.Picks", "PlayerId", "dbo.Players", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Picks", "GameId", "dbo.Games", "Id", cascadeDelete: true);
            CreateIndex("dbo.Picks", "PlayerId");
            CreateIndex("dbo.Picks", "GameId");
        }
        
        public override void Down()
        {
            DropIndex("dbo.Picks", new[] { "GameId" });
            DropIndex("dbo.Picks", new[] { "PlayerId" });
            DropForeignKey("dbo.Picks", "GameId", "dbo.Games");
            DropForeignKey("dbo.Picks", "PlayerId", "dbo.Players");
            DropPrimaryKey("dbo.Picks", new[] { "Id" });
            AddPrimaryKey("dbo.Picks", "ID");
            DropPrimaryKey("dbo.Games", new[] { "Id" });
            AddPrimaryKey("dbo.Games", "ID");
            DropPrimaryKey("dbo.Players", new[] { "Id" });
            AddPrimaryKey("dbo.Players", "ID");
            AlterColumn("dbo.Picks", "ID", c => c.Int(nullable: false, identity: true));
            AlterColumn("dbo.Games", "ID", c => c.Int(nullable: false, identity: true));
            AlterColumn("dbo.Players", "ID", c => c.Int(nullable: false, identity: true));
            RenameColumn(table: "dbo.Picks", name: "GameId", newName: "Game_ID");
            RenameColumn(table: "dbo.Picks", name: "PlayerId", newName: "Player_ID");
            CreateIndex("dbo.Picks", "Game_ID");
            CreateIndex("dbo.Picks", "Player_ID");
            AddForeignKey("dbo.Picks", "Game_ID", "dbo.Games", "ID");
            AddForeignKey("dbo.Picks", "Player_ID", "dbo.Players", "ID");
        }
    }
}
