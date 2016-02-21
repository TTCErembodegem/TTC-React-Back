namespace Ttc.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ReeksDerbyFix : DbMigration
    {
        public override void Up()
        {
            //DropTable("verslagspeler");
            //DropTable("verslagindividueel");
            //DropTable("verslag");
            //DropTable("kalender");

            //DropTable("clubploegspeler");
            //DropTable("clubploeg");
            //DropTable("reeks");
        }
        
        public override void Down()
        {
        }
    }
}
