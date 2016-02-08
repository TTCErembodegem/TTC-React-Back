namespace Ttc.DataAccess.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<Ttc.DataAccess.TtcDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;

            //SetSqlGenerator("MySql.Data.MySqlClient", new MySql.Data.Entity.MySqlMigrationSqlGenerator());
        }

        protected override void Seed(Ttc.DataAccess.TtcDbContext context)
        {
            //context.Database.ExecuteSqlCommand("DELETE FROM dbo.verslagspeler");
            //context.Database.ExecuteSqlCommand("DELETE FROM dbo.verslagindividueel");

            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );
            //
        }
    }

    //CodeGenerator = new MyCodeGenerator();
    //class MyCodeGenerator : CSharpMigrationCodeGenerator
    //{
    //    protected override void Generate(
    //        DropIndexOperation dropIndexOperation, IndentedTextWriter writer)
    //    {
    //        dropIndexOperation.Table = StripDbo(dropIndexOperation.Table);

    //        base.Generate(dropIndexOperation, writer);
    //    }

    //    // TODO: Override other Generate overloads that involve table names

    //    private string StripDbo(string table)
    //    {
    //        if (table.StartsWith("dbo."))
    //        {
    //            return table.Substring(4);
    //        }

    //        return table;
    //    }
    //}
}
