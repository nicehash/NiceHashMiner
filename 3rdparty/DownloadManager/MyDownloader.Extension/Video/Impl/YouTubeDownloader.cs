using System;
using System.Collections.Generic;
using System.Text;
using MyDownloader.Core;
using System.IO;
using MyDownloader.Extension.Protocols;
using System.Net;

namespace MyDownloader.Extension.Video.Impl
{
    public class YouTubeDownloader: BaseVideoDownloader
    {
        public const string SiteName = "You Tube";

        //http://www.youtube.com/watch?v=5zOevLN3Tic
        public const string UrlPattern = @"(?:[Yy][Oo][Uu][Tt][Uu][Bb][Ee]\.[Cc][Oo][Mm]/watch\?v=)(\w[\w|-]*)";

        protected override ResourceLocation ResolveVideoURL(string url, string pageData, 
            out string videoTitle)
        {
            videoTitle = TextUtil.JustAfter(pageData, "<meta name=\"title\" content=\"", "\">"); 

            return ResourceLocation.FromURL(String.Format("{0}/get_video?video_id={1}&t={2}", TextUtil.GetDomain(url),
                TextUtil.JustAfter(url, "v=", "&"), TextUtil.JustAfter(pageData, "&t=", "&hl=")));
        }
    }
}