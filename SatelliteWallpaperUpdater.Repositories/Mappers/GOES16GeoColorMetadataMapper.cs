using SatelliteWallpaperUpdater.Interfaces.Repositories.Mappers;
using SatelliteWallpaperUpdater.Models;
using System.Text.RegularExpressions;

namespace SatelliteWallpaperUpdater.Repositories.Mappers
{
    public class GOES16GeoColorMetadataMapper : IGOES16GeoColorMetadataMapper
    {
        private static readonly char[] separators = ['_', '-', '.'];

        // regex to break out date string since datetime does not support day of year. Break it out into 4 groups to use in manually adding later
        private static readonly Regex dateRegex = new Regex("([0-9]{4})([0-9]{3})([0-9]{2})([0-9]{2})");
        private static readonly string expectedInput = "YYYYdddHHmm_{SatelliteName}-{Instrument}-{SectorType}-{SatelliteImageType}";

        /// <summary>
        /// Expects file name to be in the following format: 
        /// YYYYdddHHmm_{SatelliteName}-{Instrument}-{SectorType}-{SatelliteImageType}...
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public SatelliteImageMetadata MapViaFileName(string fileName)
        {
            try
            {
                string[] splitFileName = fileName.Split(separators);
                DateTime observationDate;
                bool dateParseSuccess = TryGetObservationDate(splitFileName[0], out observationDate);

                if (!dateParseSuccess)
                {
                    throw GetArgumentException(fileName);
                }

                return new SatelliteImageMetadata(
                    observationDate: observationDate,
                    fullFileName: fileName,
                    satelliteName: splitFileName[1],
                    instrument: splitFileName[2],
                    sectorType: splitFileName[3],
                    satelliteImageType: splitFileName[4]);
            }
            catch (Exception ex)
            {
                throw GetArgumentException(fileName);
            }


        }

        private static ArgumentException GetArgumentException(string fileName)
        {
            return new ArgumentException($"Expected fileName to be in format: {expectedInput} but found {fileName}");
        }

        /// <summary>
        /// Convert this specific date string into a date.
        /// </summary>
        /// <param name="dateString">Expected to be in YYYYdddHHmm format</param>
        /// <param name="observationDate">The resulting observationDate, or DateTime.MinValue if not successful.</param>
        /// <returns></returns>
        private static bool TryGetObservationDate(string dateString, out DateTime observationDate)
        {
            /// So .net doesn't support day of year at all. Which is a little frusturating, 
            /// so we have to do this crazy logic to get a date out of this madness.
            observationDate = DateTime.MinValue;
            Match match = dateRegex.Match(dateString);

            if (!match.Success)
            {
                return false;
            }

            List<Group> values = match.Groups.Values.ToList();

            // start at 1 as index 0 is the entire matched group.
            int year = int.Parse(values[1].Value);
            int dayOfYear = int.Parse(values[2].Value);
            int hours = int.Parse(values[3].Value);
            int minutes = int.Parse(values[4].Value);

            // Now that we have our datetime we create it with this year then do some math to convert day of year to day and month.
            observationDate = DateTime.SpecifyKind(new DateTime(year, 1, 1)
               .AddDays(dayOfYear - 1) // minus one since we start on the 1st day and not 0.
                                       // now add the hours and minutes to get the full datetime
               .AddHours(hours)
               .AddMinutes(minutes),
               DateTimeKind.Utc);

            return true;
        }
    }
}
