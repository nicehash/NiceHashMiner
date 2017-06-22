using System;
using System.Collections.Generic;
using System.Text;
using MyDownloader.Core;

namespace MyDownloader.Extension.Notifications.Helpers
{
    class SoundHelper
    {
        private static INotificationsExtensionParameters parameters;

        public static void PlayWav(string fileName)
        {
            if (!String.IsNullOrEmpty(fileName))
            {
                new System.Media.SoundPlayer(fileName).Play();
            }
        }

        static void Instance_DownloadRemoved(object sender, DownloaderEventArgs e)
        {
            PlayWav(parameters.DownloadRemovedSoundPath);
        }

        static void Instance_DownloadEnded(object sender, DownloaderEventArgs e)
        {
            PlayWav(parameters.DownloadEndedSoundPath);
        }

        static void Instance_DownloadAdded(object sender, DownloaderEventArgs e)
        {
            PlayWav(parameters.DownloadAddedSoundPath);
        }

        public static void Start(INotificationsExtensionParameters parameters)
        {
            SoundHelper.parameters = parameters;

            DownloadManager.Instance.DownloadAdded += new EventHandler<DownloaderEventArgs>(Instance_DownloadAdded);
            DownloadManager.Instance.DownloadEnded += new EventHandler<DownloaderEventArgs>(Instance_DownloadEnded);
            DownloadManager.Instance.DownloadRemoved += new EventHandler<DownloaderEventArgs>(Instance_DownloadRemoved);
        } 
    }
}
