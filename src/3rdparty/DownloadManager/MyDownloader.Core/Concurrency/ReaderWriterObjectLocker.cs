using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace MyDownloader.Core.Concurrency
{
    public class ReaderWriterObjectLocker
    {
        #region BaseReleaser
        class BaseReleaser
        {
            protected ReaderWriterObjectLocker locker;

            public BaseReleaser(ReaderWriterObjectLocker locker)
            {
                this.locker = locker;
            }
        }
        #endregion

        #region ReaderReleaser
        class ReaderReleaser : BaseReleaser, IDisposable
        {
            public ReaderReleaser(ReaderWriterObjectLocker locker)
                : base(locker)
            {
            }

            #region IDisposable Members

            public void Dispose()
            {
                locker.locker.ReleaseReaderLock();
            }

            #endregion
        }
        #endregion

        #region WriterReleaser
        class WriterReleaser : BaseReleaser, IDisposable
        {
            public WriterReleaser(ReaderWriterObjectLocker locker)
                : base(locker)
            {
            }

            #region IDisposable Members

            public void Dispose()
            {
                locker.locker.ReleaseWriterLock();
            }

            #endregion
        }
        #endregion

        #region Fields
        private ReaderWriterLock locker;
        private IDisposable writerReleaser;
        private IDisposable readerReleaser; 
        #endregion

        #region Constructor
        public ReaderWriterObjectLocker()
        {
            // TODO: update to ReaderWriterLockSlim on .net 3.5
            locker = new ReaderWriterLock();

            writerReleaser = new WriterReleaser(this);
            readerReleaser = new ReaderReleaser(this);
        } 
        #endregion

        #region Methods
        public IDisposable LockForRead()
        {
            locker.AcquireReaderLock(-1);

            return readerReleaser;
        }

        public IDisposable LockForWrite()
        {
            locker.AcquireWriterLock(-1);

            return writerReleaser;
        } 
        #endregion
    }
}
