namespace Ttc.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Season17Load : DbMigration
    {
        public override void Up()
        {
            Sql("UPDATE club SET Naam='Erpe-Mere', CodeSporta=4052 WHERE ID=37");
        }
        
        public override void Down()
        {
        }
    }
}
