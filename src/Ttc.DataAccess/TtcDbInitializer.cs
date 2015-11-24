using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using Ttc.Model;

namespace Ttc.DataAccess
{
    /// <summary>
    /// Initial Database seeding
    /// </summary>
    public class TtcDbInitializer : CreateDatabaseIfNotExists<TtcDbContext>
    {
        //protected override void Seed(TtcDbContext context)
        //{
        //    context.Spelers.Add(new Speler());
        //    base.Seed(context);
        //}
    }
}
