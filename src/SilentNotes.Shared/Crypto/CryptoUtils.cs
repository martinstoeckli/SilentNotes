// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Runtime.CompilerServices;
using System.Text;
using SilentNotes.Crypto.KeyDerivation;

namespace SilentNotes.Crypto
{
    /// <summary>
    /// Helper functions for the crypto library
    /// </summary>
    public static class CryptoUtils
    {
        /// <summary>
        /// Gets a byte array from a given string.
        /// </summary>
        /// <param name="text">Text to convert to bytes.</param>
        /// <returns>Bytes of the text.</returns>
        public static byte[] StringToBytes(string text)
        {
            return Encoding.UTF8.GetBytes(text);
        }

        /// <summary>
        /// Gets a string from a given byte array.
        /// </summary>
        /// <param name="bytes">Array to convert to string.</param>
        /// <returns>String of the byte array.</returns>
        public static string BytesToString(byte[] bytes)
        {
            return Encoding.UTF8.GetString(bytes, 0, bytes.Length);
        }

        /// <summary>
        /// Gets a byte array from a given base64 encoded string.
        /// </summary>
        /// <param name="text">Base64 encoded text to convert to bytes.</param>
        /// <returns>Bytes of the text.</returns>
        public static byte[] Base64StringToBytes(string text)
        {
            return Convert.FromBase64String(text);
        }

        /// <summary>
        /// Gets a base64 encoded string from a given byte array.
        /// </summary>
        /// <param name="bytes">Array to convert to base64 encoded string.</param>
        /// <returns>String of the byte array.</returns>
        public static string BytesToBase64String(byte[] bytes)
        {
            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// Sets the content of an array to zero.
        /// </summary>
        /// <typeparam name="T">Type of the array to clean.</typeparam>
        /// <param name="arr">Array to clean.</param>
        [MethodImpl(MethodImplOptions.NoOptimization)]
        public static void CleanArray<T>(T[] arr)
        {
            if (arr != null)
                Array.Clear(arr, 0, arr.Length);
        }

        /// <summary>
        /// Generates a random string of a given length, using the random source of
        /// the operating system.The string contains only safe characters of this
        /// alphabet: 0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz
        /// </summary>
        /// <param name="length">Number of characters the string should have.</param>
        /// <param name="randomSource">Random generator.</param>
        /// <returns>New randomly generated string.</returns>
        public static string GenerateRandomBase62String(int length, ICryptoRandomSource randomSource)
        {
            if (length < 0)
                throw new ArgumentOutOfRangeException("length");

            StringBuilder result = new StringBuilder();
            int remainingLength = length;
            do
            {
                // We take advantage of the fast base64 encoding
                int binaryLength = (int)((remainingLength * 3.0 / 4.0) + 1.0);
                byte[] randomBytes = randomSource.GetRandomBytes(binaryLength);
                string base64String = Convert.ToBase64String(randomBytes);

                // Remove invalid characters
                result.Append(base64String);
                result.Replace("+", string.Empty);
                result.Replace("/", string.Empty);
                result.Replace("=", string.Empty);

                // If too many characters have been removed, we repeat the procedure
                remainingLength = length - result.Length;
            }
            while (remainingLength > 0);

            result.Length = length;
            return result.ToString();
        }

        /// <summary>
        /// This function can be used, if an API key must be stored inside the application code,
        /// so it doesn't show up in plain text.
        /// </summary>
        /// <param name="plainMessage">The text to hide.</param>
        /// <param name="obfuscationKey">Key to use for obfuscation, this key is usually hard coded
        /// in the application.</param>
        /// <param name="randomSource">A cryptographically random source.</param>
        /// <returns>Obfuscated message.</returns>
        public static byte[] Obfuscate(byte[] plainMessage, string obfuscationKey, ICryptoRandomSource randomSource)
        {
            EncryptorDecryptor encryptor = new EncryptorDecryptor("obfuscation");
            return encryptor.Encrypt(
                plainMessage,
                obfuscationKey,
                KeyDerivationCostType.Low,
                randomSource,
                "twofish_gcm");
        }

        /// <summary>
        /// This function can be used, if an API key must be stored inside the application code,
        /// so it doesn't show up in plain text.
        /// </summary>
        /// <param name="plainText">The text to hide.</param>
        /// <param name="obfuscationKey">Key to use for obfuscation, this key is usually hard coded
        /// in the application.</param>
        /// <param name="randomSource">A cryptographically random source.</param>
        /// <returns>Obfuscated text.</returns>
        public static string Obfuscate(string plainText, string obfuscationKey, ICryptoRandomSource randomSource)
        {
            return BytesToBase64String(Obfuscate(StringToBytes(plainText), obfuscationKey, randomSource));
        }

        /// <summary>
        /// Reverses a key obfuscated with <see cref="Obfuscate(byte[], string, ICryptoRandomSource)"/>
        /// to its original plain text.
        /// </summary>
        /// <param name="obfuscatedMessage">Obfuscated text.</param>
        /// <param name="obfuscationKey">Key to use for obfuscation, this key is usually hard coded
        /// in the application.</param>
        /// <returns>Original plain message.</returns>
        public static byte[] Deobfuscate(byte[] obfuscatedMessage, string obfuscationKey)
        {
            EncryptorDecryptor encryptor = new EncryptorDecryptor("obfuscation");
            return encryptor.Decrypt(obfuscatedMessage, obfuscationKey);
        }

        /// <summary>
        /// Reverses a string obfuscate with <see cref="Obfuscate(string, string, ICryptoRandomSource)"/>
        /// to its original plain text.
        /// </summary>
        /// <param name="obfuscatedText">Obfuscated text.</param>
        /// <param name="obfuscationKey">Key to use for obfuscation, this key is usually hard coded
        /// in the application.</param>
        /// <returns>Original plain text.</returns>
        public static string Deobfuscate(string obfuscatedText, string obfuscationKey)
        {
            return BytesToString(Deobfuscate(Base64StringToBytes(obfuscatedText), obfuscationKey));
        }
    }
}
