using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace NHM.Wpf.Views.Settings.Controls
{
    /// <summary>
    /// Interaction logic for SettingsContainer.xaml
    /// </summary>
    [ContentProperty(nameof(Children))]
    public partial class SettingsContainer : UserControl
    {
        public static readonly DependencyPropertyKey ChildrenProperty = DependencyProperty.RegisterReadOnly(
            nameof(Children),
            typeof(UIElementCollection),
            typeof(SettingsContainer),
            new PropertyMetadata());

        public static readonly DependencyProperty EnabledProperty = DependencyProperty.Register(
            nameof(Enabled),
            typeof(bool?),
            typeof(SettingsContainer),
            new FrameworkPropertyMetadata { BindsTwoWayByDefault = true });

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title), typeof(string), typeof(SettingsContainer));

        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register(nameof(Description), typeof(string), typeof(SettingsContainer));

        public UIElementCollection Children
        {
            get => (UIElementCollection) GetValue(ChildrenProperty.DependencyProperty);
            private set => SetValue(ChildrenProperty, value);
        }

        public bool? Enabled
        {
            get => (bool?) GetValue(EnabledProperty);
            set => SetValue(EnabledProperty, value);
        }

        public bool DisplayEnabled => Enabled ?? true;

        public string Title
        {
            get => (string) GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public string Description
        {
            get => (string) GetValue(DescriptionProperty);
            set => SetValue(DescriptionProperty, value);
        }

        public SettingsContainer()
        {
            InitializeComponent();
            Children = ChildrenStackPanel.Children;
            //DataContext = this;
        }
    }
}
