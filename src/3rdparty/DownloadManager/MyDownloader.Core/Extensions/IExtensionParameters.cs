using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace MyDownloader.Core.Extensions
{
    public interface IExtensionParameters
    {
        event PropertyChangedEventHandler ParameterChanged;
    }
}
