using System.Collections.Generic;
using Frenoy.Api;
using Ttc.DataAccess;
using Ttc.DataAccess.Migrations;
using Ttc.Model.Players;

namespace Frenoy.Syncer
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var context = new TtcDbContext())
            {
                Configuration.Seed(context, true, true);
                context.SaveChanges();
            }
        }
    }
}