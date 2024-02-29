using System.IO;
using System.IO.Compression;

namespace TransferEncodeDecode.Helpers
{
    internal class ByteCompressDecompress
    {
        internal static byte[] Compress(byte[] data)
        {
            using (MemoryStream output = new MemoryStream())
            {
                using (GZipStream gzip = new GZipStream(output, CompressionMode.Compress))
                {
                    gzip.Write(data, 0, data.Length);
                }

                return output.ToArray();
            }
        }

        internal static byte[] Decompress(byte[] data)
        {
            using (MemoryStream input = new MemoryStream(data))
            {
                using (MemoryStream output = new MemoryStream())
                {
                    using (GZipStream gzip = new GZipStream(input, CompressionMode.Decompress))
                    {
                        gzip.CopyTo(output);
                    }

                    return output.ToArray();
                }
            }
        }
    }
}
