// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using SilentNotes.Services;

namespace SilentNotes.UWP.Services
{
    /// <summary>
    /// Implementation of the <see cref="IAutoSynchronizationService"/> interface.
    /// </summary>
    public class AutoSynchronizationService : AutoSynchronizationServiceBase, IAutoSynchronizationService
    {
        /// <inheritdoc/>
        public override void Stop()
        {
            IsRunning = false;
        }
    }
}
