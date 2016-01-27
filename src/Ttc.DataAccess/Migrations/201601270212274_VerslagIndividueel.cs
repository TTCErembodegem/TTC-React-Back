namespace Ttc.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class VerslagIndividueel : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.verslagindividueel",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        VerslagId = c.Int(nullable: false),
                        MatchNummer = c.Int(nullable: false),
                        ThuisSpelerUniqueIndex = c.Int(nullable: false),
                        UitSpelerUniqueIndex = c.Int(nullable: false),
                        ThuisSpelerSets = c.Int(nullable: false),
                        UitSpelerSets = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.verslagindividueel");
        }
    }
}
