// Copyright © 2024 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Security;
using SilentNotes.Models;

namespace SilentNotes.Services
{
    /// <summary>
    /// Holds and manages the decrypted keys of the open safes.
    /// </summary>
    public interface ISafeKeyService : IDisposable
    {
        /// <summary>
        /// Tries to open a save with a given password.
        /// </summary>
        /// <param name="safe">The safew to open.</param>
        /// <param name="password">The password to try.</param>
        /// <returns>Returns true if the safe could have been opened or if it was already open,
        /// otherwise false.</returns>
        bool TryOpenSafe(SafeModel safe, SecureString password);

        /// <summary>
        /// Tries to get the key of a given safe.
        /// </summary>
        /// <param name="safeId">The id of the safe to get the key from.</param>
        /// <param name="key">Retrieves the safe key if sucessful, otherwise null.</param>
        /// <returns>Returns true if the key was found (the safe was open), otherwise false.</returns>
        bool TryGetKey(Guid? safeId, out byte[] key);

        /// <summary>
        /// Closes the safe by cleaning and forgetting the decrypted key.
        /// </summary>
        /// <param name="safeId">The id of the safe whose key should be removed.</param>
        void CloseSafe(Guid safeId);

        /// <summary>
        /// Checks whether a given safe is opened (decrypted key exists in the list).
        /// </summary>
        /// <param name="safeId">The id of the safe whose key we want to check.</param>
        /// <returns>Returns true if the safe is open (a derypted key exists for this safe).</returns>
        bool IsSafeOpen(Guid safeId);

        /// <summary>
        /// Cleans up and removes all keys. This should be used only when resetting the safes.
        /// </summary>
        void CloseAllSafes();
    }
}
