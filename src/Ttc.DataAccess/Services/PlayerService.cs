using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AutoMapper;
using Ttc.DataAccess.Entities;
using Ttc.Model;
using Ttc.Model.Players;

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
                return result;
            }
        }
    }
}