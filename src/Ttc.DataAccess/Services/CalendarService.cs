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

                var calendar = dbContext.Kalender
                    .WithIncludes()
                    .Where(x => x.Id == 1552)
                    //.Where(x => x.Datum >= dateBegin)
                    //.Where(x => x.Datum <= dateEnd)
                    //.Where(x => x.ThuisClubId.HasValue)
                    .OrderBy(x => x.Datum)
                    .ToList();

                //var heenmatchen = new List<Kalender>();
                //foreach (var kalender in calendar)
                //{
                //    if ((kalender.Verslag == null || !kalender.Verslag.IsSyncedWithFrenoy) && kalender.Datum.Month > 0 && kalender.Datum.Month < 9)
                //    {
                //        var prevKalender = dbContext.Kalender
                //            .WithIncludes()
                //            .Where(x => x.ThuisClubPloegId.Value == kalender.ThuisClubPloegId.Value)
                //            .Where(x => x.UitClubPloegId.Value == kalender.UitClubPloegId.Value)
                //            .Single(x => x.Datum < kalender.Datum);

                //        heenmatchen.Add(prevKalender);
                //    }
                //}
                //calendar.AddRange(heenmatchen);


                foreach (var kalender in calendar)
                {
                    if (kalender.Datum < DateTime.Now && (kalender.Verslag == null || !kalender.Verslag.IsSyncedWithFrenoy))
                    {
                        var reeks = dbContext.Reeksen.Single(x => x.Id == kalender.ThuisClubPloeg.ReeksId.Value);
                        var frenoySync = new FrenoyApi(dbContext, Constants.NormalizeCompetition(reeks.Competitie));
                        if (Constants.NormalizeCompetition(reeks.Competitie) == Competition.Vttl)
                        {
                            frenoySync.SyncMatch(reeks, kalender.ThuisClubPloeg.Code, kalender.Week.Value);
                        }
                    }
                }

                var result = Mapper.Map<IList<Kalender>, IList<Match>>(calendar);                
                return result;
            }
        }

        public Match GetMatch(int matchId)
        {
            using (var dbContext = new TtcDbContext())
            {
                var calendar = dbContext.Kalender
                    .WithIncludes()
                    .SingleOrDefault(x => x.Id == matchId);
                return Map(calendar);
            }
        }

        public Match ToggleMatchPlayer(MatchPlayer matchPlayer)
        {
            using (var dbContext = new TtcDbContext())
            {
                var existingSpeler = dbContext.VerslagenSpelers
                    .Include(x => x.Verslag)
                    .FirstOrDefault(x => x.MatchId == matchPlayer.MatchId && x.PlayerId == matchPlayer.PlayerId.Value);

                if (existingSpeler != null)
                {
                    dbContext.VerslagenSpelers.Remove(existingSpeler);
                }
                else
                {
                    var existingReport = dbContext.Verslagen.SingleOrDefault(x => x.KalenderId == matchPlayer.MatchId);
                    if (existingReport == null)
                    {
                        dbContext.Verslagen.Add(new Verslag
                        {
                            IsSyncedWithFrenoy = false,
                            KalenderId = matchPlayer.MatchId,
                            SpelerId = matchPlayer.PlayerId.Value,
                            UitslagThuis = 0,
                            UitslagUit = 0
                        });
                    }

                    var verslagSpeler = Mapper.Map<MatchPlayer, VerslagSpeler>(matchPlayer);
                    dbContext.VerslagenSpelers.Add(verslagSpeler);
                }
                dbContext.SaveChanges();
            }
            var newMatch = GetMatch(matchPlayer.MatchId);
            return newMatch;
        }

        private Match Map(Kalender kalender)
        {
            return Mapper.Map<Kalender, Match>(kalender);
        }
    }

    internal static class CalendarExtensions
    {
        public static IQueryable<Kalender> WithIncludes(this DbSet<Kalender> kalender)
        {
            return kalender
                .Include(x => x.ThuisClubPloeg)
                .Include(x => x.Verslag)
                .Include("Verslag.Individueel")
                .Include("Verslag.Spelers");
        }
    }
}