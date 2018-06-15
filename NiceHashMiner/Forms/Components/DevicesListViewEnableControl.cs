using NiceHashMiner.Configs;
using NiceHashMiner.Devices;
using NiceHashMiner.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace NiceHashMiner.Forms.Components
{
    public partial class DevicesListViewEnableControl : UserControl
    {
        private const int ENABLED = 0;

        protected AlgorithmsListView _algorithmsListView;

        public string FirstColumnText
        {
            get => listViewDevices.Columns[ENABLED].Text;
            set
            {
                if (value != null) listViewDevices.Columns[ENABLED].Text = value;
            }
        }


        public bool SaveToGeneralConfig { get; set; }

        public DevicesListViewEnableControl()
        {
            InitializeComponent();

            SaveToGeneralConfig = false;
            // intialize ListView callbacks
            listViewDevices.ItemChecked += ListViewDevicesItemChecked;
            //listViewDevices.CheckBoxes = false;
        }

        public void SetAlgorithmsListView(AlgorithmsListView algorithmsListView)
        {
            _algorithmsListView = algorithmsListView;
        }

        public virtual void SetComputeDevices(List<ComputeDevice> computeDevices)
        {
            // to not run callbacks when setting new
            var tmpSaveToGeneralConfig = SaveToGeneralConfig;
            SaveToGeneralConfig = false;
            listViewDevices.BeginUpdate();
            listViewDevices.Items.Clear();
            // set devices
            foreach (var computeDevice in computeDevices)
            {
                var lvi = new ListViewItem
                {
                    Checked = computeDevice.Enabled,
                    Text = computeDevice.GetFullName(),
                    Tag = computeDevice
                };
                //lvi.SubItems.Add(computeDevice.Name);
                listViewDevices.Items.Add(lvi);
                SetLvi(lvi, computeDevice.Index);
            }
            listViewDevices.EndUpdate();
            listViewDevices.Invalidate(true);
            // reset properties
            SaveToGeneralConfig = tmpSaveToGeneralConfig;
        }

        protected virtual void SetLvi(ListViewItem lvi, int index)
        {

        }

        public void ResetComputeDevices(List<ComputeDevice> computeDevices)
        {
            SetComputeDevices(computeDevices);
        }

        public virtual void InitLocale()
        {
            devicesHeader.Text = International.GetText("ListView_Device");
        }

        protected virtual void ListViewDevicesItemChecked(object sender, ItemCheckedEventArgs e)
        {
            if (e.Item.Tag is ComputeDevice cDevice)
            {
                cDevice.Enabled = e.Item.Checked;

                if (SaveToGeneralConfig)
                {
                    ConfigManager.GeneralConfigFileCommit();
                }
                _algorithmsListView?.RepaintStatus(cDevice.Enabled, cDevice.Uuid);
            }
        }

        public void SetDeviceSelectionChangedCallback(ListViewItemSelectionChangedEventHandler callback)
        {
            listViewDevices.ItemSelectionChanged += callback;
        }

        protected virtual void DevicesListViewEnableControl_Resize(object sender, EventArgs e)
        {
            // only one 
            foreach (ColumnHeader ch in listViewDevices.Columns)
            {
                ch.Width = Width - 10;
            }
        }
    }
}
