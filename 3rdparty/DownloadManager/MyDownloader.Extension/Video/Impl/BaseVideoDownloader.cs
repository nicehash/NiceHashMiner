using System;
using System.Collections.Generic;
using System.Text;
using MyDownloader.Extension.Protocols;
using MyDownloader.Core;
using System.IO;
using System.Net;

namespace MyDownloader.Extension.Video.Impl
{
    public abstract class BaseVideoDownloader : HttpProtocolProvider
    {
        private ResourceLocation mappedUrl;

        void downloader_Ending(object sender, EventArgs e)
        {
            Downloader d = (Downloader)sender;

            VideoConverter.ConvertIfNecessary(d);
        }

        protected abstract ResourceLocation ResolveVideoURL(string url, string pageData, out string videoTitle);

        public override void Initialize(Downloader downloader)
        {
            base.Initialize(downloader);

            downloader.Ending += new EventHandler(downloader_Ending);
        }

        public override Stream CreateStream(ResourceLocation rl, long initialPosition, long endPosition)
        {
            return base.CreateStream(mappedUrl ?? rl, initialPosition, endPosition);
        }

        public override RemoteFileInfo GetFileInfo(ResourceLocation rl, out Stream stream)
        {
            stream = null;

            mappedUrl = null;

            String title;
            String pageData = GetPageData(rl);

            mappedUrl = ResolveVideoURL(rl.URL, pageData, out title);

            WebRequest request = this.GetRequest(mappedUrl);

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                RemoteFileInfo result = new RemoteFileInfo();
                result.FileSize = response.ContentLength;
                result.AcceptRanges = String.Compare(response.Headers["Accept-Ranges"], "bytes", true) == 0;

                if (response.ResponseUri != null)
                {
                    mappedUrl.URL = response.ResponseUri.OriginalString;
                }
                else
                {
                    stream = response.GetResponseStream();
                }

                return result;
            }
        }

        private String GetPageData(ResourceLocation rl)
        {
            String pageData;

            using (StreamReader sr = new StreamReader(this.CreateStream(rl, 0, 0)))
            {
                pageData = sr.ReadToEnd();
            }

            return pageData;
        }

        public string GetTitle(ResourceLocation rl)
        {
            String pageData = GetPageData(rl);

            string title;

            mappedUrl = ResolveVideoURL(rl.URL, pageData, out title);

            return title;
        }
    }
}
