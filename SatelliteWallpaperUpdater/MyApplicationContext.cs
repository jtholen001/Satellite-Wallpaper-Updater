using Microsoft.Extensions.Options;
using SatelliteWallpaperUpdater.Configuration;
using SatelliteWallpaperUpdater.Forms;
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

        public MyApplicationContext(SatelliteDesktopUpdateService satelliteDesktopUpdateService, IOptions<AppSettings> appSettings)
        {
            updateService = satelliteDesktopUpdateService;
            this.appSettings = appSettings;
            updateService.BackgroundUpdated += BackgroundUpdated;
            updateService.Start();

            Application.ApplicationExit += new EventHandler(this.OnApplicationExit);
            InitializeComponent();
            TrayIcon.Visible = true;
        }

        private void InitializeComponent()
        {
            TrayIcon = new NotifyIcon();

            TrayIcon.BalloonTipIcon = ToolTipIcon.Info;
            TrayIcon.BalloonTipText =
              "I noticed that you double-clicked me! What can I do for you?";
            TrayIcon.BalloonTipTitle = "You called Master?";
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
            TrayIcon.ContextMenuStrip = TrayIconContextMenu;
        }

        private void OnApplicationExit(object sender, EventArgs e)
        {
            //Cleanup so that the icon will be removed when the application is closed
            TrayIcon.Visible = false;
            TrayIcon.Dispose();
        }

        private void TrayIcon_DoubleClick(object sender, EventArgs e)
        {
            //Here, you can do stuff if the tray icon is doubleclicked
            TrayIcon.ShowBalloonTip(10000);

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
    }
}
