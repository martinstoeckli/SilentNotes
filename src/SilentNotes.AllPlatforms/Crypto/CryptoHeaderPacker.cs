// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Text;
using SilentNotes.Workers;

namespace SilentNotes.Crypto
{
    /// <summary>
    /// Can pack a <see cref="CryptoHeader"/> together with a cypher, so that the same parameters
    /// can be extracted for decryption.
    /// </summary>
    public class CryptoHeaderPacker
    {
        private const char Separator = '$';
        private const string RevisionSeparator = " v=";

        /// <summary>
        /// Pack an encryption header and its cipher together to a byte array.
        /// </summary>
        /// <param name="header">Header containing the encryption parameters.</param>
        /// <param name="cipher">Cipher generated with those encryption parameters.</param>
        /// <returns>Packed cipher.</returns>
        public static byte[] PackHeaderAndCypher(CryptoHeader header, byte[] cipher)
        {
            if (header == null)
                throw new ArgumentNullException("header");
            if (cipher == null)
                throw new ArgumentNullException("cipher");
            if (!string.IsNullOrEmpty(header.Cost) && header.Cost.Contains(Separator))
                throw new ArgumentException("The cost parameter must not contain a separator character.");

            StringBuilder sb = new StringBuilder();
            sb.Append(header.PackageName + RevisionSeparator + CryptoHeader.NewestSupportedRevision);
            sb.Append(Separator);
            sb.Append(header.AlgorithmName);
            sb.Append(Separator);
            sb.Append(CryptoUtils.BytesToBase64String(header.Nonce));
            sb.Append(Separator);
            sb.Append(header.KdfName);
            sb.Append(Separator);
            if (header.Salt != null)
                sb.Append(CryptoUtils.BytesToBase64String(header.Salt));
            sb.Append(Separator);
            sb.Append(header.Cost);
            sb.Append(Separator);
            sb.Append(header.Compression);
            sb.Append(Separator);
            string stringHeader = sb.ToString();
            byte[] binaryHeader = CryptoUtils.StringToBytes(stringHeader);

            byte[] result = new byte[cipher.Length + binaryHeader.Length];
            Array.Copy(binaryHeader, 0, result, 0, binaryHeader.Length);
            Array.Copy(cipher, 0, result, binaryHeader.Length, cipher.Length);
            return result;
        }

        /// <summary>
        /// Checks whether a byte array starts with a known header, containing the app name and an
        /// optional revision number.
        /// </summary>
        /// <param name="packedCipher">Byte array to examine.</param>
        /// <param name="expectedAppName">The header must start with this app name.</param>
        /// <param name="revision">reveives the revision number.</param>
        /// <returns>Returns true if the array starts with a valid header, otherwise false.</returns>
        public static bool HasMatchingHeader(byte[] packedCipher, string expectedAppName, out int revision)
        {
            if (packedCipher != null)
            {
                // test for app name
                if (ByteArrayExtensions.ContainsAt(packedCipher, CryptoUtils.StringToBytes(expectedAppName), 0))
                {
                    int position = expectedAppName.Length;

                    // Followed by separator?
                    if (ByteArrayExtensions.ContainsAt(packedCipher, (byte)Separator, position))
                    {
                        revision = 1;
                        return true;
                    }

                    if (ByteArrayExtensions.ContainsAt(packedCipher, CryptoUtils.StringToBytes(RevisionSeparator), position))
                    {
                        int digitsStart = position + RevisionSeparator.Length;
                        int digitsEnd = digitsStart;

                        // Skip digits
                        while (ByteArrayExtensions.ContainsDigitCharAt(packedCipher, digitsEnd))
                            digitsEnd++;

                        // Followed by separator?
                        if ((digitsEnd > digitsStart) &&
                            (ByteArrayExtensions.ContainsAt(packedCipher, (byte)Separator, digitsEnd)))
                        {
                            byte[] digitsPart = new byte[digitsEnd - digitsStart];
                            Array.Copy(packedCipher, digitsStart, digitsPart, 0, digitsPart.Length);
                            revision = int.Parse(CryptoUtils.BytesToString(digitsPart));
                            return true;
                        }
                    }
                }
            }

            revision = 0;
            return false;
        }

        /// <summary>
        /// Unpacks a cipher formerly packed with <see cref="PackHeaderAndCypher(CryptoHeader, byte[])"/>.
        /// </summary>
        /// <param name="packedCipher">Packed cipher containing a header and its cipher.</param>
        /// <param name="expectedAppName">The app name which must match.</param>
        /// <param name="header">Receives the extracted header.</param>
        /// <param name="cipher">Receives the extracted cipher.</param>
        /// <exception cref="CryptoExceptionInvalidCipherFormat">Thrown if it doesn't contain a valid header.</exception>
        /// <exception cref="CryptoUnsupportedRevisionException">Thrown if it was packed with a future incompatible version.</exception>
        public static void UnpackHeaderAndCipher(byte[] packedCipher, string expectedAppName, out CryptoHeader header, out byte[] cipher)
        {
            header = null;
            cipher = null;
            if (!HasMatchingHeader(packedCipher, expectedAppName, out int revision))
                throw new CryptoExceptionInvalidCipherFormat();

            if (revision > CryptoHeader.NewestSupportedRevision)
                throw new CryptoUnsupportedRevisionException();

            int expectedSeparatorCount = (revision > 1) ? 7 : 6;
            int lastSeparatorPos = IndexOfLastSeparator(packedCipher, expectedSeparatorCount);
            if (lastSeparatorPos < 0)
                throw new CryptoExceptionInvalidCipherFormat();

            // Read cipher part
            int cipherLength = packedCipher.Length - lastSeparatorPos - 1;
            cipher = new byte[cipherLength];
            Array.Copy(packedCipher, lastSeparatorPos + 1, cipher, 0, cipherLength);

            // Read header part
            try
            {
                string headerString = Encoding.UTF8.GetString(packedCipher, 0, lastSeparatorPos + 1);
                string[] parts = headerString.Split(new char[] { Separator });
                header = new CryptoHeader
                {
                    PackageName = expectedAppName,
                    Revision = revision,
                    AlgorithmName = parts[1],
                    Nonce = CryptoUtils.Base64StringToBytes(parts[2]),
                    KdfName = string.IsNullOrEmpty(parts[3]) ? null : parts[3],
                    Salt = string.IsNullOrEmpty(parts[4]) ? null : CryptoUtils.Base64StringToBytes(parts[4]),
                    Cost = string.IsNullOrEmpty(parts[5]) ? null : parts[5],
                };
                if (revision > 1)
                {
                    header.Compression = parts[6];
                }
            }
            catch (Exception)
            {
                throw new CryptoExceptionInvalidCipherFormat();
            }
        }

        private static int IndexOfLastSeparator(byte[] packedCipher, int expectedSeparatorCount)
        {
            byte binarySeparator = Convert.ToByte(Separator);

            int separatorCount = 0;
            for (int index = 0; index < packedCipher.Length; index++)
            {
                if (binarySeparator == packedCipher[index])
                    separatorCount++;
                if (separatorCount == expectedSeparatorCount)
                    return index;
            }
            return -1;
        }
    }
}
