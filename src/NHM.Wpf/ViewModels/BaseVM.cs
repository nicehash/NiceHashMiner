using NHM.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NHM.Wpf.ViewModels
{
    public class BaseVM : NotifyChangedBase, IDisposable
    {
        protected static IEnumerable<T> GetEnumValues<T>() where T : struct
        {
            return Enum.GetValues(typeof(T)).Cast<T>();
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        { }
    }
}
