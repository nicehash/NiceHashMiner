using NHMCore;
using NHMCore.Configs;
using System.Collections.Generic;
using System.Linq;

namespace NHM.Wpf.ViewModels.Settings
{
    public class SettingsVM : BaseVM
    {
        private SettingsBaseVM _selectedPageVM;
        public SettingsBaseVM SelectedPageVM
        {
            get => _selectedPageVM;
            set
            {
                _selectedPageVM = value;
                OnPropertyChanged();
            }
        }

        public IReadOnlyList<SettingsBaseVM> PageVMs { get; }

        public bool DefaultsSet { get; private set; }
        public bool RestartRequired => DefaultsSet || ConfigManager.IsRestartNeeded();

        public SettingsVM()
            : base(Translations.Tr("Settings"))
        {
            ConfigManager.CreateBackup();

            var settingsObj = ConfigManager.GeneralConfig;

            PageVMs = new List<SettingsBaseVM>
            {
                new GeneralSettingsVM(settingsObj),
                new ProfitabilitySettingsVM(settingsObj),
                new MiningGeneralVM(settingsObj),
                SettingsContainerVM.AdvancedContainer(settingsObj)
            };

            SelectedPageVM = PageVMs[0];
        }

        // Recursively enumerate all VMs and their children
        private IEnumerable<SettingsBaseVM> AllPageVMs()
        {
            return PageVMs.SelectMany(AllChildVMs);
        }

        // Recursively enumerate all children of a VM
        private static IEnumerable<SettingsBaseVM> AllChildVMs(SettingsBaseVM vm)
        {
            var en = Enumerable.Empty<SettingsBaseVM>();
            en = en.Append(vm);
            return vm.Children.Aggregate(en, (current, child) => current.Concat(AllChildVMs(child)));
        }

        public void SetDefaults()
        {
            ConfigManager.GeneralConfig.SetDefaults();
            DefaultsSet = true;
        }

        public void Save()
        {
            ConfigManager.GeneralConfigFileCommit();
        }

        public void Revert()
        {
            // TODO
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            foreach (var vm in AllPageVMs())
            {
                vm?.Dispose();
            }
        }
    }
}
