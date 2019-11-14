using NHM.Wpf.ViewModels.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NHM.Wpf.Views.SettingsNew
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : UserControl
    {
        public Settings()
        {
            InitializeComponent();
            grid_SettingsGeneral.Visibility = Visibility.Visible;
            grid_SettingsAdvanced.Visibility = Visibility.Hidden;
        }

        private void Btn_GeneralSettings_Click(object sender, RoutedEventArgs e)
        {
            grid_SettingsGeneral.Visibility = Visibility.Visible;
            grid_SettingsAdvanced.Visibility = Visibility.Hidden;
        }

        private void Btn_AdvancedSettings_Click(object sender, RoutedEventArgs e)
        {
            grid_SettingsGeneral.Visibility = Visibility.Hidden;
            grid_SettingsAdvanced.Visibility = Visibility.Visible;
        }
    }
}
