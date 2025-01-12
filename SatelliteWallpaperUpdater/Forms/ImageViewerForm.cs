using Microsoft.Extensions.Options;
using SatelliteWallpaperUpdater.Configuration;
using SatelliteWallpaperUpdater.Helpers;
using SatelliteWallpaperUpdater.Models;
using System.ComponentModel;

namespace SatelliteWallpaperUpdater.Forms
{
    public partial class ImageViewerForm : Form
    {
        private readonly string LabelBaseText = "Current image below. Last update: ";
        private SatelliteDesktopUpdateService desktopUpdateService;
        private SatelliteImageMetadata? currentBackground;
        private IOptions<AppSettings> appSettings;

        public ImageViewerForm(
            SatelliteDesktopUpdateService satelliteDesktopUpdateService, 
            SatelliteImageMetadata? currentImage,
            IOptions<AppSettings> appSettings)
        {
            currentBackground = currentImage;
            this.appSettings = appSettings;
            InitializeComponent();
            InitializeForm();
            desktopUpdateService = satelliteDesktopUpdateService;
            desktopUpdateService.BackgroundUpdated += BackgroundUpdated;
            
        }

        private void InitializeForm()
        {
            this.Width = 512;
            this.Height = 512;

            label1.AutoSize = false;
            label1.TextAlign = ContentAlignment.MiddleCenter;
            label1.Dock = DockStyle.Fill;
            label1.Width = this.Width - 10;
            label1.Text = LabelBaseText;

            this.Icon = new Icon(new MemoryStream(Resources.front_right));

            this.Text = appSettings.Value.ApplicationName;
        }

        protected override void OnLoad(EventArgs e)
        {
            UpdateImage();
            UpdateLabelText(currentBackground?.CreationDateTimeUtc.ToLocalTime());
        }

        private void BackgroundUpdated(object sender, BackgroundUpdatedEventArgs e)
        {
            if (e.SatelliteImageMetadata?.FullFileName != null)
            {
                currentBackground = e.SatelliteImageMetadata;
                UpdateLabelText(e.UpdatedDateTimeUtc.ToLocalTime());
                UpdateImage();
            }
        }

        private void UpdateLabelText(DateTime? lastUpdatedDate)
        {
            label1.BeginInvoke(new Action(() =>
            {
                if (currentBackground != null)
                {
                    label1.Text = LabelBaseText + lastUpdatedDate;
                }
                else
                {
                    label1.Text = LabelBaseText;
                }
                label1.Refresh();
            }));

        }

        private void UpdateImage()
        {
            if (currentBackground != null)
            {
                pictureBox1.BeginInvoke(new Action(() =>
                {
                    string fullPath = FileHelper.GetFilePathOfBackgroundImage(currentBackground.FullFileName);

                    if (File.Exists(fullPath))
                    {
                        pictureBox1.ImageLocation = fullPath;
                        pictureBox1.Refresh();
                    }
                }));
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            if (desktopUpdateService != null)
            {
                desktopUpdateService.BackgroundUpdated -= BackgroundUpdated;
            }
        }
    }
}
