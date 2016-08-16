using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Frenoy.Api.FrenoyVttl;
using Ttc.DataEntities;
using Ttc.DataEntities.Core;
using Ttc.Model.Players;
using Ttc.Model.Teams;

namespace Frenoy.Api
{
    public class FrenoyPlayersApi : FrenoyApiBase
    {
        public FrenoyPlayersApi(ITtcDbContext ttcDbContext, Competition comp) : base(ttcDbContext, comp)
        {

        }

        public void StopAllPlayers(bool alsoSetGestopt)
        {
            foreach (var dbPlayer in _db.Players.Where(x => x.ClubIdVttl == Constants.OwnClubId || x.ClubIdSporta == Constants.OwnClubId))
            {
                if (alsoSetGestopt)
                {
                    dbPlayer.Gestopt = Constants.CurrentSeason - 1;
                }
                dbPlayer.ClubIdSporta = null;
                dbPlayer.ClubIdVttl = null;
            }
            _db.SaveChanges();
        }

        public void SyncPlayers()
        {
            var frenoyPlayers = _frenoy.GetMembers(new GetMembersRequest
            {
                Club = _settings.FrenoyClub,
                Credentials = new CredentialsType()
                {
                    Account = "",
                    Password = ""
                }
            });

            foreach (MemberEntryType frenoyPlayer in frenoyPlayers.MemberEntries)
            {
                if (_isVttl)
                {
                    var existingPlayer = _db.Players.SingleOrDefault(ply => ply.ComputerNummerVttl.HasValue && ply.ComputerNummerVttl.Value.ToString() == frenoyPlayer.UniqueIndex);
                    if (existingPlayer != null)
                    {
                        SetVttl(existingPlayer, frenoyPlayer);
                    }
                    else
                    {
                        var newPlayer = CreatePlayerEntity(frenoyPlayer);
                        if (newPlayer != null)
                        {
                            SetVttl(newPlayer, frenoyPlayer);
                            _db.Players.Add(newPlayer);
                            _db.SaveChanges();
                        }
                    }
                }
                else
                {
                    var existingPlayer = _db.Players.SingleOrDefault(ply => ply.LidNummerSporta.HasValue && ply.LidNummerSporta.Value.ToString() == frenoyPlayer.UniqueIndex);
                    if (existingPlayer != null)
                    {
                        SetSporta(existingPlayer, frenoyPlayer);
                    }
                    else
                    {
                        var newPlayer = CreatePlayerEntity(frenoyPlayer);
                        if (newPlayer != null)
                        {
                            SetSporta(newPlayer, frenoyPlayer);
                            _db.Players.Add(newPlayer);
                            _db.SaveChanges();
                        }
                    }
                }
            }

            _db.SaveChanges();
        }

        private static void SetVttl(PlayerEntity player, MemberEntryType frenoyPlayer)
        {
            player.Gestopt = null;

            player.IndexVttl = int.Parse(frenoyPlayer.RankingIndex);
            player.VolgnummerVttl = int.Parse(frenoyPlayer.Position);
            player.ClubIdVttl = Constants.OwnClubId;
            player.KlassementVttl = frenoyPlayer.Ranking;
            player.ComputerNummerVttl = int.Parse(frenoyPlayer.UniqueIndex);
            //player.LinkKaartVttl
        }

        private PlayerEntity CreatePlayerEntity(MemberEntryType frenoyPlayer)
        {
            string name = frenoyPlayer.LastName + " " + frenoyPlayer.FirstName;
            name = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(name.Trim().ToLowerInvariant());

            var existingPlayer = _db.Players.SingleOrDefault(x => x.Naam == name);
            if (existingPlayer != null)
            {
                if (_isVttl)
                    SetVttl(existingPlayer, frenoyPlayer);
                else
                    SetSporta(existingPlayer, frenoyPlayer);
                return null;
            }
            var newPlayer = new PlayerEntity();
            newPlayer.Naam = name;
            newPlayer.NaamKort = name;
            newPlayer.Toegang = PlayerToegang.Player;
            newPlayer.Email = frenoyPlayer.Email;
            if (frenoyPlayer.Phone != null)
            {
                newPlayer.Gsm = frenoyPlayer.Phone.Mobile;
            }
            if (frenoyPlayer.Address != null)
            {
                newPlayer.Adres = frenoyPlayer.Address.Line1;
                //frenoyPlayer.Address.Line1
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
    }
}