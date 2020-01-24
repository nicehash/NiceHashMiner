using NHM.Common;
using NiceHashMiner.ViewModels;
using NiceHashMiner.Views.Common;
using NiceHashMiner.Views.Common.NHBase;
using NiceHashMiner.Views.TDPSettings;
using NHMCore;
using NHMCore.Configs;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using NHMCore.Utils;

namespace NiceHashMiner.Views
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
            Title = ApplicationStateManager.Title;

            Translations.LanguageChanged += (s, e) => WindowUtils.Translate(this);
            LoadingBar.Visibility = Visibility.Visible;
            Topmost = GUISettings.Instance.GUIWindowsAlwaysOnTop;
            CustomDialogManager.MainWindow = this;
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
            UpdateHelpers.OnAutoUpdate = () =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    var nhmUpdatedrDialog = new CustomDialog()
                    {
                        Title = Translations.Tr("NiceHash Miner Starting Update"),
                        Description = Translations.Tr("NiceHash Miner auto updater in progress."),
                        OkText = Translations.Tr("OK"),
                        CancelVisible = Visibility.Collapsed,
                        OkVisible = Visibility.Collapsed,
                    };
                    ShowContentAsModalDialog(nhmUpdatedrDialog);
                });
            };
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
                if (Launcher.IsUpdated)
                {
                    var nhmUpdatedDialog = new CustomDialog()
                    {
                        Title = Translations.Tr("NiceHash Miner Updated"),
                        Description = Translations.Tr("Completed NiceHash Miner auto update."),
                        OkText = Translations.Tr("OK"),
                        CancelVisible = Visibility.Collapsed
                    };
                    ShowContentAsModalDialog(nhmUpdatedDialog);
                }

                if (Launcher.IsUpdatedFailed)
                {
                    var nhmUpdatedDialog = new CustomDialog()
                    {
                        Title = Translations.Tr("NiceHash Miner Autoupdate Failed"),
                        Description = Translations.Tr("NiceHash Miner auto update failed to complete. Autoupdates are disabled until next miner launch."),
                        OkText = Translations.Tr("OK"),
                        CancelVisible = Visibility.Collapsed
                    };
                    ShowContentAsModalDialog(nhmUpdatedDialog);
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
