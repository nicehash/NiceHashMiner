using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32;
using System.Windows.Forms;
using System.IO;
using System.Security.Permissions;
using System.Security.AccessControl;

namespace MyDownloader.Extension.WindowsIntegration
{
    [RegistryPermission(SecurityAction.LinkDemand, 
       Read = "Software\\Microsoft\\Windows\\CurrentVersion\\Run",
       Write = "Software\\Microsoft\\Windows\\CurrentVersion\\Run")]
    internal class WindowsStartupUtility
    {
        private static string GetKeyName()
        {
            return Path.GetFileNameWithoutExtension(Application.ExecutablePath);
        }

        private static string GetKeyValue()
        {
            return String.Format("\"{0}\" /as", Application.ExecutablePath);
        }

        private static RegistryKey GetRegistryKey()
        {
            // Create a security context for a new key that we will use to store our data.
            // The security context will restrict access to only our user.
            string user = Environment.UserDomainName + "\\" + Environment.UserName;
            RegistrySecurity security = new RegistrySecurity();
            RegistryAccessRule rule = new RegistryAccessRule(user,
                    RegistryRights.FullControl,
                    InheritanceFlags.ContainerInherit,
                    PropagationFlags.None,
                    AccessControlType.Allow);
            security.AddAccessRule(rule);

            RegistryKey key = Registry.CurrentUser.CreateSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run",
                RegistryKeyPermissionCheck.ReadWriteSubTree, security);

            return key;
        }

        public static bool IsRegistered()
        {
            using (RegistryKey key = GetRegistryKey())
            {
                string value = key.GetValue(GetKeyName(), null) as string;

                return value != null && value.Equals(GetKeyValue(), StringComparison.OrdinalIgnoreCase);
            }
        }

        public static void Register(bool register)
        {
            using (RegistryKey key = GetRegistryKey())
            {
                if (register)
                {
                    key.SetValue(GetKeyName(), GetKeyValue(), RegistryValueKind.String);
                }
                else
                {
                    key.DeleteValue(GetKeyName(), false);
                }
            }
        }
    }
}
