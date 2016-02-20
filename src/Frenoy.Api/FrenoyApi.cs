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
    public class FrenoyApi
    {
        #region Fields
        const string FrenoyVttlWsdlUrl = "http://api.vttl.be/0.7/?wsdl";
        const string FrenoySportaWsdlUrl = "http://tafeltennis.sporcrea.be/api/?wsdl";
        const string FrenoyVttlEndpoint = "http://api.vttl.be/0.7/index.php?s=vttl";
        const string FrenoySportaEndpoint = "http://tafeltennis.sporcrea.be/api/index.php?s=sporcrea";

        private readonly ITtcDbContext _db;
        private readonly FrenoySettings _settings;
        private readonly TabTAPI_PortTypeClient _frenoy;
        private readonly int _thuisClubId;
        private readonly bool _isVttl;

        private bool MapTeamPlayers = false;
        #endregion

        #region Constructor
        public FrenoyApi(ITtcDbContext ttcDbContext, Competition comp)
        {
            _db = ttcDbContext;
            bool isVttl = comp == Competition.Vttl;
            _settings = isVttl ? FrenoySettings.VttlSettings : FrenoySettings.SportaSettings;

            _isVttl = isVttl;
            if (isVttl)
            {
                _thuisClubId = _db.Clubs.Single(x => x.CodeVttl == _settings.FrenoyClub).Id;
                _frenoy = new FrenoyVttl.TabTAPI_PortTypeClient();
            }
            else
            {
                // Sporta
                _thuisClubId = _db.Clubs.Single(x => x.CodeSporta == _settings.FrenoyClub).Id;

                var binding = new BasicHttpBinding("TabTAPI_Binding");
                binding.Security.Mode = BasicHttpSecurityMode.None;
                var endpoint = new EndpointAddress(FrenoySportaEndpoint);
                _frenoy = new TabTAPI_PortTypeClient(binding, endpoint);
            }
        }
        #endregion

        public void SyncClubLokalen()
        {
            // TODO: put this in separate class
            // --> these methods need to be applied to vttl and sporta together
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
            SyncClubLokalen(clubs, getClubCode);
        }

        private void SyncClubLokalen(IEnumerable<ClubEntity> clubs, Func<ClubEntity, string> getClubCode)
        {
            foreach (var dbClub in clubs)
            {
                var oldLokalen = _db.ClubLokalen.Where(x => x.ClubId == dbClub.Id).ToArray();

                var frenoyClubs = _frenoy.GetClubs(new GetClubs
                {
                    Club = getClubCode(dbClub)
                });

                var frenoyClub = frenoyClubs.ClubEntries.FirstOrDefault();
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
            _db.SaveChanges();
        }

        #region Public API
        public void SyncAll()
        {
            // TODO: map all other results of the teams in the division aswell...            

            var frenoyTeams = _frenoy.GetClubTeams(new GetClubTeamsRequest
            {
                Club = _settings.FrenoyClub,
                Season = _settings.FrenoySeason
            });

            foreach (var frenoyTeam in frenoyTeams.TeamEntries)
            {
                // Create new division=reeks for each team in the club
                // Check if it already exists: Two teams could play in the same division
                Reeks reeks = _db.Reeksen.SingleOrDefault(x => x.FrenoyDivisionId.ToString() == frenoyTeam.DivisionId);

                // TODO: we zaten hier: FIXES for 2 teams in same reeks
                // --> Merge tabellen Reeks en ClubPloeg...
                // --> Reeksen.SingleOrDefault(... && x.TeamCode == frenoyTeam.????
                // --> Hier dan 2 reeksen toevoegen...
                // --> Daarna de Derbys maar 1x toevoegen aan de matchenlijst

                if (reeks == null)
                {
                    reeks = CreateReeks(frenoyTeam);
                    _db.Reeksen.Add(reeks);
                    CommitChanges();

                    // Create the teams in the new division=reeks
                    var frenoyDivision = _frenoy.GetDivisionRanking(new GetDivisionRankingRequest
                    {
                        DivisionId = frenoyTeam.DivisionId
                    });
                    foreach (var frenoyTeamsInDivision in frenoyDivision.RankingEntries)
                    {
                        var clubPloeg = CreateClubPloeg(reeks, frenoyTeamsInDivision);
                        _db.Opponents.Add(clubPloeg);
                    }
                    CommitChanges();
                }

                // Add Erembodegem players to the home team
                //var ploeg = _db.Opponents.Single(x => x.ClubId == _thuisClubId && x.ReeksId == reeks.Id && x.Code == frenoyTeam.Team);
                //if (MapTeamPlayers)
                //{
                //    var players = _settings.Players[ploeg.Code];
                //    foreach (var playerName in players)
                //    {
                //        var clubPloegSpeler = new ClubPloegSpeler
                //        {
                //            Kapitein = playerName == players.First() ? TeamPlayerType.Captain : TeamPlayerType.Standard,
                //            SpelerId = GetSpelerId(playerName),
                //            ReeksId = ploeg.Id
                //        };
                //        _db.ClubPloegSpelers.Add(clubPloegSpeler);
                //    }
                //    CommitChanges();
                //}

                // Create the matches=kalender table in the new  division=reeks
                GetMatchesResponse matches = _frenoy.GetMatches(new GetMatchesRequest
                {
                    Club = _settings.FrenoyClub,
                    Season = _settings.FrenoySeason,
                    DivisionId = reeks.FrenoyDivisionId.ToString(),
                    Team = reeks.TeamCode,
                    WithDetailsSpecified = true,
                    WithDetails = true,
                });
                SyncMatches(reeks.Id, reeks.TeamCode, matches);
            }
        }

        public void SyncMatch(Reeks reeks, string ploegCode, int weekName)
        {
            GetMatchesResponse matches = _frenoy.GetMatches(new GetMatchesRequest
            {
                Club = _settings.FrenoyClub,
                Season = _settings.FrenoySeason,
                Team = ploegCode,
                WithDetailsSpecified = true,
                WithDetails = true,
                WeekName = weekName.ToString()
            });
            SyncMatches(reeks.Id, ploegCode, matches);
        }

        private void SyncMatches(int reeksId, string ploegCode, GetMatchesResponse matches)
        {
            foreach (TeamMatchEntryType frenoyMatch in matches.TeamMatchesEntries.Where(x => x.HomeTeam.Trim() != "Vrij" && x.AwayTeam.Trim() != "Vrij"))
            {
                Debug.Assert(frenoyMatch.DateSpecified);
                Debug.Assert(frenoyMatch.TimeSpecified);

                // Kalender entries
                var kalender = _db.Kalender.SingleOrDefault(x => x.FrenoyMatchId == frenoyMatch.MatchId);
                if (kalender == null)
                {
                    kalender = CreateKalenderMatch(reeksId, frenoyMatch, ploegCode);
                    _db.Kalender.Add(kalender);
                }

                // Wedstrijdverslagen
                if (frenoyMatch.MatchDetails != null && frenoyMatch.MatchDetails.DetailsCreated)
                {
                    bool isForfeit = frenoyMatch.Score == null || frenoyMatch.Score.ToLowerInvariant().Contains("ff") || frenoyMatch.Score.ToLowerInvariant().Contains("af");

                    var verslag = _db.Verslagen.SingleOrDefault(x => x.KalenderId == kalender.Id);
                    if (verslag == null)
                    {
                        verslag = new Verslag
                        {
                            Kalender = kalender,
                            KalenderId = kalender.Id,
                            IsSyncedWithFrenoy = false,
                            SpelerId = Constants.SuperPlayerId,
                        };
                        _db.Verslagen.Add(verslag);
                    }

                    if (true || !verslag.IsSyncedWithFrenoy) // TODO: remove true || after final sync
                    {
                        if (!isForfeit)
                        {
                            // Uitslag
                            verslag.UitslagThuis = int.Parse(frenoyMatch.Score.Substring(0, frenoyMatch.Score.IndexOf("-")));
                            verslag.UitslagUit = int.Parse(frenoyMatch.Score.Substring(frenoyMatch.Score.IndexOf("-") + 1));
                            verslag.WO = 0;

                            // Spelers
                            var oldVerslagSpelers = _db.VerslagenSpelers.Where(x => x.MatchId == verslag.KalenderId).ToArray();
                            _db.VerslagenSpelers.RemoveRange(oldVerslagSpelers);

                            AddVerslagPlayers(frenoyMatch.MatchDetails.HomePlayers.Players, verslag, true, kalender.Thuis.Value == 1);
                            AddVerslagPlayers(frenoyMatch.MatchDetails.AwayPlayers.Players, verslag, false, kalender.Thuis.Value == 0);

                            // Matchen
                            var oldVerslagenIndividueel = _db.VerslagenIndividueel.Where(x => x.MatchId == verslag.KalenderId).ToArray();
                            _db.VerslagenIndividueel.RemoveRange(oldVerslagenIndividueel);

                            int id = 0;
                            foreach (var frenoyIndividual in frenoyMatch.MatchDetails.IndividualMatchResults)
                            {
                                VerslagIndividueel matchResult;
                                int homeUniqueIndex, awayUniqueIndex;
                                if (!int.TryParse(frenoyIndividual.HomePlayerUniqueIndex, out homeUniqueIndex)
                                    || !int.TryParse(frenoyIndividual.AwayPlayerUniqueIndex, out awayUniqueIndex))
                                {
                                    // Sporta doubles match:
                                    matchResult = new VerslagIndividueel
                                    {
                                        Id = id--,
                                        MatchId = verslag.KalenderId,
                                        MatchNumber = int.Parse(frenoyIndividual.Position),
                                        WalkOver = WalkOver.None
                                    };
                                }
                                else
                                {
                                    // Sporta/Vttl singles match
                                    matchResult = new VerslagIndividueel
                                    {
                                        Id = id--,
                                        MatchId = verslag.KalenderId,
                                        MatchNumber = int.Parse(frenoyIndividual.Position),
                                        HomePlayerUniqueIndex = homeUniqueIndex,
                                        OutPlayerUniqueIndex = awayUniqueIndex,
                                        WalkOver = WalkOver.None
                                    };
                                }
                                
                                if (frenoyIndividual.IsHomeForfeited || frenoyIndividual.IsAwayForfeited)
                                {
                                    matchResult.WalkOver = frenoyIndividual.IsHomeForfeited ? WalkOver.Home : WalkOver.Out;
                                }
                                else
                                {
                                    matchResult.HomePlayerSets = int.Parse(frenoyIndividual.HomeSetCount);
                                    matchResult.OutPlayerSets = int.Parse(frenoyIndividual.AwaySetCount);
                                }
                                _db.VerslagenIndividueel.Add(matchResult);
                            }
                        }
                        else
                        {
                            verslag.WO = 1;
                        }

                        verslag.IsSyncedWithFrenoy = true;
                    }
                }
                CommitChanges();
            }
        }

        private void AddVerslagPlayers(TeamMatchPlayerEntryType[] players, Verslag verslag, bool thuisSpeler, bool isHomeMatch)
        {
            foreach (var frenoyVerslagSpeler in players)
            {
                VerslagSpeler verslagSpeler = new VerslagSpeler
                {
                    MatchId = verslag.KalenderId,
                    Ranking = frenoyVerslagSpeler.Ranking,
                    Home = thuisSpeler,
                    Name = GetSpelerNaam(frenoyVerslagSpeler),
                    Position = int.Parse(frenoyVerslagSpeler.Position),
                    UniqueIndex = int.Parse(frenoyVerslagSpeler.UniqueIndex)
                };
                if (frenoyVerslagSpeler.VictoryCount != null)
                {
                    verslagSpeler.Won = int.Parse(frenoyVerslagSpeler.VictoryCount);
                }
                else
                {
                    Debug.Assert(frenoyVerslagSpeler.IsForfeited, "Either a VictoryCount or IsForfeited");
                }
                Speler dbPlayer = null;
                if ((isHomeMatch && thuisSpeler) || (!isHomeMatch && !thuisSpeler))
                {
                    if (_isVttl)
                    {
                        dbPlayer = _db.Spelers.SingleOrDefault(x => x.ComputerNummerVttl.HasValue && x.ComputerNummerVttl.Value.ToString() == frenoyVerslagSpeler.UniqueIndex);
                    }
                    else
                    {
                        dbPlayer = _db.Spelers.SingleOrDefault(x => x.LidNummerSporta.HasValue && x.LidNummerSporta.Value.ToString() == frenoyVerslagSpeler.UniqueIndex);
                    }
                }
                if (dbPlayer != null)
                {
                    verslagSpeler.PlayerId = dbPlayer.Id;
                    if (!string.IsNullOrWhiteSpace(dbPlayer.NaamKort))
                    {
                        verslagSpeler.Name = dbPlayer.NaamKort;
                    }
                }

                _db.VerslagenSpelers.Add(verslagSpeler);
            }
        }

        private static string GetSpelerNaam(TeamMatchPlayerEntryType frenoyVerslagSpeler)
        {
            System.Globalization.TextInfo ti = System.Globalization.CultureInfo.CurrentCulture.TextInfo;
            return ti.ToTitleCase((frenoyVerslagSpeler.FirstName + " " + frenoyVerslagSpeler.LastName).ToLowerInvariant());
        }
        #endregion

        #region Private Implementation
        private int GetSpelerId(string playerName)
        {
            var speler = _db.Spelers.Single(x => x.NaamKort == playerName);
            return speler.Id;
        }

        private readonly static Regex VttlReeksRegex = new Regex(@"Afdeling (\d+)(\w+)");
        private readonly static Regex SportaReeksRegex = new Regex(@"^(\d)(\w?)");
        private Reeks CreateReeks(TeamEntryType frenoyTeam)
        {
            var reeks = new Reeks();
            reeks.Competitie = _settings.Competitie;
            reeks.ReeksType = _settings.ReeksType;
            reeks.Jaar = _settings.Jaar;
            reeks.LinkId = $"{frenoyTeam.DivisionId}_{frenoyTeam.Team}";

            if (_isVttl)
            {
                var reeksMatch = VttlReeksRegex.Match(frenoyTeam.DivisionName);
                reeks.ReeksNummer = reeksMatch.Groups[1].Value;
                reeks.ReeksCode = reeksMatch.Groups[2].Value;
            }
            else
            {
                var reeksMatch = SportaReeksRegex.Match(frenoyTeam.DivisionName.Trim());
                reeks.ReeksNummer = reeksMatch.Groups[1].Value;
                reeks.ReeksCode = reeksMatch.Groups[2].Value;
            }

            reeks.FrenoyDivisionId = int.Parse(frenoyTeam.DivisionId);
            reeks.FrenoyTeamId = frenoyTeam.TeamId;
            return reeks;
        }

        private Kalender CreateKalenderMatch(int reeksId, TeamMatchEntryType frenoyMatch, string thuisPloegCode)
        {
            var kalender = new Kalender
            {
                FrenoyMatchId = frenoyMatch.MatchId,
                Datum = frenoyMatch.Date + new TimeSpan(frenoyMatch.Time.Hour, frenoyMatch.Time.Minute, 0),
                ThuisClubId = GetClubId(frenoyMatch.HomeClub),
                ThuisPloeg = ExtractTeamCodeFromFrenoyName(frenoyMatch.HomeTeam),
                UitClubId = GetClubId(frenoyMatch.AwayClub),
                UitPloeg = ExtractTeamCodeFromFrenoyName(frenoyMatch.AwayTeam),
                Week = int.Parse(frenoyMatch.WeekName),
                ReeksId = reeksId
            };

            //kalender.ThuisClubPloegId = GetClubPloegId(reeksId, kalender.ThuisClubId.Value, kalender.ThuisPloeg);
            kalender.UitClubPloegId = GetClubPloegId(reeksId, kalender.UitClubId.Value, kalender.UitPloeg);

            // In the db the ThuisClubId is always Erembodegem
            kalender.Thuis = kalender.ThuisClubId == _thuisClubId && kalender.ThuisPloeg == thuisPloegCode ? 1 : 0;
            if (kalender.Thuis == 0)
            {
                var thuisClubId = kalender.ThuisClubId;
                var thuisPloeg = kalender.ThuisPloeg;
                //var thuisClubPloegId = kalender.ThuisClubPloegId;

                kalender.ThuisClubId = kalender.UitClubId;
                kalender.ThuisPloeg = kalender.UitPloeg;
                //kalender.ThuisClubPloegId = kalender.UitClubPloegId;

                kalender.UitClubId = thuisClubId;
                kalender.UitPloeg = thuisPloeg;
                //kalender.UitClubPloegId = thuisClubPloegId;
            }
            return kalender;
        }

        private int GetClubPloegId(int reeksId, int clubId, string ploeg)
        {
            var cb = _db.Opponents.Single(x => x.ReeksId == reeksId && x.ClubId == clubId && x.Code == ploeg);
            return cb.Id;
        }

        private ClubPloeg CreateClubPloeg(Reeks reeks, RankingEntryType frenoyTeam)
        {
            var clubPloeg = new ClubPloeg();
            clubPloeg.ReeksId = reeks.Id;
            clubPloeg.ClubId = GetClubId(frenoyTeam.TeamClub);
            clubPloeg.Code = ExtractTeamCodeFromFrenoyName(frenoyTeam.Team);
            return clubPloeg;
        }

        private static readonly Regex ClubHasTeamCodeRegex = new Regex(@"\w$");
        private static string ExtractTeamCodeFromFrenoyName(string team)
        {
            if (ClubHasTeamCodeRegex.IsMatch(team))
            {
                return team.Substring(team.Length - 1);
            }
            Debug.Assert(false, "This code path is never been tested");
            return null;
        }

        private int GetClubId(string frenoyClubCode)
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
                Season = _settings.FrenoySeason
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
        #endregion

        #region Debug & Tech Stuff
        [Conditional("DEBUG")]
        private void CheckPlayers()
        {
            foreach (string player in _settings.Players.Values.SelectMany(x => x))
            {
                try
                {
                    GetSpelerId(player);
                }
                catch (Exception ex)
                {
                    throw new Exception("No player with NaamKort " + player, ex);
                }
            }
        }

        private void CommitChanges()
        {
            _db.SaveChanges();
        }
        #endregion

        public string GetFrenoyMatchId(int frenoyDivisionId, int week, string frenoyClubId)
        {
            var match = _frenoy.GetMatches(new GetMatchesRequest
            {   
                WeekName = week.ToString(),
                DivisionId = frenoyDivisionId.ToString(),
                Club = frenoyClubId
            });

            if (match.MatchCount == "0")
            {
                return null;
            }
            return match.TeamMatchesEntries.First().MatchId;
        }
    }
}