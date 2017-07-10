using System;
using System.Collections.Generic;
using System.Text;
using MyDownloader.Core;
using MyDownloader.Core.UI;
using System.Windows.Forms;

namespace MyDownloader.Extension.Notifications.Helpers
{
    class BalloonHelper
    {
        private static INotificationsExtensionParameters parameters;

        static void Instance_DownloadEnded(object sender, DownloaderEventArgs e)
        {
            if (parameters.ShowBallon &&
                AppManager.Instance.Application.NotifyIcon.Visible)
            {
                AppManager.Instance.Application.NotifyIcon.ShowBalloonTip(
                    parameters.BallonTimeout,
                    AppManager.Instance.Application.MainForm.Text,
                    String.Format("Download finished: {0}", e.Downloader.LocalFile),
                    ToolTipIcon.Info);
             }
        }

        public static void Start(INotificationsExtensionParameters parameters)
        {
            BalloonHelper.parameters = parameters;

            DownloadManager.Instance.DownloadEnded += new EventHandler<DownloaderEventArgs>(Instance_DownloadEnded);
        }
    }
}
