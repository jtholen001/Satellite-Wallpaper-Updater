using Microsoft.Win32;
using System.Reflection;
using System.Security.Principal;

namespace SatelliteWallpaperUpdater.Helpers
{
    public static class StartupHelper
    {
        public static void SetStartUp()
        {
            try
            {
                string appName = Assembly.GetExecutingAssembly().GetName().Name;
                string appPath = string.Concat(
                    Environment.GetFolderPath(Environment.SpecialFolder.Programs),
                    "\\Satellite Wallpaper Updater\\Satellite Wallpaper Updater", ".appref-ms");
                string keyName = @"Software\Microsoft\Windows\CurrentVersion\Run";
                using var key = Registry.CurrentUser.OpenSubKey(keyName, true);                

                if (key != null)
                {
                    key.SetValue(appName, appPath);
                }

                key.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error setting startup: {ex.Message}");
            }
        }

        public static bool IsAdministrator()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}
