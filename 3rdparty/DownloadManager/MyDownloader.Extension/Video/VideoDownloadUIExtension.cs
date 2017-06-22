using System;
using System.Collections.Generic;
using System.Text;
using MyDownloader.Core.Extensions;
using MyDownloader.Core;

namespace MyDownloader.Extension.Video
{
    public class VideoDownloadUIExtension: IUIExtension
    {
        #region IUIExtension Members

        public System.Windows.Forms.Control[] CreateSettingsView()
        {
            return null;
        }

        public void PersistSettings(System.Windows.Forms.Control[] settingsView)
        {
        }

        #endregion


        public void ShowNewVideoDialog(string url, bool modal)
        {
            if (modal)
            {
                using (UI.NewVideoDownload videoDownload = new UI.NewVideoDownload())
                {
                    if (!String.IsNullOrEmpty(url))
                    {
                        videoDownload.DownloadLocation = ResourceLocation.FromURL(url);
                    }
                    videoDownload.ShowDialog();
                }
            }
            else
            {
                UI.NewVideoDownload videoDownload = new UI.NewVideoDownload();

                if (!String.IsNullOrEmpty(url))
                {
                    videoDownload.DownloadLocation = ResourceLocation.FromURL(url);
                }

                videoDownload.ShowInTaskbar = true;
                videoDownload.MinimizeBox = true;
                videoDownload.Show();
                videoDownload.Focus();
                videoDownload.TopMost = true;
            }
        }
    }
}
