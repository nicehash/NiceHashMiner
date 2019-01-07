using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiceHashMiner.Stats.Models
{
    internal class RpcException : Exception
    {
        public int Code = 0;

        public RpcException(string message, int code)
            : base(message)
        {
            Code = code;
        }
    }
}
