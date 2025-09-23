// Copyright © 2023 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;

namespace SilentNotes.Workers
{
    /// <summary>
    /// Can be used to detect modifications between two states of the data, by calculating a
    /// fingerprint of its content.
    /// </summary>
    internal class ModificationDetector
    {
        private readonly Func<long?> _fingerPrintProvider;
        private long? _memorizedFingerPrint;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModificationDetector"/> class.
        /// </summary>
        /// <param name="fingerPrintProvider">A delegate which can calculate a finger print (hash)
        /// value, which identifies the current content of the data.</param>
        /// <param name="memorizeCurrentState">If true, <see cref="MemorizeCurrentState"/> is
        /// called immediately by the constructor.</param>
        /// <exception cref="ArgumentNullException">Is thrown if a required parameter is null.</exception>
        public ModificationDetector(Func<long?> fingerPrintProvider, bool memorizeCurrentState = true)
        {
            if (fingerPrintProvider == null)
                throw new ArgumentNullException(nameof(fingerPrintProvider));

            _fingerPrintProvider = fingerPrintProvider;
            if (memorizeCurrentState)
                MemorizeCurrentState();
        }

        /// <summary>
        /// Calculates the finger print (hash) of the current content of the data.
        /// </summary>
        public virtual void MemorizeCurrentState()
        {
            _memorizedFingerPrint = _fingerPrintProvider();
        }

        /// <summary>
        /// Checks whether the content of the data has changed since the last call to
        /// <see cref="MemorizeCurrentState"/>, by calculating and comparing a new finger print.
        /// </summary>
        /// <returns>Returns true if the content of the data has changed, otherwise false.</returns>
        public bool IsModified()
        {
            long? currentFingerprint = _fingerPrintProvider();
            return _memorizedFingerPrint != currentFingerprint;
        }

        /// <summary>
        /// Calculates a hash code out of a list of hash codes.
        /// </summary>
        /// <remarks>The function guarantees that no integer overflow can happen.</remarks>
        /// <param name="hashCodes">Enumeration of hash codes to combine.</param>
        /// <param name="withOldHashCode">Optional base hash code. If specified, the new hash is
        /// combined with the old hash value, making chaining possible.</param>
        /// <returns>Hash code calculated from the list of hash codes.</returns>
        public static long CombineHashCodes(IEnumerable<long> hashCodes, long withOldHashCode = 0)
        {
            unchecked
            {
                long result = withOldHashCode;
                foreach (long hashCode in hashCodes)
                    result = (result * 397) ^ hashCode;
                return result;
            }
        }

        /// <summary>
        /// Calculates a hash code out of a string.
        /// </summary>
        /// <remarks>The function guarantees that no integer overflow can happen. It uses SHA256
        /// to calculate the hash, giving the string a better collision resistance of 4 long hashes.</remarks>
        /// <param name="text">The text to hash.</param>
        /// <param name="withOldHashCode">Optional base hash code. If specified, the new hash is
        /// combined with the old hash value, making chaining possible.</param>
        /// <returns>Hash code calculated from the string.</returns>
        public static long CombineWithStringHash(string text, long withOldHashCode = 0)
        {
            if (text == null)
                return withOldHashCode;

            Span<byte> textHashBytes;
            using (var sha256 = SHA256.Create())
            {
                textHashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(text));
            }

            // Convert 32 byte hash into 4 long values.
            long[] hashCodes = new long[4];
            for (int index = 0; index < 4; index++)
            {
                Span<byte> bytes = textHashBytes.Slice(index * 8, 8);
                hashCodes[index] = BinaryPrimitives.ReadInt64BigEndian(bytes);
            }
            return CombineHashCodes(hashCodes, withOldHashCode);
        }

        /// <summary>
        /// Calculates a hash code out of a Guid.
        /// </summary>
        /// <remarks>The function guarantees that no integer overflow can happen. It gives the hash
        /// a better collision resistance with the whole range of a long (instead of int).
        /// Don't use it for cryptographic uses.</remarks>
        /// <param name="id">The Guid to hash.</param>
        /// <param name="withOldHashCode">Optional base hash code. If specified, the new hash is
        /// combined with the old hash value, making chaining possible.</param>
        /// <returns>Hash code calculated from the Guid.</returns>
        public static long CombineWithGuidHash(Guid id, long withOldHashCode = 0)
        {
            Span<byte> guidHashBytes;
            using (var hash = SHA256.Create())
            {
                guidHashBytes = hash.ComputeHash(id.ToByteArray());
            }

            // Convert 32 byte hash into 4 long values.
            long[] hashCodes = new long[4];
            for (int index = 0; index < 4; index++)
            {
                Span<byte> bytes = guidHashBytes.Slice(index * 8, 8);
                hashCodes[index] = BinaryPrimitives.ReadInt64BigEndian(bytes);
            }
            return CombineHashCodes(hashCodes, withOldHashCode);
        }
    }
}
