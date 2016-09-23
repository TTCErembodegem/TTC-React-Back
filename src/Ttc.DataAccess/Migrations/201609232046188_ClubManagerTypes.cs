namespace Ttc.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ClubManagerTypes : DbMigration
    {
        public override void Up()
        {
            Sql("UPDATE clubcontact SET Omschrijving='Chairman' WHERE Omschrijving='voorzitter'");
            Sql("UPDATE clubcontact SET Omschrijving='Secretary' WHERE Omschrijving='secretaris'");
            Sql("UPDATE clubcontact SET Omschrijving='Treasurer' WHERE Omschrijving='penningmeester'");
        }
        
        public override void Down()
        {
        }
    }
}
