using Microsoft.Extensions.Options;
using SatelliteWallpaperUpdater.Configuration;
using SatelliteWallpaperUpdater.Interfaces.Repositories;
using System.Diagnostics;

namespace SatelliteWallpaperUpdater.Repositories
{
    public class EventLogRepository : IEventLogRepository
    {
        private readonly IOptions<AppSettings> _appSettings;

        public EventLogRepository(IOptions<AppSettings> appSettings) 
        {
            _appSettings = appSettings;
        }

        public void WriteToEventLog(string message, EventLogEntryType type)
        {
            if (!EventLog.SourceExists(_appSettings.Value.ApplicationName))
            {
                EventLog.CreateEventSource(_appSettings.Value.ApplicationName, "Application");
            }


            using (EventLog eventLog = new("Application"))
            {
                eventLog.Source = _appSettings.Value.ApplicationName;
                eventLog.WriteEntry(message, type);
            }
        }
    }
}
