using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using AutoMapper;
using Frenoy.Api;
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
        public ICollection<Match> GetMatches()
        {
            if (MatchesPlaying)
            {
                return _matches;
            }

            using (var dbContext = new TtcDbContext())
            {
                var matchEntities = dbContext.Matches
                    .WithIncludes()
                    //.Where(x => x.Id == 802)
                    .Where(x => x.HomeClubId == Constants.OwnClubId || x.AwayClubId == Constants.OwnClubId)
                    .Where(x => x.FrenoySeason == Constants.FrenoySeason)
                    .ToList();

                var matchIds = matchEntities.Select(x => x.Id).ToArray();
                var comments = dbContext.MatchComments.Where(x => matchIds.Contains(x.MatchId)).ToArray();
                foreach (var match in matchEntities)
                {
                    match.Comments = comments.Where(x => x.MatchId == match.Id).ToArray();
                }

                var result = Mapper.Map<IList<MatchEntity>, IList<Match>>(matchEntities);
                if (result.Any(m => m.ScoreType == MatchOutcome.BeingPlayed))
                {
                    _matches = result;
                    MatchesPlaying = true;
                }
                return result;
            }
        }

        public Match GetMatch(int matchId)
        {
            using (var dbContext = new TtcDbContext())
            {
                return GetMatch(dbContext, matchId);
            }
        }

        public ICollection<OtherMatch> GetLastOpponentMatches(int teamId, OpposingTeam opponent)
        {
            using (var dbContext = new TtcDbContext())
            {
                var team = dbContext.Teams.Single(x => x.Id == teamId);
                var frenoy = new FrenoyMatchesApi(dbContext, Constants.NormalizeCompetition(team.Competition));

                var firstMatch = dbContext.Matches.Where(x => x.FrenoySeason == Constants.FrenoySeason).Min(x => x.Date);
                if (DateTime.Now > firstMatch)
                {
                    frenoy.SyncLastOpponentMatches(team, opponent);

                    var matchEntities = dbContext.Matches
                        .WithIncludes()
                        .Where(match => (match.AwayClubId == opponent.ClubId && match.AwayTeamCode == opponent.TeamCode) || (match.HomeClubId == opponent.ClubId && match.HomeTeamCode == opponent.TeamCode))
                        .Where(match => match.FrenoyDivisionId == team.FrenoyDivisionId)
                        .ToList();

                    // No comments for OpponentMatches

                    var result = Mapper.Map<IList<MatchEntity>, IList<OtherMatch>>(matchEntities);
                    return result;
                }
                else
                {
                    // Pre season: Fetch last year matches instead
                    int? divisionId = frenoy.SyncLastYearOpponentMatches(team, opponent);
                    if (divisionId.HasValue)
                    {
                        var matchEntities = dbContext.Matches
                            .WithIncludes()
                            .Where(match => (match.AwayClubId == opponent.ClubId && match.AwayTeamCode == opponent.TeamCode) || (match.HomeClubId == opponent.ClubId && match.HomeTeamCode == opponent.TeamCode))
                            .Where(match => match.FrenoyDivisionId == divisionId.Value)
                            .ToList();

                        // No comments for OpponentMatches

                        // HACK: hack om vorig jaar matchen te tonen in de frontend zonder te moeten berekenen wat hun "last year division id" is
                        foreach (var match in matchEntities)
                        {
                            match.FrenoyDivisionId = team.FrenoyDivisionId;
                        }

                        var result = Mapper.Map<IList<MatchEntity>, IList<OtherMatch>>(matchEntities);
                        return result;
                    }
                }
            }
            return null;
        }
        #endregion

        #region Boilderplate code
        private Match GetMatch(TtcDbContext dbContext, int matchId)
        {
            var match = dbContext.Matches
                    .WithIncludes()
                    .SingleOrDefault(x => x.Id == matchId);

            if (match == null)
                return null;

            var comments = dbContext.MatchComments.Where(x => x.MatchId == matchId).ToArray();
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
        public Match UpdateScore(int matchId, MatchScore score)
        {
            using (var dbContext = new TtcDbContext())
            {
                var match = dbContext.Matches
                    .WithIncludes()
                    .Single(x => x.Id == matchId);

                match.AwayScore = score.Out;
                match.HomeScore = score.Home;
                dbContext.SaveChanges();

                return GetMatch(dbContext, match.Id);
            }
        }

        #region Players
        public Match SetMyFormation(MatchPlayer matchPlayer)
        {
            using (var dbContext = new TtcDbContext())
            {
                var existingPlayer = dbContext.MatchPlayers
                    .Include(x => x.Match)
                    .Where(x => x.MatchId == matchPlayer.MatchId && x.PlayerId == matchPlayer.PlayerId)
                    .FirstOrDefault(x => x.Status != PlayerMatchStatus.Captain && x.Status != PlayerMatchStatus.Major);

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
                dbContext.SaveChanges();
            }
            var newMatch = GetMatch(matchPlayer.MatchId);
            return newMatch;
        }

        /// <summary>
        /// Toggle Captain/Major player by any player of the team on the day of the match
        /// </summary>
        public Match ToggleMatchPlayer(MatchPlayer matchPlayer)
        {
            Debug.Assert(matchPlayer.Status == PlayerMatchStatus.Captain || matchPlayer.Status == PlayerMatchStatus.Major);
            using (var dbContext = new TtcDbContext())
            {
                var match = dbContext.Matches.Find(matchPlayer.MatchId);
                var existingPlayer = dbContext.MatchPlayers
                    .Where(x => x.MatchId == matchPlayer.MatchId && x.PlayerId == matchPlayer.PlayerId)
                    .FirstOrDefault(x => x.Status == matchPlayer.Status);

                match.Block = matchPlayer.Status;
                if (existingPlayer != null)
                {
                    dbContext.MatchPlayers.Remove(existingPlayer);
                }
                else
                {
                    dbContext.MatchPlayers.Add(Mapper.Map<MatchPlayer, MatchPlayerEntity>(matchPlayer));
                }
                dbContext.SaveChanges();
            }
            var newMatch = GetMatch(matchPlayer.MatchId);
            return newMatch;
        }

        /// <summary>
        /// Set all players for the match to Captain/Major
        /// </summary>
        /// <param name="blockAlso">Also block the match to the newStatus level</param>
        public Match EditMatchPlayers(int matchId, int[] playerIds, string newStatus, bool blockAlso)
        {
            Debug.Assert(newStatus == PlayerMatchStatus.Captain || newStatus == PlayerMatchStatus.Major);
            using (var db = new TtcDbContext())
            {
                var match = db.Matches.Single(x => x.Id == matchId);
                match.Block = blockAlso ? newStatus : null;
                var existingPlayers = db.MatchPlayers
                    .Where(x => x.MatchId == matchId)
                    .Where(x => x.Status == newStatus)
                    .ToArray();
                db.MatchPlayers.RemoveRange(existingPlayers);

                for (int i = 0; i < playerIds.Length; i++)
                {
                    var player = db.Players.Find(playerIds[i]);
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
                db.SaveChanges();
            }
            var newMatch = GetMatch(matchId);
            return newMatch;
        }
        #endregion

        #region Report & Comments
        public Match UpdateReport(MatchReport report)
        {
            using (var dbContext = new TtcDbContext())
            {
                var existingMatch = dbContext.Matches.First(x => x.Id == report.MatchId);
                existingMatch.ReportPlayerId = report.PlayerId;
                existingMatch.Description = report.Text;
                dbContext.SaveChanges();
            }
            var newMatch = GetMatch(report.MatchId);
            return newMatch;
        }

        public Match AddComment(MatchComment comment)
        {
            using (var dbContext = new TtcDbContext())
            {
                var entity = Mapper.Map<MatchCommentEntity>(comment);
                entity.PostedOn = TtcDbContext.GetCurrentBelgianDateTime();
                dbContext.MatchComments.Add(entity);
                dbContext.SaveChanges();
            }
            var newMatch = GetMatch(comment.MatchId);
            return newMatch;
        }

        public Match DeleteComment(int commentId)
        {
            using (var dbContext = new TtcDbContext())
            {
                var comment = dbContext.MatchComments.Single(x => x.Id == commentId);
                dbContext.MatchComments.Remove(comment);
                dbContext.SaveChanges();

                return GetMatch(dbContext, comment.MatchId);
            }
        }
        #endregion

        #region Frenoy Sync
        public OtherMatch FrenoyOtherMatchSync(int matchId)
        {
            using (var dbContext = new TtcDbContext())
            {
                FrenoyMatchSyncCore(dbContext, matchId);
                var match = dbContext.Matches
                    .WithIncludes()
                    .Single(x => x.Id == matchId);
                return Mapper.Map<MatchEntity, OtherMatch>(match);
            }
        }

        public Match FrenoyMatchSync(int matchId)
        {
            using (var dbContext = new TtcDbContext())
            {
                FrenoyMatchSyncCore(dbContext, matchId);
                return GetMatch(dbContext, matchId);
            }
        }

        public void FrenoyTeamSync(int teamId)
        {
            using (var db = new TtcDbContext())
            {
                var team = db.Teams.Single(x => x.Id == teamId);
                var frenoySync = new FrenoyMatchesApi(db, Constants.NormalizeCompetition(team.Competition));
                frenoySync.SyncTeamMatches(team);
            }
        }

        private static void FrenoyMatchSyncCore(TtcDbContext dbContext, int matchId)
        {
            var match = dbContext.Matches
                    .WithIncludes()
                    .Single(x => x.Id == matchId);

            if (match.Date < DateTime.Now && !match.IsSyncedWithFrenoy)
            {
                var frenoySync = new FrenoyMatchesApi(dbContext, match.Competition);
                frenoySync.SyncMatchDetails(match);
            }
        }
        #endregion
        #endregion
    }
}