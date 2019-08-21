using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using NiceHashMiner;
using NiceHashMiner.Configs;

namespace NHM.Wpf.ViewModels.Settings
{
    public class SettingsVM : BaseVM, IDisposable
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

        private IEnumerable<SettingsBaseVM> AllPageVMs()
        {
            return PageVMs.SelectMany(AllChildVMs);
        }

        private static IEnumerable<SettingsBaseVM> AllChildVMs(SettingsBaseVM vm)
        {
            var en = Enumerable.Empty<SettingsBaseVM>();
            en = en.Append(vm);
            return vm.Children.Aggregate(en, (current, child) => current.Concat(AllChildVMs(child)));
        }

        public bool SetDefaults()
        {
            var result = MessageBox.Show(
                Translations.Tr(
                    "Are you sure you would like to set everything back to defaults? This will restart {0} automatically.",
                    NHMProductInfo.Name),
                Translations.Tr("Set default settings?"),
                MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return false;

            ConfigManager.GeneralConfig.SetDefaults();
            DefaultsSet = true;
            return true;
        }

        public void Save()
        {
            ConfigManager.GeneralConfigFileCommit();
        }

        public void Revert()
        {
            // TODO
        }

        public void Dispose()
        {
            foreach (var vm in AllPageVMs())
            {
                vm?.Dispose();
            }
        }
    }
}
