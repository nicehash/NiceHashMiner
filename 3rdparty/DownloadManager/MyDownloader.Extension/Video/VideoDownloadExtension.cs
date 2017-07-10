using System;
using System.Collections.Generic;
using System.Text;
using MyDownloader.Core.Extensions;
using MyDownloader.Core;
using MyDownloader.Extension.Video.Impl;
using System.Windows.Forms;

namespace MyDownloader.Extension.Video
{
    public class VideoDownloadExtension: IExtension
    {
        List<VideoDownloadHandler> handlers;

        #region IExtension Members

        public string Name
        {
            get { return "Video Downloader"; }
        }

        public IUIExtension UIExtension
        {
            get { return new VideoDownloadUIExtension(); }
        }

        #endregion

        #region Methods

        public VideoDownloadHandler GetHandlerByURL(string url)
        {
            for (int i = 0; i < handlers.Count; i++)
            {
                if (handlers[i].Matchs(url))
                {
                    return handlers[i];
                }
            }

            return null;
        }

        #endregion

        public List<VideoDownloadHandler> Handlers
        {
            get
            {
                return handlers;
            }
        }

        public VideoDownloadExtension()
        {
            handlers = new List<VideoDownloadHandler>();
            handlers.Add(new VideoDownloadHandler(YouTubeDownloader.SiteName, YouTubeDownloader.UrlPattern, typeof(YouTubeDownloader)));
            handlers.Add(new VideoDownloadHandler(GoogleVideoDownloader.SiteName, GoogleVideoDownloader.UrlPattern, typeof(GoogleVideoDownloader)));
            handlers.Add(new VideoDownloadHandler(PutfileVideoDownloader.SiteName, PutfileVideoDownloader.UrlPattern, typeof(PutfileVideoDownloader)));
            handlers.Add(new VideoDownloadHandler(MetaCafeVideoDownloader.SiteName, MetaCafeVideoDownloader.UrlPattern, typeof(MetaCafeVideoDownloader)));
            handlers.Add(new VideoDownloadHandler(BreakVideoDownloader.SiteName, BreakVideoDownloader.UrlPattern, typeof(BreakVideoDownloader)));
            
            //ProtocolProviderFactory.ResolvingProtocolProvider += new EventHandler<ResolvingProtocolProviderEventArgs>(resolvingProtocolProviderEvent);
        }
    }
}
