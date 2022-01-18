using NHM.Common;
using NHMCore.Utils;
using System;
using System.Diagnostics;
using System.Threading;
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

        private void btn_social_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var senderBtn = sender as Button;
                switch (senderBtn.Name)
                {
                    case "btn_facebook":
                        Process.Start("https://www.facebook.com/NiceHash/");
                        e.Handled = true;
                        break;
                    case "btn_instagram":
                        Process.Start("https://www.instagram.com/nicehashmining/");
                        e.Handled = true;
                        break;
                    case "btn_twitter":
                        Process.Start("https://twitter.com/NiceHashMining/");
                        e.Handled = true;
                        break;
                    case "btn_youtube":
                        Process.Start("https://www.youtube.com/c/NiceHashmining");
                        e.Handled = true;
                        break;
                    case "btn_vk":
                        Process.Start("https://vk.com/nicehashmining");
                        e.Handled = true;
                        break;
                    case "btn_github":
                        Process.Start("https://github.com/nicehash");
                        e.Handled = true;
                        break;
                    case "btn_reddit":
                        Process.Start("https://www.reddit.com/r/NiceHash/");
                        e.Handled = true;
                        break;
                    case "btn_discord":
                        Process.Start("https://discord.gg/BQae9ag");
                        e.Handled = true;
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Social", $"Exception occured: {ex.Message}");
            }
        }
    }
}
