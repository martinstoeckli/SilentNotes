// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;

namespace SilentNotes.Crypto
{
    /// <summary>
    /// Describes the interface of a cryptographically secure pseudo random number generator.
    /// </summary>
    public interface ICryptoRandomSource
    {
        /// <summary>
        /// Generates a given number of random bytes.
        /// </summary>
        /// <param name="numberOfBytes">Number of bytes to generate.</param>
        /// <returns>Random bytes.</returns>
        byte[] GetRandomBytes(int numberOfBytes);
    }
}
