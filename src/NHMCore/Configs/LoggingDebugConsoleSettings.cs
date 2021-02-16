using NHM.Common;
using NHMCore.Utils;

namespace NHMCore.Configs
{
    public class LoggingDebugConsoleSettings : NotifyChangedBase
    {
        public static LoggingDebugConsoleSettings Instance { get; } = new LoggingDebugConsoleSettings();

        private LoggingDebugConsoleSettings()
        {
            _longProps = new NotifyPropertyChangedHelper<long>(OnPropertyChanged);
            _boolProps = new NotifyPropertyChangedHelper<bool>(OnPropertyChanged);

            DebugConsole = false;
            LogToFile = true;
            LogMaxFileSize = 1048576;
        }

        private NotifyPropertyChangedHelper<long> _longProps;
        private NotifyPropertyChangedHelper<bool> _boolProps;

        public bool DebugConsole
        {
            get => _boolProps.Get(nameof(DebugConsole));
            set => _boolProps.Set(nameof(DebugConsole), value);
        }

        public bool LogToFile
        {
            get => _boolProps.Get(nameof(LogToFile));
            set => _boolProps.Set(nameof(LogToFile), value);
        }

        // in bytes
        public long LogMaxFileSize
        {
            get => _longProps.Get(nameof(LogMaxFileSize));
            set => _longProps.Set(nameof(LogMaxFileSize), value);
        }
    }
}
