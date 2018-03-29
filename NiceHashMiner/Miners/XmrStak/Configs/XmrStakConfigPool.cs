using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiceHashMiner.Miners.XmrStak.Configs
{
    public class XmrStakConfigPool
    {
        public class XmrStakPoolItem
        {
            public string pool_address;
            public string wallet_address;
            public string pool_password = "x";
            public bool use_nicehash = true;
            public bool use_tls = false;
            public string tls_fingerprint = "";
            public int pool_weight = 1;
            public string rig_id = "";

            public XmrStakPoolItem(string pool, string worker, int weight)
            {
                pool_address = pool;
                wallet_address = worker;
                pool_weight = weight;
            }
        }

        public void SetupPools(string poolAddr, string wallet)
        {
            SetupPools(new List<string> {poolAddr}, wallet);
        }

        public void SetupPools(List<string> poolAddrs, string wallet)
        {
            pool_list = new List<XmrStakPoolItem>();
            var i = 1;
            foreach (var poolAddr in poolAddrs)
            {
                pool_list.Add(new XmrStakPoolItem(poolAddr, wallet, i));
                i++;
            }
        }

        /*
         * pool_address    - Pool address should be in the form "pool.supportxmr.com:3333". Only stratum pools are supported.
         * wallet_address  - Your wallet, or pool login.
         * pool_password   - Can be empty in most cases or "x".
         * use_nicehash    - Limit the nonce to 3 bytes as required by nicehash.
         * use_tls         - This option will make us connect using Transport Layer Security.
         * tls_fingerprint - Server's SHA256 fingerprint. If this string is non-empty then we will check the server's cert against it.
         * pool_weight     - Pool weight is a number telling the miner how important the pool is. Miner will mine mostly at the pool 
         *                   with the highest weight, unless the pool fails. Weight must be an integer larger than 0.
         *
         * We feature pools up to 1MH/s. For a more complete list see M5M400's pool list at www.moneropools.com
         */
        public List<XmrStakPoolItem> pool_list = new List<XmrStakPoolItem>();

        /*
         * Currency to mine. Supported values:
         *
         *    aeon
         *    cryptonight (try this if your coin is not listed)
         *    cryptonight_lite
         *    edollar
         *    electroneum
         *    graft
         *    intense
         *    karbo
         *    monero7 (use this for Monero's new PoW)
         *    sumokoin
         *
         */
        public readonly string currency = "monero7";
    }
}
