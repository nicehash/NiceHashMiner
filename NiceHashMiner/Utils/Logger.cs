using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using NiceHashMiner.Configs;
using System;
using System.IO;

namespace NiceHashMiner
{
    public class Logger
    {
        public static bool IsInit;
        public static readonly ILog Log = LogManager.GetLogger(typeof(Logger));

        public const string LogPath = @"logs\";

        public static void ConfigureWithFile()
        {
            try
            {
                if (!Directory.Exists("logs"))
                {
                    Directory.CreateDirectory("logs");
                }
            }
            catch { }

            IsInit = true;
            try
            {
                var h = (Hierarchy) LogManager.GetRepository();

                if (ConfigManager.GeneralConfig.LogToFile)
                    h.Root.Level = Level.Info;
                //else if (ConfigManager.Instance.GeneralConfig.LogLevel == 2)
                //    h.Root.Level = Level.Warn;
                //else if (ConfigManager.Instance.GeneralConfig.LogLevel == 3)
                //    h.Root.Level = Level.Error;

                h.Root.AddAppender(CreateFileAppender());
                h.Configured = true;
            }
            catch
            {
                IsInit = false;
            }
        }

        public static IAppender CreateFileAppender()
        {
            var appender = new RollingFileAppender
            {
                Name = "RollingFileAppender",
                File = LogPath + "log.txt",
                AppendToFile = true,
                RollingStyle = RollingFileAppender.RollingMode.Size,
                MaxSizeRollBackups = 1,
                MaxFileSize = ConfigManager.GeneralConfig.LogMaxFileSize,
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
    }
}
