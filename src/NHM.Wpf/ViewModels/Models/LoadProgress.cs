using System;

namespace NHM.Wpf.ViewModels.Models
{
    public class LoadProgress : BaseVM, IProgress<(string mess, double perc)>
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

        public void Report((string mess, double perc) value)
        {
            Progress = value.perc;
            Status = value.mess;
        }
    }
}
