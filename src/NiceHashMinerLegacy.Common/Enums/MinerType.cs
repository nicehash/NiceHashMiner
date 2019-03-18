using System;

namespace NiceHashMinerLegacy.Common.Enums
{
    public enum MinerType
    {
        NONE,
        ccminer,
        [Obsolete("Miner is obsolete")]
        ccminer_CryptoNight,
        ethminer_OCL,
        ethminer_CUDA,
        sgminer,
        [Obsolete("Miner is obsolete")]
        cpuminer_opt,
        [Obsolete("Miner is obsolete")]
        nheqminer_CPU,
        [Obsolete("Miner is obsolete")]
        nheqminer_CUDA,
        [Obsolete("Miner is obsolete")]
        nheqminer_AMD,
        [Obsolete("Miner is obsolete")]
        eqm_CPU,
        [Obsolete("Miner is obsolete")]
        eqm_CUDA,
        [Obsolete("Miner is obsolete")]
        ClaymoreZcash,
        [Obsolete("Miner is obsolete")]
        ClaymoreCryptoNight,
        [Obsolete("Miner is obsolete")]
        OptiminerZcash,
        [Obsolete("Miner is obsolete")]
        excavator,
        ClaymoreDual,
        EWBF,
        [Obsolete("Miner is obsolete")]
        Xmrig,
        [Obsolete("Miner is obsolete")]
        dtsm,
        trex,
        Phoenix,
        GMiner,
        BMiner,
        TTMiner,
        END
    }
}
