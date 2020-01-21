using NHM.Common;
using NHMCore;
using NiceHashMiner.ViewModels.Models;
using System;

namespace NiceHashMiner.ViewModels
{
    /// <summary>
    /// ViewModel for startup loading bar popup.
    /// </summary>
    public class StartupLoadingVM : BaseVM, IStartupLoader
    {
        public LoadProgress PrimaryProgress { get; } = new LoadProgress();
        public LoadProgress SecondaryProgress { get; } = new LoadProgress();

        IProgress<(string, int)> IStartupLoader.PrimaryProgress => PrimaryProgress;
        IProgress<(string, int)> IStartupLoader.SecondaryProgress => SecondaryProgress;

        private static readonly string PlaceholderTitle = Translations.Tr("Loading, please wait...");

        private string _primaryTitle = PlaceholderTitle;
        public string PrimaryTitle
        {
            get => _primaryTitle;
            set
            {
                _primaryTitle = value;
                OnPropertyChanged();
            }
        }

        private string _secondaryTitle = PlaceholderTitle;
        public string SecondaryTitle
        {
            get => _secondaryTitle;
            set
            {
                _secondaryTitle = value;
                OnPropertyChanged();
            }
        }

        private bool _secondaryVisible;
        public bool SecondaryVisible
        {
            get => _secondaryVisible;
            set
            {
                _secondaryVisible = value;
                OnPropertyChanged();
            }
        }

        public StartupLoadingVM()
            : base(Translations.Tr("Loading"))
        { }
    }
}
