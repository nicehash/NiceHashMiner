using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NiceHashMiner.Forms.Components
{
    public class AlgorithmsListViewComparer : System.Collections.IComparer
    {
        private const int ENABLED = 0;
        private const int ALGORITHM = 1;
        private const int SPEED = 2;
        private const int SECSPEED = 3;
        private const int RATIO = 4;
        private const int RATE = 5;

        private int ColumnNumber;
        private SortOrder SortOrder;

        public AlgorithmsListViewComparer(int pColumnNumber, SortOrder pSortOrder)
        {
            ColumnNumber = pColumnNumber;
            SortOrder = pSortOrder;
        }

        public int Compare(object algorithm1, object algorithm2)
        {
            ListViewItem item1 = algorithm1 as ListViewItem;
            ListViewItem item2 = algorithm2 as ListViewItem;
            int result = 0;
            double double1, double2;

            switch (ColumnNumber)
            {
                case ENABLED:
                    result = item1.Checked.CompareTo(item2.Checked);
                    break;
                case ALGORITHM:
                    String AlgorithmName1 = String.Format("{0} ({1})", ((Algorithm)(item1.Tag)).AlgorithmName, ((Algorithm)(item1.Tag)).MinerBaseTypeName);
                    String AlgorithmName2 = String.Format("{0} ({1})", ((Algorithm)(item2.Tag)).AlgorithmName, ((Algorithm)(item2.Tag)).MinerBaseTypeName);
                    // Workaround for Dagger dual mining
                    if (AlgorithmName1.StartsWith("Dagger") && !(AlgorithmName1.StartsWith("DaggerHashimoto")))
                    {
                        AlgorithmName1 = AlgorithmName1.Replace("Dagger", "DaggerHashimoto");
                    }
                    if (AlgorithmName2.StartsWith("Dagger") && !(AlgorithmName2.StartsWith("DaggerHashimoto")))
                    {
                        AlgorithmName2 = AlgorithmName2.Replace("Dagger", "DaggerHashimoto");
                    }
                    result = AlgorithmName1.CompareTo(AlgorithmName2);
                    break;
                case SPEED:
                    double.TryParse(item1.SubItems[SPEED].Tag.ToString(), out double1);
                    double.TryParse(item2.SubItems[SPEED].Tag.ToString(), out double2);
                    result = double1.CompareTo(double2);
                    break;
                case SECSPEED:
                    double.TryParse(item1.SubItems[SECSPEED].Tag.ToString(), out double1);
                    double.TryParse(item2.SubItems[SECSPEED].Tag.ToString(), out double2);
                    result = double1.CompareTo(double2);
                    break;
                case RATIO:
                    double.TryParse(item1.SubItems[RATIO].Text.ToString(), out double1);
                    double.TryParse(item2.SubItems[RATIO].Text.ToString(), out double2);
                    result = double1.CompareTo(double2);
                    break;
                case RATE:
                    double.TryParse(item1.SubItems[RATE].Text.ToString(), out double1);
                    double.TryParse(item2.SubItems[RATE].Text.ToString(), out double2);
                    result = double1.CompareTo(double2);
                    break;
            }

            if (SortOrder == SortOrder.Ascending)
            {
                return result;
            }
            else
            {
                return -result;
            }

        }
    }
}