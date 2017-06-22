using System;
using System.Collections.Generic;
using System.Text;
using MyDownloader.Core;

namespace MyDownloader.Core.Extensions
{
    public interface IExtension
    {
        string Name { get; }

        IUIExtension UIExtension { get; }
    }
}
