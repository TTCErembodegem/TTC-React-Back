namespace Ttc.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MatchFormationComment : DbMigration
    {
        public override void Up()
        {
            AddColumn("matches", "FormationComment", c => c.String(unicode: false));
        }
        
        public override void Down()
        {
            DropColumn("matches", "FormationComment");
        }
    }
}
