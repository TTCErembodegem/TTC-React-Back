namespace Ttc.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ReeksKiller : DbMigration
    {
        public override void Up()
        {
            DropTable("clubploegspeler");
            DropTable("reeks");
            DropTable("kalender");
            DropTable("verslag");
            DropTable("verslagindividueel");
            DropTable("verslagspeler");
            DropTable("clubploeg");
        }
        
        public override void Down()
        {
            CreateTable(
                "clubploeg",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ReeksId = c.Int(),
                        ClubId = c.Int(),
                        Code = c.String(unicode: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "verslagspeler",
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
            
            CreateTable(
                "verslagindividueel",
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
                "verslag",
                c => new
                    {
                        KalenderId = c.Int(nullable: false),
                        SpelerId = c.Int(nullable: false),
                        Beschrijving = c.String(unicode: false),
                        UitslagThuis = c.Int(),
                        UitslagUit = c.Int(),
                        WO = c.Int(nullable: false),
                        IsSyncedWithFrenoy = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.KalenderId);
            
            CreateTable(
                "kalender",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Datum = c.DateTime(nullable: false, precision: 0),
                        Thuis = c.Int(),
                        Week = c.Int(),
                        FrenoyMatchId = c.String(unicode: false),
                        ThuisClubId = c.Int(),
                        ThuisPloeg = c.String(unicode: false),
                        ReeksId = c.Int(nullable: false),
                        UitClubId = c.Int(),
                        UitPloeg = c.String(unicode: false),
                        UitClubPloegId = c.Int(),
                        Beschrijving = c.String(unicode: false),
                        GeleideTraining = c.String(unicode: false),
                        ClubPloeg_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "reeks",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Competitie = c.String(unicode: false),
                        Reeks = c.String(unicode: false),
                        ReeksType = c.String(unicode: false),
                        ReeksCode = c.String(unicode: false),
                        Jaar = c.Int(),
                        LinkId = c.String(unicode: false),
                        FrenoyTeamId = c.String(unicode: false),
                        FrenoyDivisionId = c.Int(nullable: false),
                        TeamCode = c.String(unicode: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "clubploegspeler",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Kapitein = c.Int(nullable: false),
                        SpelerId = c.Int(),
                        ReeksId = c.Int(nullable: false),
                        ClubPloeg_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateIndex("clubploeg", "ClubId");
            CreateIndex("clubploeg", "ReeksId");
            CreateIndex("verslagspeler", "PlayerId");
            CreateIndex("verslagspeler", "MatchId");
            CreateIndex("verslagindividueel", "MatchId");
            CreateIndex("verslag", "KalenderId");
            CreateIndex("kalender", "ClubPloeg_Id");
            CreateIndex("kalender", "ReeksId");
            CreateIndex("clubploegspeler", "ClubPloeg_Id");
            CreateIndex("clubploegspeler", "ReeksId");
            CreateIndex("clubploegspeler", "SpelerId");
            AddForeignKey("clubploegspeler", "SpelerId", "dbo.speler", "Id");
            AddForeignKey("clubploegspeler", "ReeksId", "dbo.reeks", "Id", cascadeDelete: true);
            AddForeignKey("clubploegspeler", "ClubPloeg_Id", "dbo.clubploeg", "Id");
            AddForeignKey("clubploeg", "ReeksId", "dbo.reeks", "Id");
            AddForeignKey("kalender", "ClubPloeg_Id", "dbo.clubploeg", "Id");
            AddForeignKey("clubploeg", "ClubId", "dbo.club", "Id");
            AddForeignKey("verslag", "KalenderId", "dbo.kalender", "Id");
            AddForeignKey("verslagspeler", "MatchId", "dbo.verslag", "KalenderId", cascadeDelete: true);
            AddForeignKey("verslagspeler", "PlayerId", "dbo.speler", "Id", cascadeDelete: true);
            AddForeignKey("verslagindividueel", "MatchId", "dbo.verslag", "KalenderId", cascadeDelete: true);
            AddForeignKey("kalender", "ReeksId", "dbo.reeks", "Id", cascadeDelete: true);
        }
    }
}
