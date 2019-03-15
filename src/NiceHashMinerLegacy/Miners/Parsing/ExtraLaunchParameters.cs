using NiceHashMiner.Configs.ConfigJsonFile;
using System;
using System.Collections.Generic;
using NiceHashMinerLegacy.Common.Enums;

namespace NiceHashMiner.Miners.Parsing
{
    public class MinerOptionPackageFile : ConfigFile<MinerOptionPackage>
    {
        public MinerOptionPackageFile(string name)
            : base(Folders.Internals, $"{name}.json", $"{name}.json")
        { }
    }

    public static class ExtraLaunchParameters
    {
        private static readonly List<MinerOptionPackage> Defaults = new List<MinerOptionPackage>
        {
            new MinerOptionPackage(
                MinerType.ccminer,
                new List<MinerOption>
                {
                    new MinerOption("Intensity", "-i", "--intensity=", "0", MinerOptionFlagType.MultiParam, ","),
                    new MinerOption("ccminer_CUDA_Schedule", "--cuda-schedule=", "0", MinerOptionFlagType.Uni)
                }
            ),
            new MinerOptionPackage(
                MinerType.ethminer_OCL,
                new List<MinerOption>
                {
                    new MinerOption("LocalWork", "", "--cl-local-work", "0", MinerOptionFlagType.MultiParam, ","),
                    new MinerOption("GlobalWork", "", "--cl-global-work", "0", MinerOptionFlagType.MultiParam, ","),
                }
            ),
            new MinerOptionPackage(
                MinerType.ethminer_CUDA,
                new List<MinerOption>
                {
                    new MinerOption("CudaBlockSize", "", "--cuda-block-size", "0", MinerOptionFlagType.MultiParam, ","),
                    new MinerOption("CudaGridSize", "", "--cuda-grid-size", "0", MinerOptionFlagType.MultiParam, ","),
                }
            ),
            new MinerOptionPackage(
                MinerType.sgminer,
                new List<MinerOption>
                {
                    // SingleParam
                    new MinerOption("KeccakUnroll", "", "--keccak-unroll", "0", MinerOptionFlagType.SingleParam),
                    new MinerOption("HamsiExpandBig", "", "--hamsi-expand-big", "4", MinerOptionFlagType.SingleParam),
                    new MinerOption("Nfactor", "", "--nfactor", "10", MinerOptionFlagType.SingleParam),
                    // MultiParam TODO IMPORTANT check defaults
                    new MinerOption("Intensity", "-I", "--intensity", "d", MinerOptionFlagType.MultiParam,
                        ","), // default is "d" check if -1 works
                    new MinerOption("Xintensity", "-X", "--xintensity", "-1", MinerOptionFlagType.MultiParam, ","), // default none
                    new MinerOption("Rawintensity", "", "--rawintensity", "-1", MinerOptionFlagType.MultiParam, ","), // default none
                    new MinerOption("ThreadConcurrency", "", "--thread-concurrency", "-1", MinerOptionFlagType.MultiParam,
                        ","), // default none
                    new MinerOption("Worksize", "-w", "--worksize", "-1", MinerOptionFlagType.MultiParam, ","), // default none
                    new MinerOption("GpuThreads", "-g", "--gpu-threads", "1", MinerOptionFlagType.MultiParam, ","),
                    new MinerOption("LookupGap", "", "--lookup-gap", "-1", MinerOptionFlagType.MultiParam, ","), // default none
                    // Uni
                    new MinerOption("RemoveDisabled", "--remove-disabled", "--remove-disabled", "",
                        MinerOptionFlagType.Uni), // default none
                },
                // TemperatureOptions
                new List<MinerOption>
                {
                    new MinerOption("GpuFan", "", "--gpu-fan", "30-60", MinerOptionFlagType.MultiParam, ","), // default none
                    new MinerOption("TempCutoff", "", "--temp-cutoff", "95", MinerOptionFlagType.MultiParam, ","),
                    new MinerOption("TempOverheat", "", "--temp-overheat", "85", MinerOptionFlagType.MultiParam, ","),
                    new MinerOption("TempTarget", "", "--temp-target", "75", MinerOptionFlagType.MultiParam, ","),
                    new MinerOption("AutoFan", "", "--auto-fan", null, MinerOptionFlagType.Uni),
                    new MinerOption("AutoGpu", "", "--auto-gpu", null, MinerOptionFlagType.Uni)
                }
            ),
            new MinerOptionPackage(
                MinerType.ClaymoreDual,
                new List<MinerOption>
                {
                    new MinerOption("ClaymoreDual_etha", "-etha", "-etha", "-1", MinerOptionFlagType.MultiParam, ","),
                    new MinerOption("ClaymoreDual_ethi", "-ethi", "-ethi", "8", MinerOptionFlagType.MultiParam, ","),

                    new MinerOption("ClaymoreDual_eres", "-eres", "-eres", "2", MinerOptionFlagType.SingleParam, ","),
                    new MinerOption("ClaymoreDual_erate", "-erate", "-erate", "1", MinerOptionFlagType.SingleParam, ","),
                    new MinerOption("ClaymoreDual_estale", "-estale", "-estale", "1", MinerOptionFlagType.SingleParam, ","),
                    new MinerOption("ClaymoreDual_gser", "-gser", "-gser", "0", MinerOptionFlagType.SingleParam, ","),

                    new MinerOption("ClaymoreDual_wd", "-wd", "-wd", "1", MinerOptionFlagType.SingleParam, ","),
                    new MinerOption("ClaymoreDual_retrydelay", "-retrydelay", "-retrydelay", "20", MinerOptionFlagType.SingleParam, ","),
                    new MinerOption("ClaymoreDual_nofee", "-nofee", "-nofee", "0", MinerOptionFlagType.SingleParam, ","),
                    new MinerOption("ClaymoreDual_li", "-li", "-li", "0", MinerOptionFlagType.MultiParam, ","),
                    new MinerOption("ClaymoreDual_lidag", "-lidag", "-lidag", "0", MinerOptionFlagType.MultiParam, ","),

                    //MinerOptionFlagType.MultiParam might not work corectly due to ADL indexing so use single param to apply to all
                    new MinerOption("ClaymoreDual_cclock", "-cclock", "-cclock", "0", MinerOptionFlagType.MultiParam, ","),
                    new MinerOption("ClaymoreDual_mclock", "-mclock", "-mclock", "0", MinerOptionFlagType.MultiParam, ","),
                    new MinerOption("ClaymoreDual_powlim", "-powlim", "-powlim", "0", MinerOptionFlagType.MultiParam, ","),
                    new MinerOption("ClaymoreDual_cvddc", "-cvddc", "-cvddc", "0", MinerOptionFlagType.MultiParam, ","),
                    new MinerOption("ClaymoreDual_mvddc", "-mvddc", "-mvddc", "0", MinerOptionFlagType.MultiParam, ","),

                    // other and dual mining features
                    new MinerOption("ClaymoreDual_etht", "-etht", "-etht", "200", MinerOptionFlagType.SingleParam, ","),
                    new MinerOption("ClaymoreDual_allcoins", "-allcoins", "-allcoins", "", MinerOptionFlagType.SingleParam, ","),
                    new MinerOption("ClaymoreDual_r", "-r", "-r", "0", MinerOptionFlagType.SingleParam, ","),
                    new MinerOption("ClaymoreDual_mode", "-mode", "-mode", "0", MinerOptionFlagType.SingleParam,
                        ","), // this is not well supported in MultiParam

                    new MinerOption("ClaymoreDual_ftime", "-ftime", "-ftime", "0", MinerOptionFlagType.SingleParam, ","),
                    new MinerOption("ClaymoreDual_eres", "-eres", "-eres", "2", MinerOptionFlagType.SingleParam, ","),
                    new MinerOption("ClaymoreDual_dpool", "-dpool", "-dpool", "", MinerOptionFlagType.SingleParam, ","),
                    new MinerOption("ClaymoreDual_dwal", "-dwal", "-dwal", "", MinerOptionFlagType.SingleParam, ","),
                    new MinerOption("ClaymoreDual_dpsw", "-dpsw", "-dpsw", "", MinerOptionFlagType.SingleParam, ","),
                    new MinerOption("ClaymoreDual_dcoin", "-dcoin", "-dcoin", "", MinerOptionFlagType.SingleParam, ","),
                    new MinerOption("ClaymoreDual_dcri", "-dcri", "-dcri", "30", MinerOptionFlagType.MultiParam, ","),

                    new MinerOption("ClaymoreDual_dcrt", "-dcrt", "-dcrt", "5", MinerOptionFlagType.SingleParam, ","),
                    // v9.0
                    new MinerOption("ClaymoreDual_asm", "-asm", "-asm", "1", MinerOptionFlagType.MultiParam, ","),
                },
                new List<MinerOption>
                {
                    // temperature stuff
                    //MinerOptionFlagType.MultiParam might not work corectly due to ADL indexing so use single param to apply to all
                    new MinerOption("ClaymoreDual_tt", "-tt", "-tt", "1", MinerOptionFlagType.SingleParam, ","),
                    new MinerOption("ClaymoreDual_ttdcr", "-ttdcr", "-ttdcr", "1", MinerOptionFlagType.SingleParam, ","),
                    new MinerOption("ClaymoreDual_ttli", "-ttli", "-ttli", "70", MinerOptionFlagType.SingleParam, ","),
                    new MinerOption("ClaymoreDual_tstop", "-tstop", "-tstop", "0", MinerOptionFlagType.SingleParam, ","),
                    new MinerOption("ClaymoreDual_fanmax", "-fanmax", "-fanmax", "100", MinerOptionFlagType.MultiParam, ","),
                    new MinerOption("ClaymoreDual_fanmin", "-fanmin", "-fanmin", "0", MinerOptionFlagType.MultiParam, ","),
                }
            ),
            new MinerOptionPackage(
                MinerType.EWBF,
                new List<MinerOption>
                {
                    // parameters differ according to algorithm
                    new MinerOption("EWBF_fee", "--fee", "--fee", "0", MinerOptionFlagType.SingleParam, " "),
                    new MinerOption("EWBF_templimit", "--templimit", "--templimit", "90", MinerOptionFlagType.SingleParam, " "),
                    new MinerOption("EWBF_tempunits", "--tempunits", "--tempunits", "C", MinerOptionFlagType.SingleParam, " "),
                    new MinerOption("EWBF_eexit", "--eexit", "--eexit", null, MinerOptionFlagType.SingleParam, " "),
                    new MinerOption("EWBF_solver", "--solver", "--solver", "0", MinerOptionFlagType.MultiParam, " "),
                    new MinerOption("EWBF_intensity", "--intensity", "--intensity", "64", MinerOptionFlagType.MultiParam, " "),
                    new MinerOption("EWBF_powercalc", "--pec", "--pec", null, MinerOptionFlagType.Uni, " "),
                }
            ),
            new MinerOptionPackage(
                MinerType.Phoenix,
                new List<MinerOption>
                {
                    new MinerOption("phoenix_mi", "-mi", "12", MinerOptionFlagType.MultiParam, ","),
                    new MinerOption("phoenix_gt", "-gt", "0", MinerOptionFlagType.MultiParam, ","),
                    new MinerOption("phoenix_sci", "-sci", "30", MinerOptionFlagType.MultiParam, ","),
                    new MinerOption("phoenix_clKernel", "-clKernel", "1", MinerOptionFlagType.MultiParam, ","),
                    new MinerOption("phoenix_clgreen", "-clgreen", "0", MinerOptionFlagType.MultiParam, ","),
                    new MinerOption("phoenix_clNew", "-clNew", "1", MinerOptionFlagType.MultiParam, ","),
                    new MinerOption("phoenix_clf", "-clf", "1", MinerOptionFlagType.MultiParam, ","),
                    new MinerOption("phoenix_nvNew", "-nvNew", "1", MinerOptionFlagType.MultiParam, ","),
                    new MinerOption("phoenix_nvf", "-nvf", "1", MinerOptionFlagType.MultiParam, ","),
                    new MinerOption("phoenix_eres", "-eres", "2", MinerOptionFlagType.MultiParam, ","),
                    new MinerOption("phoenix_lidag", "-lidag", "0", MinerOptionFlagType.MultiParam, ","),
                    new MinerOption("phoenix_gser", "-gser", "0", MinerOptionFlagType.SingleParam),
                    new MinerOption("phoenix_altinit", "-altinit", null, MinerOptionFlagType.Uni),
                    new MinerOption("phoenix_gpow", "-gpow", "100", MinerOptionFlagType.MultiParam, ","),
                    new MinerOption("phoenix_li", "-li", "0", MinerOptionFlagType.MultiParam, ",")

                }
            ),
            new MinerOptionPackage(
                MinerType.GMiner,
                new List<MinerOption>
                {
                    new MinerOption("gminer_templimit", "-t", "--templimit", "90", MinerOptionFlagType.MultiParam, " "),
                    new MinerOption("gminer_pec", "--pec", "1", MinerOptionFlagType.SingleParam),
                    new MinerOption("gminer_electricity", "--electricity_cost", null, MinerOptionFlagType.SingleParam)
                }
            ),
            new MinerOptionPackage(
            MinerType.BMiner,
            new List<MinerOption>
                {
                    new MinerOption("bminer_max_temp", "-max-temperature", "85", MinerOptionFlagType.MultiParam),
                    new MinerOption("bminer_nofee", "-nofee", null, MinerOptionFlagType.Uni)
                }
            ),
            new MinerOptionPackage(
            MinerType.TTMiner,
            new List<MinerOption>
                {
                    new MinerOption("ttminer_intensity", "-i", "-1", MinerOptionFlagType.MultiParam, ","),
                    new MinerOption("ttminer_intensity_grid", "-ig", "-1", MinerOptionFlagType.MultiParam, ","),
                    new MinerOption("ttminer_grid_size", "-gs", "-1", MinerOptionFlagType.MultiParam, ","),
                }
            ),
        };

