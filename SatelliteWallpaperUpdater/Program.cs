using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SatelliteWallpaperUpdater.Interfaces.Repositories.Mappers;
using SatelliteWallpaperUpdater.Interfaces.Repositories;
using SatelliteWallpaperUpdater.Repositories.Mappers;
using SatelliteWallpaperUpdater.Repositories;
using SatelliteWallpaperUpdater.Configuration;
using Microsoft.Extensions.Configuration;

namespace SatelliteWallpaperUpdater
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        static void Main()
        {
            IConfiguration config = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            var host = CreateHostBuilder(config).Build();
            Application.Run(host.Services.GetRequiredService<WallpaperAppContext>());
        }
        static IHostBuilder CreateHostBuilder(IConfiguration config)
        {
            return Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    // Configuration
                    services.Configure<AppSettings>(config.GetSection(nameof(AppSettings)));

                    // Services
                    services.AddSingleton<SatelliteDesktopUpdateService>();
                    services.AddTransient<WallpaperAppContext>();

                    // Repositories
                    services.AddTransient<INESDISRepository, NESDISRepository>();
                    services.AddTransient<IEventLogRepository, EventLogRepository>();
                    services.AddTransient<IWallpaperRepository, WallPaperRepository>();

                    // Mappers
                    services.AddTransient<IGOES16GeoColorMetadataMapper, GOES16GeoColorMetadataMapper>();
                });
        }
    }
}