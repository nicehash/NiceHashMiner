using System;
using System.Collections.Generic;
using System.Text;
using MyDownloader.Core.Extensions;
using System.Windows.Forms;

namespace MyDownloader.Core.UI
{
    public class CoreUIExtention: IUIExtension
    {
        #region IUIExtension Members

        public Control[] CreateSettingsView()
        {
            return new Control[] { new Connection(), new DownloadFolder() };
        }

        public void PersistSettings(System.Windows.Forms.Control[] settingsView)
        {
            Connection connection = (Connection)settingsView[0];
            DownloadFolder downloadFolder = (DownloadFolder)settingsView[1];

            Settings.Default.MaxRetries = connection.MaxRetries;
            Settings.Default.MinSegmentSize = connection.MinSegmentSize;
            Settings.Default.RetryDelay = connection.RetryDelay;
            Settings.Default.MaxSegments = connection.MaxSegments;

            Settings.Default.DownloadFolder = downloadFolder.Folder;

            Settings.Default.Save();
        }

        #endregion
    }
}
