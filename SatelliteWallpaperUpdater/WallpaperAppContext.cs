using Microsoft.Extensions.Options;
using Microsoft.Win32;
using SatelliteWallpaperUpdater.Configuration;
using SatelliteWallpaperUpdater.Forms;
using SatelliteWallpaperUpdater.Helpers;
using SatelliteWallpaperUpdater.Interfaces.Repositories;
using SatelliteWallpaperUpdater.Models;
using System.ComponentModel;

namespace SatelliteWallpaperUpdater
{
    public class WallpaperAppContext : ApplicationContext
    {
        private readonly NotifyIcon TrayIcon;
        private readonly ContextMenuStrip TrayIconContextMenu;
        private readonly ToolStripMenuItem CloseMenuItem;
        private readonly SatelliteDesktopUpdateService updateService;
        private readonly IOptions<AppSettings> appSettings;
        private readonly IEventLogRepository eventLogRepository;
        private readonly System.Timers.Timer timer;
        private SatelliteImage? currentImage;

        public WallpaperAppContext(
            SatelliteDesktopUpdateService satelliteDesktopUpdateService,
            IOptions<AppSettings> appSettings,
            IEventLogRepository eventLogRepository)
        {
            // Set the application to run on startup
            StartupHelper.SetStartUp();

            // Create the form components
            TrayIcon = new NotifyIcon();
            TrayIconContextMenu = new ContextMenuStrip();
            CloseMenuItem = new ToolStripMenuItem();
            InitializeComponent(appSettings.Value);

            // Initialize the update service and subscribe to the event
            updateService = satelliteDesktopUpdateService;
            this.appSettings = appSettings;
            this.eventLogRepository = eventLogRepository;

            timer = new System.Timers.Timer(TimeSpan.FromMinutes(10));
            timer.Elapsed += OnTimerElapsed;
            timer.Enabled = true;
            timer.AutoReset = true;

            updateService.BackgroundUpdated += BackgroundUpdated;
            updateService.UpdateBackgroundAsync(currentImage).ConfigureAwait(false);

            Application.ApplicationExit += new EventHandler(OnApplicationExit);
            SystemEvents.PowerModeChanged += PowerModeChanged;

            CheckLoggingAbility();
        }

        private void InitializeComponent(AppSettings appSettings)
        {
            // TrayIcon
            TrayIcon.BalloonTipIcon = ToolTipIcon.Info;
            TrayIcon.Text = appSettings.ApplicationName;
            TrayIcon.Icon = new Icon(new MemoryStream(Resources.front_right));
            TrayIcon.DoubleClick += TrayIcon_DoubleClick;
            TrayIconContextMenu.SuspendLayout();

             
            // TrayIconContextMenu
            TrayIconContextMenu.Items.AddRange([CloseMenuItem]);
            TrayIconContextMenu.Name = "TrayIconContextMenu";
            TrayIconContextMenu.Size = new Size(153, 70);
            TrayIconContextMenu.ResumeLayout(false);
            TrayIcon.Visible = true;
            TrayIcon.ContextMenuStrip = TrayIconContextMenu;

            // CloseMenuItem
            CloseMenuItem.Name = "CloseMenuItem";
            CloseMenuItem.Size = new Size(152, 22);
            CloseMenuItem.Text = "Close the application";
            CloseMenuItem.Click += new EventHandler(CloseMenuItem_Click);
        }

        private void OnApplicationExit(object? sender, EventArgs e)
        {
            // Cleanup so that the icon will be removed when the application is closed 
            timer.Dispose();
            SystemEvents.PowerModeChanged -= PowerModeChanged;
        }

        private void TrayIcon_DoubleClick(object? sender, EventArgs e)
        {
            new ImageViewerForm(updateService, currentImage, appSettings).Show();
        }

        private void CloseMenuItem_Click(object? sender, EventArgs e)
        {
            if (MessageBox.Show("Closing this will prevent updates to your wallpaper, are you sure?",
                    $"Close {appSettings.Value.ApplicationName}?", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation,
                    MessageBoxDefaultButton.Button2) == DialogResult.Yes)
            {
                Application.Exit();
            }
        }

        private void BackgroundUpdated(object? sender, BackgroundUpdatedEventArgs e)
        {
            if (e.SatelliteImage != null)
            {
                currentImage = e.SatelliteImage;
            }
        }

        private void OnTimerElapsed(object? sender, EventArgs e)
        {
            // update on the interval
            updateService.UpdateBackgroundAsync(currentImage).ConfigureAwait(false);
        }

        private void PowerModeChanged(object? sender, PowerModeChangedEventArgs e)
        {
            // if the computer has been asleep we should update since its likely way off of the current time.
            if (e.Mode == PowerModes.Resume)
            {
                updateService.UpdateBackgroundAsync(currentImage).ConfigureAwait(false);
            }
        }

        private void CheckLoggingAbility()
        {
            if (!StartupHelper.IsAdministrator()  && !eventLogRepository.EventLogSourceExists())
            {
                MessageBox.Show($"The application does not have the ability to write to the event log.",
               "Please run the application in administrator mode to fix this.", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
