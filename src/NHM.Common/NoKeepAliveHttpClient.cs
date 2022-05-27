using System.Net.Http;

namespace NHM.Common
{
    public class NoKeepAliveHttpClient : HttpClient
    {
        public NoKeepAliveHttpClient() : base()
        {
            this.DefaultRequestHeaders.ConnectionClose = true;
        }
    }
}
