using NHM.MinerPluginToolkitV1.Configs;
using NHM.MinerPluginToolkitV1.ExtraLaunchParameters;
using System.Collections.Generic;

namespace LolMiner
{
    internal static class PluginInternalSettings
    {
        internal static MinerSystemEnvironmentVariables MinerSystemEnvironmentVariables = new MinerSystemEnvironmentVariables
        {
            DefaultSystemEnvironmentVariables = new Dictionary<string, string>
            {
                {"GPU_MAX_ALLOC_PERCENT", "100"},
                {"GPU_SINGLE_ALLOC_PERCENT", "100"},
                {"GPU_MAX_HEAP_SIZE", "100"},
                {"GPU_FORCE_64BIT_PTR", "1"},
                {"GPU_USE_SYNC_OBJECTS", "1"}
            }
        };

        internal static MinerOptionsPackage MinerOptionsPackage = new MinerOptionsPackage
        {
            GeneralOptions = new List<MinerOption>
            {
                /// <summary>
                /// Enables printing a log file; --log [=arg(=on)
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "lolMiner_enableLogs",
                    ShortName = "--log"
                },
                /// <summary>
                /// Path to a custom log file location
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "lolMiner_logFile",
                    ShortName = "--logfile"
                },

                /// <summary>
                /// Long statistics interval
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "lolMiner_longStats",
                    ShortName = "--longstats",
                    DefaultValue = "150"
                },
                /// <summary>
                /// Short statistics interval
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "lolMiner_shortStats",
                    ShortName = "--shortstats",
                    DefaultValue = "30"
                },
                /// <summary>
                /// Number of digits in hash speed after delimiter
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "lolMiner_decimalDigits",
                    ShortName = "--digits"
                },
                /// <summary>
                /// Enables time stamp on short statistics ("on" / "off"); --timeprint [=arg(=on)] (=off)
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "lolMiner_enableTimestamps",
                    ShortName = "--timeprint"
                },
                /// <summary>
                /// Sets the memory size (in MByte) the
                /// miner is allowed for Ethash on 4G
                /// cards. Suggested values: Windows: 4024
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "lolMiner_4GAllocSize",
                    ShortName = "--4g-alloc-size",
                },
                /// <summary>
                /// Windows: added experimental mem allocation pattern that should allow reaching epoch 375 or 376 at full speed ( * ).
                /// It is default on in Windows, you can turn it off with "--win4galloc 0"
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "lolMiner_mem4GAlloc",
                    ShortName = "--win4galloc",
                },
                /// <summary>
                /// Set the number of MBytes of GPU memory that should be left free by the miner.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "lolMiner_keepFree",
                    ShortName = "--keepfree",
                },
                /// <summary>
                /// This will disable the 2nd mining thread and slightly reduce performance of the involved cards.
                /// Use this option to reduce stumbles when a card does graphic output in parallel.
                /// Use --singlethread to set the mode for one single card
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "lolMiner_SingleThread",
                    ShortName = "--singlethread",
                },
                /// <summary>
                /// for Polaris GPUs. This will increase the performance of zombie mode (further up on the general improvement) by an other 5-15%, depending on parameter and epoch (later epochs profit more).
                /// Default value is 0 (off), for most cards the value of 2 is optimal. If you see cards getting slower then before, set to 0 or 1.
                /// Note: you either can give one value for the whole rig or provide a comma separated list for each card individually. Cards not running zombie mode ignore the parameter.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "lolMiner_zombieTune",
                    ShortName = "--zombie-tune",
                    Delimiter = ",",
                    DefaultValue = "0"
                },
                /// <summary>
                /// (default 0.0.0.0) which controls to which host address the api binds. Use 127.0.0.1 to restrict api access to only your computer,
                /// 0.0.0.0 is equivalent to everyone can access when rig is reachable on the used apiport. IPV6 ip addresses should be supported, but is untested.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "lolMiner_apihost",
                    ShortName = "--apihost"
                },
                /// <summary>
                /// Added verify routine for Ethash dag epochs 400 to 450. In case the miner will detect defect entries, the CPU will try to fix this.
                /// Mining will be paused until the repair is completed.
                /// Use --disable-dag-verify to disable the verify & repair mechanism routine.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "lolMiner_disable-dag-verify",
                    ShortName = "--disable-dag-verify",
                },
                /// <summary>
                /// The "wall of stats". To personalize your stats there are different parameters, you only need to select what you want to show. You have to add --statsformat and values separeted by comma
                /// gpuName : A shortcut name of the GPU, e.g. "RX 580" or "RTX 2060"
                /// algo : The current algorithm running. Only visible in dual modes
                ///speed : Speed in Mhs of the rig
                ///inflatedHr : Inflated Speed Mhs to compare it to the stats of inflated miners
                ///poolHr: Pool Average, it will say how luck or unluck are you. If it is higher than the speed that means you are lucky if it is lower you are in an unlucky moment. That will take in long time to the same as speed.
                ///shares: Number of shares submited. A: parameter is shares, S: are Stales and Hw: are Hardware errors
                ///sharesPerMin : Number of shares accepted per minute
                ///bestShare: Best share you have
                ///power: Power Consumption, normally in Nvidia is quite real to the Wall Watts and AMD is different depending on the board
                ///hrPerWatt: Efficiency of Mhs you get for 1 Watts usage
                ///wattPerHr: Efficiency of the Watts you need to have 1 Mhs
                ///coreclk: Core Clock in Mhz
                ///memclk: Memory Clock in Mhz, that depending on Nvidia or AMD it is show different
                ///coreT: Temperature of the Core
                ///juncT: Temperature Junction of the Core, this will be avalaible in all Polaris & newer
                ///memT: Memory Temperature, this is not avalaible in all the GPUs, at the moment only in Vega, Navi and Big Navi
                ///fanPct : Fan speed in %
                ///speedctxc : Does the g/s to h/s conversion for cortex (CTCX)
                ///fidelity : Cuckoo Solver Accuracy, fraction of found graph cycles over expected number
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "lolMiner_statsformat",
                    ShortName = "--statsformat",
                },
                /// <summary>
                /// --mode controls the loaded kernel and operation modes for Nvidia GPUs.
                /// AMD cards will currently ignore this parameter completely. You can set one value for the whole rig or provide a comma separated list of values to get individual modes for each card.
                /// Amd cards can use any value at the moment.
                /// --mode a This is the often fastest kernel when you do not apply any power limit.
                /// Gives better performance especially on RTX 3080 and 3090, but not so much on the lower tier GPUs. Use this one when power draw does not matter any ways.
                /// --mode b This is a power saving kernel an the default. Good choice for all Non-LHR Nvidia GPUs and RTX 3060 LHR V1 on the known 470.05 beta Windows driver.
                /// Also this mode will be used in glitch mode.
                /// --mode LHR1 This is the default mode for RTX 3060 LHR v1 when the driver version is between 455.45.01 and 460.39.
                /// Gives up to 80% of the potential unlocked raw card performance. In case other driver versions get used, the miner falls back to the LHR2 mode.
                /// --mode LHR2 This mode is made for all other LHR (v2) Nvidia RTX 3000 cards. Performance is approximately 67-69% of the potential unlocked raw card performance.
                /// Note that the mode was tested with a locked core clock of 1500 mhz on all cards.
                /// Higher core clock may work, but could also cause the lock to trigger on your card, especially when the card is only power limit bound and has no fixed upper core bound!
                /// --mode LHRLP With this low clocks the performance is generally lower, but the LHR semi-unlock parameters can get relaxed to give a higher share of the potential the GPU has.</summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "lolMiner_mode",
                    ShortName = "--mode",
                    Delimiter = ",",
                    DefaultValue = "missing"
                },
                /// <summary>
                /// The values we have chosen as parameters were taken after elaborate tests. Sadly the best config changed from rig to rig, so there is a parameter --lhrtune that takes a comma separated list of values.
                /// Negatives will relax settings, so the GPU will more likely unlock, positives will make the lock more likely, but improve performance.
                /// If you have a card not starting unlocked, try a value of -20 as a start, lower might be needed. This is true especially when you have your card in motherboard x16 slot.
                /// If your cards are super stable try to increase in small steps of 5.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "lolMiner_lhrtune",
                    ShortName = "--lhrtune",
                    Delimiter = ","
                },
                /// <summary>
                /// Use parameter --watchdog off/exit/script to turn off any action, exit the miner with a specific exit code or to run an external script.
                ///--watchdog off
                ///This will do nothing except for printing a message. If only a single card did crash and not the whole driver this means the other cards will continue mining.
                ///--watchdog exit
                ///This will close the miner with a exit code of 42. This can be picked up by the .sh or .bat script that did start the miner
                ///(an example is provided in mine_eth.sh and mine_eth.bat) so the miner will restart after some seconds of pause. This is recommended option for Nvidia cards.
                ///--watchdog script
                ///With this option the miner will call an external script (default path is current working directory and there emergency.sh / .bat), which can be configured with --watchdogscript. The moment the
                ///script is called the miner itself will exit. The script needs to take care about rebooting the rig or informing the OS what to do. Since this was the default behavior in previous versions it also is
                ///the default. In case the script can not be found, an error will be printed and the miner will continue as with --watchdog off.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "lolMiner_watchdog",
                    ShortName = "--watchdog"
                }
            }
        };
    }
}
