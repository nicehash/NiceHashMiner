using NHM.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHM.Common
{
    public static class BuildOptions
    {

        public static BuildTag BuildTag { get; private set; } = BuildTag.PRODUCTION;
    }
}
