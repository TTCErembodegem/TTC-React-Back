using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using AutoMapper;
using Frenoy.Api;
using Ttc.DataEntities;
using Ttc.Model.Matches;
using Ttc.Model.Teams;

namespace Ttc.DataAccess.Services
{
    public class MatchService
    {
        public ICollection<Match> GetRelevantMatches()
        {
            // TODO: kalender gaat toch niet de hoofdpagina worden
            // hoofdpagina = jouw volgende matchen. jouw team. en jouw speler details

            using (var dbContext = new TtcDbContext())
            {
                var dateBegin = DateTime.Now.AddDays(-10);
                var dateEnd = DateTime.Now.AddDays(10);

                var calendar = dbContext.Matches
                    .WithIncludes()
                    //.Where(x => x.Id == 802)
                    //.Where(x => x.Id == 467) // Sporta A vs Kruibeke B
                    //.Where(x => x.Id == 563) // Derby: Sporta A vs B
                    //.Where(x => x.Id == 484) // St-Pauwels B vs Sporta B
                    .Where(x => x.HomeClubId == Constants.OwnClubId || x.AwayClubId == Constants.OwnClubId)
                    .Where(x => x.Date >= dateBegin)
                    .Where(x => x.Date <= dateEnd)
                    .ToList();

                var matchIds = calendar.Select(x => x.Id).ToArray();
                var comments = dbContext.MatchComments.Where(x => matchIds.Contains(x.MatchId)).ToArray();
                foreach (var match in calendar)
                {
                    match.Comments = comments.Where(x => x.MatchId == match.Id).ToArray();
                }

                // TODO: Al deze dingen, doe nieuwe requests, gebruiker niet laten wachten...
                var heenmatchen = new List<MatchEntity>();
                foreach (var kalender in calendar)
                {
                    if (kalender.IsHomeMatch.HasValue && !kalender.IsSyncedWithFrenoy && kalender.Date.Month < 9)
                    {
                        MatchEntity prevKalender;
                        if (kalender.IsHomeMatch.Value)
                        {
                            prevKalender = dbContext.Matches
                                .WithIncludes()
                                .Where(x => x.HomeTeamCode == kalender.AwayPloegCode)
                                .Where(x => x.HomeClubId == kalender.AwayClubId)
                                .Where(x => x.AwayTeamId == kalender.HomeTeamId)
                                .SingleOrDefault(x => x.Date < kalender.Date);
                        }
                        else
                        {
                            prevKalender = dbContext.Matches
                                .WithIncludes()
                                .Where(x => x.AwayPloegCode == kalender.HomeTeamCode)
                                .Where(x => x.AwayClubId == kalender.HomeClubId)
                                .Where(x => x.HomeTeamId == kalender.AwayTeamId)
                                .SingleOrDefault(x => x.Date < kalender.Date);
                        }

                        if (prevKalender != null)
                            heenmatchen.Add(prevKalender);
                    }
                }
                calendar.AddRange(heenmatchen);


                foreach (var kalender in calendar)
                {
                    if (kalender.Date < DateTime.Now && !kalender.IsSyncedWithFrenoy)
                    {
                        var team = dbContext.Teams.Single(x => x.Id == kalender.HomeTeamId || x.Id == kalender.AwayTeamId);
                        var frenoySync = new FrenoyMatchesApi(dbContext, Constants.NormalizeCompetition(team.Competition));
                        frenoySync.SyncMatch(team.Id, kalender.FrenoyMatchId);
                    }
                }

                var result = Mapper.Map<IList<MatchEntity>, IList<Match>>(calendar);                
                return result;
            }
        }

        public Match GetMatch(int matchId)
        {
            using (var dbContext = new TtcDbContext())
            {
                var match = dbContext.Matches
                    .WithIncludes()
                    .Single(x => x.Id == matchId);

                var comments = dbContext.MatchComments.Where(x => x.MatchId == matchId).ToArray();
                match.Comments = comments;

                return Map(match);
            }
        }

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

        private Match Map(MatchEntity matchEntity)
        {
            return Mapper.Map<MatchEntity, Match>(matchEntity);
        }

        public ICollection<OtherMatch> GetLastOpponentMatches(int teamId, OpposingTeam opponent)
        {
            using (var dbContext = new TtcDbContext())
            {
                var team = dbContext.Teams.Single(x => x.Id == teamId);

                var frenoy = new FrenoyMatchesApi(dbContext, Constants.NormalizeCompetition(team.Competition));
                frenoy.SyncMatches(team, opponent);

                var calendar = dbContext.Matches
                    .WithIncludes()
                    .Where(kal => (kal.AwayClubId == opponent.ClubId && kal.AwayPloegCode == opponent.TeamCode) || (kal.HomeClubId == opponent.ClubId && kal.HomeTeamCode == opponent.TeamCode))
                    .Where(kal => kal.Date < DateTime.Now)
                    //.OrderByDescending(kal => kal.Date)
                    //.Take(5)
                    .ToList();

                var result = Mapper.Map<IList<MatchEntity>, IList<OtherMatch>>(calendar);
                return result;
            }
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
    }
}