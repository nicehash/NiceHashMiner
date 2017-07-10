using System;
using System.Collections.Generic;
using System.Text;
using MyDownloader.Core.Extensions;

namespace MyDownloader.Extension.AutoDownloads
{
    public interface IAutoDownloadsParameters : IExtensionParameters
    {
        int MaxJobs { get; set; }

        bool WorkOnlyOnSpecifiedTimes { get; set; }

        string TimesToWork { get; set; }

        double MaxRateOnTime { get; set; }

        bool AutoStart { get; set; }
    }
}