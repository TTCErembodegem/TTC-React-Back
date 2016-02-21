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
        public IEnumerable<Match> GetRelevantMatches()
        {
            // TODO: kalender gaat toch niet de hoofdpagina worden
            // hoofdpagina = jouw volgende matchen. jouw team. en jouw speler details

            using (var dbContext = new TtcDbContext())
            {
                var dateBegin = DateTime.Now.AddDays(-20);
                var dateEnd = DateTime.Now.AddDays(20);

                var calendar = dbContext.Matches
                    .WithIncludes()
                    //.Where(x => x.Id == 485)
                    //.Where(x => x.Id == 467) // Sporta A vs Kruibeke B
                    //.Where(x => x.Id == 563) // Derby: Sporta A vs B
                    //.Where(x => x.Id == 484) // St-Pauwels B vs Sporta B
                    .Where(x => x.HomeClubId == Constants.OwnClubId || x.AwayClubId == Constants.OwnClubId)
                    .Where(x => x.Date >= dateBegin)
                    .Where(x => x.Date <= dateEnd)
                    .OrderBy(x => x.Date)
                    .ToList();

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
                        var frenoySync = new FrenoyApi(dbContext, Constants.NormalizeCompetition(team.Competition));
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
                var calendar = dbContext.Matches
                    .WithIncludes()
                    .SingleOrDefault(x => x.Id == matchId);
                return Map(calendar);
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

        public IEnumerable<OtherMatch> GetLastOpponentMatches(int teamId, OpposingTeam opponent)
        {
            using (var dbContext = new TtcDbContext())
            {
                var team = dbContext.Teams.Single(x => x.Id == teamId);

                var frenoy = new FrenoyApi(dbContext, Constants.NormalizeCompetition(team.Competition));
                frenoy.SyncMatches(team, opponent);

                var calendar = dbContext.Matches
                    .WithIncludes()
                    .Where(kal => (kal.AwayClubId == opponent.ClubId && kal.AwayPloegCode == opponent.TeamCode) || (kal.HomeClubId == opponent.ClubId && kal.HomeTeamCode == opponent.TeamCode))
                    .Where(kal => kal.Date < DateTime.Now)
                    .OrderByDescending(kal => kal.Date)
                    .Take(5)
                    .ToList();

                var result = Mapper.Map<IList<MatchEntity>, IList<OtherMatch>>(calendar);
                return result;
            }
        }
    }
}