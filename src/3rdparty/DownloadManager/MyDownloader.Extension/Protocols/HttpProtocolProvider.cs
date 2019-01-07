using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;
using MyDownloader.Core;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace MyDownloader.Extension.Protocols
{
    public class HttpProtocolProvider : BaseProtocolProvider, IProtocolProvider
    {
        static HttpProtocolProvider()
        {
            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(certificateCallBack);

            // Set to TLS 1.2
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)SslProtocols.Tls12;
        }

        static bool certificateCallBack(
            object sender,
            X509Certificate certificate,
            X509Chain chain,
            SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        private void FillCredentials(HttpWebRequest request, ResourceLocation rl)
        {
            if (rl.Authenticate)
            {
                string login = rl.Login;
                string domain = string.Empty;

                int slashIndex = login.IndexOf('\\');

                if (slashIndex >= 0)
                {
                    domain = login.Substring(0, slashIndex );
                    login = login.Substring(slashIndex + 1);
                }

                NetworkCredential myCred = new NetworkCredential(login, rl.Password);
                myCred.Domain = domain;

                request.Credentials = myCred;
            }
        }

        #region IProtocolProvider Members

        public virtual void Initialize(Downloader downloader)
        {
        }

        public virtual Stream CreateStream(ResourceLocation rl, long initialPosition, long endPosition)
        {
            HttpWebRequest request = (HttpWebRequest)GetRequest(rl);

            FillCredentials(request, rl);

            if (initialPosition != 0)
            {
                if (endPosition == 0)
                {
                    request.AddRange((int)initialPosition);
                }
                else
                {
                    request.AddRange((int)initialPosition, (int)endPosition);
                }
            }

            WebResponse response = request.GetResponse();
            
            return response.GetResponseStream();
        }

        public virtual RemoteFileInfo GetFileInfo(ResourceLocation rl, out Stream stream)
        {
            HttpWebRequest request = (HttpWebRequest)GetRequest(rl);

            FillCredentials(request, rl);

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            RemoteFileInfo result = new RemoteFileInfo();
            result.MimeType = response.ContentType;
            result.LastModified = response.LastModified;
            result.FileSize = response.ContentLength;
            result.AcceptRanges = String.Compare(response.Headers["Accept-Ranges"], "bytes", true) == 0;

            stream = response.GetResponseStream();

            return result;
        }

        #endregion
    }
}
