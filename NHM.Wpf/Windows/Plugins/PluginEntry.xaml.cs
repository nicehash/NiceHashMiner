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

namespace NHM.Wpf.Windows.Plugins
{
    /// <summary>
    /// Interaction logic for PluginEntry.xaml
    /// </summary>
    public partial class PluginEntry : UserControl
    {
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
    }
}
