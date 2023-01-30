using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
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

        public static async Task<(bool success, string downloadedFilePath)> DownloadFileAsync(string url, string downloadFileRootPath, string fileNameNoExtension, IProgress<int> progress, CancellationToken stop)
        {
            var downloadFileLocation = GetDownloadFilePath(downloadFileRootPath, fileNameNoExtension, GetFileExtension(url));
            using var file = new FileStream(downloadFileLocation, FileMode.Create, FileAccess.Write, FileShare.None);
            using var client = new HttpClient();
            using var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            var contentLength = response.Content.Headers.ContentLength;
            using var download = await response.Content.ReadAsStreamAsync();
            if (!contentLength.HasValue) return (false, downloadFileLocation);
            var progressWrapper = new Progress<int>(totalBytes => progress.Report(GetProgressPercentage(totalBytes, contentLength.Value)));
            await download.CopyToAsync(file, 81920, progressWrapper, stop);

            int GetProgressPercentage(float totalBytes, float currentBytes) => (int)((totalBytes / currentBytes) * 100f);

            return (true, downloadFileLocation);
        }

        static async Task CopyToAsync(this Stream source, Stream destination, int bufferSize, IProgress<int> progress = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (bufferSize < 0)
                throw new ArgumentOutOfRangeException(nameof(bufferSize));
            if (source is null)
                throw new ArgumentNullException(nameof(source));
            if (!source.CanRead)
                throw new InvalidOperationException($"'{nameof(source)}' is not readable.");
            if (destination == null)
                throw new ArgumentNullException(nameof(destination));
            if (!destination.CanWrite)
                throw new InvalidOperationException($"'{nameof(destination)}' is not writable.");

            var buffer = new byte[bufferSize];
            int totalBytesRead = 0;
            int bytesRead;
            while ((bytesRead = await source.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false)) != 0)
            {
                await destination.WriteAsync(buffer, 0, bytesRead, cancellationToken).ConfigureAwait(false);
                totalBytesRead += bytesRead;
                progress?.Report(totalBytesRead);
            }
        }
    }
}
