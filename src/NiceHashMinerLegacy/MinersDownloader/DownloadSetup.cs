using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiceHashMiner.MinersDownloader
{
    internal class DownloadSetup
    {
        public DownloadSetup(string url, string dlName)
        {
            BinsDownloadUrl = url;
            BinsZipLocation = dlName;
        }

        public string BinsDownloadUrl { get; set; }
        public string BinsZipLocation { get; set; }
    }
}
