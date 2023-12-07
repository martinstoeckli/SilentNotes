// Copyright © 2022 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;

namespace SilentNotes.Services
{
    /// <summary>
    /// Allows to keep the screen on (prevents the app from going to sleep).
    /// </summary>
    public interface IKeepScreenOn
    {
        /// <summary>
        /// Starts keeping the screen on, so the device doesn't go to sleep. If a timer is already
        /// running, it will be canceled, consecutive calls will therefore renew the duration.
        /// If the timer has reached the duration, the message <see cref="KeepScreenOnChangedMessage"/>
        /// is broadcasted, so the new state can be updated in the GUI.
        /// </summary>
        /// <param name="duration">Time span after which the keep screen off is stopped automatically.</param>
        void Start(TimeSpan duration);

        /// <summary>
        /// Stops keeping the screen on, so the device can go to sleep again. If a timer is running,
        /// this timer will be canceled.
        /// </summary>
        void Stop();

        /// <summary>
        /// Gets a value indicating whether the keep screen on function is currently active.
        /// </summary>
        bool IsActive {  get; }
    }

    /// <summary>
    /// Message which can be broadcasted by the WeakReferenceMessenger to signal a state change.
    /// </summary>
    public class KeepScreenOnChangedMessage
    {
    }
}
