using System;
using System.Net;

namespace NHM.Common
{
    public class NoKeepAliveWebClient : WebClient
    {
        protected override WebRequest GetWebRequest(Uri address)
        {
            var request = base.GetWebRequest(address);
            if (request is HttpWebRequest httpWebRequest) httpWebRequest.KeepAlive = false;
            return request;
        }
    }
}
