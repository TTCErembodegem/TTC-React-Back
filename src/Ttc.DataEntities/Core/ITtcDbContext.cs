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
        DbSet<Speler> Spelers { get; set; }
        DbSet<ClubEntity> Clubs { get; set; }
        DbSet<ClubLokaal> ClubLokalen { get; set; }
        DbSet<ClubContact> ClubContacten { get; set; }
        DbSet<Reeks> Reeksen { get; set; }
        DbSet<ClubPloeg> Opponents { get; set; }
        DbSet<ClubPloegSpeler> ClubPloegSpelers { get; set; }
        DbSet<MatchEntity> Kalender { get; set; }
        DbSet<MatchPlayerEntity> VerslagenSpelers { get; set; }
        DbSet<MatchGameEntity> VerslagenIndividueel { get; set; }

        int SaveChanges(); // TODO: remove this dependency
    }
}
