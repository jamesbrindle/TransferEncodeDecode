using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace TransferEncodeDecode.Helpers
{
    internal class PathHelper
    {
        internal static string[] CleanPathsForDriveSelection(string[] paths)
        {
            try
            {
                if (paths != null && paths.Length > 0)
                {
                    if (paths[0].ToLower().Contains(":\" "))
                    {
                        paths[0] = paths[0].Replace("\"", "");
                        paths = paths[0].Split(' ');
                    }
                    else if (paths[0].ToLower().Contains(": "))
                        paths = paths[0].Split(' ');

                    if (paths[1].ToLower().Contains(":\" "))
                        paths[1] = paths[1].Replace("\"", "");

                    if (paths[1].ToLower().Contains(":\""))
                        paths[1] = paths[1].Replace("\"", "");

                    if (paths[1].ToLower().Contains(": "))
                        paths[1] = paths[1].Replace(" ", "");
                }
            }
            catch { }

            return paths;
        }

        internal static string RemoveDriveLetter(string path)
        {
            string root = Path.GetPathRoot(path);
            if (!string.IsNullOrEmpty(root) && path.StartsWith(root))
                return path.Substring(root.Length);

            return path;
        }

        internal static string FindCommonRootDirectory(string[] paths)
        {
            if (paths == null || paths.Length == 0)
                return string.Empty;

            string[][] splitPaths = paths.Select(path => path.Split(Path.DirectorySeparatorChar)).ToArray();
            int minLength = splitPaths.Min(subPath => subPath.Length);
            string commonRoot = "";

            for (int i = 0; i < minLength; i++)
            {
                string currentComponent = splitPaths[0][i];

                if (splitPaths.All(subPath => subPath[i] == currentComponent))
                    commonRoot = Path.Combine(commonRoot, currentComponent);
                else
                    break;
            }

            if (commonRoot.Length == 2)
                return commonRoot + "\\";

            if (commonRoot.Substring(1, 1) == ":")
            {
                string[] parts = commonRoot.Split(':');
                commonRoot = string.Empty;

                for (int i = 0; i < parts.Length; i++)
                {
                    if (i == 0)
                        commonRoot += parts[i] + ":\\";
                    else
                        commonRoot += parts[i];

                }
            }

            return commonRoot.IsFile() ? Path.GetDirectoryName(commonRoot) : commonRoot;
        }

        internal static void SetupTempDirectories()
        {
            if (!Directory.Exists(Program.TempPath))
            {
                Directory.CreateDirectory(Program.TempPath);
                MakeDirectoryHidden(Program.AssemblyPath);
            }
        }

        private static void MakeDirectoryHidden(string directoryPath)
        {
            if (Directory.Exists(directoryPath))
            {
                FileAttributes attributes = File.GetAttributes(directoryPath);
                attributes |= FileAttributes.Hidden;
                File.SetAttributes(directoryPath, attributes);
            }
        }

        internal static bool FilesExistAtDestination(string sourcePath, string destinationPath)
        {
            var files = Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories);

            for (int i = 0; i < files.Length; i++)
                files[i] = files[i].Replace(sourcePath, destinationPath);

            for (int i = 0; i < files.Length; i++)
            {
                if (File.Exists(files[i]))
                    return true;
            }

            return false;
        }

        internal static void DeleteFilesAtDistination(string sourcePath, string destinationPath)
        {
            var files = Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories);

            for (int i = 0; i < files.Length; i++)
                files[i] = files[i].Replace(sourcePath, destinationPath);

            for (int i = 0; i < files.Length; i++)
            {
                if (File.Exists(files[i]))
                {
                    try
                    {
                        File.Delete(files[i]);
                    }
                    catch { }
                }
            }
        }

        internal static void MoveContents(string sourceDirPath, string destDirPath, string excludePath)
        {
            if (!Directory.Exists(sourceDirPath))
            {
                Console.WriteLine($"Source directory does not exist: {sourceDirPath}");
                return;
            }

            if (!Directory.Exists(destDirPath))
                Directory.CreateDirectory(destDirPath);

            string[] files = Directory.GetFiles(sourceDirPath);
            foreach (string file in files)
            {
                string fileName = Path.GetFileName(file);
                string destFile = Path.Combine(destDirPath, fileName);

                if (file != excludePath)
                    File.Move(file, destFile);
            }

            string[] directories = Directory.GetDirectories(sourceDirPath);
            foreach (string directory in directories)
            {
                string dirName = Path.GetFileName(directory);
                string destDir = Path.Combine(destDirPath, dirName);

                Directory.CreateDirectory(destDir);
                MoveContents(directory, destDir, excludePath);
                Directory.Delete(directory, true);
            }
        }

        internal static void CleanTempDirectory()
        {
            foreach (var file in Directory.GetFiles(Program.TempPath, "*.*", SearchOption.AllDirectories))
            {
                try
                {
                    File.Delete(file);
                }
                catch { }
            }

            foreach (var directory in Directory.GetDirectories(Program.TempPath, "*.*", SearchOption.AllDirectories))
            {
                try
                {
                    Directory.Delete(directory);
                }
                catch { }
            }
        }

        internal static void DeleteArchiveTempPaths(string filePath, string directoryPath = null)
        {
            try
            {
                File.Delete(filePath);
            }
            catch { }

            if (!string.IsNullOrEmpty(directoryPath))
            {
                try
                {
                    Directory.Delete(directoryPath, true);
                }
                catch { }
            }
        }

        internal static string GetSevenZPathPath()
        {
            string sevenZPath = string.Empty;

            sevenZPath = Path.Combine(Program.AssemblyPath, "7z.exe");
            if (File.Exists(sevenZPath))
                return sevenZPath;

            sevenZPath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "Lib", "7z.exe");
            if (File.Exists(sevenZPath))
                return sevenZPath;

            sevenZPath = @"C:\Program Files\7-Zip\7z.exe";
            if (File.Exists(sevenZPath))
                return sevenZPath;

            sevenZPath = @"C:\Program Files (x86)\7-Zip\7z.exe";
            if (File.Exists(sevenZPath))
                return sevenZPath;

            var installedLocation = RegistryHelper.GetInstalledPrograms().Where(z => z.DisplayName.Contains("7-Zip")).FirstOrDefault();
            if (installedLocation != null && File.Exists(Path.Combine(installedLocation.InstallLocation, "7z.exe")))
                return Path.Combine(installedLocation.InstallLocation, "7z.exe");

            AssemblyHelper.ExtractEmbeddedResource(Assembly.GetExecutingAssembly(), "Dependencies.zip", "Embed", Program.AssemblyPath);
            sevenZPath = Path.Combine(Program.AssemblyPath, "7z.exe");
            if (File.Exists(sevenZPath))
                return sevenZPath;

            throw new ApplicationException("Unable to find 7z.exe");
        }
    }
}
