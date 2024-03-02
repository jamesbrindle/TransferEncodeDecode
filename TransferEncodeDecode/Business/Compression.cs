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
            string commonRoot = PathHelper.FindCommonRootDirectory(sourcePaths.ToArray());
            string commonRootFile = Path.Combine(Program.TempPath, $"-temp-common-root-{Extensions.GenerateShortGuid(8)}");

            File.WriteAllText(commonRootFile, commonRoot);

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

            sb.Append($"\"{commonRootFile}\" ");

            ProcessStartInfo processStartInfo = new ProcessStartInfo
            {
                FileName = PathHelper.GetSevenZPathPath(),
                Arguments = $"a -t7z -m0=lzma2 -mx=9 -aoa -y -spf2 \"{destinationPath}\" {sb}",
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                UseShellExecute = false
            };

            using (var process = new Process())
            {
                process.StartInfo = processStartInfo;
                process.Start();
                process.WaitForExit();
            }

            try
            {
                File.Delete(commonRootFile);
            }
            catch { }
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
