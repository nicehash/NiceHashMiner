using System;
using System.Collections.Generic;
using System.Text;
using MyDownloader.Core;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace MyDownloader.Extension.Video
{
    [Serializable]
    public enum VideoFormat: int
    {
        None, 
        AVI,
        MPEG,
        MP3
    }

    public static class VideoConverter
    {
        private const string VideoConverterFormatProperty = "VideoConverterFormatProperty";

        private const string ffmpeg = "ffmpeg.exe";

        private const string ToAVICmdLine = "-y -i \"{0}\" -vcodec msmpeg4v2 -acodec mp3 -sameq -ab 128 \"{1}\"";
        private const string ToMPEGCmdLine = "-y -i \"{0}\" -vcodec mpeg1video -acodec mp3 -sameq -ab 64 \"{1}\"";
        private const string ToMP3CmdLine = "-y -i \"{0}\" -f mp3 -vn -acodec copy \"{1}\"";

        public static void Convert(string fileName, VideoFormat fmt)
        {
            if (fmt == VideoFormat.None) throw new ArgumentException("Invalid argmument.", "fmt");

            string cmd = String.Empty;

            if (fmt == VideoFormat.AVI) cmd = ToAVICmdLine;
            if (fmt == VideoFormat.MPEG) cmd = ToMPEGCmdLine;
            if (fmt == VideoFormat.MP3) cmd = ToMP3CmdLine;
            
            string directory = Path.GetDirectoryName(fileName);

            // the original file name ends with 'AVI', 'MPEG', 'MP3'....new we rename the file to .FLV
            
            string flvPath = Path.Combine(directory, String.Concat(Guid.NewGuid().ToString("N"), ".flv"));
            
            try
            {
                File.Move(fileName, flvPath);

                string sourceFile = Path.GetFileName(flvPath);
                string targetFile = Path.GetFileName(fileName);

                cmd = String.Format(cmd, sourceFile, targetFile);

                string toolPath = Path.GetDirectoryName(Application.ExecutablePath);
                //toolPath = @"C:\Documents and Settings\guilherme.labigalini\Desktop\MyDownloader\MyDownloader\MyDownloader.App\bin\Debug\";
                ProcessStartInfo psi = new ProcessStartInfo(toolPath + "\\" + ffmpeg, cmd);
                psi.UseShellExecute = false;
                psi.CreateNoWindow = true;
                psi.WorkingDirectory = directory;

                using (Process p = Process.Start(psi))
                {
                    p.WaitForExit();
                }
            }
            finally
            {
                File.Delete(flvPath);
            }            
        }

        public static void ConvertIfNecessary(Downloader d)
        {
            VideoFormat fmt = GetConvertOption(d);

            if (fmt == VideoFormat.None)
            {
                return;
            }

            try
            {
                d.StatusMessage = "converting video";

                Convert(d.LocalFile, fmt);
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception ex)
            {
                d.LastError = new Exception("error converting video: " + ex.Message, ex);
            }
            finally
            {
                d.StatusMessage = null;
            }
        }

        public static VideoFormat GetConvertOption(Downloader d)
        {
            if (! d.ExtendedProperties.ContainsKey(VideoConverterFormatProperty))
            {
                return VideoFormat.None;
            }

            object objFmt = d.ExtendedProperties[VideoConverterFormatProperty];

            if (objFmt == null)
            {
                return VideoFormat.None;
            }

            return (VideoFormat)objFmt;
        }

        public static void SetConvertOption(Downloader d, VideoFormat format)
        {
            if (format == VideoFormat.None)
            {
                if (d.ExtendedProperties.ContainsKey(VideoConverterFormatProperty))
                {
                    d.ExtendedProperties.Remove(VideoConverterFormatProperty);
                }
            }

            d.ExtendedProperties[VideoConverterFormatProperty] = format;
        }
    }
}
