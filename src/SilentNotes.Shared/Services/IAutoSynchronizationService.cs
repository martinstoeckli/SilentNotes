// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;

namespace SilentNotes.Services
{
    /// <summary>
    /// This service can handle auto synchronisation of the repository with the cloud storage,
    /// at applications startup and shutdown.
    /// </summary>
    public interface IAutoSynchronizationService
    {
        /// <summary>
        /// Synchronize at applications startup time in the background. After a successful
        /// synchronization, the GUI must be updated.
        /// </summary>
        void SynchronizeAtStartup();

        /// <summary>
        /// Synchronize at applications shutdown time.
        /// </summary>
        void SynchronizeAtShutdown();

        /// <summary>
        /// Keeping this fingerprint of the last synchronization allows to detect a read-only
        /// session, when the user didn't modify anything in the repository. The value can be
        /// overwritten when the user does a manual synchronization. It can be used by the service
        /// to avoid unnecessary synchronisations.
        /// </summary>
        long LastSynchronizationFingerprint { get; set; }
    }
}
