// Copyright © 2023 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Threading.Tasks;

namespace SilentNotes.Services
{
    /// <summary>
    /// base class for implementations of the <see cref="IAutoSynchronizationService"/> interface.
    /// </summary>
    public abstract class AutoSynchronizationServiceBase : IAutoSynchronizationService
    {
        /// <inheritdoc/>
        public abstract Task SynchronizeAtStartup();

        /// <inheritdoc/>
        public abstract Task SynchronizeAtShutdown();

        /// <inheritdoc/>
        public abstract void Stop();

        /// <inheritdoc/>
        public bool IsRunning { get; set; }

        /// <inheritdoc/>
        public long LastSynchronizationFingerprint { get; set; }
    }
}
