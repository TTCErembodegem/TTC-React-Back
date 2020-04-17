namespace Ttc.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ParamEndOfSeason : DbMigration
    {
        public override void Up()
        {
            Sql("INSERT INTO parameter(sleutel, value) VALUES('endOfSeason', 'false')");
        }
        
        public override void Down()
        {
            Sql("DELETE FROM parameter WHERE sleutel='endOfSeason'");
        }
    }
}
