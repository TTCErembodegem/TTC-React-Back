namespace Ttc.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TeamCreator : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "match",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Date = c.DateTime(nullable: false, precision: 0),
                        Week = c.Int(nullable: false),
                        FrenoyMatchId = c.String(unicode: false),
                        HomeTeamId = c.Int(),
                        HomeClubId = c.Int(nullable: false),
                        HomeTeamCode = c.String(unicode: false),
                        AwayTeamId = c.Int(),
                        AwayClubId = c.Int(nullable: false),
                        AwayPloegCode = c.String(unicode: false),
                        ReportPlayerId = c.Int(nullable: false),
                        Description = c.String(unicode: false),
                        HomeScore = c.Int(),
                        AwayScore = c.Int(),
                        WalkOver = c.Boolean(nullable: false),
                        IsSyncedWithFrenoy = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("team", t => t.AwayTeamId)
                .ForeignKey("team", t => t.HomeTeamId)
                .Index(t => t.HomeTeamId)
                .Index(t => t.AwayTeamId);
            
            CreateTable(
                "team",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Competition = c.String(unicode: false),
                        Reeks = c.String(unicode: false),
                        ReeksType = c.String(unicode: false),
                        ReeksCode = c.String(unicode: false),
                        Year = c.Int(nullable: false),
                        LinkId = c.String(unicode: false),
                        FrenoyTeamId = c.String(unicode: false),
                        FrenoyDivisionId = c.Int(nullable: false),
                        TeamCode = c.String(unicode: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "teamopponent",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TeamId = c.Int(nullable: false),
                        ClubId = c.Int(nullable: false),
                        TeamCode = c.String(unicode: false),
                    })
                .PrimaryKey(t => t.Id)
                //.ForeignKey("club", t => t.ClubId, cascadeDelete: true)
                .ForeignKey("team", t => t.TeamId, cascadeDelete: true)
                .Index(t => t.TeamId)
                .Index(t => t.ClubId);
            
            CreateTable(
                "teamplayer",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PlayerType = c.Int(nullable: false),
                        PlayerId = c.Int(nullable: false),
                        TeamId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                //.ForeignKey("speler", t => t.PlayerId, cascadeDelete: true)
                .ForeignKey("team", t => t.TeamId, cascadeDelete: true)
                .Index(t => t.PlayerId)
                .Index(t => t.TeamId);
            
            CreateTable(
                "matchgame",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        MatchId = c.Int(nullable: false),
                        MatchNumber = c.Int(nullable: false),
                        HomePlayerUniqueIndex = c.Int(nullable: false),
                        HomePlayerSets = c.Int(nullable: false),
                        AwayPlayerUniqueIndex = c.Int(nullable: false),
                        AwayPlayerSets = c.Int(nullable: false),
                        WalkOver = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("match", t => t.MatchId, cascadeDelete: true)
                .Index(t => t.MatchId);
            
            CreateTable(
                "matchplayer",
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
                .PrimaryKey(t => t.Id)
                .ForeignKey("match", t => t.MatchId, cascadeDelete: true)
                //.ForeignKey("speler", t => t.PlayerId, cascadeDelete: true)
                .Index(t => t.MatchId)
                .Index(t => t.PlayerId);
            
        }
        
        public override void Down()
        {
            //DropForeignKey("matchplayer", "PlayerId", "dbo.speler");
            DropForeignKey("matchplayer", "MatchId", "dbo.match");
            DropForeignKey("match", "HomeTeamId", "dbo.team");
            DropForeignKey("matchgame", "MatchId", "dbo.match");
            DropForeignKey("match", "AwayTeamId", "dbo.team");
            DropForeignKey("teamplayer", "TeamId", "dbo.team");
            //DropForeignKey("teamplayer", "PlayerId", "dbo.speler");
            DropForeignKey("teamopponent", "TeamId", "dbo.team");
            //DropForeignKey("teamopponent", "ClubId", "dbo.club");
            //DropIndex("matchplayer", new[] { "PlayerId" });
            DropIndex("matchplayer", new[] { "MatchId" });
            DropIndex("matchgame", new[] { "MatchId" });
            DropIndex("teamplayer", new[] { "TeamId" });
            //DropIndex("teamplayer", new[] { "PlayerId" });
            //DropIndex("teamopponent", new[] { "ClubId" });
            DropIndex("teamopponent", new[] { "TeamId" });
            DropIndex("match", new[] { "AwayTeamId" });
            DropIndex("match", new[] { "HomeTeamId" });
            DropTable("matchplayer");
            DropTable("matchgame");
            DropTable("teamplayer");
            DropTable("teamopponent");
            DropTable("team");
            DropTable("match");
        }
    }
}
