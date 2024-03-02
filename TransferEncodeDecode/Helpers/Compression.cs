using System.IO;
using System.IO.Compression;

namespace TransferEncodeDecode.Helpers
{
    internal class Compression
    {
        internal static Stream Compress(Stream inputData)
        {
            MemoryStream output = new MemoryStream();
            using (GZipStream gzip = new GZipStream(output, CompressionMode.Compress, true))
            {
                inputData.CopyTo(gzip);
            }

            output.Position = 0; // Reset the position to the beginning for reading
            return output;
        }

        internal static Stream Decompress(Stream inputData)
        {
            MemoryStream output = new MemoryStream();
            using (GZipStream gzip = new GZipStream(inputData, CompressionMode.Decompress))
            {
                gzip.CopyTo(output);
            }

            output.Position = 0; // Reset the position to the beginning for reading
            return output;
        }
    }
}
