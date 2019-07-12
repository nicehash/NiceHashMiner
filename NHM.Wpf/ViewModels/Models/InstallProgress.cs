namespace NHM.Wpf.ViewModels.Models
{
    public class InstallProgress : BaseVM
    {
        private bool _isInstalling;

        public bool IsInstalling
        {
            get => _isInstalling;
            set
            {
                _isInstalling = value;
                OnPropertyChanged();
            }
        }

        private double _installProgress;

        public double Progress
        {
            get => _installProgress;
            set
            {
                _installProgress = value;
                OnPropertyChanged();
            }
        }

        private string _installStatus;

        public string Status
        {
            get => _installStatus;
            set
            {
                _installStatus = value;
                OnPropertyChanged();
            }
        }
    }
}
