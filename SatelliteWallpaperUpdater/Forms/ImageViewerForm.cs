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
        private SatelliteImage? currentBackground;
        private IOptions<AppSettings> appSettings;

        public ImageViewerForm(
            SatelliteDesktopUpdateService satelliteDesktopUpdateService,
            SatelliteImage? currentImage,
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
            UpdateLabelText(currentBackground?.Metadata?.CreationDateTimeUtc.ToLocalTime());
        }

        private void BackgroundUpdated(object sender, BackgroundUpdatedEventArgs e)
        {
            if (e.SatelliteImage?.Metadata?.FullFileName != null)
            {
                currentBackground = e.SatelliteImage;
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
            if (currentBackground?.FilePath != null)
            {
                pictureBox1.BeginInvoke(new Action(() =>
                {
                    if (File.Exists(currentBackground.FilePath))
                    {
                        pictureBox1.ImageLocation = currentBackground.FilePath;
                        pictureBox1.Refresh();
                    }
                }));
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                components?.Dispose();

                if (desktopUpdateService != null)
                {
                    desktopUpdateService.BackgroundUpdated -= BackgroundUpdated;
                }
                if (Icon != null)
                {
                    Icon.Dispose();
                }
                label1.Dispose();
                pictureBox1.Dispose();
                menuStrip1.Dispose();
                fileToolStripMenuItem.Dispose();
                setImageLocationToolStripMenuItem.Dispose();
            }
            base.Dispose(disposing);

            GC.Collect();
            Console.WriteLine("test");
        }

        private void SetImageLocation_Clicked(object sender, EventArgs e)
        {
            using (FolderBrowserDialog openFolder = new FolderBrowserDialog())
            {
                openFolder.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
                openFolder.ShowNewFolderButton = true;
                openFolder.ShowPinnedPlaces = true;
                
                DialogResult result = openFolder.ShowDialog(this);
                

                if(result == DialogResult.OK && Directory.Exists(openFolder.SelectedPath))
                {
                    Properties.Settings.Default.BackgroundImageFolder = openFolder.SelectedPath;
                    Properties.Settings.Default.Save();
                }
            }
        }
    }
}
