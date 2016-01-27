namespace Ttc.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class VerslagIndividueelWalkOver : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.verslagindividueel", "WalkOver", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.verslagindividueel", "WalkOver");
        }
    }
}
