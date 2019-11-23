// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System.IO;
using System.IO.Compression;

namespace SilentNotes.Workers
{
    /// <summary>
    /// Helper class for compression and decompression of data with the GZip algorithm.
    /// </summary>
    public static class CompressUtils
    {
        /// <summary>
        /// Compresses data with GZip. Don't use this function repeatedly on already compressed data.
        /// </summary>
        /// <param name="data">Data to compress.</param>
        /// <returns>Compressed data, or null if <paramref name="data"/> was null.</returns>
        public static byte[] Compress(byte[] data)
        {
            if (data == null)
                return null;
            if (data.Length == 0)
                return new byte[0];

            byte[] result;
            using (MemoryStream inputStream = new MemoryStream(data))
            using (MemoryStream outputStream = new MemoryStream())
            {
                using (GZipStream zipStream = new GZipStream(outputStream, CompressionLevel.Optimal))
                {
                    inputStream.CopyTo(zipStream);
                }
                result = outputStream.ToArray();
            }
            return result;
        }

        /// <summary>
        /// Decompresses data previously compressed with <see cref="Compress(byte[])"/>.
        /// </summary>
        /// <param name="compressedData">Compressed data to decompress.</param>
        /// <returns>Decompressed data, or null if <paramref name="compressedData"/> was null.</returns>
        public static byte[] Decompress(byte[] compressedData)
        {
            if (compressedData == null)
                return null;
            if (compressedData.Length == 0)
                return new byte[0];

            byte[] result;
            using (MemoryStream inputStream = new MemoryStream(compressedData))
            using (MemoryStream outputStream = new MemoryStream())
            {
                using (GZipStream zipStream = new GZipStream(inputStream, CompressionMode.Decompress))
                {
                    zipStream.CopyTo(outputStream);
                }
                result = outputStream.ToArray();
            }
            return result;
        }
    }
}
