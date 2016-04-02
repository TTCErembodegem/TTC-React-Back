namespace Ttc.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class BackupLegacyDatabase : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "backupreport",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        FrenoyMatchId = c.String(unicode: false),
                        Description = c.String(unicode: false),
                        PlayerId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "backupteamplayer",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PlayerId = c.Int(nullable: false),
                        DivisionLinkId = c.String(unicode: false),
                        TeamCode = c.String(unicode: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("backupteamplayer");
            DropTable("backupreport");
        }
    }
}
