using NHMCore.Configs.Data;
using NHMCore.Utils;
using System.Collections.Generic;
using System.ComponentModel;

namespace NHM.Wpf.ViewModels.Settings
{
    /// <summary>
    /// Base ViewModel for all settings pages.
    /// </summary>
    public class SettingsBaseVM : BaseVM
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

        // Not all page VMs will have children, but all need to have this property so it can be bound to
        // from the TreeView
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

        protected override void Dispose(bool disposing)
        {
            Config.PropertyChanged -= Config_PropertyChanged;
            base.Dispose(disposing);
        }
    }
}
