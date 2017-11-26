namespace Ttc.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PlayerRemoveOldFullNameColumn : DbMigration
    {
        public override void Up()
        {
            DropColumn("speler", "Naam");
        }
        
        public override void Down()
        {
            AddColumn("speler", "Naam", c => c.String(unicode: false));
        }
    }
}
