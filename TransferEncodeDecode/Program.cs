using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using TransferEncodeDecode.Helpers;

namespace TransferEncodeDecode
{
    internal static class Program
    {
        private static string Arguments = null;

        [STAThread]
        static void Main(string[] args)
        {
            //#if DEBUG
            //            int processId = Process.GetCurrentProcess().Id;
            //            string message = string.Format("Please attach the debugger (elevated) to process [{0}].", processId);
            //            MessageBox.Show(message, "Debug");
            //#endif

            using (new Mutex(true, "TransferEncodeDecode", out bool createdNew))
            {
                bool reRunningAsAdministrator = args != null && args.Length > 0 && args[0].ToLower() == "-a";
                bool uninstalling = args != null && args.Length > 0 && args[0].ToLower() == "-u";

                if (createdNew || reRunningAsAdministrator || uninstalling)
                {
                    try
                    {
                        if (args == null || args.Length == 0 || reRunningAsAdministrator)
                        {
                            Arguments = "-a";
                            RegistryHelper.SetupRegistry(Assembly.GetExecutingAssembly().Location);
                            return;
                        }
                        else if (uninstalling)
                        {
                            Arguments = "-u";
                            RegistryHelper.RemoveFromRegistry();
                            return;
                        }

                        TransferType? transferType = null;
                        string path = null;

                        foreach (string arg in args)
                        {
                            if (arg.Trim().ToLower() == "-d")
                                transferType = TransferType.Decode;
                            else if (arg.Trim().ToLower() == "-e")
                                transferType = TransferType.Encode;
                            else if (File.Exists(arg) || Directory.Exists(arg))
                                path = arg;
                        }

                        if (transferType == null || string.IsNullOrEmpty(path))
                            return;

                        Arguments = $"{(transferType == TransferType.Encode ? "-e" : "-d")} \"{path}\"";

                        Application.EnableVisualStyles();
                        Application.SetCompatibleTextRenderingDefault(false);
                        Application.Run(new MainForm((TransferType)transferType, path));
                    }
                    catch { }
                }
                else
                {
                    var processes = Process.GetProcessesByName("TransferEncodeDecode");
                    foreach (var process in processes)
                        WindowHelper.ShowRestoreAndFocusWindow(process.MainWindowHandle);
                }
            }
        }

        internal static void RestartTheApplicationAsAdministrator()
        {
            if (!ProcessHelper.IsRunningAsAdministrator())
            {
                // Restart program and run as admin
                var exeName = Process.GetCurrentProcess().MainModule.FileName;
                ProcessStartInfo startInfo = new ProcessStartInfo(exeName)
                {
                    UseShellExecute = true,
                    Arguments = Arguments,
                    Verb = "runas" // Run as administrator
                };

                try
                {
                    Process.Start(startInfo);
                    Application.Exit();
                }
                catch (System.ComponentModel.Win32Exception)
                {
                    MessageBox.Show(
                        "The application required elevated privileges and was not granted.",
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }
    }
}
