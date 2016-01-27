namespace Ttc.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class VerslagSpelerPositionAndUniqueIndex : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.verslagspeler", "Positie", c => c.Int(nullable: false));
            AddColumn("dbo.verslagspeler", "UniqueIndex", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.verslagspeler", "UniqueIndex");
            DropColumn("dbo.verslagspeler", "Positie");
        }
    }
}
