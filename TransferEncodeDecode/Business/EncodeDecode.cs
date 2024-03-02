using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using TransferEncodeDecode.Helpers;

namespace TransferEncodeDecode.Business
{
    internal class EncodeDecode
    {
        private readonly MainForm MainForm;
        private string tempDirectoryPath = string.Empty;
        private string tempArchivePath = string.Empty;

        public EncodeDecode(MainForm mainForm)
        {
            MainForm = mainForm;
        }

        internal void Encode()
        {
            try
            {
                tempArchivePath = Path.Combine(Program.TempPath, $"{Guid.NewGuid()}.7z");

                Compression.CompressFilesAndFoldersWith7Zip(Program.InputFiles, tempArchivePath);

                const int bufferSize = 8192;
                var buffer = new byte[bufferSize];

                string ascii85Contents = null;
                using (FileStream fileStream = new FileStream(tempArchivePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    int bytesRead;
                    while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) > 0)
                        memoryStream.Write(buffer, 0, bytesRead);

                    memoryStream.Position = 0;
                    ascii85Contents = "::::" + Ascii85Encoder.Encode(memoryStream);
                }

                MainForm.Invoke(new Action(() =>
                {
                    Clipboard.SetText(ascii85Contents);
                }));

                PathHelper.DeleteArchiveTempPaths(tempArchivePath);

                Thread.Sleep(1000);
                Application.Exit();
            }
            catch (OutOfMemoryException)
            {
                ThreadPool.QueueUserWorkItem(delegate
                {
                    MainForm.SetLabelText("File too big");
                    PathHelper.DeleteArchiveTempPaths(tempArchivePath);

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
                    PathHelper.DeleteArchiveTempPaths(tempArchivePath);

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

                if (!ascii85Contents.StartsWith("::::"))
                    throw new ApplicationException("Invalid clipboard data");

                tempArchivePath = Path.Combine(Program.TempPath, Extensions.GenerateShortGuid(8), $"{Extensions.GenerateShortGuid(8)}.7z");
                tempDirectoryPath = Path.GetDirectoryName(tempArchivePath);

                if (!Directory.Exists(tempDirectoryPath))
                    Directory.CreateDirectory(tempDirectoryPath);

                using (var inputMemoryStream = Ascii85Encoder.DecodeToStream(ascii85Contents.Substring(4)))
                using (var fileStream = new FileStream(tempArchivePath, FileMode.Create, FileAccess.Write))
                {
                    inputMemoryStream.CopyTo(fileStream);
                }

                Compression.ExtractToDirectoryWith7Zip(tempArchivePath, tempDirectoryPath);

                string childPath = Directory.GetDirectories(tempDirectoryPath)[0];
                if (PathHelper.FilesExistAtDestination(childPath, directoryPath))
                {
                    if (!ConfirmOverwrite())
                    {
                        try
                        {
                            PathHelper.DeleteArchiveTempPaths(tempArchivePath, tempDirectoryPath);
                            Application.Exit();
                        }
                        catch { }
                    }
                    else
                    {
                        PathHelper.DeleteFilesAtDistination(childPath, directoryPath);
                    }
                }

                PathHelper.MoveContents(childPath, directoryPath);
                PathHelper.DeleteArchiveTempPaths(tempArchivePath, tempDirectoryPath);

                Thread.Sleep(1000);
                Application.Exit();
            }
            catch (OutOfMemoryException)
            {
                ThreadPool.QueueUserWorkItem(delegate
                {
                    MainForm.SetLabelText("File too big");
                    PathHelper.DeleteArchiveTempPaths(tempArchivePath, tempDirectoryPath);

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

                ThreadPool.QueueUserWorkItem(delegate
                {
                    MainForm.SetLabelText(ex.Message);
                    PathHelper.DeleteArchiveTempPaths(tempArchivePath, tempDirectoryPath);

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
    }
}
