namespace Ttc.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Relations4 : DbMigration
    {
        public override void Up()
        {
            CreateIndex("dbo.kalender", "ThuisClubPloegID");
            AddForeignKey("dbo.kalender", "ThuisClubPloegID", "dbo.clubploeg", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.kalender", "ThuisClubPloegID", "dbo.clubploeg");
            DropIndex("dbo.kalender", new[] { "ThuisClubPloegID" });
        }
    }
}
