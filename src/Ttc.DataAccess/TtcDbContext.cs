using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Ttc.DataAccess.Legacy;
using Ttc.DataEntities;
using Ttc.DataEntities.Core;

namespace Ttc.DataAccess;

internal class TtcDbContext : DbContext, ITtcDbContext
{
    public DbSet<PlayerEntity> Players { get; set; }
    public DbSet<PlayerPasswordResetEntity> PlayerPasswordResets { get; set; }

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

    public DbSet<ParameterEntity> Parameters { get; set; }

    /// <summary>
    /// The year of the current season.
    /// For season 2019-2020 this is 2019.
    /// </summary>
    public int CurrentSeason
    {
        get
        {
            var year = Parameters.Single(x => x.Sleutel == "year").Value;
            return int.Parse(year);
        }
    }

    public int CurrentFrenoySeason => CurrentSeason - 2000 + 1;

    public DbSet<BackupReport> BackupReports { get; set; }
    public DbSet<BackupTeamPlayer> BackupTeamPlayers { get; set; }

    public TtcDbContext(DbContextOptions<TtcDbContext> options) : base(options)
    {
        
    }

    /// <summary>
    /// Get the same time as the Frenoy Api
    /// </summary>
    public static DateTime GetCurrentBelgianDateTime()
    {
        DateTime belgianTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "Romance Standard Time");
        return belgianTime;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ClubLokaal>()
            .HasOne(c => c.Club)
            .WithMany(l => l.Lokalen)
            .HasForeignKey(x => x.ClubId)
            .IsRequired();

        modelBuilder.Entity<ClubContact>()
            .HasOne(c => c.Club)
            .WithMany(c => c.Contacten)
            .HasForeignKey(x => x.ClubId)
            .IsRequired();
    }
}

/// <summary>
/// For EF Migrations
/// </summary>
internal class TtcDbContextFactory : IDesignTimeDbContextFactory<TtcDbContext>
{
    public TtcDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptions<TtcDbContext>();
        
        return new TtcDbContext(options);
    }
}
