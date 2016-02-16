using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Ttc.DataEntities;
using Ttc.Model.Players;

namespace Ttc.DataAccess.Services
{
    public class PlayerService
    {
        public IEnumerable<Player> GetActiveOwnClub()
        {
            using (var dbContext = new TtcDbContext())
            {
                // TODO: if the user is not logged in, do not return sensitive data like email, phone, address

                var activeOwnClubPlayers = dbContext.Spelers
                    .ToArray()
                    .Where(x => !x.IsGestopt && x.IsFromOwnClub())
                    .ToList();

                var result = Mapper.Map<IList<Speler>, IList<Player>>(activeOwnClubPlayers);
                return result;
            }
        }

        public Player GetPlayer(int playerId)
        {
            using (var dbContext = new TtcDbContext())
            {
                return Mapper.Map<Speler, Player>(dbContext.Spelers.SingleOrDefault(x => x.Id == playerId));
            }
        }
    }
}