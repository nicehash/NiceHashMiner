using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using MyDownloader.Core;

namespace MyDownloader.Extension.Video
{
    public class VideoDownloadHandler 
    {
        #region Fields

        private string urlRegExpr;
        private string name;
        private Type type;
        private Regex reg;

        #endregion

        #region Constructor

        public VideoDownloadHandler(string name, string regExp, Type type)
        {
            this.Name = name;
            this.UrlRegExpr = regExp;
            this.Type = type;

            RegexOptions options = ((RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline)
                        | RegexOptions.IgnoreCase);
            reg = new Regex(this.UrlRegExpr, options);
        }

        #endregion

        #region Properties

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public string UrlRegExpr
        {
            get { return urlRegExpr; }
            set { urlRegExpr = value; }
        }

        public Type Type
        {
            get { return type; }
            set { type = value; }
        }

        #endregion

        #region Methods

        public bool Matchs(string url)
        {
            return reg.IsMatch(url);
        }

        public override string ToString()
        {
            return this.Name;
        }

        #endregion
    }
}
