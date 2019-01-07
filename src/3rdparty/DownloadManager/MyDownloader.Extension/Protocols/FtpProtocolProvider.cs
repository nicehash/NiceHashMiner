using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using MyDownloader.Core;

namespace MyDownloader.Extension.Protocols
{
    public class FtpProtocolProvider: BaseProtocolProvider, IProtocolProvider
    {
        private void FillCredentials(FtpWebRequest request, ResourceLocation rl)
        {
            if (rl.Authenticate)
            {
                string login = rl.Login;
                string domain = string.Empty;

                int slashIndex = login.IndexOf('\\');

                if (slashIndex >= 0)
                {
                    domain = login.Substring(0, slashIndex);
                    login = login.Substring(slashIndex + 1);
                }

                NetworkCredential myCred = new NetworkCredential(login, rl.Password);
                myCred.Domain = domain;

                request.Credentials = myCred;
            }
        }

        #region IProtocolProvider Members

        public void Initialize(Downloader downloader)
        {
        }

        public Stream CreateStream(ResourceLocation rl, long initialPosition, long endPosition)
        {
            FtpWebRequest request = (FtpWebRequest)GetRequest(rl);

            FillCredentials(request, rl);

            request.Method = WebRequestMethods.Ftp.DownloadFile;
            request.ContentOffset = initialPosition;

            WebResponse response = request.GetResponse();

            return response.GetResponseStream();
        }

        public RemoteFileInfo GetFileInfo(ResourceLocation rl, out Stream stream)
        {
            FtpWebRequest request;

            RemoteFileInfo result = new RemoteFileInfo();
            result.AcceptRanges = true;

            stream = null;

            request = (FtpWebRequest)GetRequest(rl);
            request.Method = WebRequestMethods.Ftp.GetFileSize;
            FillCredentials(request, rl);

            using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
            {
                result.FileSize = response.ContentLength;
            }

            request = (FtpWebRequest)GetRequest(rl);
            request.Method = WebRequestMethods.Ftp.GetDateTimestamp;
            FillCredentials(request, rl);

            using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
            {
                result.LastModified = response.LastModified;
            }

            return result;
        }

        #endregion
    }
}
