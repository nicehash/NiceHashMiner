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
        public class XmrStakPoolItem
        {
            public string pool_address;
            public string wallet_address;
            public string pool_password = "x";
            public bool use_nicehash = true;
            public bool use_tls = false;
            public string tls_fingerprint = "";
            public int pool_weight = 1;

            public XmrStakPoolItem(string pool, string worker, int weight) {
                pool_address = pool;
                wallet_address = worker;
                pool_weight = weight;
            }
        }

        public void SetupPools(string poolAddr, string wallet) {
            SetupPools(new List<string> {poolAddr}, wallet);
        }

        public void SetupPools(List<string> poolAddrs, string wallet) {
            pool_list = new List<XmrStakPoolItem>();
            var i = 1;
            foreach (var poolAddr in poolAddrs) {
                pool_list.Add(new XmrStakPoolItem(poolAddr, wallet, i));
                i++;
            }
        }

        public void SetBenchmarkOptions(string logFile) {
            output_file = logFile;
            h_print_time = 1;
            daemon_mode = true;
            flush_stdout = true;
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
         * currency to mine
         * allowed values: 'monero' or 'aeon'
         */
        public readonly string currency = "monero";

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
        public readonly int call_timeout = 10;
        public readonly int retry_time = 30;
        public readonly int giveup_limit = 0;

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
         *
         * print_motd    - Display messages from your pool operator in the hashrate result.
         */
        public readonly int verbose_level = 4;
        public readonly bool print_motd = true;

        /*
         * Automatic hashrate report
         *
         * h_print_time - How often, in seconds, should we print a hashrate report if verbose_level is set to 4.
         *                This option has no effect if verbose_level is not 4.
         */
        public int h_print_time = 60;

        /*
         * Manual hardware AES override
         *
         * Some VMs don't report AES capability correctly. You can set this value to true to enforce hardware AES or
         * to false to force disable AES or null to let the miner decide if AES is used.
         *
         * WARNING: setting this to true on a CPU that doesn't support hardware AES will crash the miner.
         */
        public readonly bool? aes_override = null;

        /*
         * LARGE PAGE SUPPORT
         * Large pages need a properly set up OS. It can be difficult if you are not used to systems administration,
         * but the performance results are worth the trouble - you will get around 20% boost. Slow memory mode is
         * meant as a backup, you won't get stellar results there. If you are running into trouble, especially
         * on Windows, please read the common issues in the README.
         *
         * By default we will try to allocate large pages. This means you need to "Run As Administrator" on Windows.
         * You need to edit your system's group policies to enable locking large pages. Here are the steps from MSDN
         *
         * 1. On the Start menu, click Run. In the Open box, type gpedit.msc.
         * 2. On the Local Group Policy Editor console, expand Computer Configuration, and then expand Windows Settings.
         * 3. Expand Security Settings, and then expand Local Policies.
         * 4. Select the User Rights Assignment folder.
         * 5. The policies will be displayed in the details pane.
         * 6. In the pane, double-click Lock pages in memory.
         * 7. In the Local Security Setting – Lock pages in memory dialog box, click Add User or Group.
         * 8. In the Select Users, Service Accounts, or Groups dialog box, add an account that you will run the miner on
         * 9. Reboot for change to take effect.
         *
         * Windows also tends to fragment memory a lot. If you are running on a system with 4-8GB of RAM you might need
         * to switch off all the auto-start applications and reboot to have a large enough chunk of contiguous memory.
         *
         * On Linux you will need to configure large page support "sudo sysctl -w vm.nr_hugepages=128" and increase your
         * ulimit -l. To do do this you need to add following lines to /etc/security/limits.conf - "* soft memlock 262144"
         * and "* hard memlock 262144". You can also do it Windows-style and simply run-as-root, but this is NOT
         * recommended for security reasons.
         *
         * Memory locking means that the kernel can't swap out the page to disk - something that is unlikely to happen on a
         * command line system that isn't starved of memory. I haven't observed any difference on a CLI Linux system between
         * locked and unlocked memory. If that is your setup see option "no_mlck".
         */

        /*
         * use_slow_memory defines our behaviour with regards to large pages. There are three possible options here:
         * always  - Don't even try to use large pages. Always use slow memory.
         * warn    - We will try to use large pages, but fall back to slow memory if that fails.
         * no_mlck - This option is only relevant on Linux, where we can use large pages without locking memory.
         *           It will never use slow memory, but it won't attempt to mlock
         * never   - If we fail to allocate large pages we will print an error and exit.
         */
        public readonly string use_slow_memory = "warn";

        /*
         * TLS Settings
         * If you need real security, make sure tls_secure_algo is enabled (otherwise MITM attack can downgrade encryption
         * to trivially breakable stuff like DES and MD5), and verify the server's fingerprint through a trusted channel.
         *
         * tls_secure_algo - Use only secure algorithms. This will make us quit with an error if we can't negotiate a secure algo.
         */
        public readonly bool tls_secure_algo = true;

        /*
         * Daemon mode
         *
         * If you are running the process in the background and you don't need the keyboard reports, set this to true.
         * This should solve the hashrate problems on some emulated terminals.
         */
        public bool daemon_mode = false;

        /*
         * Buffered output control.
         * When running the miner through a pipe, standard output is buffered. This means that the pipe won't read
         * each output line immediately. This can cause delays when running in background.
         * Set this option to true to flush stdout after each line, so it can be read immediately.
         */
        public bool flush_stdout = false;

        /*
         * Output file
         *
         * output_file  - This option will log all output to a file.
         *
         */
        public string output_file = "";

        /*
         * Built-in web server
         * I like checking my hashrate on my phone. Don't you?
         * Keep in mind that you will need to set up port forwarding on your router if you want to access it from
         * outside of your home network. Ports lower than 1024 on Linux systems will require root.
         *
         * httpd_port - Port we should listen on. Default, 0, will switch off the server.
         */
        public int httpd_port = 0;

        /*
         * HTTP Authentication
         *
         * This allows you to set a password to keep people on the Internet from snooping on your hashrate.
         * Keep in mind that this is based on HTTP Digest, which is based on MD5. To a determined attacker
         * who is able to read your traffic it is as easy to break a bog door latch.
         *
         * http_login - Login. Empty login disables authentication.
         * http_pass  - Password.
         */
        public readonly string http_login = "";
        public readonly string http_pass = "";

        /*
         * prefer_ipv4 - IPv6 preference. If the host is available on both IPv4 and IPv6 net, which one should be choose?
         *               This setting will only be needed in 2020's. No need to worry about it now.
         */
        public readonly bool prefer_ipv4 = true;
    }

    public class XmrStakConfigCpu
    {
        private readonly int cpu_thread_num;
        public XmrStakConfigCpu(int numberOfthreads) {
            cpu_thread_num = numberOfthreads;
        }

        public void Inti_cpu_threads_conf(bool low_power_mode, bool no_prefetch, bool affine_to_cpu, bool isHyperThreading) {
            cpu_threads_conf = new List<JObject>();
            if (isHyperThreading) {
                for (int i_cpu = 0; i_cpu < cpu_thread_num; ++i_cpu) {
                    cpu_threads_conf.Add(JObject.FromObject(new { low_power_mode = low_power_mode, no_prefetch = no_prefetch, affine_to_cpu = i_cpu * 2 }));
                }
            } else {
                for (int i_cpu = 0; i_cpu < cpu_thread_num; ++i_cpu) {
                    if (affine_to_cpu) {
                        cpu_threads_conf.Add(JObject.FromObject(new { low_power_mode = low_power_mode, no_prefetch = no_prefetch, affine_to_cpu = i_cpu }));
                    } else {
                        cpu_threads_conf.Add(JObject.FromObject(new { low_power_mode = low_power_mode, no_prefetch = no_prefetch, affine_to_cpu = false }));
                    }
                }
            }
        }

        /*
         * Thread configuration for each thread. Make sure it matches the number above.
         * low_power_mode - This mode will double the cache usage, and double the single thread performance. It will 
         *                  consume much less power (as less cores are working), but will max out at around 80-85% of 
         *                  the maximum performance.
         *
         * no_prefetch -    Some sytems can gain up to extra 5% here, but sometimes it will have no difference or make
         *                  things slower.
         *
         * affine_to_cpu -  This can be either false (no affinity), or the CPU core number. Note that on hyperthreading 
         *                  systems it is better to assign threads to physical cores. On Windows this usually means selecting 
         *                  even or odd numbered cpu numbers. For Linux it will be usually the lower CPU numbers, so for a 4 
         *                  physical core CPU you should select cpu numbers 0-3.
         *
         * On the first run the miner will look at your system and suggest a basic configuration that will work,
         * you can try to tweak it from there to get the best performance.
         * 
         * A filled out configuration should look like this:
         * "cpu_threads_conf" :
         * [ 
         *      { "low_power_mode" : false, "no_prefetch" : true, "affine_to_cpu" : 0 },
         *      { "low_power_mode" : false, "no_prefetch" : true, "affine_to_cpu" : 1 },
         * ],
         */

        public List<JObject> cpu_threads_conf = new List<JObject>();
    }

    public class XmrStakConfigGpu
    {
        public class XmrStakGpuItem
        {
            public int index;
            public int threads = 24;
            public int blocks = 60;
            public int bfactor = 6;
            public int bsleep = 25;
            public bool affine_to_cpu;

            public XmrStakGpuItem(int index, int threads, bool affineToCpu) {
                this.index = index;
                this.threads = threads;
                affine_to_cpu = affineToCpu;
            }
        }

        public void SetupThreads(IEnumerable<int> indices) {
            var enumerable = indices as IList<int> ?? indices.ToList();
            // Remove threads without an activated index
            gpu_threads_conf = gpu_threads_conf.FindAll(x => enumerable.Contains(x.index));
            foreach (var i in enumerable) {
                if (gpu_threads_conf.All(x => x.index != i)) {
                    // Ideally this default shouldn't run, devices should be present in nvidia/amd.txt generated by xmr-stak with proper defaults
                    Helpers.ConsolePrint("Xmr-Stak-Config", $"GPU entry for index {i} not found, setting with default. Performance will likely not be optimal.");
                    gpu_threads_conf.Add(new XmrStakGpuItem(i, 24, false));
                }
            }
        }

        /*
         * GPU configuration. You should play around with threads and blocks as the fastest settings will vary.
         * index         - GPU index number usually starts from 0.
         * threads       - Number of GPU threads (nothing to do with CPU threads).
         * blocks        - Number of GPU blocks (nothing to do with CPU threads).
         * bfactor       - Enables running the Cryptonight kernel in smaller pieces.
         *                 Increase if you want to reduce GPU lag. Recommended setting on GUI systems - 8
         * bsleep        - Insert a delay of X microseconds between kernel launches.
         *                 Increase if you want to reduce GPU lag. Recommended setting on GUI systems - 100
         * affine_to_cpu - This will affine the thread to a CPU. This can make a GPU miner play along nicer with a CPU miner.
         *
         * On the first run the miner will look at your system and suggest a basic configuration that will work,
         * you can try to tweak it from there to get the best performance.
         *
         * A filled out configuration should look like this:
         * "gpu_threads_conf" :
         * [
         *     { "index" : 0, "threads" : 17, "blocks" : 60, "bfactor" : 0, "bsleep" :  0, "affine_to_cpu" : false},
         * ],
         */

        public List<XmrStakGpuItem> gpu_threads_conf = new List<XmrStakGpuItem>();

        /*
        * Platform index. This will be 0 unless you have different OpenCL platform - eg. AMD and Intel.
        */
        public int platform_index = 0;
    }
}
