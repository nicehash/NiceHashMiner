using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace MyDownloader.Core.Concurrency
{
    public class ObjectLocker : IDisposable
    {
        #region Fields

        private object obj; 

        #endregion

        #region Constructor

        public ObjectLocker(object obj)
        {
            this.obj = obj;
            Monitor.Enter(this.obj);
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            Monitor.Exit(this.obj);
        }

        #endregion
    } 
}
