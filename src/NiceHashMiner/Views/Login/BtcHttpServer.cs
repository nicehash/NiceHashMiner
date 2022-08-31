using Newtonsoft.Json;
using NHM.Common;
using NHMCore;
using NHMCore.Configs;
using NHMCore.Utils;
using NiceHashMiner.Views.Common;
using NiceHashMiner.Views.Common.NHBase;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace NiceHashMiner.Views.Login
{
    public delegate void SuccessfulHTTPLogin(object sender, EventArgs e);
    class BtcHttpServer
    {
        private static readonly BtcHttpServer instance = new BtcHttpServer();
        public event SuccessfulHTTPLogin SuccessfulHTTPLogin;
        private static string TAG = nameof(BtcHttpServer);
        private static CancellationTokenSource _stopServer = new CancellationTokenSource();
        private static HttpListener _listener;
        private static string _url = "http://localhost:18000/";
        static BtcHttpServer(){}
        private BtcHttpServer(){}
        public static BtcHttpServer Instance
        {
            get
            {
                return instance;
            }
        }

        record BtcResponse(string addr);

        protected void OnSuccessLogin(EventArgs e)
        {
            if (SuccessfulHTTPLogin != null) SuccessfulHTTPLogin(this, e);
        }

        public async Task HandleIncomingConnections(CancellationToken stop)
        {
            bool runServer() => !stop.IsCancellationRequested;

            while (runServer())
            {
                // Will wait here until we hear from a connection
                var ctx = await _listener.GetContextAsync();

                var req = ctx.Request;
                var resp = ctx.Response;

                // Print out some info about the request
                string _pageData =
                    $"Url {req.Url}\n" +
                    $"HttpMethod {req.HttpMethod}\n" +
                    $"HasEntityBody {req.HasEntityBody}\n" +
                    $"UserHostName {req.UserHostName}\n" +
                    $"UserAgent {req.UserAgent}\n";

                Logger.Info(TAG, _pageData);

                Task<string> handle_setaddr_OPTIONS()
                {
                    resp.ContentEncoding = Encoding.UTF8;
                    resp.ContentLength64 = 0;

                    resp.AddHeader("Access-Control-Allow-Origin", "*");
                    resp.AddHeader("Access-Control-Allow-Methods", "GET,HEAD,PUT,PATCH,POST,DELETE");
                    resp.AddHeader("Vary", "Access-Control-Request-Headers");
                    resp.AddHeader("Access-Control-Allow-Headers", "content-type");
                    resp.AddHeader("Content-Length", "0");

                    resp.StatusCode = (int)HttpStatusCode.NoContent;
                    resp.StatusDescription = "No Content";
                    resp.Close();
                    return Task.FromResult<string>(null);
                }

                async Task<string> handle_setaddr_POST()
                {
                    using var r = new StreamReader(req.InputStream, req.ContentEncoding);
                    var data = await r.ReadToEndAsync();
                    // TODO do something with the data
                    Logger.Info(TAG, $"{req.HttpMethod} - {data}");

                    string parseBtc()
                    {
                        try
                        {
                            return JsonConvert.DeserializeObject<BtcResponse>(data)?.addr;
                        }
                        catch (Exception e)
                        {
                            Logger.Error(TAG, $"parseBtc {e.Message}");
                        }
                        return null;
                    }

                    resp.ContentEncoding = Encoding.UTF8;
                    resp.ContentLength64 = 0;

                    resp.AddHeader("Access-Control-Allow-Methods", "GET,HEAD,PUT,PATCH,POST,DELETE");
                    resp.AddHeader("Access-Control-Allow-Origin", "*");
                    resp.AddHeader("Vary", "Access-Control-Request-Headers");
                    resp.AddHeader("Access-Control-Allow-Headers", "x-request-id,x-user-agent");
                    resp.Close();

                    return parseBtc();
                }

                Task<string> handle_unknown()
                {
                    resp.ContentEncoding = Encoding.UTF8;
                    resp.ContentLength64 = 0;

                    resp.AddHeader("Access-Control-Allow-Origin", "*");
                    resp.AddHeader("Access-Control-Allow-Methods", "GET,HEAD,PUT,PATCH,POST,DELETE");
                    resp.AddHeader("Vary", "Access-Control-Request-Headers");
                    resp.AddHeader("Access-Control-Allow-Headers", "content-type");
                    resp.AddHeader("Content-Length", "0");

                    resp.StatusCode = (int)HttpStatusCode.BadRequest;
                    resp.StatusDescription = "Bad Request";
                    resp.Close();

                    return Task.FromResult<string>(null);
                }

                var btc = (req.HttpMethod, req.Url.AbsolutePath) switch
                {
                    ("POST", "/setaddr") => await handle_setaddr_POST(),
                    ("OPTIONS", "/setaddr") => await handle_setaddr_OPTIONS(),
                    (_, _) => await handle_unknown(),
                };

                if (CredentialValidators.ValidateBitcoinAddress(btc))
                {
                    Logger.Info(TAG, $"BTC address received = {btc}");
                    CredentialsSettings.Instance.SetBitcoinAddress(btc);
                    OnSuccessLogin(EventArgs.Empty);
                }
            }
        }


        public async Task Run(CancellationToken stop)
        {
            // Create a Http server and start listening for incoming connections
            _listener = new HttpListener();
            _listener.Prefixes.Add(_url);
            _listener.Start();
            Logger.Info(TAG, $"Listening for connections on {_url}");

            // Handle requests
            await HandleIncomingConnections(stop);

            // Close the listener
            _listener.Close();
        }

        public void RunBackgrounTask()
        {
            _ = Task.Run(async () => await Run(_stopServer.Token));
        }

        public void Stop()
        {
            try
            {
                _stopServer.Cancel();
            }
            catch
            {

            }
        }
    }
}
