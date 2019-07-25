using System.Collections.Generic;
using NiceHashMiner.Configs.Data;

namespace NHM.Wpf.ViewModels.Settings
{
    public class SettingsBaseVM : BaseVM
    {
        public bool RestartRequired { get; protected set; }

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

        public GeneralConfig Config { get; }

        protected SettingsBaseVM(GeneralConfig confObj, string name, params SettingsBaseVM[] children)
        {
            Name = name;
            Config = confObj;
            Children = new List<SettingsBaseVM>(children);
        }
    }
}
