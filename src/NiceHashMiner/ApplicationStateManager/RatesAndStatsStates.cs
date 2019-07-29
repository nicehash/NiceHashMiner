using NiceHashMiner.Configs;
using NiceHashMiner.Stats;
using static NiceHashMiner.Translations;

namespace NiceHashMiner
{
    class RatesAndStatsStates //: INotifyPropertyChanged
    {
        public static RatesAndStatsStates Instance { get; } = new RatesAndStatsStates();

        private RatesAndStatsStates()
        {
            //LabelBalanceText = getLabelBalanceText();
        }

        //public event PropertyChangedEventHandler PropertyChanged;

        //private void NotifyPropertyChanged(String info)
        //{
        //    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        //}

        private string getLabelBalanceText()
        {
            var timeUnit = ConfigManager.GeneralConfig.TimeUnit.ToString();
            var currency = ExchangeRateApi.ActiveDisplayCurrency;
            return (currency + "/") + Tr(timeUnit) + "     " + Tr("Balance") + ":";
        }

        public string LabelBalanceText => getLabelBalanceText(); //{ get; set; }


    }
}
