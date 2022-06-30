using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NiceHashMiner.Views.Settings
{
    /// <summary>
    /// Interaction logic for SettingsSchedulerCheckBoxItem.xaml
    /// </summary>
    public partial class SettingsSchedulerCheckBoxItem : UserControl
    {
        public static readonly DependencyProperty EnabledProperty = DependencyProperty.Register(
            nameof(Enabled),
            typeof(bool?),
            typeof(SettingsSchedulerCheckBoxItem),
            new FrameworkPropertyMetadata { BindsTwoWayByDefault = true });

        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register(nameof(Label), typeof(string), typeof(SettingsSchedulerCheckBoxItem));

        public SettingsSchedulerCheckBoxItem()
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

        private void ToggleClickHandler(object sender, RoutedEventArgs e)
        {
            if (e.Source is ToggleButton tb && ToggleButtonHidden == tb)
            {
                Enabled = !Enabled;
            }

            ToggleClick?.Invoke(sender, e);
            // we save on every change
            //ConfigManager.GeneralConfigFileCommit();
        }
    }
}
