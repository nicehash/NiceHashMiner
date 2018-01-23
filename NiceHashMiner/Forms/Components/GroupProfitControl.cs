using System.Windows.Forms;

namespace NiceHashMiner.Forms.Components
{
    public partial class GroupProfitControl : UserControl
    {
        public GroupProfitControl()
        {
            InitializeComponent();

            labelSpeedIndicator.Text = International.GetText("ListView_Speed");
            labelBTCRateIndicator.Text = International.GetText("Rate");
        }


        public void UpdateProfitStats(string groupName, string deviceStringInfo,
            string speedString, string btcRateString, string currencyRateString)
        {
            groupBoxMinerGroup.Text = string.Format(International.GetText("Form_Main_MiningDevices"), deviceStringInfo);
            labelSpeedValue.Text = speedString;
            labelBTCRateValue.Text = btcRateString;
            labelCurentcyPerDayVaue.Text = currencyRateString;
        }

        public string GetStatus()
        {
            return
                string.Format("{0} {4}-Speed:{4}    {1} {4}-Rate:{4}    {2} {4}    {3}"
                , groupBoxMinerGroup.Text
                , labelSpeedValue.Text
                , labelBTCRateValue.Text
                , labelCurentcyPerDayVaue.Text
                , System.Environment.NewLine);
        }
    }
}
