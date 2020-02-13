using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace MyDownloader.Core.Instrumentation
{
    /// <summary>
    /// MyStopwatch is a debbuging tool to count the amout of time used by an operation.
    /// </summary>
    public class MyStopwatch : IDisposable
    {
        #region Fields
#pragma warning disable 0169
        private Stopwatch internalStopwatch;
        private string name;
#pragma warning restore 0169

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes the MyStopwatch
        /// </summary>
        /// <param name="name">The name of MyStopwatch</param>
        public MyStopwatch(string name)
        {
#if DEBUG
            this.name = name;
            internalStopwatch = new Stopwatch();
            internalStopwatch.Start();
#endif
        }

        #endregion

        #region Methods

        /// <summary>
        /// Disposes the MyStopwatch and writes into debug the amount of time used on the operation.
        /// </summary>
        public void Dispose()
        {
#if DEBUG
            internalStopwatch.Stop();
            Debug.WriteLine(name + ": " + internalStopwatch.Elapsed);
#endif
        }

        #endregion
    }
}
