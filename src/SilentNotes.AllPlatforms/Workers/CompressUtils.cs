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

        /// <summary>
        /// Creates a zip archive from a list of files.
        /// This function does not support sub directories!
        /// </summary>
        /// <param name="entries">List of files, containing the filename and the file content.</param>
        /// <returns>The content of the zip archive.</returns>
        public static byte[] CreateZipArchive(IEnumerable<CompressEntry> entries)
        {
            byte[] result;
            using (MemoryStream outputStream = new MemoryStream())
            {
                using (var zipArchive = new ZipArchive(outputStream, ZipArchiveMode.Create, true))
                {
                    foreach (CompressEntry inputEntry in entries)
                    {
                        var archiveEntry = zipArchive.CreateEntry(inputEntry.Name);
                        using (Stream entryOutputStream = archiveEntry.Open())
                        {
                            entryOutputStream.Write(inputEntry.Data);
                        }
                    }
                }
                result = outputStream.ToArray();
            }
            return result;
        }

        /// <summary>
        /// Opens a zip archive and returns a list of files.
        /// </summary>
        /// <param name="zipContent">The content of a zip archive.</param>
        /// <returns>List of files, containing the filename and the file content.</returns>
        public static List<CompressEntry> OpenZipArchive(byte[] zipContent)
        {
            var result = new List<CompressEntry>();
            using (MemoryStream inputArchiveStream = new MemoryStream(zipContent))
            using (ZipArchive zipArchive = new ZipArchive(inputArchiveStream, ZipArchiveMode.Read))
            {
                foreach (var archiveEntry in zipArchive.Entries)
                {
                    using (var entryInputStream = archiveEntry.Open())
                    using (var buffer = new MemoryStream())
                    {
                        entryInputStream.CopyTo(buffer);
                        result.Add(new CompressEntry
                        {
                            Name = archiveEntry.Name,
                            Data = buffer.ToArray()
                        });
                    }
                }
            }
            return result;
        }

        public class CompressEntry
        {
            public string Name { get; set; }

            public byte[] Data { get; set; }
        }
    }
}
