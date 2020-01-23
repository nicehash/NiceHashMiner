using NHM.Common;
using NHM.Common.Enums;
using System.Collections.Generic;

namespace NHMCore.Configs
{
    public class IdleMiningSettings : NotifyChangedBase
    {
        public static IdleMiningSettings Instance { get; } = new IdleMiningSettings();

        private IdleMiningSettings()
        { }

        private int _minIdleSeconds { get; set; } = 60;
        public int MinIdleSeconds
        {
            get => _minIdleSeconds;
            set
            {
                _minIdleSeconds = value;
                OnPropertyChanged(nameof(MinIdleSeconds));
            }
        }

        private bool _idleWhenNoInternetAccess = true;
        public bool IdleWhenNoInternetAccess
        {
            get => _idleWhenNoInternetAccess;
            set
            {
                _idleWhenNoInternetAccess = value;
                OnPropertyChanged(nameof(IdleWhenNoInternetAccess));
            }
        }

        private bool _startMiningWhenIdle;
        public bool StartMiningWhenIdle
        {
            get => _startMiningWhenIdle;
            set
            {
                _startMiningWhenIdle = value;
                OnPropertyChanged(nameof(StartMiningWhenIdle));
            }
        }

        private IdleCheckType _idleCheckType = IdleCheckType.SessionLock;
        public IdleCheckType IdleCheckType
        {
            get => _idleCheckType;
            set
            {
                _idleCheckType = value;
                OnPropertyChanged(nameof(IdleCheckType));
            }
        }

        public int IdleCheckTypeIndex
        {
            get => (int)IdleCheckType;
            set
            {
                IdleCheckType = (IdleCheckType)value;
                OnPropertyChanged(nameof(IdleCheckTypeIndex));
            }
        }

        public static IReadOnlyList<string> IdleCheckTypes { get; } = new List<string>
        {
            IdleCheckType.InputTimeout.ToString(),
            IdleCheckType.SessionLock.ToString(),
        };

        public bool IsIdleCheckTypeInputTimeout
        {
            get => IdleCheckType.InputTimeout == IdleCheckType && StartMiningWhenIdle;
        }
    }
}
