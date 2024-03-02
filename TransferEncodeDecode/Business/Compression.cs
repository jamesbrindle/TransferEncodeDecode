using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using TransferEncodeDecode.Helpers;

namespace TransferEncodeDecode.Business
{
    internal class Compression
    {
        public static void CompressFilesAndFoldersWith7Zip(List<string> sourcePaths, string destinationPath)
        {
            string workingDirectory = PathHelper.FindCommonRootDirectory(sourcePaths.ToArray());

            StringBuilder sb = new StringBuilder();
            foreach (string path in sourcePaths)
            {
                if (path.IsFile())
                    sb.Append($"\"{path}\" ");
                else if (path.IsDirectory())
                {
                    var files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
                    foreach (var file in files)
                        sb.Append($"\"{file}\" ");
                }
            }

            ProcessStartInfo processStartInfo = new ProcessStartInfo
            {
                FileName = PathHelper.GetSevenZPathPath(),
                Arguments = $"a -t7z -m0=lzma2 -mx=9 -aoa -y -spf2 \"{destinationPath}\" {sb}",
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                WorkingDirectory = workingDirectory,
                UseShellExecute = false
            };

            using (var process = new Process())
            {
                process.StartInfo = processStartInfo;
                process.Start();
                process.WaitForExit();
            }
        }

        public static void ExtractToDirectoryWith7Zip(string archivePath, string outputDirectory)
        {
            ProcessStartInfo processStartInfo = new ProcessStartInfo
            {
                FileName = PathHelper.GetSevenZPathPath(),
                Arguments = $"x \"{archivePath}\" -o\"{outputDirectory}\" -y",
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process process = Process.Start(processStartInfo))
            {
                process.StartInfo = processStartInfo;
                process.Start();
                process.WaitForExit();
            }
        }
    }
}
