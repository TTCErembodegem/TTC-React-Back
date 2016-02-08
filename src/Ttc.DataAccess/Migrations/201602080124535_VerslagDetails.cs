namespace Ttc.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class VerslagDetails : DbMigration
    {
        public override void Up()
        {
            CreateIndex("verslagindividueel", "KalenderId");
        }
        
        public override void Down()
        {
            DropIndex("verslagindividueel", new[] { "KalenderId" });
        }
    }
}
