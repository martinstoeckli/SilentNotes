// Copyright © 2023 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Text;
using SilentNotes.Stories;
using SilentNotes.Stories.SynchronizationStory;

namespace SilentNotes.Services
{
    /// <summary>
    /// This service can handle auto synchronisation of the repository with the cloud storage,
    /// at applications startup and shutdown, or manually triggered by the user.
    /// </summary>
    public interface ISynchronizationService
    {
        /// <summary>
        /// Synchronize manually with displaying the UI if necessary. The story is potentially
        /// interrupted with UI dialogs or with waiting for OAuth responses, therefore its end
        /// cannot be awaited.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <returns>A task which can be called async.</returns>
        Task SynchronizeManually(IServiceProvider serviceProvider);

        /// <summary>
        /// Synchronizes manually, starting with the cloud storage choice dialog. 
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <returns>A task which can be called async.</returns>
        Task SynchronizeManuallyChangeCloudStorage(IServiceProvider serviceProvider);

        /// <summary>
        /// Should be called when a manually triggered sanchronization ends.
        /// <param name="serviceProvider">The service provider.</param>
        /// </summary>
        void FinishedManualSynchronization(IServiceProvider serviceProvider);

        /// <summary>
        /// Synchronize at applications startup time in the background. After a successful
        /// synchronization, the GUI must be updated.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <returns>A task which can be called async.</returns>
        Task AutoSynchronizeAtStartup(IServiceProvider serviceProvider);

        /// <summary>
        /// Synchronize at applications shutdown time.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <returns>A task which can be called async.</returns>
        Task AutoSynchronizeAtShutdown(IServiceProvider serviceProvider);

        /// <summary>
        /// Stops a running background synchronization (started at startup or shutdown).
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        void StopAutoSynchronization(IServiceProvider serviceProvider);

        /// <summary>
        /// Gets the active manual synchronization story, or null if no story is active.
        /// </summary>
        SynchronizationStoryModel CurrentStory { get; }
    }
}
