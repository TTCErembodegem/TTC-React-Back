namespace Ttc.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MatchPlayerCustomStatusNote : DbMigration
    {
        public override void Up()
        {
            AddColumn("matchplayer", "StatusNote", c => c.String(maxLength: 300, storeType: "nvarchar"));
            AlterColumn("matchplayer", "Status", c => c.String(maxLength: 10, storeType: "nvarchar"));
        }
        
        public override void Down()
        {
            AlterColumn("matchplayer", "Status", c => c.String(unicode: false));
            DropColumn("matchplayer", "StatusNote");
        }
    }
}
