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
        DbSet<ClubPloeg> ClubPloegen { get; set; }
        DbSet<ClubPloegSpeler> ClubPloegSpelers { get; set; }
        DbSet<Kalender> Kalender { get; set; }
        DbSet<Verslag> Verslagen { get; set; }
        DbSet<VerslagSpeler> VerslagenSpelers { get; set; }
        DbSet<VerslagIndividueel> VerslagenIndividueel { get; set; }

        int SaveChanges(); // TODO: remove this dependency
    }
}
