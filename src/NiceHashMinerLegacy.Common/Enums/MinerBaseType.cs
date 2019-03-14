using System;

namespace NiceHashMinerLegacy.Common.Enums
{
    /// <summary>
    /// Do not delete obsolete enums! Always add new ones before the END enum.
    /// </summary>
    public enum MinerBaseType
    {
        NONE = 0,
        [Obsolete("Miner is obsolete")]
        cpuminer,
        ccminer,
        sgminer,
        [Obsolete("Miner is obsolete")]
        nheqminer,
        [Obsolete("Miner is obsolete")]
        eqm,
        ethminer,
        Claymore,
        [Obsolete("Miner is obsolete")]
        OptiminerAMD,
        [Obsolete("Miner is obsolete")]
        excavator,
        XmrStak,
        ccminer_alexis,
        [Obsolete("Miner is obsolete")]
        experimental,
        EWBF,
        Prospector,
        [Obsolete("Miner is obsolete")]
        Xmrig,
        [Obsolete("Miner is obsolete")]
        XmrStakAMD,
        [Obsolete("Miner is obsolete")]
        Claymore_old,
        [Obsolete("Miner is obsolete")]
        dtsm,
        trex,
        Phoenix,
        GMiner,
        BMiner,
        TTMiner,
        NBMiner,
        TeamRedMiner,
        END
    }
}
