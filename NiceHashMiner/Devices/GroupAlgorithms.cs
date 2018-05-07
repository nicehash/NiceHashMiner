using NiceHashMiner.Enums;
using System.Collections.Generic;
using NiceHashMiner.Algorithms;

namespace NiceHashMiner.Devices
{
    /// <summary>
    /// GroupAlgorithms creates defaults supported algorithms. Currently based in Miner implementation
    /// </summary>
    public static class GroupAlgorithms
    {
        private static Dictionary<MinerBaseType, List<Algorithm>> CreateForDevice(ComputeDevice device)
        {
            if (device == null) return null;
            var algoSettings = CreateDefaultsForGroup(device.DeviceGroupType);
            if (algoSettings == null) return null;
            if (device.DeviceType == DeviceType.AMD)
            {
                // sgminer stuff
                if (algoSettings.ContainsKey(MinerBaseType.sgminer))
                {
                    var sgminerAlgos = algoSettings[MinerBaseType.sgminer];
                    var lyra2REv2Index = sgminerAlgos.FindIndex(el => el.NiceHashID == AlgorithmType.Lyra2REv2);
                    var neoScryptIndex = sgminerAlgos.FindIndex(el => el.NiceHashID == AlgorithmType.NeoScrypt);
                    var cryptoNightIndex = sgminerAlgos.FindIndex(el => el.NiceHashID == AlgorithmType.CryptoNight);

                    // Check for optimized version
                    if (lyra2REv2Index > -1)
                    {
                        sgminerAlgos[lyra2REv2Index].ExtraLaunchParameters =
                            AmdGpuDevice.DefaultParam +
                            "--nfactor 10 --xintensity 64 --thread-concurrency 0 --worksize 64 --gpu-threads 2";
                    }
                    if (!device.Codename.Contains("Tahiti") && neoScryptIndex > -1)
                    {
                        sgminerAlgos[neoScryptIndex].ExtraLaunchParameters =
                            AmdGpuDevice.DefaultParam +
                            "--nfactor 10 --xintensity    2 --thread-concurrency 8192 --worksize  64 --gpu-threads 2";
                        Helpers.ConsolePrint("ComputeDevice",
                            "The GPU detected (" + device.Codename +
                            ") is not Tahiti. Changing default gpu-threads to 2.");
                    }
                    if (cryptoNightIndex > -1)
                    {
                        if (device.Codename.Contains("Hawaii"))
                        {
                            sgminerAlgos[cryptoNightIndex].ExtraLaunchParameters = "--rawintensity 640 -w 8 -g 2";
                        }
                        else if (device.Name.Contains("Vega"))
                        {
                            sgminerAlgos[cryptoNightIndex].ExtraLaunchParameters =
                                AmdGpuDevice.DefaultParam + " --rawintensity 1850 -w 8 -g 2";
                        }
                    }
                }

                // Ellesmere, Polaris
                // Ellesmere sgminer workaround, keep this until sgminer is fixed to work with Ellesmere
                if (device.Codename.Contains("Ellesmere") || device.InfSection.ToLower().Contains("polaris"))
                {
                    foreach (var algosInMiner in algoSettings)
                    {
                        foreach (var algo in algosInMiner.Value)
                        {
                            // disable all algos in list
                            if (algo.NiceHashID == AlgorithmType.Decred || algo.NiceHashID == AlgorithmType.Lbry)
                            {
                                algo.Enabled = false;
                            }
                        }
                    }
                }
                // non sgminer optimizations
                if (algoSettings.ContainsKey(MinerBaseType.Claymore_old) &&
                    algoSettings.ContainsKey(MinerBaseType.Claymore))
                {
                    var claymoreOldAlgos = algoSettings[MinerBaseType.Claymore_old];
                    var cryptoNightOldIndex =
                        claymoreOldAlgos.FindIndex(el => el.NiceHashID == AlgorithmType.CryptoNight);

                    var claymoreNewAlgos = algoSettings[MinerBaseType.Claymore];
                    var cryptoNightNewIndex =
                        claymoreNewAlgos.FindIndex(el => el.NiceHashID == AlgorithmType.CryptoNight);

                    if (cryptoNightOldIndex > -1 && cryptoNightNewIndex > -1)
                    {
                        //string regex_a_3 = "[5|6][0-9][0-9][0-9]";
                        var a4 = new List<string>
                        {
                            "270",
                            "270x",
                            "280",
                            "280x",
                            "290",
                            "290x",
                            "370",
                            "380",
                            "390",
                            "470",
                            "480"
                        };
                        foreach (var namePart in a4)
                        {
                            if (!device.Name.Contains(namePart)) continue;
                            claymoreOldAlgos[cryptoNightOldIndex].ExtraLaunchParameters = "-a 4";
                            break;
                        }

                        var old = new List<string>
                        {
                            "Verde",
                            "Oland",
                            "Bonaire"
                        };
                        foreach (var codeName in old)
                        {
                            var isOld = device.Codename.Contains(codeName);
                            claymoreOldAlgos[cryptoNightOldIndex].Enabled = isOld;
                            claymoreNewAlgos[cryptoNightNewIndex].Enabled = !isOld;
                        }
                    }
                }

                // drivers algos issue
                if (device.DriverDisableAlgos)
                {
                    algoSettings = FilterMinerAlgos(algoSettings, new List<AlgorithmType>
                    {
                        AlgorithmType.NeoScrypt,
                        AlgorithmType.Lyra2REv2
                    });
                }

                // disable by default
                {
                    var minerBases = new List<MinerBaseType>
                    {
                        MinerBaseType.ethminer,
                        MinerBaseType.OptiminerAMD
                    };
                    foreach (var minerKey in minerBases)
                    {
                        if (!algoSettings.ContainsKey(minerKey)) continue;
                        foreach (var algo in algoSettings[minerKey])
                        {
                            algo.Enabled = false;
                        }
                    }
                    if (algoSettings.ContainsKey(MinerBaseType.sgminer))
                    {
                        foreach (var algo in algoSettings[MinerBaseType.sgminer])
                        {
                            if (algo.NiceHashID == AlgorithmType.DaggerHashimoto)
                            {
                                algo.Enabled = false;
                            }
                        }
                    }
                    //if (algoSettings.ContainsKey(MinerBaseType.Claymore)) {
                    //    foreach (var algo in algoSettings[MinerBaseType.Claymore]) {
                    //        if (algo.NiceHashID == AlgorithmType.CryptoNight) {
                    //            algo.Enabled = false;
                    //        }
                    //    }
                    //}
                }
            } // END AMD case

            // check if it is Etherum capable
            if (device.IsEtherumCapale == false)
            {
                algoSettings = FilterMinerAlgos(algoSettings, new List<AlgorithmType>
                {
                    AlgorithmType.DaggerHashimoto
                });
            }

            if (algoSettings.ContainsKey(MinerBaseType.ccminer_alexis))
            {
                foreach (var unstableAlgo in algoSettings[MinerBaseType.ccminer_alexis])
                {
                    unstableAlgo.Enabled = false;
                }
            }
            if (algoSettings.ContainsKey(MinerBaseType.experimental))
            {
                foreach (var unstableAlgo in algoSettings[MinerBaseType.experimental])
                {
                    unstableAlgo.Enabled = false;
                }
            }

            // This is not needed anymore after excavator v1.1.4a
            //if (device.IsSM50() && algoSettings.ContainsKey(MinerBaseType.excavator)) {
            //    int Equihash_index = algoSettings[MinerBaseType.excavator].FindIndex((algo) => algo.NiceHashID == AlgorithmType.Equihash);
            //    if (Equihash_index > -1) {
            //        // -c1 1 needed for SM50 to work ATM
            //        algoSettings[MinerBaseType.excavator][Equihash_index].ExtraLaunchParameters = "-c1 1";
            //    }
            //}
            // NhEqMiner exceptions scope
            {
                const MinerBaseType minerBaseKey = MinerBaseType.nheqminer;
                if (algoSettings.ContainsKey(minerBaseKey) && device.Name.Contains("GTX")
                    && (device.Name.Contains("560") || device.Name.Contains("650") || device.Name.Contains("680") ||
                        device.Name.Contains("770"))
                )
                {
                    algoSettings = FilterMinerBaseTypes(algoSettings, new List<MinerBaseType>
                    {
                        minerBaseKey
                    });
                }
            }
            return algoSettings;
        }

