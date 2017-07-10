using System;
using System.Collections.Generic;
using System.Text;

namespace MyDownloader.Core
{
    public enum SegmentState
    {
        Idle,
        Connecting,
        Downloading,
        Paused,
        Finished,
        Error,
    }
}
