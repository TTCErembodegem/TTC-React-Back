namespace Ttc.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PlayerEntityStatus : DbMigration
    {
        public override void Up()
        {
            AddColumn("matchplayer", "Status", c => c.String(unicode: false));
        }
        
        public override void Down()
        {
            DropColumn("matchplayer", "Status");
        }
    }
}
