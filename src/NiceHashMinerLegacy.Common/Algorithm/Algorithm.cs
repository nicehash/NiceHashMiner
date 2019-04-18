using NiceHashMinerLegacy.Common.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace NiceHashMinerLegacy.Common.Algorithm
{
    public class Algorithm
    {
        public Algorithm(string minerID, params AlgorithmType[] ids)
        {
            MinerID = minerID;
            IDs = ids;
        }
        // Identity
        public IReadOnlyList<AlgorithmType> IDs { get; }
        public string MinerID { get; }

        public AlgorithmType FirstAlgorithmType {
            get {
                if (IDs.Count > 0) return IDs[0];
                return AlgorithmType.NONE;
            }
        }

        // variable settings
        public IList<double> Speeds { get; set; } // Make setter private???
        public bool Enabled { get; set; } = true;
        public string ExtraLaunchParameters { get; set; } = "";
        // TODO power usage???
    }
}
