using NHM.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NiceHashMiner.ViewModels
{
    /// <summary>
    /// Base ViewModel class with optional title.
    /// </summary>
    public abstract class BaseVM : NotifyChangedBase, IDisposable
    {
        public string Title { get; }

        protected BaseVM(string title)
        {
            Title = title;
        }

        protected BaseVM()
        { }

        // Quick helper method for type-safe enum enumeration
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
