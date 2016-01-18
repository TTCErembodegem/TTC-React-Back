using System.Collections;
using System.Collections.Generic;
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
                var activeOwnClubPlayers = dbContext.Players
                    .ToArray()
                    .Where(x => !x.IsGestopt && x.IsFromOwnClub());

                return Mapper.Map<IEnumerable<Speler>, IEnumerable<Player>>(activeOwnClubPlayers);
            }
        }
    }
}