using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AutoMapper;
using Ttc.DataAccess.Entities;
using Ttc.Model;
using System.Data.Entity;

namespace Ttc.DataAccess.Services
{
    public class ClubService
    {
        public IEnumerable<Club> GetActiveClubs()
        {
            using (var dbContext = new TtcDbContext())
            {
                var activeClubs = dbContext.Clubs
                    .Include(x => x.Lokalen)
                    .Include(x => x.Contacten)
                    .Where(x => x.Actief.HasValue && x.Actief == 1)
                    .ToList();

                var result = Mapper.Map<IList<ClubEntity>, IList<Club>>(activeClubs);
                return result;
            }
        }
    }
}