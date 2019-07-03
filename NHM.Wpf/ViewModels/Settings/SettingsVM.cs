using System.Collections.Generic;

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

        public SettingsVM()
        {
            PageVMs = new List<SettingsBaseVM>
            {
                new GeneralSettingsVM(null)
            };

            SelectedPageVM = PageVMs[0];
        }
    }
}
