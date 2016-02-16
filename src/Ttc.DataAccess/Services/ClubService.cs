using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Ttc.DataEntities;
using System.Data.Entity;
using Ttc.Model.Clubs;

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