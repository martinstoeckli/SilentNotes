// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System.Threading.Tasks;

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
        /// <returns>A task which can be called async.</returns>
        Task SynchronizeAtStartup();

        /// <summary>
        /// Synchronize at applications shutdown time.
        /// </summary>
        /// <returns>A task which can be called async.</returns>
        Task SynchronizeAtShutdown();

        /// <summary>
        /// Gets or sets a fingerprint of the last synchronization, which allows to detect a
        /// session without modifications in the repository. The value can be updated/overwritten
        /// when the user does a manual synchronization. It can be used by the service to avoid
        /// unnecessary synchronisations.
        /// </summary>
        long LastSynchronizationFingerprint { get; set; }
    }
}
