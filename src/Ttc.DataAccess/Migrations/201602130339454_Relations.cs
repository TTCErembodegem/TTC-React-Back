namespace Ttc.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Relations : DbMigration
    {
        public override void Up()
        {
            CreateIndex("dbo.clublokaal", "ClubId");
            AddForeignKey("dbo.clublokaal", "ClubId", "dbo.club", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.clublokaal", "ClubId", "dbo.club");
            DropIndex("dbo.clublokaal", new[] { "ClubId" });
        }
    }
}
