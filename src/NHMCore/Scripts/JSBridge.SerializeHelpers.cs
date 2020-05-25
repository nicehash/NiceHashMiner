using NHM.Common;
using NHMCore.Mining;
using NHMCore.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;

using static NHMCore.Scripts.LibJSBridge;
using NHM.Common.Enums;
using NHM.DeviceMonitoring;
using NHMCore.Switching;

namespace NHMCore.Scripts
{
    public static partial class JSBridge
    {
        class JSBridgeAppException : Exception
        {
            public int Status { get; set; }
            public string Message { get; set; }
        }

        delegate IMessage JSLogicDelegate(byte[] in_buff);

        private static int HandleProtoMessageHelper(string funName, JSLogicDelegate logic, IntPtr buffer, long in_size, ref long out_size) {
            try
            {
                // Parse in
                var in_buff = new byte[in_size];
                Marshal.Copy(buffer, in_buff, 0, (int)in_size);
                // handle logic
                var out_msg = logic(in_buff);
                out_size = out_msg.CalculateSize();
                Marshal.Copy(out_msg.ToByteArray(), 0, buffer, out_msg.CalculateSize());
                return 0;
            }
            catch (JSBridgeAppException e)
            {
                return SerializeOutErr(new StatusMessage { Status = e.Status, Message = e.Message }, buffer, ref out_size);
            }
            catch (Exception e)
            {
                Logger.Error("JSBridge.Log", $"{funName} error {e}.");
                return -2;
            }
        }

        private static int SerializeOutErr(StatusMessage out_msg, IntPtr buffer, ref long out_size)
        {
            try
            {
                out_size = out_msg.CalculateSize();
                Marshal.Copy(out_msg.ToByteArray(), 0, buffer, out_msg.CalculateSize());
                return -1;
            }
            catch (Exception e)
            {
                return -3;
            }
        }
    }
}
