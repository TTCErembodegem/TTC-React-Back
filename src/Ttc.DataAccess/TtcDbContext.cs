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
        public DbSet<ClubLokaal> ClubLokalen { get; set; }
        public DbSet<ClubContact> ClubContacten { get; set; }

        public DbSet<Reeks> Reeksen { get; set; }
        public DbSet<ClubPloeg> ClubPloegen { get; set; }
        public DbSet<Kalender> Kalender { get; set; }

        // These are used by the legacy website own
        // Let's start with brand new tables... :p
        //public DbSet<Training> Trainingen { get; set; }
        //public DbSet<ClubPloegSpeler> ClubPloegSpelers { get; set; }
        //public DbSet<Verslag> Verslagen { get; set; }
        //public DbSet<VerslagSpeler> SpelersVerslag { get; set; }

        // Used by the legacy website only
        //public DbSet<Klassement> Klassementen { get; set; }

        public TtcDbContext() : base("ttc")
        {
            Database.SetInitializer<TtcDbContext>(new TtcDbInitializer());
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            modelBuilder.Types().Configure(c => c.ToTable(ToLowerCaseTableName(c.ClrType)));

            modelBuilder.Entity<ClubLokaal>()
                .HasRequired(c => c.Club)
                .WithMany(l => l.Lokalen)
                .HasForeignKey(x => x.ClubId);

            modelBuilder.Entity<ClubContact>()
                .HasRequired(c => c.Club)
                .WithMany(c => c.Contacten)
                .HasForeignKey(x => x.ClubId);

            modelBuilder.Entity<ClubPloeg>()
                .HasRequired(c => c.Reeks)
                .WithMany(c => c.Ploegen)
                .HasForeignKey(x => x.ReeksId);
        }

        private static string ToLowerCaseTableName(Type clrType)
        {
            return clrType.Name.ToLowerInvariant();
        }
    }
}
