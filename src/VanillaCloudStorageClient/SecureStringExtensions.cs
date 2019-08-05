// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Net;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace VanillaCloudStorageClient
{
    /// <summary>
    /// Extension methods of the <see cref="SecureString"/> class.
    /// Converting a string to SecureString and back should be avoided as much as possible.
    /// Sometimes it cannot be completely avoided, so we just try to minimize the duration, a
    /// password is kept plain text in memory.
    /// </summary>
    public static class SecureStringExtensions
    {
        /// <summary>
        /// Converts a string to a SecureString. Keep the usage of this method to a minimum, try to
        /// work with SeureString all the way instead.
        /// </summary>
        /// <param name="password">Password in string form.</param>
        /// <returns>Password in a SecureString.</returns>
        public static SecureString StringToSecureString(string password)
        {
            if (password == null)
                return null;
            else
                return new NetworkCredential(null, password).SecurePassword;
        }

        /// <summary>
        /// Converts a SecureString to a string. Keep the usage of this method to a minimum, try to
        /// work with SeureString all the way instead.
        /// </summary>
        /// <param name="password">Password as SecureString.</param>
        /// <returns>Password in a plain text managed string.</returns>
        public static string SecureStringToString(this SecureString password)
        {
            if (password == null)
                return null;
            else
                return new NetworkCredential(null, password).Password;
        }

        /// <summary>
        /// Checks for equality of two SecureStrings. Note that a timing attack can reveal the
        /// original length of the string.
        /// </summary>
        /// <param name="password1">Left secure string to compare.</param>
        /// <param name="password2">Right secure string to compare.</param>
        /// <returns>Returns true if the strings are equal, otherwise false.</returns>
        public static bool AreEqual(this SecureString password1, SecureString password2)
        {
            if ((password1 == null) && (password2 == null))
                return true;
            if ((password1 == null) || (password2 == null) || (password1.Length != password2.Length))
                return false;

            // Decrypt the string to disposable memory
            IntPtr password1Bstr = IntPtr.Zero;
            IntPtr password2Bstr = IntPtr.Zero;
            try
            {
                password1Bstr = Marshal.SecureStringToBSTR(password1);
                password2Bstr = Marshal.SecureStringToBSTR(password2);
                return BstrAreEqual(password1Bstr, password2Bstr);
            }
            finally
            {
                if (password1Bstr != IntPtr.Zero)
                    Marshal.ZeroFreeBSTR(password1Bstr);
                if (password2Bstr != IntPtr.Zero)
                    Marshal.ZeroFreeBSTR(password2Bstr);
            }
        }

        private static bool BstrAreEqual(IntPtr leftBstr, IntPtr rightBstr)
        {
            if ((leftBstr == null) || (rightBstr == null))
                return false;

            int lengthLeft = Marshal.ReadInt32(leftBstr, -4);
            int lengthRight = Marshal.ReadInt32(rightBstr, -4);
            if (lengthLeft != lengthRight)
                return false;

            bool result = true;
            for (int i = 0; i < lengthLeft; i++)
            {
                byte byteLeft = Marshal.ReadByte(leftBstr + i);
                byte byteRight = Marshal.ReadByte(rightBstr + i);
                if (byteLeft != byteRight)
                    result = false;
            }
            return result;
        }

        /// <summary>
        /// Converts a byte array to a SecureString.
        /// </summary>
        /// <param name="bytes">Password as byte array. The password should be Unicode encoded,
        /// means 2 bytes represent a single character.</param>
        /// <returns>Password in a SecureString.</returns>
        public static SecureString UnicodeBytesToSecureString(byte[] bytes)
        {
            if (bytes == null)
                return null;

            SecureString result = new SecureString();
            char[] password = null;
            try
            {
                password = Encoding.Unicode.GetChars(bytes);
                foreach (char c in password)
                    result.AppendChar(c);
            }
            finally
            {
                if (password != null)
                    Array.Clear(password, 0, password.Length);
            }
            return result;
        }

        /// <summary>
        /// Converts a SecureString to to a byte array.
        /// </summary>
        /// <param name="password">Password as SecureString.</param>
        /// <returns>Array of bytes, or null if the password was null.</returns>
        public static byte[] SecureStringToUnicodeBytes(this SecureString password)
        {
            if (password == null)
                return null;
            if (password.Length == 0)
                return new byte[0];

            IntPtr passwordBstr = IntPtr.Zero;
            try
            {
                passwordBstr = Marshal.SecureStringToBSTR(password);
                int length = Marshal.ReadInt32(passwordBstr, -4);
                byte[] result = new byte[length];

                for (int i = 0; i < length; i++)
                    result[i] = Marshal.ReadByte(passwordBstr, i);
                return result;
            }
            finally
            {
                if (passwordBstr != IntPtr.Zero)
                    Marshal.ZeroFreeBSTR(passwordBstr);
            }
        }
    }
}
