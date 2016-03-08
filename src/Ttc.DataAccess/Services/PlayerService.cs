using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Data.Entity;
using AutoMapper;
using Ttc.DataEntities;
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
                // TODO: if the user is not logged in, do not return sensitive data like email, phone, address

                var activeOwnClubPlayers = dbContext.Players
                    .ToArray()
                    .Where(x => !x.IsGestopt && x.IsFromOwnClub())
                    .ToList();

                var result = Mapper.Map<IList<PlayerEntity>, IList<Player>>(activeOwnClubPlayers);
                return result;
            }
        }

        public Player GetPlayer(int playerId)
        {
            using (var dbContext = new TtcDbContext())
            {
                return Mapper.Map<PlayerEntity, Player>(dbContext.Players.SingleOrDefault(x => x.Id == playerId));
            }
        }

        public User Login(UserCredentials user)
        {
            using (var dbContext = new TtcDbContext())
            {
                var pwdCheck = dbContext.Database.SqlQuery<int>(
                    $"SELECT COUNT(0) FROM {PlayerEntity.TableName} WHERE id={{0}} AND paswoord=MD5({{1}})",
                    user.PlayerId, 
                    user.Password).FirstOrDefault();

                if (pwdCheck != 1)
                {
                    return null;
                }

                int currentYear = dbContext.CurrentYear;
                var teams = dbContext.Teams
                    .Include(x => x.Players)
                    .Where(x => x.Year == currentYear)
                    .Where(x => x.Players.Any(ply => ply.PlayerId == user.PlayerId))
                    .Select(x => x.Id);

                // TODO: CAN_MANAGETEAM hardcoded. Link security to speler.Toegang
                return new User
                {
                    PlayerId = user.PlayerId,
                    Security = new[] { "CAN_MANAGETEAM" },
                    Teams = teams.ToList()
                };
            }
        }
    }
}