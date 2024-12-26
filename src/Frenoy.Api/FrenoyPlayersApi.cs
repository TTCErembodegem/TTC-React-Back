using System.Globalization;
using FrenoyVttl;
using Microsoft.EntityFrameworkCore;
using Ttc.DataEntities;
using Ttc.DataEntities.Core;
using Ttc.Model.Players;

namespace Frenoy.Api;

public class FrenoyPlayersApi : FrenoyApiBase
{
    public FrenoyPlayersApi(ITtcDbContext ttcDbContext, Competition comp) : base(ttcDbContext, comp)
    {

    }

    public async Task StopAllPlayers(bool alsoSetGestopt)
    {
        foreach (var dbPlayer in await _db.Players.Where(x => x.ClubIdVttl == Constants.OwnClubId || x.ClubIdSporta == Constants.OwnClubId).ToArrayAsync())
        {
            if (alsoSetGestopt)
            {
                dbPlayer.Gestopt = _currentSeason - 1;
            }
            dbPlayer.ClubIdSporta = null;
            dbPlayer.ClubIdVttl = null;
        }
        await _db.SaveChangesAsync();
    }

    public async Task SyncPlayers()
    {
        var frenoyPlayers = await _frenoy.GetMembersAsync(new GetMembersRequest1
        {
            GetMembersRequest = new GetMembersRequest()
            {
                Season = (_currentSeason - 2000 + 1).ToString(),
                Club = _settings.FrenoyClub,
            }
        });

        foreach (MemberEntryType frenoyPlayer in frenoyPlayers.GetMembersResponse.MemberEntries)
        {
            string frenoyFirstName = frenoyPlayer.FirstName.ToUpperInvariant();
            string frenoyLastName = frenoyPlayer.LastName.ToUpperInvariant();
            var existingPlayer = await _db.Players.SingleOrDefaultAsync(ply => ply.FirstName.ToUpper() == frenoyFirstName && ply.LastName.ToUpper() == frenoyLastName);
            if (_isVttl)
            {
                if (existingPlayer == null)
                    existingPlayer = await _db.Players.SingleOrDefaultAsync(ply => ply.ComputerNummerVttl.HasValue && ply.ComputerNummerVttl.Value.ToString() == frenoyPlayer.UniqueIndex);

                if (existingPlayer != null)
                {
                    SetVttl(existingPlayer, frenoyPlayer);
                }
                else
                {
                    await CreatePlayerEntity(frenoyPlayer);
                }
            }
            else
            {
                if (existingPlayer == null)
                    existingPlayer = await _db.Players.SingleOrDefaultAsync(ply => ply.LidNummerSporta.HasValue && ply.LidNummerSporta.Value.ToString() == frenoyPlayer.UniqueIndex);

                if (existingPlayer != null)
                {
                    SetSporta(existingPlayer, frenoyPlayer);
                }
                else
                {
                    await CreatePlayerEntity(frenoyPlayer);
                }
            }
        }

        await _db.SaveChangesAsync();
    }

    private static void SetVttl(PlayerEntity player, MemberEntryType frenoyPlayer)
    {
        player.Gestopt = null;

        player.IndexVttl = int.Parse(frenoyPlayer.RankingIndex);
        player.VolgnummerVttl = int.Parse(frenoyPlayer.Position);
        player.ClubIdVttl = Constants.OwnClubId;
        player.KlassementVttl = frenoyPlayer.Ranking;
        player.ComputerNummerVttl = int.Parse(frenoyPlayer.UniqueIndex);
    }

    private async Task<PlayerEntity> CreatePlayerEntity(MemberEntryType frenoyPlayer)
    {
        var existingPlayer = await _db.Players.SingleOrDefaultAsync(x => x.FirstName.ToUpper() == frenoyPlayer.FirstName && x.LastName.ToUpper() == frenoyPlayer.LastName);
        bool isNew = existingPlayer == null;
        if (isNew)
        {
            existingPlayer = CreatePlayerEntityCore(frenoyPlayer);
        }

        if (_isVttl)
            SetVttl(existingPlayer, frenoyPlayer);
        else
            SetSporta(existingPlayer, frenoyPlayer);

        if (isNew)
        {
            _db.Players.Add(existingPlayer);
            await _db.SaveChangesAsync();
        }

        return existingPlayer;
    }

    private static PlayerEntity CreatePlayerEntityCore(MemberEntryType frenoyPlayer)
    {
        var newPlayer = new PlayerEntity();
        newPlayer.FirstName = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(frenoyPlayer.FirstName.ToLowerInvariant());
        newPlayer.LastName = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(frenoyPlayer.LastName.ToLowerInvariant());
        newPlayer.NaamKort = newPlayer.Name;
        newPlayer.Toegang = PlayerToegang.Player;
        newPlayer.Email = frenoyPlayer.Email;
        if (frenoyPlayer.Phone != null)
        {
            newPlayer.Gsm = frenoyPlayer.Phone.Mobile;
        }

        if (frenoyPlayer.Address != null)
        {
            newPlayer.Adres = frenoyPlayer.Address.Line1;
            newPlayer.Gemeente = frenoyPlayer.Address.ZipCode + " " + frenoyPlayer.Address.Town;
        }

        return newPlayer;
    }

    private static void SetSporta(PlayerEntity player, MemberEntryType frenoyPlayer)
    {
        player.Gestopt = null;

        player.IndexSporta = int.Parse(frenoyPlayer.RankingIndex);
        player.VolgnummerSporta = int.Parse(frenoyPlayer.Position);
        player.ClubIdSporta = Constants.OwnClubId;
        player.KlassementSporta = frenoyPlayer.Ranking;
        player.LidNummerSporta = int.Parse(frenoyPlayer.UniqueIndex);
        //player.LinkKaartSporta
    }

    public async Task<ICollection<PlayerEntity>> GetPlayers(int clubId)
    {
        var club = await _db.Clubs.FindAsync(clubId);
        var frenoyPlayers = await _frenoy.GetMembersAsync(new GetMembersRequest1
        {
            GetMembersRequest = new GetMembersRequest()
            {
                Club = club.CodeSporta,
            }
        });

        var players = frenoyPlayers.GetMembersResponse.MemberEntries
            .Select(frenoyPlayer =>
            {
                var ply = CreatePlayerEntityCore(frenoyPlayer);
                if (_isVttl)
                    SetVttl(ply, frenoyPlayer);
                else
                    SetSporta(ply, frenoyPlayer);
                return ply;
            })
            .ToArray();

        return players;
    }
}
