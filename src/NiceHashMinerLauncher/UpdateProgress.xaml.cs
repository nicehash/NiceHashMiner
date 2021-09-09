using System;
using System.Windows;

namespace NiceHashMiner
{
    /// <summary>
    /// Interaction logic for UpdateProgress.xaml
    /// </summary>
    public partial class UpdateProgress : Window
    {
        public Progress<int> Progress { get; private set; }

        public UpdateProgress()
        {
            InitializeComponent();
            Progress = new Progress<int>((p) =>
            {
                progress.Value = p;
            });
        }
    }
}
