using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SatelliteWallpaperUpdater.Configuration;
using SatelliteWallpaperUpdater.Helpers;
using SatelliteWallpaperUpdater.Interfaces.Repositories;
using SatelliteWallpaperUpdater.Models;
using System.Diagnostics;

namespace SatelliteWallpaperUpdater
{
    public class SatelliteDesktopUpdateService(
        ILogger<SatelliteDesktopUpdateService> logger,
        INESDISRepository satImageRepo,
        IEventLogRepository eventLogRepository,
        IWallpaperRepository wallpaperRepository)
    {
        private readonly ILogger<SatelliteDesktopUpdateService> _logger = logger;
        private readonly INESDISRepository _satImageRepo = satImageRepo;
        private readonly IEventLogRepository _eventLogRepository = eventLogRepository;
        private readonly IWallpaperRepository _wallpaperRepository = wallpaperRepository;
        public event EventHandler<BackgroundUpdatedEventArgs> BackgroundUpdated;

        public async Task UpdateBackgroundAsync(SatelliteImage? currentImage)
        {
            try
            {
                Stopwatch sw = Stopwatch.StartNew();

                _logger.LogInformation("Updating Desktop background.");

                SatelliteImage? result = await UpdateDesktopBackgroundAsync(currentImage);
                DateTime updatedDateTimeUtc = DateTime.UtcNow;

                _eventLogRepository.WriteToEventLog($"Update complete! Time to update {sw.ElapsedMilliseconds}ms", EventLogEntryType.Information);

                _logger.LogInformation("Update commplete! Time to update {0}ms", sw.ElapsedMilliseconds);

                BackgroundUpdatedEventArgs args = new BackgroundUpdatedEventArgs()
                {
                    Exception = null,
                    SatelliteImage = result,
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

        private async Task<SatelliteImage?> UpdateDesktopBackgroundAsync(SatelliteImage? currentImage)
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

            AttemptToDeletePastImages(currentImage);

            SatelliteImage image = await _satImageRepo.GetImageAsync(
                metadatas.Where(met => met.FullFileName.Contains("5424x5424")).First(),
                pathToSave);

            if (image?.FilePath == null)
            {
                _eventLogRepository.WriteToEventLog("Unable to get image from returned options.", EventLogEntryType.Warning);
                return null;
            }

            _wallpaperRepository.SetWallpaper(image.FilePath);
            
            Properties.Settings.Default.LastImagePath = image.FilePath;
            Properties.Settings.Default.Save();

            return image;
        }

        private bool AttemptToDeletePastImages(SatelliteImage? currentImage)
        {
            if (currentImage?.FilePath != null && File.Exists(currentImage.FilePath))
            {
                File.Delete(currentImage.FilePath);
                return true;
            }

            if(!string.IsNullOrEmpty(Properties.Settings.Default.LastImagePath))
            {
                File.Delete(Properties.Settings.Default.LastImagePath);
                return true;
            }

            return false;
        }
    }

    public class BackgroundUpdatedEventArgs : EventArgs
    {
        public SatelliteImage? SatelliteImage { get; set; }
        public DateTime UpdatedDateTimeUtc { get; set; }
        public Exception? Exception { get; set; }
    }
}
