using NiceHashMinerLegacy.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiceHashMiner.Configs
{
    public class IdleMiningSettings
    {
        public static IdleMiningSettings Instance { get; } = new IdleMiningSettings();

        private IdleMiningSettings()
        {}

        public bool StartMiningWhenIdle { get; set; } = false;

        public IdleCheckType IdleCheckType { get; set; } = IdleCheckType.SessionLock;
        public int IdleCheckTypeIndex
        {
            get => (int)IdleCheckType;
            set => IdleCheckType = (IdleCheckType)value;
        }

        public bool IsIdleCheckTypeInputTimeout
        {
            get => IdleCheckType.InputTimeout == IdleCheckType && StartMiningWhenIdle;
        }
    }
}
