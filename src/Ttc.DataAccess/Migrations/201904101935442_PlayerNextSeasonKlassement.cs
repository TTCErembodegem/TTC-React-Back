namespace Ttc.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PlayerNextSeasonKlassement : DbMigration
    {
        public override void Up()
        {
            AddColumn("speler", "NextKlassementVttl", c => c.String(unicode: false));
            AddColumn("speler", "NextKlassementSporta", c => c.String(unicode: false));
            DropColumn("speler", "PreviousKlassementVttl");
            DropColumn("speler", "PreviousKlassementSporta");
        }
        
        public override void Down()
        {
            AddColumn("speler", "PreviousKlassementSporta", c => c.String(unicode: false));
            AddColumn("speler", "PreviousKlassementVttl", c => c.String(unicode: false));
            DropColumn("speler", "NextKlassementSporta");
            DropColumn("speler", "NextKlassementVttl");
        }
    }
}
