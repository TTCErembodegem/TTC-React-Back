using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ttc.DataAccess.Entities;
using Ttc.Model;

namespace Ttc.DataAccess
{
    internal class TtcDbContext : DbContext
    {
        public DbSet<Speler> Spelers { get; set; }
        public DbSet<ClubEntity> Clubs { get; set; }

        //public DbSet<Kalender> Kalender { get; set; }
        //public DbSet<ClubLokaal> ClubLokalen { get; set; }
        //public DbSet<ClubPloeg> ClubPloegen { get; set; }
        //public DbSet<ClubPloegSpeler> ClubPloegSpelers { get; set; }
        //public DbSet<Reeks> Reeksen { get; set; }
        //public DbSet<Training> Trainingen { get; set; }
        //public DbSet<Verslag> Verslagen { get; set; }
        //public DbSet<VerslagSpeler> SpelersVerslag { get; set; }

        // Used by the legacy website
        //public DbSet<Ttc.Model.Klassement> Klassementen { get; set; }

        // No longer in use
        //public DbSet<Ttc.Model.WeekSpeler> SpelerVanDeWeek { get; set; }

        public TtcDbContext() : base("ttc")
        {
            Database.SetInitializer<TtcDbContext>(new TtcDbInitializer());
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            modelBuilder.Types().Configure(c => c.ToTable(ToLowerCaseTableName(c.ClrType)));
        }

        private static string ToLowerCaseTableName(Type clrType)
        {
            return clrType.Name.ToLowerInvariant();
        }
    }
}
