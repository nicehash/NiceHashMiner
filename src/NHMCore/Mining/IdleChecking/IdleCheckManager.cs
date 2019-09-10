using NHM.Common;
using NHM.Common.Enums;
using NHMCore.Configs;
using System;
using System.ComponentModel;

namespace NHMCore.Mining.IdleChecking
{
    public static class IdleCheckManager
    {
        private static IdleChecker _checker;

        static IdleCheckManager()
        {
            IdleMiningSettings.Instance.PropertyChanged += IdleSettingsOnPropertyChanged;
        }

        private static void IdleSettingsOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IdleMiningSettings.IdleCheckType) || 
                e.PropertyName == nameof(IdleMiningSettings.StartMiningWhenIdle))
                StartIdleCheck();
        }

        public static void StartIdleCheck(IdleCheckType type, EventHandler<IdleChangedEventArgs> evnt)
        {
            _checker?.Dispose();

            if (type == IdleCheckType.SessionLock)
                _checker = new SessionLockChecker();
            else
                _checker = new InputTimeoutChecker();

            _checker.IdleStatusChanged += evnt;
            _checker.StartChecking();
        }

        public static void StartIdleCheck()
        {
            if (!ConfigManager.GeneralConfig.StartMiningWhenIdle) return;
            StartIdleCheck(ConfigManager.GeneralConfig.IdleCheckType, IdleTick);
        }

        private static void IdleTick(object sender, IdleChangedEventArgs e)
        {
            if (MiningState.Instance.MiningManuallyStarted || !ConfigManager.GeneralConfig.StartMiningWhenIdle)
                return;

            if (e.IsIdle)
            {
                if (ApplicationStateManager.IsInMainForm)
                {
                    Logger.Info("IDLECHECK", "Entering idling state");
                    ApplicationStateManager.StartAllAvailableDevices();
                }
            }
            else if (MiningState.Instance.IsCurrentlyMining)
            {
                ApplicationStateManager.StopAllDevice();
                Logger.Info("IDLECHECK", "Resumed from idling");
            }
        }
    }
}
