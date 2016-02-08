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
    /// <remarks>
    /// Not(yet?) mapped: DbSet Parameter
    /// Entities used by the legacy website only:
    /// (The entity classes have been excluded in the VS project)
    /// public DbSet Training Trainingen { get; set; }
    /// public DbSet Klassement Klassementen { get; set; }
    /// </remarks>
    [DbConfigurationType(typeof(MySql.Data.Entity.MySqlEFConfiguration))]
    internal class TtcDbContext : DbContext
    {
        public DbSet<Speler> Spelers { get; set; }
        public DbSet<ClubEntity> Clubs { get; set; }
        public DbSet<ClubLokaal> ClubLokalen { get; set; }
        public DbSet<ClubContact> ClubContacten { get; set; }

        public DbSet<Reeks> Reeksen { get; set; }
        public DbSet<ClubPloeg> ClubPloegen { get; set; }
        public DbSet<ClubPloegSpeler> ClubPloegSpelers { get; set; }
        public DbSet<Kalender> Kalender { get; set; }
        public DbSet<Verslag> Verslagen { get; set; }
        public DbSet<VerslagSpeler> VerslagenSpelers { get; set; }
        public DbSet<VerslagIndividueel> VerslagenIndividueel { get; set; }

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

            modelBuilder.Entity<Kalender>()
                .HasRequired(c => c.ThuisClubPloeg)
                .WithMany(c => c.Matchen)
                .HasForeignKey(x => x.ThuisClubPloegId);

            modelBuilder.Entity<ClubPloegSpeler>()
                .HasRequired(c => c.Ploeg)
                .WithMany(c => c.Spelers)
                .HasForeignKey(x => x.ClubPloegId);

            modelBuilder.Entity<Kalender>()
                .HasOptional(x => x.Verslag)
                .WithRequired(x => x.Kalender);

            //modelBuilder.Entity<VerslagIndividueel>()
            //    .HasRequired(c => c.Verslag)
            //    .WithMany(c => c.Individueel)
            //    .HasForeignKey(x => x.VerslagId);

            //modelBuilder.Entity<VerslagSpeler>()
            //    .HasRequired(c => c.Verslag)
            //    .WithMany(c => c.Spelers)
            //    .HasForeignKey(x => x.VerslagId);
        }

        public TtcDbContext() : base("ttc")
        {
            //Configuration.ValidateOnSaveEnabled = false;
            Database.SetInitializer<TtcDbContext>(new CreateDatabaseIfNotExists<TtcDbContext>());
        }

        static TtcDbContext()
        {
            DbConfiguration.SetConfiguration(new MySql.Data.Entity.MySqlEFConfiguration());
        }

        private static string ToLowerCaseTableName(Type clrType) => clrType.Name.ToLowerInvariant();
    }
}
