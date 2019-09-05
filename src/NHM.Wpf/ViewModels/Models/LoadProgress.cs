using System;
using NHM.Common;

namespace NHM.Wpf.ViewModels.Models
{
    /// <summary>
    /// A notify-changed friendly implementation of <see cref="IProgress{T}"/> with message and percentage.
    /// </summary>
    public class LoadProgress : NotifyChangedBase, IProgress<(string mess, int perc)>
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

        private int _installProgress;

        public int Progress
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

        public void Report((string mess, int perc) value)
        {
            Progress = value.perc;
            Status = value.mess;
        }
    }
}
