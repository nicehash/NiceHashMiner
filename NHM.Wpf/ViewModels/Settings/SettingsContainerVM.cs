using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHM.Wpf.ViewModels.Settings
{
    public class SettingsContainerVM : SettingsBaseVM
    {
        private SettingsContainerVM(object settingsObj, string name, params SettingsBaseVM[] children) 
            : base(settingsObj, name, children)
        { }

        public static SettingsContainerVM MiningContainer(object settingsObj)
        {
            return new SettingsContainerVM(settingsObj,
                "Mining",
                new MiningGeneralVM(settingsObj));
        }

        public static SettingsContainerVM AdvancedContainer(object settingsObj)
        {
            return new SettingsContainerVM(settingsObj,
                "Advanced",
                new AdvancedGeneral(settingsObj),
                new AdvancedSwitchingVM(settingsObj));
        }
    }
}
