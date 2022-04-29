using System;

namespace NHMCore.Nhmws.V3.Models
{
    internal class RpcException : Exception
    {
        public int Code = 0;

        public RpcException(string message, ErrorCodeV3 code)
            : base(message)
        {
            Code = (int)code;
        }
    }
}
