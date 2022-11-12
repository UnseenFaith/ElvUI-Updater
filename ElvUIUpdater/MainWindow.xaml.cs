using HtmlAgilityPack;
using MahApps.Metro.Controls;
using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;

namespace ElvUIUpdater
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {

        private readonly MainWindowViewModel _viewmodel = new MainWindowViewModel();

        private readonly Timer Timer = new Timer(1000);

        private int lastHour = DateTime.Now.Hour;

        private readonly HttpClient Client = new HttpClient();

        public string DOWNLOAD_LOCATION
        {
            get => $"https://www.tukui.org/downloads/elvui-{Settings.Default.ELVUI_VERSION}.zip";
        }

        public MainWindow()
        {
            InitializeComponent();

            DataContext = _viewmodel;

            Timer.Elapsed += Timer_Elapsed;
            Timer.Start();

        }

        private async void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (lastHour < DateTime.Now.Hour || (lastHour == 23 && DateTime.Now.Hour == 0))
            {
                lastHour = DateTime.Now.Hour;
                await ParseElvUIVersion();
            }
        }

        private async Task ParseElvUIVersion()
        {
            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = await web.LoadFromWebAsync("https://www.tukui.org/download.php?ui=elvui");

            var version = doc.DocumentNode.SelectSingleNode("//*[@id='version']/b[1]");
            var date = doc.DocumentNode.SelectSingleNode("//*[@id='version']/b[2]");

            Settings.Default.ELVUI_VERSION = version.InnerText.Trim();
            Settings.Default.LAST_UPDATED = date.InnerText.Trim();
            Settings.Default.LAST_CHECKED = DateTime.Now;

            _viewmodel.Checked = Settings.Default.LAST_CHECKED.ToString("G");
            _viewmodel.ElvuiVersion = Settings.Default.ELVUI_VERSION;

            Settings.Default.Save();
        }

        private async Task<bool> CheckIfUpdateNeeded()
        {
            var settings = Settings.Default;

            await ParseElvUIVersion();

            if (string.IsNullOrEmpty(settings.CURRENT_VERSION)) return true;

            if (new Version(settings.CURRENT_VERSION) < new Version(settings.ELVUI_VERSION)) return true;
            else return false;
        }

        private bool CheckForValidPath()
        {
            string[] paths = Settings.Default.WOW_PATH.Split(Path.DirectorySeparatorChar);

            System.Diagnostics.Trace.WriteLine(Settings.Default.WOW_PATH);

            if (paths[^1] != "_retail_" || paths[^2] != "World of Warcraft")
            {
                MessageBox.Show("The path to your wow installation is incorrect. Make sure that the location set is the retail folder of your installation.", "ElvUI Update Checker");
                return false;
            }
            return true;
        }

        private async void CheckForUpdate(object sender, RoutedEventArgs e)
        {
            var update = await CheckIfUpdateNeeded();

            if (update)
            {
                MessageBox.Show($"There is an update available: {Settings.Default.ELVUI_VERSION}, Current Version {Settings.Default.CURRENT_VERSION}", "ElvUI Update Checker");
            } else
            {
                MessageBox.Show("You are already using the most up to date version of Elvui.", "ElvUI Update Checker");
            }
        }

        private async void DownloadAndUnzipFile(object sender, RoutedEventArgs e)
        {
            if (!CheckForValidPath()) return;

            if (string.IsNullOrEmpty(Settings.Default.ELVUI_VERSION))
            {
                MessageBox.Show("You need to check for updates before you can download them.", "ElvUi Addon Updater");
                return;
            }

            var extractPath = Path.Combine(Settings.Default.WOW_PATH, "Interface", "AddOns");
            var tempFile = Path.GetTempFileName();

            using var response = await Client.GetAsync(DOWNLOAD_LOCATION);
            using var stream = await response.Content.ReadAsStreamAsync();
            var zip = File.OpenWrite(tempFile);
            await stream.CopyToAsync(zip);
            zip.Close();
            ZipFile.ExtractToDirectory(tempFile, extractPath, true);
            MessageBox.Show("Downloaded and extracted new ElvUI version to the Addons folder.", "ElvUi Addon Updater");

            Settings.Default.CURRENT_VERSION = Settings.Default.ELVUI_VERSION;
            Settings.Default.Save();
            _viewmodel.Version = Settings.Default.CURRENT_VERSION;
        }

        private void WowPathChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            Settings.Default.WOW_PATH = WowPath.Text;
            Settings.Default.Save();
        }
    }
}
