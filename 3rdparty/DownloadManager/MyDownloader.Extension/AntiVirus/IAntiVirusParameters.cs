using System;
using System.Collections.Generic;
using System.Text;

namespace MyDownloader.Extension.AntiVirus
{
    public interface IAntiVirusParameters
    {
        bool CheckFileWithAV { get; set; }

        string AVFileName { get; set; }

        string FileTypes { get; set; }

        string AVParameter { get; set; }
    }
}
