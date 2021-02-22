using NHM.Common;
using System;
using System.IO;
using System.Windows;

namespace NiceHashMiner.Views
{
    /// <summary>
    /// Interaction logic for ManuallyEnterBTCTutorial.xaml
    /// </summary>
    public partial class ManuallyEnterBTCTutorial : Window
    {
        public ManuallyEnterBTCTutorial()
        {
            InitializeComponent();
        }

        private void LoginHowToGif_MediaEnded(object sender, RoutedEventArgs e)
        {
            try
            {
                LoginHowToGif.Position = TimeSpan.FromMilliseconds(1);
            }
            catch (Exception ex)
            {
                Logger.Error("ManuallyEnterBTCTutorial", $"LoginHowToGif_MediaEnded error: {ex}.");
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var gifPath = Paths.AppRootPath("assets", "enter_BTC_manually.gif");
                if (File.Exists(gifPath))
                {
                    LoginHowToGif.Source = new Uri(gifPath);
                }
                else
                {
                    Close();
                }
            }
            catch (Exception ex)
            {
                Logger.Error("ManuallyEnterBTCTutorial", $"Window_Loaded error: {ex}.");
                Close();
            }

        }
    }
}
