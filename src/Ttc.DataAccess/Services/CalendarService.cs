using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Runtime.CompilerServices;
using AutoMapper;
using Ttc.DataAccess.Entities;
using Ttc.Model.Matches;

namespace Ttc.DataAccess.Services
{
    public class CalendarService
    {
        public IEnumerable<Match> GetRelevantCalendarItems()
        {
            using (var dbContext = new TtcDbContext())
            {
                var dateBegin = DateTime.Now.AddDays(-8);
                var dateEnd = DateTime.Now.AddDays(8);

                var calendar = dbContext.Kalender
                    .Include(x => x.ThuisClubPloeg)
                    .Include(x => x.Verslag)
                    .Include("Verslag.Individueel")
                    .Include("Verslag.Spelers")
                    .Where(x => x.Datum >= dateBegin)
                    .Where(x => x.Datum <= dateEnd)
                    .Where(x => x.ThuisClubId.HasValue)
                    .OrderBy(x => x.Datum)
                    .ToList();

                var result = Mapper.Map<IList<Kalender>, IList<Match>>(calendar);
                return result;
            }
        }
    }
}