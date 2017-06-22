using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using MyDownloader.Core;

namespace MyDownloader.Extension.Video.Impl
{
    public class MetaCafeVideoDownloader: BaseVideoDownloader
    {
        public const string SiteName = "Meta Cafe";

        //http://www.metacafe.com/watch/851582/village_idiots/
        public const string UrlPattern = @"(?:[Mm][Ee][Ta][Aa][Cc][Aa][Ff][Ee]\.[Cc][Oo][Mm]/watch/)(\w[\w|-]*)/(\w[\w|-]*)";

        protected override ResourceLocation ResolveVideoURL(string url,
            string pageData, out string videoTitle)
        {
            url = TextUtil.JustAfter(pageData, "mediaURL=", "&");

            videoTitle = TextUtil.JustAfter(pageData, "<title>", "</title>");

            return ResourceLocation.FromURL(HttpUtility.UrlDecode(url));
        }
    }
}
