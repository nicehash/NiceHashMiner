using NHM.Common.Enums;
using System;
using System.Collections.Generic;

namespace NHMCore.Configs.Data
{
    [Serializable]
    public class PluginAlgorithmConfig
    {
        public string Name = ""; // Used as an indicator for easier user interaction
        public string PluginUUID;
        public string PluginVersion;
        public string AlgorithmIDs;
        public List<double> Speeds;
        public string ExtraLaunchParameters = "";
        public bool Enabled = true;
        public double PowerUsage = 0;

        public List<AlgorithmType> GetAlgorithmIDs()
        {
            var ret = new List<AlgorithmType>();
            var ids = AlgorithmIDs.Split('-');
            foreach (var id in ids)
            {

                if (Enum.TryParse(id, out AlgorithmType enumId))
                {
                    ret.Add(enumId);
                }
                else
                {
                    ret.Add(AlgorithmType.INVALID);
                }
            }

            return ret;
        }

        public string GetAlgorithmStringID()
        {
            var IDs = GetAlgorithmIDs().ToArray();
            var algorithmName = IDs.GetNameFromAlgorithmTypes();
            return $"{algorithmName}_{PluginUUID}";
        }

        public Version GetVersion()
        {
            int major = 1;
            int minor = 0;
            var ids = PluginVersion.Split('.');
            if (ids.Length > 0) int.TryParse(ids[0], out major);
            if (ids.Length > 1) int.TryParse(ids[1], out minor);
            return new Version(major, minor);
        }
    }
}

