using System;
using System.Collections.Generic;
using System.Text;

namespace MyDownloader.Extension.Video
{
    internal static class TextUtil
    {
        internal static string JustBefore(string Str, string Seq)
        {
            string Orgi = Str;
            try
            {
                Str = Str.ToLower();
                Seq = Seq.ToLower();

                return Orgi.Substring(0, Str.Length - (Str.Length - Str.IndexOf(Seq)));
            }
            catch (Exception)
            {
                return "";
            }
        }

        internal static string GetDomain(String URL)
        {
            return URL.Substring(0, URL.LastIndexOf("/"));
        }

        internal static string GetFilename(String URL)
        {
            string Filename = URL.Substring(URL.LastIndexOf("/") + 2, URL.Length - URL.LastIndexOf("/") - 2);

            if (Filename.IndexOf("&") != -1)
                Filename = JustBefore(Filename, "&");

            return Filename;
        }

        internal static string JustAfter(string Str, string Seq, string SeqEnd)
        {
            string Orgi = Str;
            try
            {
                Str = Str.ToLower();
                Seq = Seq.ToLower();

                int i = Str.IndexOf(Seq);

                if (i < 0)
                    return null;

                i = i + Seq.Length;

                int j = Str.IndexOf(SeqEnd, i);
                int end;

                if (j > 0) end = j - i;
                else end = Str.Length - i;

                return Orgi.Substring(i, end);
            }
            catch (Exception)
            {
                return "";
            }
        }

        internal static string JustAfter(string Str, string Seq)
        {
            string Orgi = Str;
            try
            {
                Str = Str.ToLower();
                Seq = Seq.ToLower();

                int i = Str.IndexOf(Seq);

                if (i < 0)
                    return null;

                i = i + Seq.Length;

                return Orgi.Substring(i, Str.Length - i);
            }
            catch (Exception)
            {
                return "";
            }
        }
    }
}
