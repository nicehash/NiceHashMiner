using System.Windows;
using System.Windows.Controls;

namespace NHM.Wpf.Views.Plugins.Controls
{
    /// <summary>
    /// Interaction logic for InlineDataDisplay.xaml
    /// </summary>
    public partial class InlineDataDisplay : UserControl
    {
        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(
            nameof(Header),
            typeof(string),
            typeof(InlineDataDisplay));

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value),
            typeof(string),
            typeof(InlineDataDisplay));

        public string Header
        {
            get => (string) GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        public string Value
        {
            get => (string) GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public InlineDataDisplay()
        {
            InitializeComponent();
        }
    }
}
