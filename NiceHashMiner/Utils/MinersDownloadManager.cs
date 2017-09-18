using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.IO.Compression;
using System.Windows.Forms;
using NiceHashMiner.Interfaces;
using System.Threading;
using NiceHashMiner.Configs;
using NiceHashMiner.Devices;

namespace NiceHashMiner.Utils {
    public static class MinersDownloadManager {
        public static DownloadSetup StandardDlSetup = new DownloadSetup(
            "http://github.com/NiceHash/NiceHashMinerLegacy/releases/download/1.8.1.3-Pre/bin_1_8_1_3.zip",
            "bins.zip",
            "bin");

        public static DownloadSetup ThirdPartyDlSetup = new DownloadSetup(
            "http://github.com/NiceHash/NiceHashMinerLegacy/releases/download/1.8.1.3-Pre/bin_3rdparty_1_8_1_3.zip",
            "bins_3rdparty.zip",
            "bin_3rdparty");
    }
}
