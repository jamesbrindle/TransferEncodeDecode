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

                const int bufferSize = 8192;
                var buffer = new byte[bufferSize];

                string ascii85Contents = null;
                using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    int bytesRead;
                    while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) > 0)
                        memoryStream.Write(buffer, 0, bytesRead);

                    memoryStream.Position = 0;
                    ascii85Contents = Ascii85Encoder.Encode(Compression.Compress(memoryStream));
                }

                MainForm.Invoke(new Action(() =>
                {
                    Clipboard.SetText(
                        $"::^:{Path.GetFileName(filePath)}:::" +
                        $"{ascii85Contents}");
                }));


                Application.Exit();
            }
            catch (OutOfMemoryException)
            {
                ThreadPool.QueueUserWorkItem(delegate
                {
                    MainForm.SetLabelText("File too big");
                    Thread.Sleep(1500);

                    Application.Exit();
                });

            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Access to the path") || ex.Message.Contains("denied"))
                {
                    Program.RestartTheApplicationAsAdministrator();
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

                string ascii85Contents = null;
                MainForm.Invoke(new Action(() =>
                {
                    ascii85Contents = Clipboard.GetText();
                }));


                if (string.IsNullOrEmpty(ascii85Contents))
                    throw new ApplicationException("No text in clipboard");

                if (!ascii85Contents.StartsWith("::^:"))
                    throw new ApplicationException("Invalid clipboard data");

                string filename = ExtractFilename(ascii85Contents);
                ascii85Contents = ExtractContent(filename, ascii85Contents);

                string outputPath = Path.Combine(directoryPath, filename);

                if (File.Exists(outputPath) && !ConfirmOverwrite())
                    Application.Exit();

                using (var inputMemoryStream = Ascii85Encoder.DecodeToStream(ascii85Contents))
                using (var decompressedStream = Compression.Decompress(inputMemoryStream))
                using (var fileStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
                {
                    decompressedStream.CopyTo(fileStream);
                }

                Thread.Sleep(1000);

                Application.Exit();
            }
            catch (OutOfMemoryException)
            {
                ThreadPool.QueueUserWorkItem(delegate
                {
                    MainForm.SetLabelText("File too big");
                    Thread.Sleep(1500);

                    Application.Exit();
                });

            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Access to the path") || ex.Message.Contains("denied"))
                {
                    Program.RestartTheApplicationAsAdministrator();
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

        private bool ConfirmOverwrite()
        {
            return MessageBox.Show(
                "File already exists. Overwrite?",
                "Overwrite?",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Exclamation) == DialogResult.Yes;
        }

        private static string ExtractFilename(string input)
        {
            string pattern = "::\\^:(.*?):::";

            Match match = Regex.Match(input, pattern);
            if (match.Success)
                return match.Groups[1].Value;

            return null;
        }

        private static string ExtractContent(string filename, string input)
        {
            return input.Substring(filename.Length + 7);
        }
    }
}
