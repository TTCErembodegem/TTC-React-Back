using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Runtime.CompilerServices;
using AutoMapper;
using Frenoy.Api;
using Ttc.Model.Matches;
using Ttc.DataEntities;

namespace Ttc.DataAccess.Services
{
    public class CalendarService
    {
        public IEnumerable<Match> GetRelevantMatches()
        {
            using (var dbContext = new TtcDbContext())
            {
                var dateBegin = DateTime.Now.AddDays(-8);
                var dateEnd = DateTime.Now.AddDays(8);

                var calendar = dbContext.Kalender
                    .WithIncludes()
                    //.Where(x => x.Id == 1635)
                    .Where(x => x.Datum >= dateBegin)
                    .Where(x => x.Datum <= dateEnd)
                    .Where(x => x.ThuisClubId.HasValue)
                    .OrderBy(x => x.Datum)
                    .ToList();

                foreach (var match in calendar)
                {
                    if (match.Datum < DateTime.Now && (match.Verslag == null || !match.Verslag.IsSyncedWithFrenoy))
                    {
                        var vttl = new FrenoyApi(dbContext, FrenoySettings.VttlSettings);
                        vttl.SyncMatch(match.ThuisClubPloeg.ReeksId.Value, match.ThuisClubPloeg.Code, match.Week.Value);
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