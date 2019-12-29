// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Text;
using SilentNotes.Services;

namespace SilentNotes.Workers
{
    /// <summary>
    /// Generates and handles cryptographically safe transfer codes.
    /// The codes use only non-interchangeable characters (e.g. never 0/O 1/I/l).
    /// </summary>
    public static class TransferCode
    {
        private const int CodeLength = 16;

        /// <summary>
        /// Determines whether a code is defined or not.
        /// </summary>
        /// <param name="code">Code variable to test.</param>
        /// <returns>Returns true if a code was set, otherwise false.</returns>
        public static bool IsCodeSet(string code)
        {
            return !string.IsNullOrWhiteSpace(code);
        }

        /// <summary>
        /// Generates a new transfer code.
        /// </summary>
        /// <param name="randomSource">Cryptographically safe random generator.</param>
        /// <returns>New transfer code.</returns>
        public static string GenerateCode(ICryptoRandomService randomSource)
        {
            return GenerateCode(CodeLength, randomSource);
        }

        /// <summary>
        /// Generates a new transfer code.
        /// </summary>
        /// <param name="length">Length of the code.</param>
        /// <param name="randomSource">Cryptographically safe random generator.</param>
        /// <returns>New transfer code.</returns>
        public static string GenerateCode(int length, ICryptoRandomService randomSource)
        {
            StringBuilder sb = new StringBuilder();
            int remainingLength = length;
            do
            {
                int binaryLength = (int)Math.Floor((remainingLength * 3.0 / 4.0) + 1.0);
                byte[] binaryString = randomSource.GetRandomBytes(binaryLength);

                // We take advantage of the fast base64 encoding
                string base64String = Convert.ToBase64String(binaryString);
                foreach (char letter in base64String)
                {
                    if (UnmixableAlphabet.IsOfCorrectAlphabet(letter))
                        sb.Append(letter);
                }

                // If too many characters have been removed, we repeat the procedure
                remainingLength = length - sb.Length;
            }
            while (remainingLength > 0);

            if (sb.Length > length)
                sb.Remove(length, sb.Length - length);
            return sb.ToString();
        }

        /// <summary>
        /// Tries to read unser input and brings it into a wellformed format without whitespaces.
        /// </summary>
        /// <param name="code">User input to validate.</param>
        /// <param name="sanitizedCode">Wellformed code.</param>
        /// <returns>Returns true if the user input was a valid code, otherwise false.</returns>
        public static bool TrySanitizeUserInput(string code, out string sanitizedCode)
        {
            sanitizedCode = null;

            if (string.IsNullOrWhiteSpace(code))
                return false;

            // remove whitespaces
            sanitizedCode = code.Replace(" ", string.Empty).Replace("-", string.Empty);

            if (sanitizedCode.Length != CodeLength)
                return false;

            if (!UnmixableAlphabet.IsOfCorrectAlphabet(sanitizedCode))
                return false;

            return true;
        }

        /// <summary>
        /// Formats a transfer code so it can be presented to the user.
        /// </summary>
        /// <param name="transferCode">Transfercode to format.</param>
        /// <returns>Formatted transfercode.</returns>
        public static string FormatTransferCodeForDisplay(string transferCode)
        {
            if (string.IsNullOrWhiteSpace(transferCode))
            {
                return string.Empty;
            }
            else
            {
                return string.Format(
                    "{0} {1} {2} {3}",
                    transferCode.Substring(0, 4),
                    transferCode.Substring(4, 4),
                    transferCode.Substring(8, 4),
                    transferCode.Substring(12, 4));
            }
        }
    }
}
