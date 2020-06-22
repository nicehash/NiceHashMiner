namespace NHM.DeviceMonitoring
{
    internal class RangeCalculator
    {
        public double Max { get; set; } = 100;
        public double Min { get; set; } = 0;
        public double Step { get; set; } = 1;

        public double PercentageStep => (double)Step / (Max - Min);

        // this is so we don't divide by 0
        public bool IsValid => Step > 0 && Max > Min && Max != Min;


        // calculates the nearest valid step value
        public double CalcStepValue(double value)
        {
            var vPartMod = value % Step;
            var vPartStepPerc = vPartMod / Step;
            // up to 50% round down otherwise round up
            var vPartStep = vPartStepPerc > 0.5 ? (Step - vPartMod) : -vPartMod;
            var vStep = value + vPartStep;
            return (int)vStep;
        }

        public double CalcValue(double perc)
        {
            var value = (perc * (Max - Min)) + Min;
            var stepValue = CalcStepValue(value);
            return stepValue;
        }

        public double CalcPercentage(double value)
        {
            var stepValue = CalcStepValue(value);
            var perc = (stepValue - Min) / (double)(Max - Min);
            return perc;
        }


        // returns percentage in 0.0 to 1.0 range
        public static double CalculatePercentage(double value, double min, double max)
        {
            var perc = (value - min) / (max - min);
            return perc;
        }

        public static double CalculateValueAMD(double percentage, double min, double max)
        {
            var value = (percentage * (max - min)) + min;
            return value;
        }

        public static double CalculateValueNVIDIA(double percentage, double point)
        {
            var value = percentage * point;
            return value;
        }
    }
}
