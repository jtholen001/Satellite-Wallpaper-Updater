using SatelliteWallpaperUpdater.Models;

namespace SatelliteWallpaperUpdater.Interfaces.Repositories.Mappers
{
    public interface IGOES16GeoColorMetadataMapper
    {
        SatelliteImageMetadata MapViaFileName(string fileName);
    }
}
