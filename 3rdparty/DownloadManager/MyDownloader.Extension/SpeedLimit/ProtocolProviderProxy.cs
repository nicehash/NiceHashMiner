using System;
using System.Collections.Generic;
using System.Text;
using MyDownloader.Core;

namespace MyDownloader.Extension.SpeedLimit
{
    public class ProtocolProviderProxy: IProtocolProvider
    {
        private IProtocolProvider proxy;
        private SpeedLimitExtension speedLimit;

        #region IProtocolProvider Members

        public void Initialize(Downloader downloader)
        {
            proxy.Initialize(downloader);
        }

        public System.IO.Stream CreateStream(ResourceLocation rl, long initialPosition, long endPosition)
        {
            return new LimitedRateStreamProxy(proxy.CreateStream(rl, initialPosition, endPosition), speedLimit);
        }

        public RemoteFileInfo GetFileInfo(ResourceLocation rl, out System.IO.Stream stream)
        {
            RemoteFileInfo result = proxy.GetFileInfo(rl, out stream);

            if (stream != null)
            {
                stream = new LimitedRateStreamProxy(stream, speedLimit);
            }

            return result;
        }

        #endregion

        public ProtocolProviderProxy(IProtocolProvider proxy, SpeedLimitExtension speedLimit)
        {
            this.proxy = proxy;
            this.speedLimit = speedLimit;
        }
    }
}
