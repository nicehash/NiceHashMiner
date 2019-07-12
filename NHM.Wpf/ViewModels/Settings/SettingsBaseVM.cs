using System.Collections.Generic;

namespace NHM.Wpf.ViewModels.Settings
{
    public class SettingsBaseVM : BaseVM
    {
        private string _name;
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        public IReadOnlyList<SettingsBaseVM> Children { get; }

        protected dynamic SettingsInstance;

        protected SettingsBaseVM(object settingsObj, string name, params SettingsBaseVM[] children)
        {
            Name = name;
            SettingsInstance = settingsObj;
            Children = new List<SettingsBaseVM>(children);
        }
    }
}
