using System.Diagnostics;

namespace SatelliteWallpaperUpdater.Interfaces.Repositories
{
    public interface IEventLogRepository
    {
        void WriteToEventLog(string message, EventLogEntryType type);
    }
}
