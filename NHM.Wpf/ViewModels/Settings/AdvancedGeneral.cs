using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NiceHashMiner.Configs.Data;

namespace NHM.Wpf.ViewModels.Settings
{
    public class AdvancedGeneral : SettingsBaseVM
    {
        public AdvancedGeneral(GeneralConfig settingsObj) 
            : base(settingsObj, "General")
        { }
    }
}
