using System;
using System.Collections.Generic;
using System.Text;

namespace ElvUIUpdater
{
    public class MainWindowViewModel : ViewModelBase
    {
        private string _pathToWow;
        public string PathToWow
        {
            get => _pathToWow;
            set => SetProperty(ref _pathToWow, value);
        }

        private string _version;

        public string Version
        {
            get => _version;
            set => SetProperty(ref _version, value);
        }

        private string _elvui;

        public string ElvuiVersion
        {
            get => _elvui;
            set => SetProperty(ref _elvui, value);
        }

        private string _checked;

        public string Checked
        {
            get => _checked;
            set => SetProperty(ref _checked, value);
        }

        private string _path;

        public string Path
        {
            get => _path;
            set => SetProperty(ref _path, value);
        }

        public MainWindowViewModel()
        {
            Version = !String.IsNullOrEmpty(Settings.Default.CURRENT_VERSION) ? Settings.Default.CURRENT_VERSION : "Unknown";
            ElvuiVersion = !String.IsNullOrEmpty(Settings.Default.ELVUI_VERSION) ? Settings.Default.ELVUI_VERSION : "Unknown";
            Path = Settings.Default.WOW_PATH;

            if (Settings.Default["LAST_CHECKED"] == null)
            {
                Checked = "No date available.";
            }
            else
            {
                Checked = Settings.Default.LAST_CHECKED.ToString("G");
            }
        }
    }
}
