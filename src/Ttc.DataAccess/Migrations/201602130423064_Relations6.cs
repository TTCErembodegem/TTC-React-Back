namespace Ttc.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Relations6 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("kalender", "ThuisClubPloegId", "clubploeg");
            DropIndex("kalender", new[] { "ThuisClubPloegId" });
            AlterColumn("kalender", "ThuisClubPloegId", c => c.Int(nullable: false));
            CreateIndex("kalender", "ThuisClubPloegId");
            AddForeignKey("kalender", "ThuisClubPloegId", "clubploeg", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("kalender", "ThuisClubPloegId", "clubploeg");
            DropIndex("kalender", new[] { "ThuisClubPloegId" });
            AlterColumn("kalender", "ThuisClubPloegId", c => c.Int());
            CreateIndex("kalender", "ThuisClubPloegId");
            AddForeignKey("kalender", "ThuisClubPloegId", "clubploeg", "Id");
        }
    }
}
