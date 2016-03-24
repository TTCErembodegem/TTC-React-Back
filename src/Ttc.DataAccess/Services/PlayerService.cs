using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Data.Entity;
using AutoMapper;
using Ttc.DataEntities;
using Ttc.Model;
using Ttc.Model.Players;
using System.IO;

namespace Ttc.DataAccess.Services
{
    public class PlayerService
    {
        public ICollection<Player> GetActiveOwnClub()
        {
            using (var dbContext = new TtcDbContext())
            {
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
                const int SystemPlayerIdFromFrontend = -1;
                if (user.PlayerId == SystemPlayerIdFromFrontend)
                {
                    user.PlayerId = dbContext.Players.Single(ply => ply.NaamKort == "SYSTEM").Id;
                }

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

        public User ChangePassword(PasswordCredentials userNewCredentials)
        {
            using (var dbContext = new TtcDbContext())
            {
                var pwdCheck = dbContext.Database.SqlQuery<int>(
                    $"SELECT COUNT(0) FROM {PlayerEntity.TableName} WHERE id={{0}} AND paswoord=MD5({{1}})",
                    userNewCredentials.PlayerId,
                    userNewCredentials.OldPassword).FirstOrDefault();

                if (pwdCheck != 1)
                {
                    return null;
                }
                else
                {
                    dbContext.Database.ExecuteSqlCommand(
                    $"UPDATE {PlayerEntity.TableName} SET paswoord=MD5({{1}}) WHERE id={{0}}",
                    userNewCredentials.PlayerId,
                    userNewCredentials.NewPassword);

                    return GetUser(userNewCredentials.PlayerId);
                }
            }
        }

        public User NewPassword(PasswordCredentials userNewCredentials)
        {
            if (userNewCredentials.PlayerId == 0)
            {
                return null;
            }

            using (var dbContext = new TtcDbContext())
            {
                var emailForPlayer = dbContext.Players.Single(x => x.Id == userNewCredentials.PlayerId).Email;
                if (!string.IsNullOrEmpty(emailForPlayer))
                {
                    string newPassword = GenerateNewPassword();
                    dbContext.Database.ExecuteSqlCommand(
                        $"UPDATE {PlayerEntity.TableName} SET paswoord=MD5({{1}}) WHERE id={{0}}",
                        userNewCredentials.PlayerId,
                        newPassword);

                    return GetUser(userNewCredentials.PlayerId);
                }
            }
            return null;
        }

        public User GetUser(int playerId)
        {
            using (var dbContext = new TtcDbContext())
            {
                return GetUser(dbContext, playerId);
            }
        }

        private static User GetUser(TtcDbContext dbContext, int playerId)
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

        private static ICollection<string> GetPlayerSecurity(PlayerToegang toegang)
        {
            switch (toegang)
            {
                case PlayerToegang.System:
                    return new[] { "CAN_MANAGETEAM", "CAN_EDITALLREPORTS", "IS_ADMIN", "IS_SYSTEM" };

                case PlayerToegang.Dev:
                    return new[] { "CAN_MANAGETEAM", "CAN_EDITALLREPORTS", "IS_ADMIN", "IS_DEV" };

                case PlayerToegang.Board:
                    return new[] { "CAN_MANAGETEAM", "CAN_EDITALLREPORTS", "IS_ADMIN" };

                case PlayerToegang.Player:
                default:
                    return new string[] { };
            }
        }

        #region New Password Helpers

        private static string GenerateNewPassword()
        {
            string path = Path.GetRandomFileName();
            path = path.Replace(".", "");
            return path;
        }
        #endregion

        public Player UpdateStyle(PlayerStyle playerStyle)
        {
            using (var dbContext = new TtcDbContext())
            {
                var existingSpeler = dbContext.Players.FirstOrDefault(x => x.Id == playerStyle.PlayerId);
                if (existingSpeler == null)
                {
                    return null;
                }

                existingSpeler.Stijl = playerStyle.Name;
                existingSpeler.BesteSlag = playerStyle.BestStroke;
                dbContext.SaveChanges();
            }
            var newMatch = GetPlayer(playerStyle.PlayerId);
            return newMatch;
        }
    }
}