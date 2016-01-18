using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ttc.Model;

namespace Ttc.UnitTests.Players
{
    public class TestSpelerDbSet: TestDbSet<Speler>
    {
        public override Speler Find(params object[] keyValues)
        {
            return this.SingleOrDefault(speler => speler.ID == (int)keyValues.Single());
        }
    }
}
