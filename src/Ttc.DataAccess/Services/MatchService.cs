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
    public class MatchService
    {
        #region Getters
        public ICollection<Match> GetRelevantMatches()
        {
            // TODO: hoofdpagina = jouw volgende matchen. jouw team. en jouw speler details
            using (var dbContext = new TtcDbContext())
            {
                var dateBegin = DateTime.Now.AddDays(-10);
                var dateEnd = DateTime.Now.AddDays(20);

                var matchEntities = dbContext.Matches
                    .WithIncludes()
                    //.Where(x => x.Id == 802)
                    .Where(x => x.HomeClubId == Constants.OwnClubId || x.AwayClubId == Constants.OwnClubId)
                    .Where(x => x.Date >= dateBegin)
                    .Where(x => x.Date <= dateEnd)
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
                    .Where(kal => (kal.AwayClubId == opponent.ClubId && kal.AwayTeamCode == opponent.TeamCode) || (kal.HomeClubId == opponent.ClubId && kal.HomeTeamCode == opponent.TeamCode))
                    .Where(kal => kal.Date <= now)
                    //.Take(5)
                    .ToList();

                // No comments for OpponentMatches

                var result = Mapper.Map<IList<MatchEntity>, IList<OtherMatch>>(matchEntities);
                return result;
            }
        }

        public Match GetFirstRoundMatch(int matchId)
        {
            using (var dbContext = new TtcDbContext())
            {
                var matchEntities = dbContext.Matches.Single(x => x.Id == matchId);
                if (!matchEntities.IsHomeMatch.HasValue)
                {
                    return null;
                }

                MatchEntity firstRoundMatch;
                if (matchEntities.IsHomeMatch.Value)
                {
                    firstRoundMatch = dbContext.Matches
                        .WithIncludes()
                        .Where(x => x.HomeTeamCode == matchEntities.AwayTeamCode)
                        .Where(x => x.HomeClubId == matchEntities.AwayClubId)
                        .Where(x => x.AwayTeamId == matchEntities.HomeTeamId)
                        .SingleOrDefault(x => x.Date < matchEntities.Date);
                }
                else
                {
                    firstRoundMatch = dbContext.Matches
                        .WithIncludes()
                        .Where(x => x.AwayTeamCode == matchEntities.HomeTeamCode)
                        .Where(x => x.AwayClubId == matchEntities.HomeClubId)
                        .Where(x => x.HomeTeamId == matchEntities.AwayTeamId)
                        .SingleOrDefault(x => x.Date < matchEntities.Date);
                }

                // TODO: comments not fetched here

                return Map(firstRoundMatch);
            }
        }
        #endregion

        #region Boilderplate code
        private Match GetMatch(TtcDbContext dbContext, int matchId)
        {
            var match = dbContext.Matches
                    .WithIncludes()
                    .Single(x => x.Id == matchId);

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
        public Match ToggleMatchPlayer(MatchPlayer matchPlayer)
        {
            using (var dbContext = new TtcDbContext())
            {
                var existingSpeler = dbContext.MatchPlayers
                    .Include(x => x.Match)
                    .FirstOrDefault(x => x.MatchId == matchPlayer.MatchId && x.PlayerId == matchPlayer.PlayerId);

                if (existingSpeler != null)
                {
                    dbContext.MatchPlayers.Remove(existingSpeler);
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

        public Match UpdateReport(MatchReport report, bool isMainReport = true)
        {
            using (var dbContext = new TtcDbContext())
            {
                if (isMainReport)
                {
                    var existingMatch = dbContext.Matches.First(x => x.Id == report.MatchId);
                    existingMatch.ReportPlayerId = report.PlayerId;
                    existingMatch.Description = report.Text;
                }
                else
                {
                    dbContext.MatchComments.Add(new MatchCommentEntity
                    {
                        PostedOn = TtcDbContext.GetCurrentBelgianDateTime(),
                        PlayerId = report.PlayerId,
                        MatchId = report.MatchId,
                        Text = report.Text
                    });
                }
                
                dbContext.SaveChanges();
            }
            var newMatch = GetMatch(report.MatchId);
            return newMatch;
        }

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

        public Match FrenoyMatchSync(int matchId)
        {
            using (var dbContext = new TtcDbContext())
            {
                FrenoyMatchSyncCore(dbContext, matchId);
                return GetMatch(dbContext, matchId);
            }
        }

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
        #endregion
    }
}