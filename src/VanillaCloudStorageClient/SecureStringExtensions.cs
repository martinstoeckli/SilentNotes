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
            if ((leftBstr == IntPtr.Zero) || (rightBstr == IntPtr.Zero))
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
        /// Converts a SecureString to to a encoded byte array. The function avoids the generation
        /// of a string, which would depend on the garbage collector to be removed from memory.
        /// </summary>
        /// <param name="secretString">Password as SecureString.</param>
        /// <param name="encoding">The encoding which is used to convert the string to bytes.
        /// If the parameter is null, it defaults to Encoding.UTF8.</param>
        /// <returns>Array of bytes, or null if the password was null.</returns>
        public static byte[] SecureStringToBytes(this SecureString secretString, Encoding encoding)
        {
            if (encoding == null)
                encoding = Encoding.UTF8;
            if (secretString == null)
                return null;
            if (secretString.Length == 0)
                return new byte[0];

            IntPtr secretBstr = IntPtr.Zero;
            char[] secretChars = null;
            try
            {
                secretBstr = Marshal.SecureStringToBSTR(secretString);
                secretChars = new char[secretString.Length];
                Marshal.Copy(secretBstr, secretChars, 0, secretChars.Length);
                return encoding.GetBytes(secretChars);
            }
            finally
            {
                if (secretBstr != IntPtr.Zero)
                    Marshal.ZeroFreeBSTR(secretBstr);
                if (secretChars != null)
                    Array.Clear(secretChars, 0, secretChars.Length);
            }
        }

        /// <summary>
        /// Converts a UTF-8 encoded byte array to a SecureString. The function avoids the generation
        /// of a string, which would depend on the garbage collector to be removed from memory.
        /// </summary>
        /// <param name="secretBytes">Password as byte array. The password should be UTF-8 encoded.</param>
        /// <param name="encoding">The encoding which should be used to build the string from the
        /// bytes. If the parameter is null, it defaults to Encoding.UTF8.</param>
        /// <returns>Password in a SecureString.</returns>
        public static SecureString BytesToSecureString(byte[] secretBytes, Encoding encoding)
        {
            if (encoding == null)
                encoding = Encoding.UTF8;
            if (secretBytes == null)
                return null;

            char[] secretChars = null;
            try
            {
                SecureString result = new SecureString();
                secretChars = encoding.GetChars(secretBytes);
                foreach (char c in secretChars)
                    result.AppendChar(c);
                return result;
            }
            finally
            {
                if (secretChars != null)
                    Array.Clear(secretChars, 0, secretChars.Length);
            }
        }
    }
}
