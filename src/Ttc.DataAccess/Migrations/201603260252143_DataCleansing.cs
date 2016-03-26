namespace Ttc.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DataCleansing : DbMigration
    {
        public override void Up()
        {
            Sql("update speler set GSM=replace(GSM, '/', '')");
            Sql("update speler set GSM = replace(GSM, '.', '')");
            Sql("update speler set GSM = replace(GSM, ' ', '')");

            Sql("update clublokaal set telefoon = replace(telefoon, '/', '')");
            Sql("update clublokaal set telefoon = replace(telefoon, '.', '')");
            Sql("update clublokaal set telefoon = replace(telefoon, ' ', '')");

            Sql("UPDATE club SET Naam= 'Gent Het Netje Over' WHERE  Naam='TTC Het Netje Over Gent'");
        }
        
        public override void Down()
        {
        }
    }
}
