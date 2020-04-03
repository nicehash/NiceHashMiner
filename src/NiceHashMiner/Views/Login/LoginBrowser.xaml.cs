using Microsoft.Toolkit.Win32.UI.Controls.Interop.WinRT;
using System;
using System.Windows;

namespace NiceHashMiner.Views.Login
{
    /// <summary>
    /// Interaction logic for LoginBrowser.xaml
    /// </summary>
    public partial class LoginBrowser : Window
    {
        public LoginBrowser()
        {
            InitializeComponent();
        }

        private void Browser_Loaded(object sender, RoutedEventArgs e)
        {
            browser.Navigate("https://test-dev.nicehash.com/my/login?nhm=1");
            browser.IsScriptNotifyAllowed = true;
            browser.ScriptNotify += Browser_ScriptNotify;

        }

        private void Browser_ScriptNotify(object sender, WebViewControlScriptNotifyEventArgs e)
        {
            Console.WriteLine(e);
            Console.WriteLine(e.Value);
        }
    }
}
