namespace Ttc.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class StartCompetition20192020 : DbMigration
    {
        public override void Up()
        {
            Sql("DELETE FROM parameter WHERE sleutel IN ('Frenoy_SeasonId', 'stduur', 'frenoy_wsdlUrlSporta', 'frenoy_wsdlUrlSporta')");
            Sql("DELETE FROM parameter WHERE sleutel LIKE 'link%'");

            Sql("UPDATE parameter SET sleutel='year', value='2018' WHERE sleutel='jaar'");
        }
        
        public override void Down()
        {
        }
    }
}
