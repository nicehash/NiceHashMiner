using NHM.Wpf.Views.Common;
using NHM.Wpf.Views.Common.NHBase;
using NHMCore.Configs;
using NHMCore.Utils;
using System.Diagnostics;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;

namespace NHM.Wpf.Views
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : BaseDialogWindow
    {
        public LoginWindow()
        {
            InitializeComponent();
            WindowUtils.Translate(this);
        }

        private void CheckBoxMode_Checked(object sender, RoutedEventArgs e)
        {
            GUISettings.Instance.DisplayTheme = "Dark";
            ThemeSetterManager.SetTheme(false);
        }

        private void CheckBoxMode_Unchecked(object sender, RoutedEventArgs e)
        {
            GUISettings.Instance.DisplayTheme = "Light";
            ThemeSetterManager.SetTheme(true);
        }

        private void Register_OnClick(object sender, RoutedEventArgs e)
        {
            Process.Start(Links.Register);
        }

        private void ManuallyEnterBtc_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
