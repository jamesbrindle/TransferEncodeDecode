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

                RegistryKey fileClick = Registry.ClassesRoot.CreateSubKey("*\\shell\\" + menuName);
                if (fileClick != null)
                {
                    fileClick.SetValue("", "Transfer - Encode");
                    fileClick.SetValue("icon", exePath);

                    RegistryKey subKey = fileClick.CreateSubKey("command");
                    if (subKey != null)
                    {
                        subKey.SetValue("", $"\"{exePath}\" -e \"%1\"");
                        subKey.Close();
                    }

                    fileClick.Close();
                }

                RegistryKey directoryClick = Registry.ClassesRoot.CreateSubKey("Directory\\Background\\shell\\" + menuName);
                if (directoryClick != null)
                {
                    directoryClick.SetValue("", "Transfer - Decode");
                    directoryClick.SetValue("icon", exePath);

                    RegistryKey subKey = directoryClick.CreateSubKey("command");
                    if (subKey != null)
                    {
                        subKey.SetValue("", $"\"{exePath}\" -d \"%V\"");
                        subKey.Close();
                    }

                    directoryClick.Close();
                }

                MessageBox.Show("Transfer Encoder - Decoder Installed", "Installed", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch
            {
                Program.RestartTheApplicationAsAdministrator(true);
            }
        }

        internal static void RemoveFromRegistry()
        {
            try
            {
                string menuName = "TransferEncode";

                // Remove registry key for file click
                string fileClickKeyPath = "*\\shell\\" + menuName;
                if (IsRegistryKeyExists(Registry.ClassesRoot, fileClickKeyPath))
                {
                    Registry.ClassesRoot.DeleteSubKeyTree(fileClickKeyPath);
                }

                // Remove registry key for directory click
                string directoryClickKeyPath = "Directory\\Background\\shell\\" + menuName;
                if (IsRegistryKeyExists(Registry.ClassesRoot, directoryClickKeyPath))
                {
                    Registry.ClassesRoot.DeleteSubKeyTree(directoryClickKeyPath);
                }

                MessageBox.Show("Transfer Encoder - Decoder Uninstalled", "Uninstalled", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch
            {
                Program.RestartTheApplicationAsAdministrator(true);
            }
        }

        private static bool IsRegistryKeyExists(RegistryKey root, string subKeyPath)
        {
            using (RegistryKey subKey = root.OpenSubKey(subKeyPath))
            {
                return subKey != null;
            }
        }
    }
}
