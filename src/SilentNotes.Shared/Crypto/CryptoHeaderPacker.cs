// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Text;

namespace SilentNotes.Crypto
{
    /// <summary>
    /// Can pack a <see cref="CryptoHeader"/> together with a cypher, so that the same parameters
    /// can be extracted for decryption.
    /// </summary>
    public class CryptoHeaderPacker
    {
        private const char Separator = '$';
        private const int ExpectedSeparatorCount = 6;

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

            StringBuilder sb = new StringBuilder();
            sb.Append(header.AppName);
            sb.Append(Separator);
            sb.Append(header.AlgorithmName);
            sb.Append(Separator);
            sb.Append(CryptoUtils.BytesToBase64String(header.Nonce));
            sb.Append(Separator);
            sb.Append(header.KdfName);
            sb.Append(Separator);
            sb.Append(CryptoUtils.BytesToBase64String(header.Salt));
            sb.Append(Separator);
            sb.Append(header.Cost);
            sb.Append(Separator);
            string stringHeader = sb.ToString();
            byte[] binaryHeader = CryptoUtils.StringToBytes(stringHeader);

            byte[] result = new byte[cipher.Length + binaryHeader.Length];
            Array.Copy(binaryHeader, 0, result, 0, binaryHeader.Length);
            Array.Copy(cipher, 0, result, binaryHeader.Length, cipher.Length);
            return result;
        }

        /// <summary>
        /// Unpacks a cipher formerly packed with <see cref="PackHeaderAndCypher(CryptoHeader, byte[])"/>.
        /// </summary>
        /// <param name="packedCipher">Packed cipher containing a header and its cipher.</param>
        /// <param name="header">Receives the extracted header.</param>
        /// <param name="cipher">Receives the extracted cipher.</param>
        public static void UnpackHeaderAndCipher(byte[] packedCipher, out CryptoHeader header, out byte[] cipher)
        {
            if (packedCipher == null)
                throw new ArgumentNullException("packedCipher");

            int lastSeparatorPos = IndexOfLastSeparator(packedCipher, ExpectedSeparatorCount);
            if (lastSeparatorPos < 0)
                throw new CryptoExceptionInvalidCipherFormat();

            // Read cipher part
            int cipherLength = packedCipher.Length - lastSeparatorPos - 1;
            cipher = new byte[cipherLength];
            Array.Copy(packedCipher, lastSeparatorPos + 1, cipher, 0, cipherLength);

            // Read header part
            string headerString = Encoding.UTF8.GetString(packedCipher, 0, lastSeparatorPos + 1);
            header = UnpackHeader(headerString);
        }

        private static CryptoHeader UnpackHeader(string headerString)
        {
            string[] parts = headerString.Split(new char[] { Separator }, StringSplitOptions.RemoveEmptyEntries);

            CryptoHeader result = new CryptoHeader();
            result.AppName = parts[0];
            result.AlgorithmName = parts[1];
            result.Nonce = CryptoUtils.Base64StringToBytes(parts[2]);
            result.KdfName = parts[3];
            result.Salt = CryptoUtils.Base64StringToBytes(parts[4]);
            result.Cost = parts[5];
            return result;
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
