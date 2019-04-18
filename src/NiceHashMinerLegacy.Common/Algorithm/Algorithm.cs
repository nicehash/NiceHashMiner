using NiceHashMinerLegacy.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NiceHashMinerLegacy.Common.Algorithm
{
    public class Algorithm
    {
        public Algorithm(string minerID, params AlgorithmType[] ids)
        {
            MinerID = minerID;
            IDs = ids;
            Speeds = ids.Select(id => 0d).ToList();
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

        public AlgorithmType SecondAlgorithmType
        {
            get
            {
                if (IDs.Count > 1) return IDs[1];
                return AlgorithmType.NONE;
            }
        }

        // variable settings
        public IList<double> Speeds { get; private set; }
        public bool Enabled { get; set; } = true;
        public string ExtraLaunchParameters { get; set; } = "";
        // TODO power usage???
    }
}
