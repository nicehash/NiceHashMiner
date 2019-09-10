using NHMCore;

namespace NHM.Wpf.ViewModels.Plugins
{
    public class PluginVM : BaseVM
    {
        private readonly PluginListVM _listVM;

        private IPluginPageVM _currentPage;
        public IPluginPageVM CurrentPage
        {
            get => _currentPage;
            set
            {
                _currentPage = value;
                OnPropertyChanged();
            }
        }

        public PluginVM()
            : base(Translations.Tr("Plugins"))
        {
            _listVM = new PluginListVM();

            _listVM.PopulatePlugins();

            CurrentPage = _listVM;
        }

        public void SetDetails(PluginEntryVM vm)
        {
            CurrentPage = new PluginDetailVM(vm);
        }

        public void SetToList()
        {
            CurrentPage = _listVM;
        }
    }
}
