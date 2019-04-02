using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiceHashMiner.MinersDownloader
{
    internal class DownloadSetup
    {
        public DownloadSetup(string url, string dlName, string inFolderName)
        {
            BinsDownloadUrl = url;
            BinsZipLocation = dlName;
            ZipedFolderName = inFolderName;
        }

        public readonly string BinsDownloadUrl;
        public readonly string BinsZipLocation;
        public readonly string ZipedFolderName;
    }
}
