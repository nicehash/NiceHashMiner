using NHM.Wpf.Windows.Common;
using NHM.Wpf.Windows.Plugins;
using System;
using System.Threading.Tasks;
using System.Windows;
using NHM.Wpf.ViewModels;

namespace NHM.Wpf.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Translations.LanguageChanged += TranslationsOnLanguageChanged;
            TranslationsOnLanguageChanged(null, null);

            var eula = new EulaWindow();
            eula.ShowDialog();
        }

        private void TranslationsOnLanguageChanged(object sender, EventArgs e)
        {
            WindowUtils.Translate(this);
        }

        private void BenchButton_Click(object sender, RoutedEventArgs e)
        {
            var bench = new BenchmarkWindow();
            bench.ShowDialog();
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            using (var settings = new SettingsWindow())
            {
                settings.ShowDialog();
            }
        }

        private void PluginButton_Click(object sender, RoutedEventArgs e)
        {
            var plugin = new PluginWindow();
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
        }

        private async void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            IsEnabled = false;
            var startup = new StartupLoadingWindow();
            startup.Owner = this;
            startup.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            startup.CanClose = false;
            startup.Show();

            if (startup.DataContext is StartupLoadingVM slvm)
                await FakeLoad(slvm.PrimaryProgress, slvm.SecondaryProgress, slvm);

            startup.CanClose = true;

            startup.Close();
            IsEnabled = true;
        }

        private static async Task FakeLoad(IProgress<(string message, double perc)> primaryProg,
            IProgress<(string message, double perc)> secProg, StartupLoadingVM vm)
        {
            for (var i = 0; i <= 100; i++)
            {
                primaryProg.Report(("Load", i));
                await Task.Delay(10);
                if (i == 60)
                {
                    vm.SecondaryVisible = true;
                    for (var j = 0; j <= 100; j++)
                    {
                        secProg.Report(("Sec load", j));
                        await Task.Delay(10);
                    }
                }
                else
                {
                    vm.SecondaryVisible = false;
                }
            }
        }
    }
}
