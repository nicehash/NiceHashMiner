using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using NHM.Wpf.Annotations;

namespace NHM.Wpf.Windows.Settings.Controls
{
    /// <summary>
    /// Interaction logic for SettingsContainer.xaml
    /// </summary>
    [ContentProperty(nameof(Children))]
    public partial class SettingsContainer : UserControl, INotifyPropertyChanged
    {

        public static readonly DependencyPropertyKey ChildrenProperty = DependencyProperty.RegisterReadOnly(
            nameof(Children),
            typeof(UIElementCollection),
            typeof(SettingsContainer),
            new PropertyMetadata());

        public UIElementCollection Children
        {
            get => (UIElementCollection) GetValue(ChildrenProperty.DependencyProperty);
            private set => SetValue(ChildrenProperty, value);
        }

        private bool? _enabled;

        public bool? Enabled
        {
            get => _enabled;
            set
            {
                _enabled = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DisplayEnabled));
            }
        }

        public bool DisplayEnabled => Enabled ?? true;

        //public static readonly DependencyProperty TitleProperty =
        //    DependencyProperty.Register(nameof(Title), typeof(string), typeof(SettingsContainer));

       // [Bindable(true)]
       public string Title { get; set; } = "Title";
        //{
        //    get => (string) GetValue(TitleProperty);
        //    set => SetValue(TitleProperty, value);
        //}

        public SettingsContainer()
        {
            InitializeComponent();
            Children = ChildrenStackPanel.Children;
            DataContext = this;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
