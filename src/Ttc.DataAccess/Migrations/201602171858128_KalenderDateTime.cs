namespace Ttc.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class KalenderDateTime : DbMigration
    {
        public override void Up()
        {
            AlterColumn("kalender", "Datum", b => b.DateTime(nullable: false, storeType: "DATETIME"));

            Sql("update kalender set datum=datum+uur");

            DropColumn("kalender", "Uur");
        }

        public override void Down()
        {
            AddColumn("kalender", "Uur", b => b.Time());

            Sql("update kalender set uur=datum");

            AlterColumn("kalender", "Datum", b => b.DateTime(nullable: false));
        }
    }
}
