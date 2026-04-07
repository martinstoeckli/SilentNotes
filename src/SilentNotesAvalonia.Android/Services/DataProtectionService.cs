using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.DataProtection;
using SilentNotes.Services;

namespace SilentNotesAvalonia.Android.Services
{
    internal class DataProtectionService : IDataProtectionService
    {
        private readonly IDataProtectionProvider _provider;

        public DataProtectionService(IDataProtectionProvider provider)
        {
            _provider = provider;
        }

        public string Protect(byte[] unprotectedData)
        {
            IDataProtector protector = _provider.CreateProtector("test");
            byte[] protectedData = protector.Protect(unprotectedData);
            return BytesToBase64String(protectedData);

        }

        public byte[] Unprotect(string protectedData)
        {
            byte[] protectedBinary = Base64StringToBytes(protectedData);
            IDataProtector protector = _provider.CreateProtector("test");
            return protector.Unprotect(protectedBinary);
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
    }
}
