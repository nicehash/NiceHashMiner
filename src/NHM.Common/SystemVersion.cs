using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHM.Common
{
    public static class SystemVersion
    {
        public static string FullName { get; set; } = "";
        public static int OsVersion { get; set; } = 10;
        public static int BuildNumber { get; set; } = 0;
    }
}
