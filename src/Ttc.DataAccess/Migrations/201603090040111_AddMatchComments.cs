namespace Ttc.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddMatchComments : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "matchcomment",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PostedOn = c.DateTime(nullable: false, precision: 0),
                        Text = c.String(unicode: false),
                        MatchId = c.Int(nullable: false),
                        PlayerId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("match", t => t.MatchId, cascadeDelete: true)
                .Index(t => t.MatchId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("matchcomment", "MatchId", "match");
            DropIndex("matchcomment", new[] { "MatchId" });
            DropTable("matchcomment");
        }
    }
}
