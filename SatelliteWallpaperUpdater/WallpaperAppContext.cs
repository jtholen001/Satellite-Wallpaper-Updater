using Microsoft.Extensions.Options;
using Microsoft.Win32;
using SatelliteWallpaperUpdater.Configuration;
using SatelliteWallpaperUpdater.Forms;
using SatelliteWallpaperUpdater.Helpers;
using SatelliteWallpaperUpdater.Interfaces.Repositories;
using SatelliteWallpaperUpdater.Models;

namespace SatelliteWallpaperUpdater
{
    public class WallpaperAppContext : ApplicationContext
    {
        //Component declarations
        private NotifyIcon TrayIcon;
        private ContextMenuStrip TrayIconContextMenu;
        private ToolStripMenuItem CloseMenuItem;
        private SatelliteDesktopUpdateService updateService;
        private SatelliteImageMetadata currentImage;
        private IOptions<AppSettings> appSettings;
        private IEventLogRepository eventLogRepository;
        private System.Timers.Timer timer;

        public WallpaperAppContext(
            SatelliteDesktopUpdateService satelliteDesktopUpdateService,
            IOptions<AppSettings> appSettings,
            IEventLogRepository eventLogRepository)
        {
            // Set the application to run on startup
            StartupHelper.SetStartUp();

            // Initialize the update service and subscribe to the event
            updateService = satelliteDesktopUpdateService;
            this.appSettings = appSettings;
            this.eventLogRepository = eventLogRepository;

            updateService.BackgroundUpdated += BackgroundUpdated;
            updateService.UpdateBackgroundAsync().ConfigureAwait(true);

            Application.ApplicationExit += new EventHandler(this.OnApplicationExit);
            InitializeComponent();

            timer = new System.Timers.Timer(TimeSpan.FromMinutes(10));
            timer.Elapsed += OnTimerElapsed;
            timer.Enabled = true;
            timer.AutoReset = true;

            SystemEvents.PowerModeChanged += PowerModeChanged;
        }

        private void InitializeComponent()
        {
            this.CheckLoggingAbility();

            TrayIcon = new NotifyIcon();

            TrayIcon.BalloonTipIcon = ToolTipIcon.Info;
            TrayIcon.Text = appSettings.Value.ApplicationName;

            TrayIcon.Icon = new Icon(new MemoryStream(Resources.front_right));

            TrayIcon.DoubleClick += TrayIcon_DoubleClick;

            TrayIconContextMenu = new ContextMenuStrip();
            CloseMenuItem = new ToolStripMenuItem();
            TrayIconContextMenu.SuspendLayout();

            // 
            // TrayIconContextMenu
            // 
            this.TrayIconContextMenu.Items.AddRange(new ToolStripItem[] {
            this.CloseMenuItem});
            this.TrayIconContextMenu.Name = "TrayIconContextMenu";
            this.TrayIconContextMenu.Size = new Size(153, 70);

            // 
            // CloseMenuItem
            // 
            this.CloseMenuItem.Name = "CloseMenuItem";
            this.CloseMenuItem.Size = new Size(152, 22);
            this.CloseMenuItem.Text = "Close the application";
            this.CloseMenuItem.Click += new EventHandler(this.CloseMenuItem_Click);

            TrayIconContextMenu.ResumeLayout(false);
            TrayIcon.Visible = true;
            TrayIcon.ContextMenuStrip = TrayIconContextMenu;
        }

        private void OnApplicationExit(object sender, EventArgs e)
        {
            //Cleanup so that the icon will be removed when the application is closed
            TrayIcon.Visible = false;
            TrayIcon.Dispose();
            timer.Dispose();
            SystemEvents.PowerModeChanged -= PowerModeChanged;
        }

        private void TrayIcon_DoubleClick(object sender, EventArgs e)
        {
            new ImageViewerForm(updateService, currentImage, appSettings).Show();
        }

        private void CloseMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Closing this will prevent updates to your wallpaper, are you sure?",
                    $"Close {appSettings.Value.ApplicationName}?", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation,
                    MessageBoxDefaultButton.Button2) == DialogResult.Yes)
            {
                Application.Exit();
            }
        }

        private void BackgroundUpdated(object sender, BackgroundUpdatedEventArgs e)
        {
            if (e.SatelliteImageMetadata != null)
            {
                currentImage = e.SatelliteImageMetadata;
            }
        }

        private void OnTimerElapsed(object sender, EventArgs e)
        {
            // update on the interval
            updateService.UpdateBackgroundAsync().ConfigureAwait(false);
        }

        private void PowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            // if the computer has been asleep we should update since its likely way off of the current time.
            if (e.Mode == PowerModes.Resume)
            {
                updateService.UpdateBackgroundAsync().ConfigureAwait(false);
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
