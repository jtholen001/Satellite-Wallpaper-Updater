namespace SatelliteWallpaperUpdater.Helpers
{
    public static class FileHelper
    {
        public static string GetFilePathOfBackgroundImage(string fileName)
        {
            return GetSaveDirectory() + "\\" + fileName;
        }

        public static string GetSaveDirectory()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.MyPictures) + "\\SatelliteImageBackgrounds";
        }
    }
}
