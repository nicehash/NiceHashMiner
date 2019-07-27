using NHM.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NHM.Wpf.ViewModels
{
    public class BaseVM : NotifyChangedBase
    {
        protected static IEnumerable<T> GetEnumValues<T>() where T : struct
        {
            return Enum.GetValues(typeof(T)).Cast<T>();
        }
    }
}
