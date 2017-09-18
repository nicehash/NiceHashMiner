using System;
using System.Collections.Generic;
using System.Text;

namespace NiceHashMiner {
    public static class Links {
        public static string VisitURL = "https://www.nicehash.com?utm_source=NHM";
        // add version
        public static string VisitURLNew = "https://github.com/NiceHash/NiceHashMinerLegacy/releases/tag/";
        // add btc adress as parameter
        public static string CheckStats = "https://www.nicehash.com/index.jsp?utm_source=NHM&p=miners&addr=";
        // help and faq
        public static string NHM_Help = "https://github.com/nicehash/NiceHashMinerLegacy/";
        public static string NHM_NoDev_Help = "https://github.com/nicehash/NiceHashMinerLegacy/wiki/Troubleshooting#nosupportdev";
        // faq
        public static string NHM_BTC_Wallet_Faq = "https://www.nicehash.com/help/how-to-create-the-bitcoin-addresswallet?utm_source=NHM";
        public static string NHM_Paying_Faq = "https://www.nicehash.com/help/when-and-how-do-you-get-paid?utm_source=NHM";
        // API
        // btc adress as parameter
        public static string NHM_API_stats = "https://api.nicehash.com/api?method=stats.provider&addr=";
        public static string NHM_API_info = "https://api.nicehash.com/api?method=simplemultialgo.info";
        public static string NHM_API_version = "https://api.nicehash.com/nicehashminer?method=version&legacy";
        //public static string NHM_API_stats_provider_workers = "https://api.nicehash.com/api?method=stats.provider.workers&addr=";

        // device profits
        public static string NHM_Profit_Check = "https://api.nicehash.com/?utm_source=NHM&p=calc&name=";

        // SMA Socket
        public static string NHM_Socket_Address = "wss://api.nicehash.com/v2/nhm";
    }
}
