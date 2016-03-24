namespace Ttc.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TeamEntityStrings : DbMigration
    {
        public override void Up()
        {
            AlterColumn("team", "Competition", c => c.String(maxLength: 10, storeType: "nvarchar"));
            AlterColumn("team", "Reeks", c => c.String(maxLength: 2, storeType: "nvarchar"));
            AlterColumn("team", "ReeksType", c => c.String(maxLength: 10, storeType: "nvarchar"));
            AlterColumn("team", "ReeksCode", c => c.String(maxLength: 2, storeType: "nvarchar"));
            AlterColumn("team", "LinkId", c => c.String(maxLength: 10, storeType: "nvarchar"));
            AlterColumn("team", "FrenoyTeamId", c => c.String(maxLength: 10, storeType: "nvarchar"));
            AlterColumn("team", "TeamCode", c => c.String(maxLength: 2, storeType: "nvarchar"));
            AlterColumn("teamopponent", "TeamCode", c => c.String(maxLength: 2, storeType: "nvarchar"));
        }
        
        public override void Down()
        {
            AlterColumn("teamopponent", "TeamCode", c => c.String(unicode: false));
            AlterColumn("team", "TeamCode", c => c.String(unicode: false));
            AlterColumn("team", "FrenoyTeamId", c => c.String(unicode: false));
            AlterColumn("team", "LinkId", c => c.String(unicode: false));
            AlterColumn("team", "ReeksCode", c => c.String(unicode: false));
            AlterColumn("team", "ReeksType", c => c.String(unicode: false));
            AlterColumn("team", "Reeks", c => c.String(unicode: false));
            AlterColumn("team", "Competition", c => c.String(unicode: false));
        }
    }
}
