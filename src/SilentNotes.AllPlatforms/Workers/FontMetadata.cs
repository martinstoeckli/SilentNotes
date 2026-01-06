// Copyright © 2025 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace SilentNotes.Workers
{
    /// <summary>
    /// Holds metadata information about a single font file. Use the <see cref="FontMetadataParser"/>
    /// to extract the metadata from a font file.
    /// </summary>
    [DebuggerDisplay("Family:{FontFamily} • SubFamily:{FontSubFamily}")]
    public class FontMetadata
    {
        /// <summary>Gets or sets the font name.</summary>
        public string FullFontName { get; set; }

        /// <summary>Gets or sets the font family name like 'Consolas'.</summary>
        public string FontFamily { get; set; }

        /// <summary>Gets the font sub family like 'regular'.</summary>
        public string FontSubFamily { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the font contains symbols, or readable characters.
        /// </summary>
        public bool IsSymbolFont { get; set; }
    }

    /// <summary>
    /// Parses font files like (*.ttf) and extracts its metadata information. This is a lightweight
    /// fast parser which reads only as much file content as necessary.
    /// </summary>
    /// <remarks>
    /// This class was written to build a list of available fontfamilies. It does not to extract
    /// a complete set of data, but is written generic so that other information can be added as
    /// needed.
    /// </remarks>
    public class FontMetadataParser
    {
        private const UInt16 PlatformIdUnicode = 0;
        private const UInt16 PlatformIdMacintosh = 1;
        private const UInt16 PlatformIdWindows = 3;

        private const UInt16 NameIdFontFamily = 1;
        private const UInt16 NameIdFontSubFamily = 2;
        private const UInt16 NameIdFullFontName = 4;

        /// <summary>
        /// Parses a single font file (like consola.ttf).
        /// </summary>
        /// <param name="fontFilePath">The absolute path to a font file.</param>
        /// <exception cref="ArgumentException">Thrown if the font file is not readable.</exception>
        /// <exception cref="Exception">Thrown if the font file is invalid.</exception>
        /// <returns>Returns the extracted font metadata, or null if the font type is not supported.</returns>
        public async ValueTask<FontMetadata> Parse(string fontFilePath)
        {
            using (Stream fontFile = new FileStream(fontFilePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true))
            {
                return await Parse(fontFile);
            }
        }

        /// <summary>
        /// Parses a font file (like *.ttf).
        /// </summary>
        /// <param name="fontFile">The open stream of the font file.</param>
        /// <exception cref="ArgumentException">Thrown if the font file is not readable.</exception>
        /// <exception cref="Exception">Thrown if the font file is invalid.</exception>
        /// <returns>Returns the extracted font metadata, or null if the font type is not supported.</returns>
        public async ValueTask<FontMetadata> Parse(Stream fontFile)
        {
            if ((fontFile == null) || (!fontFile.CanRead) || (!fontFile.CanSeek))
                throw new ArgumentException("Cannot read from the stream.", nameof(fontFile));

            long originalPos = fontFile.Position;
            try
            {
                FontType fontType = await DetectFontType(fontFile);
                switch (fontType)
                {
                    case FontType.TTF:
                    case FontType.OTF:
                        return await ParseOtf(fontFile);
                    default:
                        return null;
                }
            }
            finally
            {
                fontFile.Position = originalPos;
            }
        }

        private async ValueTask<FontType> DetectFontType(Stream fontFile)
        {
            byte[] TtfSignature = new byte[] { 0x00, 0x01, 0x00, 0x00 };

            fontFile.Position = 0;
            var signature = new byte[4];
            int bytesRead = await fontFile.ReadAsync(signature);
            if (bytesRead < 4)
                return FontType.Unknown;

            if (signature.SequenceEqual(TtfSignature))
                return FontType.TTF;
            else if (signature.SequenceEqual(TagToBytes("OTTO")))
                return FontType.OTF;
            else if (signature.SequenceEqual(TagToBytes("ttcf")))
                return FontType.TTC;
            else if (signature.SequenceEqual(TagToBytes("wOFF")))
                return FontType.WOFF;
            else if (signature.SequenceEqual(TagToBytes("wOF2")))
                return FontType.WOF2;
            else
                return FontType.Unknown;
        }

        /// <summary>
        /// Parses a *.ttf or a *.otf file for metadata.
        /// See also <see cref="https://learn.microsoft.com/de-de/typography/opentype/spec/name"/>.
        /// </summary>
        /// <param name="fontFile">The open stream of the font file.</param>
        /// <returns>Returns the extracted font metadata, or null if the font type is not supported.</returns>
        private async ValueTask<FontMetadata> ParseOtf(Stream fontFile)
        {
            FontMetadata result = new FontMetadata();
            Memory<byte> reusableBuffer = new byte[4096];

            // Place after sfnt version
            fontFile.Seek(4, SeekOrigin.Begin);

            // Determine number of table-records in table-directory
            UInt16 tableCount = await ReadUInt16(fontFile, reusableBuffer);
            fontFile.Seek(6, SeekOrigin.Current); // Skip rest of table-directory

            // Search for table-record with tag 'name'
            var tableRecords = new List<TableRecord>();
            for (int indexTableRecord = 0; indexTableRecord < tableCount; indexTableRecord++)
            {
                var tableRecord = new TableRecord
                {
                    Tag = await ReadTag(fontFile, reusableBuffer),
                    Check = await ReadUInt32(fontFile, reusableBuffer),
                    Offset = await ReadUInt32(fontFile, reusableBuffer),
                    Length = await ReadUInt32(fontFile, reusableBuffer)
                };
                tableRecords.Add(tableRecord);
            }
            TableRecord nameTableRecord = tableRecords.Find(item => item.Tag == "name");
            if (nameTableRecord == null)
                throw new Exception("No name-records found in font file.");

            // Determine number of name-records in table-record
            fontFile.Seek(nameTableRecord.Offset, SeekOrigin.Begin);
            UInt16 tableVersion = await ReadUInt16(fontFile, reusableBuffer);
            UInt16 nameCount = await ReadUInt16(fontFile, reusableBuffer);
            UInt16 storageOffset = await ReadUInt16(fontFile, reusableBuffer);

            // List name records
            var nameRecords = new List<NameRecord>();
            for (int indexNameRecord = 0; indexNameRecord < nameCount; indexNameRecord++)
            {
                var nameRecord = new NameRecord
                {
                    PlatformId = await ReadUInt16(fontFile, reusableBuffer),
                    EncodingId = await ReadUInt16(fontFile, reusableBuffer),
                    LanguageId = await ReadUInt16(fontFile, reusableBuffer),
                    NameId = await ReadUInt16(fontFile, reusableBuffer),
                    Length = await ReadUInt16(fontFile, reusableBuffer),
                    StringOffset = await ReadUInt16(fontFile, reusableBuffer)
                };
                nameRecords.Add(nameRecord);
            }

            // Prefer this order of platforms, because mac is the most difficult to get the
            // encodings right: 'PlatformIdUnicode', 'PlatformIdWindows', 'PlatformIdMacintosh'
            nameRecords.Sort(PreferredNameRecordOrderComparer);

            // Read metadata from name-record.
            UInt32 storageBase = nameTableRecord.Offset + storageOffset;
            foreach (var nameRecord in nameRecords)
            {
                if (IsSymbolicFont(nameRecord))
                    result.IsSymbolFont = true;

                switch (nameRecord.NameId)
                {
                    case NameIdFontFamily:
                        if (string.IsNullOrEmpty(result.FontFamily))
                            result.FontFamily = await ReadNameString(fontFile, reusableBuffer, nameRecord, storageBase);
                        break;
                    case NameIdFontSubFamily:
                        if (string.IsNullOrEmpty(result.FontSubFamily))
                            result.FontSubFamily = await ReadNameString(fontFile, reusableBuffer, nameRecord, storageBase);
                        break;
                    case NameIdFullFontName:
                        if (string.IsNullOrEmpty(result.FullFontName))
                            result.FullFontName = await ReadNameString(fontFile, reusableBuffer, nameRecord, storageBase);
                        break;
                }
            }
            return result;
        }

        private static async ValueTask<UInt16> ReadUInt16(Stream fontFile, Memory<byte> reusableBuffer)
        {
            var buffer = reusableBuffer.Slice(0, 2);
            await fontFile.ReadExactlyAsync(buffer);
            return BinaryPrimitives.ReadUInt16BigEndian(buffer.Span);
        }

        private static async ValueTask<UInt32> ReadUInt32(Stream fontFile, Memory<byte> reusableBuffer)
        {
            var buffer = reusableBuffer.Slice(0, 4);
            await fontFile.ReadExactlyAsync(buffer);
            return BinaryPrimitives.ReadUInt32BigEndian(buffer.Span);
        }

        private static async ValueTask<string> ReadTag(Stream fontFile, Memory<byte> reusableBuffer)
        {
            var buffer = reusableBuffer.Slice(0, 4);
            await fontFile.ReadExactlyAsync(buffer);
            return Encoding.ASCII.GetString(buffer.Span);
        }

        private static async ValueTask<string> ReadNameString(Stream fontFile, Memory<byte> reusableBuffer, NameRecord nameRecord, UInt32 storageBase)
        {
            int bufferSize = Math.Min(nameRecord.Length, reusableBuffer.Length);
            fontFile.Seek(storageBase + nameRecord.StringOffset, SeekOrigin.Begin);
            var buffer = reusableBuffer.Slice(0, bufferSize);
            await fontFile.ReadExactlyAsync(buffer);
            return DecodeNameString(nameRecord.PlatformId, nameRecord.EncodingId, buffer.Span);
        }

        private static string DecodeNameString(UInt16 platformId, UInt16 encodingId, ReadOnlySpan<byte> bytes)
        {
            string result;
            switch (platformId)
            {
                case PlatformIdUnicode:
                    result = Encoding.BigEndianUnicode.GetString(bytes);
                    break;
                case PlatformIdMacintosh:
                    result = GetMacEncoding(encodingId).GetString(bytes);
                    break;
                case PlatformIdWindows:
                    result = GetWindowsEncoding(encodingId).GetString(bytes);
                    break;
                default:
                    throw new Exception("Invalid platform id for name record in font file.");
            }
            return result.TrimEnd('\0');
        }

        private static bool IsSymbolicFont(NameRecord nameRecord)
        {
            return (nameRecord.PlatformId == PlatformIdWindows) && (nameRecord.EncodingId == 0);
        }

        private static Encoding GetMacEncoding(UInt16 encodingId)
        {
            return CodePagesEncodingProvider.Instance.GetEncoding(GetMacCodePage(encodingId));
        }

        private static int GetMacCodePage(UInt16 encodingId, int defaultCodePage = 10000)
        {
            // See https://en.wikipedia.org/wiki/Code_page#Macintosh_emulation_code_pages_2
            switch (encodingId)
            {
                case  0: return 10000; // Apple Macintosh Roman
                case  1: return 10001; // Apple Japanese
                case  2: return 10002; // Apple Traditional Chinese(Big5)
                case  3: return 10003; // Apple Korean
                case  4: return 10004; // Apple Arabic
                case  5: return 10005; // Apple Hebrew
                case  6: return 10006; // Apple Greek
                case  7: return 10007; // Apple Macintosh Cyrillic
                case 21: return 10021; // Apple Thai
                case 25: return 10008; // Apple Simplified Chinese(GB 2312)
                default:
                    return defaultCodePage;
            }
        }

        private static Encoding GetWindowsEncoding(UInt16 encodingId)
        {
            switch (encodingId)
            {
                case 3: return CodePagesEncodingProvider.Instance.GetEncoding(936);
                case 4: return CodePagesEncodingProvider.Instance.GetEncoding(950);
                case 5: return CodePagesEncodingProvider.Instance.GetEncoding(949);
                default:
                    return Encoding.BigEndianUnicode;
            }
        }

        private static byte[] TagToBytes(string tagName)
        {
            return Encoding.ASCII.GetBytes(tagName);
        }

        /// <summary>
        /// The name-record list can contain the same information for each PlatformId/EncodingId/NameId/LanguageId
        /// tuple. We prefer the platform order 'PlatformIdUnicode', 'PlatformIdWindows',
        /// 'PlatformIdMacintosh' so that as soon as the meta information could have been extracted,
        /// we can ignore later platforms are ignored.
        /// </summary>
        private int PreferredNameRecordOrderComparer(NameRecord left, NameRecord right)
        {
            // First sort by platform, put macintosh to the end.
            UInt16 platformOrderLeft = (left.PlatformId == PlatformIdMacintosh) ? UInt16.MaxValue : left.PlatformId;
            UInt16 platformOrderRight = (right.PlatformId == PlatformIdMacintosh) ? UInt16.MaxValue : right.PlatformId;
            var result = platformOrderLeft.CompareTo(platformOrderRight);

            if (result == 0)
                result = left.EncodingId.CompareTo(right.EncodingId);

            if (result == 0)
                result = left.NameId.CompareTo(right.NameId);

            if (result == 0)
                result = left.LanguageId.CompareTo(right.LanguageId);
            return result;
        }

        [DebuggerDisplay("NameId:{NameId} • Platform:{PlatformId} • Encoding:{EncodingId}")]
        private class TableRecord
        {
            /// <summary>The tag contains 4 characters, sometimes with trailing blanks.</summary>
            public string Tag { get; set; }
            public UInt32 Check { get; set; }
            public UInt32 Offset { get; set; }
            public UInt32 Length { get; set; }
        }

        [DebuggerDisplay("Platform:{PlatformId} • Encoding:{EncodingId} • NameId:{NameId}")]
        private class NameRecord
        {
            public UInt16 PlatformId { get; set; }
            public UInt16 EncodingId { get; set; }
            public UInt16 LanguageId { get; set; }
            public UInt16 NameId { get; set; }
            public UInt16 Length { get; set; }
            public UInt16 StringOffset { get; set; }
        }

        /// <summary>
        /// Enumeration of recognized font types.
        /// </summary>
        private enum FontType
        {
            /// <summary>Unknown font type</summary>
            Unknown,

            /// <summary>True type font</summary>
            TTF,

            /// <summary>Open type font (compact font format CFF)</summary>
            OTF,

            /// <summary>Open type font collection</summary>
            TTC,

            /// <summary>Web Open Font Format</summary>
            WOFF,

            /// <summary>Web Open Font Format 2</summary>
            WOF2,
        }
    }
}
