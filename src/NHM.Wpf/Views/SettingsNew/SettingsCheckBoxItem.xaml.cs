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

namespace NHM.Wpf.Views.SettingsNew
{
    /// <summary>
    /// Interaction logic for SettingsCheckBoxItem.xaml
    /// </summary>
    public partial class SettingsCheckBoxItem : UserControl
    {
        public static readonly DependencyProperty EnabledProperty = DependencyProperty.Register(
            nameof(Enabled),
            typeof(bool?),
            typeof(SettingsCheckBoxItem),
            new FrameworkPropertyMetadata { BindsTwoWayByDefault = true });

        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register(nameof(Label), typeof(string), typeof(SettingsCheckBoxItem));

        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register(nameof(Description), typeof(string), typeof(SettingsCheckBoxItem));

        public SettingsCheckBoxItem()
        {
            InitializeComponent();
        }

        public event EventHandler<RoutedEventArgs> ToggleClick;

        public bool? Enabled
        {
            get => (bool?)GetValue(EnabledProperty);
            set => SetValue(EnabledProperty, value);
        }

        public string Label
        {
            get => (string)GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        public string Description
        {
            get => (string)GetValue(DescriptionProperty);
            set => SetValue(DescriptionProperty, value);
        }

        private void ToggleClickHandler(object sender, RoutedEventArgs e)
        {
            ToggleClick?.Invoke(sender, e);
        }
    }
}
