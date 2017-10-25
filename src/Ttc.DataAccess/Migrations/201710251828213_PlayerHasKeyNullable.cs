namespace Ttc.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PlayerHasKeyNullable : DbMigration
    {
        public override void Up()
        {
            AlterColumn("speler", "HasKey", c => c.Boolean());
            Sql("UPDATE speler SET HasKey=null WHERE HasKey=0");
        }
        
        public override void Down()
        {
            AlterColumn("speler", "HasKey", c => c.Boolean(nullable: false));
        }
    }
}
