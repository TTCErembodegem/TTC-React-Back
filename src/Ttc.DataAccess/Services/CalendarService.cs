using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Runtime.CompilerServices;
using AutoMapper;
using Frenoy.Api;
using Ttc.Model.Matches;
using Ttc.DataEntities;
using Ttc.Model.Players;
using Ttc.Model.Teams;

namespace Ttc.DataAccess.Services
{
    public class CalendarService
    {
        public IEnumerable<Match> GetRelevantMatches()
        {
            using (var dbContext = new TtcDbContext())
            {
                var dateBegin = DateTime.Now.AddDays(-20);
                var dateEnd = DateTime.Now.AddDays(20);

                var calendar = dbContext.Matches
                    .WithIncludes()
                    .Where(x => x.Id == 467) // Sporta A vs Kruibeke B -- MatchOutcome is verkeerd
                    //.Where(x => x.Id == 563) // Derby: Sporta A vs B
                    //.Where(x => x.Id == 484) // St-Pauwels B vs Sporta B
                    .Where(x => x.Date >= dateBegin)
                    .Where(x => x.Date <= dateEnd)
                    .OrderBy(x => x.Date)
                    .ToList();

                // TODO: kalender gaat toch niet de hoofdpagina worden
                // hoofdpagina = jouw volgende matchen. jouw team. en jouw speler details
                // Zeker deze data met extra AJAX call...
                //var heenmatchen = new List<Kalender>();
                //foreach (var kalender in calendar)
                //{
                //    if ((kalender.Verslag == null || !kalender.Verslag.IsSyncedWithFrenoy) && kalender.Datum.Month > 0 && kalender.Datum.Month < 9)
                //    {
                //        var prevKalender = dbContext.Kalender
                //            .WithIncludes()
                //            .Where(x => x.ThuisPloeg == kalender.ThuisPloeg)
                //            .Where(x => x.UitClubPloegId.Value == kalender.UitClubPloegId.Value)
                //            .SingleOrDefault(x => x.Datum < kalender.Datum);

                //        if (prevKalender != null)
                //            heenmatchen.Add(prevKalender);
                //    }
                //}
                //calendar.AddRange(heenmatchen);


                foreach (var kalender in calendar)
                {
                    if (kalender.Date < DateTime.Now && !kalender.IsSyncedWithFrenoy)
                    {
                        var team = dbContext.Teams.Single(x => x.Id == kalender.HomeTeamId || x.Id == kalender.AwayTeamId);
                        var frenoySync = new FrenoyApi(dbContext, Constants.NormalizeCompetition(team.Competition));
                        //frenoySync.SyncMatch(reeks, kalender.Team.TeamCode, kalender.Week); // TODO: 1 parameter = frenoyMatchId
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
                    .FirstOrDefault(x => x.MatchId == matchPlayer.MatchId && x.PlayerId == matchPlayer.PlayerId.Value);

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

        public IEnumerable<Match> GetLastOpponentMatches(OpposingTeam opponent)
        {
            using (var dbContext = new TtcDbContext())
            {
                var calendar = dbContext.Matches
                    .WithIncludes()
                    .Where(kal => kal.AwayClubId == opponent.ClubId && kal.AwayPloegCode == opponent.TeamCode)
                    // TODO: krijgen nu de laatste uitslagen tegen erembodegem
                    // maar moeten de laatste matchen van die ploeg hebben
                    // mss match met HomeClubId <> 1 beginnen opslaan?
                    //.Where(kal => kal.UitClubId == opponent.ClubId && kal.UitPloeg == opponent.TeamCode)
                    // TODO: base klasse: automatisch ophalen door frenoy if nog geen uitslagen 
                    // en dbContext met de simpleinjecter en automapper
                    // frenoy sync in dit geval: die ploeg toevoegen in Reeks en alle matchen syncen...
                    .Where(kal => kal.Date < DateTime.Now)
                    .OrderByDescending(kal => kal.Date)
                    .Take(5)
                    .ToList();

                var result = Mapper.Map<IList<MatchEntity>, IList<Match>>(calendar);
                return result;
            }
        }
    }

    //internal enum KalenderFilter
    //{
    //    OwnMatches,
    //    AllMatches
    //}

    internal static class CalendarExtensions
    {
        public static IQueryable<MatchEntity> WithIncludes(this DbSet<MatchEntity> kalender)
        {
            return kalender
                .Include(x => x.HomeTeam)
                .Include(x => x.AwayTeam)
                .Include(x => x.Games)
                .Include(x => x.Players);
        }
    }
}