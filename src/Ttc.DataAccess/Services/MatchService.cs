using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Frenoy.Api;
using Ttc.DataAccess.Utilities;
using Ttc.DataEntities;
using Ttc.Model.Matches;
using Ttc.Model.Players;
using Ttc.Model.Teams;

namespace Ttc.DataAccess.Services
{
    public class MatchService : BaseService
    {
        private static IList<Match> _matches;
        public static bool MatchesPlaying;

        #region Getters
        public async Task<IList<Match>> GetMatches()
        {
            if (MatchesPlaying)
            {
                return _matches;
            }

            using (var dbContext = new TtcDbContext())
            {
                var currentFrenoySeason = dbContext.CurrentFrenoySeason;
                var matchEntities = await dbContext.Matches
                    .WithIncludes()
                    //.Where(x => x.Id == 802)
                    .Where(x => x.HomeClubId == Constants.OwnClubId || x.AwayClubId == Constants.OwnClubId)
                    .Where(x => x.FrenoySeason == currentFrenoySeason)
                    .ToListAsync();

                var matchIds = matchEntities.Select(x => x.Id).ToArray();
                var comments = await dbContext.MatchComments.Where(x => matchIds.Contains(x.MatchId)).ToArrayAsync();
                foreach (var match in matchEntities)
                {
                    match.Comments = comments.Where(x => x.MatchId == match.Id).ToArray();
                }

                var result = Mapper.Map<IList<MatchEntity>, IList<Match>>(matchEntities);
                //if (result.Any(m => m.ScoreType == MatchOutcome.BeingPlayed))
                //{
                //    _matches = result;
                      // BUG: Cached matches have sensitive information permanently removed after the first non-logged in user fetches...
                      // -> Just do it correctly with SignalR groups...
                //    MatchesPlaying = true;
                //}
                return result;
            }
        }

        public async Task<Match> GetMatch(int matchId, bool allowCache = false)
        {
            if (allowCache && _matches != null)
            {
                var ply = _matches.SingleOrDefault(x => x.Id == matchId);
                return ply ?? await GetMatch(matchId);
            }

            using (var dbContext = new TtcDbContext())
            {
                return await GetMatch(dbContext, matchId);
            }
        }

        public async Task<ICollection<OtherMatch>> GetOpponentMatches(int teamId, OpposingTeam opponent = null)
        {
            using (var dbContext = new TtcDbContext())
            {
                var team = await dbContext.Teams.SingleAsync(x => x.Id == teamId);

                async Task<MatchEntity[]> GetMatchEntities()
                {
                    // ReSharper disable AccessToDisposedClosure
                    var matches = dbContext.Matches
                        .WithIncludes()
                        .Where(match => match.FrenoyDivisionId == team.FrenoyDivisionId);
                    // ReSharper restore AccessToDisposedClosure

                    if (opponent != null)
                    {
                        matches = matches.Where(match =>
                            (match.AwayClubId == opponent.ClubId && match.AwayTeamCode == opponent.TeamCode) ||
                            (match.HomeClubId == opponent.ClubId && match.HomeTeamCode == opponent.TeamCode)
                        );
                    }

                    return await matches.ToArrayAsync();
                }


                MatchEntity[] matchEntities = await GetMatchEntities();

                if (opponent != null)
                {
                    // TODO BUG: This means that when called from Team Week Overview, no opponent is set and there is no sync...
                    // TODO PERFORMANCE: This executes too many times, make it part of initial competition load
                    var frenoy = new FrenoyMatchesApi(dbContext, Constants.NormalizeCompetition(team.Competition));
                    await frenoy.SyncOpponentMatches(team, opponent);

                    matchEntities = await GetMatchEntities();
                }

                // No comments for OpponentMatches

                var result = Mapper.Map<IList<MatchEntity>, IList<OtherMatch>>(matchEntities);
                return result;

                // TODO: Bug PreSeason code: This doesn't work! These results are NOT displayed in the MatchCard, the spinner just keeps on spinnin'
                // Pre season: Fetch last year matches instead
                //int? divisionId = frenoy.SyncLastYearOpponentMatches(team, opponent);
                //if (divisionId.HasValue)
                //{
                //    var matchEntities = dbContext.Matches
                //        .WithIncludes()
                //        .Where(match => (match.AwayClubId == opponent.ClubId && match.AwayTeamCode == opponent.TeamCode) || (match.HomeClubId == opponent.ClubId && match.HomeTeamCode == opponent.TeamCode))
                //        .Where(match => match.FrenoyDivisionId == divisionId.Value)
                //        .ToList();

                //    // No comments for OpponentMatches

                //    // HACK: hack om vorig jaar matchen te tonen in de frontend zonder te moeten berekenen wat hun "last year division id" is
                //    foreach (var match in matchEntities)
                //    {
                //        match.FrenoyDivisionId = team.FrenoyDivisionId;
                //    }

                //    var result = Mapper.Map<IList<MatchEntity>, IList<OtherMatch>>(matchEntities);
                //    return result;
                //}
            }
        }
        #endregion

