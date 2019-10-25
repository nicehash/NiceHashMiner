using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace NHM.Wpf.Views.Devices
{
    /// <summary>
    /// Interaction logic for ComputeDeviceItem.xaml
    /// </summary>
    public partial class ComputeDeviceItem : UserControl
    {
        public ComputeDeviceItem()
        {
            InitializeComponent();
            DataContextChanged += ComputeDeviceItem_DataContextChanged;
            DataContext = new ComputeDeviceItemData { };
            //DataContext = this;
        }

        private void ComputeDeviceItem_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DataContext is ComputeDeviceItemData == false)
            {
                throw new Exception("ComputeDeviceItem DataContext must implement ComputeDeviceItemData");
            }
        }

        //public interface IComputeDeviceItemData
        //{
        //    bool Enabled { get; set; }
        //    bool Name { get; set; }
        //}

        public class ComputeDeviceItemData
        {

            public virtual bool Enabled { get; set; } = false;
            //private bool _enabled = false;
            //public virtual bool Enabled
            //{
            //    get => _enabled;
            //    set
            //    {
            //        if (value == _enabled)
            //        {
            //            Debug.WriteLine("Same shit return");
            //            return;
            //        }
            //        _enabled = value;
            //        Debug.WriteLine($"Value changed to {_enabled}");
            //    }
            //}
            public virtual string DeviceName { get; set; } = "Device name";
        }

        //public static readonly DependencyProperty TitleTextProperty =
        //DependencyProperty.Register(
        //    nameof(TitleText),
        //    typeof(string),
        //    typeof(ComputeDeviceItemColumn),
        //    new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.AffectsRender));

        //public string TitleText
        //{
        //    get { return (string)GetValue(TitleTextProperty); }
        //    set { SetValue(TitleTextProperty, value); }
        //}

    }
}
