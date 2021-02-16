using Microsoft.Win32;
using NHM.Common;
using System;
using System.Security;
using System.Windows.Forms;

namespace NHMCore.Configs
{
    public class RunAtStartup
    {
        public static RunAtStartup Instance { get; } = new RunAtStartup();
        private readonly RegistryKey _rkStartup;
        private bool _enabled = false;

        private string ExePath
        {
            get
            {
                if (Launcher.IsLauncher)
                {
                    return Paths.RootPath("NiceHashMiner.exe");
                }
                return Application.ExecutablePath;
            }
        }

        private RunAtStartup()
        {
            try
            {
                _rkStartup = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                _enabled = IsInStartupRegistry();
            }
            catch (SecurityException)
            {
            }
            catch (Exception e)
            {
                Logger.Error("RunAtStartup", e.Message);
            }
        }

        public bool Enabled
        {
            get => _enabled;
            set
            {
                // Commit to registry
                try
                {
                    if (value)
                    {
                        // Add NHML to startup registry
                        _rkStartup?.SetValue(Application.ProductName, ExePath);
                    }
                    else
                    {
                        _rkStartup?.DeleteValue(Application.ProductName, false);
                    }
                }
                catch (Exception er)
                {
                    Logger.Error("RunAtStartup", er.Message);
                }
                _enabled = value;
            }
        }

        private bool IsInStartupRegistry()
        {
            // Value is stored in registry
            var startVal = "";
            try
            {
                startVal = (string)_rkStartup?.GetValue(Application.ProductName, "");
            }
            catch (Exception e)
            {
                Logger.Error("RunAtStartup", e.Message);
            }

            return startVal == ExePath;
        }
    }
}
