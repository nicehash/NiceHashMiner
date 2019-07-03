using NHM.Wpf.ViewModels.Settings;
using System.Windows;

namespace NHM.Wpf.Windows
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private readonly SettingsVM _vm;

        public SettingsWindow()
        {
            InitializeComponent();

            if (DataContext is SettingsVM vm)
                _vm = vm;
            else
            {
                _vm = new SettingsVM();
                DataContext = _vm;
            }
        }

        private void TreeView_OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is SettingsBaseVM svm)
                _vm.SelectedPageVM = svm;
        }
    }
}
