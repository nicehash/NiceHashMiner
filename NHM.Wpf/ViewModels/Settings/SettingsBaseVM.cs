using System.Collections.Generic;

namespace NHM.Wpf.ViewModels.Settings
{
    public class SettingsBaseVM : BaseVM
    {
        public string Name { get; }

        public IReadOnlyList<SettingsBaseVM> Children { get; }

        protected dynamic SettingsInstance;

        protected SettingsBaseVM(object settingsObj, string name)
        {
            Name = name;
            SettingsInstance = settingsObj;
        }
    }
}
