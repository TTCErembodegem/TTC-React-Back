using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AutoMapper;
using Ttc.DataAccess.Entities;
using Ttc.Model;

namespace Ttc.DataAccess.Services
{
    public class PlayerService
    {
        public IEnumerable<Player> GetActiveOwnClub()
        {
            using (var dbContext = new TtcDbContext())
            {
                var activeOwnClubPlayers = dbContext.Spelers
                    .ToArray()
                    .Where(x => !x.IsGestopt && x.IsFromOwnClub())
                    .ToList();

                var result = Mapper.Map<IList<Speler>, IList<Player>>(activeOwnClubPlayers);

                var first = result.First();
                Debug.Assert(first.Vttl != null);

                return result;
            }
        }
    }
}