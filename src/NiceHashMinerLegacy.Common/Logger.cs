using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiceHashMinerLegacy.Common
{
    public static class Logger
    {
        public static bool Enabled { get; set; }
        const string _logsRootPath = "logs";
        private static bool _isInitSucceess = false;
        private static readonly ILog _log = LogManager.GetLogger(typeof(Logger));

        private enum LogType
        {
            Info,
            Debug,
            Warn,
            Error
        }

        private static void LogGroupTextType(string grp, string text, LogType type)
        {
            if (!_isInitSucceess) return;
            // Console.WriteLine does nothing on x64 while debugging with VS, so use Debug. Console.WriteLine works when run from .exe
            var fallbackLogLine = $"[{DateTime.Now.ToLongTimeString()}] [{grp}] {text}";
#if DEBUG
            System.Diagnostics.Debug.WriteLine(fallbackLogLine);
#endif
#if !DEBUG
            Console.WriteLine(fallbackLogLine);
#endif


            if (!Enabled) return;
            // try will prevent an error if something tries to print an invalid character
            try
            {
                var logLine = $"[{grp}] {text}";
                switch (type)
                {
                    case LogType.Debug:
                        _log.Debug(logLine);
                        break;
                    case LogType.Warn:
                        _log.Warn(logLine);
                        break;
                    case LogType.Error:
                        _log.Error(logLine);
                        break;
                    default:
                        _log.Info(logLine);
                        break;
                }
            }
            catch { }  // Not gonna recursively call here in case something is seriously wrong
        }

        public static void Info(string grp, string text)
        {
            LogGroupTextType(grp, text, LogType.Info);
        }

        public static void Debug(string grp, string text)
        {
            LogGroupTextType(grp, text, LogType.Debug);
        }

        public static void Warn(string grp, string text)
        {
            LogGroupTextType(grp, text, LogType.Warn);
        }

        public static void Error(string grp, string text)
        {
            LogGroupTextType(grp, text, LogType.Error);
        }

        #region Delayed logging
        static object _lock = new object();
        static private Dictionary<string, DateTime> _lastLoggedMessages = new Dictionary<string, DateTime>();

        private static bool ShouldLog(string grp, string text, TimeSpan delayTimeSpan)
        {
            var key = $"grp({grp})-text({text})";
            lock (_lock)
            {
                if (!_lastLoggedMessages.ContainsKey(key))
                {
                    _lastLoggedMessages[key] = DateTime.UtcNow;
                    return true;
                }
                else
                {
                    var lastDateTime = _lastLoggedMessages[key];
                    var timeSpanDiff = DateTime.UtcNow.Subtract(lastDateTime);
                    var shouldLog = timeSpanDiff > delayTimeSpan;
                    if (shouldLog) _lastLoggedMessages[key] = DateTime.UtcNow;
                    return shouldLog;
                }
            }
        }

        public static void InfoDelayed(string grp, string text, TimeSpan delayTimeSpan)
        {
            if (!ShouldLog(grp, text, delayTimeSpan)) return;
            LogGroupTextType(grp, text, LogType.Info);
        }

        public static void DebugDelayed(string grp, string text, TimeSpan delayTimeSpan)
        {
            if (!ShouldLog(grp, text, delayTimeSpan)) return;
            LogGroupTextType(grp, text, LogType.Debug);
        }

        public static void WarnDelayed(string grp, string text, TimeSpan delayTimeSpan)
        {
            if (!ShouldLog(grp, text, delayTimeSpan)) return;
            LogGroupTextType(grp, text, LogType.Warn);
        }

        public static void ErrorDelayed(string grp, string text, TimeSpan delayTimeSpan)
        {
            if (!ShouldLog(grp, text, delayTimeSpan)) return;
            LogGroupTextType(grp, text, LogType.Error);
        }

        #endregion Delayed logging

        public static void ConfigureWithFile(bool enableFileLogging, Level logLevel, long maxFileSize = 1048576)
        {
            Enabled = enableFileLogging;
            try
            {
                if (!Directory.Exists(_logsRootPath))
                {
                    Directory.CreateDirectory(_logsRootPath);
                }
                var logFilePath = Path.Combine(_logsRootPath, "log.txt");

                // TODO broken on netstandard
#if NET45
                var h = (Hierarchy)LogManager.GetRepository();
                h.Root.Level = logLevel;

                // if we don't want file skip
                if (enableFileLogging)
                {
                    h.Root.AddAppender(CreateFileAppender(logFilePath, maxFileSize));
                }

                h.Configured = true;
#endif

                _isInitSucceess = true;
            }
            catch
            {
                _isInitSucceess = false;
            }
        }

        public static IAppender CreateFileAppender(string logFilePath, long maxFileSize)
        {
            var appender = new RollingFileAppender
            {
                Name = "RollingFileAppender",
                File = logFilePath,
                AppendToFile = true,
                RollingStyle = RollingFileAppender.RollingMode.Size,
                MaxSizeRollBackups = 1,
                MaxFileSize = maxFileSize,
                PreserveLogFileNameExtension = true,
                Encoding = System.Text.Encoding.Unicode
            };

            var layout = new PatternLayout
            {
                ConversionPattern = "[%date{yyyy-MM-dd HH:mm:ss}] [%level] %message%newline"
            };
            layout.ActivateOptions();

            appender.Layout = layout;
            appender.ActivateOptions();

            return appender;
        }

        //public static IAppender CreateConsoleAppender()
        //{
        //    var layout = new PatternLayout
        //    {
        //        ConversionPattern = "[%date{yyyy-MM-dd HH:mm:ss}] [%level] %message%newline"
        //    };
        //    layout.ActivateOptions();

        //    var consoleAppender = new ConsoleAppender
        //    {
        //        Name = "ConsoleAppender",
        //        Layout = layout
        //    };
        //    consoleAppender.ActivateOptions();

        //    return consoleAppender;
        //}
    }
}
