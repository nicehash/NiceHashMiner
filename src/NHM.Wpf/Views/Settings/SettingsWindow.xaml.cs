using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using NHM.Wpf.ViewModels.Settings;
using NHM.Wpf.Views.Common;
using NHMCore;

namespace NHM.Wpf.Views.Settings
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

            _vm = this.AssertViewModel<SettingsVM>();

            Translations.LanguageChanged += Translations_LanguageChanged;
            WindowUtils.InitWindow(this);
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
            var result = MessageBox.Show(
                Translations.Tr(
                    "Are you sure you would like to set everything back to defaults? This will restart {0} automatically.",
                    NHMProductInfo.Name),
                Translations.Tr("Set default settings?"),
                MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            _vm.SetDefaults();
            Close();
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            WindowUtils.SetForceSoftwareRendering(this);
        }
    }
}
