using NHM.Wpf.ViewModels;
using NHMCore.Configs;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NHM.Wpf.Views.SettingsNew
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : UserControl
    {
        private static bool _isGeneral = true;

        public Settings()
        {
            InitializeComponent();
            OnScreenChange();
        }

        protected void OnScreenChange()
        {
            if (_isGeneral)
            {
                grid_SettingsGeneral.Visibility = Visibility.Visible;
                spt_general.Visibility = Visibility.Visible;
                btn_SettingsGeneral.Foreground = (Brush)FindResource("Brushes.Dark.Basic.MainColor");

                grid_SettingsAdvanced.Visibility = Visibility.Hidden;
                spt_advanced.Visibility = Visibility.Hidden;
                btn_SettingsAdvanced.Foreground = (Brush)FindResource("Gray2ColorBrush");
            }
            else
            {
                grid_SettingsGeneral.Visibility = Visibility.Hidden;
                spt_general.Visibility = Visibility.Hidden;
                btn_SettingsGeneral.Foreground = (Brush)FindResource("Gray2ColorBrush");


                grid_SettingsAdvanced.Visibility = Visibility.Visible;
                spt_advanced.Visibility = Visibility.Visible;
                btn_SettingsAdvanced.Foreground = (Brush)FindResource("Brushes.Dark.Basic.MainColor");
            }
        }

        private void Btn_GeneralSettings_Click(object sender, RoutedEventArgs e)
        {
            _isGeneral = true;
            OnScreenChange();
        }

        private void Btn_AdvancedSettings_Click(object sender, RoutedEventArgs e)
        {
            _isGeneral = false;
            OnScreenChange();
        }

        private void Btn_default_Click(object sender, RoutedEventArgs e)
        {
            ConfigManager.SetDefaults();
        }
    }
}
