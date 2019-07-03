using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHM.Wpf.ViewModels.Settings
{
    public class MiningSettingsVM : SettingsBaseVM
    {
        public MiningSettingsVM(object settingsObj)
            : base(settingsObj, "Mining")
        {
            _children.Add(new MiningGeneralVM(settingsObj));
        }
    }
}
