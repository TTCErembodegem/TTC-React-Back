namespace Ttc.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CreateVerslag : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.verslagindividueel",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        MatchId = c.Int(nullable: false),
                        MatchNumber = c.Int(nullable: false),
                        HomePlayerUniqueIndex = c.Int(nullable: false),
                        HomePlayerSets = c.Int(nullable: false),
                        OutPlayerUniqueIndex = c.Int(nullable: false),
                        OutPlayerSets = c.Int(nullable: false),
                        WalkOver = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.verslagspeler",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        MatchId = c.Int(nullable: false),
                        PlayerId = c.Int(nullable: false),
                        Won = c.Int(),
                        Home = c.Boolean(nullable: false),
                        Position = c.Int(nullable: false),
                        Name = c.String(unicode: false),
                        Ranking = c.String(unicode: false),
                        UniqueIndex = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.verslagspeler");
            DropTable("dbo.verslagindividueel");
        }
    }
}