        private static readonly List<MinerOptionPackage> MinerOptionPackages = new List<MinerOptionPackage>();

        public static void InitializePackages()
        {
            foreach (var pack in Defaults)
            {
                var packageName = $"MinerOptionPackage_{pack.Name}";
                var packageFile = new MinerOptionPackageFile(packageName);
                var readPack = packageFile.ReadFile();
                if (readPack == null)
                {
                    // read has failed
                    Helpers.ConsolePrint("ExtraLaunchParameters", "Creating internal params config " + packageName);
                    MinerOptionPackages.Add(pack);
                    // create defaults
                    packageFile.Commit(pack);
                }
                else
                {
                    Helpers.ConsolePrint("ExtraLaunchParameters", "Loading internal params config " + packageName);
                    MinerOptionPackages.Add(readPack);
                }
            }
            var defaultKeys = Defaults.ConvertAll(p => p.Type);
            // extra check if DEFAULTS is missing a key
            for (var type = (MinerType.NONE + 1); type < MinerType.END; ++type)
            {
                if (defaultKeys.Contains(type) == false)
                {
                    var packageName = $"MinerOptionPackage_{Enum.GetName(typeof(MinerType), type)}";
                    var packageFile = new MinerOptionPackageFile(packageName);
                    var readPack = packageFile.ReadFile();
                    if (readPack != null)
                    {
                        // read has failed
                        Helpers.ConsolePrint("ExtraLaunchParameters", "Creating internal params config " + packageName);
                        MinerOptionPackages.Add(readPack);
                    }
                }
            }
        }

        public static MinerOptionPackage GetMinerOptionPackageForMinerType(MinerType type)
        {
            var index = MinerOptionPackages.FindIndex(p => p.Type == type);
            if (index > -1)
            {
                return MinerOptionPackages[index];
            }
            // if none found
            return new MinerOptionPackage(MinerType.NONE, new List<MinerOption>(), new List<MinerOption>());
        }
    }
}
