using NHMCore.Configs;
using NHMCore.Stats;

namespace NHMCore
{
    public class RatesAndStatsStates //: INotifyPropertyChanged
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
            return (currency + "/") + Translations.Tr(timeUnit) + "     " + Translations.Tr("Balance") + ":";
        }

        public string LabelBalanceText => getLabelBalanceText(); //{ get; set; }


    }
}
