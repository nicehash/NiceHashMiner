using System;
using System.Collections.Generic;
using MyDownloader.Core.Extensions;
using System.Windows.Forms;
namespace MyDownloader.App
{
    public interface IApp: IDisposable
    {
        Form MainForm { get; }

        NotifyIcon NotifyIcon { get; }

        List<IExtension> Extensions { get; }

        IExtension GetExtensionByType(Type type);

        void Start(string[] args);
    }
}
