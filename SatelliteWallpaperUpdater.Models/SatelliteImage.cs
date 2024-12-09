using System.Drawing;

namespace SatelliteWallpaperUpdater.Models
{
    /// <summary>
    /// Class to store some
    /// </summary>
    public class SatelliteImage
    {
        /// <summary>
        /// Metadata for the image.
        /// </summary>
        public SatelliteImageMetadata Metadata { get; }

        /// <summary>
        /// Image file to support resolution handling
        /// </summary>
        public Image Image { get; set; }

        /// <summary>
        /// Path to the image file.
        /// </summary>
        public string? FilePath { get; set; }

        /// <summary>
        /// Creates a SatelliteImage with the given parameters. 
        /// </summary>
        /// <exception cref="ArgumentException">If the given path does not exist, or if the specified file is not an image file</exception>
        public SatelliteImage(string? path, DateTime observationDate, string? satelliteName, string? instrument, string? sectorType, string? satelliteImageType)
        {
            if (!Path.Exists(path))
            {
                throw new FileNotFoundException($"No file was found at: {path}");
            }

            FilePath = path;
            Image = Image.FromFile(path);
            Metadata = new SatelliteImageMetadata(observationDate, Path.GetFileName(path), satelliteName, instrument, sectorType, satelliteImageType);
        }

        public SatelliteImage(string? path, DateTime observationDate) : this(path, observationDate, null, null, null, null) { }

        /// <summary>
        /// Creates a SatelliteImage with the given parameters. 
        /// </summary>
        /// <exception cref="ArgumentException">If the given path does not exist, or if the specified file is not an image file</exception>
        public SatelliteImage(SatelliteImageMetadata metadata, string filePath)
        {
            if (!Path.Exists(filePath))
            {
                throw new FileNotFoundException($"No file was found at: {filePath}");
            }

            FilePath = filePath;
            Image = Image.FromFile(filePath);
            Metadata = metadata;
        }
    }
}
