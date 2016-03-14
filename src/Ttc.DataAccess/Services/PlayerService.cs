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
        public ICollection<Player> GetActiveOwnClub()
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

                return GetUser(user.PlayerId);
            }
        }

        public User ChangePassword(UserCredentials userNewCredentials)
        {
            using (var dbContext = new TtcDbContext())
            {
                dbContext.Database.ExecuteSqlCommand(
                $"UPDATE {PlayerEntity.TableName} SET paswoord=MD5({{1}}) WHERE id={{0}}",
                userNewCredentials.PlayerId,
                userNewCredentials.Password);

                return GetUser(userNewCredentials.PlayerId);
            }
        }

        public User GetUser(int playerId)
        {
            using (var dbContext = new TtcDbContext())
            {
                return GetUser(dbContext, playerId);
            }
        }

        private User GetUser(TtcDbContext dbContext, int playerId)
        {
            int currentYear = dbContext.CurrentYear;
            var teams = dbContext.Teams
                .Include(x => x.Players)
                .Where(x => x.Year == currentYear)
                .Where(x => x.Players.Any(ply => ply.PlayerId == playerId))
                .Select(x => x.Id);

            var player = dbContext.Players.Single(ply => ply.Id == playerId);
            return new User
            {
                PlayerId = playerId,
                Alias = player.NaamKort,
                Security = GetPlayerSecurity(player.Toegang),
                Teams = teams.ToList()
            };
        }

        private ICollection<string> GetPlayerSecurity(PlayerToegang toegang)
        {
            switch (toegang)
            {
                case PlayerToegang.Dev:
                    return new[] { "CAN_MANAGETEAM", "CAN_EDITALLREPORTS", "IS_ADMIN", "IS_DEV" };

                case PlayerToegang.Board:
                    return new[] { "CAN_MANAGETEAM", "CAN_EDITALLREPORTS", "IS_ADMIN" };

                case PlayerToegang.Player:
                default:
                    return new string[] { };
            }
        }
    }
}