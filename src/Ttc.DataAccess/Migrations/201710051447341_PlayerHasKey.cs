namespace Ttc.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PlayerHasKey : DbMigration
    {
        public override void Up()
        {
            AddColumn("speler", "HasKey", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("speler", "HasKey");
        }
    }
}
