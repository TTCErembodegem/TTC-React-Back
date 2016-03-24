namespace Ttc.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MatchEntityFrenoyUniqueIdCanBeAbsent : DbMigration
    {
        public override void Up()
        {
            DropColumn("matches", "FrenoyUniqueId");
        }
        
        public override void Down()
        {
            AddColumn("matches", "FrenoyUniqueId", c => c.Int(nullable: false));
        }
    }
}
