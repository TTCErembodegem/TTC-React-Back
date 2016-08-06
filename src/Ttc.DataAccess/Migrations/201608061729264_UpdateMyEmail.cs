namespace Ttc.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateMyEmail : DbMigration
    {
        public override void Up()
        {
            Sql("UPDATE speler SET email='woutervs@hotmail.com' WHERE id=20");
        }
        
        public override void Down()
        {
        }
    }
}
