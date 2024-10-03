// Copyright © 2024 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;

namespace SilentNotes.Services
{
    /// <summary>
    /// Singleton which stores the current state of the synchronization service. It makes sure that
    /// only one synchronization at the time can be running.
    /// </summary>
    /// <remarks>
    /// The instance of the synchronization service itself can change and there can be multiple,
    /// but the state object is a singleton of the application.
    /// </remarks>
    public interface ISynchronizationState
    {
        /// <summary>
        /// Gets a value indicating whether a synchronization is running at this moment.
        /// </summary>
        bool IsSynchronizationRunning { get; }

        /// <summary>
        /// Gets the UTC time of the last known synchronization in the lifetime of the app.
        /// Contains null if the time of the last synchronization is not known.
        /// </summary>
        DateTime? LastFinishedSynchronization { get; }

        /// <summary>
        /// Sets the current timestamp to <see cref="LastFinishedSynchronization"/>.
        /// </summary>
        void UpdateLastFinishedSynchronization();

        /// <summary>
        /// Sets <see cref="IsSynchronizationRunning"/>> to true, if there is not another
        /// synchronization alreday running.
        /// If <see cref="IsSynchronizationRunning"/> has changed, the <see cref="SynchronizationIsRunningChangedMessage"/>
        /// is broadcasted for the types "startup" and "manually", but not for "shutdown".
        /// </summary>
        /// <remarks>
        /// This method is thread safe.
        /// </remarks>
        /// <param name="syncType">The type of the synchronization which should be started.</param>
        /// <returns>Returns true if the state has changed, otherwise false.</returns>
        bool TryStartSynchronizationState(SynchronizationType syncType);

        /// <summary>
        /// Sets <see cref="IsSynchronizationRunning"/>> to false.
        /// If <see cref="IsSynchronizationRunning"/> has changed and the <see cref="SynchronizationIsRunningChangedMessage"/>
        /// is broadcasted for the types "startup" and "manually", but not for "shutdown".
        /// </summary>
        /// <remarks>
        /// This method is thread safe.
        /// </remarks>
        void StopSynchronizationState();
    }

    /// <summary>
    /// Enumeration of all known types of synchronizations.
    /// </summary>
    public enum SynchronizationType
    {
        AtStartup,
        Manually,
        AtShutdown,
    }
}
