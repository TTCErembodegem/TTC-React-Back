using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Ttc.DataAccess.Entities;
using Ttc.Model;

namespace Ttc.DataAccess.Services
{
    public class DivisionService
    {
        public IEnumerable<Division> GetForCurrentYear()
        {
            using (var dbContext = new TtcDbContext())
            {
                var activeClubs = dbContext.Reeksen
                    .Where(x => x.Jaar == Constants.CurrentSeason)
                    .ToList();

                var result = Mapper.Map<IList<Reeks>, IList<Division>>(activeClubs);
                return result;
            }
        }
    }
}