        public static List<Algorithm> CreateForDeviceList(ComputeDevice device)
        {
            var ret = new List<Algorithm>();
            var retDict = CreateForDevice(device);
            if (retDict != null)
            {
                foreach (var kvp in retDict)
                {
                    ret.AddRange(kvp.Value);
                }
            }
            return ret;
        }

        public static Dictionary<MinerBaseType, List<Algorithm>> CreateDefaultsForGroup(DeviceGroupType deviceGroupType)
        {
            switch (deviceGroupType)
            {
                case DeviceGroupType.CPU:
                    return new Dictionary<MinerBaseType, List<Algorithm>>
                    {
                        {
                            MinerBaseType.XmrStak,
                            new List<Algorithm>
                            {
                                //new Algorithm(MinerBaseType.XmrStak, AlgorithmType.CryptoNight, "cryptonight"),
                                new Algorithm(MinerBaseType.XmrStak, AlgorithmType.CryptoNightV7, "")
                            }
                        },
                        //{
                        //    MinerBaseType.Xmrig,
                        //    new List<Algorithm>
                        //    {
                        //        //new Algorithm(MinerBaseType.Xmrig, AlgorithmType.CryptoNight, ""),
                        //        //new Algorithm(MinerBaseType.Xmrig, AlgorithmType.CryptoNightV7, "")
                        //    }
                        //}
                    };
                case DeviceGroupType.AMD_OpenCL:
                    // DisableAMDTempControl = false; TemperatureParam must be appended lastly
                    const string remDis = " --remove-disabled ";
                    var defaultParam = remDis + AmdGpuDevice.DefaultParam;
                    return new Dictionary<MinerBaseType, List<Algorithm>>
                    {
                        {
                            MinerBaseType.sgminer,
                            new List<Algorithm>
                            {
                                new Algorithm(MinerBaseType.sgminer, AlgorithmType.NeoScrypt, "neoscrypt")
                                {
                                    ExtraLaunchParameters =
                                        defaultParam +
                                        "--nfactor 10 --xintensity    2 --thread-concurrency 8192 --worksize  64 --gpu-threads 4"
                                },
                                //new Algorithm(MinerBaseType.sgminer, AlgorithmType.Lyra2REv2,  "Lyra2REv2") { ExtraLaunchParameters = DefaultParam + "--nfactor 10 --xintensity  160 --thread-concurrency    0 --worksize  64 --gpu-threads 1" },
                                new Algorithm(MinerBaseType.sgminer, AlgorithmType.DaggerHashimoto, "ethash")
                                {
                                    ExtraLaunchParameters = remDis + "--xintensity 512 -w 192 -g 1"
                                },
                                new Algorithm(MinerBaseType.sgminer, AlgorithmType.Decred, "decred")
                                {
                                    ExtraLaunchParameters =
                                        remDis + "--gpu-threads 1 --xintensity 256 --lookup-gap 2 --worksize 64"
                                },
                                new Algorithm(MinerBaseType.sgminer, AlgorithmType.Lbry, "lbry")
                                {
                                    ExtraLaunchParameters =
                                        defaultParam + "--xintensity 512 --worksize 128 --gpu-threads 2"
                                },
                                //new Algorithm(MinerBaseType.sgminer, AlgorithmType.CryptoNight, "cryptonight")
                                //{
                                //    ExtraLaunchParameters = defaultParam + "--rawintensity 512 -w 4 -g 2"
                                //},
                                new Algorithm(MinerBaseType.sgminer, AlgorithmType.Pascal, "pascal")
                                {
                                    ExtraLaunchParameters = defaultParam + "--intensity 21 -w 64 -g 2"
                                },
                                new Algorithm(MinerBaseType.sgminer, AlgorithmType.X11Gost, "sibcoin-mod")
                                {
                                    ExtraLaunchParameters = defaultParam + "--intensity 16 -w 64 -g 2"
                                },
                                new Algorithm(MinerBaseType.sgminer, AlgorithmType.Keccak, "keccak")
                                {
                                    ExtraLaunchParameters = defaultParam + "--intensity 15"
                                }
                            }
                        },
                        {
                            MinerBaseType.Claymore,
                            new List<Algorithm>
                            {
                                new Algorithm(MinerBaseType.Claymore, AlgorithmType.CryptoNightV7, ""),
                                new Algorithm(MinerBaseType.Claymore, AlgorithmType.Equihash, "equihash"),
                                new Algorithm(MinerBaseType.Claymore, AlgorithmType.DaggerHashimoto, ""),
                                new DualAlgorithm(MinerBaseType.Claymore, AlgorithmType.DaggerHashimoto, AlgorithmType.Decred),
                                new DualAlgorithm(MinerBaseType.Claymore, AlgorithmType.DaggerHashimoto, AlgorithmType.Lbry),
                                new DualAlgorithm(MinerBaseType.Claymore, AlgorithmType.DaggerHashimoto, AlgorithmType.Pascal),
                                new DualAlgorithm(MinerBaseType.Claymore, AlgorithmType.DaggerHashimoto, AlgorithmType.Sia),
                                new DualAlgorithm(MinerBaseType.Claymore, AlgorithmType.DaggerHashimoto, AlgorithmType.Blake2s),
                                new DualAlgorithm(MinerBaseType.Claymore, AlgorithmType.DaggerHashimoto, AlgorithmType.Keccak)
                            }
                        },
                        {
                            MinerBaseType.OptiminerAMD,
                            new List<Algorithm>
                            {
                                new Algorithm(MinerBaseType.OptiminerAMD, AlgorithmType.Equihash, "equihash")
                            }
                        },
                        {
                            MinerBaseType.Prospector,
                            new List<Algorithm>
                            {
                                new Algorithm(MinerBaseType.Prospector, AlgorithmType.Skunk, "sigt"),
                                new Algorithm(MinerBaseType.Prospector, AlgorithmType.Sia, "sia")
                            }
                        },
                        { 
                            MinerBaseType.XmrStak,
                            new List<Algorithm> {
                                //new Algorithm(MinerBaseType.XmrStak, AlgorithmType.CryptoNight, ""),
                                new Algorithm(MinerBaseType.XmrStak, AlgorithmType.CryptoNightV7, "")
                            }
                        }
                    };
                case DeviceGroupType.NVIDIA_2_1:
                case DeviceGroupType.NVIDIA_3_x:
                case DeviceGroupType.NVIDIA_5_x:
                case DeviceGroupType.NVIDIA_6_x:
                    var toRemoveAlgoTypes = new List<AlgorithmType>();
                    var toRemoveMinerTypes = new List<MinerBaseType>();
                    var ret = new Dictionary<MinerBaseType, List<Algorithm>>
                    {
                        {
                            MinerBaseType.ccminer,
                            new List<Algorithm>
                            {
                                new Algorithm(MinerBaseType.ccminer, AlgorithmType.NeoScrypt, "neoscrypt"),
                                new Algorithm(MinerBaseType.ccminer, AlgorithmType.Lyra2REv2, "lyra2v2"),
                                new Algorithm(MinerBaseType.ccminer, AlgorithmType.Decred, "decred"),
                                //new Algorithm(MinerBaseType.ccminer, AlgorithmType.CryptoNight, "cryptonight"),
                                new Algorithm(MinerBaseType.ccminer, AlgorithmType.Lbry, "lbry"),
                                new Algorithm(MinerBaseType.ccminer, AlgorithmType.X11Gost, "sib"),
                                new Algorithm(MinerBaseType.ccminer, AlgorithmType.Blake2s, "blake2s"),
                                new Algorithm(MinerBaseType.ccminer, AlgorithmType.Sia, "sia"),
                                //new Algorithm(MinerBaseType.ccminer, AlgorithmType.Nist5, "nist5"),
                                new Algorithm(MinerBaseType.ccminer, AlgorithmType.Keccak, "keccak"),
                                new Algorithm(MinerBaseType.ccminer, AlgorithmType.Skunk, "skunk"),
                                //new Algorithm(MinerBaseType.ccminer, AlgorithmType.CryptoNightV7, "cryptonight")
                            }
                        },
                        {
                            MinerBaseType.ccminer_alexis,
                            new List<Algorithm>
                            {
                                new Algorithm(MinerBaseType.ccminer_alexis, AlgorithmType.X11Gost, "sib"),
                                //new Algorithm(MinerBaseType.ccminer_alexis, AlgorithmType.Nist5, "nist5"),
                                new Algorithm(MinerBaseType.ccminer_alexis, AlgorithmType.Keccak, "keccak")
                            }
                        },
                        {
                            MinerBaseType.ethminer,
                            new List<Algorithm>
                            {
                                new Algorithm(MinerBaseType.ethminer, AlgorithmType.DaggerHashimoto, "daggerhashimoto")
                            }
                        },
                        {
                            MinerBaseType.nheqminer,
                            new List<Algorithm>
                            {
                                new Algorithm(MinerBaseType.nheqminer, AlgorithmType.Equihash, "equihash")
                            }
                        },
                        //{
                        //    MinerBaseType.excavator,
                        //    new List<Algorithm>
                        //    {
                        //        new Algorithm(MinerBaseType.excavator, AlgorithmType.Equihash, "equihash"),
                        //        new Algorithm(MinerBaseType.excavator, AlgorithmType.Pascal, "pascal")
                        //    }
                        //},
                        {
                            MinerBaseType.EWBF,
                            new List<Algorithm>
                            {
                                new Algorithm(MinerBaseType.EWBF, AlgorithmType.Equihash, "")
                            }
                        },
                        {
                            MinerBaseType.Claymore,
                            new List<Algorithm>
                            {
                                new Algorithm(MinerBaseType.Claymore, AlgorithmType.DaggerHashimoto, ""),
                                new DualAlgorithm(MinerBaseType.Claymore, AlgorithmType.DaggerHashimoto, AlgorithmType.Decred),
                                new DualAlgorithm(MinerBaseType.Claymore, AlgorithmType.DaggerHashimoto, AlgorithmType.Lbry),
                                new DualAlgorithm(MinerBaseType.Claymore, AlgorithmType.DaggerHashimoto, AlgorithmType.Pascal),
                                new DualAlgorithm(MinerBaseType.Claymore, AlgorithmType.DaggerHashimoto, AlgorithmType.Sia),
                                new DualAlgorithm(MinerBaseType.Claymore, AlgorithmType.DaggerHashimoto, AlgorithmType.Blake2s),
                                new DualAlgorithm(MinerBaseType.Claymore, AlgorithmType.DaggerHashimoto, AlgorithmType.Keccak)
                            }
                        },
                        {
                            MinerBaseType.dtsm,
                            new List<Algorithm>
                            {
                                new Algorithm(MinerBaseType.dtsm, AlgorithmType.Equihash, "")
                            }
                        },
                        { 
                            MinerBaseType.XmrStak,
                            new List<Algorithm> {
                                //new Algorithm(MinerBaseType.XmrStak, AlgorithmType.CryptoNight, ""),
                                new Algorithm(MinerBaseType.XmrStak, AlgorithmType.CryptoNightV7, "")
                            }
                        }
                    };
                    switch (deviceGroupType)
                    {
                        case DeviceGroupType.NVIDIA_6_x:
                        case DeviceGroupType.NVIDIA_5_x:
                            toRemoveMinerTypes.AddRange(new[]
                            {
                                MinerBaseType.nheqminer
                            });
                            break;
                        case DeviceGroupType.NVIDIA_2_1:
                        case DeviceGroupType.NVIDIA_3_x:
                            toRemoveAlgoTypes.AddRange(new[]
                            {
                                AlgorithmType.NeoScrypt,
                                AlgorithmType.Lyra2RE,
                                AlgorithmType.Lyra2REv2,
                                AlgorithmType.CryptoNightV7
                            });
                            toRemoveMinerTypes.AddRange(new[]
                            {
                                MinerBaseType.eqm,
                                MinerBaseType.EWBF,
                                MinerBaseType.dtsm
                            });
                            break;
                    }
                    if (DeviceGroupType.NVIDIA_2_1 == deviceGroupType)
                    {
                        toRemoveAlgoTypes.AddRange(new[]
                        {
                            AlgorithmType.DaggerHashimoto,
                            //AlgorithmType.CryptoNight,
                            AlgorithmType.Pascal,
                            AlgorithmType.X11Gost
                        });
                        toRemoveMinerTypes.AddRange(new[]
                        {
                            MinerBaseType.Claymore,
                            MinerBaseType.XmrStak
                        });
                    }

                    // filter unused
                    var finalRet = FilterMinerAlgos(ret, toRemoveAlgoTypes, new List<MinerBaseType>
                    {
                        MinerBaseType.ccminer
                    });
                    finalRet = FilterMinerBaseTypes(finalRet, toRemoveMinerTypes);

                    return finalRet;
            }

            return null;
        }

