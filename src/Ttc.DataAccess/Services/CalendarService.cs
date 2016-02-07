using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
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
                var dateBegin = DateTime.Now.AddDays(-7);
                var dateEnd = DateTime.Now.AddDays(7);

                var calendar = dbContext.Kalender
                    .Include(x => x.ThuisClubPloeg)
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