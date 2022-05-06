using System;

namespace NHMCore.Nhmws.V3
{
    internal class RpcException : Exception
    {
        public int Code = 0;

        public RpcException(string message, ErrorCode code)
            : base(message)
        {
            Code = (int)code;
        }
    }
}
