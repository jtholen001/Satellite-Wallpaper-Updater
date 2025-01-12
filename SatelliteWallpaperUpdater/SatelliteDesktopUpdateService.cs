using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SatelliteWallpaperUpdater.Configuration;
using SatelliteWallpaperUpdater.Helpers;
using SatelliteWallpaperUpdater.Interfaces.Repositories;
using SatelliteWallpaperUpdater.Models;
using SatelliteWallpaperUpdater.Repositories;
using System.Diagnostics;

namespace SatelliteWallpaperUpdater
{
    public class SatelliteDesktopUpdateService(
        ILogger<SatelliteDesktopUpdateService> logger,
        IOptions<AppSettings> appSettings,
        INESDISRepository satImageRepo,
        IEventLogRepository eventLogRepository)
    {
        private readonly ILogger<SatelliteDesktopUpdateService> _logger = logger;
        private readonly IOptions<AppSettings> _appSettings = appSettings;
        private readonly INESDISRepository _satImageRepo = satImageRepo;
        private readonly IEventLogRepository _eventLogRepository = eventLogRepository;
        public event EventHandler<BackgroundUpdatedEventArgs> BackgroundUpdated;

        public async Task UpdateBackgroundAsync()
        {
            try
            {
                Stopwatch sw = Stopwatch.StartNew();

                _logger.LogInformation("Updating Desktop background.");

                SatelliteImageMetadata? result = await UpdateDesktopBackgroundAsync();
                DateTime updatedDateTimeUtc = DateTime.UtcNow;

                _eventLogRepository.WriteToEventLog($"Update complete! Time to update {sw.ElapsedMilliseconds}ms", EventLogEntryType.Information);

                _logger.LogInformation("Update commplete! Time to update {0}ms", sw.ElapsedMilliseconds);

                BackgroundUpdatedEventArgs args = new BackgroundUpdatedEventArgs()
                {
                    Exception = null,
                    SatelliteImageMetadata = result,
                    UpdatedDateTimeUtc = updatedDateTimeUtc
                };

                OnBackgroundUpdated(args);
            }
            catch (Exception ex)
            {
                _eventLogRepository.WriteToEventLog($"Exception occured: {ex.Message}", EventLogEntryType.Error);
                _logger.LogError(ex, "{Message}", ex.Message);
            }
        }

        protected virtual void OnBackgroundUpdated(BackgroundUpdatedEventArgs e)
        {
            BackgroundUpdated?.Invoke(this, e);
        }

        private async Task<SatelliteImageMetadata?> UpdateDesktopBackgroundAsync()
        {
            List<SatelliteImageMetadata> metadatas = await _satImageRepo.GetLatestImagesMetadataAsync();


            if ((metadatas?.Count ?? 0) == 0)
            {
                _eventLogRepository.WriteToEventLog("Couldn't find images, this may be a transient error, or something has changed on the NESDIS site.", EventLogEntryType.Warning);
                return null;
            }
            string pathToSave = FileHelper.GetSaveDirectory();

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

            if (image?.FilePath == null)
            {
                _eventLogRepository.WriteToEventLog("Unable to get image from returned options.", EventLogEntryType.Warning);
                return null;
            }

            WallPaperRepository.SetWallpaper(image.FilePath, appSettings.Value.ApplicationName);

            return image.Metadata;
        }
    }

    public class BackgroundUpdatedEventArgs : EventArgs
    {
        public SatelliteImageMetadata? SatelliteImageMetadata { get; set; }
        public DateTime UpdatedDateTimeUtc { get; set; }
        public Exception? Exception { get; set; }
    }
}
