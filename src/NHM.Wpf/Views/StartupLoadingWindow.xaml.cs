using System.ComponentModel;
using System.Windows;
using NHM.Common;
using NHM.Wpf.ViewModels;
using NHM.Wpf.Views.Common;

namespace NHM.Wpf.Views
{
    /// <summary>
    /// Interaction logic for StartupLoadingWindow.xaml
    /// </summary>
    public partial class StartupLoadingWindow : Window
    {
        public bool CanClose { get; set; } = true;
        public IStartupLoader StartupLoader { get; }

        public StartupLoadingWindow()
        {
            InitializeComponent();

            StartupLoader = this.AssertViewModel<StartupLoadingVM>();
            //WindowUtils.InitWindow(this);
        }

        private void StartupLoadingWindow_OnClosing(object sender, CancelEventArgs e)
        {
            if (!CanClose) e.Cancel = true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            WindowUtils.SetForceSoftwareRendering(this);
        }
    }
}
