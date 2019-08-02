
namespace NHM.DeviceMonitoring
{
    internal class RangeCalculator
    {
        public int Max { get; set; } = 100;
        public int Min { get; set; } = 0;
        public int Step { get; set; } = 1;

        public double PercentageStep => (double)Step / (Max - Min);

        // this is so we don't divide by 0
        public bool IsValid => Step > 0 && Max > Min && Max != Min;


        // calculates the nearest valid step value
        public int CalcStepValue(double value)
        {
            var vPartMod = value % Step;
            var vPartStepPerc = vPartMod / Step;
            // up to 50% round down otherwise round up
            var vPartStep = vPartStepPerc > 0.5 ? (Step - vPartMod) : -vPartMod;
            var vStep = value + vPartStep;
            return (int)vStep;
        }

        public int CalcValue(double perc)
        {
            var value = (perc * (Max - Min)) + Min;
            var stepValue = CalcStepValue(value);
            return stepValue;
        }

        public double CalcPercentage(int value)
        {
            var stepValue = CalcStepValue(value);
            var perc = (stepValue - Min) / (double)(Max - Min);
            return perc;
        }
    }
}
