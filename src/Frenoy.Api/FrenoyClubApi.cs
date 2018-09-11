using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Frenoy.Api.FrenoyVttl;
using Ttc.DataEntities;
using Ttc.DataEntities.Core;
using Ttc.Model.Players;

namespace Frenoy.Api
{
    public class FrenoyClubApi : FrenoyApiBase
    {
        public FrenoyClubApi(ITtcDbContext ttcDbContext, Competition comp) : base(ttcDbContext, comp)
        {
        }

        #region ClubLokalen
        public async Task SyncClubLokalen()
        {
            // TODO: these methods need to be applied to vttl and sporta together
            // TODO: need to check with Dirk/Jelle if frenoy club locations are actually better than current data...

            Debug.Assert(false, "legacy db data might be better?");

            Func<ClubEntity, string> getClubCode;
            IEnumerable<ClubEntity> clubs;
            if (_isVttl)
            {
                getClubCode = dbClub => dbClub.CodeVttl;
                clubs = _db.Clubs.Include(x => x.Lokalen).Where(club => !string.IsNullOrEmpty(club.CodeVttl)).ToArray();
            }
            else
            {
                getClubCode = dbClub => dbClub.CodeSporta;
                clubs = _db.Clubs.Include(x => x.Lokalen).Where(club => !string.IsNullOrEmpty(club.CodeSporta)).ToArray();
            }
            await SyncClubLokalen(clubs, getClubCode);
        }

        private async Task SyncClubLokalen(IEnumerable<ClubEntity> clubs, Func<ClubEntity, string> getClubCode)
        {
            foreach (var dbClub in clubs)
            {
                var oldLokalen = await _db.ClubLokalen.Where(x => x.ClubId == dbClub.Id).ToArrayAsync();

                var frenoyClubs = await _frenoy.GetClubsAsync(new GetClubs
                {
                    Club = getClubCode(dbClub)
                });

                var frenoyClub = frenoyClubs.GetClubsResponse.ClubEntries.FirstOrDefault();
                if (frenoyClub == null)
                {
                    Debug.Print("Got some wrong CodeSporta/Vttl in legacy db: " + dbClub.Naam);
                }
                else if (frenoyClub.VenueEntries == null)
                {
                    Debug.Print("Missing frenoy data?: " + dbClub.Naam);
                }
                else if (frenoyClub.VenueEntries.Length < dbClub.Lokalen.Count)
                {
                    Debug.Print("we got better data...: " + dbClub.Naam);
                }
                else
                {
                    _db.ClubLokalen.RemoveRange(oldLokalen);

                    foreach (var frenoyLokaal in frenoyClub.VenueEntries)
                    {
                        //Debug.Assert(string.IsNullOrWhiteSpace(frenoyLokaal.Comment), "comments opslaan in db?");
                        Debug.Assert(frenoyLokaal.ClubVenue == "1");
                        var lokaal = new ClubLokaal
                        {
                            Lokaal = frenoyLokaal.Name,
                            Adres = frenoyLokaal.Street,
                            ClubId = dbClub.Id,
                            Gemeente = frenoyLokaal.Town.Substring(frenoyLokaal.Town.IndexOf(" ") + 1),
                            Telefoon = frenoyLokaal.Phone,
                            Postcode = int.Parse(frenoyLokaal.Town.Substring(0, frenoyLokaal.Town.IndexOf(" "))),
                            Hoofd = frenoyLokaal.ClubVenue == "1" ? 1 : 0
                        };
                        _db.ClubLokalen.Add(lokaal);
                    }
                }
            }
            await _db.SaveChangesAsync();
        }
        #endregion
    }
}
