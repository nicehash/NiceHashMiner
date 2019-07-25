using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NiceHashMiner.Configs.Data;

namespace NHM.Wpf.ViewModels.Settings
{
    public class SettingsContainerVM : SettingsBaseVM
    {
        private SettingsContainerVM(GeneralConfig settingsObj, string name, params SettingsBaseVM[] children) 
            : base(settingsObj, name, children)
        { }

        //public static SettingsContainerVM MiningContainer(object settingsObj)
        //{
        //    return new SettingsContainerVM(settingsObj,
        //        "Mining",
        //        new MiningGeneralVM(settingsObj));
        //}

        public static SettingsContainerVM AdvancedContainer(GeneralConfig settingsObj)
        {
            return new SettingsContainerVM(settingsObj,
                "Advanced",
                new AdvancedGeneral(settingsObj),
                new AdvancedSwitchingVM(settingsObj),
                new AdvancedMiningVM(settingsObj));
        }
    }
}
