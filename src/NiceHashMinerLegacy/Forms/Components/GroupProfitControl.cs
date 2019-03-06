using System.Windows.Forms;

namespace NiceHashMiner.Forms.Components
{
    public partial class GroupProfitControl : UserControl
    {
        public GroupProfitControl()
        {
            InitializeComponent();
            FormHelpers.TranslateFormControls(this);
        }


        public void UpdateProfitStats(string groupName, string deviceStringInfo,
            string speedString, string btcRateString, string currencyRateString)
        {
            groupBoxMinerGroup.Text = string.Format(Translations.Tr("Mining Devices {0}:"), deviceStringInfo);
            labelSpeedValue.Text = speedString;
            labelBTCRateValue.Text = btcRateString;
            labelCurentcyPerDayVaue.Text = currencyRateString;
        }
    }
}
