using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHMCore.Configs.Data;

namespace NHM.Wpf.ViewModels.Settings
{
    public class AdvancedDevicesVM : SettingsBaseVM
    {
        public AdvancedDevicesVM(GeneralConfig confObj)
            : base(confObj, "Devices")
        { }
    }
}
