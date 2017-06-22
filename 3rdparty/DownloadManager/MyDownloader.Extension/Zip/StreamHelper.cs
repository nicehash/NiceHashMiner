using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace MyDownloader.Extension.Zip
{
    internal static class StreamHelper
    {
        internal static int ReadAll(byte[] bb, int p, int sst, Stream s)
        {
            int ss = 0;
            while (ss < sst)
            {
                int r = s.Read(bb, p, sst - ss);
                if (r <= 0)
                    return ss;
                ss += r;
                p += r;
            }
            return ss;
        }
    }
}
