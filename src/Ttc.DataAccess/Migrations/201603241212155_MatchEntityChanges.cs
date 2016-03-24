namespace Ttc.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MatchEntityChanges : DbMigration
    {
        public override void Up()
        {
            AddColumn("match", "FrenoyUniqueId", c => c.Int(nullable: false));
            AddColumn("match", "FrenoyDivisionId", c => c.Int(nullable: false));
            AddColumn("match", "FrenoySeason", c => c.Int(nullable: false));
            AddColumn("match", "Competition", c => c.Int(nullable: false));
            AddColumn("match", "AwayTeamCode", c => c.String(maxLength: 2, storeType: "nvarchar"));
            AlterColumn("match", "FrenoyMatchId", c => c.String(maxLength: 20, storeType: "nvarchar"));
            AlterColumn("match", "HomeTeamCode", c => c.String(maxLength: 2, storeType: "nvarchar"));
            CreateIndex("match", "Date");
            DropColumn("match", "AwayPloegCode");
        }
        
        public override void Down()
        {
            AddColumn("match", "AwayPloegCode", c => c.String(unicode: false));
            DropIndex("match", new[] { "Date" });
            AlterColumn("match", "HomeTeamCode", c => c.String(unicode: false));
            AlterColumn("match", "FrenoyMatchId", c => c.String(unicode: false));
            DropColumn("match", "AwayTeamCode");
            DropColumn("match", "Competition");
            DropColumn("match", "FrenoySeason");
            DropColumn("match", "FrenoyDivisionId");
            DropColumn("match", "FrenoyUniqueId");
        }
    }
}
