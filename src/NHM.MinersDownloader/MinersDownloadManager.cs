using NHM.Common;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace NHM.MinersDownloader
{
    public static class MinersDownloadManager
    {
        static MinersDownloadManager()
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
                                                   | SecurityProtocolType.Tls11
                                                   | SecurityProtocolType.Tls12;
        }

        public static async Task<(bool success, string downloadedFilePath)> DownloadFileAsync(string url, string downloadFileRootPath, string fileNameNoExtension, IProgress<int> progress, CancellationToken stop)
        {
            return await DownloadFileWebClientAsync(url, downloadFileRootPath, fileNameNoExtension, progress, stop);
        }

        internal static string GetFileExtension(string urlOrNameIn)
        {
            string urlOrName = "";
            try
            {
                urlOrName = urlOrName.Replace(new Uri(urlOrNameIn).GetLeftPart(UriPartial.Path), "");
            }
            catch
            { }
            var dotAt = urlOrName.LastIndexOf('.');
            var invalidChars = Path.GetInvalidPathChars().Any(c => urlOrName.Contains(c));
            if (dotAt < 0 || invalidChars)
            {
                try
                {
                    var ext = urlOrNameIn.Substring(urlOrNameIn.LastIndexOf('.') + 1);
                    return ext;
                }
                catch (Exception)
                { }
                return "exe";
            }
            var extSize = urlOrName.Length - dotAt - 1;
            return urlOrName.Substring(urlOrName.Length - extSize);
        }

        internal static string GetDownloadFilePath(string downloadFileRootPath, string fileNameNoExtension, string fileExtension)
        {
            return Path.Combine(downloadFileRootPath, $"{fileNameNoExtension}.{fileExtension}");
        }

        public static async Task<(bool success, string downloadedFilePath)> DownloadFileWebClientAsync(string url, string downloadFileRootPath, string fileNameNoExtension, IProgress<int> progress, CancellationToken stop)
        {
            var downloadFileLocation = GetDownloadFilePath(downloadFileRootPath, fileNameNoExtension, GetFileExtension(url));
            var downloadStatus = false;
            using (var client = new WebClient())
            {
                client.Proxy = null;
                client.DownloadProgressChanged += (s, e1) =>
                {
                    progress?.Report(e1.ProgressPercentage);
                };
                client.DownloadFileCompleted += (s, e) =>
                {
                    downloadStatus = !e.Cancelled && e.Error == null;
                };
                stop.Register(client.CancelAsync);
                // Starts the download
                await client.DownloadFileTaskAsync(new Uri(url), downloadFileLocation);
            }
            return (downloadStatus, downloadFileLocation);
        }
    }
}
