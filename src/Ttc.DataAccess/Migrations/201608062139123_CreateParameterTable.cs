namespace Ttc.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CreateParameterTable : DbMigration
    {
        public override void Up()
        {
            // But it is already created by vagrant so...
            Sql("INSERT INTO parameter VALUES ('SendGridApiKey', 'key', '')");
            Sql("INSERT INTO parameter VALUES ('FromEmail', 'info@ttc-erembodegem.be', '')");
        }
        
        public override void Down()
        {
        }
    }
}
