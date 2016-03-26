namespace Ttc.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class HiddenMatchComments : DbMigration
    {
        public override void Up()
        {
            AddColumn("matchcomment", "Hidden", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("matchcomment", "Hidden");
        }
    }
}
