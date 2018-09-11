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
        public async Task<ICollection<Player>> GetOwnClub()
        {
            if (MatchService.MatchesPlaying && _players != null)
            {
                return _players;
            }

            using (var dbContext = new TtcDbContext())
            {
                var players = await dbContext.Players.ToArrayAsync();
                var result = Mapper.Map<IList<PlayerEntity>, IList<Player>>(players);

                if (MatchService.MatchesPlaying)
                {
                    _players = result;
                }

                return result;
            }
        }

        public async Task<Player> GetPlayer(int playerId, bool allowCache = false)
        {
            if (allowCache && _players != null)
            {
                var ply = _players.SingleOrDefault(x => x.Id == playerId);
                return ply ?? await GetPlayer(playerId);
            }

            using (var dbContext = new TtcDbContext())
            {
                var newPlayer = Mapper.Map<PlayerEntity, Player>(await dbContext.Players.SingleOrDefaultAsync(x => x.Id == playerId));

                if (_players != null)
                {
                    int cacheIndex = _players.IndexOf(_players.Single(x => x.Id == newPlayer.Id));
                    _players[cacheIndex] = newPlayer;
                }

                return newPlayer;
            }
        }

        public async Task<Player> UpdateStyle(PlayerStyle playerStyle)
        {
            using (var dbContext = new TtcDbContext())
            {
                var existingSpeler = await dbContext.Players.FirstOrDefaultAsync(x => x.Id == playerStyle.PlayerId);
                if (existingSpeler == null)
                {
                    return null;
                }

                existingSpeler.Stijl = playerStyle.Name;
                existingSpeler.BesteSlag = playerStyle.BestStroke;
                await dbContext.SaveChangesAsync();
            }
            var newMatch = await GetPlayer(playerStyle.PlayerId);
            return newMatch;
        }

        public async Task<Player> UpdatePlayer(Player player)
        {
            using (var dbContext = new TtcDbContext())
            {
                var existingSpeler = await dbContext.Players.FirstOrDefaultAsync(x => x.Id == player.Id);
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
                
                await dbContext.SaveChangesAsync();
                player.Id = existingSpeler.Id;
            }
            var newPlayer = await GetPlayer(player.Id);
            return newPlayer;
        }

        public async Task DeletePlayer(int playerId)
        {
            using (var dbContext = new TtcDbContext())
            {
                var player = await dbContext.Players.FindAsync(playerId);
                if (player == null) return;
                dbContext.Players.Remove(player);
                await dbContext.SaveChangesAsync();
            }
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
            existingSpeler.HasKey = player.HasKey;

            existingSpeler.FirstName = player.FirstName;
            existingSpeler.LastName = player.LastName;
            existingSpeler.NaamKort = player.Alias;
        }

        public async Task<byte[]> GetExcelExport()
        {
            using (var dbContext = new TtcDbContext())
            {
                var activePlayers = await dbContext.Players.Where(x => x.Gestopt == null).ToArrayAsync();
                var exceller = new PlayersExcelCreator(activePlayers);
                return exceller.Create();
            }
        }
        #endregion

        #region User
        public async Task<User> GetUser(int playerId)
        {
            using (var dbContext = new TtcDbContext())
            {
                return await GetUser(dbContext, playerId);
            }
        }

        private static async Task<User> GetUser(TtcDbContext dbContext, int playerId)
        {
            var teams = await dbContext.Teams
                .Include(x => x.Players)
                .Where(x => x.Year == Constants.CurrentSeason)
                .Where(x => x.Players.Any(ply => ply.PlayerId == playerId))
                .Select(x => x.Id)
                .ToListAsync();

            var player = await dbContext.Players.SingleAsync(ply => ply.Id == playerId);
            return new User
            {
                PlayerId = playerId,
                Alias = player.NaamKort,
                Security = GetPlayerSecurity(player.Toegang),
                Teams = teams
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

        public async Task FrenoySync()
        {
            using (var context = new TtcDbContext())
            {
                var vttlPlayers = new FrenoyPlayersApi(context, Competition.Vttl);
                await vttlPlayers.StopAllPlayers(false);
                await vttlPlayers.SyncPlayers();
                var sportaPlayers = new FrenoyPlayersApi(context, Competition.Sporta);
                await sportaPlayers.SyncPlayers();
            }
        }

        #region Login & Password
        private const int SystemPlayerIdFromFrontend = -1;
        public async Task<User> Login(UserCredentials user)
        {
            using (var dbContext = new TtcDbContext())
            {
                if (user.PlayerId == SystemPlayerIdFromFrontend)
                {
                    user.PlayerId = (await dbContext.Players.SingleAsync(ply => ply.NaamKort == "SYSTEM")).Id;
                }

                var pwdCheck = await dbContext.Database.SqlQuery<int>(
                    $"SELECT COUNT(0) FROM {PlayerEntity.TableName} WHERE id={{0}} AND paswoord=MD5({{1}})",
                    user.PlayerId,
                    user.Password).FirstOrDefaultAsync();

                if (pwdCheck != 1)
                {
                    return null;
                }

                return await GetUser(user.PlayerId);
            }
        }

        public async Task<User> ChangePassword(PasswordCredentials userNewCredentials)
        {
            using (var dbContext = new TtcDbContext())
            {
                var pwdCheck = await dbContext.Database.SqlQuery<int>(
                    $"SELECT COUNT(0) FROM {PlayerEntity.TableName} WHERE id={{0}} AND paswoord=MD5({{1}})",
                    userNewCredentials.PlayerId,
                    userNewCredentials.OldPassword).FirstOrDefaultAsync();

                if (pwdCheck != 1)
                {
                    return null;
                }
                else
                {
                    await dbContext.Database.ExecuteSqlCommandAsync(
                        $"UPDATE {PlayerEntity.TableName} SET paswoord=MD5({{1}}) WHERE id={{0}}",
                        userNewCredentials.PlayerId,
                        userNewCredentials.NewPassword);

                    return await GetUser(userNewCredentials.PlayerId);
                }
            }
        }

        public async Task<string> SetNewPassword(PasswordCredentials request)
        {
            using (var dbContext = new TtcDbContext())
            {
                PlayerEntity player;
                if (request.PlayerId == SystemPlayerIdFromFrontend)
                {
                    player = await dbContext.Players.SingleAsync(ply => ply.NaamKort == "SYSTEM");
                }
                else
                {
                    player = await dbContext.Players.SingleOrDefaultAsync(x => x.Id == request.PlayerId);
                }
                
                if (player != null)
                {
                    await dbContext.Database.ExecuteSqlCommandAsync(
                        $"UPDATE {PlayerEntity.TableName} SET paswoord=MD5({{1}}) WHERE id={{0}}",
                        player.Id,
                        request.NewPassword);

                    return player.Email;
                }
                return null;
            }
        }

        public async Task<Guid> EmailMatchesPlayer(string email, int playerId)
        {
            using (var dbContext = new TtcDbContext())
            {
                var player = await dbContext.Players.SingleOrDefaultAsync(x => x.Id == playerId && x.Email.ToLower() == email.ToLower());
                if (player == null)
                {
                    throw new Exception("Email komt niet overeen voor " + playerId);
                }

                var passwordReset = new PlayerPasswordResetEntity(playerId);
                dbContext.PlayerPasswordResets.Add(passwordReset);
                await dbContext.SaveChangesAsync();

                return passwordReset.Guid;
            }
        }

        public async Task SetNewPasswordFromGuid(Guid guid, int playerId, string password)
        {
            using (var dbContext = new TtcDbContext())
            {
                var now = DateTime.UtcNow;
                var resetInfo = await dbContext.PlayerPasswordResets.FirstOrDefaultAsync(x => x.Guid == guid && x.PlayerId == playerId && x.ExpiresOn > now);
                if (resetInfo != null)
                {
                    await dbContext.Database.ExecuteSqlCommandAsync(
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