using System;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Threading;

namespace TransferEncodeDecode.Helpers
{
    internal class AssemblyHelper
    {
        public static void ExtractEmbeddedResource(
            Assembly targetAssembly,
            string fileName,
            string resourcePath,
            string outputDirectory)
        {
            string outputPath = Path.Combine(outputDirectory, string.IsNullOrEmpty(fileName) ? fileName : fileName);

            using (Stream s = targetAssembly.GetManifestResourceStream(
                targetAssembly.GetName().Name.Replace("-", "_") + "." + resourcePath + "." + fileName))
            {
                if (s != null)
                {
                    byte[] buffer = new byte[s.Length];
                    s.Read(buffer, 0, buffer.Length);

                    using (BinaryWriter sw = new BinaryWriter(
                        File.Open(
                            Path.Combine(outputDirectory, string.IsNullOrEmpty(fileName) ? fileName : fileName),
                            FileMode.Create)))
                    {
                        sw.Write(buffer);
                    }
                }
            }

            ZipFile.ExtractToDirectory(outputPath, outputDirectory);

            try
            {
                Thread.Sleep(100);
                File.Delete(outputPath);
            }
            catch { }

            return;

            throw new Exception("Cannot find embedded resource '" +
                targetAssembly.GetName().Name.Replace("-", "_") + "." + resourcePath + "." + fileName);
        }
    }
}
