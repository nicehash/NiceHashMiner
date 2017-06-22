using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace MyDownloader.Extension.WindowsIntegration.ClipboardMonitor
{
    public interface IClipboardDataHandler
    {
        void HandleClipboardData(IDataObject data);
    }
}
