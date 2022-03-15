using NHM.Common;
using NHMCore.Utils;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Navigation;

namespace NiceHashMiner.Views.Settings
{
    /// <summary>
    /// Interaction logic for SettingsAbout.xaml
    /// </summary>
    public partial class SettingsAbout : UserControl
    {
        static System.Windows.Threading.DispatcherTimer timer = new System.Windows.Threading.DispatcherTimer();

        public SettingsAbout()
        {
            InitializeComponent();
            SetHyperlinkToArticle();
            var startTime = DateTime.UtcNow;
            timer.Tick += delegate (object s, EventArgs args)
            {
                var dateTime = DateTime.UtcNow - startTime;
                if(dateTime.TotalDays >= 1) tbl_uptimeText.Text = "Uptime: " + dateTime.Days + "d " + dateTime.Hours + "h " + dateTime.Minutes + "m";
                else tbl_uptimeText.Text = "Uptime: " + dateTime.Hours + "h " + dateTime.Minutes + "m";
            };
            timer.Interval = new TimeSpan(0, 1, 0);
            timer.Start();
        }

        private void SetHyperlinkToArticle()
        {
            try
            {
                var text = new Run("read this article.");
                var article = new Hyperlink(text);
                article.NavigateUri = new Uri(Links.About);
                article.RequestNavigate += ((sender, e) =>
                {
                    Process.Start(e.Uri.ToString());
                });
                tbl_aboutText.Inlines.Add(" ");
                tbl_aboutText.Inlines.Add(article);
            }
            catch (Exception ex)
            {
                Logger.Error("About", $"Error occured: {ex.Message}");
            }
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.ToString());
            e.Handled = true;
        }

        private static (string, bool) UrlForButtonName(string name)
        {
            switch (name)
            {
                case "btn_facebook":
                    return ("https://www.facebook.com/NiceHash/", true);
                case "btn_instagram":
                    return ("https://www.instagram.com/nicehashmining/", true);
                case "btn_twitter":
                    return ("https://twitter.com/NiceHashMining/", true);
                case "btn_youtube":
                    return ("https://www.youtube.com/c/NiceHashmining", true);
                case "btn_vk":
                    return ("https://vk.com/nicehashmining", true);
                case "btn_github":
                    return ("https://github.com/nicehash", true);
                case "btn_reddit":
                    return ("https://www.reddit.com/r/NiceHash/", true);
                case "btn_discord":
                    return ("https://discord.gg/BQae9ag", true);
                default:
                    return ("", false);
            }
        }

        private void btn_social_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var senderBtn = sender as Button;
                var (url, ok) = UrlForButtonName(senderBtn.Name);
                if (ok)
                {
                    Process.Start(url);
                    e.Handled = true;
                }
                else
                {
                    Logger.Error("Social", $"No URL for '{senderBtn.Name}'");
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Social", $"Exception occured: {ex.Message}");
            }
        }
    }
}
