using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHM.Common.Enums;
using NiceHashMiner.Configs.Data;

namespace NHM.Wpf.ViewModels.Settings
{
    public class MiningGeneralVM : SettingsBaseVM
    {
        public IEnumerable<IdleCheckType> IdleTypes { get; }

        public bool Enable3rdPartyCheck
        {
            get => Config.Use3rdPartyMiners == Use3rdPartyMiners.YES;
            set
            {
                Config.Use3rdPartyMiners = value ? Use3rdPartyMiners.YES : Use3rdPartyMiners.NO;
            }
        }

        public int SelectedIdleType
        {
            get => (int) Config.IdleCheckType;
            set { Config.IdleCheckType = (IdleCheckType) value; }
        }

        public MiningGeneralVM(GeneralConfig confObj)
            : base(confObj, "Mining")
        {
            var idleTypes = new List<IdleCheckType>();

            foreach (var checkType in Enum.GetValues(typeof(IdleCheckType)))
            {
                idleTypes.Add((IdleCheckType) checkType);
            }

            IdleTypes = idleTypes;
        }
    }
}