        #region Boilerplate code
        private async Task<Match> GetMatch(TtcDbContext dbContext, int matchId)
        {
            var match = await dbContext.Matches
                    .WithIncludes()
                    .SingleOrDefaultAsync(x => x.Id == matchId);

            if (match == null)
                return null;

            var comments = await dbContext.MatchComments.Where(x => x.MatchId == matchId).ToArrayAsync();
            match.Comments = comments;

            var matchModel = Map(match);
            if (MatchesPlaying)
            {
                var cachedMatch = _matches.Single(x => x.Id == matchModel.Id);
                int matchIndex = _matches.IndexOf(cachedMatch);
                _matches[matchIndex] = matchModel;
            }
            return matchModel;
        }

        private Match Map(MatchEntity matchEntity)
        {
            return Mapper.Map<MatchEntity, Match>(matchEntity);
        }
        #endregion

        #region Putters
        public async Task<Match> UpdateScore(int matchId, MatchScore score)
        {
            using (var dbContext = new TtcDbContext())
            {
                var match = await dbContext.Matches
                    .WithIncludes()
                    .SingleAsync(x => x.Id == matchId);

                if (match.IsSyncedWithFrenoy)
                {
                    return await GetMatch(dbContext, match.Id);
                }

                match.AwayScore = score.Out;
                match.HomeScore = score.Home;
                dbContext.SaveChanges();

                return await GetMatch(dbContext, match.Id);
            }
        }

        #region Players
        public async Task<Match> SetMyFormation(MatchPlayer matchPlayer)
        {
            using (var dbContext = new TtcDbContext())
            {
                var existingPlayer = await dbContext.MatchPlayers
                    .Include(x => x.Match)
                    .Where(x => x.MatchId == matchPlayer.MatchId && x.PlayerId == matchPlayer.PlayerId)
                    .FirstOrDefaultAsync(x => x.Status != PlayerMatchStatus.Captain && x.Status != PlayerMatchStatus.Major);

                if (existingPlayer != null)
                {
                    existingPlayer.Status = matchPlayer.Status;
                    existingPlayer.StatusNote = matchPlayer.StatusNote;
                }
                else
                {
                    var verslagSpeler = Mapper.Map<MatchPlayer, MatchPlayerEntity>(matchPlayer);
                    dbContext.MatchPlayers.Add(verslagSpeler);
                }
                await dbContext.SaveChangesAsync();
            }
            var newMatch = await GetMatch(matchPlayer.MatchId);
            return newMatch;
        }

        /// <summary>
        /// Toggle Captain/Major player by any player of the team on the day of the match
        /// </summary>
        public async Task<Match> ToggleMatchPlayer(MatchPlayer matchPlayer)
        {
            Debug.Assert(matchPlayer.Status == PlayerMatchStatus.Captain || matchPlayer.Status == PlayerMatchStatus.Major);
            using (var dbContext = new TtcDbContext())
            {
                var match = await dbContext.Matches.FindAsync(matchPlayer.MatchId);
                if (match == null || match.IsSyncedWithFrenoy)
                {
                    return await GetMatch(matchPlayer.MatchId);
                }

                var existingPlayer = await dbContext.MatchPlayers
                    .Where(x => x.MatchId == matchPlayer.MatchId && x.PlayerId == matchPlayer.PlayerId)
                    .FirstOrDefaultAsync(x => x.Status == matchPlayer.Status);

                match.Block = matchPlayer.Status;
                if (existingPlayer != null)
                {
                    dbContext.MatchPlayers.Remove(existingPlayer);
                }
                else
                {
                    dbContext.MatchPlayers.Add(Mapper.Map<MatchPlayer, MatchPlayerEntity>(matchPlayer));
                }
                await dbContext.SaveChangesAsync();
            }
            var newMatch = await GetMatch(matchPlayer.MatchId);
            return newMatch;
        }

        /// <summary>
        /// Set all players for the match to Captain/Major
        /// </summary>
        /// <param name="blockAlso">Also block the match to the newStatus level</param>
        public async Task<Match> EditMatchPlayers(int matchId, int[] playerIds, string newStatus, bool blockAlso, string comment)
        {
            Debug.Assert(newStatus == PlayerMatchStatus.Captain || newStatus == PlayerMatchStatus.Major);
            using (var db = new TtcDbContext())
            {
                var match = await db.Matches.SingleAsync(x => x.Id == matchId);
                if (match.IsSyncedWithFrenoy)
                {
                    return await GetMatch(matchId);
                }

                match.FormationComment = comment;
                match.Block = blockAlso ? newStatus : null;
                var existingPlayers = await db.MatchPlayers
                    .Where(x => x.MatchId == matchId)
                    .Where(x => x.Status == newStatus)
                    .ToArrayAsync();
                db.MatchPlayers.RemoveRange(existingPlayers);

                for (int i = 0; i < playerIds.Length; i++)
                {
                    var player = await db.Players.FindAsync(playerIds[i]);
                    var newMatchPlayer = new MatchPlayerEntity
                    {
                        Id = i * -1,
                        MatchId = matchId,
                        PlayerId = player.Id,
                        Name = player.NaamKort,
                        Status = newStatus,
                        Ranking = match.Competition == Competition.Vttl ? player.KlassementVttl : player.KlassementSporta,
                        Home = match.IsHomeMatch.Value,
                        Position = i
                    };
                    db.MatchPlayers.Add(newMatchPlayer);
                }
                await db.SaveChangesAsync();
            }
            var newMatch = await GetMatch(matchId);
            return newMatch;
        }
        #endregion

