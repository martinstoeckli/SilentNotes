using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.AspNetCore.DataProtection;
using SilentNotes.Services;

namespace SilentNotesAvalonia.Desktop.Services
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

    // todo: stom experimental code
    internal static class LinuxKeyUtils
    {
        // key_serial_t is int in the C API
        public const int KEY_SPEC_THREAD_KEYRING = -1;
        public const int KEY_SPEC_PROCESS_KEYRING = -2;
        public const int KEY_SPEC_SESSION_KEYRING = -3;
        public const int KEY_SPEC_USER_KEYRING = -4;
        public const int KEY_SPEC_USER_SESSION_KEYRING = -5;
        public const int KEY_SPEC_GROUP_KEYRING = -6;
        public const int KEY_SPEC_REQKEY_AUTH_KEY = -7;

        // add_key(const char *type, const char *description,
        //         const void *payload, size_t plen,
        //         key_serial_t keyring);
        [DllImport("libkeyutils.so.1", SetLastError = true, CharSet = CharSet.Ansi)]
        public static extern int add_key(
            string type,
            string description,
            byte[] payload,
            UIntPtr plen,
            int keyring);

        // keyctl(int cmd, ...);
        [DllImport("libkeyutils.so.1", SetLastError = true)]
        public static extern int keyctl(
            int cmd,
            int arg2,
            IntPtr arg3,
            IntPtr arg4,
            IntPtr arg5);
    }
}
