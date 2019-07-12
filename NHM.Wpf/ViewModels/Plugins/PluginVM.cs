using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using NHM.Wpf.ViewModels.Models.Placeholders;

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
        {
            _listVM = new PluginListVM();

            CurrentPage = _listVM;
        }

        public void SetDetails(PluginPackageInfoCR plugin)
        {
            // TODO
        }
    }
}
