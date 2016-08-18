namespace Ttc.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class BlockMatchField : DbMigration
    {
        public override void Up()
        {
            AddColumn("matches", "Block", c => c.String(maxLength: 200, storeType: "nvarchar"));
        }
        
        public override void Down()
        {
            DropColumn("matches", "Block");
        }
    }
}
