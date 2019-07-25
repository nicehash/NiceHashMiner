using System;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using NHM.Wpf.ViewModels.Settings;
using System.Windows;
using NHM.Wpf.Windows.Common;
using NiceHashMiner;
using NiceHashMiner.Configs;

namespace NHM.Wpf.Windows
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window, IDisposable
    {
        public bool RestartRequired => _vm.RestartRequired;
        public bool DefaultsSet => _vm.DefaultsSet;

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

            Translations.LanguageChanged += Translations_LanguageChanged;
            Translations_LanguageChanged(null, null);
        }

        private void Translations_LanguageChanged(object sender, EventArgs e)
        {
            WindowUtils.Translate(this);
        }

        private void TreeView_OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is SettingsContainerVM cvm)
                _vm.SelectedPageVM = cvm.Children.FirstOrDefault();
            else if (e.NewValue is SettingsBaseVM svm)
                _vm.SelectedPageVM = svm;
        }

        private void CloseButton_OnClick(object sender, RoutedEventArgs e)
        {
            _vm.Revert();
            Close();
        }

        private void SaveButton_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void DefaultsButton_OnClick(object sender, RoutedEventArgs e)
        {
            var confirm = _vm.SetDefaults();
            if (confirm) Close();
        }

        private void SettingsWindow_OnClosing(object sender, CancelEventArgs e)
        {
            if (ApplicationStateManager.BurnCalled) return;

            _vm.Save();
        }

        public void Dispose()
        {
            Translations.LanguageChanged -= Translations_LanguageChanged;
            _vm.Dispose();
        }
    }
}
