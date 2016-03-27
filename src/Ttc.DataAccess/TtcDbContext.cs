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
        public DbSet<PlayerEntity> Players { get; set; }
        public DbSet<ClubEntity> Clubs { get; set; }
        public DbSet<ClubLokaal> ClubLokalen { get; set; }
        public DbSet<ClubContact> ClubContacten { get; set; }

        public DbSet<TeamEntity> Teams { get; set; }
        public DbSet<TeamOpponentEntity> TeamOpponents { get; set; }
        public DbSet<TeamPlayerEntity> TeamPlayers { get; set; }
        public DbSet<MatchEntity> Matches { get; set; }
        public DbSet<MatchPlayerEntity> MatchPlayers { get; set; }
        public DbSet<MatchGameEntity> MatchGames { get; set; }
        public DbSet<MatchCommentEntity> MatchComments { get; set; }

        public DbSet<Backup.BackupReport> BackupReports { get; set; }
        public DbSet<Backup.BackupTeamPlayer> BackupTeamPlayers { get; set; }

        /// <summary>
        /// Get the same time as the Frenoy Api
        /// </summary>
        public static DateTime GetCurrentBelgianDateTime()
        {
            DateTime belgianTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "Romance Standard Time");
            return belgianTime;
        }

        /// <summary>
        /// Get current year season (not frenoy season!)
        /// </summary>
        public static int CurrentYear
        {
            get
            {
                if (DateTime.Now.Month < 9) return DateTime.Now.Year -1;
                return DateTime.Now.Year;
            }
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("");

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
        }

        // This is undoubtedly the most ugly c# I've ever written
#if DEBUG
        public TtcDbContext() : base("ttc")
#else
        public TtcDbContext() : base("Ttc.DataAccess.TtcDbContext")
#endif
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
