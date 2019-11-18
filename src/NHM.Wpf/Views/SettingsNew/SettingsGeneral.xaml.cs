using NHM.Wpf.ViewModels.Settings;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace NHM.Wpf.Views.SettingsNew
{
    /// <summary>
    /// Interaction logic for SettingsGeneral.xaml
    /// </summary>
    public partial class SettingsGeneral : UserControl
    {
        public SettingsGeneral()
        {
            InitializeComponent();
        }

        private void AddressHyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.AbsoluteUri);
        }
    }
}
