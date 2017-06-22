using System;
using System.Collections.Generic;
using System.Text;

namespace MyDownloader.Core.Common
{
    public static class ByteFormatter
    {
        private const long KB = 1024;
        private const long MB = KB * 1024;
        private const long GB = MB * 1024;

        private const string BFormatPattern = "{0} b";
        private const string KBFormatPattern = "{0:0} KB";
        private const string MBFormatPattern = "{0:0,###} MB";
        private const string GBFormatPattern = "{0:0,###.###} GB";

        public static string ToString(long size)
        {
            if (size < KB)
            {
                return String.Format(BFormatPattern, size);
            }
            else if (size >= KB && size < MB)
            {
                return String.Format(KBFormatPattern, size / 1024.0f);
            }
            else if (size >= MB && size < GB)
            {
                return String.Format(MBFormatPattern, size / 1024.0f);
            }
            else // size >= GB
            {
                return String.Format(GBFormatPattern, size / 1024.0f);
            }
        }
    }
}
