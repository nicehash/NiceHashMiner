using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;
using SharpCompress.Archives.SevenZip;
using NiceHashMinerLegacy.Common;

namespace NiceHashMiner.Utils
{
    public static class ArchiveHelpers
    {

        #region Extract

        public static Task<bool> ExtractFileAsync(string zipLocation, string unzipLocation, IProgress<int> progress, CancellationToken stop)
        {
            if (zipLocation.EndsWith(".7z"))
            {
                return Un7zipFileAsync(zipLocation, unzipLocation, progress, stop);
            }
            // default zip
            return UnzipFileAsync(zipLocation, unzipLocation, progress, stop);
        }


        public static async Task<bool> UnzipFileAsync(string zipLocation, string unzipLocation, IProgress<int> progress, CancellationToken stop)
        {
            try
            {
                using (var archive = ZipFile.OpenRead(zipLocation))
                {
                    float entriesCount = archive.Entries.Count;
                    float extractedEntries = 0;
                    foreach (var entry in archive.Entries)
                    {
                        if (stop.IsCancellationRequested) break;

                        extractedEntries += 1;
                        var isDirectory = entry.Name == "";
                        if (isDirectory) continue;

                        var prog = ((extractedEntries / entriesCount) * 100.0f);
                        progress?.Report((int)prog);

                        var extractPath = Path.Combine(unzipLocation, entry.FullName);
                        var dirPath = Path.GetDirectoryName(extractPath);
                        if (!Directory.Exists(dirPath))
                        {
                            Directory.CreateDirectory(Path.GetDirectoryName(extractPath));
                        }
                        //entry.ExtractToFile(extractPath, true);

                        using (var zipStream = entry.Open())
                        using (var fileStream = new FileStream(extractPath, FileMode.Create, FileAccess.Write)) // using (var fileStream = new FileStream(extractPath, FileMode.CreateNew))
                        {
                            await zipStream.CopyToAsync(fileStream);
                        }
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                Logger.Info("ArchiveHelpers", $"Error occured while unzipping file: {e.Message}");
                return false;
            }
        }

        public static async Task<bool> Un7zipFileAsync(string fileLocation, string extractLocation, IProgress<int> progress, CancellationToken stop)
        {
            try
            {
                using (Stream stream = File.OpenRead(fileLocation))
                using (var archive = SevenZipArchive.Open(stream))
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
                Logger.Info("ArchiveHelpers", $"Error occured while unzipping file with 7zip: {e.Message}");
                return false;
            }
        }
        #endregion Extract

    }
}
