namespace Ttc.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class VerslagPrimaryKey : DbMigration
    {
        public override void Up()
        {
            //delete from verslag where kalenderid in (126, 264, 322, 340, 717, 916, 1711);
            //--should no longer return rows:
            //--select kalenderid from verslag group by kalenderid having count(0) > 1;

            AddPrimaryKey("dbo.verslag", "KalenderId");
        }

        public override void Down()
        {
            DropPrimaryKey("dbo.verslag");
        }
    }
}
