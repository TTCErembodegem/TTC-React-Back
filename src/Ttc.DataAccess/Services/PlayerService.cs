using Ttc.DataEntities;
using Ttc.Model;
using Ttc.Model.Players;
using AutoMapper;
using Frenoy.Api;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using Ttc.DataAccess.Utilities;
using Ttc.DataEntities.Core;

namespace Ttc.DataAccess.Services;

public class PlayerService
{
    private readonly ITtcDbContext _context;
    private readonly IMapper _mapper;

    public PlayerService(ITtcDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    #region Player
    public async Task<ICollection<Player>> GetOwnClub()
    {
        var players = await _context.Players.ToArrayAsync();
        var result = _mapper.Map<IList<PlayerEntity>, IList<Player>>(players);
        return result;
    }

    public async Task<Player> GetPlayer(int playerId)
    {
        var player = await _context.Players.SingleAsync(x => x.Id == playerId);
        var newPlayer = _mapper.Map<PlayerEntity, Player>(player);
        return newPlayer;
    }

    public async Task<Player> UpdateStyle(PlayerStyle playerStyle)
    {
        var existingSpeler = await _context.Players.FirstOrDefaultAsync(x => x.Id == playerStyle.PlayerId);
        if (existingSpeler == null)
        {
            return null;
        }

        existingSpeler.Stijl = playerStyle.Name;
        existingSpeler.BesteSlag = playerStyle.BestStroke;
        await _context.SaveChangesAsync();
        var newMatch = await GetPlayer(playerStyle.PlayerId);
        return newMatch;
    }

    public async Task<Player> UpdatePlayer(Player player)
    {
        var existingSpeler = await _context.Players.FirstOrDefaultAsync(x => x.Id == player.Id);
        if (existingSpeler == null)
        {
            existingSpeler = new PlayerEntity();
            MapPlayer(player, existingSpeler);
            _context.Players.Add(existingSpeler);
        }
        else
        {
            MapPlayer(player, existingSpeler);
        }

        await _context.SaveChangesAsync();
        player.Id = existingSpeler.Id;
        var newPlayer = await GetPlayer(player.Id);
        return newPlayer;
    }

    public async Task DeletePlayer(int playerId)
    {
        var player = await _context.Players.FindAsync(playerId);
        if (player == null) return;
        _context.Players.Remove(player);
        await _context.SaveChangesAsync();
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
        existingSpeler.Toegang = (PlayerToegang)Enum.Parse(typeof(PlayerToegang), player.Security);
        existingSpeler.HasKey = player.HasKey;

        existingSpeler.FirstName = player.FirstName;
        existingSpeler.LastName = player.LastName;
        existingSpeler.NaamKort = player.Alias;

        existingSpeler.NextKlassementVttl = player.Vttl?.NextRanking;
        existingSpeler.NextKlassementSporta = player.Sporta?.NextRanking;
    }

    public async Task<byte[]> GetExcelExport()
    {
        var activePlayers = await _context.Players.Where(x => x.Gestopt == null).ToArrayAsync();
        var exceller = new PlayersExcelCreator(activePlayers);
        return exceller.Create();
    }
    #endregion

    #region User
    public async Task<User> GetUser(int playerId)
    {
        int currentSeason = _context.CurrentSeason;
        var teams = await _context.Teams
            .Include(x => x.Players)
            .Where(x => x.Year == currentSeason)
            .Where(x => x.Players.Any(ply => ply.PlayerId == playerId))
            .Select(x => x.Id)
            .ToListAsync();

        var player = await _context.Players.SingleAsync(ply => ply.Id == playerId);
        return new User
        {
            PlayerId = playerId,
            Alias = player.NaamKort ?? "",
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
                return [];
        }
    }
    #endregion

    public async Task FrenoySync()
    {
        var vttlPlayers = new FrenoyPlayersApi(_context, Competition.Vttl);
        await vttlPlayers.StopAllPlayers(false);
        await vttlPlayers.SyncPlayers();
        var sportaPlayers = new FrenoyPlayersApi(_context, Competition.Sporta);
        await sportaPlayers.SyncPlayers();
    }

    #region Login & Password
    private const int SystemPlayerIdFromFrontend = -1;
    public async Task<User?> Login(UserCredentials user)
    {
        if (user.PlayerId == SystemPlayerIdFromFrontend)
        {
            user.PlayerId = (await _context.Players.SingleAsync(ply => ply.NaamKort == "SYSTEM")).Id;
        }

        var playerEntity = await _context.Players.FromSqlRaw(
            $"SELECT * FROM {PlayerEntity.TableName} WHERE id={{0}} AND paswoord=MD5({{1}})",
            new MySqlParameter("@p1", user.PlayerId),
            new MySqlParameter("@p2", user.Password)
        ).FirstOrDefaultAsync();

        if (playerEntity == null)
        {
            return null;
        }

        return await GetUser(user.PlayerId);
    }

    public async Task<User?> ChangePassword(PasswordCredentials userNewCredentials)
    {
        var player = await _context.Players.FromSqlRaw(
            $"SELECT * FROM {PlayerEntity.TableName} WHERE id={{0}} AND paswoord=MD5({{1}})",
            userNewCredentials.PlayerId,
            userNewCredentials.OldPassword).FirstOrDefaultAsync();

        if (player == null)
        {
            return null;
        }

        await _context.Database.ExecuteSqlRawAsync(
            $"UPDATE {PlayerEntity.TableName} SET paswoord=MD5({{1}}) WHERE id={{0}}",
            userNewCredentials.PlayerId,
            userNewCredentials.NewPassword);

        return await GetUser(userNewCredentials.PlayerId);
    }

    public async Task<string?> SetNewPassword(PasswordCredentials request)
    {
        PlayerEntity? player;
        if (request.PlayerId == SystemPlayerIdFromFrontend)
        {
            player = await _context.Players.SingleAsync(ply => ply.NaamKort == "SYSTEM");
        }
        else
        {
            player = await _context.Players.SingleOrDefaultAsync(x => x.Id == request.PlayerId);
        }

        if (player != null)
        {
            await _context.Database.ExecuteSqlRawAsync(
                $"UPDATE {PlayerEntity.TableName} SET paswoord=MD5({{1}}) WHERE id={{0}}",
                player.Id,
                request.NewPassword);

            return player.Email;
        }
        return null;
    }

    public async Task<Guid> EmailMatchesPlayer(string email, int playerId)
    {
        var player = await _context.Players.SingleOrDefaultAsync(x => x.Id == playerId && x.Email.ToLower() == email.ToLower());
        if (player == null)
        {
            throw new Exception("Email komt niet overeen voor " + playerId);
        }

        var passwordReset = new PlayerPasswordResetEntity(playerId);
        _context.PlayerPasswordResets.Add(passwordReset);
        await _context.SaveChangesAsync();

        return passwordReset.Guid;
    }

    public async Task SetNewPasswordFromGuid(Guid guid, int playerId, string password)
    {
        var now = DateTime.UtcNow;
        var resetInfo = await _context.PlayerPasswordResets
            .Where(x => x.Guid == guid)
            .Where(x => x.PlayerId == playerId)
            .FirstOrDefaultAsync(x => x.ExpiresOn > now);

        if (resetInfo != null)
        {
            await _context.Database.ExecuteSqlRawAsync(
                $"UPDATE {PlayerEntity.TableName} SET paswoord=MD5({{1}}) WHERE id={{0}}",
                playerId,
                password);
        }
        else
        {
            throw new Exception($"Geen reset link gevonden {guid} voor speler {playerId}");
        }
    }
    #endregion
}
