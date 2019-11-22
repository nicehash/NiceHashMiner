using NHMCore.Configs;
using NHMCore.Utils;
using System.Timers;

namespace NHMCore.Mining.IdleChecking
{
    internal class InputTimeoutChecker : IdleChecker
    {
        private readonly Timer _timer = new Timer(500);
        private bool _isIdling;

        public override void StartChecking()
        {
            _timer.Elapsed += TimerOnElapsed;
            _timer.Start();
        }

        private void TimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            var msIdle = Helpers.GetIdleTime();

            if (msIdle < IdleMiningSettings.Instance.MinIdleSeconds * 1000)
            {
                if (_isIdling)
                {
                    FireStatusEvent(false);
                    _isIdling = false;
                }
            }
            else
            {
                if (!_isIdling)
                {
                    FireStatusEvent(true);
                    _isIdling = true;
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (!disposing) return;

            _timer.Dispose();
        }
    }
}
