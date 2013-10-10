namespace PickemApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChangeTotalPointsDataType : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Picks", "TotalPoints", c => c.Double(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Picks", "TotalPoints", c => c.Int(nullable: false));
        }
    }
}
