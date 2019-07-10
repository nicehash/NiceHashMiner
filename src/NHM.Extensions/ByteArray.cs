using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NHM.Extensions
{
    public static class ByteArray
    {
        public static string ToBase64String(this byte[] array)
        {
            var b = Convert.ToBase64String(array);
            return b.Trim('=').Replace('/', '-');
        }
    }
}
