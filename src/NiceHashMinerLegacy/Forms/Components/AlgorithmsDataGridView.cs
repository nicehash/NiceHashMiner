using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using static NiceHashMiner.Translations;
using NiceHashMiner.Algorithms;

namespace NiceHashMiner.Forms.Components
{
    public partial class AlgorithmsDataGridView : UserControl
    {
        private enum Column : int
        {
            Enabled = 0,
            Algorithm,
            Miner,
            Speed,
            SMA,
            BTCPerDay,
            Status
        }

        public event EventHandler<(string deviceUuid, string algorithmID, bool enabled)> SetAlgorithmEnabledState;

        private static string algorithmNameString(Algorithm a) {
            throw new NotImplementedException();
        }

        private static string speedStr(Algorithm a) {
            if (a.BenchmarkNeeded)
            {
                return Tr("none");
            }
            throw new NotImplementedException();
            return "TODO";
        }

        private static string smaPayingStr(Algorithm a)
        {
            throw new NotImplementedException();
        }

        private static string btcPayingStr(Algorithm a)
        {
            throw new NotImplementedException();
        }

        public static object[] GetRowData(Algorithm a)
        {
            var name = algorithmNameString(a);
            var speed = speedStr(a);
            var sma = smaPayingStr(a);
            var btcPerDay = btcPayingStr(a);
            object[] rowData = { a.Enabled, name, a.MinerBaseTypeName, speed, sma, btcPerDay, "todo status" };
            return rowData;
        }

        public AlgorithmsDataGridView()
        {
            InitializeComponent();
            dataGridView.CellContentClick += DataGridView_CellContentClick;
        }

        private void SetRowColumnItemValue(DataGridViewRow row, Column col, object value)
        {
            var cellItem = row.Cells[(int)col];
            cellItem.Value = value;
        }

        private void DataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            var senderGrid = (DataGridView)sender;
            Console.WriteLine($"RowIndex {e.RowIndex} ColumnIndex {e.ColumnIndex}");

            if (!(e.RowIndex >= 0)) return;

            //var columnItem = senderGrid.Columns[e.ColumnIndex];
            var row = senderGrid.Rows[e.RowIndex];
            var deviceUUID = (string)row.Tag;
            Console.WriteLine($"Row TAG {row.Tag}");
            var cellItem = row.Cells[e.ColumnIndex];
            switch (cellItem)
            {
                case DataGridViewCheckBoxCell checkbox:
                    var enabled = checkbox.Value != null && (bool)checkbox.Value;
                    checkbox.Value = !enabled;
                    SetAlgorithmEnabledState?.Invoke(null, ("", deviceUUID, !enabled));
                    break;

            }
        }
    }
}