        private static Dictionary<MinerBaseType, List<Algorithm>> FilterMinerBaseTypes(
            Dictionary<MinerBaseType, List<Algorithm>> minerAlgos, List<MinerBaseType> toRemove)
        {
            var finalRet = new Dictionary<MinerBaseType, List<Algorithm>>();
            foreach (var kvp in minerAlgos)
            {
                if (toRemove.IndexOf(kvp.Key) == -1)
                {
                    finalRet[kvp.Key] = kvp.Value;
                }
            }
            return finalRet;
        }

        private static Dictionary<MinerBaseType, List<Algorithm>> FilterMinerAlgos(
            Dictionary<MinerBaseType, List<Algorithm>> minerAlgos, List<AlgorithmType> toRemove,
            List<MinerBaseType> toRemoveBase = null)
        {
            var finalRet = new Dictionary<MinerBaseType, List<Algorithm>>();
            if (toRemoveBase == null)
            {
                // all minerbasekeys
                foreach (var kvp in minerAlgos)
                {
                    var algoList = kvp.Value.FindAll(a => toRemove.IndexOf(a.NiceHashID) == -1);
                    if (algoList.Count > 0)
                    {
                        finalRet[kvp.Key] = algoList;
                    }
                }
            }
            else
            {
                foreach (var kvp in minerAlgos)
                {
                    // filter only if base key is defined
                    if (toRemoveBase.IndexOf(kvp.Key) > -1)
                    {
                        var algoList = kvp.Value.FindAll(a => toRemove.IndexOf(a.NiceHashID) == -1);
                        if (algoList.Count > 0)
                        {
                            finalRet[kvp.Key] = algoList;
                        }
                    }
                    else
                    {
                        // keep all
                        finalRet[kvp.Key] = kvp.Value;
                    }
                }
            }
            return finalRet;
        }

        //static List<AlgorithmType> GetKeysForMinerAlgosGroup(Dictionary<MinerBaseType, List<Algorithm>> minerAlgos) {
        //    List<AlgorithmType> ret = new List<AlgorithmType>();
        //    foreach (var kvp in minerAlgos) {
        //        var currentKeys = kvp.Value.ConvertAll((a) => a.NiceHashID);
        //        foreach (var key in currentKeys) {
        //            if (ret.Contains(key) == false) {
        //                ret.Add(key);
        //            }
        //        }
        //    }
        //    return ret;
        //}
    }
}
