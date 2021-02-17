using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace NHM.Common
{
    public static class Logger
    {
        public static bool Enabled { get; set; }
        private static string _logsRootPath => Paths.RootPath("logs");
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
            // Console.WriteLine does nothing on x64 while debugging with VS, so use Debug. Console.WriteLine works when run from .exe
            var fallbackLogLine = $"[{DateTime.Now.ToLongTimeString()}] [{grp}] {text}";

            // Console.WriteLine doesn't write to debug console, while this writes to debug console and log file just fine
            System.Diagnostics.Debug.WriteLine(fallbackLogLine);

            if (!_isInitSucceess) return;
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

                var h = (Hierarchy)LogManager.GetRepository(Assembly.GetEntryAssembly());
                h.Root.Level = logLevel;

                // if we don't want file skip
                if (enableFileLogging)
                {
                    h.Root.AddAppender(CreateFileAppender(logFilePath, maxFileSize));
                }

                h.Configured = true;

                _isInitSucceess = true;
            }
            catch
            {
                _isInitSucceess = false;
            }
        }

        public static void ConfigureWithFile(string logFilePath, long maxFileSize = 1048576)
        {
            Enabled = true;
            try
            {
                var h = (Hierarchy)LogManager.GetRepository(Assembly.GetEntryAssembly());
                h.Root.Level = Level.Info;
                h.Root.AddAppender(CreateFileAppender(logFilePath, maxFileSize));
                h.Configured = true;

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

        public static void ConfigureConsoleLogging(Level logLevel)
        {
            try
            {
                var h = (Hierarchy)LogManager.GetRepository(Assembly.GetEntryAssembly());
                h.Root.Level = logLevel;

                h.Root.AddAppender(CreateConsoleAppender());
                h.Configured = true;

                _isInitSucceess = true;
            }
            catch
            {
                _isInitSucceess = false;
            }
        }

        public static IAppender CreateConsoleAppender()
        {
            var layout = new PatternLayout
            {
                ConversionPattern = "[%date{yyyy-MM-dd HH:mm:ss}] [%level] %message%newline"
            };
            layout.ActivateOptions();

            var consoleAppender = new ConsoleAppender
            {
                Name = "ConsoleAppender",
                Layout = layout
            };
            consoleAppender.ActivateOptions();
            Logger.Info("LOGGER", "CREATED APPENDER");
            return consoleAppender;
        }
    }
}
