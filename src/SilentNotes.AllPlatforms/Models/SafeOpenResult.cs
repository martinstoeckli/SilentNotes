// Copyright © 2025 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;

namespace SilentNotes.Models
{
    /// <summary>
    /// Contains information about a tried safe open action.
    /// </summary>
    public class SafeOpenResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SafeOpenResult"/> class.
        /// </summary>
        /// <param name="safe">Sets the <see cref="Safe"/> property.</param>
        /// <param name="key">Sets the <see cref="Key"/> property.</param>
        /// <param name="needsReEncryption">Sets the <see cref="NeedsReEncryption"/> property.</param>
        public SafeOpenResult(SafeModel safe, byte[] key, bool needsReEncryption)
        {
            Safe = safe;
            Key = key;
            NeedsReEncryption = needsReEncryption;
        }

        /// <summary>
        /// Gets the safe which was tried to be opened.
        /// </summary>
        public SafeModel Safe { get; }

        /// <summary>
        /// Gets the decrypted key, which can be used to decrypt the note contents.
        /// </summary>
        public byte[] Key { get; }

        /// <summary>
        /// Gets a value indicating whether the safe needs to be re encrypted to adapt to the
        /// current security level.
        /// </summary>
        public bool NeedsReEncryption { get; }
    }
}
