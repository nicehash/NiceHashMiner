using System.ComponentModel;
using System.Windows;
using NHM.Common;
using NHM.Wpf.ViewModels;

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

            if (DataContext is StartupLoadingVM vm)
            {
                StartupLoader = vm;
            }
            else
            {
                vm = new StartupLoadingVM();
                DataContext = vm;
                StartupLoader = vm;
            }
        }

        private void StartupLoadingWindow_OnClosing(object sender, CancelEventArgs e)
        {
            if (!CanClose) e.Cancel = true;
        }
    }
}
