namespace Ttc.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PlayerPrevSeasonKlassement : DbMigration
    {
        public override void Up()
        {
            AddColumn("speler", "PreviousKlassementVttl", c => c.String(unicode: false));
            AddColumn("speler", "PreviousKlassementSporta", c => c.String(unicode: false));
        }
        
        public override void Down()
        {
            DropColumn("speler", "PreviousKlassementSporta");
            DropColumn("speler", "PreviousKlassementVttl");
        }
    }
}
