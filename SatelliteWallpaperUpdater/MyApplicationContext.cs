using Microsoft.Extensions.Options;
using Microsoft.Win32;
using SatelliteWallpaperUpdater.Configuration;
using SatelliteWallpaperUpdater.Forms;
using SatelliteWallpaperUpdater.Interfaces.Repositories;
using SatelliteWallpaperUpdater.Models;

namespace SatelliteWallpaperUpdater
{
    public class MyApplicationContext : ApplicationContext
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

        public MyApplicationContext(
            SatelliteDesktopUpdateService satelliteDesktopUpdateService, 
            IOptions<AppSettings> appSettings,
            IEventLogRepository eventLogRepository)
        {
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
            TrayIcon = new NotifyIcon();

            TrayIcon.BalloonTipIcon = ToolTipIcon.Info;
            TrayIcon.Text = "Satellite Wallpaper Updater";

            TrayIcon.Icon = new Icon(new MemoryStream(Resources.front_right));

            //Optional - handle doubleclicks on the icon:
            TrayIcon.DoubleClick += TrayIcon_DoubleClick;

            //Optional - Add a context menu to the TrayIcon:
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
            this.CloseMenuItem.Text = "Close the tray icon program";
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
            if (MessageBox.Show("Do you really want to close me?",
                    "Are you sure?", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation,
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
            if(e.Mode == PowerModes.Resume)
            {
                updateService.UpdateBackgroundAsync().ConfigureAwait(false);
            }
        }
    }
}
