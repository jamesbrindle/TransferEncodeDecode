using Microsoft.Win32;
using System.Collections.Generic;
using System.Windows.Forms;

namespace TransferEncodeDecode.Helpers
{
    internal static class RegistryHelper
    {
        internal static void SetupRegistry(string exePath)
        {
            try
            {
                string menuName = "TransferEncode";
                string iconPath = exePath;

                RegistryKey fileClick = Registry.CurrentUser.CreateSubKey("Software\\Classes\\*\\shell\\" + menuName);
                if (fileClick != null)
                {
                    fileClick.SetValue("", "Transfer - Encode");
                    fileClick.SetValue("icon", exePath);
                    fileClick.SetValue("MultiSelectMode", "Player");

                    RegistryKey subKey = fileClick.CreateSubKey("command");
                    if (subKey != null)
                    {
                        subKey.SetValue("", $"\"{exePath}\" -e \"%1\"");
                        subKey.Close();
                    }

                    fileClick.Close();
                }

                RegistryKey directoryClick = Registry.CurrentUser.CreateSubKey("Software\\Classes\\Directory\\shell\\" + menuName);
                if (directoryClick != null)
                {
                    directoryClick.SetValue("", "Transfer - Encode");
                    directoryClick.SetValue("icon", exePath);
                    directoryClick.SetValue("MultiSelectMode", "Player");

                    RegistryKey subKey = directoryClick.CreateSubKey("command");
                    if (subKey != null)
                    {
                        subKey.SetValue("", $"\"{exePath}\" -e \"%1\"");
                        subKey.Close();
                    }

                    directoryClick.Close();
                }

                RegistryKey directoryBackgroundClick = Registry.CurrentUser.CreateSubKey("Software\\Classes\\Directory\\Background\\shell\\" + menuName);
                if (directoryBackgroundClick != null)
                {
                    directoryBackgroundClick.SetValue("", "Transfer - Decode");
                    directoryBackgroundClick.SetValue("icon", exePath);
                    directoryBackgroundClick.SetValue("MultiSelectMode", "Single");

                    RegistryKey subKey = directoryBackgroundClick.CreateSubKey("command");
                    if (subKey != null)
                    {
                        subKey.SetValue("", $"\"{exePath}\" -d \"%V\"");
                        subKey.Close();
                    }

                    directoryBackgroundClick.Close();
                }

                AddAppCompatibilityFlag(exePath);

                MessageBox.Show("Transfer Encoder - Decoder Installed", "Installed", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch
            {
                Program.RestartTheApplicationAsAdministrator();
            }
        }

        internal static void RemoveFromRegistry()
        {
            try
            {
                string menuName = "TransferEncode";

                string fileClickKeyPath = "Software\\Classes\\*\\shell\\" + menuName;
                if (IsRegistryKeyExists(Registry.CurrentUser, fileClickKeyPath))
                {
                    Registry.CurrentUser.DeleteSubKeyTree(fileClickKeyPath);
                }

                string directoryClickKeyPath = "Software\\Classes\\Directory\\Background\\shell\\" + menuName;
                if (IsRegistryKeyExists(Registry.CurrentUser, directoryClickKeyPath))
                {
                    Registry.CurrentUser.DeleteSubKeyTree(directoryClickKeyPath);
                }

                MessageBox.Show("Transfer Encoder - Decoder Uninstalled", "Uninstalled", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch
            {
                Program.RestartTheApplicationAsAdministrator();
            }
        }
        internal static List<InstalledProgram> GetInstalledPrograms()
        {
            var installedPrograms = new List<InstalledProgram>();
            installedPrograms.AddRange(ReadRegistryUninstall(RegistryView.Registry32));
            installedPrograms.AddRange(ReadRegistryUninstall(RegistryView.Registry64));

            return installedPrograms;
        }

        private static List<InstalledProgram> ReadRegistryUninstall(RegistryView view)
        {
            var installedPrograms = new List<InstalledProgram>();
            const string REGISTRY_KEY = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
            using (var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, view))
            {
                using (var subKey = baseKey.OpenSubKey(REGISTRY_KEY))
                {
                    foreach (string subkey_name in subKey.GetSubKeyNames())
                    {
                        using (var key = subKey.OpenSubKey(subkey_name))
                        {
                            if (!string.IsNullOrEmpty(key.GetValue("DisplayName") as string))
                            {
                                installedPrograms.Add(new InstalledProgram
                                {
                                    Platform = view == RegistryView.Registry32
                                                            ? InstalledProgram.PlatFormType.X86
                                                            : InstalledProgram.PlatFormType.X64,

                                    DisplayName = (string)key.GetValue("DisplayName"),
                                    Version = (string)key.GetValue("DisplayVersion"),
                                    InstalledDate = (string)key.GetValue("InstallDate"),
                                    Publisher = (string)key.GetValue("Publisher"),
                                    InstallLocation = (string)key.GetValue("InstallLocation"),
                                    UninstallCommand = (string)key.GetValue("UninstallString"),
                                    UninstallSubkeyName = subkey_name
                                });
                            }
                            key.Close();
                        }
                    }
                    subKey.Close();
                }

                baseKey.Close();
            }

            return installedPrograms;
        }

        private static bool IsRegistryKeyExists(RegistryKey root, string subKeyPath)
        {
            using (RegistryKey subKey = root.OpenSubKey(subKeyPath))
            {
                return subKey != null;
            }
        }

        private static void AddAppCompatibilityFlag(string exePath)
        {
            const string keyPath = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Layers";
            string value = "~ PERPROCESSSYSTEMDPIFORCEON HIGHDPIAWARE";

            using (RegistryKey baseKey = Registry.CurrentUser)
            {
                using (RegistryKey key = baseKey.CreateSubKey(keyPath))
                {
                    key?.SetValue(exePath, value, RegistryValueKind.String);
                }
            }
        }
        public class InstalledProgram
        {
            public enum PlatFormType
            {
                X86,
                X64
            }

            public PlatFormType Platform { get; set; }
            public string DisplayName { get; set; }
            public string Version { get; set; }
            public string InstalledDate { get; set; }
            public string Publisher { get; set; }
            public string InstallLocation { get; set; }
            public string UninstallCommand { get; set; }
            public string ModifyPath { get; set; }
            public string UninstallSubkeyName { get; set; }
        }
    }
}
