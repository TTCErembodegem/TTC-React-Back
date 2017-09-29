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
using Frenoy.Api;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Ttc.DataAccess.Utilities;

namespace Ttc.DataAccess.Services
{
    public class PlayerService : BaseService
    {
        private static IList<Player> _players;

        #region Player
        public ICollection<Player> GetOwnClub()
        {
            if (MatchService.MatchesPlaying && _players != null)
            {
                return _players;
            }

            using (var dbContext = new TtcDbContext())
            {
                var players = dbContext.Players.ToArray();
                var result = Mapper.Map<IList<PlayerEntity>, IList<Player>>(players);

                if (MatchService.MatchesPlaying)
                {
                    _players = result;
                }

                return result;
            }
        }

        public Player GetPlayer(int playerId, bool allowCache = false)
        {
            if (allowCache && _players != null)
            {
                var ply = _players.SingleOrDefault(x => x.Id == playerId);
                return ply ?? GetPlayer(playerId);
            }

            using (var dbContext = new TtcDbContext())
            {
                var newPlayer = Mapper.Map<PlayerEntity, Player>(dbContext.Players.SingleOrDefault(x => x.Id == playerId));

                if (_players != null)
                {
                    int cacheIndex = _players.IndexOf(_players.Single(x => x.Id == newPlayer.Id));
                    _players[cacheIndex] = newPlayer;
                }

                return newPlayer;
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
                var exceller = new PlayersExcelCreator(activePlayers.ToArray());
                return exceller.Create();
            }
        }
        #endregion

        #region User
        public User GetUser(int playerId)
        {
            using (var dbContext = new TtcDbContext())
            {
                return GetUser(dbContext, playerId);
            }
        }

        private static User GetUser(TtcDbContext dbContext, int playerId)
        {
            var teams = dbContext.Teams
                .Include(x => x.Players)
                .Where(x => x.Year == Constants.CurrentSeason)
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

                // No PlayerToegang.Captain: This happens automatically when assigned Captain to a Team

                case PlayerToegang.Player:
                default:
                    return new string[] { };
            }
        }
        #endregion

        public void FrenoySync()
        {
            using (var context = new TtcDbContext())
            {
                var vttlPlayers = new FrenoyPlayersApi(context, Competition.Vttl);
                vttlPlayers.StopAllPlayers(false);
                vttlPlayers.SyncPlayers();
                var sportaPlayers = new FrenoyPlayersApi(context, Competition.Sporta);
                sportaPlayers.SyncPlayers();
            }
        }

        #region Login & Password
        private const int SystemPlayerIdFromFrontend = -1;
        public User Login(UserCredentials user)
        {
            using (var dbContext = new TtcDbContext())
            {
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
                PlayerEntity player;
                if (request.PlayerId == SystemPlayerIdFromFrontend)
                {
                    player = dbContext.Players.Single(ply => ply.NaamKort == "SYSTEM");
                }
                else
                {
                    player = dbContext.Players.SingleOrDefault(x => x.Id == request.PlayerId);
                }
                
                if (player != null)
                {
                    dbContext.Database.ExecuteSqlCommand(
                        $"UPDATE {PlayerEntity.TableName} SET paswoord=MD5({{1}}) WHERE id={{0}}",
                        player.Id,
                        request.NewPassword);

                    return player.Email;
                }
                return null;
            }
        }

        public Guid EmailMatchesPlayer(string email, int playerId)
        {
            using (var dbContext = new TtcDbContext())
            {
                var player = dbContext.Players.SingleOrDefault(x => x.Id == playerId && x.Email.ToLower() == email.ToLower());
                if (player == null)
                {
                    throw new Exception("Email komt niet overeen voor " + playerId);
                }

                var passwordReset = new PlayerPasswordResetEntity(playerId);
                dbContext.PlayerPasswordResets.Add(passwordReset);
                dbContext.SaveChanges();

                return passwordReset.Guid;
            }
        }

        public void SetNewPasswordFromGuid(Guid guid, int playerId, string password)
        {
            using (var dbContext = new TtcDbContext())
            {
                var now = DateTime.UtcNow;
                var resetInfo = dbContext.PlayerPasswordResets.FirstOrDefault(x => x.Guid == guid && x.PlayerId == playerId && x.ExpiresOn > now);
                if (resetInfo != null)
                {
                    dbContext.Database.ExecuteSqlCommand(
                        $"UPDATE {PlayerEntity.TableName} SET paswoord=MD5({{1}}) WHERE id={{0}}",
                        playerId,
                        password);
                }
                else
                {
                    throw new Exception($"Geen reset link gevonden {guid} voor speler {playerId}");
                }
            }
        }
        #endregion
    }
}