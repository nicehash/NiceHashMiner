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

namespace NHM.Wpf.Views.Devices
{
    /// <summary>
    /// Interaction logic for ComputeDeviceItemColumn.xaml
    /// </summary>
    public partial class ComputeDeviceItemColumn : UserControl
    {
        public ComputeDeviceItemColumn()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        //public interface IComputeDeviceItemColumnData
        //{
        //    string TitleText { get; set; }
        //    string ValueText { get; set; }
        //}

        //public class ComputeDeviceItemColumnMockData
        //{
        //    public string TitleText { get; set; } = "Title";
        //    public string ValueText { get; set; } = "Value";
        //}

        //public string TitleText
        //{
        //    get => TitleTextBlock.Text;
        //    set
        //    {
        //        TitleTextBlock.Text = value;
        //    }
        //}

        public static readonly DependencyProperty TitleTextProperty =
        DependencyProperty.Register(
            nameof(TitleText),
            typeof(string),
            typeof(ComputeDeviceItemColumn),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.AffectsRender));

        public string TitleText
        {
            get { return (string)GetValue(TitleTextProperty); }
            set { SetValue(TitleTextProperty, value); }
        }

        public static readonly DependencyProperty ValueTextProperty =
        DependencyProperty.Register(
            nameof(ValueText),
            typeof(string),
            typeof(ComputeDeviceItemColumn),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.AffectsRender));

        public string ValueText
        {
            get { return (string)GetValue(ValueTextProperty); }
            set { SetValue(ValueTextProperty, value); }
        }

        //public string TitleText
        //{
        //    get
        //    {
        //        if (DataContext is IComputeDeviceItemColumnData d)
        //        {
        //            return d.TitleText;
        //        }
        //        // throw 
        //        return (string)GetValue(TitleTextProperty);
        //    }
        //    set { SetValue(TitleTextProperty, value); }
        //}

        //public string ValueText { get; set; }

        //private bool _flipStyles = false;
        //public bool FlipStyles
        //{
        //    get => _flipStyles;
        //    set
        //    {
        //        if (value == _flipStyles) return;
        //        var tmpStyle = TitleTextBlock.Style;
        //        TitleTextBlock.Style = ValueTextBlock.Style;
        //        ValueTextBlock.Style = tmpStyle;
        //    }
        //}

        
    }
}
