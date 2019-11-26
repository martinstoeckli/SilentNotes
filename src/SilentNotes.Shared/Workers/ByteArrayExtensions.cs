// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;

namespace SilentNotes.Workers
{
    /// <summary>
    /// Extension methods for byte arrays.
    /// </summary>
    public static class ByteArrayExtensions
    {
        /// <summary>
        /// Determines whether the byte array matches the specified byte array at a given position.
        /// </summary>
        /// <param name="haystack">The byte array whose content is tested.</param>
        /// <param name="needle">The sequence to look for.</param>
        /// <param name="index">The null based start position, where the <paramref name="needle"/>
        /// is expected to be.</param>
        /// <exception cref="ArgumentNullException">If one of the parameters is null.</exception>
        /// <returns>Returns true if the array contains the bytes at this position.</returns>
        public static bool ContainsAt(this byte[] haystack, byte[] needle, int index)
        {
            if (haystack == null)
                throw new ArgumentNullException(nameof(haystack));
            if (needle == null)
                throw new ArgumentNullException(nameof(needle));

            if ((index + needle.Length > haystack.Length) || (index < 0))
                return false;
            for (int position = 0; position < needle.Length; position++)
            {
                if (haystack[index + position] != needle[position])
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Determines whether the byte array matches the specified byte array at a given position.
        /// </summary>
        /// <param name="haystack">The byte array whose content is tested.</param>
        /// <param name="needle">The byte to look for.</param>
        /// <param name="index">The null based start position, where the <paramref name="needle"/>
        /// is expected to be.</param>
        /// <exception cref="ArgumentNullException">If one of the parameters is null.</exception>
        /// <returns>Returns true if the array contains the bytes at this position.</returns>
        public static bool ContainsAt(this byte[] haystack, byte needle, int index)
        {
            return ContainsAt(haystack, new[] { needle }, index);
        }

        /// <summary>
        /// Determines whether the byte array contains a number ('0'-'9') ascii character at a given
        /// position.
        /// </summary>
        /// <param name="array">The byte array whose content is tested.</param>
        /// <param name="index">The null based start position, where the digit character is expected
        /// to be.</param>
        /// <returns>Returns true if the array contains a digit char at this position.</returns>
        public static bool ContainsDigitCharAt(this byte[] array, int index)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));

            if ((index >= array.Length) || (index < 0))
                return false;

            byte candidate = array[index];
            return candidate >= '0' && candidate <= '9';
        }
    }
}
