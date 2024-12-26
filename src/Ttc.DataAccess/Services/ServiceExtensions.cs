using Microsoft.EntityFrameworkCore;
using Ttc.DataEntities;

namespace Ttc.DataAccess.Services;

internal static class ServiceExtensions
{
    public static IQueryable<MatchEntity> WithIncludes(this DbSet<MatchEntity> matches)
    {
        return matches
            //.Include(x => x.HomeTeam)
            //.Include(x => x.AwayTeam)
            .Include(x => x.Games)
            //.Include(x => x.Comments) // https://bugs.mysql.com/bug.php?id=76466 ?
            .Include(x => x.Players);
    }
}
