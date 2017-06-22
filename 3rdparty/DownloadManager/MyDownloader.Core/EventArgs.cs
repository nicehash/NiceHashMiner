using System;
using System.Collections.Generic;
using System.Text;

namespace MyDownloader.Core
{
    #region ResolvingProtocolProviderEventArgs
    public class ResolvingProtocolProviderEventArgs : EventArgs
    {
        #region Fields

        private IProtocolProvider provider;
        private string url;

        #endregion

        #region Constructor

        public ResolvingProtocolProviderEventArgs(IProtocolProvider provider,
            string url)
        {
            this.url = url;
            this.provider = provider;
        }

        #endregion

        #region Properties

        public string URL
        {
            get { return url; }
        }

        public IProtocolProvider ProtocolProvider
        {
            get { return provider; }
            set { provider = value; }
        }

        #endregion
    } 
    #endregion

    #region DownloaderEventArgs
    public class DownloaderEventArgs : EventArgs
    {
        #region Fields

        private Downloader downloader;
        private bool willStart;

        #endregion

        #region Constructor

        public DownloaderEventArgs(Downloader download)
        {
            this.downloader = download;
        }

        public DownloaderEventArgs(Downloader download, bool willStart): this(download)
        {
            this.willStart = willStart;
        }

        #endregion

        #region Properties

        public Downloader Downloader
        {
            get { return downloader; }
        }

        public bool WillStart
        {
            get { return willStart; }
        }	

        #endregion
    } 
    #endregion

    #region SegmentEventArgs
    public class SegmentEventArgs : DownloaderEventArgs
    {
        #region Fields

        private Segment segment;

        #endregion

        #region Constructor

        public SegmentEventArgs(Downloader d, Segment segment)
            : base(d)
        {
            this.segment = segment;
        }

        #endregion

        #region Properties

        public Segment Segment
        {
            get { return segment; }
            set { segment = value; }
        }

        #endregion
    } 
    #endregion
}
