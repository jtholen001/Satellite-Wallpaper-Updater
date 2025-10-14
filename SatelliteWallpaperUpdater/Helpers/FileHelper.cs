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
            if(string.IsNullOrEmpty(Properties.Settings.Default.BackgroundImageFolder))
            {
                Properties.Settings.Default.BackgroundImageFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures) + "\\SatelliteImageBackgrounds";
                Properties.Settings.Default.Save();
            }

            return Properties.Settings.Default.BackgroundImageFolder;
        }
    }
}
