namespace Ttc.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MatchEntityTableRename : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "match", newName: "matches");
        }
        
        public override void Down()
        {
            RenameTable(name: "matches", newName: "match");
        }
    }
}
