using SatelliteWallpaperUpdater.Models;

namespace SatelliteWallpaperUpdater.Interfaces.Repositories
{
    public interface INESDISRepository
    {
        public Task<string> GetAvailableImagesAsync();
        public Task<List<SatelliteImageMetadata>> GetLatestImagesMetadataAsync();
        public Task<SatelliteImage> GetImageAsync(SatelliteImageMetadata metadata, string pathToSaveFile);
    }
}
