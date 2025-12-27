// Copyright © 2025 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Text;

namespace SilentNotes.Workers
{
    /// <summary>
    /// This class can generate Guids, which are relative to another Guid. The difference between
    /// two relative Guids can e.g. be used to determine their relationship.
    /// The generation of related Guids is deterministic and can be predicted.
    /// 
    /// <example>
    ///   The following Guids differ only at the last position:
    ///   "6a541929-b9a8-4704-b95c-89584f431343"
    ///   "6a541929-b9a8-4704-b95c-89584f431345"
    ///   ➜ their relative distance is 2.
    /// </example>
    /// </summary>
    /// 
    /// <remarks>
    /// The last 6 bytes of a Guid are random values (see https://datatracker.ietf.org/doc/html/rfc4122#section-4.4),
    /// thus they can be modified without the risk of an invalid Guid being created.
    /// </remarks>
    public static class RelativeGuid
    {
        /// <summary>
        /// Creates a relative Guid to the <paramref name="originalGuid"/>, whose last part differs
        /// by a given distance.
        /// <example>
        /// CreateRelativeGuid("6a541929-b9a8-4704-b95c-89584f431343", 2)
        ///   => "6a541929-b9a8-4704-b95c-89584f431345"
        /// </example>
        /// </summary>
        /// <param name="originalGuid">The original Guid.</param>
        /// <param name="distance">The difference we want to add or substract from the original
        /// Guid (it can be positive or negative).</param>
        /// <returns>A Guid relative to the <paramref name="originalGuid"/>.</returns>
        public static Guid CreateRelativeGuid(Guid originalGuid, int distance)
        {
            byte[] guidBytes = originalGuid.ToByteArray();

            // Modify the last 4 bytes of the guid which are random
            Span<byte> endPart = guidBytes.AsSpan().Slice(12, 4);
            uint endInt = BinaryPrimitives.ReadUInt32BigEndian(endPart);

            // Add the difference (accept an overflow of the range)
            unchecked
            {
                endInt = endInt + (uint)distance;
            }

            BinaryPrimitives.WriteUInt32BigEndian(endPart, endInt);
            return new Guid(guidBytes);
        }

        /// <summary>
        /// Checks whether two Guids are related. They are related if they where created by
        /// <see cref="CreateRelativeGuid(Guid, int)"/>.
        /// </summary>
        /// <param name="guid1">First Gui to test.</param>
        /// <param name="guid2">Second Gui to test.</param>
        /// <returns>Returns true if the Guids are related, false if they are unrelated.</returns>
        public static bool AreGuidsRelated(Guid guid1, Guid guid2)
        {
            Span<byte> guidBytes1 = guid1.ToByteArray();
            Span<byte> guidBytes2 = guid2.ToByteArray();
            return AreGuidsRelated(guidBytes1, guidBytes2);
        }

        private static bool AreGuidsRelated(Span<byte> guidBytes1, Span<byte> guidBytes2)
        {
            // Compare the first 12 bytes
            Span<byte> guidStartPart1 = guidBytes1.Slice(0, 12);
            Span<byte> guidStartPart2 = guidBytes2.Slice(0, 12);
            return guidStartPart1.SequenceEqual(guidStartPart2);
        }

        /// <summary>
        /// Determines how the relative Guid compares to the original Guid and returns their distance.
        /// Call <see cref="AreGuidsRelated(Guid, Guid)"/> if you are not sure whether the
        /// Guids are related.
        /// </summary>
        /// <param name="relativeGuid">The relative Guid to compare.</param>
        /// <param name="originalGuid">The original Guid to compare.</param>
        /// <returns>The distance between the relative Guids.
        /// - A positive value indicates that the relative guid is bigger than the original guid.
        /// - The value 0 means that there is no relative distance (though the Guids aren't necessarily equal).
        /// - A negative value indicates that the relative guid is smaller than the original guid.</returns>
        public static int CompareRelativeGuids(Guid relativeGuid, Guid originalGuid)
        {
            Span<byte> relativeBytes = relativeGuid.ToByteArray();
            Span<byte> originalBytes = originalGuid.ToByteArray();
            return CompareRelativeGuids(relativeBytes, originalBytes);
        }

        private static int CompareRelativeGuids(Span<byte> relativeBytes, Span<byte> originalBytes)
        {
            // Read the last 4 bytes of the guids
            uint relativeInt = BinaryPrimitives.ReadUInt32BigEndian(relativeBytes.Slice(12, 4));
            uint originalInt = BinaryPrimitives.ReadUInt32BigEndian(originalBytes.Slice(12, 4));

            int result;
            unchecked
            {
                result = (int)(relativeInt - originalInt);
            }
            return result;
        }
    }
}
