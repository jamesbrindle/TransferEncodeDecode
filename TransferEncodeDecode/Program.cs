using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using TransferEncodeDecode.Helpers;

namespace TransferEncodeDecode
{
    internal static class Program
    {
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();

        private static string Arguments = null;

        private static readonly string ApplicationGuid = "{65EE4927-A35F-4038-B9A9-4E62321A013A}";

        internal static string AssemblyPath =
            Path.Combine(Path.GetTempPath(), ApplicationGuid);

        internal static string TempPath =
            Path.Combine(AssemblyPath, "Temp");

        internal static List<string> InputFiles = new List<string>();

        internal static TransferType? TransferType = null;
        internal static bool StartProcess { get; set; } = false;

        private static readonly int _multiFileContextMenuSelectTimeout = -1200;
        private static readonly int _timeToDeductOnNoTempArgFileFoundWaitingANewOne = -500;

        [STAThread]
        static void Main(string[] args)
        {
            //#if DEBUG
            //            int processId = Process.GetCurrentProcess().Id;
            //            string message = string.Format("Please attach the debugger (elevated) to process [{0}].", processId);
            //            MessageBox.Show(message, "Debug");
            //#endif

            SetProcessDPIAware();

            using (new Mutex(true, "TransferEncodeDecode", out bool createdNew))
            {
                bool reRunningAsAdministrator = args != null && args.Length > 0 && args[0].ToLower() == "-a";
                bool uninstalling = args != null && args.Length > 0 && args[0].ToLower() == "-u";

                if (createdNew || reRunningAsAdministrator || uninstalling)
                {
                    PathHelper.SetupTempDirectories();
                    PathHelper.CleanTempDirectory();
                    SetNotOkToStartNewInstance();

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

                        string path = null;

                        args = PathHelper.CleanArgsForDriveSelection(args);
                        foreach (string arg in args)
                        {
                            if (arg.Trim().ToLower() == "-d")
                                TransferType = TransferEncodeDecode.TransferType.Decode;
                            else if (arg.Trim().ToLower() == "-e")
                                TransferType = TransferEncodeDecode.TransferType.Encode;
                            else if (File.Exists(arg) || Directory.Exists(arg))
                            {
                                InputFiles.Add(arg);
                                path = arg;
                            }
                        }

                        if (TransferType == null || string.IsNullOrEmpty(path))
                            return;

                        Arguments = $"{(TransferType == TransferEncodeDecode.TransferType.Encode ? "-e" : "-d")} \"{path}\"";

                        CreateNewInstance();

                    }
                    catch { }
                }
                else
                {
                    if (!IsOKToStartNewInstance())
                    {
                        foreach (string arg in args)
                        {
                            if (!arg.ToLower().StartsWith("-u") &&
                                !arg.ToLower().StartsWith("-a") &&
                                !arg.ToLower().StartsWith("-d") &&
                                !arg.ToLower().StartsWith("-e"))
                            {
                                if (arg.IsFile() || arg.IsDirectory())
                                {
                                    File.WriteAllText(
                                        Path.Combine(TempPath, "-temp-args" + Guid.NewGuid().ToString()),
                                        arg);
                                }
                            }
                        }
                    }
                    else
                    {
                        CreateNewInstance();
                    }
                }
            }
        }

        private static void CreateNewInstance()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            new MainForm((TransferType)TransferType).Show();

            if (InputFiles.Count > 0)
            {
                new Thread((ThreadStart)delegate
                {
                    DateTime deductDateTime = DateTime.Now;
                    while (AddNewFilesAndGetLastFolderWriteTime(ref deductDateTime, _timeToDeductOnNoTempArgFileFoundWaitingANewOne) >
                            DateTime.Now.AddMilliseconds(_multiFileContextMenuSelectTimeout))
                    {
                        Thread.Sleep(50);
                    }

                    StartProcess = true;
                }).Start();
            }

            Application.Run();
        }

        private static bool IsOKToStartNewInstance()
        {
            if (!Directory.Exists(TempPath))
                return false;

            bool newInstanceAllowed = false;

            for (int i = 0; i < 5; i++)
            {
                try
                {
                    newInstanceAllowed = File.Exists(Path.Combine(TempPath, "-temp-nia"));
                    break;
                }
                catch
                {
                    Thread.Sleep(50);
                }
            }

            for (int i = 0; i < 5; i++)
            {
                try
                {
                    File.Delete(Path.Combine(TempPath, "-temp-nia"));
                    break;
                }
                catch
                {
                    Thread.Sleep(50);
                }
            }

            return newInstanceAllowed;
        }

        private static void SetNotOkToStartNewInstance()
        {
            for (int i = 0; i < 5; i++)
            {
                try
                {
                    File.Delete(Path.Combine(TempPath, "-temp-nia"));
                    break;
                }
                catch
                {
                    Thread.Sleep(50);
                }
            }
        }

        private static DateTime AddNewFilesAndGetLastFolderWriteTime(ref DateTime timeToDuctFromIfNoNewFiles, int millisecondsToDeductIfNoNewFiles)
        {
            for (int i = 0; i < 5; i++)
            {
                try
                {
                    var files = new DirectoryInfo(TempPath)
                       .GetFiles()
                       .Where(m => m.Name.StartsWith("-temp-args"));

                    foreach (var file in files)
                    {
                        string newPath = File.ReadAllText(file.FullName);
                        if (!InputFiles.Contains(newPath))
                            InputFiles.Add(newPath);
                    }

                    if (files.Count() == 0)
                    {
                        timeToDuctFromIfNoNewFiles = timeToDuctFromIfNoNewFiles.AddMilliseconds(millisecondsToDeductIfNoNewFiles);
                        return timeToDuctFromIfNoNewFiles;
                    }

                    return new DirectoryInfo(TempPath)
                        .GetFiles()
                        .Where(m => m.Name.StartsWith("-temp-args"))
                        .OrderByDescending(m => m.LastWriteTime)
                        .Take(1)
                        .FirstOrDefault()
                        .LastWriteTime;
                }
                catch
                {
                    Thread.Sleep(50);
                }
            }

            throw new ApplicationException("Error reading arg files list");
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
