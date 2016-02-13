namespace Ttc.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TEsty : DbMigration
    {
        public override void Up()
        {
            DropTable("dbo.verslagindividueel");
            DropTable("dbo.verslagspeler");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.verslagspeler",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        KalenderId = c.Int(nullable: false),
                        SpelerId = c.Int(nullable: false),
                        Winst = c.Int(),
                        Thuis = c.Int(nullable: false),
                        Positie = c.Int(nullable: false),
                        SpelerNaam = c.String(unicode: false),
                        Klassement = c.String(unicode: false),
                        UniqueIndex = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.verslagindividueel",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        KalenderId = c.Int(nullable: false),
                        MatchNummer = c.Int(nullable: false),
                        ThuisSpelerUniqueIndex = c.Int(nullable: false),
                        UitSpelerUniqueIndex = c.Int(nullable: false),
                        ThuisSpelerSets = c.Int(nullable: false),
                        UitSpelerSets = c.Int(nullable: false),
                        WalkOver = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);            
        }
    }
}
