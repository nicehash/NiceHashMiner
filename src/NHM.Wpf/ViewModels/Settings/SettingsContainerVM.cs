using NHMCore.Configs.Data;

namespace NHM.Wpf.ViewModels.Settings
{
    /// <summary>
    /// Simple container implementation of <see cref="SettingsBaseVM"/> that can act as a folder in the TreeView.
    /// </summary>
    /// <remarks>The first child VM passed will be used when the container itself is clicked.</remarks>
    public class SettingsContainerVM : SettingsBaseVM
    {
        private SettingsContainerVM(GeneralConfig confObj, string name, params SettingsBaseVM[] children) 
            : base(confObj, name, children)
        { }

        public static SettingsContainerVM AdvancedContainer(GeneralConfig settingsObj)
        {
            return new SettingsContainerVM(settingsObj,
                "Advanced",
                new AdvancedGeneralVM(settingsObj),
                new AdvancedSwitchingVM(settingsObj),
                new AdvancedMiningVM(settingsObj),
                new AdvancedDevicesVM(settingsObj));
        }
    }
}
