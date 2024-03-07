﻿using NHMCore.Configs.Managers;
using NHMCore.Utils;
using NiceHashMiner.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NiceHashMiner.Views.ParameterOverview
{
    /// <summary>
    /// Interaction logic for ParameterOverview.xaml
    /// </summary>
    public partial class ParameterOverview : UserControl
    {
        public ParameterOverview()
        {
            Unloaded += UpdateELPConfig;
            InitializeComponent();
        }

        public void UpdateELPConfig(object sender, RoutedEventArgs e)
        {
            ELPManager.Instance.UpdateMinerELPConfig();
        }

        private void AddressHyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Helpers.VisitUrlLink(e.Uri.AbsoluteUri);
        }
    }
}
