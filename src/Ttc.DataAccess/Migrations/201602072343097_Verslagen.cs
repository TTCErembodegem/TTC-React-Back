namespace Ttc.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Verslagen : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.verslagindividueel", "KalenderId", c => c.Int(nullable: false));
            AddColumn("dbo.verslagspeler", "KalenderId", c => c.Int(nullable: false));
            AlterColumn("dbo.verslagspeler", "SpelerId", c => c.Int(nullable: false));
            AlterColumn("dbo.verslagspeler", "Thuis", c => c.Int(nullable: false));
            CreateIndex("dbo.verslagspeler", "KalenderId");
            CreateIndex("dbo.verslagspeler", "SpelerId");
            AddForeignKey("dbo.verslagspeler", "KalenderId", "dbo.kalender", "Id", cascadeDelete: true);
            AddForeignKey("dbo.verslagspeler", "SpelerId", "dbo.speler", "Id", cascadeDelete: true);
            DropColumn("dbo.verslagindividueel", "VerslagId");
            DropColumn("dbo.verslagspeler", "VerslagId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.verslagspeler", "VerslagId", c => c.Int(nullable: false));
            AddColumn("dbo.verslagindividueel", "VerslagId", c => c.Int(nullable: false));
            DropForeignKey("dbo.verslagspeler", "SpelerId", "dbo.speler");
            DropForeignKey("dbo.verslagspeler", "KalenderId", "dbo.kalender");
            DropIndex("dbo.verslagspeler", new[] { "SpelerId" });
            DropIndex("dbo.verslagspeler", new[] { "KalenderId" });
            AlterColumn("dbo.verslagspeler", "Thuis", c => c.Int());
            AlterColumn("dbo.verslagspeler", "SpelerId", c => c.Int());
            DropColumn("dbo.verslagspeler", "KalenderId");
            DropColumn("dbo.verslagindividueel", "KalenderId");
        }
    }
}
