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
using System.Threading.Tasks;
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
        public FrenoyMatchesApi(ITtcDbContext ttcDbContext, Competition comp, bool forceSync = false)
            : base(ttcDbContext, comp, forceSync)
        {
            
        }
        #endregion

        #region Initial Season Load
        /// <summary>
        /// Initial season sync
        /// </summary>
        public async Task SyncTeamsAndMatches()
        {
            // ATTN: Not thread safe!
            var frenoyTeams = _frenoy.GetClubTeams(new GetClubTeamsRequest
            {
                Club = _settings.FrenoyClub,
                Season = _settings.FrenoySeason.ToString()
            });
            await SyncTeamsAndMatches(frenoyTeams);
        }

        private async Task SyncTeamsAndMatches(GetClubTeamsResponse frenoyTeams)
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
                    await CommitChanges();

                    // Create the teams in the new division=reeks
                    var frenoyDivision = _frenoy.GetDivisionRanking(new GetDivisionRankingRequest
                    {
                        DivisionId = frenoyTeam.DivisionId
                    });
                    foreach (var frenoyTeamsInDivision in frenoyDivision.RankingEntries.Where(x => ExtractTeamCodeFromFrenoyName(x.Team) != frenoyTeam.Team || !IsOwnClub(x.TeamClub)))
                    {
                        var teamOpponent = await CreateTeamOpponent(teamEntity, frenoyTeamsInDivision);
                        _db.TeamOpponents.Add(teamOpponent);
                    }
                    await CommitChanges();
                }

                await SyncTeamMatches(teamEntity);
            }
        }
        #endregion

        #region Public API
        public async Task SyncTeamMatches(TeamEntity team)
        {
            // Create the matches=kalender table in the new  division=reeks
            var matches = _frenoy.GetMatches(new GetMatchesRequest
            {
                Club = _settings.FrenoyClub,
                Season = _settings.FrenoySeason.ToString(), // TODO: replace with team.Year - 1999
                DivisionId = team.FrenoyDivisionId.ToString(),
                Team = team.TeamCode,
                WithDetailsSpecified = false,
                WithDetails = false,
            });
            await SyncMatches(team.Id, team.FrenoyDivisionId, matches, false);
        }

        public async Task SyncMatchDetails(MatchEntity matchEntity)
        {
            if (_forceSync || ShouldAttemptMatchSync(matchEntity.Id))
            {
                GetMatchesResponse matches = _frenoy.GetMatches(new GetMatchesRequest
                {
                    DivisionId = matchEntity.FrenoyDivisionId.ToString(),
                    WithDetailsSpecified = true,
                    WithDetails = true,
                    MatchId = matchEntity.FrenoyMatchId
                });
                Debug.Assert(matches.MatchCount == "1");
                await SyncMatchDetails(matchEntity, matches.TeamMatchesEntries[0]);
            }
        }

        public async Task<int?> SyncLastYearOpponentMatches(TeamEntity team, OpposingTeam opponent)
        {
            const int prevFrenoySeason = Constants.FrenoySeason - 1;
            string frenoyOpponentClub = GetFrenoyClubdId(opponent.ClubId);

            var opponentTeams = _frenoy.GetClubTeams(new GetClubTeamsRequest
            {
                Club = frenoyOpponentClub,
                Season = prevFrenoySeason.ToString()
            });

            var lastYearTeam = opponentTeams.TeamEntries.SingleOrDefault(x => x.Team == opponent.TeamCode && x.DivisionCategory == Constants.FrenoyTeamCategory);
            if (lastYearTeam != null)
            {
                int lastYearDivisionId = int.Parse(lastYearTeam.DivisionId);
                var ourTeam = _db.Teams.SingleOrDefault(x => x.Year == Constants.CurrentSeason - 1 && x.FrenoyDivisionId == lastYearDivisionId && x.Competition == _settings.Competition.ToString());
                if (ShouldAttemptOpponentMatchSync(opponent, team.Id, prevFrenoySeason))
                {
                    GetMatchesResponse matches = _frenoy.GetMatches(new GetMatchesRequest
                    {
                        Club = frenoyOpponentClub,
                        Season = prevFrenoySeason.ToString(),
                        Team = opponent.TeamCode,
                        WithDetailsSpecified = false,
                        WithDetails = false,
                        DivisionId = lastYearTeam.DivisionId
                    });
                    await SyncMatches(ourTeam?.Id, lastYearDivisionId, matches, false, prevFrenoySeason);
                }

                return lastYearDivisionId;
            }
            return null;
        }

        public async Task SyncOpponentMatches(TeamEntity team, OpposingTeam opponent)
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
                await SyncMatches(team.Id, team.FrenoyDivisionId, matches, false);
            }
        }
        #endregion

        #region Match Creation
        private async Task SyncMatches(int? teamId, int frenoyDivisionId, GetMatchesResponse matches, bool alsoSyncMatchDetails = true, int frenoySeason = Constants.FrenoySeason)
        {
            foreach (TeamMatchEntryType frenoyMatch in matches.TeamMatchesEntries)
            {
                // Kalender entries
                MatchEntity matchEntity = await _db.Matches.SingleOrDefaultAsync(x => x.FrenoyMatchId == frenoyMatch.MatchId && x.FrenoySeason == frenoySeason);
                if (matchEntity == null)
                {
                    matchEntity = new MatchEntity();
                    await MapMatch(matchEntity, teamId, frenoyDivisionId, frenoyMatch, frenoySeason);
                    _db.Matches.Add(matchEntity);
                    await CommitChanges();
                }
                else
                {
                    await MapMatch(matchEntity, teamId, frenoyDivisionId, frenoyMatch, frenoySeason);
                    await CommitChanges();
                }

                if (alsoSyncMatchDetails)
                {
                    await SyncMatchDetails(matchEntity, frenoyMatch);
                }
            }
        }

        private async Task MapMatch(MatchEntity entity, int? teamId, int frenoyDivisionId, TeamMatchEntryType frenoyMatch, int frenoySeason = Constants.FrenoySeason)
        {
            entity.ShouldBePlayed = !frenoyMatch.HomeTeam.Trim().StartsWith("Vrij") && !frenoyMatch.AwayTeam.Trim().StartsWith("Vrij");
            entity.FrenoyMatchId = frenoyMatch.MatchId;
            entity.Date = frenoyMatch.Date + new TimeSpan(frenoyMatch.Time.Hour, frenoyMatch.Time.Minute, 0);
            if (frenoyMatch.HomeClub != "-")
            {
                entity.HomeClubId = await GetClubId(frenoyMatch.HomeClub);
                entity.HomeTeamCode = ExtractTeamCodeFromFrenoyName(frenoyMatch.HomeTeam);
            }

            Debug.Assert(entity.ShouldBePlayed || frenoyMatch.AwayClub == "-" || frenoyMatch.HomeClub == "-");
            if (frenoyMatch.AwayClub != "-")
            {
                entity.AwayClubId = await GetClubId(frenoyMatch.AwayClub);
                entity.AwayTeamCode = ExtractTeamCodeFromFrenoyName(frenoyMatch.AwayTeam);
            }
            
            entity.Week = int.Parse(frenoyMatch.WeekName);
            entity.FrenoySeason = frenoySeason;
            entity.FrenoyDivisionId = frenoyDivisionId;
            entity.Competition = _settings.Competition;

            //TODO: we zaten hier for the derby problem
            // do not pass teamId here but find out what the Team is based on HomeClubId and HomeTeamCode
            if (teamId.HasValue)
            {
                if (entity.HomeClubId == Constants.OwnClubId)
                {
                    entity.HomeTeamId = teamId;
                }
                else if (entity.AwayClubId == Constants.OwnClubId)
                {
                    entity.AwayTeamId = teamId;
                }
            }
        }

        private async Task SyncMatchDetails(MatchEntity matchEntity, TeamMatchEntryType frenoyMatch)
        {
            if ((_forceSync || !matchEntity.IsSyncedWithFrenoy) && matchEntity.ShouldBePlayed)
            {
                if (frenoyMatch.Score != null)
                {
                    string score = frenoyMatch.Score.ToLowerInvariant();
                    bool isForfeit = score.Contains("ff") || score.Contains("af") || score.Contains("gu");
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
                    await AddMatchPlayers(frenoyMatch.MatchDetails.HomePlayers.Players, matchEntity, true);
                    await AddMatchPlayers(frenoyMatch.MatchDetails.AwayPlayers.Players, matchEntity, false);
                    //AssertMatchPlayers(matchEntity);

                    AddMatchGames(frenoyMatch, matchEntity);

                    await RemoveExistingMatchPlayersAndGames(matchEntity);
                }

                if (frenoyMatch.Score != null && frenoyMatch.MatchDetails != null && frenoyMatch.MatchDetails.DetailsCreated)
                {
                    matchEntity.IsSyncedWithFrenoy = true;
                }

                await CommitChanges();
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

        private async Task RemoveExistingMatchPlayersAndGames(MatchEntity matchEntity)
        {
            var oldMatchPlayers = await _db.MatchPlayers.Where(x => x.MatchId == matchEntity.Id).ToArrayAsync();
            _db.MatchPlayers.RemoveRange(oldMatchPlayers);

            var oldMatchGames = await _db.MatchGames.Where(x => x.MatchId == matchEntity.Id).ToArrayAsync();
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
            MatchGameEntity matchResult = null;
            if (frenoyIndividual.AwayPlayerUniqueIndex.Length == 2 || frenoyIndividual.HomePlayerUniqueIndex.Length == 2)
            {
                // TODO: We also got here when matchEntity.WalkOver = true
                //       HomePlayerUniqueIndex or AwayPlayerUniqueIndex = "" when the Team forfeited the entire season
                // 

                // TODO: We now have the Ids from doubles with Home/AwayPlayerUniqueIndex becoming an array

                // Sporta doubles match:
                matchResult = new MatchGameEntity
                {
                    Id = id,
                    MatchId = matchEntity.Id,
                    MatchNumber = int.Parse(frenoyIndividual.Position),
                    WalkOver = WalkOver.None
                };
            }
            else if (int.TryParse(frenoyIndividual.HomePlayerUniqueIndex.SingleOrDefault(), out var homeUniqueIndex) && 
                     int.TryParse(frenoyIndividual.AwayPlayerUniqueIndex.SingleOrDefault(), out var awayUniqueIndex))
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
            else
            {
                Debug.Fail("Shouldn't get here. This is either a singles or a doubles match...");
                return;
            }

            if (frenoyIndividual.IsHomeForfeited || frenoyIndividual.IsAwayForfeited)
            {
                matchResult.WalkOver = frenoyIndividual.IsHomeForfeited ? WalkOver.Home : WalkOver.Out;
            }
            else
            {
                if (frenoyIndividual.HomeSetCount == null || frenoyIndividual.AwaySetCount == null)
                {
                    // Some sort of WO?
                    // Position + Home or AwayPlayerMatchIndex is filled in but all the rest is null?

                    // TODO: Sporta doubles matches: the HomeSetCount/AwaySetCount is no longer filled in
                    // and the match result is not saved...

                    // The doubles result is still in the array of IndividualResults but has a different signature
                    // C# just silently skips it...
                    // uh...! https://github.com/gfrenoy/TabT-API

                    //[6] => stdClass Object
                    //  (
                    //      [Position] => 7
                    //      [HomePlayerMatchIndex] => Array
                    //          (
                    //              [0] => 1
                    //              [1] => 2
                    //          )
                    //
                    //      [HomePlayerUniqueIndex] => Array
                    //          (
                    //              [0] => 46671
                    //              [1] => 47012
                    //          )
                    //
                    //      [AwayPlayerMatchIndex] => Array
                    //          (
                    //              [0] => 2
                    //              [1] => 3
                    //          )
                    //
                    //      [AwayPlayerUniqueIndex] => Array
                    //          (
                    //              [0] => 8259
                    //              [1] => 9177
                    //          )
                    //
                    //      [HomeSetCount] => 0
                    //      [AwaySetCount] => 3
                    //  )
                    return;
                }

                matchResult.HomePlayerSets = int.Parse(frenoyIndividual.HomeSetCount);
                matchResult.AwayPlayerSets = int.Parse(frenoyIndividual.AwaySetCount);
            }
            matchEntity.Games.Add(matchResult);
        }

        private async Task AddMatchPlayers(TeamMatchPlayerEntryType[] players, MatchEntity match, bool thuisSpeler)
        {
            if (players == null)
                return;

            foreach (var frenoyVerslagSpeler in players)
            {
                if (string.IsNullOrWhiteSpace(frenoyVerslagSpeler.UniqueIndex))
                {
                    // Even more forfeited stuff
                    continue;
                }

                MatchPlayerEntity matchPlayerEntity = new MatchPlayerEntity
                {
                    MatchId = match.Id,
                    Ranking = frenoyVerslagSpeler.Ranking,
                    Home = thuisSpeler,
                    Name = GetSpelerNaam(frenoyVerslagSpeler),
                    Position = int.Parse(frenoyVerslagSpeler.Position),
                    UniqueIndex = int.Parse(frenoyVerslagSpeler.UniqueIndex),
                    Status = PlayerMatchStatus.Major
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
                        dbPlayer = await _db.Players.SingleOrDefaultAsync(x => x.ComputerNummerVttl.HasValue && x.ComputerNummerVttl.Value.ToString() == frenoyVerslagSpeler.UniqueIndex);
                    }
                    else
                    {
                        dbPlayer = await _db.Players.SingleOrDefaultAsync(x => x.LidNummerSporta.HasValue && x.LidNummerSporta.Value.ToString() == frenoyVerslagSpeler.UniqueIndex);
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
        private static readonly TimeSpan FrenoyPesterExpiration = TimeSpan.FromHours(1);
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
        private static bool ShouldAttemptOpponentMatchSync(OpposingTeam team, int teamId, int season = Constants.FrenoySeason)
        {
            string hash = season + team.TeamCode + team.ClubId + '-' + teamId;
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
        private static readonly Regex VttlDivisionRegex = new Regex(@"Afdeling (\d+)(\w+)");
        private static readonly Regex SportaDivisionRegex = new Regex(@"(\d)(\w)?");
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

        private async Task<TeamOpponentEntity> CreateTeamOpponent(TeamEntity teamEntity, RankingEntryType frenoyTeam)
        {
            var opponent = new TeamOpponentEntity
            {
                TeamId = teamEntity.Id,
                ClubId = await GetClubId(frenoyTeam.TeamClub),
                TeamCode = ExtractTeamCodeFromFrenoyName(frenoyTeam.Team)
            };
            return opponent;
        }
        #endregion
    }
}