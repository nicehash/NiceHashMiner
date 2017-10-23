using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NiceHashMiner.Configs;
using NiceHashMiner.Configs.Data;

namespace NiceHashMiner.Forms.Components
{
    public partial class ServiceLocationListView : UserControl
    {
        public bool SaveToGeneralConfig { get; set; }

        public ServiceLocationListView()
        {
            InitializeComponent();
            SaveToGeneralConfig = false;
        }

        public void SetServiceLocations()
        {
            listView_ServiceLocations.BeginUpdate();
            listView_ServiceLocations.Items.Clear();
            foreach (ServiceLocationConfig loc in ConfigManager.GeneralConfig.ServiceLocations)
            {
                ListViewItem lvi = new ListViewItem();
                lvi.Text = International.GetText("LocationName_" + loc.ServiceLocation);
                lvi.Tag = loc.ServiceLocation;
                lvi.Checked = loc.Enabled;
                listView_ServiceLocations.Items.Add(lvi);
            }
            // Add new service locations if exist
            if (ConfigManager.GeneralConfig.ServiceLocations.Count < Globals.MiningLocation.Length)
            {
                foreach (string ServiceLocation in Globals.MiningLocation)
                {
                    bool OldServiceLocation = false;
                    foreach (ServiceLocationConfig loc in ConfigManager.GeneralConfig.ServiceLocations)
                    {
                        if (loc.ServiceLocation == ServiceLocation)
                        {
                            OldServiceLocation = true;
                        }
                    }
                    if (!OldServiceLocation)
                    {
                        ListViewItem lvi = new ListViewItem();
                        lvi.Text = International.GetText("LocationName_" + ServiceLocation);
                        lvi.Tag = ServiceLocation;
                        lvi.Checked = true;
                        listView_ServiceLocations.Items.Add(lvi);
                        SaveToGeneralConfig = true;
                    }
                }
            }
            // TODO: remove locations which do not exist anymore
            // First location is the default one, mark bold
            listView_ServiceLocations.Items[0].Font = new System.Drawing.Font(listView_ServiceLocations.Font, System.Drawing.FontStyle.Bold);
            listView_ServiceLocations.Items[0].Selected = true;
            listView_ServiceLocations.Items[0].BackColor = SystemColors.Highlight;
            listView_ServiceLocations.Items[0].ForeColor = SystemColors.HighlightText;
            listView_ServiceLocations.Columns[0].Width = listView_ServiceLocations.Width - 4;
            listView_ServiceLocations.EndUpdate();
        }

        public void SaveServiceLocations()
        {
            foreach (ListViewItem lvi in listView_ServiceLocations.Items)
            {
                if (ConfigManager.GeneralConfig.ServiceLocations.Count <= lvi.Index)
                {
                    ServiceLocationConfig serviceLocationConfig = new ServiceLocationConfig();
                    serviceLocationConfig.ServiceLocation = lvi.Tag.ToString();
                    serviceLocationConfig.Enabled = lvi.Checked;
                    ConfigManager.GeneralConfig.ServiceLocations.Add(serviceLocationConfig);
                }
                else
                {
                    ConfigManager.GeneralConfig.ServiceLocations[lvi.Index].ServiceLocation = lvi.Tag.ToString();
                    ConfigManager.GeneralConfig.ServiceLocations[lvi.Index].Enabled = lvi.Checked;
                }
            }
        }

        private void listView_ServiceLocations_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            // Check if default location is still enabled
            if ((listView_ServiceLocations.Parent.Visible == true) && (e.NewValue == CheckState.Unchecked) && (e.Index == 0))
            {
                MessageBox.Show(International.GetText("ServiceLocationsListView_DefaultCannotBeDisabled"),
                                        International.GetText("Warning_with_Exclamation"),
                                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                e.NewValue = CheckState.Checked;
            }
            else
            {
                SaveToGeneralConfig = true;
            }
        }

        private void buttonUp_Click(object sender, EventArgs e)
        {
            if (listView_ServiceLocations.SelectedItems[0].Index != 0)
            {
                ListViewItem lvi = listView_ServiceLocations.SelectedItems[0];
                int index = lvi.Index;
                listView_ServiceLocations.Items.Remove(lvi);
                listView_ServiceLocations.Items.Insert(index - 1, lvi);
                // New default
                if (index == 1)
                {
                    listView_ServiceLocations.Items[0].Font = new System.Drawing.Font(listView_ServiceLocations.Font, System.Drawing.FontStyle.Bold);
                    listView_ServiceLocations.Items[1].Font = listView_ServiceLocations.Font;
                }
                SaveToGeneralConfig = true;
            }
        }

        private void buttonDown_Click(object sender, EventArgs e)
        {
            if (listView_ServiceLocations.SelectedItems[0].Index != listView_ServiceLocations.Items.Count - 1)
            {
                ListViewItem lvi = listView_ServiceLocations.SelectedItems[0];
                int index = lvi.Index;
                listView_ServiceLocations.Items.Remove(lvi);
                listView_ServiceLocations.Items.Insert(index + 1, lvi);
                // New default
                if (index == 0)
                {
                    listView_ServiceLocations.Items[0].Font = new System.Drawing.Font(listView_ServiceLocations.Font, System.Drawing.FontStyle.Bold);
                    listView_ServiceLocations.Items[1].Font = listView_ServiceLocations.Font;
                }
                SaveToGeneralConfig = true;
            }
        }

        private void listView_ServiceLocations_Enter(object sender, EventArgs e)
        {
            if (listView_ServiceLocations.SelectedItems.Count > 0)
            {
                listView_ServiceLocations.SelectedItems[0].BackColor = SystemColors.Window;
                listView_ServiceLocations.SelectedItems[0].ForeColor = SystemColors.WindowText;
            }
        }

        private void listView_ServiceLocations_Leave(object sender, EventArgs e)
        {
            if (listView_ServiceLocations.SelectedItems.Count > 0)
            {
                listView_ServiceLocations.SelectedItems[0].BackColor = SystemColors.Highlight;
                listView_ServiceLocations.SelectedItems[0].ForeColor = SystemColors.HighlightText;
            }
        }
    }
}
