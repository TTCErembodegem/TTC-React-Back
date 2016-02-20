using System;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using Ttc.DataEntities;
using Ttc.DataEntities.Core;

namespace Ttc.DataAccess
{
    /// <remarks>
    /// Not(yet?) mapped: DbSet Parameter
    /// </remarks>
    [DbConfigurationType(typeof(MySql.Data.Entity.MySqlEFConfiguration))]
    internal class TtcDbContext : DbContext, ITtcDbContext
    {
        public DbSet<Speler> Spelers { get; set; }
        public DbSet<ClubEntity> Clubs { get; set; }
        public DbSet<ClubLokaal> ClubLokalen { get; set; }
        public DbSet<ClubContact> ClubContacten { get; set; }

        public DbSet<Reeks> Reeksen { get; set; }
        public DbSet<ClubPloeg> Opponents { get; set; }
        public DbSet<ClubPloegSpeler> ClubPloegSpelers { get; set; }
        public DbSet<MatchEntity> Kalender { get; set; }
        public DbSet<MatchPlayerEntity> VerslagenSpelers { get; set; }
        public DbSet<MatchGameEntity> VerslagenIndividueel { get; set; }

        public DbSet<Backup.BackupReport> BackupReports { get; set; }
        public DbSet<Backup.BackupTeamPlayer> BackupTeamPlayers { get; set; }

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

            //modelBuilder.Entity<MatchEntity>()
            //    .HasRequired(c => c.Reeks)
            //    .WithMany(c => c.Matchen)
            //    .HasForeignKey(x => x.ReeksId);

            //modelBuilder.Entity<ClubPloegSpeler>()
            //    .HasRequired(c => c.Reeks)
            //    .WithMany(c => c.Spelers)
            //    .HasForeignKey(x => x.ReeksId);

            //modelBuilder.Entity<MatchEntity>()
            //    .HasOptional(x => x.Verslag)
            //    .WithRequired(x => x.MatchEntity);

            modelBuilder.Entity<MatchGameEntity>()
                .HasRequired(c => c.Match)
                .WithMany(c => c.Games)
                .HasForeignKey(x => x.MatchId);

            modelBuilder.Entity<MatchPlayerEntity>()
                .HasRequired(c => c.Match)
                .WithMany(c => c.Players)
                .HasForeignKey(x => x.MatchId);
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
