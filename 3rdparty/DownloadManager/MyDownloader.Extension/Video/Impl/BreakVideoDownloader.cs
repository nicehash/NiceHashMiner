using System;
using System.Collections.Generic;
using System.Text;
using MyDownloader.Core;
using System.Diagnostics;

namespace MyDownloader.Extension.Video.Impl
{
    public class BreakVideoDownloader: BaseVideoDownloader
    {
        public const string SiteName = "Break";

        //http://my.break.com/content/view.aspx?ContentID=393301
        //http://www.break.com/canon/canon-battle-of-viral-video-super-stars.html
        //http://www.break.com/index/tomorrow-is-spank-it-saturday.html
        public const string UrlPattern = @"(?:[Bb][Rr][Ee][Aa][Kk]\.[Cc][Oo][Mm]/)(\w[\w|-]*)";

        protected override ResourceLocation ResolveVideoURL(string url, string pageData,
            out string videoTitle)
        {
            string sGlobalFileName = TextUtil.JustAfter(pageData, "sGlobalFileName='", "';");
            string sGlobalContentFilePath = TextUtil.JustAfter(pageData, "sGlobalContentFilePath='", "';");

            string flvUrl = TextUtil.JustAfter(pageData, "so.addVariable('videoPath', '", "');")
                .Replace("'+sGlobalContentFilePath+'", sGlobalContentFilePath)
                .Replace("'+sGlobalFileName+'", sGlobalFileName);

            string wmvUrl = TextUtil.JustAfter(pageData, "<param name=\"fileName\" value=\"", " />")
                .Replace("'+sGlobalContentFilePath+'", sGlobalContentFilePath)
                .Replace("'+sGlobalFileName+'", sGlobalFileName);
            Debug.WriteLine(wmvUrl);

            videoTitle = TextUtil.JustAfter(pageData, "<meta name=\"embed_video_title\" content=\"", "\" />"); 

            return ResourceLocation.FromURL(flvUrl);
        }
    }
}