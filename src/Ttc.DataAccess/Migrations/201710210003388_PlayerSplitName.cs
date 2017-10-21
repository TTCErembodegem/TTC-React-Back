namespace Ttc.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PlayerSplitName : DbMigration
    {
        public override void Up()
        {
            AddColumn("speler", "FirstName", c => c.String(maxLength: 100, storeType: "nvarchar"));
            AddColumn("speler", "LastName", c => c.String(maxLength: 100, storeType: "nvarchar"));

            Sql("update speler set LastName=LEFT(Naam, LENGTH(Naam) - LOCATE(' ', REVERSE(Naam)))");
            Sql("update speler set FirstName=substr(Naam, LENGTH(Naam) - LOCATE(' ', REVERSE(Naam)) + 2)");
        }
        
        public override void Down()
        {
            DropColumn("speler", "LastName");
            DropColumn("speler", "FirstName");
        }
    }
}
