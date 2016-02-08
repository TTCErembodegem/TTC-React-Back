namespace Ttc.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Verslagen2 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("verslag", "id", c => c.Int(nullable: false, identity: false));
            DropPrimaryKey("verslag");
            DropColumn("verslag", "id");
        }
        
        public override void Down()
        {
            AddColumn("verslag", "Id", c => c.Int(nullable: false));
            AddPrimaryKey("verslag", "id");
            AlterColumn("verslag", "Id", c => c.Int(nullable: false, identity: true));

        }
    }
}
