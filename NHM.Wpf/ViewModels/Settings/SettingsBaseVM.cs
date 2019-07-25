using System;
using System.Collections.Generic;
using System.ComponentModel;
using NiceHashMiner.Configs.Data;
using NiceHashMiner.Utils;

namespace NHM.Wpf.ViewModels.Settings
{
    public class SettingsBaseVM : BaseVM, IDisposable
    {
        public bool RestartRequired { get; protected set; }

        protected bool IsElevated => Helpers.IsElevated;

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
            Config.PropertyChanged += Config_PropertyChanged;
            Children = new List<SettingsBaseVM>(children);
        }

        protected virtual void Config_PropertyChanged(object sender, PropertyChangedEventArgs e)
        { }

        protected virtual void Dispose(bool disposing)
        {
            Config.PropertyChanged -= Config_PropertyChanged;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
