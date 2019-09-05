using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using NHM.Wpf.Properties;

namespace NHM.Wpf.Views
{
    /// <summary>
    /// Interaction logic for ChooseLanguageWindow.xaml
    /// </summary>
    public partial class ChooseLanguageWindow : Window, INotifyPropertyChanged
    {
        private bool _canClose = false;

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
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void OkButton_OnClick(object sender, RoutedEventArgs e)
        {
            _canClose = true;
            Close();
        }

        private void ChooseLanguageWindow_OnClosing(object sender, CancelEventArgs e)
        {
            if (!_canClose) e.Cancel = true;
        }
    }
}
