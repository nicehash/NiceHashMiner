using NHM.Common;
using SharpCompress.Archives.SevenZip;
using SharpCompress.Archives.Zip;
using SharpCompress.Readers;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace NHMCore.Utils
{
    public static class ArchiveHelpers
    {

        #region Extract

        public static Task<bool> ExtractFileAsync(string password, string zipLocation, string unzipLocation, IProgress<int> progress, CancellationToken stop)
        {
            if (zipLocation.EndsWith(".7z") || SevenZipArchive.IsSevenZipFile(zipLocation))
            {
                return Un7zipFileAsync(password, zipLocation, unzipLocation, progress, stop);
            }
            // default zip
            return UnzipFileAsync(password, zipLocation, unzipLocation, progress, stop);
        }

        private static ZipArchive OpenZipArchive(string password, Stream stream)
        {
            if (string.IsNullOrEmpty(password)) return ZipArchive.Open(stream);
            return ZipArchive.Open(stream, new ReaderOptions { Password = password });
        }

        public static async Task<bool> UnzipFileAsync(string password, string fileLocation, string extractLocation, IProgress<int> progress, CancellationToken stop)
        {
            try
            {
                using (Stream stream = File.OpenRead(fileLocation))
                using (var archive = OpenZipArchive(password, stream))
                using (var reader = archive.ExtractAllEntries())
                {
                    float extractedEntries = 0;
                    float entriesCount = archive.Entries.Count;
                    while (reader.MoveToNextEntry())
                    {
                        extractedEntries += 1;
                        if (!reader.Entry.IsDirectory)
                        {
                            var extractPath = Path.Combine(extractLocation, reader.Entry.Key);
                            var dirPath = Path.GetDirectoryName(extractPath);
                            if (!Directory.Exists(dirPath))
                            {
                                Directory.CreateDirectory(Path.GetDirectoryName(extractPath));
                            }
                            using (var entryStream = reader.OpenEntryStream())
                            using (var fileStream = new FileStream(extractPath, FileMode.Create, FileAccess.Write))
                            {
                                await entryStream.CopyToAsync(fileStream);
                            }
                        }
                        var prog = ((extractedEntries / entriesCount) * 100.0f);
                        progress?.Report((int)prog);
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                Logger.Error("ArchiveHelpers", $"Error occured while unzipping file with: {e.Message}");
                return false;
            }
        }

        private static SevenZipArchive OpenSevenZipArchive(string password, Stream stream)
        {
            if (string.IsNullOrEmpty(password)) return SevenZipArchive.Open(stream);
            return SevenZipArchive.Open(stream, new ReaderOptions { Password = password });
        }

        public static async Task<bool> Un7zipFileAsync(string password, string fileLocation, string extractLocation, IProgress<int> progress, CancellationToken stop)
        {
            try
            {
                using (Stream stream = File.OpenRead(fileLocation))
                using (var archive = OpenSevenZipArchive(password, stream))
                using (var reader = archive.ExtractAllEntries())
                {
                    float extractedEntries = 0;
                    float entriesCount = archive.Entries.Count;
                    while (reader.MoveToNextEntry())
                    {
                        extractedEntries += 1;
                        if (!reader.Entry.IsDirectory)
                        {
                            var extractPath = Path.Combine(extractLocation, reader.Entry.Key);
                            var dirPath = Path.GetDirectoryName(extractPath);
                            if (!Directory.Exists(dirPath))
                            {
                                Directory.CreateDirectory(Path.GetDirectoryName(extractPath));
                            }
                            using (var entryStream = reader.OpenEntryStream())
                            using (var fileStream = new FileStream(extractPath, FileMode.Create, FileAccess.Write))
                            {
                                await entryStream.CopyToAsync(fileStream);
                            }
                        }
                        var prog = ((extractedEntries / entriesCount) * 100.0f);
                        progress?.Report((int)prog);
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                Logger.Error("ArchiveHelpers", $"Error occured while unzipping file with 7zip: {e.Message}");
                return false;
            }
        }
        #endregion Extract

    }
}