        #region Report & Comments
        public async Task<Match> UpdateReport(MatchReport report)
        {
            using (var dbContext = new TtcDbContext())
            {
                var existingMatch = await dbContext.Matches.FirstAsync(x => x.Id == report.MatchId);
                existingMatch.ReportPlayerId = report.PlayerId;
                existingMatch.Description = report.Text;
                await dbContext.SaveChangesAsync();
            }
            var newMatch = await GetMatch(report.MatchId);
            return newMatch;
        }

        public async Task<Match> AddComment(MatchComment comment)
        {
            using (var dbContext = new TtcDbContext())
            {
                var entity = Mapper.Map<MatchCommentEntity>(comment);
                entity.PostedOn = TtcDbContext.GetCurrentBelgianDateTime();
                dbContext.MatchComments.Add(entity);
                await dbContext.SaveChangesAsync();
            }
            var newMatch = await GetMatch(comment.MatchId);
            return newMatch;
        }

        public async Task<Match> DeleteComment(int commentId)
        {
            using (var dbContext = new TtcDbContext())
            {
                var comment = await dbContext.MatchComments.SingleAsync(x => x.Id == commentId);
                dbContext.MatchComments.Remove(comment);
                await dbContext.SaveChangesAsync();

                return await GetMatch(dbContext, comment.MatchId);
            }
        }
        #endregion

        #region Frenoy Sync
        public async Task<OtherMatch> FrenoyOtherMatchSync(int matchId)
        {
            using (var dbContext = new TtcDbContext())
            {
                await FrenoyMatchSyncCore(dbContext, matchId, false);
                var match = dbContext.Matches
                    .WithIncludes()
                    .Single(x => x.Id == matchId);
                return Mapper.Map<MatchEntity, OtherMatch>(match);
            }
        }

        public async Task<Match> FrenoyMatchSync(int matchId, bool forceSync)
        {
            using (var dbContext = new TtcDbContext())
            {
                await FrenoyMatchSyncCore(dbContext, matchId, forceSync);
                return await GetMatch(dbContext, matchId);
            }
        }

        public async Task FrenoyTeamSync(int teamId)
        {
            using (var db = new TtcDbContext())
            {
                var team = await db.Teams.SingleAsync(x => x.Id == teamId);
                var frenoySync = new FrenoyMatchesApi(db, Constants.NormalizeCompetition(team.Competition));
                await frenoySync.SyncTeamMatches(team);
            }
        }

        private static async Task FrenoyMatchSyncCore(TtcDbContext dbContext, int matchId, bool forceSync)
        {
            var match = await dbContext.Matches
                .WithIncludes()
                .Include(x => x.HomeTeam)
                .Include(x => x.AwayTeam)
                .SingleAsync(x => x.Id == matchId);

            if (forceSync || (match.Date < DateTime.Now && !match.IsSyncedWithFrenoy))
            {
                var frenoySync = new FrenoyMatchesApi(dbContext, match.Competition, forceSync);
                await frenoySync.SyncMatchDetails(match);
            }
        }
        #endregion
        #endregion

        #region Excel Export
        public async Task<(byte[] file, SportaMatchFileInfo fileInfo)> GetExcelExport(int matchId, bool fillInOurTeam = true)
        {
            using (var dbContext = new TtcDbContext())
            {
                var exceller = await CreateExcelCreator(matchId, dbContext);
                return (exceller.Create(fillInOurTeam), exceller.FileInfo);
            }
        }

        private static async Task<SportaMatchExcelCreator> CreateExcelCreator(int matchId, TtcDbContext dbContext)
        {
            int currentSeason = dbContext.CurrentSeason;
            var activePlayers = await dbContext.Players
                .Where(x => x.Gestopt == null)
                .Where(x => x.ClubIdSporta.HasValue)
                .ToArrayAsync();

            var match = await dbContext.Matches
                .Include(x => x.HomeTeam)
                .Include(x => x.AwayTeam)
                .Include(x => x.Players)
                .SingleAsync(x => x.Id == matchId);

            var teams = await dbContext.Teams
                .Include(x => x.Opponents.Select(o => o.Club))
                .Where(x => x.Year == currentSeason)
                .Where(x => x.Competition == Competition.Sporta.ToString())
                .ToArrayAsync();

            var frenoy = new FrenoyPlayersApi(dbContext, Competition.Sporta);
            var theirClubId = match.AwayTeamId.HasValue ? match.HomeClubId : match.AwayClubId;
            var opponentPlayers = await frenoy.GetPlayers(theirClubId);

            var exceller = new SportaMatchExcelCreator(dbContext, match, activePlayers, teams, opponentPlayers);
            return exceller;
        }
        #endregion
    }
}