using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Ttc.DataAccess.Services;
using Ttc.DataAccess.Utilities;
using Ttc.DataAccess.Utilities.AutoMapperConfig;
using Ttc.DataEntities.Core;
using Ttc.Model.Core;

namespace Ttc.DataAccess
{
    /// <summary>
    /// TTC Aalst DataAccess configuration
    /// </summary>
    public static class GlobalBackendConfiguration
    {
        public static void Configure(IServiceCollection services, TtcSettings ttcSettings)
        {
            ConfigureDbContext(services, ttcSettings);
            ConfigureAutoMapper(services);
            ConfigureServices(services);
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<ClubService>();
            services.AddScoped<ConfigService>();
            services.AddScoped<MatchService>();
            services.AddScoped<TeamService>();
            services.AddScoped<PlayerService>();
        }

        private static void ConfigureAutoMapper(IServiceCollection services)
        {
            services.AddAutoMapper(typeof(TeamProfile));
        }

        private static void ConfigureDbContext(IServiceCollection services, TtcSettings ttcSettings)
        {
            // Replace with your server version and type.
            // Use 'MariaDbServerVersion' for MariaDB.
            // Alternatively, use 'ServerVersion.AutoDetect(connectionString)'.
            var serverVersion = new MySqlServerVersion(new Version(5, 5, 60));

            services.AddDbContext<ITtcDbContext, TtcDbContext>(
                dbContextOptions => dbContextOptions
                    .UseMySql(ttcSettings.ConnectionString, serverVersion)
                    // The following three options help with debugging, but should
                    // be changed or removed for production.
                    .LogTo(Console.WriteLine, LogLevel.Warning)
                    .EnableSensitiveDataLogging()
                    .EnableDetailedErrors()
            );
        }
    }
}