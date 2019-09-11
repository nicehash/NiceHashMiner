using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using NHM.Common;
using NHM.Wpf.ViewModels;
using NHM.Wpf.Views.Common;
using NHM.Wpf.Views.Plugins;
using NHM.Wpf.Views.Settings;
using NHMCore;
using NHMCore.Configs;
using NHMCore.Mining;
using NHMCore.Utils;
using MessageBox = System.Windows.Forms.MessageBox;

namespace NHM.Wpf.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainVM _vm;
        private bool _miningStoppedOnClose;

        public MainWindow()
        {
            InitializeComponent();

            _vm = this.AssertViewModel<MainVM>();

            Translations.LanguageChanged += TranslationsOnLanguageChanged;
            TranslationsOnLanguageChanged(null, null);
        }

        private void TranslationsOnLanguageChanged(object sender, EventArgs e)
        {
            WindowUtils.Translate(this);
        }

        private async void BenchButton_Click(object sender, RoutedEventArgs e)
        {
            bool startMining;
            ApplicationStateManager.CurrentForm = ApplicationStateManager.CurrentFormState.Benchmark;

            using (var bench = new BenchmarkWindow(AvailableDevices.Devices))
            {
                startMining = bench.ShowDialog() ?? false;
            }

            ApplicationStateManager.CurrentForm = ApplicationStateManager.CurrentFormState.Main;

            if (startMining) await _vm.StartMining();
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            ApplicationStateManager.CurrentForm = ApplicationStateManager.CurrentFormState.Settings;

            using (var settings = new SettingsWindow())
            {
                settings.ShowDialog();

                if (settings.RestartRequired)
                {
                    if (!settings.DefaultsSet)
                    {
                        MessageBox.Show(
                            Translations.Tr("Settings change requires {0} to restart.", NHMProductInfo.Name),
                            Translations.Tr("Restart Notice"),
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }

                    ApplicationStateManager.RestartProgram();
                    Close();
                }
            }

            ApplicationStateManager.CurrentForm = ApplicationStateManager.CurrentFormState.Main;
        }

        private void PluginButton_Click(object sender, RoutedEventArgs e)
        {
            ApplicationStateManager.CurrentForm = ApplicationStateManager.CurrentFormState.Plugins;
            var plugin = new PluginWindow();
            ApplicationStateManager.CurrentForm = ApplicationStateManager.CurrentFormState.Main;
            plugin.ShowDialog();
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
            if (WindowState == WindowState.Minimized) // TODO && config min to tray
                Hide();
        }

        #endregion

        private void StatsHyperlink_OnClick(object sender, RoutedEventArgs e)
        {
            if (ConfigManager.CredentialsSettings.IsCredentialsValid == false) return;
            ApplicationStateManager.VisitMiningStatsPage();
        }

        private async void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            IsEnabled = false;
            var startup = new StartupLoadingWindow
            {
                Owner = this, WindowStartupLocation = WindowStartupLocation.CenterOwner, CanClose = false
            };
            startup.Show();

            await _vm.InitializeNhm(startup.StartupLoader);

            startup.CanClose = true;

            // If owner is still set to this when close is called, 
            // it will minimize the main window for some reason
            startup.Owner = null;
            startup.Close();

            IsEnabled = true;
        }

        private async void StartButton_OnClick(object sender, RoutedEventArgs e)
        {
            await _vm.StartMining();
        }

        private async void StopButton_OnClick(object sender, RoutedEventArgs e)
        {
            await _vm.StopMining();
        }

        private void HelpButton_OnClick(object sender, RoutedEventArgs e)
        {
            Process.Start(Links.NhmHelp);
        }

        private void ExchangeButton_OnClick(object sender, RoutedEventArgs e)
        {
            Process.Start(Links.NhmPayingFaq);
        }

        // Without this contrived way of closing, the application could terminate 
        // before the async method is finished (thus mining not actually stopped)
        private async void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            // Only ever try to prevent closing once
            if (_miningStoppedOnClose) return;

            _miningStoppedOnClose = true;
            e.Cancel = true;
            IsEnabled = false;
            await _vm.StopMining();
            Close();
        }
    }
}
