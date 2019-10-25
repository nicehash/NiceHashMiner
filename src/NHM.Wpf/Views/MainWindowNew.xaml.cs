using NHM.Common;
using NHM.Wpf.ViewModels;
using NHM.Wpf.Views.Common;
using NHMCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NHM.Wpf.Views
{
    /// <summary>
    /// Interaction logic for MainWindowNew.xaml
    /// </summary>
    public partial class MainWindowNew : Window
    {
        private readonly MainVM _vm;

        public MainWindowNew()
        {
            InitializeComponent();

            _vm = this.AssertViewModel<MainVM>();

            Translations.LanguageChanged += (s, e) => WindowUtils.Translate(this);
            
            WindowUtils.InitWindow(this);
        }

        private async void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            WindowUtils.SetForceSoftwareRendering(this);

            try
            {
                await _vm.InitializeNhm(LoadingBar.StartupLoader);
            }
            finally
            {
                LoadingBar.Visibility = Visibility.Collapsed;
                DashboardTabs.Visibility = Visibility.Visible;
                // Re-enable managed controls
                IsEnabled = true;
            }
        }
    }
}
