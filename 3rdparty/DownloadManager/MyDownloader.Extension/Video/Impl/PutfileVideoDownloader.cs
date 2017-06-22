using System;
using System.Collections.Generic;
using System.Text;
using MyDownloader.Core;

namespace MyDownloader.Extension.Video.Impl
{
    public class PutfileVideoDownloader: BaseVideoDownloader
    {
        public const string SiteName = "PutFile";

        //http://media.putfile.com/kilo-the-amazing-pitt
        public const string UrlPattern = @"(?:[Mm][Ee][Dd][Ii][Aa]\.[Pp][Uu][Tt][Ff][Ii][Ll][Ee]\.[Cc][Oo][Mm]/)(\w[\w|-]*)";

        protected override ResourceLocation ResolveVideoURL(string url, string pageData,
            out string videoTitle)
        {
            url = TextUtil.JustAfter(pageData, "so1.addVariable(\"flv\", \"", "\");");

            videoTitle = TextUtil.JustAfter(pageData, "<meta name=\"title\" content=\"", "\" />"); 

            return ResourceLocation.FromURL(url);
        }
    }
}
