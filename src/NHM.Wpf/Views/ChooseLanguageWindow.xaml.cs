using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using NHM.Wpf.Properties;
using NHM.Wpf.Views.Common;

namespace NHM.Wpf.Views
{
    /// <summary>
    /// Interaction logic for ChooseLanguageWindow.xaml
    /// </summary>
    public partial class ChooseLanguageWindow : Window, INotifyPropertyChanged
    {
        private int _selectedLangIndex = 0;
        public int SelectedLangIndex
        {
            get => _selectedLangIndex;
            set
            {
                _selectedLangIndex = value;
                OnPropertyChanged();
            }
        }

        private IEnumerable<string> _langNames;
        public IEnumerable<string> LangNames
        {
            get => _langNames;
            set
            {
                _langNames = value;
                OnPropertyChanged();
            }
        }

        public ChooseLanguageWindow()
        {
            InitializeComponent();
            WindowUtils.InitWindow(this);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void OkButton_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            WindowUtils.SetForceSoftwareRendering(this);
        }

        private void Window_OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            WindowUtils.Window_OnClosing(this);
        }
    }
}
