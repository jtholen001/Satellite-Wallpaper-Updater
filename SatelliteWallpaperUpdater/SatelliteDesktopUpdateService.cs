using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SatelliteWallpaperUpdater.Configuration;
using SatelliteWallpaperUpdater.Interfaces.Repositories;
using SatelliteWallpaperUpdater.Models;
using SatelliteWallpaperUpdater.Repositories;
using System.Diagnostics;

namespace SatelliteWallpaperUpdater
{
    public sealed class SatelliteDesktopUpdateService : BackgroundService
    {
        private readonly ILogger<SatelliteDesktopUpdateService> _logger;
        private readonly IOptions<AppSettings> _appSettings;
        private readonly INESDISRepository _satImageRepo;
        private readonly IEventLogRepository _eventLogRepository;

        public SatelliteDesktopUpdateService(
            ILogger<SatelliteDesktopUpdateService> logger, 
            IOptions<AppSettings> appSettings,
            INESDISRepository satImageRepo,
            IEventLogRepository eventLogRepository)
        {
            _logger = logger;
            _appSettings = appSettings;
            _satImageRepo = satImageRepo;
            _eventLogRepository = eventLogRepository;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    Stopwatch sw = Stopwatch.StartNew();

                    _logger.LogInformation("Updating Desktop background.");

                    await UpdateDesktopBackgroundAsync();

                    _eventLogRepository.WriteToEventLog($"Update complete! Time to update {sw.ElapsedMilliseconds}ms", EventLogEntryType.Information);

                    _logger.LogInformation("Update commplete! Time to update {0}ms", sw.ElapsedMilliseconds);

                    await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                // When the stopping token is canceled, for example, a call made from services.msc,
                // we shouldn't exit with a non-zero exit code. In other words, this is expected...
            }
            catch (Exception ex)
            {
                _eventLogRepository.WriteToEventLog($"Exception occured: {ex.Message}", EventLogEntryType.Error);
                _logger.LogError(ex, "{Message}", ex.Message);

                // Terminates this process and returns an exit code to the operating system.
                // This is required to avoid the 'BackgroundServiceExceptionBehavior', which
                // performs one of two scenarios:
                // 1. When set to "Ignore": will do nothing at all, errors cause zombie services.
                // 2. When set to "StopHost": will cleanly stop the host, and log errors.
                //
                // In order for the Windows Service Management system to leverage configured
                // recovery options, we need to terminate the process with a non-zero exit code.
                Environment.Exit(1);
            }
        }

        private async Task<bool> UpdateDesktopBackgroundAsync()
        {
            List<SatelliteImageMetadata> metadatas = await _satImageRepo.GetLatestImagesMetadataAsync();


            if((metadatas?.Count ?? 0) == 0)
            {
                _eventLogRepository.WriteToEventLog("Couldn't find images, this may be a transient error, or something has changed on the NESDIS site.", EventLogEntryType.Warning);
                return false;
            }
            string pathToSave = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures) + "\\SatelliteImageBackgrounds";

            if (!Directory.Exists(pathToSave))
            {
                Directory.CreateDirectory(pathToSave);
            }

            foreach (string file in Directory.EnumerateFiles(pathToSave))
            {
                File.Delete(file);
            }

            SatelliteImage image = await _satImageRepo.GetImageAsync(
                metadatas.Where(met => met.FullFileName.Contains("5424x5424")).First(),
                pathToSave);

            if(image?.FilePath == null)
            {
                _eventLogRepository.WriteToEventLog("Unable to get image from returned options.", EventLogEntryType.Warning);
                return false;
            }
            WallPaperRepository.SetWallpaper(image.FilePath);

            return true;
        }
    }
}
