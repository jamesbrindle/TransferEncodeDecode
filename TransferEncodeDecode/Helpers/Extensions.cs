using System;
using System.IO;

namespace TransferEncodeDecode.Helpers
{
    internal static class Extensions
    {
        internal static bool IsFile(this string path)
        {
            try
            {
                return !File.GetAttributes(path).HasFlag(FileAttributes.Directory);
            }
            catch { return false; }
        }

        internal static bool IsDirectory(this string path)
        {
            try
            {
                if (path.Length == 2)
                {
                    if (path.EndsWith(":"))
                        return true;
                }
                if (path.Length == 3)
                {
                    if (path.EndsWith(":\\"))
                        return true;
                }

                return File.GetAttributes(path).HasFlag(FileAttributes.Directory);
            }
            catch { return false; }
        }

        internal static string GenerateShortGuid(int length = 8)
        {
            if (length <= 0 || length > 32)
            {
                throw new ArgumentOutOfRangeException(nameof(length), "Length must be between 1 and 32.");
            }

            return Guid.NewGuid().ToString("N").Substring(0, length);
        }
    }
}
