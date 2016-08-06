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
    public class FrenoyMatchesApi : FrenoyApiBase
    {
        #region Constructor
        public FrenoyMatchesApi(ITtcDbContext ttcDbContext, Competition comp)
            : base(ttcDbContext, comp)
        {
            
        }
        #endregion

        #region Initial Season Load
        /// <summary>
        /// Initial season sync
        /// </summary>
        public void SyncTeamsAndMatches()
        {
            // ATTN: Not thread safe!
            var frenoyTeams = _frenoy.GetClubTeams(new GetClubTeamsRequest
            {
                Club = _settings.FrenoyClub,
                Season = _settings.FrenoySeason.ToString()
            });
            SyncTeamsAndMatches(frenoyTeams);
        }

        private void SyncTeamsAndMatches(GetClubTeamsResponse frenoyTeams)
        {
            foreach (var frenoyTeam in frenoyTeams.TeamEntries)
            {
                // Create new division for each team in the club
                // Check if it already exists: Two teams could play in the same division
                TeamEntity teamEntity = _db.Teams.SingleOrDefault(x => x.FrenoyDivisionId.ToString() == frenoyTeam.DivisionId && x.TeamCode == frenoyTeam.Team);
                if (teamEntity == null)
                {
                    teamEntity = CreateTeam(frenoyTeam);
                    _db.Teams.Add(teamEntity);
                    CommitChanges();

                    // Create the teams in the new division=reeks
                    var frenoyDivision = _frenoy.GetDivisionRanking(new GetDivisionRankingRequest
                    {
                        DivisionId = frenoyTeam.DivisionId
                    });
                    foreach (var frenoyTeamsInDivision in frenoyDivision.RankingEntries.Where(x => ExtractTeamCodeFromFrenoyName(x.Team) != frenoyTeam.Team || !IsOwnClub(x.TeamClub)))
                    {
                        var teamOpponent = CreateTeamOpponent(teamEntity, frenoyTeamsInDivision);
                        _db.TeamOpponents.Add(teamOpponent);
                    }
                    CommitChanges();
                }

                // Create the matches=kalender table in the new  division=reeks
                GetMatchesResponse matches = _frenoy.GetMatches(new GetMatchesRequest
                {
                    Club = _settings.FrenoyClub,
                    Season = _settings.FrenoySeason.ToString(),
                    DivisionId = teamEntity.FrenoyDivisionId.ToString(),
                    Team = teamEntity.TeamCode,
                    WithDetailsSpecified = false,
                    WithDetails = false,
                });
                SyncMatches(teamEntity.Id, teamEntity.FrenoyDivisionId, matches, false);
            }
        }
        #endregion

        #region Public API
        public void SyncMatchDetails(MatchEntity matchEntity)
        {
            if (ShouldAttemptMatchSync(matchEntity.Id))
            {
                GetMatchesResponse matches = _frenoy.GetMatches(new GetMatchesRequest
                {
                    DivisionId = matchEntity.FrenoyDivisionId.ToString(),
                    Season = _settings.FrenoySeason.ToString(),
                    WithDetailsSpecified = true,
                    WithDetails = true,
                    MatchId = matchEntity.FrenoyMatchId
                });
                Debug.Assert(matches.MatchCount == "1");
                SyncMatchDetails(matchEntity, matches.TeamMatchesEntries[0]);
            }
        }

        public void SyncLastOpponentMatches(TeamEntity team, OpposingTeam opponent)
        {
            if (ShouldAttemptOpponentMatchSync(opponent, team.Id))
            {
                GetMatchesResponse matches = _frenoy.GetMatches(new GetMatchesRequest
                {
                    Club = GetFrenoyClubdId(opponent.ClubId),
                    Season = _settings.FrenoySeason.ToString(),
                    Team = opponent.TeamCode,
                    WithDetailsSpecified = false,
                    WithDetails = false,
                    DivisionId = team.FrenoyDivisionId.ToString()
                });
                SyncMatches(team.Id, team.FrenoyDivisionId, matches, false);
            }
        }
        #endregion

        #region Match Creation
        private void SyncMatches(int teamId, int frenoyDivisionId, GetMatchesResponse matches, bool alsoSyncMatchDetails = true)
        {
            foreach (TeamMatchEntryType frenoyMatch in matches.TeamMatchesEntries.Where(x => !x.HomeTeam.Trim().StartsWith("Vrij") && !x.AwayTeam.Trim().StartsWith("Vrij")))
            {
                Debug.Assert(frenoyMatch.DateSpecified, "Probably need to filter this one out?");
                Debug.Assert(frenoyMatch.TimeSpecified);

                // Kalender entries
                MatchEntity matchEntity = _db.Matches.SingleOrDefault(x => x.FrenoyMatchId == frenoyMatch.MatchId);
                if (matchEntity == null)
                {
                    matchEntity = CreateMatch(teamId, frenoyDivisionId, frenoyMatch);
                    _db.Matches.Add(matchEntity);
                    CommitChanges();
                }

                if (alsoSyncMatchDetails)
                {
                    SyncMatchDetails(matchEntity, frenoyMatch);
                }
            }
        }

        private MatchEntity CreateMatch(int teamId, int frenoyDivisionId, TeamMatchEntryType frenoyMatch)
        {
            var matchEntity = new MatchEntity
            {
                FrenoyMatchId = frenoyMatch.MatchId,
                Date = frenoyMatch.Date + new TimeSpan(frenoyMatch.Time.Hour, frenoyMatch.Time.Minute, 0),
                HomeClubId = GetClubId(frenoyMatch.HomeClub),
                HomeTeamCode = ExtractTeamCodeFromFrenoyName(frenoyMatch.HomeTeam),
                AwayClubId = GetClubId(frenoyMatch.AwayClub),
                AwayTeamCode = ExtractTeamCodeFromFrenoyName(frenoyMatch.AwayTeam),
                Week = int.Parse(frenoyMatch.WeekName),
                FrenoySeason = _settings.FrenoySeason,
                FrenoyDivisionId = frenoyDivisionId,
                Competition = _settings.Competition,
            };

            //int weekName;
            //if (int.TryParse(frenoyMatch.WeekName, out weekName))
            //{
            //    kalender.Week = weekName;
            //}

            //TODO: we zaten hier for the derby problem
            // delete match id 563
            // do not pass teamId here but find out what the Team is based on HomeClubId and HomeTeamCode

            if (matchEntity.HomeClubId == Constants.OwnClubId)
            {
                matchEntity.HomeTeamId = teamId;
            }
            else if (matchEntity.AwayClubId == Constants.OwnClubId)
            {
                matchEntity.AwayTeamId = teamId;
            }

            return matchEntity;
        }

        private void SyncMatchDetails(MatchEntity matchEntity, TeamMatchEntryType frenoyMatch)
        {
            if (!matchEntity.IsSyncedWithFrenoy)
            {
                if (frenoyMatch.Score != null)
                {
                    bool isForfeit = frenoyMatch.Score.ToLowerInvariant().Contains("ff") || frenoyMatch.Score.ToLowerInvariant().Contains("af");
                    if (!isForfeit)
                    {
                        // Uitslag
                        matchEntity.HomeScore = int.Parse(frenoyMatch.Score.Substring(0, frenoyMatch.Score.IndexOf("-")));
                        matchEntity.AwayScore = int.Parse(frenoyMatch.Score.Substring(frenoyMatch.Score.IndexOf("-") + 1));
                        matchEntity.WalkOver = false;
                    }
                    else
                    {
                        matchEntity.WalkOver = true;
                    }
                }

                if (frenoyMatch.MatchDetails != null && frenoyMatch.MatchDetails.DetailsCreated)
                {
                    AddMatchPlayers(frenoyMatch.MatchDetails.HomePlayers.Players, matchEntity, true);
                    AddMatchPlayers(frenoyMatch.MatchDetails.AwayPlayers.Players, matchEntity, false);
                    //AssertMatchPlayers(matchEntity);

                    AddMatchGames(frenoyMatch, matchEntity);

                    RemoveExistingMatchPlayersAndGames(matchEntity);
                }

                if (frenoyMatch.Score != null && frenoyMatch.MatchDetails != null && frenoyMatch.MatchDetails.DetailsCreated)
                {
                    matchEntity.IsSyncedWithFrenoy = true;
                }

                CommitChanges();
            }
        }

        private static void AddMatchGames(TeamMatchEntryType frenoyMatch, MatchEntity matchEntity)
        {
            if (frenoyMatch.MatchDetails.IndividualMatchResults != null)
            {
                int id = 0;
                foreach (var frenoyIndividual in frenoyMatch.MatchDetails.IndividualMatchResults)
                {
                    id--;
                    AddMatchGames(frenoyIndividual, id, matchEntity);
                }
            }
        }

        private void RemoveExistingMatchPlayersAndGames(MatchEntity matchEntity)
        {
            var oldMatchPlayers = _db.MatchPlayers.Where(x => x.MatchId == matchEntity.Id).ToArray();
            _db.MatchPlayers.RemoveRange(oldMatchPlayers);

            var oldMatchGames = _db.MatchGames.Where(x => x.MatchId == matchEntity.Id).ToArray();
            _db.MatchGames.RemoveRange(oldMatchGames);
        }

        private static void AssertMatchPlayers(MatchEntity matchEntity)
        {
            var testPlayer = matchEntity.Players.Count(x => x.PlayerId != 0) == 0 || matchEntity.Players.Count > 8;
            if (testPlayer && (matchEntity.AwayTeamId.HasValue || matchEntity.HomeTeamId.HasValue))
            {
                Debug.Assert(false, "player problem");
            }
        }

        private static void AddMatchGames(IndividualMatchResultEntryType frenoyIndividual, int id, MatchEntity matchEntity)
        {
            MatchGameEntity matchResult;
            int homeUniqueIndex, awayUniqueIndex;
            if (!int.TryParse(frenoyIndividual.HomePlayerUniqueIndex, out homeUniqueIndex) || !int.TryParse(frenoyIndividual.AwayPlayerUniqueIndex, out awayUniqueIndex))
            {
                // Sporta doubles match:
                matchResult = new MatchGameEntity
                {
                    Id = id,
                    MatchId = matchEntity.Id,
                    MatchNumber = int.Parse(frenoyIndividual.Position),
                    WalkOver = WalkOver.None
                };
            }
            else
            {
                // Sporta/Vttl singles match
                matchResult = new MatchGameEntity
                {
                    Id = id,
                    MatchId = matchEntity.Id,
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
            matchEntity.Games.Add(matchResult);
        }

        private void AddMatchPlayers(TeamMatchPlayerEntryType[] players, MatchEntity match, bool thuisSpeler)
        {
            foreach (var frenoyVerslagSpeler in players)
            {
                MatchPlayerEntity matchPlayerEntity = new MatchPlayerEntity
                {
                    MatchId = match.Id,
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
                if (match.IsHomeMatch.HasValue && ((match.IsHomeMatch.Value && thuisSpeler) || (!match.IsHomeMatch.Value && !thuisSpeler)))
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

                match.Players.Add(matchPlayerEntity);
            }
        }
        #endregion

        #region Cache
        private readonly static TimeSpan FrenoyPesterExpiration = TimeSpan.FromHours(1);
        private static readonly Dictionary<int, DateTime> FrenoyNoPesterCache = new Dictionary<int, DateTime>();
        private static readonly object FrenoyNoPesterLock = new object();
        private static bool ShouldAttemptMatchSync(int matchId)
        {
            lock (FrenoyNoPesterLock)
            {
                if (!FrenoyNoPesterCache.ContainsKey(matchId))
                {
                    FrenoyNoPesterCache.Add(matchId, DateTime.Now);
                    return true;
                }

                bool shouldSync = FrenoyNoPesterCache[matchId] > DateTime.Now + FrenoyPesterExpiration;
                if (shouldSync)
                {
                    FrenoyNoPesterCache.Remove(matchId);
                }
                return shouldSync;
            }
        }

        private static readonly Dictionary<string, DateTime> FrenoyOpponentCache = new Dictionary<string, DateTime>();
        private static readonly object FrenoyOpponentLock = new object();
        private static bool ShouldAttemptOpponentMatchSync(OpposingTeam team, int teamId)
        {
            string hash = team.TeamCode + team.ClubId + '-' + teamId;
            lock (FrenoyOpponentLock)
            {
                if (!FrenoyOpponentCache.ContainsKey(hash))
                {
                    FrenoyOpponentCache.Add(hash, DateTime.Now);
                    return true;
                }

                bool shouldSync = FrenoyOpponentCache[hash] > DateTime.Now + FrenoyPesterExpiration;
                if (shouldSync)
                {
                    FrenoyOpponentCache.Remove(hash);
                }
                return shouldSync;
            }
        }
        #endregion

        #region Private Implementation
        private bool IsOwnClub(string teamClub)
        {
            return _settings.FrenoyClub == teamClub;
        }

        private static string GetSpelerNaam(TeamMatchPlayerEntryType frenoyVerslagSpeler)
        {
            System.Globalization.TextInfo ti = System.Globalization.CultureInfo.CurrentCulture.TextInfo;
            return ti.ToTitleCase((frenoyVerslagSpeler.FirstName + " " + frenoyVerslagSpeler.LastName).ToLowerInvariant());
        }

        private int GetSpelerId(string playerName)
        {
            var speler = _db.Players.Single(x => x.NaamKort == playerName);
            return speler.Id;
        }

        private string GetFrenoyClubdId(int clubId)
        {
            if (_isVttl)
            {
                return _db.Clubs.Single(x => x.Id == clubId).CodeVttl;
            }
            else
            {
                return _db.Clubs.Single(x => x.Id == clubId).CodeSporta;
            }
        }
        #endregion

        #region Create Teams
        private readonly static Regex VttlDivisionRegex = new Regex(@"Afdeling (\d+)(\w+)");
        private readonly static Regex SportaDivisionRegex = new Regex(@"(\d)(\w)?");
        private TeamEntity CreateTeam(TeamEntryType frenoyTeam)
        {
            var team = new TeamEntity();
            team.Competition = _settings.Competition.ToString();
            team.ReeksType = _settings.DivisionType;
            team.Year = _settings.Year;
            team.LinkId = $"{frenoyTeam.DivisionId}_{frenoyTeam.Team}";

            if (_isVttl)
            {
                var teamRegexMatch = VttlDivisionRegex.Match(frenoyTeam.DivisionName);
                team.ReeksNummer = teamRegexMatch.Groups[1].Value;
                team.ReeksCode = teamRegexMatch.Groups[2].Value;
            }
            else
            {
                var teamRegexMatch = SportaDivisionRegex.Match(frenoyTeam.DivisionName.Trim());
                team.ReeksNummer = teamRegexMatch.Groups[1].Value;
                team.ReeksCode = teamRegexMatch.Groups[2].Value;
            }

            team.FrenoyDivisionId = int.Parse(frenoyTeam.DivisionId);
            team.FrenoyTeamId = frenoyTeam.TeamId;
            team.TeamCode = frenoyTeam.Team;
            return team;
        }

        private TeamOpponentEntity CreateTeamOpponent(TeamEntity teamEntity, RankingEntryType frenoyTeam)
        {
            var opponent = new TeamOpponentEntity
            {
                TeamId = teamEntity.Id,
                ClubId = GetClubId(frenoyTeam.TeamClub),
                TeamCode = ExtractTeamCodeFromFrenoyName(frenoyTeam.Team)
            };
            return opponent;
        }
        #endregion
    }
}