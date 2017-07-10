using System;
using System.Collections.Generic;
using System.Text;
using MyDownloader.Core.Extensions;
using System.Windows.Forms;
using MyDownloader.Extension.Notifications.UI;

namespace MyDownloader.Extension.Notifications
{
    public class NotificationsUIExtension : IUIExtension
    {
        #region IUIExtension Members

        public Control[] CreateSettingsView()
        {
            return new Control[] { new SoundsOptions(), new XPBalloonOptions() };
        }

        public void PersistSettings(Control[] settingsView)
        {
            SoundsOptions sounds = (SoundsOptions)settingsView[0];
            XPBalloonOptions ballon = (XPBalloonOptions)settingsView[1];

            Settings.Default.DownloadAddedSound = sounds.DownloadAdded;
            Settings.Default.DownloadRemovedSound = sounds.DownloadRemoved;
            Settings.Default.DownloadEndedSound = sounds.DownloadEnded;

            Settings.Default.ShowBallon = ballon.ShowBallon;
            Settings.Default.BallonTimeout = ballon.BallonTimeout;

            Settings.Default.Save();
        }

        #endregion
    }
}
