using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Data.Entity;
using System.Drawing;
using AutoMapper;
using Ttc.DataEntities;
using Ttc.Model;
using Ttc.Model.Players;
using System.IO;
using System.Net.Mail;
using System.Threading.Tasks;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Ttc.DataAccess.Utilities;

namespace Ttc.DataAccess.Services
{
    public class PlayerService : BaseService
    {
        #region Player
        public ICollection<Player> GetOwnClub()
        {
            using (var dbContext = new TtcDbContext())
            {
                var players = dbContext.Players.ToArray();
                var result = Mapper.Map<IList<PlayerEntity>, IList<Player>>(players);
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

        public Player UpdatePlayer(Player player)
        {
            using (var dbContext = new TtcDbContext())
            {
                var existingSpeler = dbContext.Players.FirstOrDefault(x => x.Id == player.Id);
                if (existingSpeler == null)
                {
                    existingSpeler = new PlayerEntity();
                    MapPlayer(player, existingSpeler);
                    dbContext.Players.Add(existingSpeler);
                }
                else
                {
                    MapPlayer(player, existingSpeler);
                }
                
                dbContext.SaveChanges();
                player.Id = existingSpeler.Id;
            }
            var newPlayer = GetPlayer(player.Id);
            return newPlayer;
        }

        private static void MapPlayer(Player player, PlayerEntity existingSpeler)
        {
            existingSpeler.Gsm = player.Contact.Mobile;
            existingSpeler.Email = player.Contact.Email;
            existingSpeler.Adres = player.Contact.Address;
            existingSpeler.Gemeente = player.Contact.City;

            existingSpeler.Stijl = player.Style.Name;
            existingSpeler.BesteSlag = player.Style.BestStroke;

            existingSpeler.Gestopt = player.QuitYear;
            existingSpeler.Toegang = (PlayerToegang) Enum.Parse(typeof (PlayerToegang), player.Security);

            existingSpeler.Naam = player.Name;
            existingSpeler.NaamKort = player.Alias;
        }

        public byte[] GetExcelExport()
        {
            using (var dbContext = new TtcDbContext())
            {
                var activePlayers = dbContext.Players.Where(x => x.Gestopt == null);
                var exceller = new PlayerExcelCreator(activePlayers.ToArray());
                return exceller.Create();
            }
        }
        #endregion

        #region User
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

        public string SetNewPassword(PasswordCredentials request)
        {
            using (var dbContext = new TtcDbContext())
            {
                var player = dbContext.Players.SingleOrDefault(x => x.Id == request.PlayerId);
                if (player != null)
                {
                    dbContext.Database.ExecuteSqlCommand(
                        $"UPDATE {PlayerEntity.TableName} SET paswoord=MD5({{1}}) WHERE id={{0}}",
                        request.PlayerId,
                        request.NewPassword);

                    return player.Email;
                }
                return null;
            }
        }

        public string RequestNewPassword(NewPasswordRequest request)
        {
            if (request.PlayerId == 0)
            {
                return null;
            }

            using (var dbContext = new TtcDbContext())
            {
                var player = dbContext.Players.SingleOrDefault(x => x.Id == request.PlayerId && x.Email == request.Email);
                if (player != null)
                {
                    string newPassword = GenerateNewPassword();
                    dbContext.Database.ExecuteSqlCommand(
                        $"UPDATE {PlayerEntity.TableName} SET paswoord=MD5({{1}}) WHERE id={{0}}",
                        request.PlayerId,
                        newPassword);

                    return newPassword;
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
            int currentYear = TtcDbContext.CurrentYear;
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

        private static string GenerateNewPassword()
        {
            string path = Path.GetRandomFileName();
            path = path.Replace(".", "");
            return path;
        }
        #endregion
    }
}