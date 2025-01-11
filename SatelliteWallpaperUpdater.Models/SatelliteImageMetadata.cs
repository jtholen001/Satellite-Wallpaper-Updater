namespace SatelliteWallpaperUpdater.Models
{
    public class SatelliteImageMetadata
    {
        /// <summary>
        /// Full file name including the extension.
        /// </summary>
        public string FullFileName { get; }

        public DateTime CreationDateTimeUtc { get; }

        /// <summary>
        /// Time the observation occured.
        /// </summary>
        public DateTime ObservationDate { get; }

        /// <summary>
        /// Name of the satellite the image came from ie GOES16.
        /// </summary>
        public string? SatelliteName { get; }

        /// <summary>
        /// Instrument used to get the image ie ABI = Advanced Baseline Imager.
        /// </summary>
        public string? Instrument { get; }

        /// <summary>
        /// The type of image taken. ie. Full Disk, CONUS etc.
        /// </summary>
        public string? SectorType { get; }

        /// <summary>
        /// How the image was composed. ie. band type or combinations of things such as GeoColor.
        /// </summary>
        public string? SatelliteImageType { get; }

        /// <summary>
        /// Creates a SatelliteImage with the given parameters. 
        /// </summary>
        /// <exception cref="ArgumentException">If the given path does not exist, or if the specified file is not an image file</exception>
        public SatelliteImageMetadata(DateTime observationDate, DateTime creationDateTimeUtc, string fullFileName, string? satelliteName, string? instrument, string? sectorType, string? satelliteImageType)
        {
            ObservationDate = observationDate;
            CreationDateTimeUtc = creationDateTimeUtc;
            FullFileName = fullFileName;
            SatelliteName = satelliteName;
            Instrument = instrument;
            SectorType = sectorType;
            SatelliteImageType = satelliteImageType;
        }

        public SatelliteImageMetadata(DateTime observationDate, DateTime creationDateTimeUtc, string fullFileName) : this(observationDate, creationDateTimeUtc, fullFileName, null, null, null, null) { }
    }
}
