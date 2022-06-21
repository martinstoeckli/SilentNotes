// Copyright © 2022 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Text;

namespace SilentNotes.Services
{
    /// <summary>
    /// Allows to keep the screen on (prevents the app from going to sleep).
    /// </summary>
    public interface IKeepScreenOn
    {
        /// <summary>
        /// Starts keeping the screen on, so the device doesn't go to sleep.
        /// </summary>
        void Start();

        /// <summary>
        /// Stops keeping the screen on, so the device can go to sleep again. If a timer is running
        /// after calling <see cref="StopAfter(TimeSpan)"/>, this timer will be canceled.
        /// </summary>
        void Stop();

        /// <summary>
        /// Calls <see cref="Stop"/> after a given time period. If a timer is already running, it
        /// will be canceled, consecutive calls will therefore renew the duration.
        /// </summary>
        /// <param name="duration">Time span after which the keep screen off is stopped automatically.</param>
        void StopAfter(TimeSpan duration);

        /// <summary>
        /// This event will be called whenever the state changes.
        /// </summary>
        event EventHandler<bool> StateChanged;
    }
}
