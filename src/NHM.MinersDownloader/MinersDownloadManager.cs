using CG.Web.MegaApiClient;
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
                   | SecurityProtocolType.Tls12
                   | SecurityProtocolType.Ssl3;
        }

        public static async Task<(bool success, string downloadedFilePath)> DownloadFileAsync(string url, string downloadFileRootPath, string fileNameNoExtension, IProgress<int> progress, CancellationToken stop)
        {
            // TODO switch for mega upload
            if (IsMegaUpload(url))
            {
                return await DownlaodWithMegaAsync(url, downloadFileRootPath, fileNameNoExtension, progress, stop);
            }
            return await DownloadFileWebClientAsync(url, downloadFileRootPath, fileNameNoExtension, progress, stop);
        }

        internal static bool IsMegaUpload(string url)
        {
            return url.Contains("mega.co.nz") || url.Contains("mega.nz");
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
            using (var client = new System.Net.WebClient())
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

        #region Mega

        internal static bool IsMegaURLFolder(string url)
        {
            return url.Contains("/#F!") || url.Contains("/folder");
        }
        internal static Task<(bool success, string downloadedFilePath)> DownlaodWithMegaAsync(string url, string downloadFileRootPath, string fileNameNoExtension, IProgress<int> progress, CancellationToken stop)
        {
            if (IsMegaURLFolder(url))
            {
                return DownlaodWithMegaFromFolderAsync(url, downloadFileRootPath, fileNameNoExtension, progress, stop);
            }
            return DownlaodWithMegaFileAsync(url, downloadFileRootPath, fileNameNoExtension, progress, stop);
        }


        // non folder
        internal static async Task<(bool success, string downloadedFilePath)> DownlaodWithMegaFileAsync(string url, string downloadFileRootPath, string fileNameNoExtension, IProgress<int> progress, CancellationToken stop)
        {
            var client = new MegaApiClient();
            var downloadFileLocation = "";
            try
            {
                client.LoginAnonymous();
                Uri fileLink = new Uri(url);
                INodeInfo node = await client.GetNodeFromLinkAsync(fileLink);
                Console.WriteLine($"Downloading {node.Name}");
                var doubleProgress = new Progress<double>((p) => progress?.Report((int)p));
                downloadFileLocation = GetDownloadFilePath(downloadFileRootPath, fileNameNoExtension, GetFileExtension(node.Name));
                await client.DownloadFileAsync(fileLink, downloadFileLocation, doubleProgress, stop);
            }
            catch (Exception e)
            {
                Logger.Error("MinersDownloadManager", $"MegaFile error: {e.Message}");
            }
            finally
            {
                client.Logout();
            }

            var success = File.Exists(downloadFileLocation);
            return (success, downloadFileLocation);
        }

        internal static async Task<(bool success, string downloadedFilePath)> DownlaodWithMegaFromFolderAsync(string url, string downloadFileRootPath, string fileNameNoExtension, IProgress<int> progress, CancellationToken stop)
        {
            var client = new MegaApiClient();
            var downloadFileLocation = "";
            try
            {
                client.LoginAnonymous();
                var folderUrl = new Uri(url.Split('?').FirstOrDefault());
                var nodes = await client.GetNodesFromLinkAsync(folderUrl);
                var node = nodes.FirstOrDefault(n => url.Contains(n.Id));
                //Console.WriteLine($"Downloading {node.Name}");
                var doubleProgress = new Progress<double>((p) => progress?.Report((int)p));
                downloadFileLocation = GetDownloadFilePath(downloadFileRootPath, fileNameNoExtension, GetFileExtension(node.Name));
                await client.DownloadFileAsync(node, downloadFileLocation, doubleProgress, stop);
            }
            catch (Exception e)
            {
                Logger.Error("MinersDownloadManager", $"MegaFolder error: {e.Message}");
            }
            finally
            {
                client.Logout();
            }

            var success = File.Exists(downloadFileLocation);
            return (success, downloadFileLocation);
        }
        #endregion Mega 

    }
}
