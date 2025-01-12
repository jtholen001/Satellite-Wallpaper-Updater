using Microsoft.Win32;
using SatelliteWallpaperUpdater.Interfaces.Repositories;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SatelliteWallpaperUpdater.Repositories
{
    public class WallPaperRepository(IEventLogRepository eventLogRepository) : IWallpaperRepository
    {
        private IEventLogRepository _eventLogRepository = eventLogRepository;

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int SystemParametersInfo(uint action, uint uParam, string vParam, uint winIni);

        private static readonly uint SPI_SETDESKWALLPAPER = 0x14;
        private static readonly uint SPIF_UPDATEINIFILE = 0x01;
        private static readonly uint SPIF_SENDWININICHANGE = 0x02;

        public void SetWallpaper(string file)
        {
            if (File.Exists(file))
            {
                // Gets the current user's settings
                var currentUser = Registry.CurrentUser.OpenSubKey("Volatile Environment");

                RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true);

                key.SetValue(@"WallpaperStyle", 6.ToString()); // 6 is fit
                key.SetValue(@"TileWallpaper", 0.ToString());
                key.SetValue("WallPaper", file);

                var result = SystemParametersInfo(SPI_SETDESKWALLPAPER, 1, file, SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);

                if (result == 0)
                {
                    _eventLogRepository.WriteToEventLog($"There was an error: {Marshal.GetLastWin32Error()}", EventLogEntryType.Error);
                    return;
                }
            }
        }
    }
}
