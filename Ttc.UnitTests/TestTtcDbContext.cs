using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ttc.DataAccess;
using Ttc.UnitTests.Players;
using Ttc.Model;
using System.Data.Entity;

namespace Ttc.UnitTests
{
    public class TestTtcDbContext: TtcDbContext
    {
        public TestTtcDbContext()
        {
            this.Spelers = new TestSpelerDbSet();
        }

        public DbSet<Speler> Spelers { get; set; }

        //public int SaveChanges()
        //{
        //    return 0;
        //}

        //public void MarkAsModified(Speler item) { }
        //public void Dispose() { }
    }
}
