// Copyright © 2024 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Security;
using SilentNotes.Crypto;
using SilentNotes.Models;

namespace SilentNotes.Services
{
    /// <summary>
    /// Implementation of the <see cref="ISafeKeyService"/> interface.
    /// </summary>
    public class SafeKeyService : ISafeKeyService
    {
        protected readonly Dictionary<Guid, byte[]> _safeKeys;

        /// <summary>
        /// Initializes a new instance of the <see cref="SafeKeyService"/> class.
        /// </summary>
        public SafeKeyService()
        {
            _safeKeys = new Dictionary<Guid, byte[]>();
        }

        /// <inheritdoc/>
        public bool TryOpenSafe(SafeModel safe, SecureString password)
        {
            if (!_safeKeys.ContainsKey(safe.Id))
            {
                if (SafeModel.TryDecryptKey(safe.SerializeableKey, password, out byte[] decryptedKey))
                    _safeKeys.Add(safe.Id, decryptedKey);
            }
            return IsSafeOpen(safe.Id);
        }

        /// <inheritdoc/>
        public bool TryGetKey(Guid? safeId, out byte[] key)
        {
            if (!safeId.HasValue)
            {
                key = null;
                return false;
            }
            else
                return _safeKeys.TryGetValue(safeId.Value, out key);
        }

        /// <inheritdoc/>
        public void CloseSafe(Guid safeId)
        {
            if (_safeKeys.TryGetValue(safeId, out byte[] key))
            {
                _safeKeys.Remove(safeId);
                CryptoUtils.CleanArray(key);
            }
        }

        /// <inheritdoc/>
        public bool IsSafeOpen(Guid safeId)
        {
            return _safeKeys.ContainsKey(safeId);
        }

        /// <inheritdoc/>
        public void CloseAllSafes()
        {
            foreach (Guid key in _safeKeys.Keys)
                CloseSafe(key);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            CloseAllSafes();
        }
    }
}
