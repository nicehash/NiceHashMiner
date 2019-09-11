using NHM.Common;
using NHM.Common.Enums;

namespace NHMCore.Configs
{
    public class IdleMiningSettings : NotifyChangedBase
    {
        public static IdleMiningSettings Instance { get; } = new IdleMiningSettings();

        private IdleMiningSettings()
        { }

        private bool _startMiningWhenIdle;
        public bool StartMiningWhenIdle
        {
            get => _startMiningWhenIdle;
            set
            {
                _startMiningWhenIdle = value;
                OnPropertyChanged();
            }
        }

        private IdleCheckType _idleCheckType = IdleCheckType.SessionLock;
        public IdleCheckType IdleCheckType
        {
            get => _idleCheckType;
            set
            {
                _idleCheckType = value;
                OnPropertyChanged();
            }
        }
        public int IdleCheckTypeIndex
        {
            get => (int)IdleCheckType;
            set => IdleCheckType = (IdleCheckType)value;
        }

        public bool IsIdleCheckTypeInputTimeout
        {
            get => IdleCheckType.InputTimeout == IdleCheckType && StartMiningWhenIdle;
        }
    }
}
