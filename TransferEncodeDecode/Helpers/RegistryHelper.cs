using Microsoft.Win32;
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
                    fileClick.SetValue("MultiSelectMode", "Single");

                    RegistryKey subKey = fileClick.CreateSubKey("command");
                    if (subKey != null)
                    {
                        subKey.SetValue("", $"\"{exePath}\" -e \"%1\"");
                        subKey.Close();
                    }

                    fileClick.Close();
                }

                RegistryKey directoryClick = Registry.CurrentUser.CreateSubKey("Software\\Classes\\Directory\\Background\\shell\\" + menuName);
                if (directoryClick != null)
                {
                    directoryClick.SetValue("", "Transfer - Decode");
                    directoryClick.SetValue("icon", exePath);
                    directoryClick.SetValue("MultiSelectMode", "Single");

                    RegistryKey subKey = directoryClick.CreateSubKey("command");
                    if (subKey != null)
                    {
                        subKey.SetValue("", $"\"{exePath}\" -d \"%V\"");
                        subKey.Close();
                    }

                    directoryClick.Close();
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
    }
}
