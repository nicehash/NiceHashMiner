using System;
using System.Timers;

namespace NHMCore.Mining.Benchmarking
{
    internal class PowerHelper : IDisposable
    {
        private Timer _timer;

        private double _powerSum;
        private int _powerCount;
        private bool _started;

        private readonly ComputeDevice _device;

        private double _interval = 1000;

        public double Interval
        {
            get => _interval;
            set
            {
                if (value <= 0) return;

                _interval = value;
                if (_timer != null)
                    _timer.Interval = value;
            }
        }

        public double LoadThreshold = 30;

        public PowerHelper(ComputeDevice dev)
        {
            _device = dev;
        }

        public void Start()
        {
            if (_timer == null)
                _timer = new Timer(Interval);

            _timer.Elapsed += TimerOnElapsed;
            _timer.Start();
        }

        private void TimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            var power = _device.PowerUsage;
            if ((_device.Load > LoadThreshold && power > 0) || _started)
            {
                _powerSum += power;
                _powerCount++;
                _started = true;
            }
        }

        public double Stop()
        {
            _timer.Stop();

            return CalculatePower();
        }

        private double CalculatePower()
        {
            var power = _powerSum / Math.Max(1, _powerCount);

            _powerSum = 0;
            _powerCount = 0;
            _started = false;

            return power;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
