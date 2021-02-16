using NHMCore.Configs;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace NiceHashMiner.Views.Settings
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
            var tb = e.Source as ToggleButton;
            if (ToggleButtonHidden == tb)
            {
                Enabled = !Enabled;
            }

            ToggleClick?.Invoke(sender, e);
            // we save on every change
            ConfigManager.GeneralConfigFileCommit();
        }
    }
}
