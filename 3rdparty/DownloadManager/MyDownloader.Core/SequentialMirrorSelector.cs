using System;
using System.Collections.Generic;
using System.Text;

namespace MyDownloader.Core
{
    public class SequentialMirrorSelector: IMirrorSelector
    {
        private Downloader downloader;
        private int queryMirrorCount;

        #region IMirrorSelector Members

        public void Init(Downloader downloader)
        {
            queryMirrorCount = 0;
            this.downloader = downloader;
        }

        public ResourceLocation GetNextResourceLocation()
        {
            if (downloader.Mirrors == null || downloader.Mirrors.Count == 0)
            {
                return this.downloader.ResourceLocation;
            }

            lock (downloader.Mirrors)
            {
                if (queryMirrorCount >= downloader.Mirrors.Count)
                {
                    queryMirrorCount = 0;

                    return this.downloader.ResourceLocation;
                }

                return downloader.Mirrors[queryMirrorCount++];
            }
        }

        #endregion
    }
}
