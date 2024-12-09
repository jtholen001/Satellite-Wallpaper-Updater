namespace SatelliteWallpaperUpdater.Repositories.Helpers
{
    // TODO make this data driven updating on a day by day basis. This would make the code less brittle and updateable in the future.
    public static class KnownImageSizesSelector
    {
        public enum ImageSizes
        {
            Small = 0,
            Medium = 1,
            Large = 2,
        }
        public static List<string> Sizes = new List<string>()
        {
            "339x339",
            "678x678",
            "1808x1808",
            "5424x5424",
            "10848x10848",
            "21696x21696",
        };

        public static IDictionary<ImageSizes, string> ImageSizeDict => new Dictionary<ImageSizes, string>()
        {
            { ImageSizes.Small, "678x678"},
            { ImageSizes.Medium,  "1808x1808" },
            { ImageSizes.Large, "5424x5424" }
        };
    }
}
