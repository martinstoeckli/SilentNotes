// Copyright © 2024 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;

namespace SilentNotes.Services
{
    /// <summary>
    /// Implementation of the <see cref="ISynchronizationService"/> interface.
    /// </summary>
    public class SynchronizationState : ISynchronizationState
    {
        private readonly object _lock = new object();
        private readonly IMessengerService _messenger;
        private SynchronizationType? _currentSynchronizationType;

        /// <summary>
        /// Initializes a new instance of the <see cref="SynchronizationState"/> class.
        /// </summary>
        /// <param name="messenger">A messenger interface for testing, or null if no messages
        /// needs to be sent in tests.</param>
        public SynchronizationState(IMessengerService messenger)
        {
            _messenger = messenger;
        }

        /// <inheritdoc/>
        public bool IsSynchronizationRunning
        {
            get { return _currentSynchronizationType.HasValue; }
        }

        /// <inheritdoc/>
        public DateTime? LastFinishedSynchronization { get; private set; }

        /// <inheritdoc/>
        public bool TryStartSynchronizationState(SynchronizationType syncType)
        {
            lock (_lock)
            {
                // Cannot start synchronization if another one is already running.
                if (IsSynchronizationRunning)
                    return false;

                _currentSynchronizationType = syncType;
                if (ShouldSendChangedMessage(_currentSynchronizationType.Value))
                    _messenger?.Send(new SynchronizationIsRunningChangedMessage(true));
                return true;
            }
        }

        /// <inheritdoc/>
        public void StopSynchronizationState()
        {
            lock (_lock)
            {
                if (!IsSynchronizationRunning)
                    return;

                bool shouldSendChangeMessage = ShouldSendChangedMessage(_currentSynchronizationType.Value);
                _currentSynchronizationType = null;
                LastFinishedSynchronization = DateTime.UtcNow;
                if (shouldSendChangeMessage)
                    _messenger?.Send(new SynchronizationIsRunningChangedMessage(false));
            }
        }

        private bool ShouldSendChangedMessage(SynchronizationType syncType)
        {
            return ((syncType == SynchronizationType.AtStartup) || (syncType == SynchronizationType.Manually));
        }
    }
}
