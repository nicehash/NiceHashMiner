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
using System.Windows.Navigation;
using System.Windows.Shapes;
using NHM.Wpf.ViewModels;
using NHM.Wpf.ViewModels.Models;

namespace NHM.Wpf.Windows.Plugins
{
    /// <summary>
    /// Interaction logic for PluginEntry.xaml
    /// </summary>
    public partial class PluginEntry : UserControl
    {
        public event EventHandler<PluginEventArgs> InstallClick;
        public event EventHandler<PluginEventArgs> DetailsClick;

        public static readonly DependencyProperty PluginProperty = DependencyProperty.Register(
            nameof(Plugin),
            typeof(PluginVM.FakePlugin),
            typeof(PluginEntry),
            new PropertyMetadata());

        public PluginVM.FakePlugin Plugin
        {
            get => (PluginVM.FakePlugin) GetValue(PluginProperty);
            set => SetValue(PluginProperty, value);
        }

        public PluginEntry()
        {
            InitializeComponent();
        }

        private void InstallButton_OnClick(object sender, RoutedEventArgs e)
        {
            InstallClick?.Invoke(this, new PluginEventArgs(Plugin));
        }

        private void DetailsButton_OnClick(object sender, RoutedEventArgs e)
        {
            DetailsClick?.Invoke(this, new PluginEventArgs(Plugin));
        }
    }
}
