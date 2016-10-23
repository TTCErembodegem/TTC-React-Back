namespace Ttc.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UserPasswordResetLinks : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "playerpasswordresetentity",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Guid = c.Guid(nullable: false),
                        ExpiresOn = c.DateTime(nullable: false, precision: 0),
                        PlayerId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("playerpasswordresetentity");
        }
    }
}
