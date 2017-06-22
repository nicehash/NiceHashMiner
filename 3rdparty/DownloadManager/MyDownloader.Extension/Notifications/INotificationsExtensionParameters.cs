using System;
using System.Collections.Generic;
using System.Text;

namespace MyDownloader.Extension.Notifications
{
    public interface INotificationsExtensionParameters
    {
        string DownloadAddedSoundPath { get; set; }

        string DownloadRemovedSoundPath { get; set; }

        string DownloadEndedSoundPath { get; set; }

        bool ShowBallon { get; set; }

        int BallonTimeout { get; set; }
    }
}
