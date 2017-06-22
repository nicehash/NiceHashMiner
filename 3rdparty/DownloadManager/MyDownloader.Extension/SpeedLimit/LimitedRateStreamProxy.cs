using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;

namespace MyDownloader.Extension.SpeedLimit
{
    public class LimitedRateStreamProxy: Stream
    {
        private Stream proxy;
        private SpeedLimitExtension speedLimit;

        #region Stream

        public override bool CanRead
        {
            get { return proxy.CanRead; }
        }

        public override bool CanSeek
        {
            get { return proxy.CanSeek; }
        }

        public override bool CanWrite
        {
            get { return proxy.CanWrite; }
        }

        public override void Flush()
        {
            proxy.Flush();
        }

        public override long Length
        {
            get { return proxy.Length; }
        }

        public override long Position
        {
            get
            {
                return proxy.Position;
            }
            set
            {
                proxy.Position = value;
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            speedLimit.WaitFor();

            return proxy.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return proxy.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            proxy.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            proxy.Write(buffer, offset, count);
        } 

        #endregion

        public LimitedRateStreamProxy(Stream proxy, SpeedLimitExtension speedLimit)
        {
            this.speedLimit = speedLimit;
            this.proxy = proxy;
        }
    }
}
