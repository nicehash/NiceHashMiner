using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHMCore.Configs.Data;

namespace NHM.Wpf.ViewModels.Settings
{
    public class AdvancedSwitchingVM : SettingsBaseVM
    {
        public AdvancedSwitchingVM(GeneralConfig confObj) 
            : base(confObj, "Switching")
        { }
    }
}
