using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ttc.DataEntities.Core
{
    public interface ITtcDbContext
    {
        DbSet<PlayerEntity> Players { get; set; }
        DbSet<ClubEntity> Clubs { get; set; }
        DbSet<ClubLokaal> ClubLokalen { get; set; }
        DbSet<ClubContact> ClubContacten { get; set; }

        DbSet<TeamEntity> Teams { get; set; }
        DbSet<TeamOpponentEntity> TeamOpponents { get; set; }
        DbSet<TeamPlayerEntity> TeamPlayers { get; set; }

        DbSet<MatchEntity> Matches { get; set; }
        DbSet<MatchPlayerEntity> MatchPlayers { get; set; }
        DbSet<MatchGameEntity> MatchGames { get; set; }

        DbSet<ParameterEntity> Parameters { get; set; }

        int CurrentSeason { get; }
        int CurrentFrenoySeason { get; }

        Task<int> SaveChangesAsync();
    }
}
