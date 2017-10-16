using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.ServiceModel;
using System.ServiceModel.Configuration;
using System.Text.RegularExpressions;
using Frenoy.Api;
using Frenoy.Api.FrenoyVttl;
using Ttc.DataEntities;
using Ttc.DataEntities.Core;
using Ttc.Model.Matches;
using Ttc.Model.Players;
using Ttc.Model.Teams;
using Match = Ttc.Model.Matches.Match;

namespace Frenoy.Api
{
    public class FrenoyApiBase
    {
        #region Fields
        const string FrenoyVttlWsdlUrl = "http://api.vttl.be/0.7/?wsdl";
        const string FrenoySportaWsdlUrl = "https://ttonline.sporta.be/api/?wsdl";
        const string FrenoyVttlEndpoint = "http://api.vttl.be/0.7/index.php?s=vttl";
        const string FrenoySportaEndpoint = "https://ttonline.sporta.be/api/index.php?s=sporcrea";

        protected readonly FrenoySettings _settings;
        protected readonly TabTAPI_PortTypeClient _frenoy;
        protected readonly bool _isVttl;
        protected readonly ITtcDbContext _db;
        protected readonly int _thuisClubId;
        protected readonly bool _forceSync;
        #endregion

        #region Constructor
        public FrenoyApiBase(ITtcDbContext ttcDbContext, Competition comp, bool forceSync = false)
        {
            _forceSync = forceSync;
            _db = ttcDbContext;

            bool isVttl = comp == Competition.Vttl;
            _settings = isVttl ? FrenoySettings.VttlSettings : FrenoySettings.SportaSettings;

            _isVttl = isVttl;
            if (isVttl)
            {
                _frenoy = new FrenoyVttl.TabTAPI_PortTypeClient();
                _thuisClubId = _db.Clubs.Single(x => x.CodeVttl == _settings.FrenoyClub).Id;
            }
            else
            {
                // Sporta
                _thuisClubId = _db.Clubs.Single(x => x.CodeSporta == _settings.FrenoyClub).Id;

                var binding = new BasicHttpBinding("TabTAPI_Binding");
                binding.Security.Mode = BasicHttpSecurityMode.Transport;
                var endpoint = new EndpointAddress(FrenoySportaEndpoint);
                _frenoy = new TabTAPI_PortTypeClient(binding, endpoint);
            }
        }
        #endregion     

        private static readonly Regex ClubHasTeamCodeRegex = new Regex(@"(\w)( \(af\))?$");
        protected static string ExtractTeamCodeFromFrenoyName(string team)
        {
            // team == Sint-Niklase Tafeltennisclub D
            var regMatch = ClubHasTeamCodeRegex.Match(team);
            if (regMatch.Success)
            {
                return regMatch.Groups[1].Value;
            }
            Debug.Assert(false, "This code path is never been tested");
            return null;
        }

        protected static bool ExtractIsForfaitFromFrenoyName(string team)
        {
            var regMatch = ClubHasTeamCodeRegex.Match(team);
            return regMatch.Groups[2].Success;
        }

        protected int GetClubId(string frenoyClubCode)
        {
            ClubEntity club;
            if (_isVttl)
            {
                club = _db.Clubs.SingleOrDefault(x => x.CodeVttl == frenoyClubCode);
            }
            else
            {
                club = _db.Clubs.SingleOrDefault(x => x.CodeSporta == frenoyClubCode);
            }
            if (club == null)
            {
                club = CreateClub(frenoyClubCode);
            }
            return club.Id;
        }

        private ClubEntity CreateClub(string frenoyClubCode)
        {
            Debug.Assert(_isVttl, "or need to write an if");
            var frenoyClub = _frenoy.GetClubs(new GetClubs
            {
                Club = frenoyClubCode,
                Season = _settings.FrenoySeason.ToString()
            });
            Debug.Assert(frenoyClub.ClubEntries.Count() == 1);

            var club = new ClubEntity
            {
                CodeVttl = frenoyClubCode,
                Actief = 1,
                Naam = frenoyClub.ClubEntries.First().LongName,
                Douche = 0
            };
            _db.Clubs.Add(club);
            CommitChanges();

            foreach (var frenoyLokaal in frenoyClub.ClubEntries.First().VenueEntries)
            {
                var lokaal = new ClubLokaal
                {
                    ClubId = club.Id,
                    Telefoon = frenoyLokaal.Phone,
                    Lokaal = frenoyLokaal.Name,
                    Adres = frenoyLokaal.Street,
                    Postcode = int.Parse(frenoyLokaal.Town.Substring(0, frenoyLokaal.Town.IndexOf(" "))),
                    Gemeente = frenoyLokaal.Town.Substring(frenoyLokaal.Town.IndexOf(" ") + 1),
                    Hoofd = 1
                };
                _db.ClubLokalen.Add(lokaal);
            }

            return club;
        }

        protected void CommitChanges()
        {
            _db.SaveChanges();
        }
    }
}