namespace Ttc.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MatchPlayerEntityStrings : DbMigration
    {
        public override void Up()
        {
            AlterColumn("matchplayer", "Name", c => c.String(maxLength: 50, storeType: "nvarchar"));
            AlterColumn("matchplayer", "Ranking", c => c.String(maxLength: 5, storeType: "nvarchar"));
        }
        
        public override void Down()
        {
            AlterColumn("matchplayer", "Ranking", c => c.String(unicode: false));
            AlterColumn("matchplayer", "Name", c => c.String(unicode: false));
        }
    }
}
