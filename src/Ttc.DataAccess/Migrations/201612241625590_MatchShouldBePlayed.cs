namespace Ttc.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MatchShouldBePlayed : DbMigration
    {
        public override void Up()
        {
            AddColumn("matches", "ShouldBePlayed", c => c.Boolean(nullable: false, defaultValue: true));

            Sql("UPDATE matches SET ShouldBePlayed=1");
        }
        
        public override void Down()
        {
            DropColumn("matches", "ShouldBePlayed");
        }
    }
}
