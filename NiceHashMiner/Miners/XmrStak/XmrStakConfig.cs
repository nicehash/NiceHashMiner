using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace NiceHashMiner.Miners
{
    public class XmrStakConfig
    {
        public XmrStakConfig(string poolAddr, string wallet, int port) {
            pool_address = poolAddr;
            wallet_address = wallet;
            httpd_port = port;
        }

        /*
         * TLS Settings
         * If you need real security, make sure tls_secure_algo is enabled (otherwise MITM attack can downgrade encryption
         * to trivially breakable stuff like DES and MD5), and verify the server's fingerprint through a trusted channel. 
         *
         * use_tls         - This option will make us connect using Transport Layer Security.
         * tls_secure_algo - Use only secure algorithms. This will make us quit with an error if we can't negotiate a secure algo.
         * tls_fingerprint - Server's SHA256 fingerprint. If this string is non-empty then we will check the server's cert against it.
         */
        public readonly bool use_tls = false;
        public readonly bool tls_secure_algo = true;
        public readonly string tls_fingerprint = "";

        /*
         * pool_address	  - Pool address should be in the form "pool.supportxmr.com:3333". Only stratum pools are supported.
         * wallet_address - Your wallet, or pool login.
         * pool_password  - Can be empty in most cases or "x".
         */
        public readonly string pool_address; // : "pool.supportxmr.com:3333",
        public readonly string wallet_address;
        public readonly string pool_password = "x";

        /*
         * Network timeouts.
         * Because of the way this client is written it doesn't need to constantly talk (keep-alive) to the server to make 
         * sure it is there. We detect a buggy / overloaded server by the call timeout. The default values will be ok for 
         * nearly all cases. If they aren't the pool has most likely overload issues. Low call timeout values are preferable -
         * long timeouts mean that we waste hashes on potentially stale jobs. Connection report will tell you how long the
         * server usually takes to process our calls.
         *
         * call_timeout - How long should we wait for a response from the server before we assume it is dead and drop the connection.
         * retry_time	- How long should we wait before another connection attempt.
         *                Both values are in seconds.
         * giveup_limit - Limit how many times we try to reconnect to the pool. Zero means no limit. Note that stak miners
         *                don't mine while the connection is lost, so your computer's power usage goes down to idle.
         */
        public int call_timeout = 10;
        public int retry_time = 10;
        public int giveup_limit = 0;

        /*
         * Output control.
         * Since most people are used to miners printing all the time, that's what we do by default too. This is suboptimal
         * really, since you cannot see errors under pages and pages of text and performance stats. Given that we have internal
         * performance monitors, there is very little reason to spew out pages of text instead of concise reports.
         * Press 'h' (hashrate), 'r' (results) or 'c' (connection) to print reports.
         *
         * verbose_level - 0 - Don't print anything. 
         *                 1 - Print intro, connection event, disconnect event
         *                 2 - All of level 1, and new job (block) event if the difficulty is different from the last job
         *                 3 - All of level 1, and new job (block) event in all cases, result submission event.
         *                 4 - All of level 3, and automatic hashrate report printing 
         */
        public int verbose_level = 4;

        /*
         * Automatic hashrate report
         *
         * h_print_time - How often, in seconds, should we print a hashrate report if verbose_level is set to 4.
         *                This option has no effect if verbose_level is not 4.
         */
        public int h_print_time = 60;

        /*
         * Daemon mode
         *
         * If you are running the process in the background and you don't need the keyboard reports, set this to true.
         * This should solve the hashrate problems on some emulated terminals.
         */
        public bool daemon_mode = false;

        /*
         * Output file
         *
         * output_file  - This option will log all output to a file.
         *
         */
        public readonly string output_file = "";

        /*
         * Built-in web server
         * I like checking my hashrate on my phone. Don't you?
         * Keep in mind that you will need to set up port forwarding on your router if you want to access it from
         * outside of your home network. Ports lower than 1024 on Linux systems will require root.
         *
         * httpd_port - Port we should listen on. Default, 0, will switch off the server.
         */
        public readonly int httpd_port;

        /*
         * prefer_ipv4 - IPv6 preference. If the host is available on both IPv4 and IPv6 net, which one should be choose?
         *               This setting will only be needed in 2020's. No need to worry about it now.
         */
        public bool prefer_ipv4 = true;
    }
}
