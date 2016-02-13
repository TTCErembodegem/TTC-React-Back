namespace Ttc.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Relations2 : DbMigration
    {
        public override void Up()
        {
            CreateIndex("dbo.clubploeg", "ClubId");
            AddForeignKey("dbo.clubploeg", "ClubId", "dbo.club", "Id");

            CreateIndex("dbo.clubploeg", "ReeksId");
            AddForeignKey("dbo.clubploeg", "ReeksId", "dbo.reeks", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.clubploeg", "ClubId", "dbo.club");
            DropIndex("dbo.clubploeg", new[] { "ClubId" });

            DropForeignKey("dbo.clubploeg", "ReeksId", "dbo.club");
            DropIndex("dbo.clubploeg", new[] { "ReeksId" });
        }
    }
}
