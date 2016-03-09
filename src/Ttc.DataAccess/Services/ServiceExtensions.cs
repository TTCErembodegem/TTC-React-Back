using System.Data.Entity;
using System.Linq;
using System.Runtime.CompilerServices;
using Ttc.DataEntities;
using Ttc.Model.Players;

namespace Ttc.DataAccess.Services
{
    internal static class ServiceExtensions
    {
        public static IQueryable<MatchEntity> WithIncludes(this DbSet<MatchEntity> kalender)
        {
            return kalender
                .Include(x => x.HomeTeam)
                .Include(x => x.AwayTeam)
                .Include(x => x.Games)
                //.Include(x => x.Comments) // https://bugs.mysql.com/bug.php?id=76466 ?
                .Include(x => x.Players);
        }
    }
}