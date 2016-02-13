namespace Ttc.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Relations3 : DbMigration
    {
        public override void Up()
        {
            CreateIndex("dbo.clubploegspeler", "SpelerId");
            AddForeignKey("dbo.clubploegspeler", "SpelerId", "dbo.speler", "Id");

            CreateIndex("dbo.clubploegspeler", "ClubPloegId");
            AddForeignKey("dbo.clubploegspeler", "ClubPloegId", "dbo.clubploeg", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.clubploegspeler", "SpelerId", "dbo.speler");
            DropIndex("dbo.clubploegspeler", new[] { "SpelerId" });

            DropForeignKey("dbo.clubploegspeler", "ClubPloegId", "dbo.speler");
            DropIndex("dbo.clubploegspeler", new[] { "ClubPloegId" });
        }
    }
}
