using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NiceHashMiner.Configs.Data;

namespace NHM.Wpf.ViewModels.Settings
{
    public class AdvancedMiningVM : SettingsBaseVM
    {
        public AdvancedMiningVM(GeneralConfig confObj) 
            : base(confObj, "Mining")
        { }
    }
}
