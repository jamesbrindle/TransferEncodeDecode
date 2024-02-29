using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using TransferEncodeDecode.Helpers;

namespace TransferEncodeDecode.Business
{
    internal class EncodeDecode
    {
        private readonly MainForm MainForm;

        public EncodeDecode(MainForm mainForm)
        {
            MainForm = mainForm;
        }

        internal void Encode(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                    throw new ApplicationException("Path does not exist");

                // Reading all bytes from the file without locking it
                using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    byte[] fileBytes = new byte[fileStream.Length];
                    fileStream.Read(fileBytes, 0, fileBytes.Length);
                    byte[] compressedBytes = ByteCompressDecompress.Compress(fileBytes);
                    Clipboard.SetText($"::^:{Path.GetFileName(filePath)}:::{Convert.ToBase64String(compressedBytes)}");

                    Thread.Sleep(1000);
                }

                Application.Exit();
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Access to the path") || ex.Message.Contains("denied"))
                {
                    Program.RestartTheApplicationAsAdministrator(false);
                    return;
                }

                Console.WriteLine("Error reading file: " + ex.Message);
                ThreadPool.QueueUserWorkItem(delegate
                {
                    MainForm.SetLabelText(ex.Message);
                    Thread.Sleep(1500);

                    Application.Exit();
                });
            }
        }

        internal void Decode(string directoryPath)
        {
            try
            {
                if (!Directory.Exists(directoryPath))
                    throw new ApplicationException("Directory does not exist");

                string raw = Clipboard.GetText();

                if (raw != null && raw.Length == 0)
                    throw new ApplicationException("No text in clipboard");

                if (!raw.StartsWith("::^:"))
                    throw new ApplicationException("Invalid clipboard data");

                string filename = ExtractFilename(raw);
                string base64Contents = ExtractContent(raw);

                string outputPath = Path.Combine(directoryPath, filename);

                if (File.Exists(outputPath))
                {
                    if (
                        MessageBox.Show(
                            "File already exists. Overwrite?",
                            "Overwrite?",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Exclamation) == DialogResult.Yes)
                    {
                        File.Delete(outputPath);
                        Thread.Sleep(500);
                        File.WriteAllBytes(Path.Combine(directoryPath, filename), ByteCompressDecompress.Decompress(Convert.FromBase64String(base64Contents)));
                    }
                }
                else
                {
                    File.WriteAllBytes(Path.Combine(directoryPath, filename), ByteCompressDecompress.Decompress(Convert.FromBase64String(base64Contents)));
                }

                Thread.Sleep(1000);
                Application.Exit();
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Access to the path") || ex.Message.Contains("denied"))
                {
                    Program.RestartTheApplicationAsAdministrator(false);
                    return;
                }

                Console.WriteLine("Error reading clipboard: " + ex.Message);

                ThreadPool.QueueUserWorkItem(delegate
                {
                    MainForm.SetLabelText(ex.Message);
                    Thread.Sleep(1500);

                    Application.Exit();
                });
            }
        }

        private static string ExtractFilename(string input)
        {
            string pattern = "::\\^:(.*?):::";

            Match match = Regex.Match(input, pattern);
            if (match.Success)
                return match.Groups[1].Value;

            return null;
        }

        private static string ExtractContent(string input)
        {
            string pattern = ":::(.*)";

            Match match = Regex.Match(input, pattern);
            if (match.Success)
                return match.Groups[1].Value;

            return null;
        }
    }
}
