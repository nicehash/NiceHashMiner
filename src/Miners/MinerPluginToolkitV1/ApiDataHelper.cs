using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace MinerPluginToolkitV1
{
    // this is temporary since we know it alredy works, try not to use it or replace it later
    public class ApiDataHelper
    {
        public static string GetHttpRequestNhmAgentStrin(string cmd)
        {
            return "GET /" + cmd + " HTTP/1.1\r\n" +
                   "Host: 127.0.0.1\r\n" +
                   "User-Agent: NiceHashMinerPlugin/0.0.0.1" + "\r\n" +
                   "\r\n";
        }

        public delegate bool IsApiEofFun(byte third, byte second, byte last);
        private IsApiEofFun _isApiEof;

        public ApiDataHelper(IsApiEofFun isApiEof = null)
        {
            _isApiEof = isApiEof;
        }

        protected virtual bool IsApiEof(byte third, byte second, byte last)
        {
            if (_isApiEof != null)
            {
                return _isApiEof(third, second, last);
            }
            return false;
        }

        public async Task<string> GetApiDataAsync(int port, string dataToSend, bool exitHack = false,
            bool overrideLoop = false)
        {
            string responseFromServer = null;
            try
            {
                var tcpc = new TcpClient("127.0.0.1", port);
                var nwStream = tcpc.GetStream();

                var bytesToSend = Encoding.ASCII.GetBytes(dataToSend);
                await nwStream.WriteAsync(bytesToSend, 0, bytesToSend.Length);

                var incomingBuffer = new byte[tcpc.ReceiveBufferSize];
                var prevOffset = -1;
                var offset = 0;
                var fin = false;

                while (!fin && tcpc.Client.Connected)
                {
                    var r = await nwStream.ReadAsync(incomingBuffer, offset, tcpc.ReceiveBufferSize - offset);
                    for (var i = offset; i < offset + r; i++)
                    {
                        if (incomingBuffer[i] == 0x7C || incomingBuffer[i] == 0x00
                                                      || (i > 2 && IsApiEof(incomingBuffer[i - 2],
                                                              incomingBuffer[i - 1], incomingBuffer[i]))
                                                      || overrideLoop)
                        {
                            fin = true;
                            break;
                        }

                        // Not working
                        //if (IncomingBuffer[i] == 0x5d || IncomingBuffer[i] == 0x5e) {
                        //    fin = true;
                        //    break;
                        //}
                    }

                    offset += r;
                    if (exitHack)
                    {
                        if (prevOffset == offset)
                        {
                            fin = true;
                            break;
                        }

                        prevOffset = offset;
                    }
                }

                tcpc.Close();

                if (offset > 0)
                    responseFromServer = Encoding.ASCII.GetString(incomingBuffer);
            }
            catch (Exception ex)
            {
                //Helpers.ConsolePrint(MinerTag(), ProcessTag() + " GetAPIData reason: " + ex.Message);
                return null;
            }

            return responseFromServer;
        }
    }
}
