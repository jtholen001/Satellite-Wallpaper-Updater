using System.Diagnostics;

namespace SatelliteWallpaperUpdater.Interfaces.Repositories
{
    public interface IEventLogRepository
    {
        public void WriteToEventLog(string message, EventLogEntryType type);

        public bool EventLogSourceExists();
    }
}
