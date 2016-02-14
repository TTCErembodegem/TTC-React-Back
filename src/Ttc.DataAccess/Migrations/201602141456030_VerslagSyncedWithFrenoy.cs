namespace Ttc.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class VerslagSyncedWithFrenoy : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.verslag", "IsSyncedWithFrenoy", c => c.Boolean(nullable: false));
            DropColumn("dbo.verslag", "Details");
        }
        
        public override void Down()
        {
            AddColumn("dbo.verslag", "Details", c => c.Int());
            DropColumn("dbo.verslag", "IsSyncedWithFrenoy");
        }
    }
}
