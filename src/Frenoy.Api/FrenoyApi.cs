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
                // Create new division for each team in the club
                // Check if it already exists: Two teams could play in the same division
                TeamEntity teamEntity = _db.Teams.SingleOrDefault(x => x.FrenoyDivisionId.ToString() == frenoyTeam.DivisionId && x.TeamCode == frenoyTeam.Team);
                if (teamEntity == null)
                {
                    teamEntity = CreateReeks(frenoyTeam);
                    _db.Teams.Add(teamEntity);
                    CommitChanges();

                    // Create the teams in the new division=reeks
                    var frenoyDivision = _frenoy.GetDivisionRanking(new GetDivisionRankingRequest
                    {
                        DivisionId = frenoyTeam.DivisionId
                    });
                    foreach (var frenoyTeamsInDivision in frenoyDivision.RankingEntries.Where(x => ExtractTeamCodeFromFrenoyName(x.Team) != frenoyTeam.Team || !IsOwnClub(x.TeamClub)))
                    {
                        var clubPloeg = CreateClubPloeg(teamEntity, frenoyTeamsInDivision);
                        _db.TeamOpponents.Add(clubPloeg);
                    }
                    CommitChanges();
                }

                // Create the matches=kalender table in the new  division=reeks
                GetMatchesResponse matches = _frenoy.GetMatches(new GetMatchesRequest
                {
                    Club = _settings.FrenoyClub,
                    Season = _settings.FrenoySeason,
                    DivisionId = teamEntity.FrenoyDivisionId.ToString(),
                    Team = teamEntity.TeamCode,
                    WithDetailsSpecified = true,
                    WithDetails = true,
                });
                SyncMatches(teamEntity.Id, matches);
            }
        }

        private bool IsOwnClub(string teamClub)
        {
            return _settings.FrenoyClub == teamClub;
        }

        public void SyncMatch(int teamId, string frenoyMatchId)
        {
            GetMatchesResponse matches = _frenoy.GetMatches(new GetMatchesRequest
            {
                Club = _settings.FrenoyClub,
                Season = _settings.FrenoySeason,
                WithDetailsSpecified = true,
                WithDetails = true,
                MatchId = frenoyMatchId
            });
            SyncMatches(teamId, matches);
        }

        public void SyncMatch(int teamId, string ploegCode, int weekName)
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
            SyncMatches(teamId, matches);
        }

        private void SyncMatches(int reeksId, GetMatchesResponse matches)
        {
            foreach (TeamMatchEntryType frenoyMatch in matches.TeamMatchesEntries.Where(x => x.HomeTeam.Trim() != "Vrij" && x.AwayTeam.Trim() != "Vrij"))
            {
                Debug.Assert(frenoyMatch.DateSpecified);
                Debug.Assert(frenoyMatch.TimeSpecified);

                // Kalender entries
                MatchEntity kalender = _db.Matches.SingleOrDefault(x => x.FrenoyMatchId == frenoyMatch.MatchId);
                if (kalender == null)
                {
                    kalender = CreateKalenderMatch(reeksId, frenoyMatch);
                    _db.Matches.Add(kalender);
                }

                // Wedstrijdverslagen
                if (!kalender.IsSyncedWithFrenoy && frenoyMatch.MatchDetails != null && frenoyMatch.MatchDetails.DetailsCreated)
                {
                    bool isForfeit = frenoyMatch.Score == null || frenoyMatch.Score.ToLowerInvariant().Contains("ff") || frenoyMatch.Score.ToLowerInvariant().Contains("af");
                    if (!isForfeit)
                    {
                        // Uitslag
                        kalender.HomeScore = int.Parse(frenoyMatch.Score.Substring(0, frenoyMatch.Score.IndexOf("-")));
                        kalender.AwayScore = int.Parse(frenoyMatch.Score.Substring(frenoyMatch.Score.IndexOf("-") + 1));
                        kalender.WalkOver = false;

                        // Spelers
                        var oldVerslagSpelers = _db.MatchPlayers.Where(x => x.MatchId == kalender.Id).ToArray();
                        _db.MatchPlayers.RemoveRange(oldVerslagSpelers);

                        AddVerslagPlayers(frenoyMatch.MatchDetails.HomePlayers.Players, kalender, true);
                        AddVerslagPlayers(frenoyMatch.MatchDetails.AwayPlayers.Players, kalender, false);

                        // Matchen
                        var oldVerslagenIndividueel = _db.MatchGames.Where(x => x.MatchId == kalender.Id).ToArray();
                        _db.MatchGames.RemoveRange(oldVerslagenIndividueel);

                        int id = 0;
                        foreach (var frenoyIndividual in frenoyMatch.MatchDetails.IndividualMatchResults)
                        {
                            MatchGameEntity matchResult;
                            int homeUniqueIndex, awayUniqueIndex;
                            if (!int.TryParse(frenoyIndividual.HomePlayerUniqueIndex, out homeUniqueIndex)
                                || !int.TryParse(frenoyIndividual.AwayPlayerUniqueIndex, out awayUniqueIndex))
                            {
                                // Sporta doubles match:
                                matchResult = new MatchGameEntity
                                {
                                    Id = id--,
                                    MatchId = kalender.Id,
                                    MatchNumber = int.Parse(frenoyIndividual.Position),
                                    WalkOver = WalkOver.None
                                };
                            }
                            else
                            {
                                // Sporta/Vttl singles match
                                matchResult = new MatchGameEntity
                                {
                                    Id = id--,
                                    MatchId = kalender.Id,
                                    MatchNumber = int.Parse(frenoyIndividual.Position),
                                    HomePlayerUniqueIndex = homeUniqueIndex,
                                    AwayPlayerUniqueIndex = awayUniqueIndex,
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
                                matchResult.AwayPlayerSets = int.Parse(frenoyIndividual.AwaySetCount);
                            }
                            _db.MatchGames.Add(matchResult);
                        }
                    }
                    else
                    {
                        kalender.WalkOver = true;
                    }

                    kalender.IsSyncedWithFrenoy = true;
                }
                CommitChanges();
            }
        }

        private void AddVerslagPlayers(TeamMatchPlayerEntryType[] players, MatchEntity verslag, bool thuisSpeler)
        {
            foreach (var frenoyVerslagSpeler in players)
            {
                MatchPlayerEntity matchPlayerEntity = new MatchPlayerEntity
                {
                    MatchId = verslag.Id,
                    Ranking = frenoyVerslagSpeler.Ranking,
                    Home = thuisSpeler,
                    Name = GetSpelerNaam(frenoyVerslagSpeler),
                    Position = int.Parse(frenoyVerslagSpeler.Position),
                    UniqueIndex = int.Parse(frenoyVerslagSpeler.UniqueIndex)
                };
                if (frenoyVerslagSpeler.VictoryCount != null)
                {
                    matchPlayerEntity.Won = int.Parse(frenoyVerslagSpeler.VictoryCount);
                }
                else
                {
                    Debug.Assert(frenoyVerslagSpeler.IsForfeited, "Either a VictoryCount or IsForfeited");
                }

                PlayerEntity dbPlayer = null;
                if (verslag.IsHomeMatch.HasValue && ((verslag.IsHomeMatch.Value && thuisSpeler) || (!verslag.IsHomeMatch.Value && !thuisSpeler)))
                {
                    if (_isVttl)
                    {
                        dbPlayer = _db.Players.SingleOrDefault(x => x.ComputerNummerVttl.HasValue && x.ComputerNummerVttl.Value.ToString() == frenoyVerslagSpeler.UniqueIndex);
                    }
                    else
                    {
                        dbPlayer = _db.Players.SingleOrDefault(x => x.LidNummerSporta.HasValue && x.LidNummerSporta.Value.ToString() == frenoyVerslagSpeler.UniqueIndex);
                    }
                }
                if (dbPlayer != null)
                {
                    matchPlayerEntity.PlayerId = dbPlayer.Id;
                    if (!string.IsNullOrWhiteSpace(dbPlayer.NaamKort))
                    {
                        matchPlayerEntity.Name = dbPlayer.NaamKort;
                    }
                }

                _db.MatchPlayers.Add(matchPlayerEntity);
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
            var speler = _db.Players.Single(x => x.NaamKort == playerName);
            return speler.Id;
        }

        private readonly static Regex VttlReeksRegex = new Regex(@"Afdeling (\d+)(\w+)");
        private readonly static Regex SportaReeksRegex = new Regex(@"(\d)(\w)?");
        private TeamEntity CreateReeks(TeamEntryType frenoyTeam)
        {
            var reeks = new TeamEntity();
            reeks.Competition = _settings.Competitie;
            reeks.ReeksType = _settings.ReeksType;
            reeks.Year = _settings.Jaar;
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
            reeks.TeamCode = frenoyTeam.Team;
            return reeks;
        }

        private MatchEntity CreateKalenderMatch(int reeksId, TeamMatchEntryType frenoyMatch)
        {
            var kalender = new MatchEntity
            {
                FrenoyMatchId = frenoyMatch.MatchId,
                Date = frenoyMatch.Date + new TimeSpan(frenoyMatch.Time.Hour, frenoyMatch.Time.Minute, 0),
                HomeClubId = GetClubId(frenoyMatch.HomeClub),
                HomeTeamCode = ExtractTeamCodeFromFrenoyName(frenoyMatch.HomeTeam),
                AwayClubId = GetClubId(frenoyMatch.AwayClub),
                AwayPloegCode = ExtractTeamCodeFromFrenoyName(frenoyMatch.AwayTeam),
                Week = int.Parse(frenoyMatch.WeekName),
            };

            //TODO: we zaten hier
            // delete match id 563
            // do not pass reeksId here but find out what the Team is based on HomeClubId and HomeTeamCode

            if (kalender.HomeClubId == Constants.OwnClubId)
            {
                kalender.HomeTeamId = reeksId;
            }
            else if (kalender.AwayClubId == Constants.OwnClubId)
            {
                kalender.AwayTeamId = reeksId;
            }
            return kalender;
        }

        private TeamOpponentEntity CreateClubPloeg(TeamEntity teamEntity, RankingEntryType frenoyTeam)
        {
            var clubPloeg = new TeamOpponentEntity();
            clubPloeg.TeamId = teamEntity.Id;
            clubPloeg.ClubId = GetClubId(frenoyTeam.TeamClub);
            clubPloeg.TeamCode = ExtractTeamCodeFromFrenoyName(frenoyTeam.Team);
            return clubPloeg;
        }

        private static readonly Regex ClubHasTeamCodeRegex = new Regex(@"(\w)( \(af\))?$");
        private static string ExtractTeamCodeFromFrenoyName(string team)
        {
            var regMatch = ClubHasTeamCodeRegex.Match(team);
            if (regMatch.Success)
            {
                return regMatch.Groups[0].Value;
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