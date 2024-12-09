using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SatelliteWallpaperUpdater.Interfaces.Repositories.Mappers;
using SatelliteWallpaperUpdater.Interfaces.Repositories;
using SatelliteWallpaperUpdater.Repositories.Mappers;
using SatelliteWallpaperUpdater.Repositories;
using SatelliteWallpaperUpdater.Configuration;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace SatelliteWallpaperUpdater
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            IConfiguration config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            var builder = CreateHostBuilder(config).Build().RunAsync();

            Application.Run(new MyApplicationContext());
        }
        static IHostBuilder CreateHostBuilder(IConfiguration config)
        {
            return Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    // Configuration
                    services.Configure<AppSettings>(config.GetSection(nameof(AppSettings)));

                    // Services
                    services.AddHostedService<SatelliteDesktopUpdateService>();

                    // Repositories
                    services.AddTransient<INESDISRepository, NESDISRepository>();
                    services.AddTransient<IEventLogRepository, EventLogRepository>();

                    // Mappers
                    services.AddTransient<IGOES16GeoColorMetadataMapper, GOES16GeoColorMetadataMapper>();
                });
        }
    }
}