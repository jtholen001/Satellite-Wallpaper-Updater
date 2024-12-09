using Microsoft.Win32;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SatelliteWallpaperUpdater.Repositories
{
    public static class WallPaperRepository
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int SystemParametersInfo(uint action, uint uParam, string vParam, uint winIni);

        private static readonly uint SPI_SETDESKWALLPAPER = 0x14;
        private static readonly uint SPIF_UPDATEINIFILE = 0x01;
        private static readonly uint SPIF_SENDWININICHANGE = 0x02;

        public static void SetWallpaper(string file)
        {
            if (File.Exists(file))
            {
                var test = Registry.CurrentUser.OpenSubKey("Volatile Environment");

                RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true);
                //RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true);

                key.SetValue(@"WallpaperStyle", 6.ToString()); // 6 is fit
                key.SetValue(@"TileWallpaper", 0.ToString());
                key.SetValue("WallPaper", file);

                var result = SystemParametersInfo(SPI_SETDESKWALLPAPER, 1, file, SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);

                if (result == 0)
                {
                    WriteToEventLog($"There was an error: {Marshal.GetLastWin32Error()}", EventLogEntryType.Error);
                    return;
                }

                WriteToEventLog($"Current User: {test.GetValue("USERNAME")}{Environment.NewLine} Result: {result}", EventLogEntryType.Information);
            }
        }

        private static void WriteToEventLog(string message, EventLogEntryType type)
        {
            if (!EventLog.SourceExists("Satellite Desktop Update Service"))
            {
                EventLog.CreateEventSource("Satellite Desktop Update Service", "Application");
            }


            using (EventLog eventLog = new("Application"))
            {
                eventLog.Source = "Satellite Desktop Update Service";
                eventLog.WriteEntry(message, type);
            }
        }
    }
}
