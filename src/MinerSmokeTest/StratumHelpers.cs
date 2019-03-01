using NiceHashMinerLegacy.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinerSmokeTest
{
    //todo remove after merge with plugins
    public static class StratumHelpers
    {
            private static string GetAlgorithmUrlName(AlgorithmType algorithmType)

            {

                if (algorithmType < 0)

                {

                    return "";

                }

                const string NOT_FOUND = "NameNotFound type not supported";

                var name = Enum.GetName(typeof(AlgorithmType), algorithmType) ?? NOT_FOUND;

                if (name == NOT_FOUND)

                {

                    return "";

                }

                // strip out the _UNUSED

                name = name.Replace("_UNUSED", "");

                return name.ToLower();

            }



            public static string GetLocationUrl(AlgorithmType algorithmType, string miningLocation, NhmConectionType conectionType)

            {

                var name = GetAlgorithmUrlName(algorithmType);

                // if name is empty return

                if (name == "") return "";



                var nPort = 3333 + algorithmType;

                var sslPort = 30000 + nPort;



                // NHMConectionType.NONE

                var prefix = "";

                var port = nPort;

                switch (conectionType)

                {

                    case NhmConectionType.LOCKED:

                        return miningLocation;

                    case NhmConectionType.STRATUM_TCP:

                        prefix = "stratum+tcp://";

                        break;

                    case NhmConectionType.STRATUM_SSL:

                        prefix = "stratum+ssl://";

                        port = sslPort;

                        break;

                }



#if TESTNET

            return prefix

                   + name

                   + "-test." + miningLocation

                   + ".nicehash.com:"

                   + port;

#elif TESTNETDEV

            return prefix

                   + "stratum-test." + miningLocation

                   + ".nicehash.com:"

                   + port;

#else

                return prefix

                       + name

                       + "." + miningLocation

                       + ".nicehash.com:"

                       + port;

#endif

            }
    
        }
    
}
