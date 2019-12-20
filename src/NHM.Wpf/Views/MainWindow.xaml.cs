using NHM.Common;
using NHM.Wpf.ViewModels;
using NHM.Wpf.Views.Common;
using NHM.Wpf.Views.Common.NHBase;
using NHM.Wpf.Views.TDPSettings;
using NHMCore;
using NHMCore.Configs;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace NHM.Wpf.Views
{
    /// <summary>
    /// Interaction logic for MainWindowNew2.xaml
    /// </summary>
    public partial class MainWindow : NHMMainWindow
    {
        private readonly MainVM _vm;
        private bool _miningStoppedOnClose;

        public MainWindow()
        {
            InitializeComponent();

            _vm = this.AssertViewModel<MainVM>();

            Translations.LanguageChanged += (s, e) => WindowUtils.Translate(this);
            LoadingBar.Visibility = Visibility.Visible;
            Topmost = GUISettings.Instance.GUIWindowsAlwaysOnTop;
        }

        private void GUISettings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(GUISettings.GUIWindowsAlwaysOnTop))
            {
                this.Topmost = _vm.GUISettings.GUIWindowsAlwaysOnTop;
            }
        }

        #region Start-Loaded/Closing
        private async void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            ThemeSetterManager.SetThemeSelectedThemes();
            await MainWindow_OnLoadedTask();
            _vm.GUISettings.PropertyChanged += GUISettings_PropertyChanged;
        }

        // just in case we add more awaits this signature will await all of them
        private async Task MainWindow_OnLoadedTask()
        {
            try
            {
                await _vm.InitializeNhm(LoadingBar.StartupLoader);
            }
            finally
            {
                LoadingBar.Visibility = Visibility.Collapsed;
                // Re-enable managed controls
                IsEnabled = true;
                SetTabButtonsEnabled();
                if (BuildOptions.SHOW_TDP_SETTINGS)
                {
                    var tdpWindow = new TDPSettingsWindow();
                    tdpWindow.DataContext = _vm;
                    tdpWindow.Show();
                }
            }
        }

        private async void MainWindow_OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            await MainWindow_OnClosingTask(e);
        }

        private async Task MainWindow_OnClosingTask(System.ComponentModel.CancelEventArgs e)
        {
            // Only ever try to prevent closing once
            if (_miningStoppedOnClose) return;

            _miningStoppedOnClose = true;
            //e.Cancel = true;
            IsEnabled = false;
            //await _vm.StopMining();
            await ApplicationStateManager.BeforeExit();
            ApplicationStateManager.ExecuteApplicationExit();
            //Close();
        }
        #endregion Start-Loaded/Closing

        protected override void OnTabSelected(ToggleButtonType tabType)
        {
            var tabName = tabType.ToString();
            foreach (TabItem tab in MainTabs.Items)
            {
                if (tabName.Contains(tab.Name))
                {
                    MainTabs.SelectedItem = tab;
                    break;
                }
            }
        }

        #region Minimize to tray stuff
        private void CloseMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void TaskbarIcon_OnTrayMouseDoubleClick(object sender, RoutedEventArgs e)
        {
            Show();
            WindowState = WindowState.Normal;
            Activate();
        }

        private void MainWindow_OnStateChanged(object sender, EventArgs e)
        {
            if (!_vm.GUISettings.MinimizeToTray) return;
            if (WindowState == WindowState.Minimized) // TODO && config min to tray
                Hide();
        }
        #endregion Minimize to tray stuff
    }
}
