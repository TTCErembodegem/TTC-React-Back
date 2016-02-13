namespace Ttc.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Relations7 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("clubploeg", "ReeksId", "reeks");
            DropForeignKey("clubploegspeler", "ClubPloegId", "clubploeg");
            DropIndex("clubploeg", new[] { "ReeksId" });
            DropIndex("clubploegspeler", new[] { "ClubPloegId" });
            AlterColumn("clubploeg", "ReeksId", c => c.Int(nullable: false));
            AlterColumn("clubploegspeler", "ClubPloegId", c => c.Int(nullable: false));
            CreateIndex("clubploeg", "ReeksId");
            CreateIndex("clubploegspeler", "ClubPloegId");
            AddForeignKey("clubploeg", "ReeksId", "reeks", "Id", cascadeDelete: true);
            AddForeignKey("clubploegspeler", "ClubPloegId", "clubploeg", "Id", cascadeDelete: true);


            CreateIndex("verslag", "SpelerId");
            AddForeignKey("verslag", "SpelerId", "speler", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("clubploegspeler", "ClubPloegId", "clubploeg");
            DropForeignKey("clubploeg", "ReeksId", "reeks");
            DropIndex("clubploegspeler", new[] { "ClubPloegId" });
            DropIndex("clubploeg", new[] { "ReeksId" });
            AlterColumn("clubploegspeler", "ClubPloegId", c => c.Int());
            AlterColumn("clubploeg", "ReeksId", c => c.Int());
            CreateIndex("clubploegspeler", "ClubPloegId");
            CreateIndex("clubploeg", "ReeksId");
            AddForeignKey("clubploegspeler", "ClubPloegId", "clubploeg", "Id");
            AddForeignKey("clubploeg", "ReeksId", "reeks", "Id");


            DropForeignKey("verslag", "SpelerId", "speler");
            DropIndex("verslag", new[] { "SpelerId" });
        }
    }
}
