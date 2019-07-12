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

namespace NHM.Wpf.Windows.Plugins
{
    /// <summary>
    /// Interaction logic for PluginEntry.xaml
    /// </summary>
    public partial class PluginEntry : UserControl
    {
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
            nameof(Title),
            typeof(string),
            typeof(PluginEntry),
            new PropertyMetadata());
        public static readonly DependencyProperty VersionProperty = DependencyProperty.Register(
            nameof(Version),
            typeof(Version),
            typeof(PluginEntry),
            new PropertyMetadata());
        public static readonly DependencyProperty AuthorProperty = DependencyProperty.Register(
            nameof(Author),
            typeof(string),
            typeof(PluginEntry),
            new PropertyMetadata());

        public string Title
        {
            get => (string) GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public Version Version
        {
            get => (Version) GetValue(VersionProperty);
            set => SetValue(VersionProperty, value);
        }

        public string Author
        {
            get => (string) GetValue(AuthorProperty);
            set => SetValue(AuthorProperty, value);
        }

        public PluginEntry()
        {
            InitializeComponent();
        }
    }
}
