namespace Ttc.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class IncorrectPlayerInfos : DbMigration
    {
        public override void Up()
        {
            Sql("UPDATE speler SET email=\"guyjacobs7@gmail.com\" where id=28");
            Sql("UPDATE speler set email=\"etienne.cornu@telenet.be\" where id=30");
            Sql("UPDATE speler set email=\"hugo.redant@telenet.be\" where id=79");

        }

        public override void Down()
        {
        }
    }
}
