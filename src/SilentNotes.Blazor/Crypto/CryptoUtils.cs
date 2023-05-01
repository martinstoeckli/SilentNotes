// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text;
using SilentNotes.Crypto.KeyDerivation;
using VanillaCloudStorageClient;

namespace SilentNotes.Crypto
{
    /// <summary>
    /// Helper functions for the crypto library
    /// </summary>
    public static class CryptoUtils
    {
        private const string CryptorObfuscationPackageName = "obfuscation";

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
        /// Truncates a key to a maximum length.
        /// </summary>
        /// <param name="key">Key to truncate.</param>
        /// <param name="maxLength">The part of the key which is longer than this, will be truncated.</param>
        /// <returns>Returns a truncated key, or the original key if its length was smaller or equal
        /// than <paramref name="maxLength"/>.</returns>
        public static byte[] TruncateKey(byte[] key, int maxLength)
        {
            byte[] result;
            if ((key == null) || (key.Length <= maxLength))
            {
                result = key;
            }
            else
            {
                result = new byte[maxLength];
                Array.Copy(key, 0, result, 0, maxLength);
            }
            return result;
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
        public static byte[] Obfuscate(byte[] plainMessage, SecureString obfuscationKey, ICryptoRandomSource randomSource)
        {
            ICryptor encryptor = new Cryptor(CryptorObfuscationPackageName, randomSource);
            return encryptor.Encrypt(
                plainMessage,
                obfuscationKey,
                KeyDerivationCostType.Low,
                "xchacha20_poly1305");
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
        public static string Obfuscate(string plainText, SecureString obfuscationKey, ICryptoRandomSource randomSource)
        {
            return BytesToBase64String(Obfuscate(StringToBytes(plainText), obfuscationKey, randomSource));
        }

        /// <summary>
        /// Reverses a key obfuscated with <see cref="Obfuscate(byte[], SecureString, ICryptoRandomSource)"/>
        /// to its original plain text.
        /// </summary>
        /// <param name="obfuscatedMessage">Obfuscated text.</param>
        /// <param name="obfuscationKey">Key to use for obfuscation, this key is usually hard coded
        /// in the application.</param>
        /// <returns>Original plain message.</returns>
        public static byte[] Deobfuscate(byte[] obfuscatedMessage, SecureString obfuscationKey)
        {
            ICryptor encryptor = new Cryptor(CryptorObfuscationPackageName, null);
            return encryptor.Decrypt(obfuscatedMessage, obfuscationKey);
        }

        /// <summary>
        /// Reverses a string obfuscate with <see cref="Obfuscate(string, SecureString, ICryptoRandomSource)"/>
        /// to its original plain text.
        /// </summary>
        /// <param name="obfuscatedText">Obfuscated text.</param>
        /// <param name="obfuscationKey">Key to use for obfuscation, this key is usually hard coded
        /// in the application.</param>
        /// <returns>Original plain text.</returns>
        public static string Deobfuscate(string obfuscatedText, SecureString obfuscationKey)
        {
            return BytesToString(Deobfuscate(Base64StringToBytes(obfuscatedText), obfuscationKey));
        }

        /// <summary>
        /// Alias of <see cref="SecureStringExtensions.StringToSecureString(string)"/>
        /// </summary>
        /// <param name="password">Password in string form.</param>
        /// <returns>Password in a SecureString.</returns>
        public static SecureString StringToSecureString(string password)
        {
            return SecureStringExtensions.StringToSecureString(password);
        }
    }
}
