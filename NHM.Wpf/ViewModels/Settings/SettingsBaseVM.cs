using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace NHM.Wpf.ViewModels.Settings
{
    public class SettingsBaseVM : BaseVM
    {
        public string Name { get; }

        protected readonly ObservableCollection<SettingsBaseVM> _children;
        public IReadOnlyList<SettingsBaseVM> Children => _children;

        protected dynamic SettingsInstance;

        protected SettingsBaseVM(object settingsObj, string name)
        {
            Name = name;
            SettingsInstance = settingsObj;
            _children = new ObservableCollection<SettingsBaseVM>();
        }
    }
}
