// TODO decide what to use here
//#define USE_MODAL 


using NHMCore.Configs;
using NHMCore.Mining;
using NiceHashMiner.Views.Common;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace NiceHashMiner.Views.Benchmark.ComputeDeviceItem
{
    /// <summary>
    /// Interaction logic for AlgorithmItem.xaml
    /// </summary>
    public partial class AlgorithmItem : UserControl
    {
        AlgorithmContainer _algorithmContainer;
#if USE_MODAL
        readonly AlgorithmSettings _algorithmSettings;
#endif
        public AlgorithmItem()
        {
            InitializeComponent();
#if USE_MODAL
            _algorithmSettings = new AlgorithmSettings();
            _algorithmSettings.MaxWidth = 392;
            _algorithmSettings.MaxHeight = 798;
            _algorithmSettings.CloseClick += (s, e) => CustomDialogManager.HideCurrentModal();
#endif

            DataContextChanged += AlgorithmItem_DataContextChanged;
            WindowUtils.Translate(this);
        }

        private void AlgorithmItem_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is AlgorithmContainer algorithmContainer)
            {
                _algorithmContainer = algorithmContainer;
                AlgorithmSettingsContextMenu.DataContext = e.NewValue;
#if USE_MODAL
                _algorithmSettings.DataContext = e.NewValue;
#endif
                return;
            }
            throw new Exception("unsupported datacontext type");
        }

        private readonly HashSet<Button> _toggleButtonsGuard = new HashSet<Button>();
        private void AlgorithmSettings_Button_Click(object sender, RoutedEventArgs e)
        {
#if USE_MODAL
            CustomDialogManager.ShowDialog(_algorithmSettings);
#else
            if (sender is Button tButton && !_toggleButtonsGuard.Contains(tButton))
            {
                _toggleButtonsGuard.Add(tButton);
                AlgorithmSettingsContextMenu.IsOpen = true;
                RoutedEventHandler closedHandler = null;
                closedHandler += (s, e2) =>
                {
                    _toggleButtonsGuard.Remove(tButton);
                    //tButton.IsChecked = false;
                    AlgorithmSettingsContextMenu.Closed -= closedHandler;
                };
                AlgorithmSettingsContextMenu.Closed += closedHandler;
            }
#endif
        }

        private void CloseAlgorithmSettings_Button_Click(object sender, RoutedEventArgs e)
        {
            AlgorithmSettingsContextMenu.IsOpen = false;
        }

        private void AlgorithmSettingsContextMenu_Closed(object sender, RoutedEventArgs e)
        {
            ConfigManager.CommitBenchmarksForDevice(_algorithmContainer.ComputeDevice);
        }

        private void EnableChanged(object sender, RoutedEventArgs e)
        {
            ConfigManager.CommitBenchmarksForDevice(_algorithmContainer.ComputeDevice);
        }
    }
}
