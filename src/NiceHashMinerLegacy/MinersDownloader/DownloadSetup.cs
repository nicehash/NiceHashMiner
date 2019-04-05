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

        public string BinsDownloadUrl { get; set; }
        public string BinsZipLocation { get; set; }
        public string ZipedFolderName { get; set; }
    }
}
