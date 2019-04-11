// TESTNET
#if TESTNET || TESTNETDEV

﻿using NiceHashMiner.Miners;

namespace NiceHashMiner.Interfaces
{
    public interface IRatesComunication
    {
        void ClearRatesAll();

        void AddRateInfo(ApiData iApiData, double paying, bool isApiGetException);
    }
}
#endif
