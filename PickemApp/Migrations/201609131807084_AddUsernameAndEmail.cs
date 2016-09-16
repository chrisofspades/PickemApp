namespace PickemApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddUsernameAndEmail : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Players", "Username", c => c.String());
            AddColumn("dbo.Players", "Email", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Players", "Email");
            DropColumn("dbo.Players", "Username");
        }
    }
}
