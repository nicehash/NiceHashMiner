using System.Windows;
using System.Windows.Controls;

namespace NHM.Wpf.Views.Plugins.Controls
{
    /// <summary>
    /// Interaction logic for PluginInstallProgress.xaml
    /// </summary>
    public partial class PluginInstallProgress : UserControl
    {
        public static readonly DependencyProperty ProgressProperty = DependencyProperty.Register(
            nameof(Progress),
            typeof(double),
            typeof(PluginInstallProgress));

        public static readonly DependencyProperty StatusProperty = DependencyProperty.Register(
            nameof(Status),
            typeof(string),
            typeof(PluginInstallProgress));

        public double Progress
        {
            get => (double) GetValue(ProgressProperty);
            set => SetValue(ProgressProperty, value);
        }

        public string Status
        {
            get => (string) GetValue(StatusProperty);
            set => SetValue(StatusProperty, value);
        }

        public PluginInstallProgress()
        {
            InitializeComponent();
        }
    }
}
