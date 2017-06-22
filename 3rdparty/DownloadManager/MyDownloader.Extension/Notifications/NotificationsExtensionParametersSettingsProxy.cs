using System;
using System.Collections.Generic;
using System.Text;

namespace MyDownloader.Extension.Notifications
{
    internal class NotificationsExtensionParametersSettingsProxy: INotificationsExtensionParameters
    {
        #region INotificationsExtensionParameters Members

        public string DownloadAddedSoundPath
        {
            get
            {
                return Settings.Default.DownloadAddedSound;
            }
            set
            {
                Settings.Default.DownloadAddedSound = value;
            }
        }

        public string DownloadRemovedSoundPath
        {
            get
            {
                return Settings.Default.DownloadRemovedSound;
            }
            set
            {
                Settings.Default.DownloadRemovedSound = value;
            }
        }

        public string DownloadEndedSoundPath
        {
            get
            {
                return Settings.Default.DownloadEndedSound;
            }
            set
            {
                Settings.Default.DownloadEndedSound = value;
            }
        }

        public bool ShowBallon
        {
            get
            {
                return Settings.Default.ShowBallon;
            }
            set
            {
                Settings.Default.ShowBallon = value;
            }
        }

        public int BallonTimeout
        {
            get
            {
                return Settings.Default.BallonTimeout;
            }
            set
            {
                Settings.Default.BallonTimeout = value;
            }
        }

        #endregion
    }
}
