using System;
using System.Collections.Generic;
using System.Data.Entity;
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
        #region Getters
        public ICollection<Match> GetMatches()
        {
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
                frenoy.SyncLastOpponentMatches(team, opponent);

                var now = DateTime.Now;
                var matchEntities = dbContext.Matches
                    .WithIncludes()
                    .Where(match => (match.AwayClubId == opponent.ClubId && match.AwayTeamCode == opponent.TeamCode) || (match.HomeClubId == opponent.ClubId && match.HomeTeamCode == opponent.TeamCode))
                    .Where(match => match.FrenoyDivisionId == team.FrenoyDivisionId)
                    //.Where(match => match.Date <= now) // Return all matches for easy linking to heenronde/terugronde
                    .ToList();

                // No comments for OpponentMatches

                var result = Mapper.Map<IList<MatchEntity>, IList<OtherMatch>>(matchEntities);
                return result;
            }
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

            return Map(match);
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
        public Match ToggleMatchPlayer(MatchPlayer matchPlayer)
        {
            using (var dbContext = new TtcDbContext())
            {
                var existingSpeler = dbContext.MatchPlayers
                    .Include(x => x.Match)
                    .FirstOrDefault(x => x.MatchId == matchPlayer.MatchId && x.PlayerId == matchPlayer.PlayerId);

                if (existingSpeler != null)
                {
                    if (existingSpeler.Status == matchPlayer.Status)
                    {
                        dbContext.MatchPlayers.Remove(existingSpeler);
                    }
                    else
                    {
                        existingSpeler.Status = matchPlayer.Status;
                    }
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