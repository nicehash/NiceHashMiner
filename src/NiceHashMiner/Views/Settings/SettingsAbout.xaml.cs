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
                    Helpers.VisitUrlLink(e.Uri.ToString());
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
            Helpers.VisitUrlLink(e.Uri.ToString());
            e.Handled = true;
        }

        private static (string, bool) UrlForButtonName(string name)
        {
            var url = name switch
            {
                "btn_facebook" => "https://www.facebook.com/NiceHash/",
                "btn_instagram" => "https://www.instagram.com/nicehash_official/",
                "btn_twitter" => "https://twitter.com/NiceHashMining/",
                "btn_youtube" => "https://www.youtube.com/c/NiceHash_Official",
                "btn_vk" => "https://vk.com/nicehashmining",
                "btn_github" => "https://github.com/nicehash",
                "btn_reddit" => "https://www.reddit.com/r/NiceHash/",
                "btn_discord" => "https://discord.gg/nicehash",
                _ => null,
            };
            return (url, url != null);
        }

        private void btn_social_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is not Button senderBtn) return;
                var (url, ok) = UrlForButtonName(senderBtn.Name);
                if (ok)
                {
                    Helpers.VisitUrlLink(url);
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
