namespace Ttc.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MatchCommentWithImage : DbMigration
    {
        public override void Up()
        {
            AddColumn("matchcomment", "ImageUrl", c => c.String(maxLength: 100, storeType: "nvarchar"));
        }
        
        public override void Down()
        {
            DropColumn("matchcomment", "ImageUrl");
        }
    }
}
